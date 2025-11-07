using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using CollabDoc.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CollabDoc.Realtime.Hubs
{
    public class CollabHub : Hub
    {
        // 🧠 Danh sách user đang hoạt động theo document
        private static readonly ConcurrentDictionary<string, HashSet<string>> ActiveUsers = new();

        // 📦 Lưu "full state" (document binary Yjs)
        private static readonly ConcurrentDictionary<string, byte[]> DocumentStates = new();

        private readonly ILogger<CollabHub> _logger;
        private readonly IDocumentRepository _documentRepository;

        public CollabHub(IDocumentRepository documentRepository, ILogger<CollabHub> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        // Khi user kết nối SignalR
        public override async Task OnConnectedAsync()
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
            _logger.LogInformation("User {UserEmail} connected with ConnectionId {ConnectionId}",
                userEmail, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        // ❌ Khi user ngắt kết nối (đóng tab, reload, mất mạng)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (exception != null)
            {
                _logger.LogWarning(exception, "User {UserEmail} disconnected with error. ConnectionId: {ConnectionId}",
                    userEmail, Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("User {UserEmail} disconnected normally. ConnectionId: {ConnectionId}",
                    userEmail, Context.ConnectionId);
            }

            var documentsLeft = new List<string>();

            foreach (var kvp in ActiveUsers)
            {
                var documentId = kvp.Key;
                var users = kvp.Value;
                bool removed = false;

                lock (users)
                {
                    removed = users.Remove(userEmail);
                }

                if (removed)
                {
                    documentsLeft.Add(documentId);

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);

                    // Thông báo cho các client khác biết user này đã rời đi
                    await Clients.OthersInGroup(documentId).SendAsync(HubEvents.UserLeft, new
                    {
                        DocumentId = documentId,
                        User = userEmail,
                        ConnectionId = Context.ConnectionId
                    });

                    _logger.LogDebug("User {UserEmail} removed from document {DocumentId} group (disconnected). Remaining users: {UserCount}",
                        userEmail, documentId, users.Count);

                    // Nếu group trống → xóa luôn entry
                    if (users.Count == 0)
                    {
                        ActiveUsers.TryRemove(documentId, out _);
                        _logger.LogDebug("Document {DocumentId} group removed - no active users remaining", documentId);
                    }
                }
            }

            if (documentsLeft.Count > 0)
            {
                _logger.LogInformation("User {UserEmail} left {DocumentCount} document groups: {DocumentIds}",
                    userEmail, documentsLeft.Count, string.Join(", ", documentsLeft));
            }

            await base.OnDisconnectedAsync(exception);
        }

        // 🪩 Khi user join vào document
        public async Task JoinDocumentGroup(string documentId)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(documentId))
            {
                _logger.LogWarning("User {UserEmail} attempted to join empty/null document ID", userEmail);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, documentId);

            // Thêm user vào danh sách ActiveUsers
            var users = ActiveUsers.GetOrAdd(documentId, _ => new HashSet<string>());
            bool isNewUser;

            lock (users)
            {
                isNewUser = users.Add(userEmail);
            }

            _logger.LogInformation("User {UserEmail} joined document {DocumentId}. Total users in document: {UserCount}. New user: {IsNewUser}",
                userEmail, documentId, users.Count, isNewUser);

            // 📤 Gửi danh sách user đang trong document cho chính user mới
            await Clients.Caller.SendAsync(HubEvents.ActiveUsers, users.ToList());

            // 📢 Thông báo cho các user khác trong nhóm rằng có người mới vào
            await Clients.OthersInGroup(documentId).SendAsync(HubEvents.UserJoined, new
            {
                DocumentId = documentId,
                User = userEmail,
                ConnectionId = Context.ConnectionId
            });

            // 📦 Nếu server đã có document state → gửi cho người mới
            if (DocumentStates.TryGetValue(documentId, out var state) && state is { Length: > 0 })
            {
                _logger.LogDebug("Sending cached initial state to user {UserEmail} for document {DocumentId}. State size: {StateSize} bytes",
                    userEmail, documentId, state.Length);

                await Clients.Caller.SendAsync(HubEvents.InitialState, state);
            }
            else
            {
                _logger.LogDebug("No cached state for document {DocumentId}. Requesting sync from other users for {UserEmail}",
                    documentId, userEmail);

                await Clients.OthersInGroup(documentId)
                    .SendAsync(HubEvents.RequestSync, documentId, Context.ConnectionId);
                await Clients.Caller.SendAsync(HubEvents.NoInitialState, documentId);
            }
        }

        // 👋 Khi user chủ động rời document (hoặc tắt editor)
        public async Task LeaveDocumentGroup(string documentId)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(documentId))
            {
                _logger.LogWarning("User {UserEmail} attempted to leave empty/null document ID", userEmail);
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, documentId);

            var remainingUsers = 0;
            if (ActiveUsers.TryGetValue(documentId, out var users))
            {
                lock (users)
                {
                    users.Remove(userEmail);
                    remainingUsers = users.Count;
                }

                if (remainingUsers == 0)
                {
                    ActiveUsers.TryRemove(documentId, out _);
                    _logger.LogDebug("Document {DocumentId} group removed - last user {UserEmail} left", documentId, userEmail);
                }
            }

            _logger.LogInformation("User {UserEmail} left document {DocumentId}. Remaining users: {UserCount}",
                userEmail, documentId, remainingUsers);

            await Clients.OthersInGroup(documentId).SendAsync(HubEvents.UserLeft, new
            {
                DocumentId = documentId,
                User = userEmail,
                ConnectionId = Context.ConnectionId
            });
        }

        // 🔥 Gửi Yjs delta update đến các client khác
        public async Task SendYjsUpdate(string documentId, List<int> update)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(documentId))
            {
                _logger.LogWarning("User {UserEmail} sent Yjs update with empty document ID", userEmail);
                return;
            }

            if (update is null || update.Count == 0)
            {
                _logger.LogDebug("User {UserEmail} sent empty Yjs update for document {DocumentId} - skipping",
                    userEmail, documentId);
                return;
            }

            _logger.LogTrace("Broadcasting Yjs update from user {UserEmail} for document {DocumentId}. Update size: {UpdateSize} bytes",
                userEmail, documentId, update.Count);

            await Clients.OthersInGroup(documentId).SendAsync(HubEvents.YjsUpdate, update);
        }

        // ⛳ Gửi full snapshot (state) khi có người mới join
        public async Task SendFullState(string documentId, string requesterConnectionId, List<int> fullState)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(documentId) || string.IsNullOrWhiteSpace(requesterConnectionId))
            {
                _logger.LogWarning("User {UserEmail} sent invalid full state - missing document ID or requester connection ID",
                    userEmail);
                return;
            }

            if (fullState == null || fullState.Count == 0)
            {
                _logger.LogWarning("User {UserEmail} sent empty full state for document {DocumentId} to requester {RequesterId}",
                    userEmail, documentId, requesterConnectionId);
                return;
            }

            var bytes = fullState.Select(b => (byte)b).ToArray();
            DocumentStates[documentId] = bytes;

            _logger.LogDebug("User {UserEmail} provided full state for document {DocumentId} to requester {RequesterId}. State size: {StateSize} bytes",
                userEmail, documentId, requesterConnectionId, bytes.Length);

            await Clients.Client(requesterConnectionId).SendAsync(HubEvents.InitialState, fullState);
        }

        // 👥 Awareness update
        public async Task SendAwarenessUpdate(string documentId, List<int> update)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(documentId))
            {
                _logger.LogWarning("User {UserEmail} sent awareness update with empty document ID", userEmail);
                return;
            }

            if (update is null || update.Count == 0)
            {
                _logger.LogTrace("User {UserEmail} sent empty awareness update for document {DocumentId} - skipping",
                    userEmail, documentId);
                return;
            }

            _logger.LogTrace("Broadcasting awareness update from user {UserEmail} for document {DocumentId}. Update size: {UpdateSize} bytes",
                userEmail, documentId, update.Count);

            await Clients.OthersInGroup(documentId).SendAsync(HubEvents.AwarenessUpdate, update);
        }

        //  Auto-save document state từ client
        public async Task SaveDocumentState(string documentId, List<int> fullState)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";

            if (string.IsNullOrWhiteSpace(documentId))
            {
                _logger.LogWarning("User {UserEmail} attempted to save document with empty/null document ID", userEmail);
                return;
            }

            if (fullState == null || fullState.Count == 0)
            {
                _logger.LogWarning("User {UserEmail} attempted to save empty state for document {DocumentId}",
                    userEmail, documentId);
                return;
            }

            var bytes = fullState.Select(b => (byte)b).ToArray();
            DocumentStates[documentId] = bytes;

            var base64State = Convert.ToBase64String(bytes);

            try
            {
                await _documentRepository.UpdateContentAsync(documentId, base64State);
                await Clients.Groups(documentId).SendAsync("DocumentAutoSaved", documentId, DateTime.UtcNow);

                _logger.LogInformation("Document {DocumentId} auto-saved by user {UserEmail}. State size: {StateSize} bytes",
                    documentId, userEmail, bytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-save document {DocumentId} for user {UserEmail}. State size: {StateSize} bytes",
                    documentId, userEmail, bytes.Length);

                // Có thể gửi thông báo lỗi về client
                await Clients.Caller.SendAsync("DocumentSaveError", documentId, "Failed to save document");
            }
        }
    }
}
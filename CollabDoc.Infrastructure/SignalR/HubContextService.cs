using Microsoft.AspNetCore.SignalR;
using CollabDoc.Realtime.Hubs;
using CollabDoc.Application.Interfaces;

namespace CollabDoc.Infrastructure.SignalR
{
    public class HubContextService : ISignalRHubService
    {
        private readonly IHubContext<CollabHub> _hubContext;

        public HubContextService(IHubContext<CollabHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyDocumentUpdatedAsync(string documentId, string updatedBy)
        {
            await _hubContext.Clients.Group(documentId).SendAsync(HubEvents.YjsUpdate, new
            {
                DocumentId = documentId,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

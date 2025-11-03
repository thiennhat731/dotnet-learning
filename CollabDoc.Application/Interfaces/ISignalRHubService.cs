namespace CollabDoc.Application.Interfaces
{
    public interface ISignalRHubService
    {
        Task NotifyDocumentUpdatedAsync(string documentId, string updatedBy);
    }
}

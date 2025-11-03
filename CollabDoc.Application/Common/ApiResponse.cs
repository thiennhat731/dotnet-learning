namespace CollabDoc.Application.Common
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Message { get; set; } = "Success";
        public string? Error { get; set; }
        public T? Data { get; set; }

        public ApiResponse(int status, string message, T? data = default, string? error = null)
        {
            Status = status;
            Message = message;
            Data = data;
            Error = error;
        }
    }
}

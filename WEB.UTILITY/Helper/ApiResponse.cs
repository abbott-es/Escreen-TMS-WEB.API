namespace WEB.UTILITY.Helper
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public string? TraceId { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) =>
            new()
            {
                Success = true,
                Data = data,
                Message = message,
                TraceId = traceId
            };

        public static ApiResponse<T> Fail(List<string> errors, string? message = null, string? traceId = null) =>
            new()
            {
                Success = false,
                Errors = errors,
                Message = message,
                TraceId = traceId
            };
    }
}

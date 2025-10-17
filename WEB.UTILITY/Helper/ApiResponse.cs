using System.Net;

namespace WEB.UTILITY.Helper
{
    public class ApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool Success { get; set; }
        public T? Response { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public string? TraceId { get; set; }

        public static ApiResponse<T> Ok(T data, HttpStatusCode statusCode = HttpStatusCode.OK, string? message = null, string? traceId = null) =>
            new()
            {
                StatusCode = statusCode,
                Success = true,
                Response = data,
                Message = message,
                TraceId = traceId
            };

        public static ApiResponse<T> Fail(List<string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string? message = null, string? traceId = null) =>
            new()
            {
                StatusCode = statusCode,
                Success = false,
                Errors = errors,
                Message = message,
                TraceId = traceId
            };
    }
}

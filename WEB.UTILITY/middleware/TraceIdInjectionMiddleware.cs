using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Buffers;

namespace WEB.UTILITY.middleware
{
    public class TraceIdInjectionMiddleware
    {
        private readonly RequestDelegate _next;

        public TraceIdInjectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            context.Response.Body = originalBodyStream;
            memoryStream.Seek(0, SeekOrigin.Begin);

            if (context.Response.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    using var document = await JsonDocument.ParseAsync(memoryStream, new JsonDocumentOptions { AllowTrailingCommas = true });

                    var buffer = new ArrayBufferWriter<byte>();
                    using (var writer = new Utf8JsonWriter(buffer))
                    {
                        writer.WriteStartObject();

                        foreach (var prop in document.RootElement.EnumerateObject())
                        {
                            prop.WriteTo(writer);
                        }

                        writer.WriteString("traceId", context.TraceIdentifier);
                        writer.WriteEndObject();
                    }

                    context.Response.ContentLength = buffer.WrittenCount;
                    await context.Response.Body.WriteAsync(buffer.WrittenMemory);
                    return;
                }
                catch
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    await memoryStream.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
    }
}

using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Common
{
    /// <summary>
    /// Bắt mọi exception chưa xử lý và trả về { "message": "..." } với status code phù hợp.
    /// </summary>
    public class ExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (status, message) = ex switch
                {
                    NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                    BadRequestException => (HttpStatusCode.BadRequest, ex.Message),
                    ConflictException => (HttpStatusCode.Conflict, ex.Message),
                    // Vi phạm khóa ngoại / unique index trong database
                    DbUpdateException => (HttpStatusCode.Conflict,
                        "Dữ liệu bị ràng buộc hoặc trùng lặp, không thể thực hiện thao tác"),
                    _ => (HttpStatusCode.InternalServerError, "Lỗi hệ thống")
                };

                if (status == HttpStatusCode.InternalServerError || ex is DbUpdateException)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý {Method} {Path}", context.Request.Method, context.Request.Path);
                }

                if (context.Response.HasStarted)
                {
                    throw;
                }

                context.Response.Clear();
                context.Response.StatusCode = (int)status;
                context.Response.ContentType = "application/json";

                var body = new ErrorResponse(message);
                await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
            }
        }
    }
}

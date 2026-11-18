using System.Text.Json.Serialization;

namespace EnglishCenterManagement_CSW306_Project.Common
{
    /// <summary>
    /// Body lỗi chung của toàn dự án, trùng với quy ước trong README và FeatureControllerBase:
    /// { "message": "..." }. Riêng lỗi validation có thêm "errors" (danh sách từng lỗi).
    /// Thành công thì mọi module trả thẳng dữ liệu (không bọc thêm lớp nào).
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }

        public ErrorResponse(string message, List<string>? errors = null)
        {
            Message = message;
            Errors = errors;
        }
    }
}

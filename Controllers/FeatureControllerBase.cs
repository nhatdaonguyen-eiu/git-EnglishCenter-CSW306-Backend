using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    // Lớp cha dùng chung cho 3 controller của TV4 (Enrollments, Attendances, Scores).
    // Đổi lỗi của Service thành mã HTTP, body lỗi giữ cùng dạng { message } như CoursesController.
    // Nếu TV3 chuẩn hóa định dạng lỗi (T16) thì chỉ cần sửa hàm này.
    [ApiController]
    public abstract class FeatureControllerBase : ControllerBase
    {
        protected IActionResult ErrorResponse<T>(FeatureResult<T> result)
        {
            var body = new { message = result.Message };

            return result.Error switch
            {
                ResultError.NotFound => NotFound(body),     // 404
                ResultError.Conflict => Conflict(body),     // 409
                _ => BadRequest(body)                       // 400
            };
        }
    }
}

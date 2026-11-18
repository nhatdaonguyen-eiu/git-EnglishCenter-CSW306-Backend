using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    // Cùng quy ước với các controller khác: thành công trả thẳng dữ liệu,
    // sửa/xóa trả 204, lỗi do ExceptionMiddleware đổi thành { message } (404, 409, 400...).
    // TODO (sau khi TV2 merge JWT): bật phân quyền, ví dụ [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _service;

        public ClassesController(IClassService service)
        {
            _service = service;
        }

        // GET: api/Classes
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        // GET: api/Classes/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _service.GetByIdAsync(id));

        // GET: api/Classes/by-course/1   (T19)
        [HttpGet("by-course/{courseId:int}")]
        public async Task<IActionResult> GetByCourse(int courseId)
            => Ok(await _service.GetByCourseAsync(courseId));

        // GET: api/Classes/by-teacher/2   (T19)
        [HttpGet("by-teacher/{teacherId:int}")]
        public async Task<IActionResult> GetByTeacher(int teacherId)
            => Ok(await _service.GetByTeacherAsync(teacherId));

        // POST: api/Classes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/Classes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClassDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        // DELETE: api/Classes/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

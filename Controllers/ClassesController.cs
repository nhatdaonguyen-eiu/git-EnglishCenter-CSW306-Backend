using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _service;

        public ClassesController(IClassService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(ApiResponse<List<ClassDto>>.Ok(await _service.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(ApiResponse<ClassDto>.Ok(await _service.GetByIdAsync(id)));

        // T19
        [HttpGet("by-course/{courseId:int}")]
        public async Task<IActionResult> GetByCourse(int courseId)
            => Ok(ApiResponse<List<ClassDto>>.Ok(await _service.GetByCourseAsync(courseId)));

        // T19
        [HttpGet("by-teacher/{teacherId:int}")]
        public async Task<IActionResult> GetByTeacher(int teacherId)
            => Ok(ApiResponse<List<ClassDto>>.Ok(await _service.GetByTeacherAsync(teacherId)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateClassDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<ClassDto>.Ok(created, "Tạo lớp thành công"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateClassDto dto)
            => Ok(ApiResponse<ClassDto>.Ok(await _service.UpdateAsync(id, dto), "Cập nhật thành công"));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

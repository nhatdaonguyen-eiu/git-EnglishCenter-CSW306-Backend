using EnglishCenterManagement_CSW306_Project.DTOs.TeacherDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        // GET: api/Teachers
        [HttpGet]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            return Ok(teachers);
        }

        // GET: api/Teachers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            var teacher = await _teacherService.GetTeacherByIdAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = $"Không tìm thấy giáo viên có Id = {id}" });
            }
            return Ok(teacher);
        }

        // POST: api/Teachers
        [HttpPost]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateUpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTeacher = await _teacherService.CreateTeacherAsync(dto);
            return CreatedAtAction(nameof(GetTeacherById), new { id = createdTeacher.Id }, createdTeacher);
        }

        // PUT: api/Teachers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] CreateUpdateTeacherDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _teacherService.UpdateTeacherAsync(id, dto);
            if (!isUpdated)
            {
                return NotFound(new { message = $"Không tìm thấy giáo viên có Id = {id} để cập nhật" });
            }

            return NoContent();
        }

        // DELETE: api/Teachers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var isDeleted = await _teacherService.DeleteTeacherAsync(id);
            if (!isDeleted)
            {
                return NotFound(new { message = $"Không tìm thấy giáo viên có Id = {id} để xóa" });
            }

            return NoContent();
        }
    }
}
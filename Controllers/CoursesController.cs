using EnglishCenterManagement_CSW306_Project.DTOs.CourseDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound(new { message = $"Không tìm thấy khóa học có Id = {id}" });
            }
            return Ok(course);
        }

        // POST: api/Courses
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateUpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCourse = await _courseService.CreateCourseAsync(dto);
            return CreatedAtAction(nameof(GetCourseById), new { id = createdCourse.Id }, createdCourse);
        }

        // PUT: api/Courses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CreateUpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isUpdated = await _courseService.UpdateCourseAsync(id, dto);
            if (!isUpdated)
            {
                return NotFound(new { message = $"Không tìm thấy khóa học có Id = {id} để cập nhật" });
            }

            return NoContent(); // 204 Không trả về data sau khi update thành công
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var isDeleted = await _courseService.DeleteCourseAsync(id);
            if (!isDeleted)
            {
                return NotFound(new { message = $"Không tìm thấy khóa học có Id = {id} để xóa" });
            }

            return NoContent();
        }
    }
}
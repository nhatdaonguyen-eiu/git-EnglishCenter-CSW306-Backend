using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _service;

        public StudentsController(IStudentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(ApiResponse<List<StudentDto>>.Ok(await _service.GetAllAsync()));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(ApiResponse<StudentDto>.Ok(await _service.GetByIdAsync(id)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateStudentDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<StudentDto>.Ok(created, "Tạo học viên thành công"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateStudentDto dto)
            => Ok(ApiResponse<StudentDto>.Ok(await _service.UpdateAsync(id, dto), "Cập nhật thành công"));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

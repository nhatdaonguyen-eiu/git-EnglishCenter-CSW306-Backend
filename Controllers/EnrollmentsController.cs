using EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    // TODO (sau khi TV2 merge JWT): bật phân quyền
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    public class EnrollmentsController : FeatureControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // GET: api/Enrollments?studentId=1&classId=2&status=Active
        [HttpGet]
        public async Task<IActionResult> GetEnrollments([FromQuery] int? studentId, [FromQuery] int? classId, [FromQuery] string? status)
        {
            var enrollments = await _enrollmentService.GetEnrollmentsAsync(studentId, classId, status);
            return Ok(enrollments);
        }

        // GET: api/Enrollments/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrollmentById(int id)
        {
            var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
            if (enrollment == null)
            {
                return NotFound(new { message = $"Không tìm thấy ghi danh có Id = {id}" });
            }
            return Ok(enrollment);
        }

        // POST: api/Enrollments  -> ghi danh học viên vào lớp
        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto dto)
        {
            var result = await _enrollmentService.EnrollAsync(dto);
            if (!result.Success) return ErrorResponse(result);

            return CreatedAtAction(nameof(GetEnrollmentById), new { id = result.Data!.Id }, result.Data);
        }

        // PUT: api/Enrollments/5/cancel  -> hủy ghi danh
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _enrollmentService.CancelAsync(id);
            if (!result.Success) return ErrorResponse(result);

            return Ok(result.Data);
        }

        // PUT: api/Enrollments/5/transfer  -> chuyển sang lớp khác (trả về ghi danh mới)
        [HttpPut("{id:int}/transfer")]
        public async Task<IActionResult> Transfer(int id, [FromBody] TransferEnrollmentDto dto)
        {
            var result = await _enrollmentService.TransferAsync(id, dto);
            if (!result.Success) return ErrorResponse(result);

            return Ok(result.Data);
        }
    }
}

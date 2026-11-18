using EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    // TODO (sau khi TV2 merge JWT): bật phân quyền
    // Admin + Teacher: ghi/sửa/xóa điểm danh, xem thống kê. Student: chỉ xem của mình.
    [Authorize(Roles = "Admin,Teacher")]
    [Route("api/[controller]")]
    public class AttendancesController : FeatureControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendancesController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: api/Attendances?classId=1&date=2026-10-05
        [HttpGet]
        public async Task<IActionResult> GetAttendances(
            [FromQuery] int? enrollmentId, [FromQuery] int? classId,
            [FromQuery] int? studentId, [FromQuery] DateTime? date)
        {
            var attendances = await _attendanceService.GetAttendancesAsync(enrollmentId, classId, studentId, date);
            return Ok(attendances);
        }

        // GET: api/Attendances/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByIdAsync(id);
            if (attendance == null)
            {
                return NotFound(new { message = $"Không tìm thấy bản ghi điểm danh có Id = {id}" });
            }
            return Ok(attendance);
        }

        // POST: api/Attendances
        [HttpPost]
        public async Task<IActionResult> CreateAttendance([FromBody] CreateAttendanceDto dto)
        {
            var result = await _attendanceService.CreateAttendanceAsync(dto);
            if (!result.Success) return ErrorResponse(result);

            return CreatedAtAction(nameof(GetAttendanceById), new { id = result.Data!.Id }, result.Data);
        }

        // PUT: api/Attendances/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAttendanceDto dto)
        {
            var result = await _attendanceService.UpdateAttendanceAsync(id, dto);
            if (!result.Success) return ErrorResponse(result);

            return NoContent();
        }

        // DELETE: api/Attendances/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var result = await _attendanceService.DeleteAttendanceAsync(id);
            if (!result.Success) return ErrorResponse(result);

            return NoContent();
        }

        // GET: api/Attendances/stats/student/3  -> thống kê điểm danh của 1 học viên (theo từng lớp)
        [HttpGet("stats/student/{studentId:int}")]
        public async Task<IActionResult> GetStatsByStudent(int studentId)
        {
            var result = await _attendanceService.GetStatsByStudentAsync(studentId);
            if (!result.Success) return ErrorResponse(result);

            return Ok(result.Data);
        }

        // GET: api/Attendances/stats/class/1  -> thống kê điểm danh của 1 lớp (theo từng học viên)
        [HttpGet("stats/class/{classId:int}")]
        public async Task<IActionResult> GetStatsByClass(int classId)
        {
            var result = await _attendanceService.GetStatsByClassAsync(classId);
            if (!result.Success) return ErrorResponse(result);

            return Ok(result.Data);
        }
    }
}

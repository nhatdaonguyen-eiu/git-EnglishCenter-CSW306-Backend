using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Attendance, AttendanceDto>> ToDto = a => new AttendanceDto
        {
            Id = a.Id,
            EnrollmentId = a.EnrollmentId ?? 0,
            StudentId = a.Enrollment!.StudentId,
            StudentName = a.Enrollment.Student!.FullName,
            ClassId = a.Enrollment.ClassId,
            ClassName = a.Enrollment.Class!.Name,
            SessionDate = a.SessionDate,
            Status = a.Status
        };

        // ===================== T23: CRUD =====================

        public async Task<IEnumerable<AttendanceDto>> GetAttendancesAsync(int? enrollmentId, int? classId, int? studentId, DateTime? date)
        {
            var query = _context.Attendances.AsQueryable();

            if (enrollmentId.HasValue) query = query.Where(a => a.EnrollmentId == enrollmentId.Value);
            if (classId.HasValue) query = query.Where(a => a.Enrollment!.ClassId == classId.Value);
            if (studentId.HasValue) query = query.Where(a => a.Enrollment!.StudentId == studentId.Value);
            if (date.HasValue)
            {
                var day = date.Value.Date;
                query = query.Where(a => a.SessionDate == day);
            }

            return await query
                .OrderByDescending(a => a.SessionDate)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<AttendanceDto?> GetAttendanceByIdAsync(int id)
        {
            return await _context.Attendances
                .Where(a => a.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();
        }

        public async Task<FeatureResult<AttendanceDto>> CreateAttendanceAsync(CreateAttendanceDto dto)
        {
            var status = AttendanceStatus.Normalize(dto.Status);
            if (status == null)
                return FeatureResult<AttendanceDto>.Invalid($"Trạng thái không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AttendanceStatus.All)}");

            var enrollment = await _context.Enrollments.FindAsync(dto.EnrollmentId);
            if (enrollment == null)
                return FeatureResult<AttendanceDto>.NotFound($"Không tìm thấy ghi danh có Id = {dto.EnrollmentId}");

            if (enrollment.Status == EnrollmentStatus.Cancelled)
                return FeatureResult<AttendanceDto>.Invalid("Ghi danh này đã bị hủy, không thể điểm danh");

            // Chỉ lấy phần ngày (bỏ giờ) để 1 buổi học chỉ điểm danh 1 lần
            var sessionDate = dto.SessionDate!.Value.Date;

            var duplicated = await _context.Attendances
                .AnyAsync(a => a.EnrollmentId == dto.EnrollmentId && a.SessionDate == sessionDate);
            if (duplicated)
                return FeatureResult<AttendanceDto>.Conflict("Học viên đã được điểm danh trong ngày này");

            var attendance = new Attendance
            {
                EnrollmentId = dto.EnrollmentId,
                SessionDate = sessionDate,
                Status = status
            };
            _context.Attendances.Add(attendance);
            if (!await _context.TrySaveChangesAsync())
                return FeatureResult<AttendanceDto>.Conflict("Dữ liệu vừa được người khác thay đổi (có thể đã bị trùng), vui lòng thử lại");

            var result = await GetAttendanceByIdAsync(attendance.Id);
            return FeatureResult<AttendanceDto>.Ok(result!);
        }

        public async Task<FeatureResult<bool>> UpdateAttendanceAsync(int id, UpdateAttendanceDto dto)
        {
            var status = AttendanceStatus.Normalize(dto.Status);
            if (status == null)
                return FeatureResult<bool>.Invalid($"Trạng thái không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AttendanceStatus.All)}");

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return FeatureResult<bool>.NotFound($"Không tìm thấy bản ghi điểm danh có Id = {id}");

            var sessionDate = dto.SessionDate!.Value.Date;

            // Nếu đổi sang ngày khác thì ngày đó không được trùng với buổi đã điểm danh
            if (sessionDate != attendance.SessionDate)
            {
                var duplicated = await _context.Attendances.AnyAsync(a =>
                    a.EnrollmentId == attendance.EnrollmentId && a.SessionDate == sessionDate && a.Id != id);
                if (duplicated)
                    return FeatureResult<bool>.Conflict("Học viên đã được điểm danh trong ngày này");
            }

            attendance.SessionDate = sessionDate;
            attendance.Status = status;
            await _context.SaveChangesAsync();
            return FeatureResult<bool>.Ok(true);
        }

        public async Task<FeatureResult<bool>> DeleteAttendanceAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return FeatureResult<bool>.NotFound($"Không tìm thấy bản ghi điểm danh có Id = {id}");

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return FeatureResult<bool>.Ok(true);
        }

        // ===================== T24: Thống kê =====================

        // Thống kê của 1 học viên: mỗi lớp đã/đang học là 1 dòng (kể cả lớp đã hủy, để xem lịch sử)
        public async Task<FeatureResult<List<AttendanceStatDto>>> GetStatsByStudentAsync(int studentId)
        {
            if (!await _context.Students.AnyAsync(s => s.Id == studentId))
                return FeatureResult<List<AttendanceStatDto>>.NotFound($"Không tìm thấy học viên có Id = {studentId}");

            var query = _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .OrderBy(e => e.Class!.Name);

            return FeatureResult<List<AttendanceStatDto>>.Ok(await BuildStatsAsync(query));
        }

        // Thống kê của 1 lớp: mỗi học viên đang học (hoặc đã hoàn thành) là 1 dòng
        public async Task<FeatureResult<List<AttendanceStatDto>>> GetStatsByClassAsync(int classId)
        {
            if (!await _context.Classes.AnyAsync(c => c.Id == classId))
                return FeatureResult<List<AttendanceStatDto>>.NotFound($"Không tìm thấy lớp học có Id = {classId}");

            var query = _context.Enrollments
                .Where(e => e.ClassId == classId && e.Status != EnrollmentStatus.Cancelled)
                .OrderBy(e => e.Student!.FullName);

            return FeatureResult<List<AttendanceStatDto>>.Ok(await BuildStatsAsync(query));
        }

        // Đếm số buổi theo từng trạng thái cho danh sách ghi danh truyền vào.
        // Muốn đổi cách tính % chuyên cần thì sửa đúng dòng AttendanceRate bên dưới.
        private static async Task<List<AttendanceStatDto>> BuildStatsAsync(IQueryable<Enrollment> enrollments)
        {
            var rows = await enrollments.Select(e => new AttendanceStatDto
            {
                EnrollmentId = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student!.FullName,
                ClassId = e.ClassId,
                ClassName = e.Class!.Name,
                EnrollmentStatus = e.Status,
                TotalSessions = e.Attendances.Count(),
                PresentCount = e.Attendances.Count(a => a.Status == AttendanceStatus.Present),
                AbsentCount = e.Attendances.Count(a => a.Status == AttendanceStatus.Absent),
                ExcusedCount = e.Attendances.Count(a => a.Status == AttendanceStatus.Excused)
            }).ToListAsync();

            foreach (var row in rows)
            {
                row.AttendanceRate = row.TotalSessions == 0
                    ? 0
                    : Math.Round(row.PresentCount * 100.0 / row.TotalSessions, 1);
            }
            return rows;
        }
    }
}

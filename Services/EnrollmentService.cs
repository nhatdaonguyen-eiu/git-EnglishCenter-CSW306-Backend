using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        // Dùng AppDbContext trực tiếp (thay vì IRepository) vì module này cần
        // join / đếm sĩ số / lưu nhiều thay đổi trong 1 lần SaveChanges (chuyển lớp).
        private readonly AppDbContext _context;

        public EnrollmentService(AppDbContext context)
        {
            _context = context;
        }

        // Chuyển Entity -> DTO ở 1 chỗ duy nhất, mọi hàm bên dưới đều dùng lại
        private static readonly Expression<Func<Enrollment, EnrollmentDto>> ToDto = e => new EnrollmentDto
        {
            Id = e.Id,
            StudentId = e.StudentId,
            StudentName = e.Student!.FullName,
            ClassId = e.ClassId,
            ClassName = e.Class!.Name,
            EnrolledAt = e.EnrolledAt,
            Status = e.Status
        };

        // ===================== Xem =====================

        public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsAsync(int? studentId, int? classId, string? status)
        {
            var query = _context.Enrollments.AsQueryable();

            if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
            if (classId.HasValue) query = query.Where(e => e.ClassId == classId.Value);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(e => e.Status == status);

            return await query
                .OrderByDescending(e => e.EnrolledAt)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<EnrollmentDto?> GetEnrollmentByIdAsync(int id)
        {
            return await _context.Enrollments
                .Where(e => e.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();
        }

        // ===================== T21: Ghi danh =====================

        public async Task<FeatureResult<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto)
        {
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null)
                return FeatureResult<EnrollmentDto>.NotFound($"Không tìm thấy học viên có Id = {dto.StudentId}");

            var targetClass = await _context.Classes.FindAsync(dto.ClassId);
            if (targetClass == null)
                return FeatureResult<EnrollmentDto>.NotFound($"Không tìm thấy lớp học có Id = {dto.ClassId}");

            var prepared = await PrepareEnrollmentAsync(student.Id, targetClass);
            if (!prepared.Success)
                return prepared.CastError<EnrollmentDto>();

            if (!await _context.TrySaveChangesAsync())
                return FeatureResult<EnrollmentDto>.Conflict("Dữ liệu vừa được người khác thay đổi (có thể đã bị trùng), vui lòng thử lại");

            var result = await GetEnrollmentByIdAsync(prepared.Data!.Id);
            return FeatureResult<EnrollmentDto>.Ok(result!);
        }

        // ===================== T22: Hủy / chuyển lớp =====================

        public async Task<FeatureResult<EnrollmentDto>> CancelAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return FeatureResult<EnrollmentDto>.NotFound($"Không tìm thấy ghi danh có Id = {id}");

            if (enrollment.Status != EnrollmentStatus.Active)
                return FeatureResult<EnrollmentDto>.Invalid("Chỉ có thể hủy ghi danh đang ở trạng thái Active");

            enrollment.Status = EnrollmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            var result = await GetEnrollmentByIdAsync(id);
            return FeatureResult<EnrollmentDto>.Ok(result!);
        }

        // Chuyển lớp = hủy ghi danh ở lớp cũ + ghi danh vào lớp mới (lưu cùng 1 lần SaveChanges).
        // Điểm danh / điểm số cũ vẫn gắn với ghi danh ở lớp cũ, lớp mới bắt đầu từ đầu.
        // Kết quả trả về là ghi danh MỚI (ở lớp mới).
        public async Task<FeatureResult<EnrollmentDto>> TransferAsync(int id, TransferEnrollmentDto dto)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return FeatureResult<EnrollmentDto>.NotFound($"Không tìm thấy ghi danh có Id = {id}");

            if (enrollment.Status != EnrollmentStatus.Active)
                return FeatureResult<EnrollmentDto>.Invalid("Chỉ có thể chuyển lớp khi ghi danh đang ở trạng thái Active");

            if (dto.NewClassId == enrollment.ClassId)
                return FeatureResult<EnrollmentDto>.Invalid("Lớp mới phải khác lớp hiện tại");

            var newClass = await _context.Classes.FindAsync(dto.NewClassId);
            if (newClass == null)
                return FeatureResult<EnrollmentDto>.NotFound($"Không tìm thấy lớp học có Id = {dto.NewClassId}");

            var prepared = await PrepareEnrollmentAsync(enrollment.StudentId, newClass);
            if (!prepared.Success)
                return prepared.CastError<EnrollmentDto>();

            enrollment.Status = EnrollmentStatus.Cancelled;
            if (!await _context.TrySaveChangesAsync())
                return FeatureResult<EnrollmentDto>.Conflict("Dữ liệu vừa được người khác thay đổi (có thể đã bị trùng), vui lòng thử lại");

            var result = await GetEnrollmentByIdAsync(prepared.Data!.Id);
            return FeatureResult<EnrollmentDto>.Ok(result!);
        }

        // ===================== Dùng chung nội bộ =====================

        // Kiểm tra luật ghi danh và chuẩn bị bản ghi (CHƯA SaveChanges):
        //  - đã ghi danh lớp này (Active/Completed) -> Conflict
        //  - lớp đủ sĩ số                           -> Conflict
        //  - từng hủy lớp này trước đó              -> bật lại bản ghi cũ (vì DB có unique StudentId + ClassId)
        //  - còn lại                                -> tạo bản ghi mới
        private async Task<FeatureResult<Enrollment>> PrepareEnrollmentAsync(int studentId, Class targetClass)
        {
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.ClassId == targetClass.Id);

            if (existing != null && existing.Status != EnrollmentStatus.Cancelled)
                return FeatureResult<Enrollment>.Conflict("Học viên đã ghi danh vào lớp này rồi");

            var activeCount = await _context.Enrollments
                .CountAsync(e => e.ClassId == targetClass.Id && e.Status == EnrollmentStatus.Active);
            if (activeCount >= targetClass.MaxStudents)
                return FeatureResult<Enrollment>.Conflict($"Lớp {targetClass.Name} đã đủ sĩ số ({targetClass.MaxStudents})");

            if (existing != null)
            {
                existing.Status = EnrollmentStatus.Active;
                existing.EnrolledAt = DateTime.Now;
                return FeatureResult<Enrollment>.Ok(existing);
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                ClassId = targetClass.Id,
                EnrolledAt = DateTime.Now,
                Status = EnrollmentStatus.Active
            };
            _context.Enrollments.Add(enrollment);
            return FeatureResult<Enrollment>.Ok(enrollment);
        }
    }
}

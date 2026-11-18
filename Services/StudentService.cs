using System.Linq.Expressions;
using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class StudentService : IStudentService
    {
        // Dùng AppDbContext trực tiếp (giống module Enrollment) để mọi bước kiểm tra
        // (trùng email, còn ghi danh...) chạy thành câu SQL dưới database,
        // không kéo cả bảng lên bộ nhớ rồi mới lọc.
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        // Chuyển Entity -> DTO ở 1 chỗ duy nhất; EF chỉ select đúng các cột này
        private static readonly Expression<Func<Student, StudentDto>> ToDto = s => new StudentDto
        {
            Id = s.Id,
            FullName = s.FullName,
            DateOfBirth = s.DateOfBirth,
            Phone = s.Phone,
            Email = s.Email,
            Level = s.Level,
            AvatarUrl = s.AvatarUrl
        };

        public async Task<List<StudentDto>> GetAllAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<StudentDto> GetByIdAsync(int id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            return student ?? throw new NotFoundException($"Không tìm thấy học viên có Id = {id}");
        }

        public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
        {
            var email = dto.Email!.Trim();
            await EnsureEmailNotUsedAsync(email, excludeId: null);

            var student = new Student
            {
                FullName = dto.FullName!.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                Email = email,
                Level = dto.Level
            };

            _context.Students.Add(student);

            // Hai request cùng lúc có thể cùng vượt qua bước kiểm tra trên;
            // khi đó unique index của database chặn lại và ta trả 409 thay vì 500.
            if (!await _context.TrySaveChangesAsync())
                throw new ConflictException("Email đã tồn tại");

            return await GetByIdAsync(student.Id);
        }

        public async Task UpdateAsync(int id, UpdateStudentDto dto)
        {
            var student = await _context.Students.FindAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy học viên có Id = {id}");

            var email = dto.Email!.Trim();
            await EnsureEmailNotUsedAsync(email, excludeId: id);

            student.FullName = dto.FullName!.Trim();
            student.DateOfBirth = dto.DateOfBirth;
            student.Phone = dto.Phone;
            student.Email = email;
            student.Level = dto.Level;

            if (!await _context.TrySaveChangesAsync())
                throw new ConflictException("Email đã tồn tại");
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy học viên có Id = {id}");

            if (await _context.Enrollments.AnyAsync(e => e.StudentId == id))
                throw new ConflictException("Học viên đã có ghi danh, không thể xóa");

            _context.Students.Remove(student);

            if (!await _context.TrySaveChangesAsync())
                throw new ConflictException("Không thể xóa học viên vì còn dữ liệu liên quan");
        }

        // Email không được trùng (không phân biệt hoa thường) với học viên khác.
        // Kiểm tra bằng 1 câu truy vấn AnyAsync dưới database.
        private async Task EnsureEmailNotUsedAsync(string email, int? excludeId)
        {
            var normalized = email.ToLower();

            var query = _context.Students.Where(s => s.Email!.ToLower() == normalized);
            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            if (await query.AnyAsync())
                throw new ConflictException("Email đã tồn tại");
        }
    }
}

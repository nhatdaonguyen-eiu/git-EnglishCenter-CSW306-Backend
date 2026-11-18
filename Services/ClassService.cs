using System.Linq.Expressions;
using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class ClassService : IClassService
    {
        // Dùng AppDbContext trực tiếp (giống module Enrollment) vì cần join sang Course và Teacher
        // để lấy tên. EF gộp thành 1 câu SQL, không phải tải cả bảng Course và Teacher lên bộ nhớ.
        private readonly AppDbContext _context;

        public ClassService(AppDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Class, ClassDto>> ToDto = c => new ClassDto
        {
            Id = c.Id,
            Name = c.Name,
            CourseId = c.CourseId,
            CourseName = c.Course!.Name,
            TeacherId = c.TeacherId,
            TeacherName = c.Teacher!.FullName,
            Schedule = c.Schedule,
            MaxStudents = c.MaxStudents,
            StartDate = c.StartDate,
            EndDate = c.EndDate
        };

        public async Task<List<ClassDto>> GetAllAsync()
        {
            return await _context.Classes
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<ClassDto> GetByIdAsync(int id)
        {
            var cls = await _context.Classes
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            return cls ?? throw new NotFoundException($"Không tìm thấy lớp học có Id = {id}");
        }

        public async Task<ClassDto> CreateAsync(CreateClassDto dto)
        {
            await ValidateAsync(dto);

            var cls = new Class
            {
                Name = dto.Name!.Trim(),
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
                Schedule = dto.Schedule,
                MaxStudents = dto.MaxStudents,
                StartDate = dto.StartDate!.Value,
                EndDate = dto.EndDate!.Value
            };

            _context.Classes.Add(cls);

            if (!await _context.TrySaveChangesAsync())
                throw new ConflictException("Không thể lưu lớp học vì dữ liệu bị ràng buộc");

            return await GetByIdAsync(cls.Id);
        }

        public async Task UpdateAsync(int id, UpdateClassDto dto)
        {
            var cls = await _context.Classes.FindAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy lớp học có Id = {id}");

            await ValidateAsync(dto);

            cls.Name = dto.Name!.Trim();
            cls.CourseId = dto.CourseId;
            cls.TeacherId = dto.TeacherId;
            cls.Schedule = dto.Schedule;
            cls.MaxStudents = dto.MaxStudents;
            cls.StartDate = dto.StartDate!.Value;
            cls.EndDate = dto.EndDate!.Value;

            if (!await _context.TrySaveChangesAsync())
                throw new ConflictException("Không thể lưu lớp học vì dữ liệu bị ràng buộc");
        }

        public async Task DeleteAsync(int id)
        {
            var cls = await _context.Classes.FindAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy lớp học có Id = {id}");

            if (await _context.Enrollments.AnyAsync(e => e.ClassId == id))
                throw new ConflictException("Lớp đã có học viên ghi danh, không thể xóa");

            _context.Classes.Remove(cls);

            if (!await _context.TrySaveChangesAsync())
                throw new ConflictException("Không thể xóa lớp học vì còn dữ liệu liên quan");
        }

        // ---------- T19: xem lớp theo khóa, theo giáo viên ----------

        public async Task<List<ClassDto>> GetByCourseAsync(int courseId)
        {
            if (!await _context.Courses.AnyAsync(c => c.Id == courseId))
                throw new NotFoundException($"Không tìm thấy khóa học có Id = {courseId}");

            return await _context.Classes
                .AsNoTracking()
                .Where(c => c.CourseId == courseId)
                .OrderBy(c => c.Id)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<List<ClassDto>> GetByTeacherAsync(int teacherId)
        {
            if (!await _context.Teachers.AnyAsync(t => t.Id == teacherId))
                throw new NotFoundException($"Không tìm thấy giáo viên có Id = {teacherId}");

            return await _context.Classes
                .AsNoTracking()
                .Where(c => c.TeacherId == teacherId)
                .OrderBy(c => c.Id)
                .Select(ToDto)
                .ToListAsync();
        }

        // ---------- helper ----------

        private async Task ValidateAsync(CreateClassDto dto)
        {
            if (dto.EndDate!.Value <= dto.StartDate!.Value)
                throw new BadRequestException("Ngày kết thúc phải sau ngày bắt đầu");

            var courseId = dto.CourseId!.Value;
            var teacherId = dto.TeacherId!.Value;

            if (!await _context.Courses.AnyAsync(c => c.Id == courseId))
                throw new NotFoundException($"Không tìm thấy khóa học có Id = {courseId}");

            if (!await _context.Teachers.AnyAsync(t => t.Id == teacherId))
                throw new NotFoundException($"Không tìm thấy giáo viên có Id = {teacherId}");
        }
    }
}

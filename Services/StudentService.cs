using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Repositories;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class StudentService : IStudentService
    {
        private readonly IRepository<Student> _repo;

        public StudentService(IRepository<Student> repo)
        {
            _repo = repo;
        }

        public async Task<List<StudentDto>> GetAllAsync()
        {
            var students = await _repo.GetAllAsync();
            return students.Select(ToDto).ToList();
        }

        public async Task<StudentDto> GetByIdAsync(int id)
        {
            var student = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy học viên có id {id}");
            return ToDto(student);
        }

        public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
        {
            await EnsureEmailNotUsedAsync(dto.Email!, excludeId: null);

            var student = new Student
            {
                FullName = dto.FullName!.Trim(),
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                Email = dto.Email!.Trim(),
                Level = dto.Level
            };

            await _repo.AddAsync(student);
            return ToDto(student);
        }

        public async Task<StudentDto> UpdateAsync(int id, UpdateStudentDto dto)
        {
            var student = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy học viên có id {id}");

            await EnsureEmailNotUsedAsync(dto.Email!, excludeId: id);

            student.FullName = dto.FullName!.Trim();
            student.DateOfBirth = dto.DateOfBirth;
            student.Phone = dto.Phone;
            student.Email = dto.Email!.Trim();
            student.Level = dto.Level;

            await _repo.UpdateAsync(student);
            return ToDto(student);
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy học viên có id {id}");

            await _repo.DeleteAsync(student.Id);
        }

        // Email không được trùng (không phân biệt hoa thường) với học viên khác.
        private async Task EnsureEmailNotUsedAsync(string email, int? excludeId)
        {
            var normalized = email.Trim().ToLowerInvariant();
            var all = await _repo.GetAllAsync();

            var duplicated = all.Any(s =>
                s.Id != excludeId &&
                s.Email != null &&
                s.Email.Trim().ToLowerInvariant() == normalized);

            if (duplicated)
            {
                throw new ConflictException("Email đã tồn tại");
            }
        }

        private static StudentDto ToDto(Student s) => new()
        {
            Id = s.Id,
            FullName = s.FullName,
            DateOfBirth = s.DateOfBirth,
            Phone = s.Phone,
            Email = s.Email,
            Level = s.Level,
            AvatarUrl = s.AvatarUrl
        };
    }
}

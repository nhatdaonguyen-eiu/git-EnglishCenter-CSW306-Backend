using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.DTOs.TeacherDTOs;
using EnglishCenterManagement_CSW306_Project.Repositories;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IRepository<Teacher> _teacherRepository;

        public TeacherService(IRepository<Teacher> teacherRepository)
        {
            _teacherRepository = teacherRepository;
        }

        public async Task<IEnumerable<TeacherDto>> GetAllTeachersAsync()
        {
            var teachers = await _teacherRepository.GetAllAsync();
            return teachers.Select(t => new TeacherDto
            {
                Id = t.Id,
                FullName = t.FullName,
                Specialization = t.Specialization,
                Phone = t.Phone,
                Email = t.Email
            }).ToList();
        }

        public async Task<TeacherDto?> GetTeacherByIdAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return null;

            return new TeacherDto
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                Specialization = teacher.Specialization,
                Phone = teacher.Phone,
                Email = teacher.Email
            };
        }

        public async Task<TeacherDto> CreateTeacherAsync(CreateUpdateTeacherDto dto)
        {
            var teacher = new Teacher
            {
                FullName = dto.FullName,
                Specialization = dto.Specialization,
                Phone = dto.Phone,
                Email = dto.Email
            };

            await _teacherRepository.AddAsync(teacher);

            return new TeacherDto
            {
                Id = teacher.Id,
                FullName = teacher.FullName,
                Specialization = teacher.Specialization,
                Phone = teacher.Phone,
                Email = teacher.Email
            };
        }

        public async Task<bool> UpdateTeacherAsync(int id, CreateUpdateTeacherDto dto)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return false;

            teacher.FullName = dto.FullName;
            teacher.Specialization = dto.Specialization;
            teacher.Phone = dto.Phone;
            teacher.Email = dto.Email;

            await _teacherRepository.UpdateAsync(teacher);
            return true;
        }

        public async Task<bool> DeleteTeacherAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return false;

            await _teacherRepository.DeleteAsync(id);
            return true;
        }
    }
}
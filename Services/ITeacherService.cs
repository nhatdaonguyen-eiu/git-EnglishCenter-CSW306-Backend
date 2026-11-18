using EnglishCenterManagement_CSW306_Project.DTOs.TeacherDTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherDto>> GetAllTeachersAsync();
        Task<TeacherDto?> GetTeacherByIdAsync(int id);
        Task<TeacherDto> CreateTeacherAsync(CreateUpdateTeacherDto dto);
        Task<bool> UpdateTeacherAsync(int id, CreateUpdateTeacherDto dto);
        Task<bool> DeleteTeacherAsync(int id);
    }
}
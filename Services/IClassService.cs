using EnglishCenterManagement_CSW306_Project.DTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IClassService
    {
        Task<List<ClassDto>> GetAllAsync();
        Task<ClassDto> GetByIdAsync(int id);
        Task<ClassDto> CreateAsync(CreateClassDto dto);
        Task UpdateAsync(int id, UpdateClassDto dto);
        Task DeleteAsync(int id);

        // T19
        Task<List<ClassDto>> GetByCourseAsync(int courseId);
        Task<List<ClassDto>> GetByTeacherAsync(int teacherId);
    }
}

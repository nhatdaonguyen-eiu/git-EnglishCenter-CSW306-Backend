using EnglishCenterManagement_CSW306_Project.DTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IStudentService
    {
        Task<List<StudentDto>> GetAllAsync();
        Task<StudentDto> GetByIdAsync(int id);
        Task<StudentDto> CreateAsync(CreateStudentDto dto);
        Task<StudentDto> UpdateAsync(int id, UpdateStudentDto dto);
        Task DeleteAsync(int id);
    }
}

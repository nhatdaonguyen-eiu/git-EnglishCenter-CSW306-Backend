using EnglishCenterManagement_CSW306_Project.DTOs.CourseDTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateUpdateCourseDto dto);
        Task<bool> UpdateCourseAsync(int id, CreateUpdateCourseDto dto);
        Task<bool> DeleteCourseAsync(int id);
    }
}
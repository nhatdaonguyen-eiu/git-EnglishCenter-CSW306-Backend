using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.DTOs.CourseDTOs;
using EnglishCenterManagement_CSW306_Project.Repositories;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course> _courseRepository;

        public CourseService(IRepository<Course> courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllAsync();
            return courses.Select(c => new CourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Fee = (decimal)c.Fee,
                TotalSessions = (int)c.TotalSessions
            }).ToList();
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return null;

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Fee = (decimal)course.Fee,
                TotalSessions = (int)course.TotalSessions
            };
        }

        public async Task<CourseDto> CreateCourseAsync(CreateUpdateCourseDto dto)
        {
            var course = new Course
            {
                Name = dto.Name,
                Description = dto.Description,
                Fee = dto.Fee,
                TotalSessions = dto.TotalSessions
            };

            await _courseRepository.AddAsync(course);

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Fee = (decimal)course.Fee,
                TotalSessions = (int)course.TotalSessions
            };
        }

        public async Task<bool> UpdateCourseAsync(int id, CreateUpdateCourseDto dto)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return false;

            course.Name = dto.Name;
            course.Description = dto.Description;
            course.Fee = dto.Fee;
            course.TotalSessions = dto.TotalSessions;

            await _courseRepository.UpdateAsync(course);
            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return false;

            await _courseRepository.DeleteAsync(id);
            return true;
        }
    }
}
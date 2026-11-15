using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Repositories;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class ClassService : IClassService
    {
        private readonly IRepository<Class> _classRepo;
        private readonly IRepository<Course> _courseRepo;
        private readonly IRepository<Teacher> _teacherRepo;

        public ClassService(
            IRepository<Class> classRepo,
            IRepository<Course> courseRepo,
            IRepository<Teacher> teacherRepo)
        {
            _classRepo = classRepo;
            _courseRepo = courseRepo;
            _teacherRepo = teacherRepo;
        }

        public async Task<List<ClassDto>> GetAllAsync()
        {
            var classes = await _classRepo.GetAllAsync();
            return await ToDtoListAsync(classes);
        }

        public async Task<ClassDto> GetByIdAsync(int id)
        {
            var cls = await _classRepo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy lớp có id {id}");
            return (await ToDtoListAsync(new[] { cls })).First();
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

            await _classRepo.AddAsync(cls);
            return await GetByIdAsync(cls.Id);
        }

        public async Task<ClassDto> UpdateAsync(int id, UpdateClassDto dto)
        {
            var cls = await _classRepo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy lớp có id {id}");

            await ValidateAsync(dto);

            cls.Name = dto.Name!.Trim();
            cls.CourseId = dto.CourseId;
            cls.TeacherId = dto.TeacherId;
            cls.Schedule = dto.Schedule;
            cls.MaxStudents = dto.MaxStudents;
            cls.StartDate = dto.StartDate!.Value;
            cls.EndDate = dto.EndDate!.Value;

            await _classRepo.UpdateAsync(cls);
            return await GetByIdAsync(cls.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var cls = await _classRepo.GetByIdAsync(id)
                ?? throw new NotFoundException($"Không tìm thấy lớp có id {id}");

            // Lớp đã có ghi danh sẽ vi phạm khóa ngoại -> middleware trả 409
            await _classRepo.DeleteAsync(cls.Id);
        }

        // ---------- T19: xem lớp theo khóa, theo giáo viên ----------

        public async Task<List<ClassDto>> GetByCourseAsync(int courseId)
        {
            _ = await _courseRepo.GetByIdAsync(courseId)
                ?? throw new NotFoundException($"Không tìm thấy khóa học có id {courseId}");

            var classes = (await _classRepo.GetAllAsync()).Where(c => c.CourseId == courseId);
            return await ToDtoListAsync(classes);
        }

        public async Task<List<ClassDto>> GetByTeacherAsync(int teacherId)
        {
            _ = await _teacherRepo.GetByIdAsync(teacherId)
                ?? throw new NotFoundException($"Không tìm thấy giáo viên có id {teacherId}");

            var classes = (await _classRepo.GetAllAsync()).Where(c => c.TeacherId == teacherId);
            return await ToDtoListAsync(classes);
        }

        // ---------- helper ----------

        private async Task ValidateAsync(CreateClassDto dto)
        {
            if (dto.EndDate!.Value <= dto.StartDate!.Value)
            {
                throw new BadRequestException("Ngày kết thúc phải sau ngày bắt đầu");
            }

            _ = await _courseRepo.GetByIdAsync(dto.CourseId!.Value)
                ?? throw new NotFoundException($"Không tìm thấy khóa học có id {dto.CourseId}");

            _ = await _teacherRepo.GetByIdAsync(dto.TeacherId!.Value)
                ?? throw new NotFoundException($"Không tìm thấy giáo viên có id {dto.TeacherId}");
        }

        // Repository không Include nên tự tra tên khóa học / giáo viên một lần cho cả danh sách.
        private async Task<List<ClassDto>> ToDtoListAsync(IEnumerable<Class> classes)
        {
            var courses = (await _courseRepo.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
            var teachers = (await _teacherRepo.GetAllAsync()).ToDictionary(t => t.Id, t => t.FullName);

            return classes.Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                CourseId = c.CourseId,
                CourseName = c.CourseId.HasValue && courses.TryGetValue(c.CourseId.Value, out var cn) ? cn : null,
                TeacherId = c.TeacherId,
                TeacherName = c.TeacherId.HasValue && teachers.TryGetValue(c.TeacherId.Value, out var tn) ? tn : null,
                Schedule = c.Schedule,
                MaxStudents = c.MaxStudents,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            }).ToList();
        }
    }
}

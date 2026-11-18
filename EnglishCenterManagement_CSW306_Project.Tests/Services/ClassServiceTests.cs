using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Tests.Services
{
    // T18, T19: Unit test ClassService (tên khóa/giáo viên, lọc theo khóa/giáo viên, kiểm tra dữ liệu đầu vào)
    public class ClassServiceTests
    {
        // Tạo sẵn: 2 khóa (1 IELTS, 2 TOEIC), 2 giáo viên (1 Hoa, 2 Minh),
        // 3 lớp: Id 1 (khóa 1, GV 1), Id 2 (khóa 2, GV 2), Id 3 (khóa 1, GV 2); học viên 1 ghi danh lớp 1
        private static AppDbContext CreateContextWithData()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);

            context.Courses.AddRange(
                new Course { Id = 1, Name = "IELTS", Fee = 1000000, TotalSessions = 20 },
                new Course { Id = 2, Name = "TOEIC", Fee = 900000, TotalSessions = 20 });
            context.Teachers.AddRange(
                new Teacher { Id = 1, FullName = "Co Hoa", Email = "hoa@test.com" },
                new Teacher { Id = 2, FullName = "Thay Minh", Email = "minh@test.com" });

            var start = DateTime.Today;
            var end = DateTime.Today.AddMonths(3);
            context.Classes.AddRange(
                new Class { Id = 1, Name = "Lop A", CourseId = 1, TeacherId = 1, MaxStudents = 5, Schedule = "T2", StartDate = start, EndDate = end },
                new Class { Id = 2, Name = "Lop B", CourseId = 2, TeacherId = 2, MaxStudents = 5, Schedule = "T3", StartDate = start, EndDate = end },
                new Class { Id = 3, Name = "Lop C", CourseId = 1, TeacherId = 2, MaxStudents = 5, Schedule = "T4", StartDate = start, EndDate = end });

            context.Students.Add(new Student { Id = 1, FullName = "An", Email = "an@test.com", DateOfBirth = new DateTime(2005, 1, 1) });
            context.Enrollments.Add(new Enrollment { Id = 1, StudentId = 1, ClassId = 1, EnrolledAt = DateTime.Now, Status = "Active" });
            context.SaveChanges();
            return context;
        }

        private static CreateClassDto NewDto(int courseId = 1, int teacherId = 1) => new()
        {
            Name = "Lop moi",
            CourseId = courseId,
            TeacherId = teacherId,
            Schedule = "T7",
            MaxStudents = 10,
            StartDate = new DateTime(2027, 1, 1),
            EndDate = new DateTime(2027, 4, 1)
        };

        // ---------- Xem ----------

        [Fact]
        public async Task GetAll_ReturnsCourseAndTeacherNames()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            var list = await service.GetAllAsync();

            Assert.Equal(3, list.Count);
            var first = list.Single(c => c.Id == 1);
            Assert.Equal("IELTS", first.CourseName);
            Assert.Equal("Co Hoa", first.TeacherName);
        }

        [Fact]
        public async Task GetById_NotFound_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
        }

        // ---------- T19: lọc theo khóa / giáo viên ----------

        [Fact]
        public async Task GetByCourse_ReturnsOnlyClassesOfThatCourse()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            var list = await service.GetByCourseAsync(1);

            Assert.Equal(new[] { 1, 3 }, list.Select(c => c.Id).ToArray());
        }

        [Fact]
        public async Task GetByCourse_UnknownCourse_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByCourseAsync(99));
        }

        [Fact]
        public async Task GetByTeacher_ReturnsOnlyClassesOfThatTeacher()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            var list = await service.GetByTeacherAsync(2);

            Assert.Equal(new[] { 2, 3 }, list.Select(c => c.Id).ToArray());
        }

        [Fact]
        public async Task GetByTeacher_UnknownTeacher_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByTeacherAsync(99));
        }

        // ---------- Tạo mới ----------

        [Fact]
        public async Task Create_ValidData_ReturnsClassWithNames()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            var created = await service.CreateAsync(NewDto(courseId: 2, teacherId: 1));

            Assert.True(created.Id > 0);
            Assert.Equal("TOEIC", created.CourseName);
            Assert.Equal("Co Hoa", created.TeacherName);
            Assert.Equal(4, context.Classes.Count());
        }

        [Fact]
        public async Task Create_UnknownCourse_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(NewDto(courseId: 99)));
        }

        [Fact]
        public async Task Create_UnknownTeacher_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(NewDto(teacherId: 99)));
        }

        [Fact]
        public async Task Create_EndDateBeforeStartDate_ThrowsBadRequest()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            var dto = NewDto();
            dto.EndDate = new DateTime(2026, 12, 1); // trước ngày bắt đầu

            await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAsync(dto));
        }

        // ---------- Xóa ----------

        [Fact]
        public async Task Delete_ClassWithEnrollment_ThrowsConflict()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(1));

            Assert.Equal(3, context.Classes.Count());
        }

        [Fact]
        public async Task Delete_ClassWithoutEnrollment_RemovesClass()
        {
            using var context = CreateContextWithData();
            var service = new ClassService(context);

            await service.DeleteAsync(2);

            Assert.Equal(2, context.Classes.Count());
        }
    }
}

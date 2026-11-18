using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Tests.Services
{
    // T20: Unit test StudentService (email không trùng, không tìm thấy, xóa khi còn ghi danh)
    // Mỗi test dùng 1 database InMemory riêng, không đụng đến SQL Server thật.
    public class StudentServiceTests
    {
        // Tạo sẵn: 1 khóa, 1 giáo viên, 1 lớp, 3 học viên; học viên 1 đã ghi danh lớp 1
        private static AppDbContext CreateContextWithData()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);

            context.Courses.Add(new Course { Id = 1, Name = "IELTS", Fee = 1000000, TotalSessions = 20 });
            context.Teachers.Add(new Teacher { Id = 1, FullName = "Co Hoa", Email = "hoa@test.com" });
            context.Classes.Add(new Class { Id = 1, Name = "Lop A", CourseId = 1, TeacherId = 1, MaxStudents = 5, Schedule = "T2", StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(3) });
            context.Students.AddRange(
                new Student { Id = 1, FullName = "An", Email = "an@test.com", DateOfBirth = new DateTime(2005, 1, 1) },
                new Student { Id = 2, FullName = "Bao", Email = "bao@test.com", DateOfBirth = new DateTime(2005, 2, 2) },
                new Student { Id = 3, FullName = "Chau", Email = "chau@test.com", DateOfBirth = new DateTime(2005, 3, 3) });
            context.Enrollments.Add(new Enrollment { Id = 1, StudentId = 1, ClassId = 1, EnrolledAt = DateTime.Now, Status = "Active" });
            context.SaveChanges();
            return context;
        }

        private static CreateStudentDto NewDto(string email) => new()
        {
            FullName = "Nguyen Van A",
            DateOfBirth = new DateTime(2003, 1, 1),
            Email = email,
            Level = "A1"
        };

        // ---------- Tạo mới ----------

        [Fact]
        public async Task Create_DuplicateEmail_ThrowsConflict()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(NewDto("an@test.com")));

            Assert.Equal(3, context.Students.Count()); // không tạo thêm bản ghi
        }

        [Fact]
        public async Task Create_DuplicateEmailDifferentCase_ThrowsConflict()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(NewDto("  AN@Test.COM ")));
        }

        [Fact]
        public async Task Create_NewEmail_SavesAndReturnsStudent()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            var created = await service.CreateAsync(NewDto("moi@test.com"));

            Assert.True(created.Id > 0);
            Assert.Equal("moi@test.com", created.Email);
            Assert.Equal(4, context.Students.Count());
        }

        // ---------- Xem ----------

        [Fact]
        public async Task GetAll_ReturnsAllStudents()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            var list = await service.GetAllAsync();

            Assert.Equal(3, list.Count);
        }

        [Fact]
        public async Task GetById_NotFound_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(99));
        }

        // ---------- Sửa ----------

        [Fact]
        public async Task Update_EmailOfAnotherStudent_ThrowsConflict()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            var dto = new UpdateStudentDto
            {
                FullName = "An",
                DateOfBirth = new DateTime(2005, 1, 1),
                Email = "bao@test.com" // email của học viên 2
            };

            await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(1, dto));
        }

        [Fact]
        public async Task Update_KeepOwnEmail_Succeeds()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            var dto = new UpdateStudentDto
            {
                FullName = "An (đã sửa)",
                DateOfBirth = new DateTime(2005, 1, 1),
                Email = "an@test.com" // giữ nguyên email của chính mình
            };

            await service.UpdateAsync(1, dto);

            var updated = await service.GetByIdAsync(1);
            Assert.Equal("An (đã sửa)", updated.FullName);
        }

        [Fact]
        public async Task Update_NotFound_ThrowsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            var dto = new UpdateStudentDto { FullName = "X", DateOfBirth = new DateTime(2005, 1, 1), Email = "x@test.com" };

            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, dto));
        }

        // ---------- Xóa ----------

        [Fact]
        public async Task Delete_StudentWithEnrollment_ThrowsConflict()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(1));

            Assert.Equal(3, context.Students.Count());
        }

        [Fact]
        public async Task Delete_StudentWithoutEnrollment_RemovesStudent()
        {
            using var context = CreateContextWithData();
            var service = new StudentService(context);

            await service.DeleteAsync(3);

            Assert.Equal(2, context.Students.Count());
        }
    }
}

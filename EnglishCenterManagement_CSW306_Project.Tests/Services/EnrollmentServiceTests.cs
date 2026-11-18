using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Tests.Services
{
    // T26: Unit test EnrollmentService (chặn trùng, kiểm tra sĩ số, hủy, chuyển lớp)
    // Mỗi test dùng 1 database InMemory riêng, không đụng đến SQL Server thật.
    public class EnrollmentServiceTests
    {
        // ---------- Dữ liệu giả cho test ----------

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        // Tạo sẵn: 1 khóa, 1 giáo viên, 3 học viên (Id 1, 2, 3), 2 lớp (Id 1: tối đa maxStudentsClassA, Id 2: tối đa 5)
        private static AppDbContext CreateContextWithData(int maxStudentsClassA = 2)
        {
            var context = CreateContext();

            context.Courses.Add(new Course { Id = 1, Name = "IELTS", Fee = 1000000, TotalSessions = 20 });
            context.Teachers.Add(new Teacher { Id = 1, FullName = "Co Hoa", Email = "hoa@test.com" });

            context.Students.AddRange(
                new Student { Id = 1, FullName = "An", Email = "an@test.com", DateOfBirth = new DateTime(2005, 1, 1) },
                new Student { Id = 2, FullName = "Bao", Email = "bao@test.com", DateOfBirth = new DateTime(2005, 2, 2) },
                new Student { Id = 3, FullName = "Chau", Email = "chau@test.com", DateOfBirth = new DateTime(2005, 3, 3) });

            // InMemory kiểm tra thuộc tính [Required], nên Class phải có CourseId và TeacherId
            context.Classes.AddRange(
                new Class { Id = 1, Name = "Lop A", CourseId = 1, TeacherId = 1, MaxStudents = maxStudentsClassA, Schedule = "T2", StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(3) },
                new Class { Id = 2, Name = "Lop B", CourseId = 1, TeacherId = 1, MaxStudents = 5, Schedule = "T3", StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(3) });

            context.SaveChanges();
            return context;
        }

        private static CreateEnrollmentDto Dto(int studentId, int classId) =>
            new() { StudentId = studentId, ClassId = classId };

        // ---------- Ghi danh ----------

        [Fact]
        public async Task Enroll_ValidStudentAndClass_ReturnsActiveEnrollment()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);

            var result = await service.EnrollAsync(Dto(1, 1));

            Assert.True(result.Success);
            Assert.Equal(EnrollmentStatus.Active, result.Data!.Status);
            Assert.Equal("An", result.Data.StudentName);
            Assert.Equal("Lop A", result.Data.ClassName);
            Assert.Equal(1, await context.Enrollments.CountAsync());
        }

        [Fact]
        public async Task Enroll_StudentNotFound_ReturnsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);

            var result = await service.EnrollAsync(Dto(999, 1));

            Assert.False(result.Success);
            Assert.Equal(ResultError.NotFound, result.Error);
        }

        [Fact]
        public async Task Enroll_ClassNotFound_ReturnsNotFound()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);

            var result = await service.EnrollAsync(Dto(1, 999));

            Assert.False(result.Success);
            Assert.Equal(ResultError.NotFound, result.Error);
        }

        [Fact]
        public async Task Enroll_SameStudentSameClassTwice_ReturnsConflict()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            await service.EnrollAsync(Dto(1, 1));

            var result = await service.EnrollAsync(Dto(1, 1));

            Assert.False(result.Success);
            Assert.Equal(ResultError.Conflict, result.Error);
            Assert.Equal(1, await context.Enrollments.CountAsync()); // không tạo thêm bản ghi
        }

        [Fact]
        public async Task Enroll_ClassIsFull_ReturnsConflict()
        {
            using var context = CreateContextWithData(maxStudentsClassA: 2);
            var service = new EnrollmentService(context);
            await service.EnrollAsync(Dto(1, 1));
            await service.EnrollAsync(Dto(2, 1)); // lớp A đã đủ 2/2

            var result = await service.EnrollAsync(Dto(3, 1));

            Assert.False(result.Success);
            Assert.Equal(ResultError.Conflict, result.Error);
            Assert.Equal(2, await context.Enrollments.CountAsync());
        }

        [Fact]
        public async Task Enroll_AfterCancel_ReusesOldRecord()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            var first = await service.EnrollAsync(Dto(1, 1));
            await service.CancelAsync(first.Data!.Id);

            var again = await service.EnrollAsync(Dto(1, 1));

            Assert.True(again.Success);
            Assert.Equal(first.Data.Id, again.Data!.Id);                 // dùng lại bản ghi cũ
            Assert.Equal(EnrollmentStatus.Active, again.Data.Status);
            Assert.Equal(1, await context.Enrollments.CountAsync());
        }

        // ---------- Hủy ghi danh ----------

        [Fact]
        public async Task Cancel_ActiveEnrollment_SetsStatusCancelled()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            var enrolled = await service.EnrollAsync(Dto(1, 1));

            var result = await service.CancelAsync(enrolled.Data!.Id);

            Assert.True(result.Success);
            Assert.Equal(EnrollmentStatus.Cancelled, result.Data!.Status);
        }

        [Fact]
        public async Task Cancel_AlreadyCancelled_ReturnsInvalid()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            var enrolled = await service.EnrollAsync(Dto(1, 1));
            await service.CancelAsync(enrolled.Data!.Id);

            var result = await service.CancelAsync(enrolled.Data.Id);

            Assert.False(result.Success);
            Assert.Equal(ResultError.Invalid, result.Error);
        }

        [Fact]
        public async Task Cancel_FreesUpSlotInFullClass()
        {
            using var context = CreateContextWithData(maxStudentsClassA: 1);
            var service = new EnrollmentService(context);
            var first = await service.EnrollAsync(Dto(1, 1)); // lớp A đầy 1/1
            await service.CancelAsync(first.Data!.Id);

            var result = await service.EnrollAsync(Dto(2, 1)); // học viên khác vào được

            Assert.True(result.Success);
        }

        // ---------- Chuyển lớp ----------

        [Fact]
        public async Task Transfer_ToAnotherClass_CancelsOldAndCreatesNew()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            var enrolled = await service.EnrollAsync(Dto(1, 1));

            var result = await service.TransferAsync(enrolled.Data!.Id, new TransferEnrollmentDto { NewClassId = 2 });

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.ClassId);
            Assert.Equal(EnrollmentStatus.Active, result.Data.Status);

            var old = await service.GetEnrollmentByIdAsync(enrolled.Data.Id);
            Assert.Equal(EnrollmentStatus.Cancelled, old!.Status);
        }

        [Fact]
        public async Task Transfer_ToFullClass_ReturnsConflictAndKeepsOldEnrollment()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            context.Classes.Find(2)!.MaxStudents = 1;
            await context.SaveChangesAsync();
            await service.EnrollAsync(Dto(2, 2));                 // lớp B đầy 1/1
            var enrolled = await service.EnrollAsync(Dto(1, 1));  // An học lớp A

            var result = await service.TransferAsync(enrolled.Data!.Id, new TransferEnrollmentDto { NewClassId = 2 });

            Assert.False(result.Success);
            Assert.Equal(ResultError.Conflict, result.Error);

            var old = await service.GetEnrollmentByIdAsync(enrolled.Data.Id);
            Assert.Equal(EnrollmentStatus.Active, old!.Status);   // chưa bị hủy
        }

        [Fact]
        public async Task Transfer_ToSameClass_ReturnsInvalid()
        {
            using var context = CreateContextWithData();
            var service = new EnrollmentService(context);
            var enrolled = await service.EnrollAsync(Dto(1, 1));

            var result = await service.TransferAsync(enrolled.Data!.Id, new TransferEnrollmentDto { NewClassId = 1 });

            Assert.False(result.Success);
            Assert.Equal(ResultError.Invalid, result.Error);
        }
    }
}

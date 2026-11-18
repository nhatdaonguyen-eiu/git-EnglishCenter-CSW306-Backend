using System.ComponentModel.DataAnnotations;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Tests.Services
{
    // Kiểm tra điểm phải nằm trong khoảng 0-10 (lỗi do tester báo: 10.4 và -0.4 vẫn được nhận)
    public class ScoreServiceTests
    {
        private static AppDbContext CreateContextWithEnrollment()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);

            context.Courses.Add(new Course { Id = 1, Name = "IELTS", Fee = 1000000, TotalSessions = 20 });
            context.Teachers.Add(new Teacher { Id = 1, FullName = "Co Hoa", Email = "hoa@test.com" });
            context.Students.Add(new Student { Id = 1, FullName = "An", Email = "an@test.com", DateOfBirth = new DateTime(2005, 1, 1) });
            context.Classes.Add(new Class { Id = 1, Name = "Lop A", CourseId = 1, TeacherId = 1, MaxStudents = 5, Schedule = "T2", StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(3) });
            context.Enrollments.Add(new Enrollment { Id = 1, StudentId = 1, ClassId = 1, EnrolledAt = DateTime.Now, Status = "Active" });
            context.SaveChanges();
            return context;
        }

        private static bool PassesValidation(object dto) =>
            Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), validateAllProperties: true);

        private static readonly decimal[] BadValues = { -1m, -0.5m, -0.4m, 10.4m, 10.5m, 11m };
        private static readonly decimal[] GoodValues = { 0m, 0.01m, 5.5m, 9.99m, 10m };

        // ---------- Kiểm tra attribute [Range] trong DTO ----------

        [Fact]
        public void CreateScoreDto_Range_RejectsOutOfRangeValues()
        {
            foreach (var v in BadValues)
                Assert.False(PassesValidation(new CreateScoreDto { EnrollmentId = 1, Type = "Midterm", Value = v }));
            foreach (var v in GoodValues)
                Assert.True(PassesValidation(new CreateScoreDto { EnrollmentId = 1, Type = "Midterm", Value = v }));
        }

        [Fact]
        public void UpdateScoreDto_Range_RejectsOutOfRangeValues()
        {
            foreach (var v in BadValues)
                Assert.False(PassesValidation(new UpdateScoreDto { Value = v }));
            foreach (var v in GoodValues)
                Assert.True(PassesValidation(new UpdateScoreDto { Value = v }));
        }

        // ---------- Kiểm tra trong ScoreService ----------

        [Fact]
        public async Task CreateScore_OutOfRange_ReturnsInvalidAndSavesNothing()
        {
            using var context = CreateContextWithEnrollment();
            var service = new ScoreService(context);

            foreach (var v in BadValues)
            {
                var result = await service.CreateScoreAsync(new CreateScoreDto { EnrollmentId = 1, Type = "Midterm", Value = v });

                Assert.False(result.Success);
                Assert.Equal(ResultError.Invalid, result.Error);
            }
            Assert.Equal(0, await context.Scores.CountAsync());
        }

        [Fact]
        public async Task CreateScore_BoundaryValues_AreAccepted()
        {
            using var context = CreateContextWithEnrollment();
            var service = new ScoreService(context);

            var zero = await service.CreateScoreAsync(new CreateScoreDto { EnrollmentId = 1, Type = "Midterm", Value = 0 });
            var ten = await service.CreateScoreAsync(new CreateScoreDto { EnrollmentId = 1, Type = "Final", Value = 10 });

            Assert.True(zero.Success);
            Assert.True(ten.Success);
        }

        [Fact]
        public async Task UpdateScore_OutOfRange_ReturnsInvalidAndKeepsOldValue()
        {
            using var context = CreateContextWithEnrollment();
            var service = new ScoreService(context);
            var created = await service.CreateScoreAsync(new CreateScoreDto { EnrollmentId = 1, Type = "Midterm", Value = 7 });

            foreach (var v in BadValues)
            {
                var result = await service.UpdateScoreAsync(created.Data!.Id, new UpdateScoreDto { Value = v });

                Assert.False(result.Success);
                Assert.Equal(ResultError.Invalid, result.Error);
            }
            var current = await service.GetScoreByIdAsync(created.Data!.Id);
            Assert.Equal(7m, current!.Value);
        }
    }
}

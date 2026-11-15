using EnglishCenterManagement_CSW306_Project.Common;
using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Repositories;
using EnglishCenterManagement_CSW306_Project.Services;
using Moq;
using Xunit;

namespace EnglishCenter.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IRepository<Student>> _repo = new();
        private readonly StudentService _service;

        public StudentServiceTests()
        {
            _service = new StudentService(_repo.Object);
        }

        private static List<Student> ExistingStudents() => new()
        {
            new Student { Id = 1, FullName = "Lê Hoàng An", Email = "an.le@student.example", DateOfBirth = new DateTime(2005, 3, 12) },
            new Student { Id = 2, FullName = "Phạm Gia Bảo", Email = "bao.pham@student.example", DateOfBirth = new DateTime(2004, 7, 25) }
        };

        private static CreateStudentDto NewDto(string email) => new()
        {
            FullName = "Nguyễn Văn A",
            DateOfBirth = new DateTime(2003, 1, 1),
            Email = email,
            Level = "A1"
        };

        [Fact]
        public async Task CreateAsync_DuplicateEmail_ThrowsConflict()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(ExistingStudents());

            await Assert.ThrowsAsync<ConflictException>(
                () => _service.CreateAsync(NewDto("an.le@student.example")));

            _repo.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_DuplicateEmailDifferentCase_ThrowsConflict()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(ExistingStudents());

            await Assert.ThrowsAsync<ConflictException>(
                () => _service.CreateAsync(NewDto("AN.LE@Student.Example")));
        }

        [Fact]
        public async Task CreateAsync_NewEmail_AddsStudent()
        {
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(ExistingStudents());

            var result = await _service.CreateAsync(NewDto("moi@student.example"));

            Assert.Equal("moi@student.example", result.Email);
            _repo.Verify(r => r.AddAsync(It.Is<Student>(s => s.Email == "moi@student.example")), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ThrowsNotFound()
        {
            _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Student?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(99));
        }

        [Fact]
        public async Task UpdateAsync_EmailUsedByAnotherStudent_ThrowsConflict()
        {
            var students = ExistingStudents();
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(students[0]);
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(students);

            var dto = new UpdateStudentDto
            {
                FullName = "Lê Hoàng An",
                DateOfBirth = new DateTime(2005, 3, 12),
                Email = "bao.pham@student.example"   // email của học viên 2
            };

            await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateAsync(1, dto));
        }

        [Fact]
        public async Task UpdateAsync_KeepOwnEmail_Succeeds()
        {
            var students = ExistingStudents();
            _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(students[0]);
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(students);

            var dto = new UpdateStudentDto
            {
                FullName = "Lê Hoàng An (đã sửa)",
                DateOfBirth = new DateTime(2005, 3, 12),
                Email = "an.le@student.example"      // giữ nguyên email của chính mình
            };

            var result = await _service.UpdateAsync(1, dto);

            Assert.Equal("Lê Hoàng An (đã sửa)", result.FullName);
            _repo.Verify(r => r.UpdateAsync(It.IsAny<Student>()), Times.Once);
        }
    }
}

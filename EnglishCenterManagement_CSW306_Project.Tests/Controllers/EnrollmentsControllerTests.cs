using EnglishCenterManagement_CSW306_Project.Controllers;
using EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EnglishCenterManagement_CSW306_Project.Tests.Controllers
{
    // Unit test Controller với Service giả (Moq): không cần database,
    // chỉ kiểm tra Controller đổi kết quả của Service thành đúng mã HTTP.
    public class EnrollmentsControllerTests
    {
        private readonly Mock<IEnrollmentService> _service = new();
        private readonly EnrollmentsController _controller;

        public EnrollmentsControllerTests()
        {
            _controller = new EnrollmentsController(_service.Object);
        }

        // Body lỗi có dạng { message = "..." }
        private static string? MessageOf(ObjectResult result) =>
            result.Value!.GetType().GetProperty("message")!.GetValue(result.Value) as string;

        [Fact]
        public async Task Enroll_ServiceSucceeds_Returns201PointingToGetById()
        {
            var dto = new CreateEnrollmentDto { StudentId = 3, ClassId = 1 };
            var created = new EnrollmentDto { Id = 7, StudentId = 3, ClassId = 1, Status = "Active" };
            _service.Setup(s => s.EnrollAsync(dto)).ReturnsAsync(FeatureResult<EnrollmentDto>.Ok(created));

            var result = await _controller.Enroll(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(EnrollmentsController.GetEnrollmentById), createdResult.ActionName);
            Assert.Equal(7, createdResult.RouteValues!["id"]);
            Assert.Same(created, createdResult.Value);
        }

        [Fact]
        public async Task Enroll_ServiceReportsConflict_Returns409WithServiceMessage()
        {
            var dto = new CreateEnrollmentDto { StudentId = 1, ClassId = 1 };
            _service.Setup(s => s.EnrollAsync(dto))
                .ReturnsAsync(FeatureResult<EnrollmentDto>.Conflict("Học viên đã ghi danh vào lớp này rồi"));

            var result = await _controller.Enroll(dto);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Học viên đã ghi danh vào lớp này rồi", MessageOf(conflict));
        }

        [Fact]
        public async Task GetEnrollmentById_ServiceReturnsNull_Returns404()
        {
            _service.Setup(s => s.GetEnrollmentByIdAsync(99)).ReturnsAsync((EnrollmentDto?)null);

            var result = await _controller.GetEnrollmentById(99);

            Assert.IsType<NotFoundObjectResult>(result);
            _service.Verify(s => s.GetEnrollmentByIdAsync(99), Times.Once);
        }
    }
}

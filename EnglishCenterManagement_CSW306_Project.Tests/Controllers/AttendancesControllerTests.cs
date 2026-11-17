using EnglishCenterManagement_CSW306_Project.Controllers;
using EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EnglishCenterManagement_CSW306_Project.Tests.Controllers
{
    public class AttendancesControllerTests
    {
        private readonly Mock<IAttendanceService> _service = new();
        private readonly AttendancesController _controller;

        public AttendancesControllerTests()
        {
            _controller = new AttendancesController(_service.Object);
        }

        [Fact]
        public async Task GetAttendances_PassesFiltersToServiceAndReturns200()
        {
            var date = new DateTime(2026, 10, 5);
            var rows = new List<AttendanceDto> { new() { Id = 1, EnrollmentId = 1, Status = "Present" } };
            _service.Setup(s => s.GetAttendancesAsync(null, 1, null, date)).ReturnsAsync(rows);

            var result = await _controller.GetAttendances(null, 1, null, date);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(rows, ok.Value);
            _service.Verify(s => s.GetAttendancesAsync(null, 1, null, date), Times.Once);
        }

        [Fact]
        public async Task DeleteAttendance_ServiceSucceeds_Returns204()
        {
            _service.Setup(s => s.DeleteAttendanceAsync(4)).ReturnsAsync(FeatureResult<bool>.Ok(true));

            var result = await _controller.DeleteAttendance(4);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAttendance_NotFound_Returns404()
        {
            _service.Setup(s => s.DeleteAttendanceAsync(99))
                .ReturnsAsync(FeatureResult<bool>.NotFound("Không tìm thấy bản ghi điểm danh có Id = 99"));

            var result = await _controller.DeleteAttendance(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}

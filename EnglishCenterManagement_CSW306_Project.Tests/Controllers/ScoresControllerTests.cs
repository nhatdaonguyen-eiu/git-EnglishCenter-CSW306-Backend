using EnglishCenterManagement_CSW306_Project.Controllers;
using EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EnglishCenterManagement_CSW306_Project.Tests.Controllers
{
    public class ScoresControllerTests
    {
        private readonly Mock<IScoreService> _service = new();
        private readonly ScoresController _controller;

        public ScoresControllerTests()
        {
            _controller = new ScoresController(_service.Object);
        }

        [Fact]
        public async Task CreateScore_ServiceReportsInvalid_Returns400()
        {
            var dto = new CreateScoreDto { EnrollmentId = 1, Type = "Quiz", Value = 8 };
            _service.Setup(s => s.CreateScoreAsync(dto))
                .ReturnsAsync(FeatureResult<ScoreDto>.Invalid("Loại điểm không hợp lệ"));

            var result = await _controller.CreateScore(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateScore_ServiceSucceeds_Returns204AndPassesIdAndBodyToService()
        {
            var dto = new UpdateScoreDto { Value = 9, Comment = "Tiến bộ" };
            _service.Setup(s => s.UpdateScoreAsync(5, dto)).ReturnsAsync(FeatureResult<bool>.Ok(true));

            var result = await _controller.UpdateScore(5, dto);

            Assert.IsType<NoContentResult>(result);
            _service.Verify(s => s.UpdateScoreAsync(5, dto), Times.Once);
        }

        [Fact]
        public async Task GetTranscript_StudentNotFound_Returns404()
        {
            _service.Setup(s => s.GetTranscriptAsync(99))
                .ReturnsAsync(FeatureResult<TranscriptDto>.NotFound("Không tìm thấy học viên có Id = 99"));

            var result = await _controller.GetTranscript(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}

using EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    // TODO (sau khi TV2 merge JWT): bật phân quyền
    // Admin + Teacher: nhập/sửa/xóa điểm. Student: chỉ xem bảng điểm của mình.
    // [Authorize(Roles = "Admin,Teacher")]
    [Route("api/[controller]")]
    public class ScoresController : FeatureControllerBase
    {
        private readonly IScoreService _scoreService;

        public ScoresController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        // GET: api/Scores?classId=1&type=Final
        [HttpGet]
        public async Task<IActionResult> GetScores(
            [FromQuery] int? enrollmentId, [FromQuery] int? classId,
            [FromQuery] int? studentId, [FromQuery] string? type)
        {
            var scores = await _scoreService.GetScoresAsync(enrollmentId, classId, studentId, type);
            return Ok(scores);
        }

        // GET: api/Scores/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetScoreById(int id)
        {
            var score = await _scoreService.GetScoreByIdAsync(id);
            if (score == null)
            {
                return NotFound(new { message = $"Không tìm thấy điểm có Id = {id}" });
            }
            return Ok(score);
        }

        // POST: api/Scores
        [HttpPost]
        public async Task<IActionResult> CreateScore([FromBody] CreateScoreDto dto)
        {
            var result = await _scoreService.CreateScoreAsync(dto);
            if (!result.Success) return ErrorResponse(result);

            return CreatedAtAction(nameof(GetScoreById), new { id = result.Data!.Id }, result.Data);
        }

        // PUT: api/Scores/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateScore(int id, [FromBody] UpdateScoreDto dto)
        {
            var result = await _scoreService.UpdateScoreAsync(id, dto);
            if (!result.Success) return ErrorResponse(result);

            return NoContent();
        }

        // DELETE: api/Scores/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteScore(int id)
        {
            var result = await _scoreService.DeleteScoreAsync(id);
            if (!result.Success) return ErrorResponse(result);

            return NoContent();
        }

        // GET: api/Scores/transcript/student/3  -> bảng điểm của 1 học viên
        [HttpGet("transcript/student/{studentId:int}")]
        public async Task<IActionResult> GetTranscript(int studentId)
        {
            var result = await _scoreService.GetTranscriptAsync(studentId);
            if (!result.Success) return ErrorResponse(result);

            return Ok(result.Data);
        }
    }
}

using EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IScoreService
    {
        // T25: CRUD điểm số
        Task<IEnumerable<ScoreDto>> GetScoresAsync(int? enrollmentId, int? classId, int? studentId, string? type);
        Task<ScoreDto?> GetScoreByIdAsync(int id);
        Task<FeatureResult<ScoreDto>> CreateScoreAsync(CreateScoreDto dto);
        Task<FeatureResult<bool>> UpdateScoreAsync(int id, UpdateScoreDto dto);
        Task<FeatureResult<bool>> DeleteScoreAsync(int id);

        // T25: bảng điểm của 1 học viên
        Task<FeatureResult<TranscriptDto>> GetTranscriptAsync(int studentId);
    }
}

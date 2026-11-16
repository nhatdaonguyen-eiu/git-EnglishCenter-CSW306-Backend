using EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IEnrollmentService
    {
        Task<IEnumerable<EnrollmentDto>> GetEnrollmentsAsync(int? studentId, int? classId, string? status);
        Task<EnrollmentDto?> GetEnrollmentByIdAsync(int id);

        // T21: ghi danh (chặn trùng, kiểm tra sĩ số)
        Task<FeatureResult<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto);

        // T22: hủy ghi danh, chuyển lớp
        Task<FeatureResult<EnrollmentDto>> CancelAsync(int id);
        Task<FeatureResult<EnrollmentDto>> TransferAsync(int id, TransferEnrollmentDto dto);
    }
}

using EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IAttendanceService
    {
        // T23: CRUD điểm danh theo buổi học
        Task<IEnumerable<AttendanceDto>> GetAttendancesAsync(int? enrollmentId, int? classId, int? studentId, DateTime? date);
        Task<AttendanceDto?> GetAttendanceByIdAsync(int id);
        Task<FeatureResult<AttendanceDto>> CreateAttendanceAsync(CreateAttendanceDto dto);
        Task<FeatureResult<bool>> UpdateAttendanceAsync(int id, UpdateAttendanceDto dto);
        Task<FeatureResult<bool>> DeleteAttendanceAsync(int id);

        // T24: thống kê điểm danh
        Task<FeatureResult<List<AttendanceStatDto>>> GetStatsByStudentAsync(int studentId);
        Task<FeatureResult<List<AttendanceStatDto>>> GetStatsByClassAsync(int classId);
    }
}

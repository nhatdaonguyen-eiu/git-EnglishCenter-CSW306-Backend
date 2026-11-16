using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs
{
    public class CreateAttendanceDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "EnrollmentId không hợp lệ")]
        public int EnrollmentId { get; set; }

        [Required(ErrorMessage = "Ngày học là bắt buộc")]
        public DateTime? SessionDate { get; set; }

        // Present / Absent / Excused (xem Constants/FeatureConstants.cs)
        [Required(ErrorMessage = "Trạng thái điểm danh là bắt buộc")]
        public string? Status { get; set; }
    }
}

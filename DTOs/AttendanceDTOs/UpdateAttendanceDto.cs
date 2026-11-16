using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs
{
    public class UpdateAttendanceDto
    {
        [Required(ErrorMessage = "Ngày học là bắt buộc")]
        public DateTime? SessionDate { get; set; }

        [Required(ErrorMessage = "Trạng thái điểm danh là bắt buộc")]
        public string? Status { get; set; }
    }
}

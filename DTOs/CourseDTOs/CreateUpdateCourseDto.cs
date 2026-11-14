using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.CourseDTOs
{
    public class CreateUpdateCourseDto
    {
        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Học phí không được âm")]
        public decimal Fee { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số buổi học phải lớn hơn 0")]
        public int TotalSessions { get; set; }
    }
}
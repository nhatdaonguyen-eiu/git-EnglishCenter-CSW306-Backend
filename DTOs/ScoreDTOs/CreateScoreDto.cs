using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs
{
    public class CreateScoreDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "EnrollmentId không hợp lệ")]
        public int EnrollmentId { get; set; }

        // Midterm / Final (xem Constants/FeatureConstants.cs)
        [Required(ErrorMessage = "Loại điểm là bắt buộc")]
        public string? Type { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm phải nằm trong khoảng 0 đến 10")]
        public decimal Value { get; set; }

        [StringLength(300)]
        public string? Comment { get; set; }
    }
}

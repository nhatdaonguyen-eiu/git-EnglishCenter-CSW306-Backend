using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs
{
    // Khi sửa điểm chỉ cho đổi Value và Comment (không đổi học viên / loại điểm)
    public class UpdateScoreDto
    {
        [Range(0, 10, ErrorMessage = "Điểm phải nằm trong khoảng 0 đến 10")]
        public decimal Value { get; set; }

        [StringLength(300)]
        public string? Comment { get; set; }
    }
}

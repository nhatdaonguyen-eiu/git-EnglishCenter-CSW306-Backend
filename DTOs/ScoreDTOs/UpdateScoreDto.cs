using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs
{
    // Khi sửa điểm chỉ cho đổi Value và Comment (không đổi học viên / loại điểm)
    public class UpdateScoreDto
    {
        // Phải dùng Range kiểu decimal. Range(0, 10) là bản số nguyên: nó làm tròn điểm
        // thành số nguyên trước khi so sánh, nên 10.4 hay -0.4 vẫn bị coi là hợp lệ.
        [Range(typeof(decimal), "0", "10",
            ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
            ErrorMessage = "Điểm phải nằm trong khoảng 0 đến 10")]
        public decimal Value { get; set; }

        [StringLength(300)]
        public string? Comment { get; set; }
    }
}

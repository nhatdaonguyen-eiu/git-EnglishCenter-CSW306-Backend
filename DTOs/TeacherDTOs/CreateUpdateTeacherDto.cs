using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.TeacherDTOs
{
    public class CreateUpdateTeacherDto
    {
        [Required(ErrorMessage = "Họ tên giáo viên là bắt buộc")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string? Email { get; set; }
    }
}
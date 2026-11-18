using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs
{
    /// <summary>Dữ liệu học viên trả ra ngoài.</summary>
    public class StudentDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Level { get; set; }
        public string? AvatarUrl { get; set; }
    }

    /// <summary>Dữ liệu nhận vào khi tạo học viên.</summary>
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime? DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? Level { get; set; }
    }

    /// <summary>Dữ liệu nhận vào khi cập nhật học viên (cùng quy tắc với tạo mới).</summary>
    public class UpdateStudentDto : CreateStudentDto
    {
    }
}

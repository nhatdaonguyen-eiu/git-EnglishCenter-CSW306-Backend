using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs
{
    /// <summary>Dữ liệu lớp học trả ra ngoài.</summary>
    public class ClassDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? Schedule { get; set; }
        public int MaxStudents { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>Dữ liệu nhận vào khi tạo lớp.</summary>
    public class CreateClassDto
    {
        [Required(ErrorMessage = "Tên lớp là bắt buộc")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "CourseId là bắt buộc")]
        public int? CourseId { get; set; }

        [Required(ErrorMessage = "TeacherId là bắt buộc")]
        public int? TeacherId { get; set; }

        [Required(ErrorMessage = "Lịch học là bắt buộc")]
        [StringLength(100)]
        public string? Schedule { get; set; }

        [Range(1, 500, ErrorMessage = "Sĩ số tối đa phải từ 1 đến 500")]
        public int MaxStudents { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime? EndDate { get; set; }
    }

    /// <summary>Dữ liệu nhận vào khi cập nhật lớp (cùng quy tắc với tạo mới).</summary>
    public class UpdateClassDto : CreateClassDto
    {
    }
}

using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int? EnrollmentId { get; set; }
        [Required]
        public DateTime SessionDate { get; set; }

        [Required]
        [StringLength(20)]
        public string? Status { get; set; }

        public Enrollment? Enrollment { get; set; }

    }
}

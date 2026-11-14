using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public int ClassId  { get; set; }
        public Class? Class { get; set; }

        [Required]
        public DateTime EnrolledAt { get; set; }

        [Required]
        [StringLength(20)]
        public string? Status { get; set; }

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Score> Scores { get; set; } = new List<Score>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        public int? CourseId {  get; set; }
        public Course? Course { get; set; }

        [Required]
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        [Required]
        [StringLength(100)]
        public string? Schedule {  get; set; }

        [Required]
        public int MaxStudents { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal? Fee { get; set; }

        [Required]
        public int? TotalSessions { get; set; }

        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}

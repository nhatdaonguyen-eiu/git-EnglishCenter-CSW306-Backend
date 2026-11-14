using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Score
    {
        [Key]
        public int Id   { get; set; }

        [Required]
        public int? EnrollmentId { get; set; }
        public Enrollment? Enrollment { get; set; }

        [Required]
        [StringLength(20)]
        public string? Type { get; set; }

        [Required]
        public decimal? Value {  get; set; }

        [StringLength(300)]
        public string? Comment { get; set; }
    }
}

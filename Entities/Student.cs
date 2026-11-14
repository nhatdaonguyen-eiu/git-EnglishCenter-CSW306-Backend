using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? Phone {  get; set; }

        [Required]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? Level { get; set; }

        [StringLength(300)]
        public string? AvatarUrl { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}

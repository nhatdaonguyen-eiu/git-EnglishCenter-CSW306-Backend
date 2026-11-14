using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(20)]
        public string? Phone {  get; set; }

        [Required]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        public AppUser? User { get; set; }

        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}

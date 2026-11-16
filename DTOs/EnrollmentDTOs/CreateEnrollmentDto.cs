using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs
{
    public class CreateEnrollmentDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "StudentId không hợp lệ")]
        public int StudentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ClassId không hợp lệ")]
        public int ClassId { get; set; }
    }
}

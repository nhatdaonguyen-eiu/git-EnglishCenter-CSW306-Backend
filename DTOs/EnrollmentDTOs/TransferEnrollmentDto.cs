using System.ComponentModel.DataAnnotations;

namespace EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs
{
    public class TransferEnrollmentDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "NewClassId không hợp lệ")]
        public int NewClassId { get; set; }
    }
}

namespace EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public DateTime EnrolledAt { get; set; }
        public string? Status { get; set; }
    }
}

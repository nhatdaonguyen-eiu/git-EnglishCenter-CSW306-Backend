namespace EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public DateTime SessionDate { get; set; }
        public string? Status { get; set; }
    }
}

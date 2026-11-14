namespace EnglishCenterManagement_CSW306_Project.DTOs.CourseDTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Fee { get; set; }
        public int TotalSessions { get; set; }
    }
}
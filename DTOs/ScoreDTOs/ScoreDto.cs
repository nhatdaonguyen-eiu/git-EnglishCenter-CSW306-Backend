namespace EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs
{
    public class ScoreDto
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? Type { get; set; }
        public decimal Value { get; set; }
        public string? Comment { get; set; }
    }
}

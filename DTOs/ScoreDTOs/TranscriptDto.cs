namespace EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs
{
    // Bảng điểm của một học viên: mỗi lớp (Enrollment) kèm danh sách điểm
    public class TranscriptDto
    {
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public List<TranscriptClassDto> Classes { get; set; } = new();
    }

    public class TranscriptClassDto
    {
        public int EnrollmentId { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? EnrollmentStatus { get; set; }
        public List<TranscriptScoreDto> Scores { get; set; } = new();
    }

    public class TranscriptScoreDto
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public decimal Value { get; set; }
        public string? Comment { get; set; }
    }
}

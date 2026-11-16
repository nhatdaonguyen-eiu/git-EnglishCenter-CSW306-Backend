namespace EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs
{
    // Thống kê điểm danh của MỘT học viên trong MỘT lớp (1 dòng = 1 Enrollment)
    public class AttendanceStatDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? EnrollmentStatus { get; set; }

        public int TotalSessions { get; set; }   // số buổi đã được điểm danh
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }

        // % có mặt = PresentCount / TotalSessions * 100 (có phép KHÔNG tính là có mặt)
        public double AttendanceRate { get; set; }
    }
}

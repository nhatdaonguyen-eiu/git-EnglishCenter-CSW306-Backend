namespace EnglishCenterManagement_CSW306_Project.Constants
{
    // Trạng thái điểm danh. Muốn đổi tên / thêm trạng thái thì chỉ sửa ở file này.
    public static class AttendanceStatus
    {
        public const string Present = "Present";        // Có mặt
        public const string Absent = "Absent";          // Vắng
        public const string Excused = "Excused";        // Có phép

        public static readonly string[] All = { Present, Absent, Excused };

        // Nhận "present", "PRESENT"... và trả về đúng giá trị chuẩn; không hợp lệ thì trả null
        public static string? Normalize(string? value) =>
            All.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

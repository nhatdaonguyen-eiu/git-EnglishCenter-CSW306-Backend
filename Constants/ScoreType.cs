namespace EnglishCenterManagement_CSW306_Project.Constants
{
    // Loại điểm. Muốn thêm loại điểm mới (vd "Quiz") thì chỉ sửa ở file này.
    public static class ScoreType
    {
        public const string Midterm = "Midterm";        // Giữa kỳ
        public const string Final = "Final";            // Cuối kỳ

        public static readonly string[] All = { Midterm, Final };

        // Khoảng điểm hợp lệ (khớp ràng buộc CK_Score_Value trong DB)
        public const decimal MinValue = 0;
        public const decimal MaxValue = 10;

        public static bool IsValidValue(decimal value) => value >= MinValue && value <= MaxValue;

        public static string? Normalize(string? value) =>
            All.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

namespace EnglishCenterManagement_CSW306_Project.Constants
{
    // Loại điểm. Muốn thêm loại điểm mới (vd "Quiz") thì chỉ sửa ở file này.
    public static class ScoreType
    {
        public const string Midterm = "Midterm";        // Giữa kỳ
        public const string Final = "Final";            // Cuối kỳ

        public static readonly string[] All = { Midterm, Final };

        public static string? Normalize(string? value) =>
            All.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

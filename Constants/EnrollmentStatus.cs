namespace EnglishCenterManagement_CSW306_Project.Constants
{
    // Trạng thái ghi danh. Muốn đổi tên / thêm trạng thái thì chỉ sửa ở file này.
    // Giá trị "Active" khớp với dữ liệu seed trong AppDbContext.
    public static class EnrollmentStatus
    {
        public const string Active = "Active";          // Đang học
        public const string Cancelled = "Cancelled";    // Đã hủy
        public const string Completed = "Completed";    // Hoàn thành
    }
}

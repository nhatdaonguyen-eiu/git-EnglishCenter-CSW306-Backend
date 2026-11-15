namespace EnglishCenterManagement_CSW306_Project.Common
{
    /// <summary>Không tìm thấy dữ liệu -> 404</summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    /// <summary>Dữ liệu gửi lên không hợp lệ -> 400</summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    /// <summary>Trùng dữ liệu / xung đột -> 409</summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}

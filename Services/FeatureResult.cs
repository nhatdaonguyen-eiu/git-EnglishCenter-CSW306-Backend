namespace EnglishCenterManagement_CSW306_Project.Services
{
    public enum ResultError
    {
        None,
        NotFound,   // -> 404
        Conflict,   // -> 409 (trùng, đầy lớp...)
        Invalid     // -> 400 (dữ liệu / trạng thái không hợp lệ)
    }

    // Kết quả trả về của Service (module Enrollment / Attendance / Score).
    // Service chỉ nói "thành công hay lỗi gì", Controller mới quyết định mã HTTP.
    // Sau này nếu nhóm thống nhất cách trả lỗi khác (T16) thì chỉ cần sửa FeatureControllerBase.
    public class FeatureResult<T>
    {
        public T? Data { get; private set; }
        public ResultError Error { get; private set; }
        public string? Message { get; private set; }
        public bool Success => Error == ResultError.None;

        public static FeatureResult<T> Ok(T data) => new() { Data = data };
        public static FeatureResult<T> NotFound(string message) => Fail(ResultError.NotFound, message);
        public static FeatureResult<T> Conflict(string message) => Fail(ResultError.Conflict, message);
        public static FeatureResult<T> Invalid(string message) => Fail(ResultError.Invalid, message);

        public static FeatureResult<T> Fail(ResultError error, string? message) =>
            new() { Error = error, Message = message };

        // Chuyển một kết quả lỗi sang kiểu dữ liệu khác (giữ nguyên loại lỗi + thông báo)
        public FeatureResult<TOther> CastError<TOther>() =>
            FeatureResult<TOther>.Fail(Error, Message);
    }
}

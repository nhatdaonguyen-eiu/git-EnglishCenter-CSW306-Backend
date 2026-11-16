using EnglishCenterManagement_CSW306_Project.Services;

// Đặt trong namespace này để Program.cs không cần thêm dòng using nào
namespace Microsoft.Extensions.DependencyInjection
{
    // Đăng ký DI cho các Service của TV4 (Enrollment, Attendance, Score).
    // Program.cs chỉ cần đúng 1 dòng: builder.Services.AddEnrollmentAttendanceScoreServices();
    // Muốn tắt cả module thì xóa dòng đó đi.
    public static class EnrollmentModuleExtensions
    {
        public static IServiceCollection AddEnrollmentAttendanceScoreServices(this IServiceCollection services)
        {
            services.AddScoped<IEnrollmentService, EnrollmentService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IScoreService, ScoreService>();

            return services;
        }
    }
}

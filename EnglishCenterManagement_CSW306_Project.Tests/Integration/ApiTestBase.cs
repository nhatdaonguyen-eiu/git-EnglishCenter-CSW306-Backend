using EnglishCenterManagement_CSW306_Project.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnglishCenterManagement_CSW306_Project.Tests.Integration
{
    // T32: Integration test. Chạy cả ứng dụng thật trong bộ nhớ (routing, model validation,
    // controller, service) và gọi API bằng HttpClient, chỉ thay SQL Server bằng database InMemory.
    // Dùng AppDbContext làm "điểm vào" vì class Program (top-level statements) là internal.
    public class ApiFactory : WebApplicationFactory<AppDbContext>
    {
        // Mỗi factory 1 database riêng để các test không ảnh hưởng nhau
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    // Lớp cha của các test API. xUnit tạo 1 đối tượng mới cho mỗi test,
    // nên mỗi test có 1 ứng dụng + 1 database mới chứa đúng dữ liệu seed trong AppDbContext:
    //   Học viên 1 (Lê Hoàng An), 2 (Phạm Gia Bảo), 3 (Vũ Ngọc Châu)
    //   Lớp 1 (IELTS-F01, tối đa 15), lớp 2 (TOEIC-T01, tối đa 20)
    //   Ghi danh 1 (học viên 1 - lớp 1), ghi danh 2 (học viên 2 - lớp 1), đều Active
    public abstract class ApiTestBase : IDisposable
    {
        private readonly ApiFactory _factory = new();
        protected HttpClient Client { get; }

        protected ApiTestBase()
        {
            Client = _factory.CreateClient();
            WithDb(db => db.Database.EnsureCreated()); // nạp dữ liệu seed (HasData)
        }

        // Đọc / sửa thẳng database để chuẩn bị dữ liệu hoặc kiểm tra kết quả
        protected void WithDb(Action<AppDbContext> action)
        {
            using var scope = _factory.Services.CreateScope();
            action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        public void Dispose()
        {
            Client.Dispose();
            _factory.Dispose();
        }
    }
}

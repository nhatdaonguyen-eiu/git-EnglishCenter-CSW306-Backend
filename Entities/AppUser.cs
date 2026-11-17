using Microsoft.AspNetCore.Identity;

namespace EnglishCenterManagement_CSW306_Project.Entities
{
    public class AppUser : IdentityUser
    {

        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } //Nhật bổ sung AvatarUrl để lưu trữ đường dẫn ảnh đại diện của người dùng
    }
}



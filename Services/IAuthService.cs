using EnglishCenterManagement_CSW306_Project.DTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
using EnglishCenterManagement_CSW306_Project.DTOs;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public interface IProfileService
    {
        Task<object> GetProfileAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    }
}
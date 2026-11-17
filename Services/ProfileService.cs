using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.AspNetCore.Identity;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        public ProfileService(UserManager<AppUser> userManager) => _userManager = userManager;

        public async Task<object> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null!;
            return new { user.Id, user.Email, user.FullName, user.PhoneNumber, user.AvatarUrl };
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
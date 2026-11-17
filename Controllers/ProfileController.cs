using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        
        public ProfileController(IProfileService profileService) 
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var profile = await _profileService.GetProfileAsync(userId);
            return profile != null ? Ok(profile) : NotFound();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
        {
            
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _profileService.UpdateProfileAsync(userId, dto);
            return result ? Ok(new { Message = "Cập nhật thành công" }) : BadRequest("Cập nhật thất bại");
        }

        // API Admin
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")] 
        public IActionResult AdminDashboard()
        {
            return Ok(new { Message = "Chào Admin! Bạn đã có quyền truy cập hệ thống." });
        }
    }
}
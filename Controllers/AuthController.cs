using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishCenterManagement_CSW306_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService) 
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Kiểm tra Validation đầu vào
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Kiểm tra Validation 
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            return result.IsSuccess ? Ok(result) : Unauthorized(result.Message);
        }
    }
}
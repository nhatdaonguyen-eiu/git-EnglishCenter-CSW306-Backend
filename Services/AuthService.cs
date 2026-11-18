using EnglishCenterManagement_CSW306_Project.DTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new AppUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

            // BẢO MẬT: Mặc định người đăng ký luôn nhận Role là Student
            // Tuyệt đối không cho phép client tự gửi "Admin" để leo thang đặc quyền
            var assignedRole = "Student";

            // Chỉ cho phép chọn Teacher nếu được chỉ định, còn lại mặc định là Student
            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
            {
                assignedRole = "Teacher";
            }

            if (!await _roleManager.RoleExistsAsync(assignedRole))
                await _roleManager.CreateAsync(new IdentityRole(assignedRole));
            
            await _userManager.AddToRoleAsync(user, assignedRole);

            return new AuthResponseDto { IsSuccess = true, Message = "Đăng ký thành công!" };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return new AuthResponseDto { IsSuccess = false, Message = "Email hoặc mật khẩu không đúng." };

            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            foreach (var role in userRoles) authClaims.Add(new Claim(ClaimTypes.Role, role));

            var token = GetToken(authClaims);
            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Message = "Đăng nhập thành công"
            };
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            return new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"]!)),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
        }
    }
}
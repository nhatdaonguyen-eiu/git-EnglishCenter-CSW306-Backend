using EnglishCenterManagement_CSW306_Project.Data;
using Microsoft.EntityFrameworkCore;
using EnglishCenterManagement_CSW306_Project.Repositories;
using EnglishCenterManagement_CSW306_Project.Entities;
using EnglishCenterManagement_CSW306_Project.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwt["Key"]!))
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddRateLimiter(o => {
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(c => 
        RateLimitPartition.GetFixedWindowLimiter(c.User.Identity?.Name ?? c.Request.Headers.Host.ToString(), 
        _ => new FixedWindowRateLimiterOptions { AutoReplenishment = true, PermitLimit = 100, QueueLimit = 0, Window = TimeSpan.FromMinutes(1) }));
    o.RejectionStatusCode = 429;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { 
        Description = "Bearer [token]", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer" 
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new List<string>() } 
    });
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddEnrollmentAttendanceScoreServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "Teacher", "Student" })
        if (!await rm.RoleExistsAsync(role)) await rm.CreateAsync(new IdentityRole(role));
}

app.Run();
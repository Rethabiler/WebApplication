using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ── LOGIN ──
        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // ── INPUT WHITELISTING WITH REGEX ──
            // Username: only letters, numbers, underscores, 3-50 chars
            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]{3,50}$"))
                return BadRequest(new { message = "Invalid username format." });

            // Password: 8-100 chars
            if (!Regex.IsMatch(request.Password, @"^.{8,100}$"))
                return BadRequest(new { message = "Invalid password format." });

            // Find user
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            // ── SECURITY: same message for wrong user or wrong password ──
            // This prevents username enumeration attacks
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password." });

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role
            });
        }

        // ── CREATE USER (Admin only — no self-registration) ──
        // POST: api/auth/create-user
        [HttpPost("create-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            // Validate username
            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]{3,50}$"))
                return BadRequest(new { message = "Invalid username format." });

            // Validate password strength
            // Must have: 8+ chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
            if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
                return BadRequest(new { message = "Password must be 8+ chars with uppercase, lowercase, number and special character." });

            // Check username not already taken
            if (await _context.AppUsers.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "Username already exists." });

            // ── HASH PASSWORD with BCrypt (automatically salts) ──
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new AppUser
            {
                FullName = request.FullName,
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role ?? "Employee"
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{user.Username}' created successfully." });
        }

        // ── SEED ADMIN USER (run once to create first admin) ──
        // POST: api/auth/seed
        [HttpPost("seed")]
        public async Task<IActionResult> SeedAdmin()
        {
            if (await _context.AppUsers.AnyAsync())
                return BadRequest(new { message = "Users already exist." });

            var admin = new AppUser
            {
                FullName = "System Administrator",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                Role = "Admin"
            };

            _context.AppUsers.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin user created. Username: admin, Password: Admin@12345" });
        }

        // ── JWT TOKEN GENERATOR ──
        private string GenerateJwtToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // ── REQUEST MODELS ──
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;
using AuthService.Helpers;
using AuthService.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Database context, JWT service, and configuration
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;
        private readonly IConfiguration _configuration;

        // Constructor to set values
        public AuthController(ApplicationDbContext context, JwtService jwt, IConfiguration configuration)
        {
            _context = context;
            _jwt = jwt;
            _configuration = configuration;
        }

        // Register new user
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest("User already exists.");

            // Get key from appsettings.json
            var key = _configuration["Jwt:Key"];

            // Encrypt password
            var (iv, cipher) = CryptoHelper.Encrypt(user.PasswordHash, key);
            user.PasswordIV = iv;
            user.PasswordHash = cipher;

            // Set default role if not given
            if (string.IsNullOrEmpty(user.Role))
                user.Role = "User";

            
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully and saved to DB!");
        }

        // Login user
        [HttpPost("login")]
        public IActionResult Login([FromBody] User request)
        {
            // Get encryption key
            var key = _configuration["Jwt:Key"];

            
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            
            if (user == null)
                return Unauthorized("Invalid username.");

            // Decrypt saved password
            var decryptedPassword = CryptoHelper.Decrypt(user.PasswordIV, user.PasswordHash, key);

            
            if (decryptedPassword != request.PasswordHash)
                return Unauthorized("Invalid password.");

            // Create JWT token
            var token = _jwt.GenerateToken(user);

            return Ok(new { token });
        }

        // Validate token (used by other services)
        [HttpPost("validate")]
        public IActionResult ValidateToken([FromBody] TokenRequest model)
        {
            // Check if token is given
            if (string.IsNullOrWhiteSpace(model.Token))
                return BadRequest(new { Valid = false, Message = "Token missing." });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                // Check token is valid or not
                var principal = tokenHandler.ValidateToken(model.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                // Get username and role from token
                var username = principal.Identity?.Name;
                var role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                return Ok(new { Valid = true, Username = username, Role = role });
            }
            catch (Exception ex)
            {
                // If token not valid
                return Unauthorized(new { Valid = false, Message = ex.Message });
            }
        }

        // Class for token validation input
        public class TokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }

         [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.Select(u => new {
                u.Id,
                u.Username,
                u.Email,
                u.Role
            }).ToList();

            return Ok(users);
        }
    }
}

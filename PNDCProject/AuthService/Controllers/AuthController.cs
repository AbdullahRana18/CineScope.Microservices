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
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwt;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, JwtService jwt, IConfiguration configuration)
        {
            _context = context;
            _jwt = jwt;
            _configuration = configuration;
        }

        // ✅ REGISTER
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
                return BadRequest("User already exists.");

            var key = _configuration["Jwt:Key"];
            var (iv, cipher) = CryptoHelper.Encrypt(user.PasswordHash, key);
            user.PasswordIV = iv;
            user.PasswordHash = cipher;

            if (string.IsNullOrEmpty(user.Role))
                user.Role = "User";

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully and saved to DB!");
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public IActionResult Login([FromBody] User request)
        {
            var key = _configuration["Jwt:Key"];
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
                return Unauthorized("Invalid username.");

            var decryptedPassword = CryptoHelper.Decrypt(user.PasswordIV, user.PasswordHash, key);

            if (decryptedPassword != request.PasswordHash)
                return Unauthorized("Invalid password.");

            var token = _jwt.GenerateToken(user);
            return Ok(new { token });
        }

        // ✅ VALIDATE TOKEN for MovieService
        [HttpPost("validate")]
        public IActionResult ValidateToken([FromBody] TokenRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Token))
                return BadRequest(new { Valid = false, Message = "Token missing." });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
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

                var username = principal.Identity?.Name;
                var role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                return Ok(new { Valid = true, Username = username, Role = role });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Valid = false, Message = ex.Message });
            }
        }

        public class TokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}

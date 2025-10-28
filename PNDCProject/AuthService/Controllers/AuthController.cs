using Microsoft.AspNetCore.Mvc;
using AuthService.Models;
using AuthService.Services;
using AuthService.Helpers;
using AuthService.Data;

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

            // Encrypt password before saving
            var key = _configuration["Jwt:Key"];
            var (iv, cipher) = CryptoHelper.Encrypt(user.PasswordHash, key);
            user.PasswordIV = iv;
            user.PasswordHash = cipher;

            if (string.IsNullOrEmpty(user.Role))
                user.Role = "User";

            _context.Users.Add(user);
            _context.SaveChanges(); // ✅ actually saves data in DB

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
            return Ok(new { Token = token });
        }

        // ✅ PROTECTED ROUTE (ROLE = ADMIN)
        [HttpGet("protected")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult Protected()
        {
            return Ok("You are authorized as Admin!");
        }
    }
}

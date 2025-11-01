using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthService.Models;

namespace AuthService.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;  // To access settings from appsettings.json

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Create JWT token for a user
        public string GenerateToken(User user)
        {
            // Add user data in token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),  // Username
                new Claim("email", user.Email),                         // Email
                new Claim(ClaimTypes.Role, user.Role)                   // Role
            };

            // Create signing key from secret key in settings
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build token with all info
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            // Return token string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

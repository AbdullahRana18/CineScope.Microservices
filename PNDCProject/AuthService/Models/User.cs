namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }             // User ID
        public string Username { get; set; } = string.Empty;   // User name
        public string Email { get; set; } = string.Empty;      // User email
        public string PasswordHash { get; set; } = string.Empty; // Encrypted password
        public string? PasswordIV { get; set; }                 // Encryption IV
        public string Role { get; set; } = "User";              // User role
    }
}

namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }

        // User input fields
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // These are generated automatically during registration
        public string? PasswordIV { get; set; }

        public string Role { get; set; } = "User";
    }
}

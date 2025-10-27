namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordIV { get; set; } // AES IV for encryption
        public string PasswordHash { get; set; } // Encrypted password
        public string Role { get; set; }
    }
}

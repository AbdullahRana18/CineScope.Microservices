using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data
{
    // Database context class
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Users table
        public DbSet<User> Users { get; set; }
    }
}

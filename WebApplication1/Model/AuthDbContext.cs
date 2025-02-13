using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;  // <-- Your custom ApplicationUser namespace

namespace WebApplication1.Model
{
    // Make sure to change IdentityDbContext to IdentityDbContext<ApplicationUser>
    // so that EF Core knows about your custom ApplicationUser properties.
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

        // 1. Use DI to get both DbContextOptions and IConfiguration
        public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }
        public DbSet<PasswordHistory> PasswordHistories { get; set; }
        // 2. Optionally override OnConfiguring if you want a fallback
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // If the DbContextOptions aren’t configured yet, configure here
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = _configuration.GetConnectionString("AuthConnectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        // 3. Optionally override OnModelCreating to configure additional aspects
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // e.g. Configure custom constraints/indexes here, if needed
        }
    }
}

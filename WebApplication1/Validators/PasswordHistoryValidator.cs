using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;    // <-- whatever your namespace is for AuthDbContext
using WebApplication1.Models;   // <-- for ApplicationUser & PasswordHistory
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Validators
{
    public class PasswordHistoryValidator : IPasswordValidator<ApplicationUser>
    {
        private readonly AuthDbContext _dbContext;

        public PasswordHistoryValidator(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IdentityResult> ValidateAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user,
            string password)
        {
            // 1. Get last 2 password hashes from PasswordHistories
            var lastTwoHashes = await _dbContext.PasswordHistories
                .Where(ph => ph.UserId == user.Id)
                .OrderByDescending(ph => ph.CreatedDate)
                .Take(2)
                .Select(ph => ph.HashedPassword)
                .ToListAsync();

            // 2. Compare new password with old hashes
            foreach (var oldHash in lastTwoHashes)
            {
                var verificationResult = manager.PasswordHasher.VerifyHashedPassword(user, oldHash, password);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "You cannot reuse any of your last 2 passwords."
                    });
                }
            }

            // 3. If it passes these checks, password is valid
            return IdentityResult.Success;
        }
    }
}

using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Extensions
{
    public static class LockoutExtensions
    {
        public static async Task CheckAndRemoveLockoutAsync(this SignInManager<ApplicationUser> signInManager, ApplicationUser user)
        {
            if (await signInManager.UserManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await signInManager.UserManager.GetLockoutEndDateAsync(user);
                if (lockoutEnd.HasValue && lockoutEnd.Value <= DateTime.UtcNow)
                {
                    await signInManager.UserManager.SetLockoutEndDateAsync(user, null);
                }
            }
        }
    }
}

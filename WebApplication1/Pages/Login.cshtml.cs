using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // First, attempt to sign in using the provided credentials.
                var signInResult = await _signInManager.PasswordSignInAsync(
                    LModel.Email, LModel.Password, LModel.RememberMe, lockoutOnFailure: false);

                if (signInResult.Succeeded)
                {
                    // Retrieve the user object to check when they last changed their password.
                    var user = await _userManager.FindByEmailAsync(LModel.Email);

                    // Retrieve the expiration threshold (default to 90 days if not set in configuration)
                    int expirationDays = _configuration.GetValue<int>("PasswordExpirationDays", 90);
                    var passwordAge = DateTime.UtcNow - user.PasswordLastChanged;

                    if (passwordAge.TotalDays > expirationDays)
                    {
                        // Optional: Sign the user out if they were partially signed in.
                        await _signInManager.SignOutAsync();

                        // Add an error message or redirect to a password reset/change page.
                        ModelState.AddModelError("", "Your password has expired. Please reset your password.");
                        return RedirectToPage("ResetPassword"); // Or whichever page you use for password resets.
                    }

                    // If the password is still valid, create the authentication cookie.
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("Department", "HR")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal);
                    return RedirectToPage("ChangePassword");
                }
                ModelState.AddModelError("", "Username or Password incorrect");
            }
            return Page();
        }
    }
}

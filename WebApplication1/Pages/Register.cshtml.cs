using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.ViewModels;
using WebApplication1.Models;
using WebApplication1.Helpers;
using WebApplication1.Model; // <-- For AuthDbContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly AuthDbContext _dbContext; // <-- Inject our DbContext
        private readonly IConfiguration _configuration;
        private readonly GoogleCaptchaService _captchaService;


        [BindProperty]
        public Register RModel { get; set; }

        // Expose the Site Key to the view
        public string RecaptchaSiteKey { get; set; }

        // Add AuthDbContext to the constructor's parameters
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment env,
            AuthDbContext dbContext,
            IConfiguration configuration,
            GoogleCaptchaService captchaService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
            _dbContext = dbContext;
            _configuration = configuration;
            _captchaService = captchaService;// 3) Store it for later
        }

        public void OnGet()
        {
            RecaptchaSiteKey = _configuration["GoogleReCaptcha:SiteKey"];
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Get the token from the request
            var recaptchaToken = Request.Form["g-recaptcha-response"];

            // 2. Verify it
            bool isCaptchaValid = await _captchaService.VerifyToken(recaptchaToken);
            if (!isCaptchaValid)
            {
                ModelState.AddModelError(string.Empty, "Captcha validation failed. Please try again.");
                return Page();
            }

            // Validate photo
            if (RModel.Photo == null || RModel.Photo.Length == 0)
            {
                ModelState.AddModelError("Photo", "Please upload a JPG photo.");
                return Page();
            }

            var extension = Path.GetExtension(RModel.Photo.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg")
            {
                ModelState.AddModelError("Photo", "Only JPG files are allowed.");
                return Page();
            }

            // Save photo to /wwwroot/images/users/
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadPath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                await RModel.Photo.CopyToAsync(fileStream);
            }

            // Encrypt sensitive data
            var (encryptedCard, cardIV) = CryptoHelper.EncryptString(RModel.CreditCardNo);
            var (encryptedMobile, mobileIV) = CryptoHelper.EncryptString(RModel.MobileNo);
            var (encryptedBilling, billingIV) = CryptoHelper.EncryptString(RModel.BillingAddress);

            // Create a new ApplicationUser object
            var user = new ApplicationUser
            {
                UserName = RModel.Email,
                Email = RModel.Email,
                FirstName = RModel.FirstName,
                LastName = RModel.LastName,

                EncryptedCreditCard = encryptedCard,
                EncryptedCreditCardIV = cardIV,

                EncryptedMobileNo = encryptedMobile,
                MobileNoIV = mobileIV,

                EncryptedBillingAddress = encryptedBilling,
                BillingAddressIV = billingIV,

                ShippingAddress = RModel.ShippingAddress,
                PhotoPath = $"/images/users/{fileName}"
            };

            // Create the user with Identity
            var result = await _userManager.CreateAsync(user, RModel.Password);
            if (result.Succeeded)
            {
                // 1. Retrieve the newly created user's password hash
                var userPasswordHash = user.PasswordHash;  // <--- Use the property directly

                // 2. Store it in PasswordHistories
                var passwordHistory = new PasswordHistory
                {
                    UserId = user.Id,
                    HashedPassword = userPasswordHash,
                    CreatedDate = DateTime.UtcNow
                };
                _dbContext.PasswordHistories.Add(passwordHistory);
                await _dbContext.SaveChangesAsync();

                // 3. (Optional) Keep only the last 2 password histories for this user
                var histories = await _dbContext.PasswordHistories
                    .Where(ph => ph.UserId == user.Id)
                    .OrderByDescending(ph => ph.CreatedDate)
                    .ToListAsync();

                if (histories.Count > 2)
                {
                    var oldEntries = histories.Skip(2).ToList();
                    _dbContext.PasswordHistories.RemoveRange(oldEntries);
                    await _dbContext.SaveChangesAsync();
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("LandingPage");
            }

            // if creation failed, show the errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}

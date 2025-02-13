using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Pages
{
    [Authorize]
    public class LandingPageModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LandingPageModel(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ApplicationUser CurrentUser { get; set; }

        public string DecryptedCreditCard { get; set; }
        public string DecryptedMobileNo { get; set; }
        public string DecryptedBillingAddress { get; set; }

        [BindProperty]
        public LandingPageViewModel UpdateModel { get; set; }  // For user update form

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Login");
            }

            CurrentUser = await _userManager.GetUserAsync(User);

            if (CurrentUser == null)
            {
                return NotFound("User not found.");
            }

            // Decrypt any fields you want to display in plain text:
            if (!string.IsNullOrEmpty(CurrentUser.EncryptedCreditCard) &&
                !string.IsNullOrEmpty(CurrentUser.EncryptedCreditCardIV))
            {
                DecryptedCreditCard = CryptoHelper.DecryptString(
                    CurrentUser.EncryptedCreditCard,
                    CurrentUser.EncryptedCreditCardIV
                );
            }

            if (!string.IsNullOrEmpty(CurrentUser.EncryptedMobileNo) &&
                !string.IsNullOrEmpty(CurrentUser.MobileNoIV))
            {
                DecryptedMobileNo = CryptoHelper.DecryptString(
                    CurrentUser.EncryptedMobileNo,
                    CurrentUser.MobileNoIV
                );
            }

            if (!string.IsNullOrEmpty(CurrentUser.EncryptedBillingAddress) &&
                !string.IsNullOrEmpty(CurrentUser.BillingAddressIV))
            {
                DecryptedBillingAddress = CryptoHelper.DecryptString(
                    CurrentUser.EncryptedBillingAddress,
                    CurrentUser.BillingAddressIV
                );
            }

            // Pre-populate the UpdateModel for the user to see current data:
            UpdateModel = new LandingPageViewModel
            {
                MobileNo = DecryptedMobileNo,
                BillingAddress = DecryptedBillingAddress,
                ShippingAddress = CurrentUser.ShippingAddress,
                CreditCardNo = DecryptedCreditCard
            };

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateUser()
        {
            if (!ModelState.IsValid)
            {
                // Reload user fields so they are not null in the Razor:
                await LoadUserFieldsAsync();
                return Page();
            }

            // Get current user
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return NotFound("User not found.");
            }

            // Encrypt sensitive data
            var (encryptedCard, cardIV) = CryptoHelper.EncryptString(UpdateModel.CreditCardNo);
            var (encryptedMobile, mobileIV) = CryptoHelper.EncryptString(UpdateModel.MobileNo);
            var (encryptedBilling, billingIV) = CryptoHelper.EncryptString(UpdateModel.BillingAddress);

            // Update user fields
            CurrentUser.EncryptedCreditCard = encryptedCard;
            CurrentUser.EncryptedCreditCardIV = cardIV;
            CurrentUser.EncryptedMobileNo = encryptedMobile;
            CurrentUser.MobileNoIV = mobileIV;
            CurrentUser.EncryptedBillingAddress = encryptedBilling;
            CurrentUser.BillingAddressIV = billingIV;
            CurrentUser.ShippingAddress = UpdateModel.ShippingAddress;

            // Save changes
            var result = await _userManager.UpdateAsync(CurrentUser);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                // Reload fields
                await LoadUserFieldsAsync();
                return Page();
            }

            // Re-sign-in if you want updated claims, etc. (optional)
            await _signInManager.RefreshSignInAsync(CurrentUser);

            // Optionally show a success message or redirect
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToPage();
        }

        // Helper method to reload your decrypted fields so the Page can show them
        private async Task LoadUserFieldsAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(CurrentUser.EncryptedCreditCard) &&
                !string.IsNullOrEmpty(CurrentUser.EncryptedCreditCardIV))
            {
                DecryptedCreditCard = CryptoHelper.DecryptString(
                    CurrentUser.EncryptedCreditCard,
                    CurrentUser.EncryptedCreditCardIV
                );
            }

            if (!string.IsNullOrEmpty(CurrentUser.EncryptedMobileNo) &&
                !string.IsNullOrEmpty(CurrentUser.MobileNoIV))
            {
                DecryptedMobileNo = CryptoHelper.DecryptString(
                    CurrentUser.EncryptedMobileNo,
                    CurrentUser.MobileNoIV
                );
            }

            if (!string.IsNullOrEmpty(CurrentUser.EncryptedBillingAddress) &&
                !string.IsNullOrEmpty(CurrentUser.BillingAddressIV))
            {
                DecryptedBillingAddress = CryptoHelper.DecryptString(
                    CurrentUser.EncryptedBillingAddress,
                    CurrentUser.BillingAddressIV
                );
            }
        }
    }
}
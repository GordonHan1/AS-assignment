using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Reset Method")]
        [Required]
        public string ResetMethod { get; set; } // "Email" or "SMS"
    }
}

using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class LandingPageViewModel
    {
        [Required]
        [Display(Name = "Mobile No")]
        public string MobileNo { get; set; }

        [Required]
        [Display(Name = "Billing Address")]
        public string BillingAddress { get; set; }

        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required]
        [DataType(DataType.CreditCard)]
        [Display(Name = "Credit Card No.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit card number must be exactly 16 digits")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Credit card number must be exactly 16 digits")]
        public string CreditCardNo { get; set; }
    }
}

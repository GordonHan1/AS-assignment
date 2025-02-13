using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        // Encrypted Credit Card
        public string EncryptedCreditCard { get; set; }
        public string EncryptedCreditCardIV { get; set; }

        // Encrypted Mobile Number
        public string EncryptedMobileNo { get; set; }  // store the encrypted mobile no.
        public string MobileNoIV { get; set; }         // store the IV used for encryption

        // Encrypted Billing Address
        public string EncryptedBillingAddress { get; set; }
        public string BillingAddressIV { get; set; }

        // Shipping Address (if you want to keep it as plain text, leave it as is)
        public string ShippingAddress { get; set; }

        // Photo path
        public string PhotoPath { get; set; }
    }

}

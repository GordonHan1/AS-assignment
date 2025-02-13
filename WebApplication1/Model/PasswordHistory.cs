using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class PasswordHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string HashedPassword { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace UserRegistration.DTO
{
    public class LoginDTO
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace UserRegistration.DTO
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Username { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must be alphanumeric and at least 8 characters long.")]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Phone]
        public string Telephone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

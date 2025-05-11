using System.ComponentModel.DataAnnotations;

namespace Entities.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

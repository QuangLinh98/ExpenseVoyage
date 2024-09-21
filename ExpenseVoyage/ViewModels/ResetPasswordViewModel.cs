using System.ComponentModel.DataAnnotations;

namespace ExpenseVoyage.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }
        public string Email { get; set; }
        [Required]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}

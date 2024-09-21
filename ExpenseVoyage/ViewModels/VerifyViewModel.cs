using System.ComponentModel.DataAnnotations;

namespace ExpenseVoyage.ViewModels
{
    public class VerifyViewModel
    {
        [Required(ErrorMessage = "OTP is required.")]
        public string Otp { get; set; }
    }
}

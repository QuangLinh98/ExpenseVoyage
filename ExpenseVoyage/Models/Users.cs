using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Models
{
    public class Users
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserId { get; set; }

		[Required]
		[StringLength(30, ErrorMessage = "User Name cannot be longer than 30 characters")]
		public string Username { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		[StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[StringLength(100, ErrorMessage = "Password cannot be longer than 100 characters")]
		public string Password { get; set; }

		[StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
		public string Address { get; set; }

		[Required(ErrorMessage = "Phone is required")]
		[Phone(ErrorMessage = "Invalid Phone Number")]
		[RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number must be 10-11 digits.")]
		public string Phone { get; set; }

		public bool IsNewUser { get; set; }                 // Chức năng: Để set điều kiện user mới đăng ký phải xác thực email mới cho phép login và user cần resetPassword có thể login sau khi reset mà không cần xác thực

		public string Role { get; set; } = "User";
		public string? ImagePath { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

		public bool EmailConfirmed { get; set; }
		public string? EmailConfirmationToken { get; set; }
		public string? ResetPasswordToken { get; set; }
		public DateTime? ResetPasswordTokenExpiration { get; set; }

		// Điều hướng đến các chuyến đi
		public virtual ICollection<Trips>? Trips { get; set; }

		// Điều hướng đến các thông báo
		public virtual ICollection<Notifications>? Notifications { get; set; }

	}
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.ViewModels
{
	public class EditUserViewModel
	{
		public int UserId { get; set; }

		[Required]
		[StringLength(30, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 30 characters long.")]
		public string Username { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		[StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
		public string Email { get; set; }

		[StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters")]
		public string Address { get; set; }

		[Required(ErrorMessage = "Phone is required")]
		[Phone(ErrorMessage = "Invalid Phone Number")]
		[RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number must be 10-11 digits.")]
		public string Phone { get; set; }

		public string? ImagePath { get; set; }
		[NotMapped]
		public IFormFile? ImageFile { get; set; }
	}
}

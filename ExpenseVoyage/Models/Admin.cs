using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
	public class Admin
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(30, ErrorMessage = "User Name cannot be longer than 30 characters")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		[StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
		public string Email { get; set; }

		[Required(ErrorMessage = "Phone is required")]
		[Phone(ErrorMessage = "Invalid Phone Number")]
		[RegularExpression(@"^\d{10,11}$", ErrorMessage = "Phone number must be 10-11 digits.")]
		public string Phone { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[StringLength(100, ErrorMessage = "Password cannot be longer than 100 characters")]
		public string Password { get; set; }

		public string ImagePath { get; set; }

		[NotMapped]
		public IFormFile? ImageFile { get; set; }
	}
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
	public class Trips
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TripId { get; set; }

		[ForeignKey("User")]
		public int UserId { get; set; }

		public virtual Users? User { get; set; }

		[Required]
		public string Departure { get; set; }

		[Required]
		[StringLength(255)]
		public string Destination { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		[Required]
		[Column(TypeName = "decimal(10,2)")]
		public decimal TotalBudget { get; set; }

		public virtual ICollection<Itineraries>? Itineraries { get; set; }
	}
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExpenseVoyage.Models
{
    public class Expenses
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ExpenseId { get; set; }

		[ForeignKey("Itineraries")]
		public int ItineraryId { get; set; }

		public virtual Itineraries? Itineraries { get; set; }

		[Required]
		[Column(TypeName = "decimal(10,2)")]
		public decimal Amount { get; set; }

		[Required]
		[StringLength(50)]
		public string Category { get; set; }

		[Required]
		public DateTime Date { get; set; }

		[ValidateNever]
		public string? Note { get; set; }

		[ValidateNever]
		public string? Description { get; set; }


	}
}

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
    public class DestinationImage
	{
        public int Id { get; set; }

        public string? ImageUrl { get; set; }
        [ValidateNever]
        public int? DestinationId { get; set; }
        [ForeignKey("DestinationId")]
        public Destinations? Destinations { get; set; }

    }
}

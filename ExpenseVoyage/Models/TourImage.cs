using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
    public class TourImage
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public int? ExpenseTourId { get; set; }
        [ValidateNever]
        [ForeignKey("ExpenseTourId")]
        public ExpenseTour? ExpenseTours { get; set; }
    }

}

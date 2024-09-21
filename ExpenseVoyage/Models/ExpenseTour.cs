using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
    public class ExpenseTour
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpenseTourId { get; set; }
        public int TourID { get; set; }

        [ForeignKey("TourID")]

        [ValidateNever]
        public Tour? Tours { get; set; }
        [Required]
        public string ExpenseTourName { get; set; }
        [Required]
        public int Day { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Category { get; set; }
        [Required]
        public string Derparture { get; set; }
        [Required]
        public string Destination { get; set; }
        [Required]
        public int Cost { get; set; }
        [ValidateNever]
        public List<TourImage>? TourImages { get; set; }

    }
}

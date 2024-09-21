using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExpenseVoyage.Models
{
    public class Itineraries
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItineraryId { get; set; }

        [ForeignKey("Trip")]
        public int TripId { get; set; }
        public virtual Trips? Trip { get; set; }

        [Required]
		public int Day { get; set; }

		[Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required]
        public string Departure { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        public virtual ICollection<Expenses>? Expenses { get; set; }
    }
}

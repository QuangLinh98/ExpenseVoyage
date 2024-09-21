using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
    public class Destinations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        
        public int DestinationId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

		[ValidateNever]
		public List<DestinationImage>? DestinationImages { get; set; }

		[ValidateNever]
        public virtual ICollection<Photos>? Photos { get; set; }

    }
}

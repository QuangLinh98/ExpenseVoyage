using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
    public class Photos
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int PhotoId { get; set; }

		[ForeignKey("DestinationId")]
		public int DestinationId { get; set; }

		[ValidateNever]
		public virtual Destinations? Destination { get; set; }
		public string DestinationName { get; set; }

		[ValidateNever]
		public List<PhotosImage>? PhotosImages { get; set; }

		public string Caption { get; set; }
	}
}

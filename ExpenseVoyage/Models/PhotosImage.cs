using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVoyage.Models
{
    public class PhotosImage
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; }

        [ValidateNever]
        public int? PhotoId { get; set; }
        [ForeignKey("PhotoId")]
        public Photos? Photos { get; set; }




    }
}

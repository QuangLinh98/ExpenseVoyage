using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ExpenseVoyage.Models
{
    public class Currencies
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

		public string CurrencyCode { get; set; }

		[Required]
		[Column(TypeName = "decimal(10, 2)")]
		public decimal ExchangeRate { get; set; }

		[Required]
		[StringLength(50)]
		public string CurrencyName { get; set; } // Tên tiền tệ (VD: United States Dollar)
	}
}

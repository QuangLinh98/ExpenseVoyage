using ExpenseVoyage.Models;

namespace ExpenseVoyage.DTOs
{
	public class ItineraryExpenseViewModel
	{
		public int TripId { get; set; }
		public int ItineraryId { get; set; }
        public Trips? Trip { get; set; }
		    public int? Day { get; set; } // Add this property

        public List<Itineraries>? Itineraries { get; set; }
		public List<Expenses>? Expenses { get; set; }
	}
}

using ExpenseVoyage.Models;
using ExpenseVoyage.ViewModels;

namespace ExpenseVoyage.Viewmodels
{
    public class HomeViewModel
    {
        public IEnumerable<Photos>? Photos { get; set; }

        public IEnumerable<PhotosImage>? PhotosImages { get; set; }
        public IEnumerable<DestinationImage>? DestinationImages { get; set; }
        public IEnumerable<Tour>? Tours {  get; set; }
        public ExpenseTour? ExpenseTours { get; set; }
		public IEnumerable<ExpenseTour>? ExpenseTourList { get; set; }
		public Destinations? Destinations { get; set; }
		public IEnumerable<Destinations>? DestinationList { get; set; }
		public IEnumerable<Currencies>? Currencies { get; set; }
        public IEnumerable<TourImage>? TourImages { get; set; }
        public EditUserViewModel? EditUserViewModels { get; set; }
        
        public IEnumerable<Expenses>? Expenses { get; set; }
		public Destinations? SelectedDestination { get; set; } // Sử dụng cho trang DestinationDetails




	}
}

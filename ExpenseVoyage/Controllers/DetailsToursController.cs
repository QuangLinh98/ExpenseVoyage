using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
    public class DetailsToursController : Controller
    {

        private readonly DatabaseContext _databaseContext;

        public DetailsToursController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;

        }

        public IActionResult Index()
        {
            var expenseToursList = _databaseContext.ExpenseTours.Include(e => e.TourImages).ToList();
            var destinations = _databaseContext.Destinations.ToList();

            var model = new HomeViewModel
            {
                DestinationList =destinations,
                ExpenseTourList = expenseToursList,
            };

            return View(model);
        }
    }
}

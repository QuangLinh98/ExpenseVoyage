using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
    public class AboutController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        public AboutController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<IActionResult> Index()
        {
            var photos = await _databaseContext.Photos
                .Include(d => d.PhotosImages)
                .Include(c => c.Destination)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Photos = photos
                // Set other properties if needed
            };

            return View(viewModel);
        }

    }
}

using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
    public class GalleryController : Controller
    {
        private readonly DatabaseContext _db;
        public GalleryController(DatabaseContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var photos = await _db.Photos
                .Include(d => d.PhotosImages)
                .Include(c => c.Destination)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Photos = photos
                // Add other properties if needed, such as Tours, Destinations, etc.
            };

            return View(viewModel);
        }



    }
}

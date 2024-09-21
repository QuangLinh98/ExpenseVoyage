
using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
    public class TourController : Controller
    {
        private DatabaseContext _dbContext;

        public TourController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            var listTour = _dbContext.Tours.ToList();
            return View(listTour); // Trả về danh sách tour trực tiếp
        }


        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Tour tours)
        {
            if (!ModelState.IsValid)
            {

                return View(tours);
            }
            else
            {
                await _dbContext.Tours.AddAsync(tours);
                await _dbContext.SaveChangesAsync();
                TempData["success"] = "Tours Create successfully";

                return RedirectToAction("Index");
            }

        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tour = await _dbContext.Tours.FirstOrDefaultAsync(t => t.TourId == id);
                if (tour == null)
                {
                    TempData["error"] = "Tour không tồn tại";
                    return RedirectToAction("Index");
                }
                var expenseTours = await _dbContext.ExpenseTours
                    .Where(et => et.TourID == id)
                    .ToListAsync();
                if (expenseTours.Any())
                {
                    _dbContext.ExpenseTours.RemoveRange(expenseTours);
                    await _dbContext.SaveChangesAsync();
                }
                _dbContext.Tours.Remove(tour);
                await _dbContext.SaveChangesAsync();
                TempData["success"] = "Tour đã được xóa thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Xóa tour thất bại: " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        public async Task<IActionResult> Edit(int id)
        {

            var tourExisted = await _dbContext.Tours.FirstOrDefaultAsync(p => p.TourId == id);

            return View(tourExisted);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Tour tours)
        {
            try
            {
                var tourExisted = await _dbContext.Tours.FirstOrDefaultAsync(p => p.TourId == id);

                if (tourExisted == null)
                {
                    return RedirectToAction("Index");
                }

                tourExisted.Departure = tours.Departure;
                tourExisted.StartDate = tours.StartDate;
                tourExisted.EndDate = tours.EndDate;
                tourExisted.Totalbudget = tours.Totalbudget;
                tourExisted.NameTour = tours.NameTour;
                _dbContext.Tours.Update(tourExisted);

                await _dbContext.SaveChangesAsync();
                TempData["success"] = "tourExisted Update successfully";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "tourExisted Update falessssss";
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(tours);
        }


    }
}

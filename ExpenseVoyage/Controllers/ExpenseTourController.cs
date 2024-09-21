using ExpenseVoyage.Helper;
using ExpenseVoyage.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
    public class ExpenseTourController : Controller
    {
        private DatabaseContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ExpenseTourController(DatabaseContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var expenseTours = await _dbContext.ExpenseTours.Include(c => c.TourImages).ToListAsync();
            return View(expenseTours);
        }

        public async Task<IActionResult> Create()
        {
            var tour = await _dbContext.Tours.ToListAsync();
            ViewBag.TourID = new SelectList(tour, "TourId", "TourId");
            ViewBag.TourNAME = new SelectList(tour, "NameTour", "NameTour");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ExpenseTour expenseTour, List<IFormFile> formFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _dbContext.ExpenseTours.AddAsync(expenseTour);
                    await _dbContext.SaveChangesAsync();

                    foreach (var item in formFiles)
                    {
                        var imagePath = await UploadFile.SaveImage("TourImage", item, _webHostEnvironment);
                        var tourImages = new TourImage
                        {
                            ImageUrl = imagePath,
                            ExpenseTourId = expenseTour.ExpenseTourId
                        };
                        await _dbContext.TourImages.AddAsync(tourImages);
                        await _dbContext.SaveChangesAsync();
                    }

                    TempData["success"] = "ExpenseTours Create successfully";

                    return RedirectToAction("Index");
                }
                else
                {

                    TempData["error"] = "ExpenseTours Create falessssss";

                    return RedirectToAction("Index");
                }


            }
            catch (Exception ex)
            {

                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(expenseTour);
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var EXtour = await _dbContext.ExpenseTours.FirstOrDefaultAsync(d => d.ExpenseTourId == id);

                if (EXtour == null)
                {
                    TempData["error"] = "ExpenseTours not found.";
                    return RedirectToAction("Index");
                }
                _dbContext.ExpenseTours.Remove(EXtour);
                await _dbContext.SaveChangesAsync();
                TempData["success"] = "ExpenseTours deleted successfully.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error deleting ExpenseTours.";
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        public async Task<IActionResult> Edit(int id)
        {

            var tourExisted = await _dbContext.ExpenseTours.Include(c => c.TourImages).FirstOrDefaultAsync(p => p.ExpenseTourId == id);

            return View(tourExisted);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] ExpenseTour expenseTours, List<IFormFile>? formFiles, List<int>? selectedImageIds)
        {
            try
            {
                var EXTourExisted = await _dbContext.ExpenseTours.Include(c => c.TourImages).FirstOrDefaultAsync(p => p.ExpenseTourId == id);

                if (EXTourExisted == null)
                {
                    return RedirectToAction("Index");
                }
                if (selectedImageIds != null && selectedImageIds.Count > 0)
                {
                    var selectedImages = await _dbContext.TourImages.Where(img => selectedImageIds.Contains(img.Id)).ToListAsync();

                    foreach (var image in selectedImages)
                    {
                        if (!string.IsNullOrEmpty(image.ImageUrl))
                        {
                            UploadFile.DeleteImage(_webHostEnvironment, image.ImageUrl);
                        }
                    }

                    _dbContext.TourImages.RemoveRange(selectedImages);
                }
                if (formFiles?.Count() > 0)
                {
                    foreach (var item in formFiles)
                    {
                        var imagePath = await UploadFile.SaveImage("TourImage", item, _webHostEnvironment);
                        var tourImages = new TourImage
                        {
                            ImageUrl = imagePath,
                            ExpenseTourId = expenseTours.ExpenseTourId
                        };
                        await _dbContext.TourImages.AddAsync(tourImages);
                    }
                }

                EXTourExisted.Derparture = expenseTours.Derparture;
                EXTourExisted.Day = expenseTours.Day;
                EXTourExisted.Date = expenseTours.Date;
                EXTourExisted.Category = expenseTours.Category;
                EXTourExisted.Destination = expenseTours.Destination;
                EXTourExisted.Cost = expenseTours.Cost;
                _dbContext.ExpenseTours.Update(EXTourExisted);

                await _dbContext.SaveChangesAsync();
                TempData["success"] = "EXTourExisted Update successfully";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "EXTourExisted Update falessssss";
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(expenseTours);
        }

    }
}

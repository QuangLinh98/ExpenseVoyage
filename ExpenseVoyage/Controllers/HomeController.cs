using System.Diagnostics;
using System.Linq.Expressions;
using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly DatabaseContext _databaseContext;

		public HomeController(ILogger<HomeController> logger, DatabaseContext databaseContext)
		{
			_databaseContext = databaseContext;
			_logger = logger;
		}

		public async Task<IActionResult> DestinationDetails(string selectedDestination)
		{
			if (string.IsNullOrEmpty(selectedDestination))
			{
				return RedirectToAction("Index");
			}

			// Lấy thông tin địa điểm dựa trên tên
			var destination = await _databaseContext.Destinations
				.Include(d => d.DestinationImages)
				.FirstOrDefaultAsync(d => d.Name == selectedDestination);

			if (destination == null)
			{
				return NotFound();
			}

			// Trả về HomeViewModel với SelectedDestination
			var model = new HomeViewModel
			{
				SelectedDestination = destination
			};

			return View(model);
		}




		public async Task<IActionResult> Index()
		{
			// Sử dụng ToListAsync() để lấy danh sách destinations một cách bất đồng bộ
			var destinations = await _databaseContext.Destinations
				.Include(d => d.DestinationImages)
				.ToListAsync();

			// Tạo SelectList từ danh sách destinations
			ViewBag.DestinationList = new SelectList(destinations, "Name", "Name");


			// Lấy danh sách tour ngẫu nhiên
			var randomTours = await _databaseContext.ExpenseTours
				.OrderBy(r => Guid.NewGuid())  // Sắp xếp ngẫu nhiên
				.Take(2)                       // Lấy 2 bản ghi ngẫu nhiên
				.Include(c => c.TourImages)    // Bao gồm ảnh tour
				.ToListAsync();

			var model = new HomeViewModel
			{
				PhotosImages = await _databaseContext.PhotosImages.Include(d => d.Photos).ToListAsync(),
				DestinationImages = await _databaseContext.DestinationImages.Include(d => d.Destinations).ToListAsync(),
				ExpenseTourList = randomTours
			};

			return View(model);
		}

		public async Task<IActionResult> DetailsTour(int id)
		{
			var expenseTour = await _databaseContext.ExpenseTours
								 .Include(c => c.TourImages)
								 .FirstOrDefaultAsync(t => t.ExpenseTourId == id);

			if (expenseTour == null)
			{
				return NotFound(); // Xử lý trường hợp không tìm thấy tour
			}

			var model = new HomeViewModel
			{
				ExpenseTours = expenseTour, // Chuyển đổi thành danh sách
				PhotosImages = await _databaseContext.PhotosImages.ToListAsync(),
				DestinationImages = await _databaseContext.DestinationImages.ToListAsync()
			};

			return View(model); // Trả về HomeViewModel cho view
		}


		public IActionResult Details(int id)
		{
			// Fetch the destination based on ID with its related Destinations
			var destinations = _databaseContext.Destinations.Include(c => c.DestinationImages).FirstOrDefault(d => d.DestinationId == id);

			if (destinations == null)
			{
				return NotFound();
			}


			var model = new HomeViewModel
			{
				Destinations = destinations, // Assign the fetched destination
				DestinationImages = destinations.DestinationImages // Include related images if necessary
			};

			return View(model); // Pass the HomeViewModel to the View
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}

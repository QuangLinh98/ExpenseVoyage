using ExpenseVoyage.DTOs;
using ExpenseVoyage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
	public class UserTripController : Controller
	{
		private readonly DatabaseContext _dbContext;

		public UserTripController(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IActionResult> Index()
		{
			var destinations = await _dbContext.Destinations.ToListAsync();
			ViewBag.Destination = new SelectList(destinations, "Name", "Name", "DestinationId");

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Trips model)
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null)
			{
				// Handle case where userId is not in session, e.g., user not logged in
				ModelState.AddModelError("", "User is not logged in.");
				TempData["Trip"] = model.ToString();
				return RedirectToAction("Index");
			}

			model.UserId = (int)userId;

			var destinations = await _dbContext.Destinations.ToListAsync();
			ViewBag.Destination = new SelectList(destinations, "Name", "Name", "DestinationId");
			if (model.Departure == model.Destination)
			{
				ModelState.AddModelError("Destination", "Departure and destination cannot be the same.");
			}

			if (model.StartDate < DateTime.Now.Date)
			{
				ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
			}

			if (model.EndDate < DateTime.Now.Date)
			{
				ModelState.AddModelError("EndDate", "End date cannot be in the past.");
			}

			if (model.StartDate >= model.EndDate)
			{
				ModelState.AddModelError("EndDate", "Start date must be earlier than end date.");
			}
			if (ModelState.IsValid)
			{
				_dbContext.Trips.Add(model);
				await _dbContext.SaveChangesAsync();

				return RedirectToAction("Details", new { id = model.TripId });
			}

			return RedirectToAction("Index");
		}


		[HttpPost]
		public IActionResult EditTrip(Trips model)
		{
			if (ModelState.IsValid)
			{
				var existingTrip = _dbContext.Trips.Find(model.TripId);
				if (existingTrip != null)
				{
					// Server-side validations
					if (model.StartDate < DateTime.Today)
					{
						ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
					}
					if (model.EndDate < DateTime.Today)
					{
						ModelState.AddModelError("EndDate", "End date cannot be in the past.");
					}
					if (model.EndDate < model.StartDate)
					{
						ModelState.AddModelError("EndDate", "End date must be after start date.");
					}
					if (model.Departure.Equals(model.Destination, StringComparison.OrdinalIgnoreCase))
					{
						ModelState.AddModelError("Destination", "Departure and Destination cannot be the same.");
					}

					if (ModelState.IsValid)
					{
						existingTrip.TripId = model.TripId;
						existingTrip.Departure = model.Departure;
						existingTrip.Destination = model.Destination;
						existingTrip.StartDate = model.StartDate;
						existingTrip.EndDate = model.EndDate;
						existingTrip.TotalBudget = model.TotalBudget;

						_dbContext.Trips.Update(existingTrip);
						_dbContext.SaveChanges();

						return RedirectToAction("Details", new { id = model.TripId });
					}
				}
				else
				{
					ModelState.AddModelError("", "Trip not found.");
				}
			}
			// If the model state is not valid, reload the view with the existing data
			return View(model);
		}

		public async Task<IActionResult> Details(int id)
		{
			var trip = await _dbContext.Trips
			.Include(t => t.Itineraries)
			.ThenInclude(i => i.Expenses)
			.FirstOrDefaultAsync(t => t.TripId == id);

			if (trip == null)
			{
				return NotFound();
			}
			decimal totalItineraryCost = trip.Itineraries?.Sum(i => i.Cost) ?? 0;

			decimal totalExpenseCost = trip.Itineraries
				   .SelectMany(i => i.Expenses)
				   .Sum(e => e.Amount);

			decimal remainingBudget = trip.TotalBudget - (totalItineraryCost + totalExpenseCost);

			ViewBag.TotalItineraryCost = totalItineraryCost;
			ViewBag.TotalExpenseCost = totalExpenseCost;
			ViewBag.RemainingBudget = remainingBudget;

			return View(trip);

		}
		public async Task<IActionResult> CreateItinerary(int id)
		{
			var trip = await _dbContext.Trips.FindAsync(id);

			var destinations = await _dbContext.Destinations.ToListAsync();
			ViewBag.Destination = new SelectList(destinations, "Name", "Name");

			var dates = Enumerable.Range(0, (trip.EndDate - trip.StartDate).Days + 1)
					.Select(offset => trip.StartDate.AddDays(offset))
					.ToList();

			ViewBag.Dates = dates.Select(date => new SelectListItem
			{
				Value = date.ToString("yyyy-MM-dd"),
				Text = date.ToShortDateString()
			});
			if (trip == null)
			{
				return NotFound();
			}

			return PartialView("_CreateItineraryForm", new Itineraries { TripId = trip.TripId });
		}

		[HttpPost]
		public async Task<IActionResult> SaveItnExpenses(Itineraries model)
		{
			if (ModelState.IsValid)
			{
				var trip = await _dbContext.Trips.FindAsync(model.TripId);

				if (trip == null)
				{
					return NotFound();
				}
				if (!ModelState.IsValid)
				{
					return PartialView("_CreateItineraryForm", new Itineraries { TripId = trip.TripId });
				}
				trip.TripId = model.TripId;
				await _dbContext.Itineraries.AddAsync(model);
				await _dbContext.SaveChangesAsync();

				var totalItineraryCost = await _dbContext.Itineraries
			.Where(i => i.TripId == trip.TripId)
			.SumAsync(i => i.Cost);

				// Tính ngân sách còn lại
				decimal remainingBudget = trip.TotalBudget - totalItineraryCost;

				// Kiểm tra điều kiện ngân sách
				if (remainingBudget < 0)
				{
					string message = "Nội dung chuyến đi vượt quá ngân sách.";
					await CreateNotification(trip.UserId, message, "Error");
					ModelState.AddModelError("", message);

				}
				else if (remainingBudget < trip.TotalBudget * 0.10m)
				{
					string message = "Ngân sách còn lại dưới 10% ngân sách tổng.";
					await CreateNotification(trip.UserId, message, "Warning");
					ModelState.AddModelError("", message);
				}
				return RedirectToAction("Details", new { id = model.TripId });
			}

			return RedirectToAction("Index");
		}

		private async Task CreateNotification(int userId, string message, string type)
		{
			var notification = new Notifications
			{
				UserId = userId,
				Message = message,
				Type = type,
				Date = DateTime.Now
			};

			_dbContext.Notifications.Add(notification);
			await _dbContext.SaveChangesAsync();
		}
		public async Task<IActionResult> DeleteItinerary(int id)
		{
			var itinerary = await _dbContext.Itineraries.Include(i => i.Trip)
		.FirstOrDefaultAsync(i => i.ItineraryId == id);

			if (itinerary != null)
			{
				_dbContext.Itineraries.Remove(itinerary);
				await _dbContext.SaveChangesAsync();
			}

			return RedirectToAction("Details", new { id = itinerary.TripId });
		}

		public async Task<IActionResult> CreateExpenses(int id)
		{
			var itinerary = await _dbContext.Itineraries.FindAsync(id);
			var trip = await _dbContext.Trips.FindAsync(itinerary?.TripId);
			if (itinerary == null)
			{
				return NotFound();
			}
			if (trip == null)
			{
				return NotFound();
			}
			var dates = Enumerable.Range(0, (trip.EndDate - trip.StartDate).Days + 1)
					.Select(offset => trip.StartDate.AddDays(offset))
					.ToList();

			ViewBag.Dates = dates.Select(date => new SelectListItem
			{
				Value = date.ToString("yyyy-MM-dd"),
				Text = date.ToShortDateString()
			});

			var model = new ItineraryExpenseViewModel
			{
				TripId = itinerary.TripId,
				ItineraryId = id,
				Itineraries = new List<Itineraries>(),
				Expenses = new List<Expenses>(),
				Day = itinerary.Day
			};
			return View(model);

		}
		[HttpPost]
		public async Task<IActionResult> SaveExpense(ItineraryExpenseViewModel model)
		{
			var trip = await _dbContext.Trips.FindAsync(model.TripId); // Correct TripId

			if (ModelState.IsValid)
			{
				foreach (var expense in model.Expenses)
				{
					expense.ItineraryId = model.ItineraryId;
					await _dbContext.Expenses.AddAsync(expense);
				}

				await _dbContext.SaveChangesAsync();
				return RedirectToAction("Details", new { id = trip.TripId });
			}

			return RedirectToAction("CreateExpenses", new { id = model.TripId });
		}

		public async Task<IActionResult> DeleteExpense(int id)
		{
			var expense = await _dbContext.Expenses.Include(i => i.Itineraries)
		.FirstOrDefaultAsync(i => i.ExpenseId == id);

			if (expense != null)
			{
				_dbContext.Expenses.Remove(expense);
				await _dbContext.SaveChangesAsync();
			}

			return RedirectToAction("Details", new { id = expense?.Itineraries?.TripId });
		}
	}
}

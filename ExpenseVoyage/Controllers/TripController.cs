using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ExpenseVoyage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExpenseVoyage.DTOs;

namespace ExpenseVoyage.Controllers
{
	public class TripController : Controller
	{
		private readonly DatabaseContext _dbContext;

		public TripController(DatabaseContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IActionResult> List()
		{

			var trips = await _dbContext.Trips.Include(t => t.User).ToListAsync();

			return View(trips);
		}
		public async Task<IActionResult> Create()
		{
			var users = await _dbContext.Users.ToListAsync();
			var destinations = await _dbContext.Destinations.ToListAsync();

			ViewBag.Users = new SelectList(users, "UserId", "Username", "UserId");
			ViewBag.Destination = new SelectList(destinations, "Name", "Name", "DestinationId");

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Trips model)
		{
			var users = await _dbContext.Users.ToListAsync();
			var destinations = await _dbContext.Destinations.ToListAsync();

			ViewBag.Users = new SelectList(users, "UserId", "Username", "UserId");
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



			return View(model);
		}

		public async Task<IActionResult> Update(int idy)
		{
			var trip = await _dbContext.Trips.FindAsync(idy);

			var users = await _dbContext.Users.ToListAsync();
			var destinations = await _dbContext.Destinations.ToListAsync();

			ViewBag.Users = new SelectList(users, "UserId", "Username", "UserId");
			ViewBag.Destination = new SelectList(destinations, "Name", "Name", "DestinationId");

			return View(trip);
		}
		[HttpPost]
		public async Task<IActionResult> Update(Trips model)
		{
			var users = await _dbContext.Users.ToListAsync();
			var destinations = await _dbContext.Destinations.ToListAsync();

			ViewBag.Users = new SelectList(users, "UserId", "Username", model.UserId);
			ViewBag.Departure = new SelectList(destinations, "Name", "Name", model.Departure);
			ViewBag.Destination = new SelectList(destinations, "Name", "Name", model.Destination);

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
				var existingTrip = await _dbContext.Trips.FindAsync(model.TripId);

				if (existingTrip == null)
				{
					return NotFound();
				}

				existingTrip.UserId = model.UserId;
				existingTrip.Departure = model.Departure;
				existingTrip.Destination = model.Destination;
				existingTrip.StartDate = model.StartDate;
				existingTrip.EndDate = model.EndDate;
				existingTrip.TotalBudget = model.TotalBudget;

				_dbContext.Update(existingTrip);
				await _dbContext.SaveChangesAsync();

				return RedirectToAction("List");
			}

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

			ViewBag.totalItineraryCost = totalItineraryCost;
			ViewBag.totalExpenseCost = totalExpenseCost;
			ViewBag.RemainingBudget = remainingBudget;
			return View(trip);
		}
		public async Task<IActionResult> CreateItinerary(int id)
		{

			var trip = await _dbContext.Trips.FindAsync(id);

			var destinations = await _dbContext.Destinations.ToListAsync();
			ViewBag.Destination = new SelectList(destinations, "Name", "Name", "DestinationId");

			if (trip == null)
			{
				return NotFound();
			}

			return View(new Itineraries { TripId = trip.TripId });
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
					return View("CreateItinerary", model);
				}
				trip.TripId = model.TripId;
				await _dbContext.Itineraries.AddAsync(model);
				await _dbContext.SaveChangesAsync();

				return RedirectToAction("Details", new { id = model.TripId });
			}

			return View("CreateItn", model);
		}


		[HttpPost]
		public IActionResult CreateItineraries(int tripId, List<Itineraries> itineraries)
		{
			if (!ModelState.IsValid)
			{
				return View("Error", ModelState);
			}

			var trip = _dbContext.Trips
				.Include(t => t.Itineraries)
				.FirstOrDefault(t => t.TripId == tripId);

			if (trip == null)
			{
				return NotFound();
			}

			foreach (var itinerary in itineraries)
			{
				itinerary.TripId = tripId;
				_dbContext.Itineraries.Add(itinerary);
			}



			_dbContext.SaveChanges();

			return RedirectToAction("Details", new { id = tripId });
		}
		[HttpPost]
		public async Task<IActionResult> SaveItnExpenses1(ItineraryExpenseViewModel model)
		{
			if (ModelState.IsValid)
			{
				var trip = await _dbContext.Trips.FindAsync(model.TripId);

				if (trip == null)
				{
					return NotFound();
				}

				foreach (var itinerary in model.Itineraries)
				{
					if (itinerary.Date < trip.StartDate || itinerary.Date > trip.EndDate)
					{
						ModelState.AddModelError($"Itineraries[{model.Itineraries.IndexOf(itinerary)}].Date", "Date must be within the trip's start and end dates.");
					}
				}

				if (!ModelState.IsValid)
				{
					return View("CreateItn", model);
				}
				// Save itineraries if the model state is valid
				foreach (var itinerary in model.Itineraries)
				{
					itinerary.TripId = model.TripId;
					await _dbContext.Itineraries.AddAsync(itinerary);
				}

				await _dbContext.SaveChangesAsync();

				return RedirectToAction("Details", new { id = model.TripId });
			}

			return View("CreateItn", model);
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

			return RedirectToAction("Details", "Trip", new { id = itinerary.TripId });
		}

		public async Task<IActionResult> CreateExpense(int id)
		{
			var itinerary = await _dbContext.Itineraries.FindAsync(id);

			if (itinerary == null)
			{
				return NotFound();
			}
			var model = new ItineraryExpenseViewModel
			{
				TripId = itinerary.TripId,
				ItineraryId = id,
				Itineraries = new List<Itineraries>(),
				Expenses = new List<Expenses>()
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

			return RedirectToAction("List", new { id = model.TripId });
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

			return RedirectToAction("Details", "Trip", new { id = expense?.Itineraries?.TripId });
		}
	}

}
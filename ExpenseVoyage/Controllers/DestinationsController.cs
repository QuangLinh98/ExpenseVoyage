
using ExpenseVoyage.Helper;
using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExpenseVoyage.Controllers
{

	public class DestinationsController : Controller
	{
		private DatabaseContext _dbContext;

		private IWebHostEnvironment _webHostEnvironment;
		public DestinationsController(DatabaseContext dbContext, IWebHostEnvironment webHostEnvironment)
		{
			_dbContext = dbContext;
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task<IActionResult> Index(string? searchTerm)
		{
			var destinations = _dbContext.Destinations.Include(d => d.DestinationImages).AsQueryable();
			var destinationxx = await SearchAsync(destinations, searchTerm, nameof(Destinations.Name));
			ViewBag.DestinationNAME = new SelectList(destinations, "Name", "Name");

			return View(destinationxx);
		}


		public async Task<IQueryable<T>> SearchAsync<T>(IQueryable<T> query, string searchTerm, params string[] propertyNames)
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
			{
				return query;
			}

			var parameter = Expression.Parameter(typeof(T), "x");
			var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
			var predicate = propertyNames.Select(propertyName =>
			{
				var property = Expression.Property(parameter, propertyName);
				// Kiểm tra xem kiểu thuộc tính có phải là string không
				if (property.Type == typeof(string))
				{
					return Expression.Call(property, containsMethod, Expression.Constant(searchTerm));
				}
				return null; // Hoặc một biểu thức khác phù hợp
			})
			.Where(expression => expression != null)
			.Aggregate<Expression, Expression>(null, (current, expression) => current == null ? expression : Expression.OrElse(current, expression));

			var finalPredicate = Expression.Lambda<Func<T, bool>>(predicate, parameter);
			return query.Where(finalPredicate);
		}


		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromForm] Destinations destinations, List<IFormFile> formFiles)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _dbContext.Destinations.AddAsync(destinations);
					await _dbContext.SaveChangesAsync();
				}
				foreach (var item in formFiles)
				{
					var imagePath = await UploadFile.SaveImage("DestinationImage", item, _webHostEnvironment);
					var destinationImages = new DestinationImage
					{
						ImageUrl = imagePath,
						DestinationId = destinations.DestinationId
					};
					await _dbContext.DestinationImages.AddAsync(destinationImages);
					await _dbContext.SaveChangesAsync();
				}
				TempData["success"] = "Destinations Create successfully";


				return RedirectToAction("Index");

			}
			catch (Exception ex)
			{
				TempData["error"] = "Destinations Create falessssss";
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			return View(destinations);
		}
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				// Tìm Destination cùng với các hình ảnh liên quan
				var destination = await _dbContext.Destinations.Include(d => d.DestinationImages).Include(v => v.Photos)
					.FirstOrDefaultAsync(d => d.DestinationId == id);


				if (destination == null)
				{
					return RedirectToAction("Index");
				}

				// Xóa các file hình ảnh từ hệ thống tệp
				if (destination.DestinationImages != null && destination.DestinationImages.Count > 0)
				{
					foreach (var image in destination.DestinationImages)
					{
						if (!string.IsNullOrEmpty(image.ImageUrl))
						{
							UploadFile.DeleteImage(_webHostEnvironment, image.ImageUrl);
						}
					}
					// Xóa các bản ghi từ bảng MultipleImage
					_dbContext.DestinationImages.RemoveRange(destination.DestinationImages);
				}

				var groupedPhotos = await _dbContext.Photos
					 .Include(d => d.Destination)
					 .Include(p => p.PhotosImages) // Include the PhotosImages for deletion
					 .GroupBy(p => p.DestinationId)
					 .ToListAsync();
				if (groupedPhotos != null)
				{
					foreach (var group in groupedPhotos)
					{
						foreach (var photos in group)
						{
							// Xóa các file hình ảnh từ hệ thống tệp
							if (photos.PhotosImages != null && photos.PhotosImages.Count > 0)
							{
								foreach (var image in photos.PhotosImages)
								{
									if (!string.IsNullOrEmpty(image.ImageUrl))
									{
										UploadFile.DeleteImage(_webHostEnvironment, image.ImageUrl);
									}
								}
								// Xóa các bản ghi từ bảng MultipleImage
								_dbContext.PhotosImages.RemoveRange(photos.PhotosImages);
							}
							// Xóa bản ghi photos
							_dbContext.Photos.Remove(photos);
						}
					}

					// Lưu các thay đổi vào cơ sở dữ liệu
					await _dbContext.SaveChangesAsync();
				}



				// Xóa bản ghi Destination

				_dbContext.Destinations.Remove(destination);
				// Lưu các thay đổi vào cơ sở dữ liệu
				await _dbContext.SaveChangesAsync();
				TempData["success"] = "Destinations delete successfully";


				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["error"] = "Destinations delete falessssss";
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			return View();

		}


		public async Task<IActionResult> Edit(int id)
		{

			var destinationExisted = await _dbContext.Destinations.Include(p => p.DestinationImages)
															  .FirstOrDefaultAsync(p => p.DestinationId == id);

			return View(destinationExisted);
		}
		[HttpPost]
		public async Task<IActionResult> Edit(int id, [FromForm] Destinations destinations, List<IFormFile>? formFiles, List<int>? selectedImageIds)
		{
			try
			{
				var destinationsExisted = await _dbContext.Destinations.Include(p => p.DestinationImages).FirstOrDefaultAsync(p => p.DestinationId == id);

				if (destinationsExisted == null)
				{
					return RedirectToAction("Index");
				}

				if (selectedImageIds != null && selectedImageIds.Count > 0)
				{
					var selectedImages = await _dbContext.DestinationImages.Where(img => selectedImageIds.Contains(img.Id)).ToListAsync();

					foreach (var image in selectedImages)
					{
						if (!string.IsNullOrEmpty(image.ImageUrl))
						{
							UploadFile.DeleteImage(_webHostEnvironment, image.ImageUrl);
						}
					}

					_dbContext.DestinationImages.RemoveRange(selectedImages);
				}

				if (formFiles?.Count() > 0)
				{
					foreach (var item in formFiles)
					{
						var imagePath = await UploadFile.SaveImage("DestinationImage", item, _webHostEnvironment);
						var destinationImages = new DestinationImage
						{
							ImageUrl = imagePath,
							DestinationId = destinations.DestinationId
						};
						await _dbContext.DestinationImages.AddAsync(destinationImages);
					}
				}

				destinationsExisted.Description = destinations.Description;
				destinationsExisted.Name = destinations.Name;

				_dbContext.Destinations.Update(destinationsExisted);

				await _dbContext.SaveChangesAsync();

				TempData["success"] = "Destinations Update successfully";

				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["error"] = "Destinations Update falessssss";
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			return View(destinations);
		}



	}
}

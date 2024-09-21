using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseVoyage.Models;
using ExpenseVoyage.Helper;

namespace ExpenseVoyage.Controllers
{
	public class PhotosController : Controller
	{
		private readonly DatabaseContext _dbContext;

		private readonly IWebHostEnvironment _webHostEnvironment;
		public PhotosController(DatabaseContext dbContext, IWebHostEnvironment webHostEnvironment)
		{
			_dbContext = dbContext;
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task<IActionResult> Index()
		{
			var photos = await _dbContext.Photos.Include(d => d.PhotosImages).Include(c => c.Destination).ToListAsync();

			return View(photos);
		}
		public async Task<IActionResult> Create()
		{
			var destinations = await _dbContext.Destinations.ToListAsync();
			ViewBag.DestinationID = new SelectList(destinations, "DestinationId", "DestinationId");
			ViewBag.DestinationNAME = new SelectList(destinations, "Name", "Name");
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromForm] Photos photo, List<IFormFile> formFiles)
		{
			try
			{
				if (ModelState.IsValid)
				{
					await _dbContext.Photos.AddAsync(photo);
					await _dbContext.SaveChangesAsync();
				}
				foreach (var item in formFiles)
				{
					var imagePath = await UploadFile.SaveImage("PhotoImage", item, _webHostEnvironment);
					var photoImages = new PhotosImage
					{
						ImageUrl = imagePath,
						PhotoId = photo.PhotoId
					};
					await _dbContext.PhotosImages.AddAsync(photoImages);
					await _dbContext.SaveChangesAsync();
				}

				TempData["success"] = "Photos Create successfully";

				return RedirectToAction("Index");

			}
			catch (Exception ex)
			{
				TempData["error"] = "Photos Create falessssss";
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			return View(photo);
		}
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				// Tìm photos cùng với các hình ảnh liên quan
				var photos = await _dbContext.Photos
					.Include(d => d.PhotosImages)
					.FirstOrDefaultAsync(d => d.PhotoId == id);

				if (photos == null)
				{
					return RedirectToAction("Index");
				}

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
				// Lưu các thay đổi vào cơ sở dữ liệu
				await _dbContext.SaveChangesAsync();
				TempData["success"] = "Photos Delete successfully";


				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["error"] = "Photos Delete falessssss";
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			return View();

		}


		public async Task<IActionResult> Edit(int id)
		{

			var photoExisted = await _dbContext.Photos.Include(p => p.PhotosImages)
															  .FirstOrDefaultAsync(p => p.PhotoId == id);

			return View(photoExisted);
		}
		[HttpPost]
		public async Task<IActionResult> Edit(int id, [FromForm] Photos photo, List<IFormFile>? formFiles, List<int>? selectedImageIds)
		{
			try
			{
				var photoExisted = await _dbContext.Photos.Include(p => p.PhotosImages).FirstOrDefaultAsync(p => p.PhotoId == id);

				if (photoExisted == null)
				{
					return RedirectToAction("Index");
				}

				if (selectedImageIds != null && selectedImageIds.Count > 0)
				{
					var selectedImages = await _dbContext.PhotosImages.Where(img => selectedImageIds.Contains(img.Id)).ToListAsync();

					foreach (var image in selectedImages)
					{
						if (!string.IsNullOrEmpty(image.ImageUrl))
						{
							UploadFile.DeleteImage(_webHostEnvironment, image.ImageUrl);
						}
					}

					_dbContext.PhotosImages.RemoveRange(selectedImages);
				}

				if (formFiles?.Count() > 0)
				{
					foreach (var item in formFiles)
					{
						var imagePath = await UploadFile.SaveImage("PhotoImage", item, _webHostEnvironment);
						var photoImage = new PhotosImage
						{
							ImageUrl = imagePath,
							PhotoId = photo.PhotoId
						};
						await _dbContext.PhotosImages.AddAsync(photoImage);
					}
				}

				photoExisted.Caption = photo.Caption;

				_dbContext.Photos.Update(photoExisted);

				await _dbContext.SaveChangesAsync();
				TempData["success"] = "Photos Update successfully";


				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["error"] = "Photos Update falessssss";
				ModelState.AddModelError(string.Empty, ex.Message);
			}
			return View(photo);
		}
	}
}

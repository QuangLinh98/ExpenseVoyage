using ExpenseVoyage.Helper;
using ExpenseVoyage.Models;
using ExpenseVoyage.Service;
using ExpenseVoyage.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ExpenseVoyage.Controllers
{
	public class AdminController : Controller
	{
		private readonly IDistributedCache _cache;
		private readonly IPasswordHasher<Users> _passwordHasher;
		private readonly EmailService _emailService;
		private readonly EmailSetting _emailSettings;
		private readonly DatabaseContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public AdminController(DatabaseContext context,
							  IDistributedCache cache,
								EmailService emailService,
								IOptions<EmailSetting> emailSettings,
								IPasswordHasher<Users> passwordHasher,
								IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_cache = cache;
			_passwordHasher = passwordHasher;
			_emailService = emailService;
			_emailSettings = emailSettings.Value;
			_webHostEnvironment = webHostEnvironment;
		}
		public async Task<IActionResult> Index()
		{
			var users = await _context.Users.ToListAsync();
			return View(users);
		}

		public IActionResult Login()
		{
			ViewBag.HideSideBar = true;
			return View();
		}

		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var existingUser = await _context.Users.FindAsync(id);
				if (existingUser == null)
				{
					return NotFound();
				}
				_context.Users.Remove(existingUser);
				await _context.SaveChangesAsync();

				//Xóa cache
				string cacheKey = $"User:{existingUser.Email}";
				await _cache.RemoveAsync(cacheKey);

				TempData["Message"] = "User deleted successfully!";
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", ex.Message);
			}
			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Profile()
		{
			//Users user = null;

			// Lấy email từ session
			string email = HttpContext.Session.GetString("AdminEmail");
			if (string.IsNullOrEmpty(email))
			{
				TempData["ErrorMessage"] = "User not logged in.";
				return RedirectToAction("Login");
			}

			// Lấy thông tin người dùng từ database theo email
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				TempData["ErrorMessage"] = "User not found.";
				return RedirectToAction("Login");
			}

			// Chuyển đổi từ model Users sang EditUserViewModel
			var viewModel = new EditUserViewModel
			{
				UserId = user.UserId,
				Username = user.Username,
				Email = user.Email,
				Address = user.Address,
				Phone = user.Phone,
				ImagePath = user.ImagePath
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Profile(EditUserViewModel model)
		{
			if (!ModelState.IsValid)
			{
				TempData["ErrorMessageProfile"] = "Information update invalid";
				return RedirectToAction("Profile");
			}

			//Lấy Email từ session 
			string email = HttpContext.Session.GetString("AdminEmail");
			if (string.IsNullOrEmpty(email))
			{
				 
				return RedirectToAction("Login");
			}

			// Lấy thông tin người dùng từ database theo email
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null)
			{
				TempData["ErrorMessageProfile"] = "User not found.";
				return RedirectToAction("Login");
			}

			// Cập nhật các trường thông tin người dùng
			user.Username = model.Username;
			user.Phone = model.Phone;
			user.Address = model.Address;

			if (model.ImageFile != null && model.ImageFile.Length > 0)
			{
				var subFolder = "UserImages";
				var saveImagePath = await UploadFile.SaveImage(subFolder, model.ImageFile, _webHostEnvironment);
				user.ImagePath = saveImagePath;
				HttpContext.Session.SetString("UserImagePath", user.ImagePath);
			}
			else
			{
				model.ImagePath = user.ImagePath;
			}

			// Lưu các thay đổi vào database
			_context.Users.Update(user);
			await _context.SaveChangesAsync();

			// Cập nhật thông tin người dùng trong cache
			string cacheKey = $"User:{email}";
			string cachedUser = JsonConvert.SerializeObject(user);
			await _cache.SetStringAsync(cacheKey, cachedUser, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
			});

			TempData["SuccessMessageProfile"] = "Profile updated successfully.";
			return RedirectToAction("Profile");
		}

		public async Task<IActionResult> Logout()
		{
			HttpContext.Session.Remove("AdminId");
			HttpContext.Session.Remove("Adminname");
			return Redirect("~/Admin/Login");
		}
	}
}

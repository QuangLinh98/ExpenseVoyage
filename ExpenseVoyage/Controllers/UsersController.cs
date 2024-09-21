using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseVoyage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using ExpenseVoyage.Service;
using ExpenseVoyage.Helper;
using Microsoft.Extensions.Options;
using ExpenseVoyage.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using ExpenseVoyage.Viewmodels;


namespace ExpenseVoyage.Controllers
{
	public class UsersController : Controller
	{
		private readonly DatabaseContext _context;
		private readonly IPasswordHasher<Users> _passwordHasher;
		private readonly EmailService _emailService;
		private readonly EmailSetting _emailSettings;
		private readonly IDistributedCache _cache;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public UsersController(DatabaseContext context,
			IPasswordHasher<Users> passwordHasher,
			EmailService emailService,
			IOptions<EmailSetting> emailSettings,
			IDistributedCache cache,
			IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_passwordHasher = passwordHasher;
			_emailService = emailService;
			_emailSettings = emailSettings.Value;
			_cache = cache;
			_webHostEnvironment = webHostEnvironment;
		}

		// GET: Users
		public async Task<IActionResult> Index()
		{
			return View(await _context.Users.ToListAsync());
		}

		//Login  View
		public IActionResult Login()
		{
			return View();
		}

		
		public async Task<IActionResult> Profile()
		{
			//Users user = null;

			// Lấy email từ session
			string email = HttpContext.Session.GetString("UserEmail");
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
			var ModelView = new EditUserViewModel
			{
				UserId = user.UserId,
				Username = user.Username,
				Email = user.Email,
				Address = user.Address,
				Phone = user.Phone,
				ImagePath = user.ImagePath
			};

			var mewModes = new HomeViewModel
			{
				EditUserViewModels = ModelView,
			};

			return View(mewModes);
		}

		[HttpPost]
		public async Task<IActionResult> Profile(HomeViewModel model)
		{
			if (!ModelState.IsValid)
			{
				TempData["ErrorMessageProfile"] = "Information update invalid";
				return RedirectToAction("Profile");
			}

			//Lấy Email từ session 
			string email = HttpContext.Session.GetString("UserEmail");
			if (string.IsNullOrEmpty(email))
			{
				TempData["ErrorMessage"] = "User not logged in.";
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
			user.Username = model.EditUserViewModels.Username;
			user.Phone = model.EditUserViewModels.Phone;
			user.Address = model.EditUserViewModels.Address;

			if (model.EditUserViewModels.ImageFile != null && model.EditUserViewModels.ImageFile.Length > 0)
			{
				var subFolder = "UserImages";
				var saveImagePath = await UploadFile.SaveImage(subFolder, model.EditUserViewModels.ImageFile,_webHostEnvironment);
				user.ImagePath = saveImagePath;
				HttpContext.Session.SetString("UserImagePath", user.ImagePath);
			}
			else
			{
				model.EditUserViewModels.ImagePath = user.ImagePath;
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

		public IActionResult Register()
		{
			return View();
		}



		[HttpPost]
		public async Task<IActionResult> Register(Users user, IFormFile imageFile)
		{
			if (ModelState.IsValid)
			{
				//user.Role = "User";

				// Kiểm tra email đã tồn tại trong hệ thống chưa
				var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
				if (existingUser != null)
				{
					// Thêm lỗi nếu email đã tồn tại
					ModelState.AddModelError("Email", "Email already exists");
					return View(user);
				}

				// Băm mật khẩu trước khi lưu vào cơ sở dữ liệu
				user.Password = _passwordHasher.HashPassword(user, user.Password);

				// Xử lý ảnh nếu có
				if (imageFile != null && imageFile.Length > 0)
				{
					var subFolder = "UserImages";
					var saveImagePath = await UploadFile.SaveImage(subFolder, imageFile, _webHostEnvironment);
					user.ImagePath = saveImagePath;
				}

				// Lưu người dùng vào cơ sở dữ liệu
				await _context.Users.AddAsync(user);
				await _context.SaveChangesAsync();

				// Tạo mã OTP ngẫu nhiên (6 chữ số)
				var otp = GenerateOtp(6);

				// Lưu OTP vào cache với thời gian hết hạn
				var cacheKey = $"OTP_{user.Email}";
				var cacheOptions = new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // OTP có thời hạn 10 phút
				};
				await _cache.SetStringAsync(cacheKey, otp, cacheOptions);

				// Gửi OTP đến email người dùng
				await _emailService.SendOtpEmail(user.Email, otp);

				// Điều hướng đến trang Verify OTP
				TempData["Email"] = user.Email; // Lưu email vào TempData để sử dụng khi verify OTP
				return RedirectToAction("VerifyOtp");
			}
			return View(user);
		}

		// Phương thức tạo mã OTP ngẫu nhiên
		private string GenerateOtp(int length)
		{
			var random = new Random();
			string otp = string.Empty;

			for (int i = 0; i < length; i++)
			{
				otp += random.Next(0, 10); // Sinh các chữ số ngẫu nhiên từ 0 đến 9
			}

			return otp;
		}

		public IActionResult VerifyOtp()
		{
			return View();
		}


		[HttpPost]
		public async Task<IActionResult> VerifyOtp(string otp)
		{
			var email = TempData["Email"]?.ToString();
			if (email == null)
			{
				ModelState.AddModelError("", "OTP verification failed. Please try again.");
				return View();
			}

			var cachedOtp = await _cache.GetStringAsync($"OTP_{email}");
			if (cachedOtp == null)
			{
				ModelState.AddModelError("", "OTP has expired. Please request a new one.");
				return View();
			}

			if (otp == cachedOtp)
			{
				//OTP chính xác, đánh dấu email của người dùng là đã xác nhận
				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
				if (user != null)
				{
					user.EmailConfirmed = true;

					// Cập nhật người dùng trong cơ sở dữ liệu
					_context.Users.Update(user);
					await _context.SaveChangesAsync();

					// Xóa OTP khỏi bộ đệm sau khi xác nhận thành công
					await _cache.RemoveAsync($"OTP_{email}");

					TempData["Message"] = "Email confirmed successfully!";
					return RedirectToAction("Login");
				}
			}
			else
			{
				ModelState.AddModelError("", "Invalid OTP. Please try again.");
			}

			return View();
		}

		private bool VerifyPassword(Users user, string password)
		{
			var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
			return result == PasswordVerificationResult.Success;
		}

		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			try
			{
				if (string.IsNullOrEmpty(email))
				{
					ModelState.AddModelError("Email", "Email is required.");
					return View();
				}

				var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
				if (user != null)
				{
					// Sinh mã OTP hoặc tạo token để đặt lại mật khẩu
					string token = Guid.NewGuid().ToString(); // Ví dụ đơn giản về token
					await _cache.SetStringAsync($"PasswordResetToken:{email}", token, new DistributedCacheEntryOptions
					{
						AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Thời gian hết hạn token
					});

					var resetLink = Url.Action("ResetPassword", "Users", new { token = token, email = email }, Request.Scheme);

					await _emailService.SendEmail(email, "Password Reset Request", resetLink); // Phương thức gửi email với liên kết đặt lại mật khẩu

					TempData["Message"] = "Password reset link has been sent to your email.";
				}
			}

			catch (Exception ex)
			{
				ModelState.AddModelError("", ex.Message);
			}
			return View();
		}

		public async Task SendPasswordResetEmail(string email, string resetLink)
		{
			try
			{
				var subject = "Password Reset Request";
				var body = $"To reset your password, please click the following link: <a href='{resetLink}'>Reset Password</a>";

				await _emailService.SendEmail(email, subject, body);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending email: {ex.Message}");
			}
		}

		[HttpGet]
		public IActionResult ResetPassword(string token, string email)
		{
			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
			{
				return BadRequest("Invalid request.");
			}

			// Tạo mô hình và gán giá trị token và email
			var model = new ResetPasswordViewModel
			{
				Token = token,
				Email = email
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// Kiểm tra token từ cache
			var cachedToken = await _cache.GetStringAsync($"PasswordResetToken:{model.Email}");
			if (cachedToken == null || cachedToken != model.Token)
			{
				ModelState.AddModelError("", "Invalid or expired token.");
				return View(model);
			}

			// Lấy người dùng từ cơ sở dữ liệu
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
			if (user == null)
			{
				ModelState.AddModelError("", "User not found.");
				return View(model);
			}

			// Kiểm tra mật khẩu mới và xác nhận mật khẩu
			if (model.NewPassword != model.ConfirmPassword)
			{
				ModelState.AddModelError("", "Passwords do not match.");
				return View(model);
			}

			// Mã hóa mật khẩu mới
			var hashedNewPassword = _passwordHasher.HashPassword(user, model.NewPassword);

			// Cập nhật mật khẩu của người dùng
			user.Password = hashedNewPassword;
			try
			{
				// Cập nhật mật khẩu của người dùng
				user.Password = hashedNewPassword;
				_context.Users.Update(user);
				await _context.SaveChangesAsync();

				// Xác minh xem mật khẩu đã được cập nhật
				var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
				if (updatedUser == null || !VerifyPassword(updatedUser, model.NewPassword))
				{
					throw new Exception("Password was not updated correctly in the database.");
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "An error occurred while updating the password: " + ex.Message);
				return View(model);
			}

			await _cache.RemoveAsync($"User:{model.Email}");

			TempData["Message"] = "Your password has been reset successfully.";
			return RedirectToAction("Login");
		}


		public async Task<IActionResult> Logout()
		{
			HttpContext.Session.Remove("UserId");
			HttpContext.Session.Remove("Username");
			HttpContext.Session.Remove("UserImagePath");
			return Redirect("~/Home/Index");
		}

	}
}

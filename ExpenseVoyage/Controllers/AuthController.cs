using ExpenseVoyage.Models;
using ExpenseVoyage.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace ExpenseVoyage.Controllers
{

    public class AuthController : Controller
    {
        private readonly IDistributedCache _cache;
        private readonly IPasswordHasher<Users> _passwordHasher;
        private readonly EmailService _emailService;
        private readonly EmailSetting _emailSettings;
        private readonly DatabaseContext _context;

        public AuthController(IDistributedCache cache, IPasswordHasher<Users> passwordHasher, EmailService emailService, IOptions<EmailSetting> emailSettings, DatabaseContext context)
        {
            _cache = cache;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _context = context;
        }



        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string loginType)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Email and Password are required.";
                return RedirectToAction("Login", new { loginType = loginType });
            }

            // Người dùng chưa có trong cache, lấy từ cơ sở dữ liệu
            Users user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                if (loginType == "admin")
                {
                    TempData["ErrorMessageLogin"] = "Invalid Email";
                    return RedirectToAction("Login", "Admin");
                }
                else
                {
                    TempData["ErrorMessageLogin"] = "Invalid Email";
                    return RedirectToAction("Login", "Users");
                }
            }
            else
            {
                //Kiểm tra email đã được xác nhận chưa
                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Please confirm your email before logging in.");
                    return View();
                }
                //Kiểm tra mật khẩu
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("", "Invalid login attempt. Please check your credentials.");

                    if (loginType == "admin")
                    {
                        TempData["ErrorMessageLogin"] = "Invalid Password";
                        return RedirectToAction("Login", "Admin");
                    }
                    else
                    {
                        TempData["ErrorMessageLogin"] = "Invalid Password";
                        return RedirectToAction("Login", "Users");
                    }

                }
                else
                {

                    if (!string.IsNullOrEmpty(user.ImagePath))
                    {
                        HttpContext.Session.SetString("AdminImagePath", user.ImagePath);
                    }
                    if (user.Role == "User")
                    {
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        if (!string.IsNullOrEmpty(user.ImagePath))
                        {
                            HttpContext.Session.SetString("UserImagePath", user.ImagePath);
                        }
                        return Redirect("~/Home/Index");

                    }
                    else if (user.Role == "Admin")
                    {
                        HttpContext.Session.SetInt32("AdminId", user.UserId);
                        HttpContext.Session.SetString("Adminname", user.Username);
                        HttpContext.Session.SetString("AdminEmail", user.Email);
                        return Redirect("~/admin");
                    }
                }
                return RedirectToAction("Login");

            }
        }





    }
}

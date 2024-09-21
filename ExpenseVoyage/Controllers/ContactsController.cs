using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseVoyage.Models;
using System.Net.Mail;
using System.Net;

namespace ExpenseVoyage.Controllers
{
	public class ContactsController : Controller
	{
		private readonly DatabaseContext _context;

		public ContactsController(DatabaseContext context)
		{
			_context = context;
		}

		// GET: Contacts
		public async Task<IActionResult> Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> SendEmail(Contacts contact)
		{
			if (ModelState.IsValid)
			{
				var subject = "New Contact Form Submission";
				var body = $"You have received a new message from {contact.Name} ({contact.Email}).\n\n" +
						   $"Phone: {contact.Phone}\n\n" +
						   $"Message:\n{contact.Message}";

				var emailSent = await SendEmailAsync("linhnqt1s2303008@fpt.edu.vn", subject, body);

				if (emailSent)
				{
					TempData["Message"] = "Your message has been sent successfully.";
					return RedirectToAction("Index"); // Redirect to a confirmation page
				}
				else
				{
					TempData["Message"] = "There was an error sending your message. Please try again.";
					return RedirectToAction("Index"); // Redirect back to the contact page
				}
			}

			return View("Index", contact);
		}

		private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
		{
			try
			{
				var smtpClient = new SmtpClient("smtp.gmail.com")
				{
					Port = 587,
					Credentials = new NetworkCredential("linhnqt1s2303008@fpt.edu.vn", "pxaj ubqv sxqq waam"),
					EnableSsl = true,
				};

				var mailMessage = new MailMessage
				{
					From = new MailAddress("linhnqt1s2303008@fpt.edu.vn", "ExpenseVoyage Contact"),
					Subject = subject,
					Body = body,
					IsBodyHtml = false,
				};

				mailMessage.To.Add(toEmail);

				await smtpClient.SendMailAsync(mailMessage);

				return true;
			}
			catch (Exception ex)
			{
				// Log lỗi nếu cần thiết
				Console.WriteLine(ex.Message);
				return false;
			}
		}

	}
}

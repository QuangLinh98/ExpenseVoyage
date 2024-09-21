using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace ExpenseVoyage.Service
{
	public class EmailService
	{
		private readonly EmailSetting _emailSetting;

		public EmailService(IOptions<EmailSetting> emailSetting)
		{
			_emailSetting = emailSetting.Value;   // Vì trong program mình không có cấu hình option nên cần sử dung IOption và .Value để lấy giá trị 
		}

		public async Task SendEmail(string toEmail, string subject, string HtmlContent)
		{
			var fromAddress = new MailAddress(_emailSetting.FromEmail, "E-Smart");
			var toAddress = new MailAddress(toEmail);

			var smtp = new SmtpClient
			{
				Host = _emailSetting.Host,
				Port = _emailSetting.Port,
				EnableSsl = _emailSetting.EnableSsl,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				Credentials = new NetworkCredential(_emailSetting.FromEmail, _emailSetting.FromPassword)
			};

			using var message = new MailMessage(fromAddress, toAddress)
			{
				Subject = subject,
				Body = HtmlContent,
				IsBodyHtml = true // Đánh dấu nội dung email là HTML
			};
			await smtp.SendMailAsync(message);
		}

		public async Task SendOtpEmail(string email, string otp)
		{

			var subject = "Your OTP Code";
			var body = $"<h1>OTP Verification</h1><p>Your OTP code is:</p><h2>{otp}</h2><p>Please use this code to complete your verification. The code is valid for 10 minutes.</p>";

			// Call the SendEmail method from EmailService to send the confirmation email
			await SendEmail(email, subject, body);
		}
		
	}
}

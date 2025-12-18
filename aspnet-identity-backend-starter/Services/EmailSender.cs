using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TextingService.Settings;

namespace TextingService.Services
{
	public class EmailSender : IEmailSender
	{
		private readonly ILogger _logger;
		private readonly EmailSenderOptions _emailSenderOptions;

		public EmailSender(IOptions<EmailSenderOptions> emailSenderOptions, ILogger<EmailSender> logger)
		{
			_emailSenderOptions = emailSenderOptions.Value;
			_logger = logger;
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			if(string.IsNullOrEmpty(_emailSenderOptions.SendGridApiKey))
				throw new Exception("Null SendGrid Api Key");

			if(string.IsNullOrEmpty(_emailSenderOptions.FromEmail))
				throw new Exception("Null sender email");

			if(string.IsNullOrEmpty(_emailSenderOptions.FromName))
				throw new Exception("Null from name");

			await Execute(_emailSenderOptions.SendGridApiKey, subject, htmlMessage, email);
		}

		private async Task Execute(string apiKey, string subject, string message, string toEmail)
		{
			var client = new SendGridClient(apiKey);
			var from = new EmailAddress(_emailSenderOptions.FromEmail, _emailSenderOptions.FromName);
			var to = new EmailAddress(toEmail);
			var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);

			// Disable click tracking.
			// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
			msg.SetClickTracking(false, false);

			var response = await client.SendEmailAsync(msg);

			_logger.LogInformation(response.IsSuccessStatusCode
                ? $"Email to {toEmail} queued successfully!"
                : $"Failure Email to {toEmail}");
		}
	}
}

namespace TextingService.Settings
{
	public class EmailSenderOptions
	{
		public string? SendGridApiKey { get; set; }
		public string? FromEmail { get; set; }
		public string? FromName { get; set; }
	}
}

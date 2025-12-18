namespace TextingService.Data.Requests
{
	public class ResetPasswordConfirmRequest
	{
		public required string Token { get; set; }
		public required string Email { get; set; }
		public required string NewPassword { get; set; }
	}
}

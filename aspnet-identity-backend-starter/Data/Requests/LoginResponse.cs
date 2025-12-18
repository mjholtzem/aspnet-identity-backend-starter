namespace TextingService.Data.Requests;

public class LoginResponse
{
	public string Token { get; set; }

	/// <summary>
	/// Time (in seconds) before the token expires
	/// </summary>
	public int ExpiresIn { get; set; }
}

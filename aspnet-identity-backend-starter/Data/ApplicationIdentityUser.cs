using Microsoft.AspNetCore.Identity;

namespace TextingService.Data
{
	/// <summary>
	/// "User" data class for a user within the Identity Framework (distinct from UserProfile which is a user in the App context)
	/// </summary>
	public class ApplicationIdentityUser : IdentityUser<Guid>
	{
		public UserProfile UserProfile { get; set; } = default!;
	}
}

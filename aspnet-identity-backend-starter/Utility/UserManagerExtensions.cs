using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TextingService.Data;

namespace TextingService.Utility
{
	public static class UserManagerExtensions
	{
		public static async Task<ApplicationIdentityUser?> GetUserWithProfileAsync(this UserManager<ApplicationIdentityUser> userManager, AppDbContext db, ClaimsPrincipal user)
		{
			var idString = userManager.GetUserId(user);
			if(string.IsNullOrEmpty(idString)) return null;

			var id = Guid.Parse(idString);

			var identityUser = await db.Users
				.Include(identity => identity.UserProfile)
				.FirstOrDefaultAsync(u => u.Id == id);

			return identityUser;
		}
	}
}

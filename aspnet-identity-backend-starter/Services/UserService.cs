using Microsoft.EntityFrameworkCore;
using TextingService.Data;

namespace TextingService.Services
{
	public class UserService
	{
		private readonly AppDbContext _db;

		public UserService(AppDbContext db)
		{
			_db = db;
		}

		public async Task<UserProfile> CreateUserProfile(ApplicationIdentityUser identityUser, string displayName, string email)
		{
			var userProfile = new UserProfile
			{
				UserName = displayName,
				IdentityUserId = identityUser.Id,
				IdentityUser = identityUser,
				CreatedAt = DateTime.UtcNow
			};

			_db.Profiles.Add(userProfile);
			await _db.SaveChangesAsync();

			return userProfile;
		}
	}
}

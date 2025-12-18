using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TextingService.Data;
using TextingService.Data.Requests;
using TextingService.Services;
using TextingService.Utility;

namespace TextingService.Controllers
{
	/// <summary>
	/// Controller responsible for user realted endpoints. Particularly things
	/// associated with UserProfile which is the application specific user data (not related to Authentication)
	/// </summary>
	[ApiController]
	[Route("[controller]")]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly AppDbContext _db;
		private readonly UserService _userService;
		private readonly UserManager<ApplicationIdentityUser> _userManager;

		public UserController(AppDbContext db, UserService userService, UserManager<ApplicationIdentityUser> userManager)
		{
			_db = db;
			_userService = userService;
			_userManager = userManager;
		}

		[HttpPost("getOrCreate")]
		public async Task<IActionResult> GetOrCreate()
		{
			var identityUser = await _userManager.GetUserWithProfileAsync(_db, User);
			if(identityUser == null)
				return NotFound("Identity user not found from token");

			var userProfile = identityUser.UserProfile;

			if(userProfile != null)
				return Ok(userProfile);

			var displayName = identityUser.UserName;
			var email = identityUser.Email;

			userProfile = await _userService.CreateUserProfile(identityUser, displayName!, email!);
			return Ok(userProfile);
		}

		[HttpDelete("delete")]
		public async Task<IActionResult> DeleteUser()
		{
			var identity = await _userManager.GetUserWithProfileAsync(_db, User);
			if(identity == null)
				return NotFound("Identity user not found from token");

			if(identity.UserProfile == null)
				return NotFound("No User profile found");

			_db.Profiles.Remove(identity.UserProfile);
			await _db.SaveChangesAsync();

			return Ok();
		}

		[HttpPatch("changeUserName")]
		public async Task<IActionResult> ChangeUserName([FromBody] ChangeUserNameRequest request)
		{
			var identity = await _userManager.GetUserWithProfileAsync(_db, User);
			if(identity == null)
				return NotFound("Identity user not found from token");

			var profile = identity.UserProfile;
			if(profile == null)
				return NotFound("No User profile found");

			profile.UserName = request.UserName;
			var result = await _db.SaveChangesAsync();
			if(result == 0)
				return Problem("Failed to update UserName");

			return Ok("Username Updated!");
		}
	}
}

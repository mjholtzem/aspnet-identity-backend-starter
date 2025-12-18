using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TextingService.Utility
{
	public static class ClaimsExtensions
	{
		public static bool IsEmailVerified(this ClaimsPrincipal user)
		{
			return user.FindFirstValue("email_verified")?.ToLower() == "true";
		}

		public static string GetEmail(this ClaimsPrincipal user)
		{
			return user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
		}

		public static string GetDisplayName(this ClaimsPrincipal user)
		{
			return user.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
		}
	}
}

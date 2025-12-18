using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TextingService.Settings;

namespace TextingService.Services
{
	/// <summary>
	/// Genrates JWT tokens using configured settings and user information
	/// </summary>
	public class TokenService
	{
		private readonly JwtSettings _jwtSettings;

		public TokenService(IOptions<JwtSettings> jwtSettings)
		{
			_jwtSettings = jwtSettings.Value;
		}

		public string GenerateToken(Guid userId, string username, string email, List<string> roles, bool emailIsVerified)
		{
			//Key that is used by both the token generator and validator to ensure that the token is valid
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

			//defines how the token will be signed (key + hashing algorithm)
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			//generate data payload for the token
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, userId.ToString()),
				new(JwtRegisteredClaimNames.Name, username),
				new(JwtRegisteredClaimNames.Email, email),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //unique Id for this particular token
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), //timstamp of token creation
                new("email_verified", emailIsVerified ? "true" : "false")
			};

			//Add roles - this uses ClaimTypes.Role rather than JwtRegisteredClaimNames because "role" is outside of the normal Jwt spec
			foreach(var role in roles) claims.Add(new(ClaimTypes.Role, role));

			//contains token creation parameters
			var descriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
				Issuer = _jwtSettings.Issuer,
				Audience = _jwtSettings.Audience,
				SigningCredentials = credentials,
			};

			var handler = new JwtSecurityTokenHandler();
			var token = handler.CreateToken(descriptor);

			//return serialized token
			return handler.WriteToken(token);
		}
	}
}

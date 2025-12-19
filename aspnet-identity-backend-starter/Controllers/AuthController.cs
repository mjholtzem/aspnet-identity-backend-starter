using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TextingService.Data;
using TextingService.Data.Requests;
using TextingService.Services;
using TextingService.Settings;
using TextingService.Utility;

namespace TextingService.Controllers;

/// <summary>
/// Handles all authentication related endpoints (register, login, email confirmation, password reset, etc)
/// Uses the EntityCoreFramework and CoreIdenity frameworks for user management
/// 
/// This is meant to be a good starting point for a back-end but some improvements will be necessary for production.
/// particuarly around the password reset flow which should really utilize a front-end form to hit the endpoint rather than
/// including a token in the email content
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
	private readonly TokenService _tokenService;
	private readonly JwtSettings _jwtSettings;
	private readonly UserManager<ApplicationIdentityUser> _userManager;
	private readonly IEmailSender _emailSender;

	public AuthController(TokenService tokenService,
		IEmailSender emailSender,
		IOptions<JwtSettings> jwtSettings,
		UserManager<ApplicationIdentityUser> userManager)
	{
		_tokenService = tokenService;
		_emailSender = emailSender;
		_jwtSettings = jwtSettings.Value;
		_userManager = userManager;
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		var identityUser = new ApplicationIdentityUser
		{
			UserName = request.Username,
			Email = request.Email
		};

		//Create Identity User
		var result = await _userManager.CreateAsync(identityUser, request.Password);

		if(!result.Succeeded)
			return BadRequest(result.Errors);

		await SendConfirmationEmailAsync(identityUser, identityUser.Email);

		return Ok("Registration was successful. Please verify your email.");
	}

	[HttpGet("confirmEmail")]
	public async Task<IActionResult> ConfirmEmail(string userId, string token)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if(user == null)
			return BadRequest("Invalid user.");

		var result = await _userManager.ConfirmEmailAsync(user, token);
		if(!result.Succeeded)
			return BadRequest("Invalid or expired token.");

		return Ok("Email confirmed successfully.");
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		var identityUser = await _userManager.FindByEmailAsync(request.Email);
		if(identityUser == null) return Unauthorized("Invalid credentials");

		var passwordIsValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
		if(!passwordIsValid) return Unauthorized("Invalid credentials");

		var roles = new List<string> { "User" };
		var token = _tokenService.GenerateToken(identityUser.Id, identityUser.UserName!, identityUser.Email!, new List<string>(), identityUser.EmailConfirmed);

		return Ok(new LoginResponse
		{
			Token = token,
			ExpiresIn = _jwtSettings.ExpirationMinutes * 60
		});
	}

	[HttpPost("resendConfirmation")]
	[Authorize]
	public async Task<IActionResult> ResendConfirmation()
	{
		var email = User.GetEmail();
		if(string.IsNullOrEmpty(email))
			return BadRequest("Email not found in token.");

		var identityUser = await _userManager.GetUserAsync(User);
		if(identityUser == null)
			return BadRequest("User not found.");

		await SendConfirmationEmailAsync(identityUser, email);

		return Ok("Confirmation email re-sent!");
	}

	[HttpPost("changeEmail")]
	[Authorize]
	public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
	{
		var identityUser = await _userManager.GetUserAsync(User);
		if(identityUser == null)
			return NotFound();

		var token = await _userManager.GenerateChangeEmailTokenAsync(identityUser, request.Email);

		//Link to confirmation endpoint that will be included in the confirmation email
		var confirmationLink = Url.Action(
			"confirmEmailChange",
			"Auth",
			new { userId = identityUser.Id, request.Email, token },
			Request.Scheme);

		//Send email
		await _emailSender.SendEmailAsync(request.Email, "Confirm your email change",
			$"Please confirm your new email by clicking this link: <a href='{confirmationLink}'>Confirmation Link</a>");

		return Ok("A confirmation link has been send to your email!");
	}

	[HttpGet("confirmEmailChange")]
	public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if(user == null)
			return NotFound();

		var result = await _userManager.ChangeEmailAsync(user, email, token);

		if(!result.Succeeded)
			return BadRequest(result.Errors);

		return Ok("Email changed successfully.");
	}

	[HttpPost("resetPassword")]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
	{
		var identityUser = await _userManager.FindByEmailAsync(request.Email);
		if(identityUser == null)
			return NotFound();

		var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);

		//Link to confirmation endpoint that will be included in the confirmation email
		var passwordResetLink = "https://google.com"; //obviously not a real link.You would want to send a link to a real front-end here

		//Send email
		await _emailSender.SendEmailAsync(request.Email, "Password Reset Request",
			$"Please follow this link to : <a href='{passwordResetLink}'>reset your password (not a real link)</a>. Token (for testing): {token}");

		return Ok("Password reset email sent!");
	}

	[HttpPost("confirmResetPassword")]
	public async Task<IActionResult> ConfirmResetPassword([FromBody] ResetPasswordConfirmRequest request)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);
		if(user == null)
			return NotFound();

		var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
		if(!result.Succeeded)
			return BadRequest(result.Errors);

		return Ok("Password has been reset");
	}

	private async Task SendConfirmationEmailAsync(ApplicationIdentityUser identityUser, string email)
	{
		//Generate token for confirmation email
		var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

		//Link to confirmation endpoint that will be included in the confirmation email
		var confirmationLink = Url.Action(
			"confirmEmail",
			"Auth",
			new { userId = identityUser.Id, token },
			Request.Scheme);

		//Send email
		await _emailSender.SendEmailAsync(identityUser.Email!, "Confirm your email",
			$"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>");
	}
}

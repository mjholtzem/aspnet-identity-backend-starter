using System.Text;
using TextingService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using TextingService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TextingService.Settings;
using TextingService.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();

//Email Sending
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddOptions<EmailSenderOptions>().Bind(builder.Configuration.GetSection(nameof(EmailSenderOptions)));

//Identity Framework
builder.Services
	.AddIdentityCore<ApplicationIdentityUser>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequiredLength = 8;
		options.Password.RequireUppercase = false;
		options.Password.RequireNonAlphanumeric = false;

		options.User.RequireUniqueEmail = true;
	})
	.AddRoles<IdentityRole<Guid>>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

//Jwt Generation
var jwtSettingsSection = builder.Configuration.GetSection(nameof(JwtSettings));
builder.Services.AddOptions<JwtSettings>().Bind(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings!.Secret);

//Add authentication services that are reponsible for processing the Jwts on incoming requests
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = true;
	options.SaveToken = true;

	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidateAudience = true,
		ValidAudience = jwtSettings.Audience,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

//Add authorization services that are responsible for determining if a user is allowed to access a resource (also just generally the [Authorize] attribute)
builder.Services.AddAuthorization(options =>
{
	//usage - add [Authorize(Policy = "ConfirmedEmailOnly")] to endpoint
	options.AddPolicy("ConfirmedEmailOnly", policy =>
	{
		policy.RequireClaim("email_verified", "true");
	});
});

//Database access via the Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlite("Data Source=app.db");
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

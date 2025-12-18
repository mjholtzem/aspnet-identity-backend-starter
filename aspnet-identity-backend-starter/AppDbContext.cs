using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TextingService.Data;
using Microsoft.AspNetCore.Identity;

namespace TextingService
{
	public class AppDbContext : IdentityDbContext<ApplicationIdentityUser, IdentityRole<Guid>, Guid>
	{
		public DbSet<UserProfile> Profiles { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<UserProfile>()
				.HasOne(profile => profile.IdentityUser)
				.WithOne(identity => identity.UserProfile)
				.HasForeignKey<UserProfile>(profile => profile.IdentityUserId)
				.IsRequired();
		}
	}
}

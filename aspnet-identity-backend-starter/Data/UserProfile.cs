using System.Text.Json.Serialization;

namespace TextingService.Data
{
	public class UserProfile
	{
		public int Id { get; private set; }

		public string? Email => IdentityUser?.Email;

		public required string UserName { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public Guid IdentityUserId { get; set; }

		[JsonIgnore]
		public ApplicationIdentityUser? IdentityUser { get; set; }
	}
}

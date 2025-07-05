using Microsoft.AspNetCore.Identity;
namespace SaGaMarket.Server.Identity
{
    public class SaGaMarketIdentityUser : IdentityUser<Guid>
    {
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

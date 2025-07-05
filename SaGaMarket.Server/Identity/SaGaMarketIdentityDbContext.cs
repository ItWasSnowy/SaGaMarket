
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;

namespace SaGaMarket.Identity;

public class SaGaMarketIdentityDbContext : IdentityDbContext<SaGaMarketIdentityUser, IdentityRole<Guid>, Guid>
{
    public SaGaMarketIdentityDbContext(DbContextOptions<SaGaMarketIdentityDbContext> options)
        : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Кастомизация схемы Identity при необходимости
    }
}
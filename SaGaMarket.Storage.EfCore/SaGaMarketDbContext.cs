using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Entities;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SaGaMarket.Infrastructure.Data
{
    public class SaGaMarketDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Variant> Variants { get; set; }

        public SaGaMarketDbContext(DbContextOptions<SaGaMarketDbContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne()
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.ProductsForSale)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne()
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithOne()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .UsingEntity(j => j.ToTable("ProductTags"));

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Products)
                .WithMany(p => p.Orders)
                .UsingEntity(j => j.ToTable("OrderProducts"));

            modelBuilder.Entity<Review>()
                .HasMany(r => r.Comments)
                .WithOne()
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PriceGraph>()
        .HasKey(pg => pg.VariantId); // Указываем, что VariantId является первичным ключом
            modelBuilder.Entity<Variant>()
        .HasOne(v => v.priceHistory) // Указываем, что Variant имеет один PriceGraph
        .WithOne(pg => pg.Variant) // Указываем, что PriceGraph ссылается на один Variant
        .HasForeignKey<PriceGraph>(pg => pg.VariantId) // Указываем внешний ключ
        .OnDelete(DeleteBehavior.Cascade); // Указываем поведение при удалении
        }
    }
}
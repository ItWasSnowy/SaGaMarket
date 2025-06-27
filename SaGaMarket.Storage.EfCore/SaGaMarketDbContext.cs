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

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Review)
                    .WithMany(r => r.Comments)
                    .HasForeignKey(c => c.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.TimeCreate)
                    .HasDefaultValueSql("NOW()");

                entity.Property(c => c.TimeLastUpdate)
                    .HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(u => u.Comments)
                    .WithOne(c => c.Author)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Reviews)
                    .WithOne(r => r.Author)
                    .HasForeignKey(r => r.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.ProductsForSale)
                    .WithOne(p => p.Seller)
                    .HasForeignKey(p => p.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasMany(p => p.Variants)
                    .WithOne(v => v.Product)
                    .HasForeignKey(v => v.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Reviews)
                    .WithOne(r => r.Product)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Tags)
                    .WithMany(t => t.Products)
                    .UsingEntity(j => j.ToTable("ProductTags"));

                entity.HasMany(p => p.Orders)
                    .WithMany(o => o.Products)
                    .UsingEntity(j => j.ToTable("OrderProducts"));
            });

            modelBuilder.Entity<Variant>(entity =>
            {
                entity.HasOne(v => v.PriceHistory)
                    .WithOne(pg => pg.Variant)
                    .HasForeignKey<PriceGraph>(pg => pg.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasMany(r => r.Comments)
                    .WithOne(c => c.Review)
                    .HasForeignKey(c => c.ReviewId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PriceGraph>()
                .HasKey(pg => pg.VariantId);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId);
            });

            // Конфигурация OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId);
                entity.HasOne(oi => oi.Variant)
                      .WithMany()
                      .HasForeignKey(oi => oi.VariantId);
            });
        }
    }
}
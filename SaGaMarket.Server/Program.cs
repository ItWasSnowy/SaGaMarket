using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Core.UseCases;
using SaGaMarket.Core.UseCases.CommentUseCases;
using SaGaMarket.Core.UseCases.ReviewUseCases;
using SaGaMarket.Core.UseCases.ProductUseCases;
using SaGaMarket.Core.UseCases.UserUseCases;
using SaGaMarket.Core.UseCases.VariantUseCases;
using SaGaMarket.Core.UseCases.TagUseCases;
using SaGaMarket.Core.UseCases.OrderUseCases;
using System.Text.Json.Serialization;
using SaGaMarket.Core.Services;
using SaGaMarket.Identity;
using SaGaMarket.Core.UseCases.Tags;
using SaGaMarket.Server.Identity;
using SaGaMarket.Storage.EfCore.Repository;
using TourGuide.Core.UseCases.TagUseCases;
using SaGaMarket.Storage.EfCore;
using SaGaMarket.Core.UseCases.CartUseCases;


namespace SaGaMarket.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Configure databases
            builder.Services.AddDbContext<SaGaMarketDbContext>(o =>
                o.UseNpgsql("Host=localhost;Port=5432;Database=SaGaMarket;Username=postgres;Password=123"));

            builder.Services.AddDbContext<SaGaMarketIdentityDbContext>(o =>
                o.UseNpgsql("Host=localhost;Port=5432;Database=SaGaMarketIdentity;Username=postgres;Password=123"));

            // Configure Identity with roles
            builder.Services.AddIdentity<SaGaMarketIdentityUser, IdentityRole<Guid>>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<SaGaMarketIdentityDbContext>()
                .AddDefaultTokenProviders();

            // Configure authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.LoginPath = "/api/Account/Login";
                    options.LogoutPath = "/api/Account/Logout";
                });

            // Configure authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomerOnly", policy => policy.RequireRole("customer"));
                options.AddPolicy("SellerOnly", policy => policy.RequireRole("seller"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
            });

            // Register application services
            builder.Services.AddScoped<CreateCommentUseCase>();
            builder.Services.AddScoped<DeleteCommentUseCase>();
            builder.Services.AddScoped<GetCommentUseCase>();
            builder.Services.AddScoped<GetCommentsByAuthorUseCase>();
            builder.Services.AddScoped<GetCommentsByReviewUseCase>();
            builder.Services.AddScoped<UpdateCommentUseCase>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();

            builder.Services.AddScoped<CreateReviewUseCase>();
            builder.Services.AddScoped<DeleteReviewUseCase>();
            builder.Services.AddScoped<GetReviewUseCase>();
            builder.Services.AddScoped<GetAllReviewsOfOneProductUseCase>();
            builder.Services.AddScoped<UpdateReviewUseCase>();
            builder.Services.AddScoped<GetProductsRatingsUseCase>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

            builder.Services.AddScoped<CreateProductUseCase>();
            builder.Services.AddScoped<DeleteProductUseCase>();
            builder.Services.AddScoped<GetProductUseCase>();
            builder.Services.AddScoped<UpdateProductUseCase>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddScoped<CreateUserUseCase>();
            builder.Services.AddScoped<DeleteUserUseCase>();
            builder.Services.AddScoped<GetUserUseCase>();
            builder.Services.AddScoped<UpdateUserUseCase>();
            builder.Services.AddScoped<GetByEmailUserUseCase>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<CreateVariantUseCase>();
            builder.Services.AddScoped<DeleteVariantUseCase>();
            builder.Services.AddScoped<GetVariantUseCase>();
            builder.Services.AddScoped<UpdateVariantUseCase>();
            builder.Services.AddScoped<UpdateCountVariantUseCase>();
            builder.Services.AddScoped<GetAllVariantsOfOneProductUseCase>();
            builder.Services.AddScoped<IVariantRepository, VariantRepository>();

            builder.Services.AddScoped<TagCreateUseCase>();
            builder.Services.AddScoped<DeleteTagUseCase>();
            builder.Services.AddScoped<GetTagUseCase>();
            builder.Services.AddScoped<AddTagToProductUseCase>();
            builder.Services.AddScoped<RemoveTagFromProductUseCase>();
            builder.Services.AddScoped<GetAllTagsByProductUseCase>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();

            builder.Services.AddScoped<CreateOrderUseCase>();
            builder.Services.AddScoped<DeleteOrderUseCase>();
            builder.Services.AddScoped<GetOrderUseCase>();
            builder.Services.AddScoped<UpdateOrderUseCase>();
            builder.Services.AddScoped<AddVariantToOrderFromCartUseCase>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            builder.Services.AddScoped<IUserRoleService, UserRepository>();
            builder.Services.AddScoped<GetUserRoleUseCase>();
            builder.Services.AddScoped<GetUserCartUseCase>();
            builder.Services.AddScoped<AddToCartUseCase>();
            builder.Services.AddScoped<RemoveFromCartUseCase>();
            builder.Services.AddScoped<GetCartItemsInfoUseCase>();
            builder.Services.AddScoped<AddToFavoritesUseCase>();
            builder.Services.AddScoped<GetAllOrdersOneUserUseCase>();
            builder.Services.AddScoped<RemoveFromFavoritesUseCase>();
            builder.Services.AddScoped<GetProductWithPagination>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IFavoritesRepository, FavoritesRepository>();
            builder.Services.AddScoped<GetUserFavoritesUseCase>();
            builder.Services.AddScoped<GetProductsInfoUseCase>();
            builder.Services.AddScoped<ClearCartUseCase>();
            builder.Services.AddScoped<ProductRepository>();
            //------------------------------
            builder.Services.AddHttpClient(); // Äëÿ IHttpClientFactory
            builder.Services.AddScoped<IPaymentService, YooKassaPaymentService>();
            builder.Services.AddScoped<YooKassaPaymentService>();
            

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("X-Total-Count");
                });
            });

            // Configure JSON serialization
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseStaticFiles();

            // Initialize roles and admin user
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SaGaMarketIdentityUser>>();
                var context = scope.ServiceProvider.GetRequiredService<SaGaMarketDbContext>();

                // Create roles
                foreach (var roleName in new[] { "customer", "seller", "admin" })
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    }
                }

                // Create default admin user
                var adminEmail = "admin@example.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new SaGaMarketIdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    //var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    //if (result.Succeeded)
                    //{
                    //    await userManager.AddToRoleAsync(adminUser, "admin");

                    //    // Create corresponding user in main database
                    //    context.Users.Add(new Core.Entities.User
                    //    {
                    //        UserId = adminUser.Id,
                    //        Role = Core.Entities.Role.admin
                    //    });
                    //    await context.SaveChangesAsync();
                    //}
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
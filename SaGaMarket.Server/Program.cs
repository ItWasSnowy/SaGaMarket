
using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Core.UseCases.CommentUseCases;
using SaGaMarket.Core.UseCases;
using SaGaMarket.Infrastructure.Data;
using SaGaMarket.Storage.EfCore.Repository;
using SaGaMarket.Core.UseCases.ReviewUseCases;
using SaGaMarket.Core.UseCases.ProductUseCases;
using SaGaMarket.Core.UseCases.UserUseCases;
using SaGaMarket.Core.UseCases.VariantUseCases;
using SaGaMarket.Core.UseCases.TagUseCases;
using TourGuide.Core.UseCases.TagUseCases;
using SaGaMarket.Core.UseCases.Tags;
using SaGaMarket.Core.UseCases.OrderUseCases;
using System.Text.Json.Serialization;
using SaGaMarket.Core.Services;

namespace SaGaMarket.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<SaGaMarketDbContext>(o =>
    o.UseNpgsql("Host=localhost;Port=5432;Database=SaGaMarket;Username=postgres;Password=123"));

            builder.Services.AddScoped<CreateCommentUseCase>();
            builder.Services.AddScoped<DeleteCommentUseCase>();
            builder.Services.AddScoped<GetCommentUseCase>();
            builder.Services.AddScoped<GetCommentsByAuthorUseCase>();
            builder.Services.AddScoped<GetCommentsByReviewUseCase>();
            builder.Services.AddScoped<UpdateCommentUseCase>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            //------------------------------
            builder.Services.AddScoped<CreateReviewUseCase>();
            builder.Services.AddScoped<DeleteReviewUseCase>();
            builder.Services.AddScoped<GetReviewUseCase>();
            builder.Services.AddScoped<UpdateReviewUseCase>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            //-------------------------------
            builder.Services.AddScoped<CreateProductUseCase>();
            builder.Services.AddScoped<DeleteProductUseCase>();
            builder.Services.AddScoped<GetProductUseCase>();
            builder.Services.AddScoped<UpdateProductUseCase>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            //-------------------------------
            builder.Services.AddScoped<CreateUserUseCase>();
            builder.Services.AddScoped<DeleteUserUseCase>();
            builder.Services.AddScoped<GetUserUseCase>();
            builder.Services.AddScoped<UpdateUserUseCase>();
            builder.Services.AddScoped<GetByEmailUserUseCase>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            //-------------------------------
            builder.Services.AddScoped<CreateVariantUseCase>();
            builder.Services.AddScoped<DeleteVariantUseCase>();
            builder.Services.AddScoped<GetVariantUseCase>();
            builder.Services.AddScoped<UpdateVariantUseCase>();
            builder.Services.AddScoped<UpdateCountVariantUseCase>();
            builder.Services.AddScoped<GetAllVariantsOfOneProductUseCase>();
            builder.Services.AddScoped<IVariantRepository, VariantRepository>();
            //-------------------------------
            builder.Services.AddScoped<TagCreateUseCase>();
            builder.Services.AddScoped<DeleteTagUseCase>();
            builder.Services.AddScoped<GetTagUseCase>();
            builder.Services.AddScoped<AddTagToProductUseCase>();
            builder.Services.AddScoped<RemoveTagFromProductUseCase>();
            builder.Services.AddScoped<GetAllTagsByProductUseCase>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            //------------------------------
            builder.Services.AddScoped<CreateOrderUseCase>();
            builder.Services.AddScoped<DeleteOrderUseCase>();
            builder.Services.AddScoped<GetOrderUseCase>();
            builder.Services.AddScoped<UpdateOrderUseCase>();
            builder.Services.AddScoped<AddVariantToOrderFromCartUseCase>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            //-------------------------------
            builder.Services.AddScoped<IUserRoleService, UserRepository>();
            builder.Services.AddScoped<GetUserRoleUseCase>();
            builder.Services.AddScoped<AddToCartUseCase>();
            builder.Services.AddScoped<RemoveFromCartUseCase>();
            builder.Services.AddScoped<AddToFavoritesUseCase>();
            builder.Services.AddScoped<RemoveFromFavoritesUseCase>();
            builder.Services.AddScoped<GetProductWithPagination>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            
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

            builder.Services.AddControllers()
     .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
         options.JsonSerializerOptions.WriteIndented = true;
     });



            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

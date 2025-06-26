
using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Core.UseCases.CommentUseCases;
using SaGaMarket.Core.UseCases;
using SaGaMarket.Infrastructure.Data;
using SaGaMarket.Storage.EfCore.Repository;
using SaGaMarket.Core.UseCases.ReviewUseCases;
using SaGaMarket.Core.UseCases.ProductUseCases;
using SaGaMarket.Core.UseCases.UserUseCases;

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
            //----------------------------
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


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

public class CreateProductUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;

    public CreateProductUseCase(IProductRepository productRepository, IUserRepository userRepository)
    {
        _productRepository = productRepository;
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(CreateProductRequest request, Guid sellerId)
    {
        var user = await _userRepository.Get(sellerId);

        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        if (user.Role == Role.customer)
        {
            throw new UnauthorizedAccessException("Only sellers and admins can create products");
        }

        var product = new Product
        {
            SellerId = sellerId,
            Category = request.Category,
            Name = request.Name,
            AverageRating = 0,
        };

        return await _productRepository.Create(product);
    }
    public class CreateProductRequest
    {
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;


    }
}


    


using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

public class GetUserRoleUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserRoleUseCase> _logger;

    public GetUserRoleUseCase(
        IUserRepository userRepository,
        ILogger<GetUserRoleUseCase> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserRoleInfoDto> Execute(Guid userId)
    {
        try
        {
            var role = await _userRepository.GetUserRoleAsync(userId);
            if (role == null)
                throw new ArgumentException("User not found");

            var isSellerWithCustomerFunctionality = role == Role.seller
                ? await _userRepository.IsSellerWithCustomerFunctionalityAsync(userId)
                : false;

            return new UserRoleInfoDto
            {
                Role = role.Value,
                CanPurchase = role == Role.customer || isSellerWithCustomerFunctionality,
                CanSell = role == Role.seller || role == Role.admin
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user role info for {UserId}", userId);
            throw;
        }
    }
}

public class UserRoleInfoDto
{
    public Role Role { get; set; }
    public bool CanPurchase { get; set; }
    public bool CanSell { get; set; }
    public bool IsAdmin => Role == Role.admin;
    public string RoleDescription => GetRoleDescription();

    private string GetRoleDescription()
    {
        return Role switch
        {
            Role.customer => "Customer",
            Role.seller => CanPurchase ? "Seller with purchasing" : "Seller",
            Role.admin => "Administrator",
            _ => "Unknown"
        };
    }
}
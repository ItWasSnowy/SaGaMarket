using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.VariantUseCases;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;
using System;
using System.Threading.Tasks;
using static SaGaMarket.Core.UseCases.VariantUseCases.CreateVariantUseCase;
using static SaGaMarket.Core.UseCases.VariantUseCases.UpdateCountVariantUseCase;
using static SaGaMarket.Core.UseCases.VariantUseCases.UpdateVariantUseCase;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VariantController : ControllerBase
{
    private readonly CreateVariantUseCase _createVariantUseCase;
    private readonly GetVariantUseCase _getVariantUseCase;
    private readonly UpdateVariantUseCase _updateVariantUseCase;
    private readonly DeleteVariantUseCase _deleteVariantUseCase;
    private readonly GetAllVariantsOfOneProductUseCase _getAllVariantsUseCase;
    private readonly UpdateCountVariantUseCase _updateCountVariantUseCase;
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;

    public VariantController(
        CreateVariantUseCase createVariantUseCase,
        GetVariantUseCase getVariantUseCase,
        UpdateVariantUseCase updateVariantUseCase,
        GetAllVariantsOfOneProductUseCase getAllVariantsUseCase,
        UpdateCountVariantUseCase updateCountVariantUseCase,
        DeleteVariantUseCase deleteVariantUseCase,
        UserManager<SaGaMarketIdentityUser> userManager)
    {
        _createVariantUseCase = createVariantUseCase;
        _getVariantUseCase = getVariantUseCase;
        _updateVariantUseCase = updateVariantUseCase;
        _deleteVariantUseCase = deleteVariantUseCase;
        _updateCountVariantUseCase = updateCountVariantUseCase;
        _getAllVariantsUseCase = getAllVariantsUseCase;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> Create([FromBody] CreateVariantRequest request)
    {
        try
        {
            var variantId = await _createVariantUseCase.Handle(request);
            return Ok(new { VariantId = variantId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var variant = await _getVariantUseCase.Handle(id);
            return variant == null
                ? NotFound(new { Error = "Variant not found" })
                : Ok(variant);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("products/{productId}/variants")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllVariants(Guid productId)
    {
        try
        {
            var variants = await _getAllVariantsUseCase.Handle(productId);
            return Ok(variants);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVariantRequest request)
    {
        try
        {
            await _updateVariantUseCase.Handle(id, request);
            return Ok(new { Message = "Variant updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPut("update-count/{id}")]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> UpdateCount(
        Guid id,
        [FromBody] UpdateCountVariantRequest request)
    {
        try
        {
            await _updateCountVariantUseCase.Handle(id, request);
            return Ok(new { Message = "Variant count updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "seller,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _deleteVariantUseCase.Handle(id);
            return Ok(new { Message = "Variant deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}
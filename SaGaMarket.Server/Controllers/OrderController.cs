using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.OrderUseCases;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly CreateOrderUseCase _createOrderUseCase;
        private readonly GetOrderUseCase _getOrderUseCase;
        private readonly UpdateOrderUseCase _updateOrderUseCase;
        private readonly DeleteOrderUseCase _deleteOrderUseCase;
        private readonly AddVariantToOrderFromCartUseCase _addVariantToOrderFromCartUseCase;

        public OrderController(
            AddVariantToOrderFromCartUseCase addVariantToOrderFromCartUseCase,
            CreateOrderUseCase createOrderUseCase,
            GetOrderUseCase getOrderUseCase,
            UpdateOrderUseCase updateOrderUseCase,
            DeleteOrderUseCase deleteOrderUseCase)
        {
            _addVariantToOrderFromCartUseCase = addVariantToOrderFromCartUseCase;
            _createOrderUseCase = createOrderUseCase;
            _getOrderUseCase = getOrderUseCase;
            _updateOrderUseCase = updateOrderUseCase;
            _deleteOrderUseCase = deleteOrderUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(
       [FromBody] CreateOrderUseCase.CreateOrderRequest request, [FromQuery] Guid customerId)
        {
            try
            {
 
                var orderId = await _createOrderUseCase.Handle(request, customerId);
                return Ok(orderId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            var order = await _getOrderUseCase.Execute(id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderUseCase.UpdateOrderRequest request, [FromQuery] Guid customerId)
        {
            if (request == null)
            {
                return BadRequest("Invalid order data.");
            }

            try
            {
                await _updateOrderUseCase.Handle(id, request, customerId);
                return NoContent(); // Успешное обновление
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // Доступ запрещен
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the order.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid customerId)
        {
            try
            {
                await _deleteOrderUseCase.Handle(id, customerId);
                return NoContent(); // Успешное удаление
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }

        [HttpPost("{orderId}/add-variant")]
        public async Task<IActionResult> AddVariantToOrder([FromBody] AddVariantToOrderFromCartUseCase.AddVariantToOrderRequest request, [FromRoute] Guid orderId)
        {
            if (request == null)
            {
                return BadRequest("Invalid order item data.");
            }
            var orderItemId = await _addVariantToOrderFromCartUseCase.Handle(request, orderId);
            return CreatedAtAction(nameof(GetOrderItem), new { id = orderItemId }, null);
        }
        [HttpGet]
        public IActionResult GetOrderItem(Guid id)
        {
            return Ok();
        }
    }
}

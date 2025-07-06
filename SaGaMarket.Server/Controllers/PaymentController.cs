
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            // Получаем userId из авторизованного пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return Unauthorized();
            }

            // Обновляем запрос с реальным userId
            request.UserId = userGuid;

            var result = await _paymentService.CreatePaymentAsync(request);
            return Ok(result);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> HandleCallback([FromBody] PaymentNotification notification)
        {
            await _paymentService.ProcessCallbackAsync(notification);
            return Ok();
        }
    }
}
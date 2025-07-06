using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Interfaces;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases
{
    public class YooKassaPaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<YooKassaPaymentService> _logger;
        private readonly string _shopId;
        private readonly string _secretKey;

        public YooKassaPaymentService(
     IConfiguration config,
     IOrderRepository orderRepository,
     IHttpClientFactory httpClientFactory,
     ILogger<YooKassaPaymentService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;

            // Получаем настройки
            _shopId = config["YooKassa:ShopId"]?.Trim();
            _secretKey = config["YooKassa:SecretKey"]?.Trim();

            if (string.IsNullOrEmpty(_shopId))
                throw new ArgumentNullException("YooKassa:ShopId not configured");
            if (string.IsNullOrEmpty(_secretKey))
                throw new ArgumentNullException("YooKassa:SecretKey not configured");

            // Настраиваем HttpClient
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://api.yookassa.ru/v3/");

            // Правильное формирование Basic Auth
            var authString = $"{_shopId}:{_secretKey}";
            var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64Auth);

            // Добавляем User-Agent (требование API ЮKassa)
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YourApp/1.0");
        }

        public async Task<PaymentCreationResult> CreatePaymentAsync(CreatePaymentRequest request)
        {
            try
            {
                // Проверка входных данных
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (request.Amount <= 0) throw new ArgumentException("Amount must be positive");
                if (string.IsNullOrEmpty(request.Description)) request.Description = "Оплата заказа";

                // Формируем корректный запрос для ЮKassa
                var paymentRequest = new
                {
                    amount = new
                    {
                        value = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                        currency = "RUB"
                    },
                    capture = true,
                    confirmation = new
                    {
                        type = "redirect",
                        return_url = request.ReturnUrl ?? "https://your-site.com/payment/complete"
                    },
                    description = request.Description,
                    metadata = new Dictionary<string, string>
                    {
                        ["orderId"] = request.OrderId.ToString(),
                        ["userId"] = request.UserId.ToString()
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(paymentRequest, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("payments", content);

                // Анализ ошибки если статус не успешный
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"YooKassa error: {response.StatusCode} - {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var paymentResponse = JsonSerializer.Deserialize<YooKassaPaymentResponse>(responseContent);

                return new PaymentCreationResult
                {
                    PaymentId = paymentResponse.Id,
                    ConfirmationUrl = paymentResponse.Confirmation?.ConfirmationUrl,
                    Status = MapStatus(paymentResponse.Status)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment creation failed");
                throw;
            }
        }

        public async Task ProcessCallbackAsync(PaymentNotification notification)
        {
            try
            {
                _logger.LogInformation("Processing webhook event: {Event}", notification.Event);

                if (notification.Object?.Metadata == null ||
                    !notification.Object.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                    !Guid.TryParse(orderIdStr, out var orderId))
                {
                    _logger.LogError("Failed to get orderId from metadata");
                    return;
                }

                var order = await _orderRepository.Get(orderId);
                if (order == null)
                {
                    _logger.LogError("Order {OrderId} not found", orderId);
                    return;
                }

                switch (notification.Event)
                {
                    case "payment.waiting_for_capture":
                        await CapturePaymentAsync(notification.Object.Id);
                        break;

                    case "payment.succeeded":
                        order.orderStatus = OrderStatus.Confirmed;
                        foreach (var item in order.OrderItems)
                        {
                            item.OrderStatus = OrderItemStatus.Confirmed;
                        }
                        await _orderRepository.Update(order);
                        _logger.LogInformation("Order {OrderId} confirmed", orderId);
                        break;

                    case "payment.canceled":
                        order.orderStatus = OrderStatus.Cancelled;
                        foreach (var item in order.OrderItems)
                        {
                            item.OrderStatus = OrderItemStatus.Cancelled;
                        }
                        await _orderRepository.Update(order);
                        _logger.LogInformation("Order {OrderId} canceled", orderId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                throw;
            }
        }

        public async Task<PaymentStatus> CheckPaymentStatusAsync(string paymentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"payments/{paymentId}");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var paymentResponse = JsonSerializer.Deserialize<YooKassaPaymentResponse>(responseContent);

                return MapStatus(paymentResponse.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment status");
                throw;
            }
        }

        public async Task<bool> RefundPaymentAsync(string paymentId, decimal amount)
        {
            try
            {
                var refundRequest = new
                {
                    payment_id = paymentId,
                    amount = new
                    {
                        value = amount.ToString("0.00"),
                        currency = "RUB"
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refundRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("refunds", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                throw;
            }
        }

        private async Task CapturePaymentAsync(string paymentId)
        {
            try
            {
                var captureRequest = new
                {
                    amount = new
                    {
                        value = "100.00", // Should be actual amount from order
                        currency = "RUB"
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(captureRequest),
                    Encoding.UTF8,
                    "application/json");

                await _httpClient.PostAsync($"payments/{paymentId}/capture", content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error capturing payment");
                throw;
            }
        }

        private PaymentStatus MapStatus(string status)
        {
            return status switch
            {
                "pending" => PaymentStatus.Pending,
                "waiting_for_capture" => PaymentStatus.Pending,
                "succeeded" => PaymentStatus.Succeeded,
                "canceled" => PaymentStatus.Failed,
                _ => PaymentStatus.Failed
            };
        }

        private class YooKassaPaymentResponse
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public YooKassaConfirmation Confirmation { get; set; }
        }

        private class YooKassaConfirmation
        {
            public string ConfirmationUrl { get; set; }
        }
    }

    public interface IPaymentService
    {
        Task<PaymentCreationResult> CreatePaymentAsync(CreatePaymentRequest request);
        Task ProcessCallbackAsync(PaymentNotification notification);
        Task<PaymentStatus> CheckPaymentStatusAsync(string paymentId);
        Task<bool> RefundPaymentAsync(string paymentId, decimal amount);
    }

    public class CreatePaymentRequest
    {
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string ReturnUrl { get; set; }
        public List<PaymentItem> Items { get; set; } = new();
    }

    public class PaymentItem
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class PaymentNotification
    {
        public string Event { get; set; }
        public PaymentObject Object { get; set; }
    }

    public class PaymentObject
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public bool Paid { get; set; }
        public decimal Amount { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class PaymentCreationResult
    {
        public string PaymentId { get; set; }
        public string ConfirmationUrl { get; set; }
        public PaymentStatus Status { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Succeeded,
        Failed
    }
}
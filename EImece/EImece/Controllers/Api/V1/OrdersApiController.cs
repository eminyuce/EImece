using EImece.Domain.Services.IServices;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace EImece.Controllers.Api.V1
{
    /// <summary>
    /// Order lookup and tracking endpoints.
    /// </summary>
    [RoutePrefix("api/v1/orders")]
    public class OrdersApiController : ApiController
    {
        private readonly IOrderService _orderService;

        public OrdersApiController(IOrderService orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        /// <summary>
        /// Gets order tracking and summary information by order number.
        /// </summary>
        [HttpGet]
        [Route("track/{orderNumber}")]
        public async Task<IHttpActionResult> TrackOrder(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                return BadRequest("Order number is required.");

            var order = await _orderService.GetStorefrontOrderByOrderNumberAsync(orderNumber.Trim());
            if (order == null)
                return NotFound();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.CreatedDate,
                order.OrderStatus,
                order.PaymentStatus,
                order.ShipmentCompanyName,
                order.ShipmentTrackingNumber,
                Total = order.PaidPriceDecimal,
                ItemCount = order.OrderProducts != null ? order.OrderProducts.Count : 0
            });
        }
    }
}

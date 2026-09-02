using EImece.Domain.Services.IServices;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace EImece.Controllers.Api.V1
{
    /// <summary>
    /// Shopping cart and coupon lookup endpoints.
    /// </summary>
    [RoutePrefix("api/v1/cart")]
    public class CartApiController : ApiController
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICouponService _couponService;

        public CartApiController(
            IShoppingCartService shoppingCartService,
            ICouponService couponService)
        {
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
            _couponService = couponService ?? throw new ArgumentNullException(nameof(couponService));
        }

        /// <summary>
        /// Gets shopping cart by order GUID.
        /// </summary>
        [HttpGet]
        [Route("{orderGuid}")]
        public async Task<IHttpActionResult> GetCart(string orderGuid)
        {
            if (string.IsNullOrWhiteSpace(orderGuid))
                return BadRequest("Order GUID is required.");

            var cart = await _shoppingCartService.GetShoppingCartByOrderGuidAsync(orderGuid);
            if (cart == null)
                return NotFound();

            return Ok(new
            {
                cart.Id,
                cart.OrderGuid,
                cart.CreatedDate,
                cart.UpdatedDate
            });
        }

        public class ValidateCouponRequest
        {
            public string Code { get; set; }
        }

        /// <summary>
        /// Validates a coupon code and returns discount details.
        /// </summary>
        [HttpPost]
        [Route("validate-coupon")]
        public async Task<IHttpActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Coupon code is required.");

            var coupon = await _couponService.GetStorefrontCouponByCodeAsync(request.Code.Trim(), 1);
            if (coupon == null || !coupon.IsActive)
            {
                return Ok(new { IsValid = false, Message = "Coupon not found or inactive." });
            }

            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate)
            {
                return Ok(new { IsValid = false, Message = "Coupon has expired or is not yet active." });
            }

            return Ok(new
            {
                IsValid = true,
                coupon.Code,
                coupon.Discount,
                coupon.DiscountPercentage,
                DiscountType = coupon.DiscountType.ToString(),
                coupon.IsFreeShipping,
                coupon.MinimumOrderAmount,
                coupon.MaximumDiscountAmount
            });
        }
    }
}

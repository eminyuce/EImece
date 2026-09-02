using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace EImece.Controllers.Api.V1
{
    /// <summary>
    /// Newsletter subscriber management endpoints.
    /// </summary>
    [RoutePrefix("api/v1/subscribers")]
    public class SubscribersApiController : ApiController
    {
        private readonly ISubscriberService _subscriberService;

        public SubscribersApiController(ISubscriberService subscriberService)
        {
            _subscriberService = subscriberService ?? throw new ArgumentNullException(nameof(subscriberService));
        }

        public class SubscribeRequest
        {
            public string Email { get; set; }
            public string Source { get; set; }
        }

        /// <summary>
        /// Subscribes an email address to the newsletter.
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            if (GeneralHelper.IsNotValidEmail(request.Email))
                return BadRequest("Invalid email format.");

            var exists = await _subscriberService.SubscriberExistsByEmailAsync(request.Email);
            if (!exists)
            {
                var subscriber = new Subscriber
                {
                    Name = request.Email,
                    Email = request.Email.Trim(),
                    Note = string.IsNullOrWhiteSpace(request.Source) ? "API-V1-Subscription" : request.Source.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Position = 1,
                    Lang = 1
                };
                await _subscriberService.SaveOrEditEntityAsync(subscriber);
            }

            return Ok(new { Success = true, Message = "Subscribed successfully." });
        }
    }
}

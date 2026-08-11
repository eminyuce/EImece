namespace EImece.Domain.Models.Payment
{
    /// <summary>
    /// Provider-neutral callback payload. For iyzico Checkout Form, the gateway POSTs
    /// <c>token</c> to the callback URL (same binding shape as Iyzipay RetrieveCheckoutFormRequest).
    /// </summary>
    public class PaymentCallbackRequest
    {
        /// <summary>
        /// Token returned by checkout initialize / posted back by the payment provider.
        /// </summary>
        public string Token { get; set; }

        public string ConversationId { get; set; }

        public string Locale { get; set; }
    }
}

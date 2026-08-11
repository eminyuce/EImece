namespace EImece.Domain.Models.Payment
{
    /// <summary>
    /// Provider-neutral result of starting a hosted checkout / payment form.
    /// </summary>
    public class PaymentInitializeResult
    {
        /// <summary>
        /// HTML/script content to render the provider's checkout form (e.g. iyzico CheckoutFormContent).
        /// </summary>
        public string CheckoutFormContent { get; set; }

        public string Token { get; set; }

        public string Status { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ConversationId { get; set; }

        public string BasketId { get; set; }

        public string PaymentPageUrl { get; set; }

        public string ProviderName { get; set; }
    }
}

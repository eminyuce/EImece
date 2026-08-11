using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    /// <summary>
    /// Strategy interface for interchangeable payment providers (Strategy pattern).
    /// </summary>
    public interface IPaymentStrategy
    {
        /// <summary>
        /// Stable provider key used for configuration selection (e.g. "Iyzico", "Stripe").
        /// </summary>
        string ProviderName { get; }

        Task<PaymentInitializeResult> InitializeCheckoutAsync(
            ShoppingCartSession cart,
            string userId,
            string callbackAction = "PaymentResult");

        Task<PaymentInitializeResult> InitializeBuyNowAsync(BuyNowModel model);

        Task<PaymentResult> RetrievePaymentResultAsync(string token);
    }
}

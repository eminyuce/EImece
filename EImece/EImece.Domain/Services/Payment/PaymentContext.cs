using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Services.IServices;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Services.Payment
{
    /// <summary>
    /// Strategy context: holds the current <see cref="IPaymentStrategy"/> and delegates payment operations.
    /// Clients (e.g. PaymentController) depend on this type rather than a concrete provider.
    /// </summary>
    public class PaymentContext
    {
        private IPaymentStrategy _strategy;

        public PaymentContext(IPaymentStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Currently linked strategy (provider).
        /// </summary>
        public IPaymentStrategy Strategy
        {
            get { return _strategy; }
        }

        public string ProviderName
        {
            get { return _strategy.ProviderName; }
        }

        /// <summary>
        /// Replace the strategy at runtime (e.g. tests or multi-tenant overrides).
        /// </summary>
        public virtual void SetStrategy(IPaymentStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        [Timed("service.payment_context.initialize_checkout")]
        public virtual Task<PaymentInitializeResult> InitializeCheckoutAsync(
            ShoppingCartSession cart,
            string userId,
            string callbackAction = "PaymentResult")
        {
            return _strategy.InitializeCheckoutAsync(cart, userId, callbackAction);
        }

        [Timed("service.payment_context.initialize_buy_now")]
        public virtual Task<PaymentInitializeResult> InitializeBuyNowAsync(BuyNowModel model)
        {
            return _strategy.InitializeBuyNowAsync(model);
        }

        [Timed("service.payment_context.retrieve_payment_result")]
        public virtual Task<PaymentResult> RetrievePaymentResultAsync(string token)
        {
            return _strategy.RetrievePaymentResultAsync(token);
        }
    }
}

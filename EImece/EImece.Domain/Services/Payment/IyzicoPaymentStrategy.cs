using EImece.Domain.Helpers;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Services.IServices;
using Iyzipay.Model;
using Iyzipay.Request;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Services.Payment
{
    /// <summary>
    /// Concrete iyzico Checkout Form strategy. Delegates entirely to <see cref="IyzicoService"/>
    /// (initialize + retrieve) so the live payment process stays unchanged; only SDK→DTO mapping is added.
    /// </summary>
    public class IyzicoPaymentStrategy : IPaymentStrategy
    {
        private readonly IyzicoService _iyzicoService;

        public IyzicoPaymentStrategy(IyzicoService iyzicoService)
        {
            _iyzicoService = iyzicoService ?? throw new ArgumentNullException(nameof(iyzicoService));
        }

        public string ProviderName
        {
            get { return "Iyzico"; }
        }

        [Timed("service.iyzico.initialize_checkout")]
        public virtual async Task<PaymentInitializeResult> InitializeCheckoutAsync(
            ShoppingCartSession cart,
            string userId,
            string callbackAction = "PaymentResult")
        {
            // Unchanged iyzico Checkout Form initialize path (callback URL, basket, installments, etc.).
            CheckoutFormInitialize sdkResult = await _iyzicoService
                .CreateCheckoutFormInitializeAsync(cart, userId, callbackAction)
                .ConfigureAwait(false);

            return MapInitialize(sdkResult);
        }

        [Timed("service.iyzico.initialize_buy_now")]
        public virtual async Task<PaymentInitializeResult> InitializeBuyNowAsync(BuyNowModel model)
        {
            CheckoutFormInitialize sdkResult = await _iyzicoService
                .CreateCheckoutFormInitializeBuyNowAsync(model)
                .ConfigureAwait(false);

            return MapInitialize(sdkResult);
        }

        [Timed("service.iyzico.retrieve_payment_result")]
        public virtual async Task<PaymentResult> RetrievePaymentResultAsync(string token)
        {
            // Same retrieve call as before: pass token through to IyzicoService (no extra validation).
            var request = new RetrieveCheckoutFormRequest { Token = token };
            CheckoutForm checkoutForm = await _iyzicoService
                .GetCheckoutFormAsync(request)
                .ConfigureAwait(false);

            return MapPaymentResult(checkoutForm);
        }

        private PaymentInitializeResult MapInitialize(CheckoutFormInitialize sdkResult)
        {
            if (sdkResult == null)
            {
                return null;
            }

            // CF Initialize returns token + form HTML / paymentPageUrl for the existing responsive form div.
            return new PaymentInitializeResult
            {
                CheckoutFormContent = sdkResult.CheckoutFormContent,
                Token = sdkResult.Token,
                Status = sdkResult.Status,
                ErrorCode = sdkResult.ErrorCode,
                ErrorMessage = sdkResult.ErrorMessage,
                ConversationId = sdkResult.ConversationId,
                PaymentPageUrl = sdkResult.PaymentPageUrl,
                ProviderName = ProviderName
            };
        }

        private PaymentResult MapPaymentResult(CheckoutForm checkoutForm)
        {
            if (checkoutForm == null)
            {
                return null;
            }

            // Field mapping mirrors the previous ShoppingCartService ← CheckoutForm assignments exactly
            // (including Installment via ToStr()) so order persistence stays bit-compatible.
            return new PaymentResult
            {
                Token = checkoutForm.Token,
                Price = checkoutForm.Price,
                PaidPrice = checkoutForm.PaidPrice,
                Installment = checkoutForm.Installment.HasValue
                    ? checkoutForm.Installment.Value.ToStr()
                    : string.Empty,
                Currency = checkoutForm.Currency,
                PaymentId = checkoutForm.PaymentId,
                PaymentStatus = checkoutForm.PaymentStatus,
                FraudStatus = checkoutForm.FraudStatus,
                MerchantCommissionRate = checkoutForm.MerchantCommissionRate,
                MerchantCommissionRateAmount = checkoutForm.MerchantCommissionRateAmount,
                IyziCommissionRateAmount = checkoutForm.IyziCommissionRateAmount,
                IyziCommissionFee = checkoutForm.IyziCommissionFee,
                CardType = checkoutForm.CardType,
                CardAssociation = checkoutForm.CardAssociation,
                CardFamily = checkoutForm.CardFamily,
                CardToken = checkoutForm.CardToken,
                CardUserKey = checkoutForm.CardUserKey,
                BinNumber = checkoutForm.BinNumber,
                LastFourDigits = checkoutForm.LastFourDigits,
                BasketId = checkoutForm.BasketId,
                ConversationId = checkoutForm.ConversationId,
                ConnectorName = checkoutForm.ConnectorName,
                AuthCode = checkoutForm.AuthCode,
                HostReference = checkoutForm.HostReference,
                Phase = checkoutForm.Phase,
                Status = checkoutForm.Status,
                ErrorCode = checkoutForm.ErrorCode,
                ErrorMessage = checkoutForm.ErrorMessage,
                Locale = checkoutForm.Locale,
                SystemTime = checkoutForm.SystemTime,
                ProviderName = ProviderName
            };
        }
    }
}

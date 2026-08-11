using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Services.IServices;
using Iyzipay.Model;
using Iyzipay.Request;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Services.Payment
{
    /// <summary>
    /// Concrete iyzico payment strategy. Wraps <see cref="IyzicoService"/> and maps SDK types to neutral DTOs.
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

        public async Task<PaymentInitializeResult> InitializeCheckoutAsync(
            ShoppingCartSession cart,
            string userId,
            string callbackAction = "PaymentResult")
        {
            CheckoutFormInitialize sdkResult = await _iyzicoService
                .CreateCheckoutFormInitializeAsync(cart, userId, callbackAction)
                .ConfigureAwait(false);

            return MapInitialize(sdkResult);
        }

        public async Task<PaymentInitializeResult> InitializeBuyNowAsync(BuyNowModel model)
        {
            CheckoutFormInitialize sdkResult = await _iyzicoService
                .CreateCheckoutFormInitializeBuyNowAsync(model)
                .ConfigureAwait(false);

            return MapInitialize(sdkResult);
        }

        public async Task<PaymentResult> RetrievePaymentResultAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Payment token cannot be null or empty.", nameof(token));
            }

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

            // Checkout Form Initialize returns token + form HTML / paymentPageUrl (no BasketId).
            // See https://docs.iyzico.com/en/payment-methods/checkoutform/cf-implementation
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

            return new PaymentResult
            {
                Token = checkoutForm.Token,
                Price = checkoutForm.Price,
                PaidPrice = checkoutForm.PaidPrice,
                Installment = checkoutForm.Installment.HasValue
                    ? checkoutForm.Installment.Value.ToString()
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

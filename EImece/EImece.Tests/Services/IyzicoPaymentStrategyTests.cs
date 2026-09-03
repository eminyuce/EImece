using EImece.Domain.Configuration;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Services;
using EImece.Domain.Services.Payment;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class IyzicoPaymentStrategyTests
    {
        private sealed class FakeIyzicoService : IyzicoService
        {
            public CheckoutFormInitialize NextInitializeResult { get; set; }
            public CheckoutForm NextRetrieveResult { get; set; }

            public ShoppingCartSession LastCart { get; private set; }
            public string LastUserId { get; private set; }
            public string LastCallbackAction { get; private set; }
            public BuyNowModel LastBuyNowModel { get; private set; }
            public string LastTokenRequested { get; private set; }

            public FakeIyzicoService()
                : base(
                    new NullLogger<IyzicoService>(),
                    Options.Create(new IyzicoOptions { ApiKey = "dummy-api-key", SecretKey = "dummy-secret-key" }))
            {
            }

            public override Task<CheckoutFormInitialize> CreateCheckoutFormInitializeAsync(
                ShoppingCartSession shoppingCart,
                string userId,
                string actionName = "PaymentResult",
                string callbackUrl = null)
            {
                LastCart = shoppingCart;
                LastUserId = userId;
                LastCallbackAction = actionName;
                return Task.FromResult(NextInitializeResult);
            }

            public override Task<CheckoutFormInitialize> CreateCheckoutFormInitializeBuyNowAsync(
                BuyNowModel buyNowModel,
                string callbackUrl = null)
            {
                LastBuyNowModel = buyNowModel;
                return Task.FromResult(NextInitializeResult);
            }

            public override Task<CheckoutForm> GetCheckoutFormAsync(RetrieveCheckoutFormRequest model)
            {
                LastTokenRequested = model != null ? model.Token : null;
                return Task.FromResult(NextRetrieveResult);
            }
        }

        [TestMethod]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            try
            {
                new IyzicoPaymentStrategy(null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void ProviderName_ReturnsIyzico()
        {
            var fakeService = new FakeIyzicoService();
            var strategy = new IyzicoPaymentStrategy(fakeService);

            Assert.AreEqual("Iyzico", strategy.ProviderName);
        }

        [TestMethod]
        public async Task InitializeCheckoutAsync_MapsSdkResultAccurately()
        {
            var fakeService = new FakeIyzicoService
            {
                NextInitializeResult = new CheckoutFormInitialize
                {
                    CheckoutFormContent = "<script>iyziInit();</script>",
                    Token = "test-token-123",
                    Status = "success",
                    ErrorCode = null,
                    ErrorMessage = null,
                    ConversationId = "ORD-998877",
                    PaymentPageUrl = "https://sandbox-cpp.iyzipay.com?token=test-token-123"
                }
            };

            var strategy = new IyzicoPaymentStrategy(fakeService);
            var cart = new ShoppingCartSession { OrderGuid = "guid-abc" };

            var result = await strategy.InitializeCheckoutAsync(cart, "user-42", "CustomCallbackAction");

            Assert.IsNotNull(result);
            Assert.AreEqual("<script>iyziInit();</script>", result.CheckoutFormContent);
            Assert.AreEqual("test-token-123", result.Token);
            Assert.AreEqual("success", result.Status);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual("ORD-998877", result.ConversationId);
            Assert.AreEqual("https://sandbox-cpp.iyzipay.com?token=test-token-123", result.PaymentPageUrl);
            Assert.AreEqual("Iyzico", result.ProviderName);

            Assert.AreSame(cart, fakeService.LastCart);
            Assert.AreEqual("user-42", fakeService.LastUserId);
            Assert.AreEqual("CustomCallbackAction", fakeService.LastCallbackAction);
        }

        [TestMethod]
        public async Task InitializeCheckoutAsync_NullSdkResult_ReturnsNull()
        {
            var fakeService = new FakeIyzicoService { NextInitializeResult = null };
            var strategy = new IyzicoPaymentStrategy(fakeService);

            var result = await strategy.InitializeCheckoutAsync(new ShoppingCartSession(), "user-1");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task InitializeBuyNowAsync_MapsSdkResultAccurately()
        {
            var fakeService = new FakeIyzicoService
            {
                NextInitializeResult = new CheckoutFormInitialize
                {
                    CheckoutFormContent = "<div>BuyNowForm</div>",
                    Token = "buynow-token-99",
                    Status = "success",
                    ErrorCode = null,
                    ErrorMessage = null,
                    ConversationId = "c-10-p-5",
                    PaymentPageUrl = "https://sandbox-cpp.iyzipay.com?token=buynow-token-99"
                }
            };

            var strategy = new IyzicoPaymentStrategy(fakeService);
            var buyNow = new BuyNowModel
            {
                OrderGuid = "buynow-guid-1",
                ProductId = 5,
                Customer = new CustomerDto { Id = 10 }
            };

            var result = await strategy.InitializeBuyNowAsync(buyNow);

            Assert.IsNotNull(result);
            Assert.AreEqual("<div>BuyNowForm</div>", result.CheckoutFormContent);
            Assert.AreEqual("buynow-token-99", result.Token);
            Assert.AreEqual("success", result.Status);
            Assert.AreEqual("c-10-p-5", result.ConversationId);
            Assert.AreEqual("Iyzico", result.ProviderName);
            Assert.AreSame(buyNow, fakeService.LastBuyNowModel);
        }

        [TestMethod]
        public async Task InitializeBuyNowAsync_NullSdkResult_ReturnsNull()
        {
            var fakeService = new FakeIyzicoService { NextInitializeResult = null };
            var strategy = new IyzicoPaymentStrategy(fakeService);

            var result = await strategy.InitializeBuyNowAsync(new BuyNowModel());

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RetrievePaymentResultAsync_FullFieldMappingVerification()
        {
            var fakeService = new FakeIyzicoService
            {
                NextRetrieveResult = new CheckoutForm
                {
                    Token = "retrieved-token-xyz",
                    Price = "150.00",
                    PaidPrice = "165.00",
                    Installment = 6,
                    Currency = "TRY",
                    PaymentId = "123456789",
                    PaymentStatus = "SUCCESS",
                    FraudStatus = 1,
                    MerchantCommissionRate = "2.5",
                    MerchantCommissionRateAmount = "3.75",
                    IyziCommissionRateAmount = "4.20",
                    IyziCommissionFee = "0.25",
                    CardType = "CREDIT_CARD",
                    CardAssociation = "MASTER_CARD",
                    CardFamily = "Bonus",
                    CardToken = "card-tok-11",
                    CardUserKey = "card-user-22",
                    BinNumber = "552879",
                    LastFourDigits = "0008",
                    BasketId = "BSK-100",
                    ConversationId = "CONV-200",
                    ConnectorName = "Garanti",
                    AuthCode = "AUTH999",
                    HostReference = "HOSTREF888",
                    Phase = "AUTH",
                    Status = "success",
                    ErrorCode = null,
                    ErrorMessage = null,
                    Locale = "tr",
                    SystemTime = 1600000000L
                }
            };

            var strategy = new IyzicoPaymentStrategy(fakeService);
            var result = await strategy.RetrievePaymentResultAsync("retrieved-token-xyz");

            Assert.IsNotNull(result);
            Assert.AreEqual("retrieved-token-xyz", result.Token);
            Assert.AreEqual("150.00", result.Price);
            Assert.AreEqual("165.00", result.PaidPrice);
            Assert.AreEqual("6", result.Installment);
            Assert.AreEqual("TRY", result.Currency);
            Assert.AreEqual("123456789", result.PaymentId);
            Assert.AreEqual("SUCCESS", result.PaymentStatus);
            Assert.AreEqual(1, result.FraudStatus);
            Assert.AreEqual("2.5", result.MerchantCommissionRate);
            Assert.AreEqual("3.75", result.MerchantCommissionRateAmount);
            Assert.AreEqual("4.20", result.IyziCommissionRateAmount);
            Assert.AreEqual("0.25", result.IyziCommissionFee);
            Assert.AreEqual("CREDIT_CARD", result.CardType);
            Assert.AreEqual("MASTER_CARD", result.CardAssociation);
            Assert.AreEqual("Bonus", result.CardFamily);
            Assert.AreEqual("card-tok-11", result.CardToken);
            Assert.AreEqual("card-user-22", result.CardUserKey);
            Assert.AreEqual("552879", result.BinNumber);
            Assert.AreEqual("0008", result.LastFourDigits);
            Assert.AreEqual("BSK-100", result.BasketId);
            Assert.AreEqual("CONV-200", result.ConversationId);
            Assert.AreEqual("Garanti", result.ConnectorName);
            Assert.AreEqual("AUTH999", result.AuthCode);
            Assert.AreEqual("HOSTREF888", result.HostReference);
            Assert.AreEqual("AUTH", result.Phase);
            Assert.AreEqual("success", result.Status);
            Assert.IsNull(result.ErrorCode);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual("tr", result.Locale);
            Assert.AreEqual(1600000000L, result.SystemTime);
            Assert.AreEqual("Iyzico", result.ProviderName);

            Assert.AreEqual("retrieved-token-xyz", fakeService.LastTokenRequested);
        }

        [TestMethod]
        public async Task RetrievePaymentResultAsync_NullInstallment_FormatsToStringEmpty()
        {
            var fakeService = new FakeIyzicoService
            {
                NextRetrieveResult = new CheckoutForm
                {
                    Token = "tok-1",
                    Installment = null,
                    Status = "success"
                }
            };

            var strategy = new IyzicoPaymentStrategy(fakeService);
            var result = await strategy.RetrievePaymentResultAsync("tok-1");

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Installment);
        }

        [TestMethod]
        public async Task RetrievePaymentResultAsync_NullCheckoutForm_ReturnsNull()
        {
            var fakeService = new FakeIyzicoService { NextRetrieveResult = null };
            var strategy = new IyzicoPaymentStrategy(fakeService);

            var result = await strategy.RetrievePaymentResultAsync("any-token");

            Assert.IsNull(result);
        }
    }
}

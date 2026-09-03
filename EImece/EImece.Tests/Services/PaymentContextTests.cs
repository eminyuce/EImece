using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using EImece.Domain.Services.IServices;
using EImece.Domain.Services.Payment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class PaymentContextTests
    {
        private sealed class TestPaymentStrategy : IPaymentStrategy
        {
            public string ProviderName { get; set; } = "TestProvider";

            public PaymentInitializeResult NextInitResult { get; set; }
            public PaymentResult NextPaymentResult { get; set; }

            public ShoppingCartSession CapturedCart { get; private set; }
            public string CapturedUserId { get; private set; }
            public string CapturedCallbackAction { get; private set; }
            public BuyNowModel CapturedBuyNowModel { get; private set; }
            public string CapturedToken { get; private set; }

            public Task<PaymentInitializeResult> InitializeCheckoutAsync(
                ShoppingCartSession cart,
                string userId,
                string callbackAction = "PaymentResult")
            {
                CapturedCart = cart;
                CapturedUserId = userId;
                CapturedCallbackAction = callbackAction;
                return Task.FromResult(NextInitResult);
            }

            public Task<PaymentInitializeResult> InitializeBuyNowAsync(BuyNowModel model)
            {
                CapturedBuyNowModel = model;
                return Task.FromResult(NextInitResult);
            }

            public Task<PaymentResult> RetrievePaymentResultAsync(string token)
            {
                CapturedToken = token;
                return Task.FromResult(NextPaymentResult);
            }
        }

        [TestMethod]
        public void Constructor_NullStrategy_ThrowsArgumentNullException()
        {
            try
            {
                new PaymentContext(null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void StrategyAndProviderName_PropertiesReturnAssignedStrategy()
        {
            var strategy = new TestPaymentStrategy { ProviderName = "Iyzico" };
            var context = new PaymentContext(strategy);

            Assert.AreSame(strategy, context.Strategy);
            Assert.AreEqual("Iyzico", context.ProviderName);
        }

        [TestMethod]
        public void SetStrategy_ReplacesStrategyAccurately()
        {
            var strategy1 = new TestPaymentStrategy { ProviderName = "Iyzico" };
            var strategy2 = new TestPaymentStrategy { ProviderName = "Stripe" };
            var context = new PaymentContext(strategy1);

            context.SetStrategy(strategy2);

            Assert.AreSame(strategy2, context.Strategy);
            Assert.AreEqual("Stripe", context.ProviderName);
        }

        [TestMethod]
        public void SetStrategy_NullStrategy_ThrowsArgumentNullException()
        {
            try
            {
                var strategy = new TestPaymentStrategy();
                var context = new PaymentContext(strategy);
                context.SetStrategy(null);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task InitializeCheckoutAsync_DelegatesToStrategy()
        {
            var expectedResult = new PaymentInitializeResult { Token = "token-1", ProviderName = "Iyzico" };
            var strategy = new TestPaymentStrategy { NextInitResult = expectedResult };
            var context = new PaymentContext(strategy);
            var cart = new ShoppingCartSession();

            var result = await context.InitializeCheckoutAsync(cart, "user-99", "MyAction");

            Assert.AreSame(expectedResult, result);
            Assert.AreSame(cart, strategy.CapturedCart);
            Assert.AreEqual("user-99", strategy.CapturedUserId);
            Assert.AreEqual("MyAction", strategy.CapturedCallbackAction);
        }

        [TestMethod]
        public async Task InitializeBuyNowAsync_DelegatesToStrategy()
        {
            var expectedResult = new PaymentInitializeResult { Token = "buynow-tok" };
            var strategy = new TestPaymentStrategy { NextInitResult = expectedResult };
            var context = new PaymentContext(strategy);
            var model = new BuyNowModel();

            var result = await context.InitializeBuyNowAsync(model);

            Assert.AreSame(expectedResult, result);
            Assert.AreSame(model, strategy.CapturedBuyNowModel);
        }

        [TestMethod]
        public async Task RetrievePaymentResultAsync_DelegatesToStrategy()
        {
            var expectedResult = new PaymentResult { Token = "retrieved-tok", PaymentStatus = "SUCCESS" };
            var strategy = new TestPaymentStrategy { NextPaymentResult = expectedResult };
            var context = new PaymentContext(strategy);

            var result = await context.RetrievePaymentResultAsync("retrieved-tok");

            Assert.AreSame(expectedResult, result);
            Assert.AreEqual("retrieved-tok", strategy.CapturedToken);
        }
    }
}

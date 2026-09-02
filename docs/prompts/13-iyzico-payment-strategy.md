# Refactor iyzico payments to Strategy pattern

- **Captured:** 2026-08-11 8:35:16 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are a Senior .NET Architect working on the EImece e-commerce project (ASP.NET MVC + EImece.Domain).

Goal: Refactor the current hard-coded iyzico payment integration into the Strategy behavioral design pattern so that payment providers become interchangeable.

Current state (from source):
- IyzicoService (EImece.Domain/Services/IyzicoService.cs) contains all payment logic:
  - CreateCheckoutFormInitializeAsync(ShoppingCartSession, userId, actionName)
  - CreateCheckoutFormInitializeBuyNowAsync(BuyNowModel)
  - GetCheckoutFormAsync(RetrieveCheckoutFormRequest)
  - Uses Iyzipay SDK, AppConfig.Iyzico* keys, Options, CheckoutFormInitialize, CheckoutForm, Buyer, Address, BasketItem, etc.
- PaymentController directly injects and calls IyzicoService for PlaceOrder, PaymentResult, BuyNow flows, callback handling, price matching, order creation, etc.
- Payment is tightly coupled; there is no abstraction.

Requirements (Strategy pattern):

1. Define a clear Strategy interface (e.g. IPaymentStrategy or IPaymentProvider) that encapsulates the payment behaviors currently in IyzicoService.  
   Typical methods (adapt to existing signatures and return types):
   - Task<PaymentInitializeResult> InitializeCheckoutAsync(ShoppingCartSession cart, string userId, string callbackAction = "PaymentResult");
   - Task<PaymentInitializeResult> InitializeBuyNowAsync(BuyNowModel model);
   - Task<PaymentResult> RetrievePaymentResultAsync(string token / RetrieveCheckoutFormRequest);
   - Any other shared operations needed by PaymentController (status check, paid-price validation helpers, etc.).

2. Create concrete strategy classes:
   - IyzicoPaymentStrategy : implements the interface and contains (or wraps) the existing IyzicoService logic.
   - Prepare the design so a second provider (e.g. StripePaymentStrategy, PayTRPaymentStrategy, or a mock) can be added later without touching PaymentController.

3. Introduce a Context (e.g. PaymentContext or PaymentService) that:
   - Holds a reference to the current IPaymentStrategy.
   - Delegates Initialize / Retrieve calls to the linked strategy.
   - Allows the strategy to be replaced at runtime or via DI configuration.

4. Dependency Injection & configuration:
   - Register the strategy (and context) in the existing DI setup (Microsoft.Extensions.DependencyInjection / previous Ninject remnants).
   - Prefer configuration-driven selection (AppConfig / settings key such as "PaymentProvider" = "Iyzico") so switching providers requires only config change + new strategy class.
   - Keep IyzicoService as an internal implementation detail of IyzicoPaymentStrategy if useful, or move its code into the strategy.

5. Update PaymentController:
   - Replace direct IyzicoService injection with the Context (or the IPaymentStrategy).
   - Keep all existing controller actions, callback URL building, order-guid encryption, validation, order creation, and thank-you flows intact.
   - Ensure async/await, logging, OpenTelemetry activity tags, and SensitiveDataMasker usage are preserved.

6. Supporting types:
   - Introduce neutral result DTOs (PaymentInitializeResult, PaymentResult, etc.) that hide Iyzipay-specific types (CheckoutFormInitialize, CheckoutForm) from the rest of the application.
   - Map iyzico-specific objects inside the concrete strategy only.

7. Constraints:
   - Do not break existing shopping-cart, order, customer, or address flows.
   - Preserve all validation, currency formatting (CurrencyHelper), installment settings, and callback security checks.
   - Keep the change incremental and testable; existing unit/integration tests around payment should still pass after adaptation.
   - Follow the project’s coding style, NLog, and observability patterns.

Deliverables:
- IPaymentStrategy interface + any result DTOs.
- IyzicoPaymentStrategy implementation (migrated from current IyzicoService).
- PaymentContext (or equivalent service) that holds and uses the strategy.
- Updated DI registration.
- Updated PaymentController (and any other callers).
- Brief explanation of how to add a new payment provider later.

Apply the Strategy pattern exactly as described: the context holds a strategy reference and delegates the behavior; clients can replace the strategy to change payment processing without modifying the context or controller.

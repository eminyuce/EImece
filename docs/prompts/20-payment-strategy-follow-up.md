# Extend payment Strategy pattern

- **Captured:** 2026-08-15 6:33:39 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are working on the EImece e-commerce project (ASP.NET MVC 5, .NET Framework 4.8.1, Entity Framework 6).

The project already uses a Payment Strategy pattern:
- IPaymentStrategy
- PaymentContext
- IyzicoPaymentStrategy (Iyzico / Iyzipay)

Task: Improve payment failure, timeout, and retry handling with proper idempotency for Iyzico payments.

Goals
- Prevent double-charging the customer when the user retries or when a timeout occurs
- Handle network timeouts and temporary Iyzico errors gracefully
- Make the checkout flow safe under retries, browser refresh, and double-clicks
- Keep the existing Strategy pattern
- No new NuGet packages and no tech stack upgrade

Requirements

1. Idempotency key
   - Generate a unique idempotency / conversation key before calling Iyzico (e.g. Guid or order-based key).
   - Persist this key with the order (or a payment attempt record) BEFORE calling the payment API.
   - Always send the same key to Iyzico as conversationId (or the equivalent field the current integration uses).
   - If the same key is used again, the system must not create a second successful charge.

2. Payment attempt tracking
   - Before calling Iyzico, record a payment attempt with status = Pending (and the idempotency key).
   - After the Iyzico response:
     - Success → mark attempt and order as Paid / Completed
     - Failure → mark attempt as Failed and store the error message / error code
     - Timeout / unknown → mark as Unknown or Pending-Review (do NOT automatically assume failure or success)
   - Never create a new paid order if an existing successful payment already exists for the same idempotency key or order.

3. Timeout & retry behaviour
   - Set a reasonable timeout on the Iyzico HTTP call (if not already set).
   - On timeout or transient network error:
     - Do not immediately mark the order as failed.
     - Prefer to query Iyzico (payment retrieve / detail) using conversationId or paymentId when possible to learn the real status.
     - Only then decide whether the order is Paid, Failed, or needs manual review.
   - Limit automatic retries (e.g. max 1–2 safe retries for transient errors only).
   - Never retry a request that might have already succeeded without checking status first.

4. User experience
   - On double-click or refresh during payment: detect the existing pending/successful attempt and show a clear message instead of starting a second payment.
   - On failure: show a user-friendly message (do not expose raw Iyzico technical errors).
   - On timeout / unknown: tell the user “Payment is being verified, please wait / check your orders” and avoid creating a duplicate order.

5. Safety rules
   - The happy path (successful first payment) must stay unchanged.
   - Admin users must still be able to see payment status and error details.
   - Log every payment attempt (success, failure, timeout) with the idempotency key using the existing logging infrastructure.
   - Do not store full card data.

6. Technical constraints
   - Stay inside the existing Payment Strategy / IyzicoPaymentStrategy structure.
   - Prefer extending the current payment flow rather than rewriting it.
   - Use the existing Order / payment-related entities and DbContext.
   - Thread-safe and safe under concurrent requests for the same order.
   - No distributed lock service; a database unique constraint or careful status check is enough.

7. Deliverables
   - Code changes for idempotency key generation and persistence
   - Updated IyzicoPaymentStrategy (or PaymentContext) that respects the key and handles timeout/retry safely
   - Clear status transitions for a payment attempt (Pending → Paid / Failed / Unknown)
   - Protection against double submission on the checkout action
   - Short comments explaining the idempotency and timeout strategy
   - List of the exact files you changed

Important
- Keep the solution minimal and production-safe.
- Correctness (no double charge) is more important than fancy retry logic.
- After implementation, briefly describe how a timeout scenario and a double-click scenario are now handled.

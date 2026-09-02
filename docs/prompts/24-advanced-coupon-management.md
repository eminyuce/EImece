# Advanced coupon management and validation

- **Captured:** 2026-08-15 8:35:56 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

# EImece — Advanced Coupon Management & Validation

You are working on the *EImece open-source e-commerce application*.

## Technology Stack

* ASP.NET MVC 5
* .NET Framework 4.8.1
* Entity Framework 6
* C#
* Repository + Service architecture
* Existing Dependency Injection configuration
* Existing MVC/Razor storefront and Admin panel
* Existing Order, Customer, Product, Category, Cart and Pricing infrastructure

## Primary Objective

Upgrade the existing coupon functionality from a primarily time-based coupon system into a robust, maintainable campaign/rule-based coupon system.

The implementation must integrate with the *existing architecture and domain model*.

Do NOT create a parallel coupon/order/cart architecture.

Before modifying anything, inspect the existing:

* Coupon entity/model
* Coupon repository
* Coupon service
* Cart service/repository
* Checkout flow
* Order creation flow
* Customer/user model
* Product/category relationships
* Sale/discount pricing logic
* Admin coupon controller
* Admin coupon views
* Storefront coupon controller/actions
* Cart/checkout views
* Existing migrations/database initialization
* Existing validation and error-handling patterns

Reuse existing functionality wherever possible.

---

# 1. IMPORTANT IMPLEMENTATION RULES

## 1.1 Inspect before coding

Do not immediately create new classes or properties.

First determine:

1. How coupons currently work.
2. Where coupon validation currently happens.
3. Where coupon discounts are calculated.
4. Where cart totals are calculated.
5. Where orders are finalized.
6. How customers/users are represented.
7. How products and categories are related.
8. How sale/discounted products are identified.
9. How database changes are currently handled.
10. How Admin CRUD pages are implemented.

Then implement the feature using the existing patterns.

If equivalent functionality already exists, extend it instead of duplicating it.

---

# 2. COUPON FEATURES

The coupon system must support the following rules.

## 2.1 Basic coupon properties

Support:

* Coupon code
* Active/inactive status
* Start date/time
* End date/time
* Discount type:

  * Percentage
  * Fixed amount
  * Free shipping
* Discount value
* Optional maximum discount amount for percentage coupons

Existing time-based coupons must continue working without requiring administrators to recreate them.

Use backward-compatible defaults where possible.

---

# 3. USAGE LIMITS

Support:

## 3.1 Global usage limit

Example:

text
Maximum usage: 100


After 100 successful redemptions:

text
UsageLimitReached


The limit must be enforced at final order creation, not only when the coupon is initially applied.

---

## 3.2 Per-customer usage limit

Example:

text
Maximum uses per customer: 3


The customer may successfully redeem the coupon up to three times.

---

## 3.3 One-time per customer

Support:

text
Maximum uses per customer = 1


This must work independently from the global usage limit.

---

## 3.4 First-order-only

The coupon is valid only when the customer has no previous successful/completed orders.

Do NOT simply check whether an Order record exists if cancelled/failed orders are possible.

Use the existing order status/business rules to determine what constitutes a successful order.

---

# 4. CART RULES

## 4.1 Minimum order amount

Example:

text
Minimum order amount = 500 TRY


The coupon cannot be used when the eligible cart amount is below the configured minimum.

Return:

text
MinOrderAmountNotMet


Clearly define whether the minimum amount uses:

* subtotal before discount
* eligible-product subtotal
* shipping
* taxes

Prefer the existing application's pricing semantics and document the chosen behaviour.

---

# 5. PRODUCT RESTRICTIONS

Coupons may optionally apply only to:

* Specific products
* Specific categories

Examples:

text
Product A + Product B only


or:

text
Shoes category only


Rules:

* If product restrictions exist, only eligible cart items receive the discount.
* If category restrictions exist, only products belonging to eligible categories qualify.
* If both product and category restrictions exist, define and consistently enforce the relationship.
* A coupon must fail if the cart contains zero eligible products.

Return:

text
NotApplicableToCartItems


Do not apply the discount to unrelated products in the same cart.

---

# 6. SALE PRODUCT EXCLUSION

Support:

text
ExcludeSaleItems


When enabled:

* Products already discounted by the existing pricing/sale mechanism cannot receive the coupon discount.
* Other eligible products in the same cart may still receive the coupon.

If the existing application has a specific definition of a sale/discounted product, reuse it.

Do not create a second sale-price mechanism.

---

# 7. COUPON STACKING

Default behaviour:

text
Only one coupon per order


Prevent applying a second coupon when another coupon is already active.

Return:

text
StackingNotAllowed


If the current system already has stacking functionality, preserve compatibility while making the policy explicit.

Design the model so stacking could be supported later without rewriting the validation architecture.

---

# 8. CUSTOMER / AUDIENCE RULES

Support the following optional restrictions.

## 8.1 Logged-in customers only

If enabled:

text
LoginRequired


Guest users cannot apply the coupon.

---

## 8.2 New customers

Support a configurable new-customer rule.

Possible implementation:

* Registered after a configured date
* First-order-only
* Or both

Do not duplicate the first-order logic unnecessarily.

---

## 8.3 Birthday coupon

Support birthday-based eligibility if customer birthday information exists.

Preferred configuration:

text
BirthdayCoupon = true
BirthdayWindow = Week | Month


Examples:

text
Birthday week


or:

text
Birthday month


If customer birthday data already exists, reuse it.

If it does not exist, add the minimum required field.

Birthday coupons should normally require an authenticated customer.

Return:

text
BirthdayNotEligible


when appropriate.

Be careful with:

* Year of birth
* Leap-day birthdays
* Month boundaries
* Week boundaries
* Date/time zones

Birthday eligibility should compare month/day rather than the customer's birth year.

---

# 9. FREE SHIPPING

Support a coupon type or flag that makes shipping cost zero.

Prefer whichever approach fits the existing order/pricing model cleanly.

Do not duplicate shipping calculation logic.

The final payable amount must never become negative.

---

# 10. DISCOUNT CALCULATION

Support:

## Percentage

Example:

text
20%


Optional cap:

text
Maximum discount = 200 TRY


Calculation:

text
discount = eligibleAmount * percentage
discount = min(discount, maximumDiscount)


---

## Fixed amount

Example:

text
50 TRY


Never allow:

text
discount > eligible payable amount


---

## Free shipping

Shipping becomes:

text
0


subject to the coupon's other eligibility rules.

---

# 11. CENTRAL COUPON VALIDATION

Create or refactor to a *single central coupon validation component/service*.

Do NOT scatter coupon rules across:

* Controllers
* Views
* Cart code
* Checkout code
* Order code

All coupon validation must eventually flow through one central component.

For example, conceptually:

text
ValidateCoupon(...)


or an equivalent method matching the existing architecture.

The validation component should evaluate:

* Coupon existence
* Active status
* Start date
* End date
* Usage limits
* Customer usage
* First-order requirement
* Birthday eligibility
* Login requirement
* Minimum order amount
* Product restrictions
* Category restrictions
* Sale-item exclusion
* Coupon stacking
* Cart eligibility
* Currency
* Discount calculation constraints

Do not blindly copy this method name if the project has a better established convention.

---

# 12. STRUCTURED VALIDATION RESULTS

Do not rely only on strings such as:

text
"Coupon is invalid"


Use a structured result compatible with the existing project.

Conceptually:

text
IsValid
ReasonCode
Message
DiscountAmount
ShippingDiscount


Possible reason codes:

text
CouponNotFound
CouponInactive
CouponExpired
CouponNotYetValid
MinOrderAmountNotMet
NotApplicableToCartItems
UsageLimitReached
CustomerUsageLimitReached
AlreadyUsedByCustomer
FirstOrderOnly
BirthdayNotEligible
LoginRequired
StackingNotAllowed
SaleItemsExcluded
InvalidCurrency
InvalidDiscount


Use the project's existing error/result conventions if they already exist.

---

# 13. REVALIDATION

Coupon validation must occur at all critical points.

## Apply coupon

Validate before storing/applying it to the cart/session.

## Cart changes

Revalidate after:

* Add product
* Remove product
* Change quantity
* Product becomes unavailable
* Product price changes
* Sale status changes
* Cart subtotal changes

If the coupon becomes invalid:

1. Remove it from the active cart.
2. Recalculate totals.
3. Show a clear message.

Never trust a previously validated coupon.

---

# 14. CHECKOUT / ORDER CREATION

The most important validation must happen immediately before final order creation.

The flow should conceptually be:

text
Customer submits checkout
        ↓
Load current cart/database state
        ↓
Validate coupon again
        ↓
Validate usage limits again
        ↓
Calculate final discount
        ↓
Create order
        ↓
Record coupon redemption
        ↓
Commit transaction


Do not rely on the coupon validation performed when the coupon was originally applied.

---

# 15. CONCURRENT COUPON USAGE

This is a critical requirement.

Example:

text
Coupon limit = 1
User A submits checkout
User B submits checkout at the same time


Both requests must NOT successfully consume the same final available redemption.

Use the existing EF6/database capabilities to make the redemption operation safe.

Prefer a transactional database-level approach.

The implementation must prevent:

text
UsageCount = 1
SuccessfulOrdersUsingCoupon = 2


when the configured maximum is 1.

Do not solve concurrency merely with:

csharp
if (usageCount < limit)
{
    usageCount++;
}


because this is vulnerable to race conditions.

Consider:

* Database transaction
* Atomic update
* Coupon redemption record
* Unique constraint/index where appropriate
* Appropriate transaction isolation
* Existing order/coupon architecture

Use the simplest reliable approach compatible with SQL Server + EF6.

---

# 16. COUPON REDEMPTION HISTORY

Admin must be able to see:

* Coupon
* Order
* Customer
* Date/time
* Discount amount
* Coupon code
* Relevant redemption information

If the existing system does not have a proper coupon usage/redemption entity, introduce one.

Prefer a dedicated redemption table/entity over relying only on a mutable integer counter.

For example, conceptually:

text
CouponRedemption
----------------
Id
CouponId
OrderId
CustomerId
DiscountAmount
CreatedDate


Adapt naming and relationships to the existing domain conventions.

The redemption record must represent a *successful completed coupon use*, not merely an attempt to apply a coupon.

---

# 17. ADMIN PANEL

Extend the existing coupon Admin CRUD.

Admin must be able to configure:

### Basic

* Code
* Active
* Start date
* End date
* Discount type
* Discount value
* Maximum discount

### Usage

* Global usage limit
* Per-customer usage limit
* First-order-only
* One-time-per-customer

### Cart

* Minimum order amount
* Product restrictions
* Category restrictions
* Exclude sale items

### Audience

* Logged-in only
* New customer
* Birthday coupon
* Birthday window

### Shipping

* Free shipping

### Stacking

* Allow/disallow coupon stacking

Do not make the Admin UI unnecessarily complicated.

Use the existing Admin styling/components.

Admin should also see:

text
Total redemptions
Remaining redemptions


when a global usage limit exists.

Also provide access to redemption history/customer/order information using the existing Admin conventions.

---

# 18. STOREFRONT

The storefront must support:

### Apply

Customer enters coupon code.

Success:

text
Coupon applied successfully.


Failure:

Display the appropriate validation message.

### Display

The active coupon must be visible in:

* Cart
* Checkout/order summary

Show:

text
Coupon: SAVE20
Discount: -200 TRY


where appropriate.

### Remove

Customer must be able to remove the active coupon.

### Cart changes

Coupon must automatically be revalidated after cart modifications.

---

# 19. GUEST CHECKOUT

Define explicit guest behaviour.

Recommended:

### Guest can use

* Public time-based coupons
* Minimum-order coupons
* Product/category coupons
* Global usage-limited coupons

### Guest cannot use by default

* One-time-per-customer
* Per-customer usage limit
* Birthday coupon
* Customer-specific/VIP coupon
* First-order customer logic if customer identity cannot be reliably established

Return:

text
LoginRequired


when authentication is required.

Do not identify customers using insecure client-side/session-only information.

---

# 20. CURRENCY

The application may support multiple currencies/languages.

Coupon validation and discount calculation must respect the existing application's currency model.

Do not hard-code:

text
TRY
USD
EUR


into coupon logic.

If a coupon is currency-specific, validate it accordingly.

Do not compare monetary values from different currencies.

Reuse the existing money/currency infrastructure.

---

# 21. DATABASE / EF6

Use the existing Entity Framework 6 conventions.

Before adding migrations or schema changes:

1. Inspect existing database initialization/migration strategy.
2. Follow the project's existing convention.
3. Avoid destructive migrations.
4. Preserve existing coupon records.
5. Provide sensible defaults for new nullable/boolean fields.
6. Ensure existing coupons remain valid.

Do not introduce a new ORM or data-access mechanism.

Do not add NuGet packages.

---

# 22. BACKWARD COMPATIBILITY

Existing coupons must continue working.

For example, an existing coupon containing only:

text
Code
StartDate
EndDate
Discount


must continue to work exactly as before unless it violates a newly configured rule.

Do not require administrators to edit every existing coupon.

---

# 23. CLEAN ARCHITECTURE REQUIREMENT

Follow the existing:

text
Controller
    ↓
Service
    ↓
Repository
    ↓
Entity Framework


architecture.

Coupon business rules belong in the service/domain layer, not controllers.

Controllers should not contain large blocks such as:

csharp
if (coupon != null &&
    coupon.IsActive &&
    coupon.StartDate < DateTime.Now &&
    ...)


Centralize the business rules.

Do not duplicate coupon validation between:

* CartController
* CheckoutController
* OrderService
* CouponController

---

# 24. CLEAN CODE

Remove dead code related to the old coupon implementation only when it is confirmed unused.

Do not remove public methods merely because they appear unused without checking:

* MVC action routing
* Razor views
* reflection
* dependency injection
* JavaScript/AJAX calls
* configuration
* external callers

Avoid speculative refactoring.

Keep the change focused on coupon functionality.

---

# 25. TESTING

Add/update tests where the existing project supports them.

At minimum test:

### Basic

* Valid coupon
* Invalid code
* Inactive coupon
* Expired coupon
* Future coupon

### Usage

* Global usage limit
* Global limit reached
* Per-customer limit
* One-time customer coupon
* First-order coupon

### Cart

* Minimum order amount
* Eligible product
* Ineligible product
* Category restriction
* No eligible products
* Sale item exclusion

### Audience

* Guest
* Logged-in customer
* Birthday eligible
* Birthday not eligible

### Discount

* Percentage
* Percentage maximum cap
* Fixed amount
* Discount greater than cart total
* Free shipping

### Stacking

* First coupon applies
* Second coupon rejected

### Cart changes

* Coupon valid before cart change
* Cart change makes coupon invalid
* Coupon automatically removed

### Concurrency

Test the limited-redemption scenario where multiple requests attempt to consume the final available coupon redemption.

---

# 26. DO NOT OVER-ENGINEER V1

Do NOT implement Buy X Get Y in this version.

Do NOT implement a generic campaign/rules engine.

Do NOT introduce a strategy-pattern framework with dozens of classes unless the existing architecture genuinely requires it.

Do NOT introduce external packages.

The priority is:

1. Correctness
2. Concurrency safety
3. Backward compatibility
4. Centralized validation
5. Maintainability
6. Admin usability

---

# 27. REQUIRED DEVELOPMENT PROCESS

Follow this sequence.

## Phase 1 — Analyze

Inspect the entire existing coupon implementation and related cart/order flow.

Produce a short analysis containing:

* Existing coupon entity
* Existing coupon repository
* Existing coupon service
* Existing coupon controller/actions
* Existing cart integration
* Existing checkout integration
* Existing order creation
* Existing customer model
* Existing product/category relationships
* Existing sale-price logic
* Existing database/migration approach

Do not modify code during this phase.

## Phase 2 — Design

Before implementation, determine:

* Required entity changes
* Required new entity/entities
* Required service methods
* Required repository methods
* Required Admin changes
* Required storefront changes
* Required database changes
* Concurrency strategy

Keep the design minimal.

## Phase 3 — Implement

Implement the feature incrementally.

Preserve existing behaviour.

Do not rewrite unrelated code.

## Phase 4 — Verify

Build the complete solution.

Fix:

* Compilation errors
* EF mapping errors
* Razor errors
* MVC routing errors
* Dependency injection errors
* Runtime errors

Then verify existing checkout functionality.

## Phase 5 — Review

Perform a final review specifically for:

* Duplicate coupon validation
* Race conditions
* Incorrect order-status checks
* Negative totals
* Guest/customer identity problems
* Currency inconsistencies
* Sale-product handling
* Existing coupon compatibility
* Dead code
* Unnecessary database queries
* N+1 queries
* Missing indexes
* Transaction boundaries

---

# 28. PERFORMANCE REQUIREMENTS

Coupon validation can happen frequently, so avoid unnecessary database queries.

Pay particular attention to:

* Customer usage count
* Global redemption count
* Product restrictions
* Category restrictions
* First-order checks

Use efficient EF6 queries.

Do not load entire order histories or entire product catalogs into memory just to validate a coupon.

Prefer database-side:

text
Any()
Count()
Exists-style queries


where appropriate.

Avoid N+1 queries when validating product/category restrictions.

---

# 29. FINAL DELIVERABLE

When implementation is complete, provide:

## Changed Files

List every changed/added file and explain why.

Example:

text
Coupon.cs
- Added usage and campaign configuration fields.

CouponService.cs
- Added centralized validation.

CouponRedemption.cs
- Added successful redemption tracking.

CouponRepository.cs
- Added redemption queries.

Admin/CouponController.cs
- Added new coupon configuration.

Admin coupon views
- Added campaign configuration fields.

CartService.cs
- Added coupon revalidation.

OrderService.cs
- Added final transactional coupon validation/redemption.


Use the actual project filenames.

## Database Changes

Explain:

* New tables
* New columns
* Indexes
* Constraints
* Migration changes

## Supported Features

Provide a concise matrix:

| Feature                          | Supported |
| -------------------------------- | --------- |
| Percentage                       | Yes       |
| Fixed amount                     | Yes       |
| Maximum discount                 | Yes       |
| One-time/customer                | Yes       |
| Global usage limit               | Yes       |
| Per-customer limit               | Yes       |
| Minimum order                    | Yes       |
| Product restriction              | Yes       |
| Category restriction             | Yes       |
| Sale exclusion                   | Yes       |
| First order                      | Yes       |
| Birthday                         | Yes       |
| Free shipping                    | Yes       |
| Single coupon/order              | Yes       |
| Concurrent redemption protection | Yes       |

## Edge Cases

Explicitly explain how the implementation handles:

* Already used coupon
* Expired coupon
* Future coupon
* Minimum amount failure
* No eligible products
* Deactivated coupon
* Concurrent final redemption
* Cart changes
* Guest checkout
* Logged-in customer
* Currency
* Birthday
* Sale items
* Negative totals

## Final Verification

Report:

text
Build: PASS/FAIL
Tests: PASS/FAIL
Existing checkout flow: PASS/FAIL
Database migration: PASS/FAIL
Concurrency protection: PASS/FAIL


Do not claim something is tested if it was not actually tested.

# Most Important Requirement

*Do not optimize for the smallest amount of code. Optimize for a simple, centralized, correct coupon implementation that integrates cleanly with the existing EImece architecture.*

Before making architectural changes, inspect the existing implementation and reuse it.

Do not create duplicate business logic.
Do not break existing coupons.
Do not break checkout.
Do not introduce new technologies or NuGet packages.
Do not guess the existing domain model—inspect it first.

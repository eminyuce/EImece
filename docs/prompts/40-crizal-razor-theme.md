# Create the Crizal Razor theme from an HTML template

- **Captured:** 2026-09-02 7:40:45 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are an expert Front-End Engineer and ASP.NET MVC Architect with 20 years of experience specializing in HTML/CSS, JavaScript, jQuery, and ASP.NET MVC 5 Razor templating (.NET Framework 4.8.1).

# Task: Create a New "Crizal" Razor Theme Based on an HTML Template

## Objective
Develop a completely new, independent Razor theme named "Crizal" for an existing ASP.NET MVC 5 application. You will convert static HTML files into dynamic, modular Razor views. 

This is strictly a UI/UX redesign task. Do NOT alter any backend business logic, Models, or core Controller logic.

## Reference Materials
1. Primary Design Direction: https://erayweb.com/2027/index-arttem03.htm
2. Source HTML Template: "Crizal - Multipurpose Responsive Template" (I will provide the specific HTML snippets, CSS, and JS files from this template in my follow-up prompts, or you can reference them from my IDE workspace).

## Deliverables & Expected Output
Please provide the code and folder structure for the following:

1. Folder Structure: Define where the CSS, JS, Fonts, and Images from the Crizal template should live in the ~/Content/ and ~/Scripts/ folders.
2. Layout File (_Layout.cshtml): Create a master layout page incorporating the Crizal template's header, navigation, footer, and required <head> assets.
3. Razor Syntax: 
   - Use @RenderBody() for main content injection.
   - Use @RenderSection("scripts", required: false) for page-specific scripts.
   - Use Url.Content() or ~/ for all asset paths (images, stylesheets, scripts) to ensure they resolve correctly at runtime.
4. BundleConfig: Provide the C# code for BundleConfig.cs to properly bundle and minify the Crizal CSS and jQuery/JS files.
5. Home Page (Index.cshtml): Convert the main body of the reference page into a standard Razor view that hooks into the new _Layout.cshtml.

## Constraints & Rules
- Do not use modern .NET Core tag helpers; strictly stick to .NET Framework 4.8.1 @Html helpers and traditional Razor syntax.
- Maintain all existing responsive design elements, CSS classes, and jQuery initializations exactly as they are in the source template.
- Ensure all static assets (images, CSS, JS) are mapped correctly to the MVC folder structure.

Are you ready to begin? If so, acknowledge these instructions and I will provide the first batch of HTML source code for the Layout file.

---

### Hard Constraints
- Do NOT migrate to ASP.NET Core
- Do NOT upgrade ASP.NET MVC or .NET Framework
- Do NOT rewrite business logic, controllers, services, or database access
- Do NOT change pricing, cart, checkout, payment, authentication, or authorization logic
- Do NOT redesign the Admin panel
- Do NOT invent routes, functionality, or data that does not exist
- Do NOT break the existing design
- Do NOT copy the HTML template blindly — adapt it properly into Razor views that work with the existing application data and models

The new theme must become one of the application’s independent designs under the name Crizal.

---

## Phase 1 – Thorough Audit (Mandatory First Step)

### A. Inspect the Existing ASP.NET MVC Application
Before writing any code, inspect the actual source code and understand:

- Controllers & Actions (especially client-facing ones)
- Models / ViewModels used by the UI
- Existing Razor views structure
- _Layout.cshtml, _ViewStart.cshtml, and shared partials
- Navigation, Header, Footer
- Product listing & product detail pages
- Shopping cart, Checkout, Payment pages
- Account / Authentication pages
- Stories / Content pages
- Search functionality
- Forms, Tables, Pagination
- Error pages
- Existing CSS / JS / Bundles / Bootstrap version
- Image structure and how assets are currently served
- Any existing multi-theme / design-switching mechanism

Do not guess. Base everything on the real codebase.

### B. Inspect the Crizal HTML Template
Thoroughly study the Crizal template, especially:

- https://erayweb.com/2027/index-arttem03.htm (main reference)
- Layout structure, header, navigation, footer
- Typography, color scheme, spacing, components
- Product/service sections, cards, buttons, forms
- Responsive behavior
- CSS and JavaScript structure used by the template

Understand how the design system works so you can faithfully recreate the same modern look & feel in Razor.

---

## Phase 2 – Theme Architecture

Create the new design as a fully independent Razor theme named Crizal.

Recommended structure (adapt to the actual controllers):


Views/
└── Designs/
    └── Crizal/
        ├── Shared/
        │   ├── _Layout.cshtml
        │   ├── _Header.cshtml
        │   ├── _Navigation.cshtml
        │   ├── _Footer.cshtml
        │   └── _Scripts.cshtml
        ├── Home/
        ├── Products/
        ├── Stories/
        ├── Cart/
        ├── Checkout/
        ├── Account/
        └── ...


Critical Rule:  
The Crizal theme must be self-contained.  
Do not let Crizal views silently fall back to the old design’s views.

If a required page has not yet been redesigned, clearly list it as “still pending” instead of mixing old and new UI.

---

## Phase 3 – Design System (Based on Crizal Template)

Faithfully adapt the design system from the Crizal template (https://erayweb.com/2027/index-arttem03.htm):

### Typography
- Use the same font family and hierarchy as the Crizal template
- Clear hierarchy for H1–H3, body, small text, labels, buttons, navigation

### Spacing & Layout
- Match the spacing, section padding, and container widths used in the Crizal template
- Maintain a coherent spacing scale

### Color Palette
Extract and define the exact color system from the Crizal template using CSS variables:
css
:root {
  --crizal-primary: ...;
  --crizal-secondary: ...;
  --crizal-background: ...;
  --crizal-surface: ...;
  --crizal-text: ...;
  --crizal-muted: ...;
  --crizal-border: ...;
  --crizal-success: ...;
  --crizal-warning: ...;
  --crizal-danger: ...;
}


### Other Tokens
- Match border radius, shadows, and icon style from the template
- Use one consistent icon system

---

## Phase 4 – CSS & Assets

- Prefer reusing the CSS/JS from the Crizal template where possible, but organize it cleanly under the new theme.
- Isolate Crizal CSS so it does not affect the existing design.
- Recommended structure:

Content/designs/crizal/
├── css/
│   ├── theme.css
│   ├── components.css
│   └── responsive.css
├── js/
└── images/ (or link to existing images)


- Avoid one giant unmaintainable stylesheet and excessive !important.
- Do not load assets belonging to other themes.

---

## Phase 5 – Core Implementation Order

1. Global Layout  
   _Layout.cshtml, Header, Navigation, Footer — match the Crizal template style

2. Home Page  
   Recreate the look and feel of https://erayweb.com/2027/index-arttem03.htm using real application data

3. Product Listing  
   Modern product grid matching Crizal card style, with filtering, sorting, pagination (where supported)

4. Product Detail  
   Strong visual hierarchy, clear primary CTA, gallery, related products

5. Shopping Cart  
   Mobile-friendly layout matching the template’s aesthetic

6. Checkout  
   Clear, trustworthy multi-section experience (do not change payment logic)

7. Forms  
   Consistent, accessible, mobile-friendly forms styled like the Crizal template

8. Authentication pages  
   Login, Register, Forgot/Reset Password, Account pages

9. Stories / Content pages (if they exist)

10. Search (if it exists)

11. Empty states and Error pages (404, 500, Unauthorized, Forbidden, etc.)

---

## Design Quality Requirements

The final Crizal theme must look like a modern commercial website built with the Crizal design language, not an old ASP.NET MVC site with a skin applied on top.

Required qualities:
- Strong visual hierarchy matching the Crizal template
- Consistent spacing and components
- Professional typography
- Excellent responsive behavior (320px → 1920px)
- Accessible (semantic HTML, labels, focus states, contrast, alt text)
- Clean forms
- Strong product presentation
- Polished empty & error states
- Good performance (lazy loading, optimized images, no unused theme assets)

### Responsive breakpoints to support
320, 375, 414, 768, 1024, 1280, 1440, 1920 px

---

## Components to Standardize (in Crizal style)
Buttons, Cards, Product cards, Story cards, Badges, Alerts, Forms, Inputs, Selects, Tables, Pagination, Breadcrumbs, Tabs, Dropdowns, Modals, Empty states, Loading states, Error states.

---

## JavaScript Guidelines
- Only UI behavior (mobile menu, dropdowns, modals, quantity controls, galleries, filters, etc.)
- Prefer using or adapting the JavaScript already present in the Crizal template
- Do not rewrite business logic in JavaScript
- Existing AJAX / API behavior must continue to work

---

## Final Deliverables

After implementation, provide:

1. List of all files created
2. List of all files modified
3. Pages that were fully redesigned
4. How the Crizal template was adapted into Razor
5. Any new dependencies introduced
6. Confirmation that existing business functionality still works
7. Responsive testing performed
8. Known limitations
9. Screens/pages that still need design work

The final Crizal theme must be complete enough to present to a real client as a professional production UI.


build and deployed to IIS, it will be running under http://localhost:81/


## Phase 6 – Playwright Validation & Regression Testing

Playwright must be used to validate the Crizal theme after implementation.

The application is an existing ASP.NET MVC 5 application running under IIS at:

`http://localhost:81/`

Do not assume the application runs under Visual Studio/IIS Express. Use the deployed IIS URL above for browser testing.

### A. Playwright Setup

If Playwright is not already configured in the repository:

* Add a dedicated Playwright test project/folder without modifying the ASP.NET MVC application's backend architecture.
* Install Playwright and its required browser binaries.
* Use the existing application at `http://localhost:81/` as the test target.
* Do not start a separate development server unless it is absolutely required.
* Do not replace or reconfigure IIS.

Run:

bash
npx playwright install


Use Chromium for the primary UI validation and use additional supported browsers where practical.

### B. Initial Application Audit with Playwright

Before making UI changes, use Playwright to inspect the currently deployed application where practical.

Capture screenshots of important existing pages so the redesigned Crizal theme can be compared against the actual application.

At minimum inspect:

* Home page
* Product listing
* Product detail
* Cart
* Checkout
* Login
* Register
* Account
* Stories
* Search
* Relevant content pages
* Existing error pages

Do not invent URLs.

Determine the actual URLs from the application's controllers, routes, Razor views, navigation, and existing links.

### C. Crizal UI Validation

After implementing each Crizal page, use Playwright to verify that:

* The page loads successfully.
* There are no unexpected HTTP errors.
* There are no unexpected JavaScript console errors.
* Required CSS files load successfully.
* Required JavaScript files load successfully.
* Images load correctly.
* Fonts load correctly.
* Navigation works.
* Dropdowns work.
* Mobile navigation works.
* Buttons are visible and usable.
* Forms render correctly.
* Modals render correctly.
* Pagination works where supported.
* Product cards render correctly.
* Product detail galleries work where supported.
* Existing AJAX functionality continues to work.
* Existing forms continue to submit to their existing endpoints.
* Existing links continue to point to their existing routes.

### D. Responsive Testing

Use Playwright to test the Crizal theme at these viewport sizes:

text
320 × 800
375 × 812
414 × 896
768 × 1024
1024 × 768
1280 × 800
1440 × 900
1920 × 1080


For each important page verify:

* No horizontal scrolling caused by layout defects.
* No overlapping elements.
* No clipped content.
* No broken navigation.
* No unreadable text.
* No buttons extending outside containers.
* No broken product grids.
* No broken tables.
* No broken forms.
* No broken modals.
* Mobile navigation behaves correctly.
* Images maintain appropriate proportions.
* Content fits within the intended container.

### E. Visual Regression Testing

Use Playwright screenshots for important Crizal pages.

Create a baseline screenshot after the Crizal implementation has been approved.

For subsequent changes, compare screenshots against the baseline.

Important pages should include:

* Home
* Product listing
* Product detail
* Cart
* Checkout
* Login
* Account
* Stories
* Search

Do not treat every pixel difference as a defect.

Ignore differences caused by:

* Dynamic timestamps
* Random content
* User-specific information
* Animations
* Dynamic prices where applicable

Focus on genuine layout and visual regressions.

### F. Console and Network Validation

During Playwright testing, collect:

* Browser console errors
* Failed network requests
* HTTP 4xx responses
* HTTP 5xx responses
* Missing CSS
* Missing JavaScript
* Missing images
* Missing fonts

Pay particular attention to:

text
404
403
500
JavaScript exceptions
CSS loading failures
image loading failures
font loading failures


Do not silently ignore these problems.

Investigate whether the problem is caused by:

* Incorrect Razor asset paths
* Incorrect `Url.Content()` usage
* Incorrect BundleConfig paths
* Incorrect relative URLs
* Missing Crizal assets
* IIS static-file configuration
* Incorrect Razor view locations
* Incorrect controller/view mapping

### G. Existing Functionality Regression Testing

Playwright must verify that the Crizal redesign does not break existing functionality.

The following functionality must remain controlled by the existing ASP.NET MVC application:

* Authentication
* Authorization
* Product loading
* Product search
* Product filtering
* Cart operations
* Quantity changes
* Cart removal
* Checkout
* Payment
* Customer/account functionality
* Existing AJAX calls
* Existing forms
* Existing validation
* Existing routes

Do not replace backend functionality with Playwright-specific or JavaScript-only implementations.

Playwright is only a test/validation mechanism.

### H. Critical User Journeys

Where the corresponding functionality exists in the real application, create Playwright tests for critical user journeys.

Example:

text
Home
  ↓
Product listing
  ↓
Product detail
  ↓
Add to cart
  ↓
Cart
  ↓
Checkout


Authentication journey:

text
Login
  ↓
Authenticated page
  ↓
Account
  ↓
Logout


Search journey:

text
Search
  ↓
Search results
  ↓
Product detail


Do not create tests for functionality that does not exist in the application.

### I. Test Failure Rules

If Playwright identifies a problem:

1. Inspect the actual source code.
2. Identify the root cause.
3. Fix the Razor/CSS/JS/theme implementation.
4. Rebuild the application.
5. Deploy to IIS if required.
6. Re-run the affected Playwright tests.
7. Re-run the relevant regression tests.

Do not simply modify Playwright tests to make failures disappear.

Never weaken or delete a test merely because the implementation fails.

### J. Playwright Test Organization

Use a structure similar to:

text
Playwright/
├── tests/
│   ├── home.spec.js
│   ├── navigation.spec.js
│   ├── products.spec.js
│   ├── product-detail.spec.js
│   ├── cart.spec.js
│   ├── checkout.spec.js
│   ├── authentication.spec.js
│   ├── stories.spec.js
│   ├── search.spec.js
│   └── responsive.spec.js
├── screenshots/
├── playwright.config.js
└── package.json


Adapt this structure to the existing repository rather than blindly creating duplicate configuration.

### K. Playwright Configuration

Configure the test base URL as:

javascript
baseURL: 'http://localhost:81'


Tests should normally use relative URLs such as:

javascript
await page.goto('/');


instead of repeatedly hardcoding:

javascript
await page.goto('http://localhost:81/');
```

### L. Final Acceptance Criteria

The Crizal theme is not considered complete until:

* The application builds successfully.
* The application runs successfully under IIS.
* http://localhost:81/ loads correctly.
* Crizal assets load correctly.
* All implemented Crizal Razor views render correctly.
* No unintended old-theme views are displayed.
* No unexpected JavaScript console errors exist.
* No unexpected HTTP 404/500 errors exist.
* Critical existing functionality remains operational.
* Responsive testing passes at all required viewport sizes.
* Important pages have been visually inspected using Playwright screenshots.
* No significant layout overflow or responsive defects remain.
* Playwright tests pass for all implemented functionality.

### Important Rule

Playwright must validate the real deployed ASP.NET MVC 5 application.

Do not create fake/mock pages solely to make Playwright tests pass.

Do not modify backend business logic to satisfy a Playwright test.

If Playwright discovers a problem in the Crizal implementation, fix the actual Razor/CSS/JavaScript/theme implementation.

Playwright-Driven UI/UX Validation

Use Playwright as an integral part of the Crizal theme implementation process.

The purpose of Playwright is not only automated functional testing. It must also be used to *inspect the actual rendered UI, identify visual/design problems, and iteratively improve the Crizal theme*.

The application is deployed to IIS and must be tested at:

text
http://localhost:81/


Do not test against a fake/mock application or a separate frontend implementation.

---

## 6.1 Use Playwright After Every Major UI Implementation

After implementing each major section:

1. Build the ASP.NET MVC application.
2. Deploy/update the IIS application if necessary.
3. Open the real page using Playwright.
4. Capture screenshots.
5. Inspect the rendered result.
6. Identify visual defects.
7. Fix the Razor/CSS/JavaScript implementation.
8. Rebuild and test again.
9. Repeat until the page is visually polished.

Do not consider a page complete merely because the Razor code compiles.

The actual rendered browser result is the source of truth.

---

## 6.2 Pages to Inspect

Use the application's actual routes discovered during the Phase 1 source-code audit.

Do not invent routes.

Inspect all implemented Crizal pages, including where they exist:

* Home
* Product listing
* Product detail
* Cart
* Checkout
* Login
* Register
* Forgot password
* Account
* Stories
* Search
* Content pages
* Error pages
* Empty states

If a page has not been implemented in Crizal, do not silently display the old theme.

Mark it as:

text
Crizal implementation pending


or report it as pending in the implementation summary.

---

# 6.3 Visual Inspection Requirements

When inspecting a page with Playwright, actively look for:

### Layout

* Content width
* Container alignment
* Section spacing
* Header height
* Navigation alignment
* Footer alignment
* Grid alignment
* Card dimensions
* Uneven whitespace
* Elements touching container edges
* Elements extending outside containers
* Horizontal overflow
* Vertical spacing inconsistencies

### Typography

Check:

* Font family
* Font loading
* Font sizes
* Font weights
* Line heights
* Heading hierarchy
* Text wrapping
* Button text alignment
* Navigation typography
* Product title wrapping
* Long text behavior

### Visual hierarchy

Check whether:

* Primary CTA is visually obvious.
* Important information receives appropriate emphasis.
* Product price is easy to find.
* Product title is clearly visible.
* Secondary actions are visually subordinate.
* Sections have clear separation.
* The page does not look like an old MVC application with a CSS skin.

### Components

Inspect:

* Buttons
* Product cards
* Story cards
* Forms
* Inputs
* Selects
* Alerts
* Badges
* Tables
* Pagination
* Breadcrumbs
* Tabs
* Dropdowns
* Modals
* Empty states
* Loading states
* Error states

All components must visually belong to the same Crizal design system.

---

# 6.4 Responsive Visual Testing

Use Playwright to inspect the pages at all required viewport sizes:

text
320 × 800
375 × 812
414 × 896
768 × 1024
1024 × 768
1280 × 800
1440 × 900
1920 × 1080


For each viewport, inspect the actual rendered page.

Pay particular attention to:

### Mobile

* Mobile navigation
* Hamburger menu
* Header layout
* Product grid
* Product images
* Product titles
* Prices
* Buttons
* Forms
* Tables
* Checkout sections
* Modal width
* Pagination
* Footer

### Tablet

Check that the layout does not become an awkward intermediate desktop/mobile hybrid.

### Desktop

Check:

* Maximum content width
* Grid proportions
* Section spacing
* Header alignment
* Navigation spacing
* Product card dimensions
* Footer columns
* Excessive empty space

### Large screens

At 1440px and 1920px, prevent content from becoming excessively stretched.

Use the Crizal template's intended container/max-width behavior.

---

# 6.5 Detect Horizontal Overflow

Use Playwright to explicitly detect horizontal overflow.

The page should normally satisfy:

javascript
document.documentElement.scrollWidth <= document.documentElement.clientWidth


If this fails, investigate the actual cause.

Do not simply hide the problem using:

css
overflow-x: hidden;


unless the design genuinely requires it.

Find and fix the element causing the overflow.

Typical causes include:

* Fixed-width elements
* Images
* Tables
* Bootstrap rows
* Negative margins
* Long text
* Buttons
* Navigation
* Product grids
* Modals

---

# 6.6 Screenshot-Based Design Review

For each major page, capture screenshots using Playwright.

Example:

javascript
await page.screenshot({
    path: 'screenshots/home-desktop.png',
    fullPage: true
});


Also capture important mobile layouts.

Example:

text
screenshots/
├── home/
│   ├── 320.png
│   ├── 375.png
│   ├── 414.png
│   ├── 768.png
│   ├── 1024.png
│   ├── 1280.png
│   ├── 1440.png
│   └── 1920.png
├── products/
├── product-detail/
├── cart/
├── checkout/
└── account/


Use these screenshots to evaluate the actual UI.

---

# 6.7 Do Not Trust Source Code Alone

A page is NOT considered visually complete because:

* HTML is valid.
* Razor compiles.
* CSS looks correct in source code.
* Classes match the Crizal template.
* The page returns HTTP 200.

The rendered browser output must also be inspected.

For example, if the source contains:

html
<div class="product-grid">


that does not prove that the product grid actually looks correct.

Use Playwright to inspect the rendered result.

---

# 6.8 Detect Broken Assets

Use Playwright to identify:

* Missing images
* Missing CSS
* Missing JavaScript
* Missing fonts
* Incorrect asset URLs
* 404 responses
* 403 responses
* 500 responses

Pay particular attention to Razor paths.

All Crizal assets must work correctly when deployed under:

text
http://localhost:81/


Do not assume that a relative path that works from a static HTML file will work correctly inside ASP.NET MVC Razor.

Use:

csharp
@Url.Content("~/Content/designs/crizal/...")


or:

csharp
~/Content/designs/crizal/...


where appropriate.

---

# 6.9 Console Error Detection

Playwright must monitor browser console errors.

The final implementation should not introduce unexpected:

text
JavaScript errors
ReferenceErrors
TypeErrors
jQuery errors
plugin initialization errors
missing function errors


If a Crizal template JavaScript plugin fails because of incorrect script ordering, missing dependencies, or incorrect asset paths, fix the implementation.

Do not simply suppress the error.

---

# 6.10 Network Error Detection

Monitor failed network requests.

Investigate unexpected:

text
404
403
500
502


responses.

Distinguish between legitimate application responses and actual errors.

Do not modify backend behavior simply to satisfy the visual test.

---

# 6.11 Validate Existing Functionality

The Crizal theme must preserve existing application behavior.

Use Playwright to verify existing functionality where applicable:

text
Navigation
Product search
Product listing
Product detail
Add to cart
Remove from cart
Quantity controls
Cart totals
Checkout
Login
Logout
Account pages
Forms
AJAX operations
Pagination
Filtering


Do not rewrite business logic.

Playwright must interact with the existing application exactly as a real user would.

---

# 6.12 Visual Consistency Between Pages

Use Playwright screenshots to compare the visual language of different pages.

The following must feel like one product:

text
Home
   ↓
Product listing
   ↓
Product detail
   ↓
Cart
   ↓
Checkout
   ↓
Account


Check for consistency in:

* Header
* Navigation
* Footer
* Container width
* Typography
* Buttons
* Colors
* Cards
* Forms
* Breadcrumbs
* Alerts
* Spacing
* Border radius
* Shadows
* Icons

Do not allow one page to look like the old design while another page uses Crizal.

---

# 6.13 Iterative UI Improvement

Use the following development loop:

text
IMPLEMENT
    ↓
BUILD
    ↓
DEPLOY TO IIS
    ↓
OPEN WITH PLAYWRIGHT
    ↓
CAPTURE SCREENSHOT
    ↓
INSPECT UI
    ↓
IDENTIFY DESIGN PROBLEMS
    ↓
FIX RAZOR/CSS/JS
    ↓
REBUILD
    ↓
RETEST
    ↓
APPROVE


Repeat this process for every major page.

---

# 6.14 Prioritize Real Visual Problems

When Playwright reveals a design problem, prioritize issues in this order:

1. Broken layout
2. Content overflow
3. Broken responsive behavior
4. Missing/broken assets
5. Broken navigation
6. Incorrect typography
7. Poor spacing
8. Poor visual hierarchy
9. Inconsistent components
10. Minor cosmetic details

Do not spend time polishing colors while the layout is broken.

---

# 6.15 Final Visual Acceptance Criteria

The Crizal theme is considered complete only when Playwright-based inspection confirms:

* No significant horizontal overflow.
* No broken responsive layouts.
* No missing visual assets.
* No unexpected JavaScript console errors.
* No unexpected HTTP errors.
* Header is consistent across Crizal pages.
* Navigation is consistent across Crizal pages.
* Footer is consistent across Crizal pages.
* Typography is consistent.
* Buttons are consistent.
* Cards are consistent.
* Forms are consistent.
* Product presentation is consistent.
* Mobile layouts are professionally designed.
* Desktop layouts are professionally designed.
* Large-screen layouts do not become excessively stretched.
* Existing functionality remains operational.
* Crizal does not accidentally load components from the old theme.

The final result must be judged by the *actual browser-rendered UI*, not only by the source code.

---

# Important AI Coding Agent Rule

Do not say:

> "The design should look good."

Instead, verify the actual rendered application using Playwright.

If a screenshot reveals that a component is:

* misaligned,
* too large,
* too small,
* poorly spaced,
* overflowing,
* inconsistent,
* visually unbalanced,
* broken on mobile,
* or inconsistent with the Crizal template,

fix the implementation before continuing.

Continue the inspect → fix → retest cycle until the rendered UI is production quality.

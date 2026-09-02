# Storefront visual QA loop with Playwright

- **Captured:** 2026-08-21 10:54:34 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Visual QA & Fix Loop for Storefront Pages (Playwright + Chromium)
Use Playwright with Chromium to drive the entire verification and fix cycle for the storefront page(s).
Process (strict loop)

Navigate to the storefront page with Playwright + Chromium.
Capture full-page and key viewport screenshots (desktop + mobile breakpoints).
Inspect the actual rendered DOM and visual output.
Identify visual defects (layout shifts, spacing, alignment, overflow, contrast, responsiveness, broken components, etc.).
Fix the underlying Razor, CSS, and/or JavaScript.
Rebuild the application.
Re-test in the browser and capture new screenshots.
Repeat until the page is visually polished.

Non-negotiable rules

The rendered browser result is the single source of truth.
Do not treat a page as complete just because the Razor code compiles or the build succeeds.
A page is only done when the screenshots show a clean, professional, defect-free UI across the tested viewports.
Prefer precise, minimal fixes that solve the observed visual problems rather than broad refactors.

These pages currently have clear visual problems. Start by taking screenshots, document the defects you see, then fix them iteratively until the rendered result is solid.

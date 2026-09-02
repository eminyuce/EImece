# Fix Lighthouse performance and accessibility issues

- **Captured:** 2026-08-07 5:44:09 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are an expert frontend performance and accessibility engineer.

I need you to analyze and fix the critical issues found in Lighthouse reports (Mobile + Desktop) for the website running at http://localhost:81/.

### Current Scores

*Mobile:*
- Performance: 0.40
- Accessibility: 0.76
- Best Practices: 0.96
- SEO: 0.92

*Desktop:*
- Performance: 0.81
- Accessibility: 0.73
- Best Practices: 1.00
- SEO: 0.92

---

### Priority 1 – Performance (Highest Impact)

*Critical Metrics (Mobile):*
- Largest Contentful Paint (LCP): 14.4s (score 0)
- First Contentful Paint (FCP): 6.5s (score 0.02)
- Time to Interactive (TTI): 14.7s
- Speed Index: 6.5s
- Cumulative Layout Shift (CLS): 0.431 (score 0.22)

*Main Opportunities (both Mobile & Desktop):*

1. *Image Delivery* (Est. savings: 770–820 KiB)
   - Many images are served at 1200×900 but displayed at much smaller sizes (89×67, 432×324, 444×333, 561×421, etc.)
   - Some images are not using modern formats (WebP/AVIF)
   - Fix: Implement proper responsive images with srcset + sizes, convert to modern formats, and serve correctly sized images.

2. *Unused CSS* (Est. savings: 755–776 KiB)
   - Extremely large amount of unused CSS. Purge or properly code-split/remove unused styles.

3. *Unused JavaScript* (Est. savings: 399–402 KiB)
   - Large amount of unused JS. Code-split, tree-shake, and lazy-load non-critical JavaScript.

4. *Render-blocking resources*
   - Mobile: ~5.6 seconds potential savings
   - Desktop: ~1.1 seconds potential savings
   - Defer/async non-critical CSS & JS, extract and inline critical CSS, preload key resources.

5. *Caching*
   - ~1.76–1.79 MB potential savings from better cache headers (Cache-Control).

6. *Total network payload is too large* (~2.7 MB). Aggressively reduce transfer size.

7. *LCP Discovery issues*
   - The LCP element is not discovered early enough (missing fetchpriority="high", not preloaded, or incorrectly lazy-loaded).

8. *Network dependency tree* is too deep / has long critical chains.

9. *Back/Forward Cache (bfcache)* is blocked (mainly due to Cache-Control: no-store).

---

### Priority 2 – Accessibility (Both reports)

Fix these failing audits:

- Images missing [alt] attributes
- Buttons without accessible names (button-name)
- Links without discernible names (link-name)
- Insufficient color contrast (color-contrast)
- Heading elements not in sequential order (heading-order)
- Missing <main> landmark (landmark-one-main)
- Elements using prohibited ARIA attributes (aria-prohibited-attr)
- role="none" / role="presentation" conflicts (presentation-role-conflict)
- Invalid list structure (Desktop only)

---

### Your Tasks

1. Identify the actual sources of the largest images, CSS bundles, and JavaScript files.
2. Implement correctly sized responsive images (srcset, sizes) + modern formats (WebP/AVIF).
3. Remove/purge unused CSS (use PurgeCSS, Tailwind purge, CSS modules, or critical CSS extraction).
4. Code-split and lazy-load JavaScript. Remove or defer unnecessary third-party scripts.
5. Fix *all* accessibility issues listed above.
6. Optimize LCP discovery (fetchpriority="high", preload the LCP image, avoid lazy-loading it).
7. Improve caching headers and remove unnecessary no-store that blocks bfcache.
8. Reduce CLS (reserve space for images and fonts, avoid layout shifts).
9. Reduce overall page weight significantly.

### Target Goals
- Mobile Performance score ≥ 85–90
- Accessibility score ≥ 95
- Mobile LCP < 2.5s
- CLS < 0.1

Provide concrete code changes with file paths and clear explanations for every fix.

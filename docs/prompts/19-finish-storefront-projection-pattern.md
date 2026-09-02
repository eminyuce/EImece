# Finish storefront projection pattern everywhere

- **Captured:** 2026-08-14 10:40:10 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Finish the same pattern everywhere (highest impact)
You already did it well for ProductCategory. Do the identical treatment for every other storefront-heavy aggregate:
•  Product (list cards, detail, related, search results)
•  Brand / Tag
•  Menu / Banner / Story / FAQ
•  Cart summary / Mini-cart
•  Order confirmation / tracking views
Rule: no full entity + Include on any public storefront path. Only projections + AsNoTracking + the lightest possible DTO.
Also finish the FrontModel cleanup you started in the copilot prompt (entities must not appear in any end-user FrontModel).
2. Caching must match the new read model
Light projections only help if they are cached properly:
•  Cache the exact DTOs that the views consume (card projection, navigation tree, main-page categories).
•  Use hierarchical / prefix keys so admin changes can invalidate cleanly.
•  Document the cache key strategy and the invalidation points (product save, category move, price change, activation, etc.).
•  Decide TTL + “stale-while-revalidate” behaviour for navigation trees and category cards.
Without this, the nice projections still hit the database on every request under load.

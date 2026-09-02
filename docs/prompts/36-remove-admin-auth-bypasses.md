# Remove admin auth bypasses and keep 2FA

- **Captured:** 2026-08-24 10:07:02 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

Do not introduce authentication bypasses or hardcoded admin exceptions. Admin authentication and 2FA must always follow the application’s System Settings configuration. Remove any 2FA bypass email list and the BypassAdminAuth mechanism entirely. Also remove ExposeDetailedErrors if it is not actively required and is already covered by the application’s standard error-handling configuration.

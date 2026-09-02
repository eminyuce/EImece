# Admin system health status badge

- **Captured:** 2026-08-15 6:34:04 AM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

You are working on the EImece e-commerce project (ASP.NET MVC 5, .NET Framework 4.8.1, Entity Framework 6).

Task: Add a system health status indicator (green/red badge) to the Admin area that is visible after an administrator logs in.

Requirements:

1. Location
   - Place a small health badge in the Admin shared layout (top-right area, near the user name / logout).
   - It must be visible on every Admin page after login.
   - Only show it to authenticated users with Administrator role.

2. Visual design
   - Small colored circle/dot + short label.
   - Green + text “System OK” when healthy.
   - Red + text “System issue” when unhealthy or unreachable.
   - Subtle styling that matches the existing Admin UI (no new CSS frameworks).
   - Optional: tooltip showing “Last checked: X seconds ago”.

3. Behavior
   - On page load, call the existing endpoint GET /health (or /healthz) via AJAX.
   - Parse the JSON response.
   - If Status === "UP" → green.
   - If Status === "DOWN" or the request fails/times out → red.
   - Poll every 60–90 seconds.
   - When the user clicks the badge:
     - Show a small dropdown or Bootstrap modal.
     - Display the full Details map from the health response (sqlServer, fileStorage, backgroundServices, externalApi, etc.).
     - Keep it simple — plain key/value list is enough.

4. Technical constraints
   - Reuse the existing /health endpoint. Do NOT create a new health endpoint.
   - Use the jQuery that is already present in the project.
   - No new NuGet packages.
   - No changes to the Domain layer or repositories.
   - Prefer a partial view under the Admin area (e.g. Areas/Admin/Views/Shared/_SystemHealthBadge.cshtml).
   - Keep JavaScript in a small file or inside the partial (whichever matches the project style better).
   - Do not break existing Admin layout or scripts.

5. Deliverables
   - The partial view.
   - The JavaScript that calls /health and updates the badge.
   - The exact place where the partial is rendered in the Admin layout.
   - Minimal CSS (inline or existing admin stylesheet).
   - Short comment in the code explaining the polling interval and the expected JSON shape.

Important: Keep the implementation minimal, robust, and consistent with the current Admin UI style. Do not over-engineer.

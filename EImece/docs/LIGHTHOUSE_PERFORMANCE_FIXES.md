# Lighthouse performance & accessibility fixes

## Root causes addressed

| Issue | Cause | Fix |
|-------|--------|-----|
| Oversized images (770–820 KiB) | `IsImageFullSrcUnderMediaFolder=true` ignored crop sizes and served full `/media/images/*` | Always use `/images/wXhY/...` resize proxy when width/height > 0; config default set to `false` |
| No modern formats | WebP helper unused | `ImagesController` negotiates `Accept: image/webp` via `FilesHelper.GetResizedImageAsWebP` |
| Unused CSS (~755 KiB) | Layout loaded `theme.min.css` + identical `theme-5c77fc.min.css` + full rounded skin (~284 KiB) | Removed duplicate skins; added tiny `perf-overrides.css` |
| Unused JS (~400 KiB) | `eimeceScripts` re-bundled jQuery while `vendor.min.js` already includes jQuery 3.3.1; Modernizr blocked head | Dropped duplicate jQuery; removed Modernizr from layout |
| LCP discovery | Hero/LCP img lacked preload/`fetchpriority`, carousel `autoHeight` caused CLS | Preload first hero, `fetchpriority=high`, fixed carousel height, width/height attrs |
| Cache / bfcache | Authenticated `CustomOutputCache` set `no-store`; static files lacked long cache | Use `private, max-age=0` without `no-store`; add `clientCache` + webp MIME |
| Accessibility | Missing `main`, empty thumb links, missing alts, aria on bare divs, heading skips, invalid nested `<ul>` | See view changes below |

## Key files

- `EImece.Domain/Helpers/Extensions/EntityExtension.cs` — resized URLs + srcset helper
- `EImece.Domain/Helpers/FilesHelper.cs` — WebP encode path
- `EImece/Controllers/ImagesController.cs` — Accept negotiation + immutable cache
- `EImece/Views/Shared/_Layout.cshtml` — CSS/JS, `<main>`, LCP preload
- `EImece/Views/Home/Index.cshtml` — hero LCP + heading order
- `EImece/Content/mstore/css/perf-overrides.css` — rounded/contrast/CLS
- `EImece/App_Start/BundleConfig.cs` — bundle composition
- `EImece/Web.config` — image mode + static caching

## Deploy note

Rebuild and redeploy (or copy `_publish/Eimece` after build) so `EImece.dll` / `EImece.Domain.dll` pick up image/WebP/cache attribute changes. View-only changes are not enough for image delivery.

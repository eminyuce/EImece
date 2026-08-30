# CAPTCHA configuration (Legacy + Google reCAPTCHA)

EImece supports **two captcha providers** via Web.config. The default is the original arithmetic image CAPTCHA for backward compatibility. Google reCAPTCHA v2 is optional.

## Web.config

```xml
<!-- Legacy | Recaptcha | None -->
<add key="CaptchaProvider" value="Legacy" />

<!-- Used only when CaptchaProvider=Recaptcha (or when CaptchaProvider is omitted and RecaptchaEnabled=true) -->
<add key="RecaptchaEnabled" value="false" />
<add key="RecaptchaSiteKey" value="YOUR_SITE_KEY" />
<add key="RecaptchaSecretKey" value="YOUR_SECRET_KEY" />
```

| `CaptchaProvider` | Behavior |
|-------------------|----------|
| `Legacy` (default) | Original weak arithmetic image CAPTCHA (`GetCaptcha` + Session) |
| `Recaptcha` | Google reCAPTCHA v2 (“I’m not a robot”) |
| `None` | No captcha widget or validation |

Aliases for Legacy: `Arithmetic`, `Weak`, `Old`.  
Aliases for Recaptcha: `Google`, `GoogleRecaptcha`, `RecaptchaV2`.

If `CaptchaProvider` is omitted and `RecaptchaEnabled=true`, Google reCAPTCHA is used.

## Switching modes

**Keep the old CAPTCHA (default):**
```xml
<add key="CaptchaProvider" value="Legacy" />
```

**Enable Google reCAPTCHA:**
```xml
<add key="CaptchaProvider" value="Recaptcha" />
<add key="RecaptchaSiteKey" value="..." />
<add key="RecaptchaSecretKey" value="..." />
```

## Get Google reCAPTCHA keys

1. Open https://www.google.com/recaptcha/admin
2. Register a site → **reCAPTCHA v2** → **“I’m not a robot” Checkbox**
3. Add your domain(s)
4. Copy Site Key + Secret Key into Web.config (do not commit real secrets)
5. Set `CaptchaProvider` to `Recaptcha`

For local testing, Google publishes temporary test keys in the
[reCAPTCHA FAQ](https://developers.google.com/recaptcha/docs/faq#id-like-to-run-automated-tests-with-recaptcha.-what-should-i-do)
(“I’d like to run automated tests with reCAPTCHA”). Prefer keeping those
out of source control and injecting them via local/user secrets or environment-specific config.

## Developer usage

**View:**
```cshtml
@Html.CaptchaWidget("CustomerLogin")
```

**Controller:**
```csharp
using EImece.Filters;
using EImece.Domain.Services;

[HttpPost]
[ValidateAntiForgeryToken]
[ValidateCaptcha(Prefix = "CustomerLogin")]
public ActionResult Login(LoginViewModel model)
{
    if (CaptchaService.HasValidationError(ModelState))
    {
        ModelState.AddModelError("", CaptchaService.GetErrorMessage());
        return View(model);
    }
    // ...
}
```

### Package layout

| Type | Location |
|------|----------|
| `CaptchaService`, `RecaptchaService` | `EImece.Domain/Services/` |
| `CaptchaProviderType` | `EImece.Domain/Models/Enums/` |
| `ValidateCaptchaAttribute` | `EImece.Domain/Helpers/AttributeHelper/` |
| `CaptchaHtmlHelper` | `EImece.Domain/Helpers/HtmlHelpers/` |

`Prefix` is required for Legacy Session keys (`Captcha` + prefix). It is ignored in Recaptcha / None modes.

## Protected forms

Admin login, customer login, register, forgot password, contact us, product reviews.

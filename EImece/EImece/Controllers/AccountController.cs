using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using EImece.Web.Controllers;
using EImece.Web.Filters;
using EImece.Web.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Owin.Security;
using Resources;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [Authorize]
    [NoCache]
    public class AccountController : BaseController
    {
        private const string IndexAction = "Index";
        private const string AdminAreaName = "admin";
        private const string DashboardAction = "Dashboard";
        private const string LockoutAction = "Lockout";
        private const string AdminLoginAction = "AdminLogin";

        private readonly IIdentityManager IdentityManager;
        private readonly IUsersService UsersService;
        private readonly IRazorEngineHelper RazorEngineHelper;
        private readonly IAuthenticationManager AuthenticationManager;
        private readonly ApplicationSignInManager SignInManager;
        private readonly ApplicationUserManager UserManager;
        private readonly TwoFactorTokenService TwoFactorTokenService;
        private readonly ICustomerService CustomerService;

        public AccountController(ISettingService settingService,
            AutoMapper.IMapper mapper,
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            ICustomerService customerService,
            IIdentityManager identityManager,
            IUsersService usersService,
            IRazorEngineHelper razorEngineHelper,
            IAuthenticationManager authenticationManager,
            TwoFactorTokenService twoFactorTokenService, ILogger<AccountController> logger)
            : base(settingService, mapper, logger)
        {
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            SignInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            CustomerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            IdentityManager = identityManager ?? throw new ArgumentNullException(nameof(identityManager));
            UsersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            RazorEngineHelper = razorEngineHelper ?? throw new ArgumentNullException(nameof(razorEngineHelper));
            AuthenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
            TwoFactorTokenService = twoFactorTokenService ?? throw new ArgumentNullException(nameof(twoFactorTokenService));
        }

        [AllowAnonymous]
        public ActionResult AdminLogin(string returnUrl = "")
        {
            if (!Domain.AppConfig.AdminLoginEnabled)
            {
                Logger.LogInformation("AdminLoginEnabled is false. Redirecting AdminLogin to home.");
                return RedirectToAction(IndexAction, "Home", new { area = "" });
            }

            ViewBag.ReturnUrl = returnUrl;
            ApplyAdminCulture();
            return View();
        }

        private void ApplyAdminCulture()
        {
            var settingService = SettingService;
            if (settingService == null)
            {
                try
                {
                    settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                }
                catch
                {
                    settingService = null;
                }
            }

            var adminPanelLang = settingService?.GetSettingByKey(Domain.Constants.AdminPanelLanguage);
            if (!string.IsNullOrWhiteSpace(adminPanelLang))
            {
                SetCurrentCulture(adminPanelLang);
            }
            else
            {
                var cookie = Request?.Cookies["Language"] ?? Request?.Cookies[Domain.Constants.CultureCookieName];
                if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
                {
                    SetCurrentCulture(cookie.Value);
                }
                else
                {
                    SetCurrentCulture(Domain.Constants.TR);
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "AdminLogin")]
        [RateLimit("login", DefaultLimit = 5, DefaultWindowMinutes = 15)]
        public async Task<ActionResult> AdminLogin(LoginViewModel model, string returnUrl = "")
        {
            ApplyAdminCulture();
            Logger.LogDebug($"Entering AdminLogin POST with email: {model?.Email}, returnUrl: {returnUrl}");

            if (!Domain.AppConfig.AdminLoginEnabled)
            {
                Logger.LogDebug("AdminLoginEnabled is false. Rejecting AdminLogin POST.");
                return RedirectToAction(IndexAction, "Home", new { area = "" });
            }

            if (model == null)
            {
                Logger.LogError("Model is null. Throwing ArgumentException.");
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.LogError("Captcha validation failed for AdminLogin.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                Logger.LogDebug("Returning AdminLogin view with captcha error.");
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                Logger.LogDebug("Model state is invalid. Adding error. Returning AdminLogin view with errors.");
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
                return View(model);
            }

            bool isCustomer = this.isUserAsCustomerRole(model);
            Logger.LogDebug($"User role check: isCustomer = {isCustomer}");
            if (isCustomer)
            {
                Logger.LogDebug("Customer role detected for admin login. Adding error.");
                ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                Logger.LogDebug("Returning AdminLogin view with role error.");
                return View(model);
            }

            // Seed/local users often have UserName != Email (e.g. seed-admin / admin@eimece.test).
            var user = await FindUserByEmailOrUserNameAsync(model.Email);
            if (user == null)
            {
                Logger.LogDebug($"No user found for email: {model.Email}");
                ModelState.AddModelError("", Resource.NoUserFound);
                return View(model);
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                Logger.LogInformation("Account locked out for email: {0}", model.Email);
                ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                return await LockoutViewAsync(user.Id, admin: true);
            }

            if (!await UserManager.CheckPasswordAsync(user, model.Password))
            {
                await UserManager.AccessFailedAsync(user.Id);
                Logger.LogDebug($"Password check failed for {model.Email}");
                ModelState.AddModelError("", Resource.InvalidLoginAttemptPasswordNotCorrect);
                return View(model);
            }

            await UserManager.ResetAccessFailedCountAsync(user.Id);

            // Custom TOTP authenticator (Otp.NET) — do not sign in until code is verified.
            // The Admin → System Settings master switch (RequireAdminAuthenticator Setting row)
            // can disable 2FA for the entire app: enrolled admins then sign in with password only.
            bool requireAdminAuth = SettingService?.GetSettingByKey(Domain.Constants.RequireAdminAuthenticator).ToBool(Domain.Constants.DefaultRequireAdminAuthenticator)
                                   ?? Domain.Constants.DefaultRequireAdminAuthenticator;
            if (requireAdminAuth
                && user.TwoFactorAuthenticatorEnabled
                && !string.IsNullOrEmpty(user.AuthenticatorKey))
            {
                string token = await TwoFactorTokenService.CreateTokenAsync(user.Id);
                Logger.LogDebug("Authenticator 2FA required. Redirecting to VerifyAuthenticator.");
                return RedirectToAction("VerifyAuthenticator", new
                {
                    token = token,
                    rememberMe = model.RememberMe,
                    returnUrl = returnUrl
                });
            }

            Logger.LogInformation($"Attempting sign-in for user: {user.UserName} (email: {model.Email})");
            var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
            Logger.LogDebug($"Sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.LogDebug("Sign-in successful. Redirecting to Dashboard.");
                    return RedirectToAction(IndexAction, DashboardAction, new { @area = AdminAreaName });

                case SignInStatus.LockedOut:
                    Logger.LogInformation("Account locked out for email: {0}", model.Email);
                    ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                    Logger.LogDebug("Returning Lockout view.");
                    return await LockoutViewAsync(user.Id, admin: true);

                case SignInStatus.RequiresVerification:
                    Logger.LogDebug($"Account requires verification for email: {model.Email}");
                    ModelState.AddModelError("", string.Format(Resource.AccountRequiresVerification, model.Email));
                    Logger.LogDebug("Redirecting to SendCode.");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                default:
                    Logger.LogDebug($"Unexpected sign-in result for email: {model.Email}: {result}");
                    ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                    Logger.LogDebug("Returning AdminLogin view with failure error.");
                    return View(model);
            }
        }

        private bool isUserAsCustomerRole(LoginViewModel model)
        {
            var login = (model.Email ?? string.Empty).Trim();
            return UsersService.IsUserInRole(login, Domain.Constants.CustomerRole);
        }

        /// <summary>
        /// Resolves Identity users by email or user name so login forms that collect an email
        /// still work when AspNetUsers.UserName differs from Email (seed accounts).
        /// </summary>
        private async Task<ApplicationUser> FindUserByEmailOrUserNameAsync(string emailOrUserName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName))
            {
                return null;
            }

            var key = emailOrUserName.Trim();
            var user = await UserManager.FindByEmailAsync(key);
            if (user != null)
            {
                return user;
            }

            return await UserManager.FindByNameAsync(key);
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "CustomerLogin")]
        [RateLimit("login", DefaultLimit = 5, DefaultWindowMinutes = 15)]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            Logger.LogDebug($"Entering Login POST with email: {model?.Email}, returnUrl: {returnUrl}");
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            if (model == null)
            {
                Logger.LogError("Model is null. Throwing ArgumentException.");
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.LogError("Captcha validation failed for Login.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                Logger.LogDebug("Returning Login view with captcha error.");
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", Resource.ModelIsNotCorrect);
                return View(model);
            }

            bool isCustomer = this.isUserAsCustomerRole(model);
            Logger.LogDebug($"User role check: isCustomer = {isCustomer}");
            if (!isCustomer)
            {
                Logger.LogDebug("Non-customer role detected for customer login. Adding error.");
                ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                Logger.LogDebug("Returning Login view with role error.");
                return View(model);
            }

            var customerUser = await FindUserByEmailOrUserNameAsync(model.Email);
            if (customerUser == null)
            {
                Logger.LogDebug($"No customer user found for email: {model.Email}");
                ModelState.AddModelError("", Resource.NoUserFound);
                return View(model);
            }

            Logger.LogInformation($"Attempting sign-in for user: {customerUser.UserName} (email: {model.Email})");
            var result = await SignInManager.PasswordSignInAsync(customerUser.UserName, model.Password, model.RememberMe, shouldLockout: false);
            Logger.LogDebug($"Sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.LogDebug("Sign-in successful. Redirecting to Customer Home.");
                    return RedirectToAction(IndexAction, "Home", new { @area = "customers" });

                case SignInStatus.LockedOut:
                    Logger.LogInformation("Account locked out for email: {0}", model.Email);
                    ModelState.AddModelError("", string.Format(Resource.AccountLockedOut, model.Email));
                    Logger.LogDebug("Returning Lockout view.");
                    return await LockoutViewAsync(customerUser.Id, admin: false);

                case SignInStatus.RequiresVerification:
                    Logger.LogDebug($"Account requires verification for email: {model.Email}");
                    ModelState.AddModelError("", string.Format(Resource.AccountRequiresVerification, model.Email));
                    Logger.LogDebug("Redirecting to SendCode.");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                    var user = await FindUserByEmailOrUserNameAsync(model.Email);
                    if (user != null)
                    {
                        bool checkPassword = await SignInManager.UserManager.CheckPasswordAsync(user, model.Password);
                        Logger.LogDebug($"Password check for {model.Email}: {checkPassword}");
                        if (!checkPassword)
                        {
                            ModelState.AddModelError("", Resource.InvalidLoginAttemptPasswordNotCorrect);
                        }
                        else
                        {
                            ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                        }
                    }
                    else
                    {
                        Logger.LogDebug($"No user found for email: {model.Email}");
                        ModelState.AddModelError("", Resource.NoUserFound);
                    }
                    Logger.LogDebug("Returning Login view with failure error.");
                    return View(model);

                default:
                    Logger.LogDebug($"Unexpected sign-in result for email: {model.Email}");
                    ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                    Logger.LogDebug("Returning Login view with default error.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            Logger.LogDebug($"Entering VerifyCode with provider: {provider}, returnUrl: {returnUrl}, rememberMe: {rememberMe}");
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                Logger.LogError("User has not been verified.");
                Logger.LogDebug("Returning Error view.");
                return View("Error");
            }
            Logger.LogDebug("User verified. Returning VerifyCode view.");
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            Logger.LogDebug($"Entering VerifyCode POST with provider: {model.Provider}");
            if (!ModelState.IsValid)
            {
                Logger.LogDebug("Model state is invalid. Returning view with errors.");
                return View(model);
            }

            Logger.LogDebug($"Attempting two-factor sign-in. Provider={model.Provider}");
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            Logger.LogDebug($"Two-factor sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.LogInformation("Two-factor sign-in successful. Redirecting to local URL.");
                    return RedirectToLocal(model.ReturnUrl);

                case SignInStatus.LockedOut:
                    Logger.LogInformation("Account locked out. Returning Lockout view.");
                    return await LockoutViewAsync(null, admin: false);

                case SignInStatus.Failure:
                default:
                    Logger.LogDebug("Two-factor sign-in failed. Adding error.");
                    ModelState.AddModelError("", Resource.InvalidCode);
                    Logger.LogDebug("Returning VerifyCode view with error.");
                    return View(model);
            }
        }

        /// <summary>
        /// TOTP authenticator verification after AdminLogin password check (secure DB token).
        /// </summary>
        [AllowAnonymous]
        public ActionResult VerifyAuthenticator(string token, bool rememberMe = false, string returnUrl = null)
        {
            Logger.LogDebug("Entering VerifyAuthenticator GET.");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(AdminLoginAction);
            }

            return View(new VerifyAuthenticatorViewModel
            {
                Token = token,
                RememberMe = rememberMe,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyAuthenticator(VerifyAuthenticatorViewModel model)
        {
            Logger.LogDebug("Entering VerifyAuthenticator POST.");
            if (model == null || string.IsNullOrEmpty(model.Token))
            {
                return RedirectToAction(AdminLoginAction);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userId = await TwoFactorTokenService.ValidateAndConsumeTokenAsync(model.Token);
            if (userId == null)
            {
                ModelState.AddModelError("", AdminResource.SessionTimedOutPleaseLoginAgain);
                return RedirectToAction(AdminLoginAction);
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(AdminLoginAction);
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                return await LockoutViewAsync(user.Id, admin: true);
            }

            bool isValid = Domain.Helpers.AuthenticatorHelper.VerifyCode(user.AuthenticatorKey, model.Code);
            if (isValid)
            {
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInManager.SignInAsync(user, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
                Logger.LogInformation("Authenticator verification succeeded. Redirecting to admin dashboard.");

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction(IndexAction, DashboardAction, new { area = AdminAreaName });
            }

            await UserManager.AccessFailedAsync(user.Id);
            string newToken = await TwoFactorTokenService.CreateTokenAsync(user.Id);
            ModelState.AddModelError("", AdminResource.InvalidVerificationCode);
            model.Token = newToken;
            model.Code = null;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register(string returnUrl = "")
        {
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            var model = new RegisterViewModel();
            model.IsPermissionGranted = true;
            model.Country = "Türkiye";
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "CustomerRegister")]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl = "")
        {
            Logger.LogDebug($"Entering Register POST with email: {model.Email}, returnUrl={returnUrl}");
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.LogError("Captcha validation failed for Register.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                Logger.LogDebug("Returning Register view with captcha error.");
                return View(model);
            }
            if (ModelState.IsValid && GeneralHelper.IsGsmNumberNotValid(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", Resource.GsmNumberNotValidMessage);
            }
            if (ModelState.IsValid && !IyzicoBuyerValidator.IsIyzicoAcceptedEmail(model.Email))
            {
                ModelState.AddModelError("Email", Resource.IyzicoEmailNotValidMessage);
            }
            if (ModelState.IsValid)
            {
                var identity = (model.IdentityNumber ?? string.Empty).Trim();
                if (identity.Length != 11 || !identity.All(char.IsDigit))
                {
                    ModelState.AddModelError("IdentityNumber", Resource.MandatoryField);
                }
            }
            if (ModelState.IsValid)
            {
                var user = model.GetUser();
                Logger.LogInformation($"Creating user with email: {user.Email}");
                var result = await UserManager.CreateAsync(user, model.Password);
                Logger.LogDebug($"User creation result: {result.Succeeded}");
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    Logger.LogDebug("Generated email confirmation token.");
                    var emailTemplate = await RazorEngineHelper.ConfirmYourAccountEmailBodyAsync(model.Email, model.FirstName + " " + model.LastName, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                    Logger.LogInformation("Confirmation email sent.");

                    IdentityManager.AddUserToRole(user.Id, Domain.Constants.CustomerRole);
                    try
                    {
                        await CustomerService.SaveRegisterViewModelAsync(user.Id, model);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "SaveRegisterViewModelAsync failed for user {UserId}. Rolling back Identity user.", user.Id);
                        await UserManager.DeleteAsync(user);
                        ModelState.AddModelError("PhoneNumber", Resource.GsmNumberNotValidMessage);
                        ModelState.AddModelError("", Resource.RequestIsNotValid);
                        return View(model);
                    }
                    Logger.LogInformation($"Assigned Customer role and saved customer data for user ID: {user.Id}");

                    IdentitySignout();
                    Logger.LogDebug("Signed out after registration setup.");

                    var result2 = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, false, shouldLockout: false);
                    Logger.LogDebug($"Post-registration sign-in result: {result2}");
                    return await CompleteRegistrationSignInAsync(model, returnUrl, result2);
                }
                else
                {
                    Logger.LogError($"User registration failed for email: {model.Email}. Errors: {string.Join(", ", result.Errors)}");
                    AddErrors(result);
                }
            }
            else
            {
                if (!model.Password.Any(char.IsLower))
                    ModelState.AddModelError("Password", Resource.PasswordMustContainLowerCase);
                if (!model.Password.Any(char.IsUpper))
                    ModelState.AddModelError("Password", Resource.PasswordMustContainUpperCase);
                if (!model.Password.Any(char.IsDigit))
                    ModelState.AddModelError("Password", Resource.PasswordMustContainDigit);
                if (model.Password.Length < 6)
                    ModelState.AddModelError("Password", Resource.PasswordMinLength);

                Logger.LogDebug("Model state is invalid. Adding error.");
                ModelState.AddModelError("", Resource.RequestIsNotValid);
            }
            Logger.LogDebug("Returning Register view with errors.");
            return View(model);
        }

        private async Task<ActionResult> CompleteRegistrationSignInAsync(RegisterViewModel model, string returnUrl, SignInStatus result2)
        {
            switch (result2)
            {
                case SignInStatus.Success:
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        Logger.LogInformation($"Post-registration sign-in successful. Redirecting to returnUrl: {returnUrl}");
                        return Redirect(returnUrl);
                    }
                    Logger.LogInformation("Post-registration sign-in successful. Redirecting to Customer Home.");
                    return RedirectToAction(IndexAction, "Home", new { @area = "customers" });

                case SignInStatus.LockedOut:
                    Logger.LogInformation("Account locked out for email: {0}", model.Email);
                    ModelState.AddModelError("", $"The account {model.Email} LockedOut");
                    return await LockoutViewAsync(null, admin: false);

                case SignInStatus.RequiresVerification:
                    Logger.LogDebug($"Account requires verification for email: {model.Email}");
                    ModelState.AddModelError("", $"The account {model.Email} RequiresVerification");
                    return View(model);

                case SignInStatus.Failure:
                    var user2 = await FindUserByEmailOrUserNameAsync(model.Email);
                    if (user2 != null)
                    {
                        bool checkPassword = await SignInManager.UserManager.CheckPasswordAsync(user2, model.Password);
                        Logger.LogDebug($"Password check for {model.Email}: {checkPassword}");
                        if (!checkPassword)
                            ModelState.AddModelError("", "Invalid login attempt. Password is not correct");
                        else
                            ModelState.AddModelError("", "Invalid login attempt." + result2.ToString());
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                    }
                    return View(model);

                default:
                    Logger.LogDebug($"Unexpected sign-in result for email: {model.Email}");
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public void IdentitySignout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
            Logger.LogInformation("Signed out user.");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                Logger.LogError("ConfirmEmail failed: userId or code is null.");
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            Logger.LogInformation("Email confirmation result: {0} UserId={1}", result.Succeeded, userId);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "ForgotPassword")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            Logger.LogDebug($"Entering ForgotPassword POST with email: {model.Email}");
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.LogError("Captcha validation failed for ForgotPassword.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                return View(model);
            }
            if (ModelState.IsValid)
            {
                var user = await FindUserByEmailOrUserNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", Resource.NoUserFound);
                    return View("ForgotPassword");
                }
                if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    Logger.LogDebug($"Email not confirmed for user ID: {user.Id}");

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    Logger.LogDebug("Generated email confirmation token.");
                    var emailTemplate = await RazorEngineHelper.ConfirmYourAccountEmailBodyAsync(model.Email, user.FirstName + " " + user.LastName, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                    ModelState.AddModelError("", Resource.UserEmailNotConfirmed);
                    return View("ForgotPassword");
                }
                else
                {
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    Logger.LogDebug("Generated password reset token.");
                    var emailTemplate = await RazorEngineHelper.ForgotPasswordEmailBodyAsync(model.Email, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                    Logger.LogInformation("Password reset email sent.");
                    Logger.LogDebug("Redirecting to ForgotPasswordConfirmation.");
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }
            Logger.LogDebug("Model state is invalid. Returning ForgotPassword view.");
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string userId, string code)
        {
            if (code == null)
            {
                Logger.LogError("ResetPassword failed: code is null. UserId={0}", userId);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(userId);
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Email = user.Email;
            model.Code = code;
            Logger.LogDebug($"Retrieved user email: {user.Email} for reset.");
            Logger.LogDebug("Returning ResetPassword view.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            Logger.LogDebug($"Entering ResetPassword POST with email: {model.Email}");
            if (!ModelState.IsValid)
            {
                Logger.LogDebug("Model state is invalid. Returning view with errors.");
                return View(model);
            }
            var user = await FindUserByEmailOrUserNameAsync(model.Email);
            if (user == null)
            {
                Logger.LogDebug($"No user found for email: {model.Email}. Redirecting to confirmation.");
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                Logger.LogInformation("Password reset successful. Email={0}", model.Email);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            Logger.LogInformation("Password reset failed. Returning view with errors.");
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            Logger.LogDebug($"Entering SendCode with returnUrl: {returnUrl}, rememberMe: {rememberMe}");
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                Logger.LogError("No verified user ID found.");
                Logger.LogDebug("Returning Error view.");
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            Logger.LogDebug($"Retrieved {factorOptions.Count} two-factor providers for user ID: {userId}");
            Logger.LogDebug("Returning SendCode view.");
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            Logger.LogDebug($"Entering SendCode POST with provider: {model.SelectedProvider}");
            if (!ModelState.IsValid)
            {
                Logger.LogDebug("Model state is invalid. Returning view.");
                return View();
            }

            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                Logger.LogError($"Failed to send two-factor code for provider: {model.SelectedProvider}");
                Logger.LogDebug("Returning Error view.");
                return View("Error");
            }
            Logger.LogInformation("Two-factor code sent successfully. Redirecting to VerifyCode.");
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            Logger.LogDebug($"Entering ExternalLoginCallback with returnUrl: {returnUrl}");
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                Logger.LogError("No external login info found.");
                Logger.LogDebug("Redirecting to Login.");
                return RedirectToAction("Login");
            }

            Logger.LogInformation($"Attempting external sign-in for provider: {loginInfo.Login.LoginProvider}");
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            Logger.LogDebug($"External sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.LogInformation("External sign-in successful. Redirecting to local URL.");
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    Logger.LogInformation("Account locked out. Returning Lockout view.");
                    return await LockoutViewAsync(null, admin: false);

                case SignInStatus.RequiresVerification:
                    Logger.LogDebug("Requires verification. Redirecting to SendCode.");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                case SignInStatus.Failure:
                default:
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    Logger.LogDebug("External sign-in failed. Returning ExternalLoginConfirmation view.");
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            Logger.LogDebug($"Entering ExternalLoginConfirmation POST with email: {model.Email}");
            if (User.Identity.IsAuthenticated)
            {
                Logger.LogDebug("User already authenticated. Redirecting to Manage Index.");
                return RedirectToAction(IndexAction, "Manage");
            }

            if (ModelState.IsValid)
            {
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    Logger.LogError("No external login info found.");
                    Logger.LogDebug("Returning ExternalLoginFailure view.");
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                Logger.LogDebug($"User creation result: {result.Succeeded}");
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    Logger.LogDebug($"Add login result: {result.Succeeded}");
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        Logger.LogInformation("User signed in with external login. Redirecting to local URL.");
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
                Logger.LogDebug("External login confirmation failed. Adding errors.");
            }
            ViewBag.ReturnUrl = returnUrl;
            Logger.LogDebug("Returning ExternalLoginConfirmation view with errors.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Logger.LogInformation("User signed out. Redirecting to Home Index.");
            return RedirectToAction(IndexAction, "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                string errorMessage = error.ToLowerInvariant(); // Büyük/küçük harf duyarlılığını kaldırmak için (teknik karşılaştırma, kültürden bağımsız)

                if (errorMessage.Contains("passwords must have at least one lowercase"))
                {
                    ModelState.AddModelError("", "Şifrelerde en az bir küçük harf ('a'-'z') bulunmalıdır.");
                }
                else if (errorMessage.Contains("passwords must have at least one uppercase"))
                {
                    ModelState.AddModelError("", "Şifrelerde en az bir büyük harf ('A'-'Z') bulunmalıdır.");
                }
                else if (errorMessage.Contains("passwords must be at least"))
                {
                    ModelState.AddModelError("", "Şifre en az 6 karakter olmalıdır.");
                }
                else if (errorMessage.Contains("passwords must have at least one digit"))
                {
                    ModelState.AddModelError("", "Şifrelerde en az bir sayı bulunmalıdır.");
                }
                else
                {
                    // Bilinmeyen hata mesajlarını olduğu gibi ekle
                    ModelState.AddModelError("", error);
                }
            }
        }

        private ActionResult RedirectToLocal(string returnUrl = "")
        {
            Logger.LogDebug($"Entering RedirectToLocal with returnUrl: {returnUrl}");
            bool isAdmin = User.IsInRole(Domain.Constants.AdministratorRole) || User.IsInRole(Domain.Constants.EditorRole);
            Logger.LogDebug($"User isAdmin: {isAdmin}");
            if (isAdmin)
            {
                Logger.LogDebug("Admin role detected. Redirecting to Admin Dashboard.");
                return RedirectToAction(IndexAction, DashboardAction, new { @area = AdminAreaName });
            }
            else if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                Logger.LogDebug($"Valid local returnUrl. Redirecting to: {returnUrl}");
                return Redirect(returnUrl);
            }
            Logger.LogDebug("Default redirect to Admin Dashboard.");
            return RedirectToAction(IndexAction, DashboardAction, new { @area = AdminAreaName });
        }

        private async Task<ActionResult> LockoutViewAsync(string userId, bool admin)
        {
            int minutes = UserLockoutHelper.DefaultLockoutMinutes;
            int remainingSeconds = UserLockoutHelper.DefaultLockoutMinutes * 60;
            if (!string.IsNullOrEmpty(userId))
            {
                var end = await UserManager.GetLockoutEndDateAsync(userId).ConfigureAwait(false);
                minutes = UserLockoutHelper.RemainingMinutes(end);
                remainingSeconds = UserLockoutHelper.RemainingSeconds(end);
            }

            ViewBag.LockoutMinutes = minutes;
            ViewBag.LockoutRemainingSeconds = remainingSeconds;
            if (admin)
            {
                return View("AdminLockout");
            }

            ViewBag.LockoutRetryUrl = Url.Action("Login", "Account");
            return View(LockoutAction);
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            private static ILogger Logger =>
                LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(ChallengeResult))
                ?? NullLogger.Instance;

            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                Logger.LogDebug($"Creating ChallengeResult with provider: {provider}, redirectUri: {redirectUri}, userId: {userId}");
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                Logger.LogDebug($"Executing ChallengeResult for provider: {LoginProvider}");
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                    Logger.LogDebug($"Added XsrfKey '{XsrfKey}' with UserId: {UserId} to properties.");
                }
                else
                {
                    Logger.LogDebug($"No UserId provided; XsrfKey '{XsrfKey}' not added to properties.");
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
                Logger.LogDebug("Authentication challenge issued.");
            }
        }

        #endregion Helpers
    }
}
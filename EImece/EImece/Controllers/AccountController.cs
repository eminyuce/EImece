using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EImece.Domain.DependencyInjection;
using EImece.Filters;
using NLog;
using Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string IndexAction = "Index";
        private const string AdminAreaName = "admin";
        private const string DashboardAction = "Dashboard";
        private const string LockoutAction = "Lockout";
        private const string AdminLoginAction = "AdminLogin";

        [Inject]
        public IIdentityManager IdentityManager { get; set; }

        [Inject]
        public IUsersService UsersService { get; set; }

        [Inject]
        public IRazorEngineHelper RazorEngineHelper { get; set; }

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public TwoFactorTokenService TwoFactorTokenService { get; set; }

        private readonly ICustomerService CustomerService;

        public AccountController(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager, ICustomerService customerService)
        {
            Logger.Info("AccountController constructor called. Initializing UserManager, SignInManager, and CustomerService.");
            UserManager = userManager;
            SignInManager = signInManager;
            CustomerService = customerService;
        }

        [AllowAnonymous]
        public ActionResult AdminLogin(string returnUrl = "")
        {
            Logger.Info($"Entering AdminLogin with returnUrl: {returnUrl}");

            if (!Domain.AppConfig.AdminLoginEnabled)
            {
                Logger.Info("AdminLoginEnabled is false. Redirecting AdminLogin to home.");
                return RedirectToAction(IndexAction, "Home", new { area = "" });
            }

            ViewBag.ReturnUrl = returnUrl;
            ApplyAdminCulture();
            Logger.Info("Returning AdminLogin view.");
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
            Logger.Info($"Entering AdminLogin POST with email: {model?.Email}, returnUrl: {returnUrl}");

            if (!Domain.AppConfig.AdminLoginEnabled)
            {
                Logger.Debug("AdminLoginEnabled is false. Rejecting AdminLogin POST.");
                return RedirectToAction(IndexAction, "Home", new { area = "" });
            }

            if (model == null)
            {
                Logger.Error("Model is null. Throwing ArgumentException.");
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.Error("Captcha validation failed for AdminLogin.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                Logger.Debug("Returning AdminLogin view with captcha error.");
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                Logger.Debug("Model state is invalid. Adding error. Returning AdminLogin view with errors.");
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
                return View(model);
            }

            bool isCustomer = this.isUserAsCustomerRole(model);
            Logger.Debug($"User role check: isCustomer = {isCustomer}");
            if (isCustomer)
            {
                Logger.Debug("Customer role detected for admin login. Adding error.");
                ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                Logger.Debug("Returning AdminLogin view with role error.");
                return View(model);
            }

            // Seed/local users often have UserName != Email (e.g. seed-admin / admin@eimece.test).
            var user = await FindUserByEmailOrUserNameAsync(model.Email);
            if (user == null)
            {
                Logger.Debug($"No user found for email: {model.Email}");
                ModelState.AddModelError("", Resource.NoUserFound);
                return View(model);
            }

            if (await UserManager.IsLockedOutAsync(user.Id))
            {
                Logger.Debug($"Account locked out for email: {model.Email}");
                ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                return View(LockoutAction);
            }

            if (!await UserManager.CheckPasswordAsync(user, model.Password))
            {
                await UserManager.AccessFailedAsync(user.Id);
                Logger.Debug($"Password check failed for {model.Email}");
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
                Logger.Debug("Authenticator 2FA required. Redirecting to VerifyAuthenticator.");
                return RedirectToAction("VerifyAuthenticator", new
                {
                    token = token,
                    rememberMe = model.RememberMe,
                    returnUrl = returnUrl
                });
            }

            Logger.Info($"Attempting sign-in for user: {user.UserName} (email: {model.Email})");
            var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
            Logger.Debug($"Sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.Debug("Sign-in successful. Redirecting to Dashboard.");
                    return RedirectToAction(IndexAction, DashboardAction, new { @area = AdminAreaName });

                case SignInStatus.LockedOut:
                    Logger.Debug($"Account locked out for email: {model.Email}");
                    ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                    Logger.Debug("Returning Lockout view.");
                    return View(LockoutAction);

                case SignInStatus.RequiresVerification:
                    Logger.Debug($"Account requires verification for email: {model.Email}");
                    ModelState.AddModelError("", string.Format(Resource.AccountRequiresVerification, model.Email));
                    Logger.Debug("Redirecting to SendCode.");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                default:
                    Logger.Debug($"Unexpected sign-in result for email: {model.Email}: {result}");
                    ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                    Logger.Debug("Returning AdminLogin view with failure error.");
                    return View(model);
            }
        }

        private bool isUserAsCustomerRole(LoginViewModel model)
        {
            Logger.Info($"Entering isUserAsCustomerRole for email: {model.Email}");
            var login = (model.Email ?? string.Empty).Trim();
            bool isCustomer = UsersService.IsUserInRole(login, Domain.Constants.CustomerRole);
            Logger.Info($"User role check result: isCustomer = {isCustomer}");
            return isCustomer;
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
            Logger.Info($"Entering Login with returnUrl: {returnUrl}");
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            Logger.Info("Returning Login view.");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "CustomerLogin")]
        [RateLimit("login", DefaultLimit = 5, DefaultWindowMinutes = 15)]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            Logger.Info($"Entering Login POST with email: {model?.Email}, returnUrl: {returnUrl}");
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            if (model == null)
            {
                Logger.Error("Model is null. Throwing ArgumentException.");
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.Error("Captcha validation failed for Login.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                Logger.Debug("Returning Login view with captcha error.");
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", Resource.ModelIsNotCorrect);
                return View(model);
            }

            bool isCustomer = this.isUserAsCustomerRole(model);
            Logger.Debug($"User role check: isCustomer = {isCustomer}");
            if (!isCustomer)
            {
                Logger.Debug("Non-customer role detected for customer login. Adding error.");
                ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                Logger.Debug("Returning Login view with role error.");
                return View(model);
            }

            var customerUser = await FindUserByEmailOrUserNameAsync(model.Email);
            if (customerUser == null)
            {
                Logger.Debug($"No customer user found for email: {model.Email}");
                ModelState.AddModelError("", Resource.NoUserFound);
                return View(model);
            }

            Logger.Info($"Attempting sign-in for user: {customerUser.UserName} (email: {model.Email})");
            var result = await SignInManager.PasswordSignInAsync(customerUser.UserName, model.Password, model.RememberMe, shouldLockout: false);
            Logger.Debug($"Sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.Debug("Sign-in successful. Redirecting to Customer Home.");
                    return RedirectToAction(IndexAction, "Home", new { @area = "customers" });

                case SignInStatus.LockedOut:
                    Logger.Debug($"Account locked out for email: {model.Email}");
                    ModelState.AddModelError("", string.Format(Resource.AccountLockedOut, model.Email));
                    Logger.Debug("Returning Lockout view.");
                    return View(LockoutAction);

                case SignInStatus.RequiresVerification:
                    Logger.Debug($"Account requires verification for email: {model.Email}");
                    ModelState.AddModelError("", string.Format(Resource.AccountRequiresVerification, model.Email));
                    Logger.Debug("Redirecting to SendCode.");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                    var user = await FindUserByEmailOrUserNameAsync(model.Email);
                    if (user != null)
                    {
                        bool checkPassword = await SignInManager.UserManager.CheckPasswordAsync(user, model.Password);
                        Logger.Debug($"Password check for {model.Email}: {checkPassword}");
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
                        Logger.Debug($"No user found for email: {model.Email}");
                        ModelState.AddModelError("", Resource.NoUserFound);
                    }
                    Logger.Debug("Returning Login view with failure error.");
                    return View(model);

                default:
                    Logger.Debug($"Unexpected sign-in result for email: {model.Email}");
                    ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                    Logger.Debug("Returning Login view with default error.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            Logger.Info($"Entering VerifyCode with provider: {provider}, returnUrl: {returnUrl}, rememberMe: {rememberMe}");
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                Logger.Error("User has not been verified.");
                Logger.Info("Returning Error view.");
                return View("Error");
            }
            Logger.Info("User verified. Returning VerifyCode view.");
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            Logger.Info($"Entering VerifyCode POST with provider: {model.Provider}");
            if (!ModelState.IsValid)
            {
                Logger.Info("Model state is invalid. Returning view with errors.");
                return View(model);
            }

            Logger.Info($"Attempting two-factor sign-in with code: {model.Code}");
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            Logger.Info($"Two-factor sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.Info("Two-factor sign-in successful. Redirecting to local URL.");
                    return RedirectToLocal(model.ReturnUrl);

                case SignInStatus.LockedOut:
                    Logger.Info("Account locked out. Returning Lockout view.");
                    return View(LockoutAction);

                case SignInStatus.Failure:
                default:
                    Logger.Info("Two-factor sign-in failed. Adding error.");
                    ModelState.AddModelError("", Resource.InvalidCode);
                    Logger.Info("Returning VerifyCode view with error.");
                    return View(model);
            }
        }

        /// <summary>
        /// TOTP authenticator verification after AdminLogin password check (secure DB token).
        /// </summary>
        [AllowAnonymous]
        public ActionResult VerifyAuthenticator(string token, bool rememberMe = false, string returnUrl = null)
        {
            Logger.Info("Entering VerifyAuthenticator GET.");
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
            Logger.Info("Entering VerifyAuthenticator POST.");
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
                return View(LockoutAction);
            }

            bool isValid = Domain.Helpers.AuthenticatorHelper.VerifyCode(user.AuthenticatorKey, model.Code);
            if (isValid)
            {
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInManager.SignInAsync(user, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
                Logger.Info("Authenticator verification succeeded. Redirecting to admin dashboard.");

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
            Logger.Info($"Entering Register action. returnUrl={returnUrl}");
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            var model = new RegisterViewModel();
            model.IsPermissionGranted = true;
            ViewBag.ReturnUrl = returnUrl;
            Logger.Info("Returning Register view with default model.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "CustomerRegister")]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl = "")
        {
            Logger.Info($"Entering Register POST with email: {model.Email}, returnUrl={returnUrl}");
            if (!IsProductPriceEnabled)
            {
                return RedirectToAction(IndexAction, "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.Error("Captcha validation failed for Register.");
                ModelState.AddModelError("", CaptchaService.GetErrorMessage());
                Logger.Info("Returning Register view with captcha error.");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                var user = model.GetUser();
                Logger.Info($"Creating user with email: {user.Email}");
                var result = await UserManager.CreateAsync(user, model.Password);
                Logger.Info($"User creation result: {result.Succeeded}");
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    Logger.Info("User signed in after registration.");

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    Logger.Info($"Generated email confirmation token. Callback URL: {callbackUrl}");
                    var emailTemplate = await RazorEngineHelper.ConfirmYourAccountEmailBodyAsync(model.Email, model.FirstName + " " + model.LastName, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                    Logger.Info("Confirmation email sent.");

                    IdentityManager.AddUserToRole(user.Id, Domain.Constants.CustomerRole);
                    await CustomerService.SaveRegisterViewModelAsync(user.Id, model);
                    Logger.Info($"Assigned Customer role and saved customer data for user ID: {user.Id}");

                    IdentitySignout();
                    Logger.Info("Signed out after registration setup.");

                    var result2 = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, false, shouldLockout: false);
                    Logger.Info($"Post-registration sign-in result: {result2}");
                    return await CompleteRegistrationSignInAsync(model, returnUrl, result2);
                }
                else
                {
                    Logger.Error($"User registration failed for email: {model.Email}. Errors: {string.Join(", ", result.Errors)}");
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

                Logger.Info("Model state is invalid. Adding error.");
                ModelState.AddModelError("", Resource.RequestIsNotValid);
            }
            Logger.Info("Returning Register view with errors.");
            return View(model);
        }

        private async Task<ActionResult> CompleteRegistrationSignInAsync(RegisterViewModel model, string returnUrl, SignInStatus result2)
        {
            switch (result2)
            {
                case SignInStatus.Success:
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        Logger.Info($"Post-registration sign-in successful. Redirecting to returnUrl: {returnUrl}");
                        return Redirect(returnUrl);
                    }
                    Logger.Info("Post-registration sign-in successful. Redirecting to Customer Home.");
                    return RedirectToAction(IndexAction, "Home", new { @area = "customers" });

                case SignInStatus.LockedOut:
                    Logger.Debug($"Account locked out for email: {model.Email}");
                    ModelState.AddModelError("", $"The account {model.Email} LockedOut");
                    return View(LockoutAction);

                case SignInStatus.RequiresVerification:
                    Logger.Debug($"Account requires verification for email: {model.Email}");
                    ModelState.AddModelError("", $"The account {model.Email} RequiresVerification");
                    return View(model);

                case SignInStatus.Failure:
                    var user2 = await FindUserByEmailOrUserNameAsync(model.Email);
                    if (user2 != null)
                    {
                        bool checkPassword = await SignInManager.UserManager.CheckPasswordAsync(user2, model.Password);
                        Logger.Info($"Password check for {model.Email}: {checkPassword}");
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
                    Logger.Debug($"Unexpected sign-in result for email: {model.Email}");
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public void IdentitySignout()
        {
            Logger.Info("Entering IdentitySignout.");
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie);
            Logger.Info("Signed out user.");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            Logger.Info($"Entering ConfirmEmail with userId: {userId}, code: {code}");
            if (userId == null || code == null)
            {
                Logger.Error("UserId or code is null.");
                Logger.Info("Returning Error view.");
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            Logger.Info($"Email confirmation result: {result.Succeeded}");
            Logger.Info($"Returning {(result.Succeeded ? "ConfirmEmail" : "Error")} view.");
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            Logger.Info("Entering ForgotPassword action.");
            Logger.Info("Returning ForgotPassword view.");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateCaptcha(Prefix = "ForgotPassword")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            Logger.Info($"Entering ForgotPassword POST with email: {model.Email}");
            if (CaptchaService.HasValidationError(ModelState))
            {
                Logger.Error("Captcha validation failed for ForgotPassword.");
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
                    Logger.Info($"Email not confirmed for user ID: {user.Id}");

                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    Logger.Info($"Generated email confirmation token. Callback URL: {callbackUrl}");
                    var emailTemplate = await RazorEngineHelper.ConfirmYourAccountEmailBodyAsync(model.Email, user.FirstName + " " + user.LastName, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                    ModelState.AddModelError("", Resource.UserEmailNotConfirmed);
                    return View("ForgotPassword");
                }
                else
                {
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    Logger.Info($"Generated password reset token. Callback URL: {callbackUrl}");
                    var emailTemplate = await RazorEngineHelper.ForgotPasswordEmailBodyAsync(model.Email, callbackUrl);
                    await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                    Logger.Info("Password reset email sent.");
                    Logger.Info("Redirecting to ForgotPasswordConfirmation.");
                    return RedirectToAction("ForgotPasswordConfirmation", "Account");
                }
            }
            Logger.Info("Model state is invalid. Returning ForgotPassword view.");
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            Logger.Info("Entering ForgotPasswordConfirmation action.");
            Logger.Info("Returning ForgotPasswordConfirmation view.");
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string userId, string code)
        {
            Logger.Info($"Entering ResetPassword with userId: {userId}, code: {code}");
            if (code == null)
            {
                Logger.Error("Code is null.");
                Logger.Info("Returning BadRequest status.");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(userId);
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Email = user.Email;
            model.Code = code;
            Logger.Info($"Retrieved user email: {user.Email} for reset.");
            Logger.Info("Returning ResetPassword view.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            Logger.Info($"Entering ResetPassword POST with email: {model.Email}");
            if (!ModelState.IsValid)
            {
                Logger.Info("Model state is invalid. Returning view with errors.");
                return View(model);
            }
            var user = await FindUserByEmailOrUserNameAsync(model.Email);
            if (user == null)
            {
                Logger.Info($"No user found for email: {model.Email}. Redirecting to confirmation.");
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            Logger.Info($"Password reset result: {result.Succeeded}");
            if (result.Succeeded)
            {
                Logger.Info("Password reset successful. Redirecting to confirmation.");
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            Logger.Info("Password reset failed. Returning view with errors.");
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            Logger.Info("Entering ResetPasswordConfirmation action.");
            Logger.Info("Returning ResetPasswordConfirmation view.");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            Logger.Info($"Entering ExternalLogin with provider: {provider}, returnUrl: {returnUrl}");
            Logger.Info("Initiating external login challenge.");
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            Logger.Info($"Entering SendCode with returnUrl: {returnUrl}, rememberMe: {rememberMe}");
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                Logger.Error("No verified user ID found.");
                Logger.Info("Returning Error view.");
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            Logger.Info($"Retrieved {factorOptions.Count} two-factor providers for user ID: {userId}");
            Logger.Info("Returning SendCode view.");
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            Logger.Info($"Entering SendCode POST with provider: {model.SelectedProvider}");
            if (!ModelState.IsValid)
            {
                Logger.Info("Model state is invalid. Returning view.");
                return View();
            }

            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                Logger.Error($"Failed to send two-factor code for provider: {model.SelectedProvider}");
                Logger.Info("Returning Error view.");
                return View("Error");
            }
            Logger.Info("Two-factor code sent successfully. Redirecting to VerifyCode.");
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            Logger.Info($"Entering ExternalLoginCallback with returnUrl: {returnUrl}");
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                Logger.Error("No external login info found.");
                Logger.Info("Redirecting to Login.");
                return RedirectToAction("Login");
            }

            Logger.Info($"Attempting external sign-in for provider: {loginInfo.Login.LoginProvider}");
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            Logger.Info($"External sign-in result: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    Logger.Info("External sign-in successful. Redirecting to local URL.");
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    Logger.Info("Account locked out. Returning Lockout view.");
                    return View(LockoutAction);

                case SignInStatus.RequiresVerification:
                    Logger.Info("Requires verification. Redirecting to SendCode.");
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                case SignInStatus.Failure:
                default:
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    Logger.Info("External sign-in failed. Returning ExternalLoginConfirmation view.");
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            Logger.Info($"Entering ExternalLoginConfirmation POST with email: {model.Email}");
            if (User.Identity.IsAuthenticated)
            {
                Logger.Info("User already authenticated. Redirecting to Manage Index.");
                return RedirectToAction(IndexAction, "Manage");
            }

            if (ModelState.IsValid)
            {
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    Logger.Error("No external login info found.");
                    Logger.Info("Returning ExternalLoginFailure view.");
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                Logger.Info($"User creation result: {result.Succeeded}");
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    Logger.Info($"Add login result: {result.Succeeded}");
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        Logger.Info("User signed in with external login. Redirecting to local URL.");
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
                Logger.Info("External login confirmation failed. Adding errors.");
            }
            ViewBag.ReturnUrl = returnUrl;
            Logger.Info("Returning ExternalLoginConfirmation view with errors.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Logger.Info("Entering LogOff action.");
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Logger.Info("User signed out. Redirecting to Home Index.");
            return RedirectToAction(IndexAction, "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            Logger.Info("Entering ExternalLoginFailure action.");
            Logger.Info("Returning ExternalLoginFailure view.");
            return View();
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private void AddErrors(IdentityResult result)
        {
            Logger.Info("Entering AddErrors method.");
            foreach (var error in result.Errors)
            {
                Logger.Info($"Adding error: {error}");
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
            Logger.Info($"Entering RedirectToLocal with returnUrl: {returnUrl}");
            bool isAdmin = User.IsInRole(Domain.Constants.AdministratorRole) || User.IsInRole(Domain.Constants.EditorRole);
            Logger.Info($"User isAdmin: {isAdmin}");
            if (isAdmin)
            {
                Logger.Info("Admin role detected. Redirecting to Admin Dashboard.");
                return RedirectToAction(IndexAction, DashboardAction, new { @area = AdminAreaName });
            }
            else if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                Logger.Info($"Valid local returnUrl. Redirecting to: {returnUrl}");
                return Redirect(returnUrl);
            }
            Logger.Info("Default redirect to Admin Dashboard.");
            return RedirectToAction(IndexAction, DashboardAction, new { @area = AdminAreaName });
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                Logger.Info($"Creating ChallengeResult with provider: {provider}, redirectUri: {redirectUri}, userId: {userId}");
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                Logger.Info($"Executing ChallengeResult for provider: {LoginProvider}");
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                    Logger.Info($"Added XsrfKey '{XsrfKey}' with UserId: {UserId} to properties.");
                }
                else
                {
                    Logger.Info($"No UserId provided; XsrfKey '{XsrfKey}' not added to properties.");
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
                Logger.Info("Authentication challenge issued.");
            }
        }

        #endregion Helpers
    }
}
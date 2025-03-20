using EImece.Domain.DbContext;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Ninject;
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
        private const string CaptchaAdminLogin = "CaptchaAdminLogin";
        private const string CaptchaCustomerLogin = "CaptchaCustomerLogin";
        private const string CaptchaCustomerRegister = "CaptchaCustomerRegister";

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        private ICustomerService CustomerService;

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
            ViewBag.ReturnUrl = returnUrl;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Domain.Constants.TR);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Domain.Constants.TR);
            Logger.Info("Set culture to Turkish (TR).");
            Logger.Info("Returning AdminLogin view.");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminLogin(LoginViewModel model, string returnUrl = "")
        {
            Logger.Info($"Entering AdminLogin POST with email: {model?.Email}, returnUrl: {returnUrl}");
            if (model == null)
            {
                Logger.Error("Model is null. Throwing ArgumentException.");
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                Logger.Info("Model state is invalid. Adding error.");
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
                Logger.Info("Returning AdminLogin view with errors.");
                return View(model);
            }

            if (Session[CaptchaAdminLogin] == null || !Session[CaptchaAdminLogin].ToString().Equals(model.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.Error($"Captcha validation failed. Session: {Session[CaptchaAdminLogin]}, Input: {model.Captcha}");
                ModelState.AddModelError("Captcha", AdminResource.WrongSum);
                ModelState.AddModelError("", AdminResource.WrongSum);
                Logger.Info("Returning AdminLogin view with captcha error.");
                return View(model);
            }
            else
            {
                bool isCustomer = this.isUserAsCustomerRole(model);
                Logger.Info($"User role check: isCustomer = {isCustomer}");
                if (isCustomer)
                {
                    Logger.Info("Customer role detected for admin login. Adding error.");
                    ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                    Logger.Info("Returning AdminLogin view with role error.");
                    return View(model);
                }

                Logger.Info($"Attempting sign-in for email: {model.Email}");
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                Logger.Info($"Sign-in result: {result}");

                switch (result)
                {
                    case SignInStatus.Success:
                        Logger.Info("Sign-in successful. Redirecting to Dashboard.");
                        return RedirectToAction("Index", "Dashboard", new { @area = "admin" });

                    case SignInStatus.LockedOut:
                        Logger.Debug($"Account locked out for email: {model.Email}");
                        ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                        Logger.Info("Returning Lockout view.");
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        Logger.Debug($"Account requires verification for email: {model.Email}");
                        ModelState.AddModelError("", $"The account {model.Email} RequiresVerification");
                        Logger.Info("Redirecting to SendCode.");
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    case SignInStatus.Failure:
                        var user = ApplicationDbContext.Users.FirstOrDefault(u => u.UserName.Equals(model.Email));
                        if (user != null)
                        {
                            bool checkPassword = SignInManager.UserManager.CheckPassword(user, model.Password);
                            Logger.Info($"Password check for {model.Email}: {checkPassword}");
                            if (!checkPassword)
                            {
                                ModelState.AddModelError("", Resource.InvalidLoginAttemptPasswordNotCorrect);
                            }
                            else
                            {
                                ModelState.AddModelError("", Resource.InvalidLoginAttempt + result.ToString());
                            }
                        }
                        else
                        {
                            Logger.Info($"No user found for email: {model.Email}");
                            ModelState.AddModelError("", Resource.NoUserFound);
                        }
                        Logger.Info("Returning AdminLogin view with failure error.");
                        return View(model);

                    default:
                        Logger.Debug($"Unexpected sign-in result for email: {model.Email}");
                        ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                        Logger.Info("Returning AdminLogin view with default error.");
                        return View(model);
                }
            }
        }

        private bool isUserAsCustomerRole(LoginViewModel model)
        {
            Logger.Info($"Entering isUserAsCustomerRole for email: {model.Email}");
            var usersRoles = from u in ApplicationDbContext.Users
                             from ur in u.Roles
                             join r in ApplicationDbContext.Roles on ur.RoleId equals r.Id
                             where u.UserName.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase)
                             select new { Role = r.Name };
            bool isCustomer = usersRoles.Any(r => r.Role.Equals(Domain.Constants.CustomerRole, StringComparison.InvariantCultureIgnoreCase));
            Logger.Info($"User role check result: isCustomer = {isCustomer}");
            return isCustomer;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            Logger.Info($"Entering Login with returnUrl: {returnUrl}");
            ViewBag.ReturnUrl = returnUrl;
            Logger.Info("Returning Login view.");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            Logger.Info($"Entering Login POST with email: {model?.Email}, returnUrl: {returnUrl}");
            if (model == null)
            {
                Logger.Error("Model is null. Throwing ArgumentException.");
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                Logger.Info("Model state is invalid. Adding error.");
                ModelState.AddModelError("", "Model is not correct.");
                Logger.Info("Returning Login view with errors.");
                return View(model);
            }

            if (Session[CaptchaCustomerLogin] == null || !Session[CaptchaCustomerLogin].ToString().Equals(model.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.Error($"Captcha validation failed. Session: {Session[CaptchaCustomerLogin]}, Input: {model.Captcha}");
                ModelState.AddModelError("Captcha", AdminResource.WrongSum);
                ModelState.AddModelError("", AdminResource.WrongSum);
                Logger.Info("Returning Login view with captcha error.");
                return View(model);
            }
            else
            {
                bool isCustomer = this.isUserAsCustomerRole(model);
                Logger.Info($"User role check: isCustomer = {isCustomer}");
                if (!isCustomer)
                {
                    Logger.Info("Non-customer role detected for customer login. Adding error.");
                    ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                    Logger.Info("Returning Login view with role error.");
                    return View(model);
                }

                Logger.Info($"Attempting sign-in for email: {model.Email}");
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                Logger.Info($"Sign-in result: {result}");

                switch (result)
                {
                    case SignInStatus.Success:
                        Logger.Info("Sign-in successful. Redirecting to Customer Home.");
                        return RedirectToAction("Index", "Home", new { @area = "customers" });

                    case SignInStatus.LockedOut:
                        Logger.Debug($"Account locked out for email: {model.Email}");
                        ModelState.AddModelError("", $"The account {model.Email} LockedOut");
                        Logger.Info("Returning Lockout view.");
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        Logger.Debug($"Account requires verification for email: {model.Email}");
                        ModelState.AddModelError("", $"The account {model.Email} RequiresVerification");
                        Logger.Info("Redirecting to SendCode.");
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    case SignInStatus.Failure:
                        var user = ApplicationDbContext.Users.FirstOrDefault(u => u.UserName.Equals(model.Email));
                        if (user != null)
                        {
                            bool checkPassword = SignInManager.UserManager.CheckPassword(user, model.Password);
                            Logger.Info($"Password check for {model.Email}: {checkPassword}");
                            if (!checkPassword)
                            {
                                ModelState.AddModelError("", Resource.InvalidLoginAttemptPasswordNotCorrect);
                            }
                            else
                            {
                                ModelState.AddModelError("", Resource.InvalidLoginAttempt + result.ToString());
                            }
                        }
                        else
                        {
                            Logger.Info($"No user found for email: {model.Email}");
                            ModelState.AddModelError("", Resource.NoUserFound);
                        }
                        Logger.Info("Returning Login view with failure error.");
                        return View(model);

                    default:
                        Logger.Debug($"Unexpected sign-in result for email: {model.Email}");
                        ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                        Logger.Info("Returning Login view with default error.");
                        return View(model);
                }
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
                    return View("Lockout");

                case SignInStatus.Failure:
                default:
                    Logger.Info("Two-factor sign-in failed. Adding error.");
                    ModelState.AddModelError("", "Invalid code.");
                    Logger.Info("Returning VerifyCode view with error.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            Logger.Info("Entering Register action.");
            var model = new RegisterViewModel();
            model.IsPermissionGranted = true;
            Logger.Info("Returning Register view with default model.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            Logger.Info($"Entering Register POST with email: {model.Email}");
            if (ModelState.IsValid)
            {
                if (Session[CaptchaCustomerRegister] == null || !Session[CaptchaCustomerRegister].ToString().Equals(model.Captcha, StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.Error($"Captcha validation failed. Session: {Session[CaptchaCustomerRegister]}, Input: {model.Captcha}");
                    ModelState.AddModelError("Captcha", AdminResource.WrongSum);
                    ModelState.AddModelError("", AdminResource.WrongSum);
                    Logger.Info("Returning Register view with captcha error.");
                    return View(model);
                }
                else
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
                        var emailTemplate = RazorEngineHelper.ConfirmYourAccountEmailBody(model.Email, model.FirstName + " " + model.LastName, callbackUrl);
                        await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                        Logger.Info("Confirmation email sent.");

                        IdentityManager.AddUserToRole(user.Id, Domain.Constants.CustomerRole);
                        CustomerService.SaveRegisterViewModel(user.Id, model);
                        Logger.Info($"Assigned Customer role and saved customer data for user ID: {user.Id}");

                        IdentitySignout();
                        Logger.Info("Signed out after registration setup.");

                        var result2 = await SignInManager.PasswordSignInAsync(model.Email, model.Password, false, shouldLockout: false);
                        Logger.Info($"Post-registration sign-in result: {result2}");
                        switch (result2)
                        {
                            case SignInStatus.Success:
                                Logger.Info("Post-registration sign-in successful. Redirecting to Customer Home.");
                                return RedirectToAction("Index", "Home", new { @area = "customers" });

                            case SignInStatus.LockedOut:
                                Logger.Debug($"Account locked out for email: {model.Email}");
                                ModelState.AddModelError("", $"The account {model.Email} LockedOut");
                                Logger.Info("Returning Lockout view.");
                                return View("Lockout");

                            case SignInStatus.RequiresVerification:
                                Logger.Debug($"Account requires verification for email: {model.Email}");
                                ModelState.AddModelError("", $"The account {model.Email} RequiresVerification");
                                Logger.Info("Returning Register view with verification error.");
                                return View(model);

                            case SignInStatus.Failure:
                                var user2 = ApplicationDbContext.Users.First(u => u.UserName.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase));
                                bool checkPassword = SignInManager.UserManager.CheckPassword(user2, model.Password);
                                Logger.Info($"Password check for {model.Email}: {checkPassword}");
                                if (!checkPassword)
                                    ModelState.AddModelError("", "Invalid login attempt. Password is not correct");
                                else
                                    ModelState.AddModelError("", "Invalid login attempt." + result2.ToString());
                                Logger.Info("Returning Register view with failure error.");
                                return View(model);

                            default:
                                Logger.Debug($"Unexpected sign-in result for email: {model.Email}");
                                ModelState.AddModelError("", "Invalid login attempt.");
                                Logger.Info("Returning Register view with default error.");
                                return View(model);
                        }
                    }
                    else
                    {
                        Logger.Error($"User registration failed for email: {model.Email}. Errors: {string.Join(", ", result.Errors)}");
                        AddErrors(result);
                    }
                }
            }
            else
            {
                Logger.Info("Model state is invalid. Adding error.");
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
            }
            Logger.Info("Returning Register view with errors.");
            return View(model);
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
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            Logger.Info($"Entering ForgotPassword POST with email: {model.Email}");
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    Logger.Info($"No user found for email: {model.Email}");
                    ModelState.AddModelError("", Resource.NoUserFound);
                    Logger.Info("Returning ForgotPassword view with error.");
                    return View("ForgotPassword");
                }
                if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    Logger.Info($"Email not confirmed for user ID: {user.Id}");
                    ModelState.AddModelError("", Resource.UserEmailNotConfirmed);
                    Logger.Info("Returning ForgotPassword view with confirmation error.");
                    return View("ForgotPassword");
                }

                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                Logger.Info($"Generated password reset token. Callback URL: {callbackUrl}");
                var emailTemplate = RazorEngineHelper.ForgotPasswordEmailBody(model.Email, callbackUrl);
                await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                Logger.Info("Password reset email sent.");
                Logger.Info("Redirecting to ForgotPasswordConfirmation.");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
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
            var user = await UserManager.FindByNameAsync(model.Email);
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
                    return View("Lockout");

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
                return RedirectToAction("Index", "Manage");
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
            return RedirectToAction("Index", "Home");
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
                ModelState.AddModelError("", error);
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
                return RedirectToAction("Index", "Dashboard", new { @area = "admin" });
            }
            else if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                Logger.Info($"Valid local returnUrl. Redirecting to: {returnUrl}");
                return Redirect(returnUrl);
            }
            Logger.Info("Default redirect to Admin Dashboard.");
            return RedirectToAction("Index", "Dashboard", new { @area = "admin" });
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
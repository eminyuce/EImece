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
using System.Linq;
using System.Net;
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
            UserManager = userManager;
            SignInManager = signInManager;
            CustomerService = customerService;
        }

        [AllowAnonymous]
        public ActionResult AdminLogin(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminLogin(LoginViewModel model, string returnUrl = "")
        {
            if (model == null)
            {
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
                return View(model);
            }

            //validate the captcha through the session variable stored from GetCaptcha
            if (Session[CaptchaAdminLogin] == null || !Session[CaptchaAdminLogin].ToString().Equals(model.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("Captcha", AdminResource.WrongSum);
                ModelState.AddModelError("", AdminResource.WrongSum);
                return View(model);
            }
            else
            {
                bool isCustomer = this.isUserAsCustomerRole(model);
                if (isCustomer)
                {
                    ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                    return View(model);
                }

                //ApplicationUser signedUser = UserManager.FindByEmail(model.Email);
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToAction("Index", "Dashboard", new { @area = "admin" });

                    case SignInStatus.LockedOut:
                        Logger.Debug(string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                        ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        Logger.Debug("The account  " + model.Email + " RequiresVerification ");
                        ModelState.AddModelError("", "The account  " + model.Email + " RequiresVerification ");
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    case SignInStatus.Failure:
                        var user = ApplicationDbContext.Users.FirstOrDefault(u => u.UserName.Equals(model.Email));
                        if (user != null)
                        {
                            bool checkPassword = SignInManager.UserManager.CheckPassword(user, model.Password);
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
                            ModelState.AddModelError("", Resource.NoUserFound);
                        }
                       
                        return View(model);

                    default:
                        Logger.Debug(string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                        ModelState.AddModelError("", string.Format(Resource.InvalidLoginAttemptEmailLockedOut, model.Email));
                        return View(model);
                }
            }
        }

        private bool isUserAsCustomerRole(LoginViewModel model)
        {
            var users = ApplicationDbContext.Users.AsQueryable();
            var usersRoles = from u in ApplicationDbContext.Users
                             from ur in u.Roles
                             join r in ApplicationDbContext.Roles on ur.RoleId equals r.Id
                             where u.UserName.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase)
                             select new
                             {
                                 Role = r.Name
                             };

            bool isCustomer = usersRoles.Any(r => r.Role.Equals(Domain.Constants.CustomerRole, StringComparison.InvariantCultureIgnoreCase));
            return isCustomer;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            //if (Request.IsAuthenticated)
            //{
            //    return RedirectToAction("Index", "Dashboard", new { @area = "admin" });
            //}
            // AuthenticationManager.User.Identity.GetUserId();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
            if (model == null)
            {
                throw new ArgumentException();
            }
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Model is not correct.");
                return View(model);
            }

            //validate the captcha through the session variable stored from GetCaptcha
            if (Session[CaptchaCustomerLogin] == null || !Session[CaptchaCustomerLogin].ToString().Equals(model.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("Captcha", AdminResource.WrongSum);
                ModelState.AddModelError("", AdminResource.WrongSum);
                return View(model);
            }
            else
            {
                bool isCustomer = this.isUserAsCustomerRole(model);
                if (!isCustomer)
                {
                    ModelState.AddModelError("", AdminResource.WrongAccountLoginAttempt);
                    return View(model);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

                Logger.Debug("The account " + model.Email + "   " + result.ToString());
                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToAction("Index", "Home", new { @area = "customers" });

                    case SignInStatus.LockedOut:
                        Logger.Debug("The account  " + model.Email + " LockedOut ");
                        ModelState.AddModelError("", "The account  " + model.Email + " LockedOut ");
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        Logger.Debug("The account  " + model.Email + " RequiresVerification ");
                        ModelState.AddModelError("", "The account  " + model.Email + " RequiresVerification ");
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    case SignInStatus.Failure:
                        var user = ApplicationDbContext.Users.FirstOrDefault(u => u.UserName.Equals(model.Email));
                        if (user != null)
                        {
                            bool checkPassword = SignInManager.UserManager.CheckPassword(user, model.Password);
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
                            ModelState.AddModelError("", Resource.NoUserFound);
                        }
                        return View(model);

                    default:
                        Logger.Debug("Invalid login attempt " + model.Email + " LockedOut ");
                        ModelState.AddModelError("", Resource.InvalidLoginAttempt);
                        return View(model);
                }
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new RegisterViewModel();
            model.IsPermissionGranted = true;
            return View(model);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Session[CaptchaCustomerRegister] == null || !Session[CaptchaCustomerRegister].ToString().Equals(model.Captcha, StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("Captcha", AdminResource.WrongSum);
                    ModelState.AddModelError("", AdminResource.WrongSum);
                    return View(model);
                }
                else
                {
                    // var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    var user = model.GetUser();
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        var emailTemplate = RazorEngineHelper.ConfirmYourAccountEmailBody(model.Email, model.FirstName + " " + model.LastName, callbackUrl);
                        await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);
                        IdentityManager.AddUserToRole(user.Id, Domain.Constants.CustomerRole);
                        CustomerService.SaveRegisterViewModel(user.Id, model);
                        IdentitySignout();
                        var result2 = await SignInManager.PasswordSignInAsync(model.Email, model.Password, false, shouldLockout: false);
                        switch (result2)
                        {
                            case SignInStatus.Success:
                                return RedirectToAction("Index", "Home", new { @area = "customers" });

                            case SignInStatus.LockedOut:
                                Logger.Debug("The account  " + model.Email + " LockedOut ");
                                ModelState.AddModelError("", "The account  " + model.Email + " LockedOut ");
                                return View("Lockout");

                            case SignInStatus.RequiresVerification:
                                Logger.Debug("The account  " + model.Email + " RequiresVerification ");
                                ModelState.AddModelError("", "The account  " + model.Email + " RequiresVerification ");
                                return View(model);

                            case SignInStatus.Failure:
                                user = ApplicationDbContext.Users.First(u => u.UserName.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase));
                                bool checkPassword = SignInManager.UserManager.CheckPassword(user, model.Password);
                                if (!checkPassword)
                                {
                                    ModelState.AddModelError("", "Invalid login attempt.Password is not correct");
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Invalid login attempt." + result.ToString());
                                }
                                return View(model);

                            default:
                                Logger.Debug("Invalid login attempt " + model.Email + " LockedOut ");
                                ModelState.AddModelError("", "Invalid login attempt.");
                                return View(model);
                        }
                    }
                    else
                    {
                        Logger.Error("No Successful Register:" + model);
                    }
                    AddErrors(result);
                }
            }
            else
            {
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public void IdentitySignout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie,
            DefaultAuthenticationTypes.ExternalCookie);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", Resource.NoUserFound);
                    return View("ForgotPassword");
                }
                if (!(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    ModelState.AddModelError("", Resource.UserEmailNotConfirmed);
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPassword");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { userId = user.Id, code = code },
                    protocol: Request.Url.Scheme);

                var emailTemplate = RazorEngineHelper.ForgotPasswordEmailBody(model.Email, callbackUrl);
                await UserManager.SendEmailAsync(user.Id, emailTemplate.Item1, emailTemplate.Item2);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string userId, string code)
        {
            if (code == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(userId);
            ResetPasswordViewModel model = new ResetPasswordViewModel();
            model.Email = user.Email;
            model.Code = code;
            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                case SignInStatus.Failure:

                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
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
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl = "")
        {
            bool isAdmin = User.IsInRole(Domain.Constants.AdministratorRole) || User.IsInRole(Domain.Constants.EditorRole);
            if (isAdmin)
            {
                return RedirectToAction("Index", "Dashboard", new { @area = "admin" });
            }
            else if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
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
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion Helpers
    }
}
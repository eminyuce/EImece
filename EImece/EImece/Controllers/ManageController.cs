using EImece.Web.Controllers;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using NLog; // Include the NLog namespace
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Resources;

using EImece.Domain.Services.IServices;

namespace EImece.Controllers
{
    [Authorize]
    public class ManageController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string IndexAction = "Index"; // Initialize NLog Logger

        public ApplicationSignInManager SignInManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        public ManageController(
            ISettingService settingService,
            AutoMapper.IMapper mapper,
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager)
            : base(settingService, mapper)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Legacy Identity template URL → Customers account home.
        /// </summary>
        // GET: /Manage/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            Logger.Debug("Legacy /Manage/ChangePassword redirected to Customers/Home/ChangePassword.");
            return RedirectToActionPermanent("ChangePassword", "Home", new { area = "Customers" });
        }

        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            Logger.Debug("ChangePassword action called.");

            if (!ModelState.IsValid)
            {
                Logger.Warn("ModelState is invalid for ChangePassword.");
                return View(model);
            }

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                Logger.Info("Password changed successfully.");

                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                return RedirectToAction(IndexAction, new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            AddErrors(result);
            Logger.Error("Failed to change password: {0}", string.Join(", ", result.Errors));
            return View(model);
        }

        /// <summary>
        /// Legacy Identity ManageLogins URL. External logins are managed from the customer account area.
        /// </summary>
        // GET: /Manage/ManageLogins
        [HttpGet]
        public ActionResult ManageLogins(ManageMessageId? message)
        {
            Logger.Debug("Legacy /Manage/ManageLogins redirected to Customers/Home/Index. Message={0}", message);
            return RedirectToActionPermanent(IndexAction, "Home", new { area = "Customers" });
        }

        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            Logger.Debug("Index action called with message: {0}", message);

            ViewBag.StatusMessage = GetIndexStatusMessage(message);

            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var model = new IndexViewModel
            {
                HasPassword = user != null && user.PasswordHash != null,
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                AuthenticatorEnabled = user != null && user.TwoFactorAuthenticatorEnabled,
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };

            return View(model);
        }

        // GET: /Manage/EnableAuthenticator
        public async Task<ActionResult> EnableAuthenticator()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.TwoFactorAuthenticatorEnabled)
            {
                TempData["StatusMessage"] = "İki faktörlü doğrulama zaten etkin.";
                return RedirectToAction(IndexAction);
            }

            if (string.IsNullOrEmpty(user.AuthenticatorKey))
            {
                user.AuthenticatorKey = AuthenticatorHelper.GenerateSecretKey();
                await UserManager.UpdateAsync(user);
            }

            return View(await BuildEnableAuthenticatorViewModelAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty(user.AuthenticatorKey))
            {
                ModelState.AddModelError("", Resource.AuthenticatorKeyNotFound);
                return View(await BuildEnableAuthenticatorViewModelAsync(user));
            }

            if (!ModelState.IsValid || !AuthenticatorHelper.VerifyCode(user.AuthenticatorKey, model?.Code))
            {
                ModelState.AddModelError("", Resource.InvalidVerificationCode);
                return View(await BuildEnableAuthenticatorViewModelAsync(user));
            }

            user.TwoFactorAuthenticatorEnabled = true;
            await UserManager.UpdateAsync(user);

            TempData["StatusMessage"] = Resource.TwoFactorEnabledSuccess;
            return RedirectToAction(IndexAction, new { Message = ManageMessageId.SetTwoFactorSuccess });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableAuthenticator()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return HttpNotFound();
            }

            user.TwoFactorAuthenticatorEnabled = false;
            user.AuthenticatorKey = null;
            await UserManager.UpdateAsync(user);

            TempData["StatusMessage"] = "İki faktörlü doğrulama kapatıldı.";
            return RedirectToAction(IndexAction);
        }

        private async Task<EnableAuthenticatorViewModel> BuildEnableAuthenticatorViewModelAsync(ApplicationUser user)
        {
            string accountName = !string.IsNullOrEmpty(user.Email) ? user.Email : user.UserName;
            string siteName = SettingService != null
                ? await SettingService.GetSettingByKeyAsync(Domain.Constants.CompanyName)
                : null;
            if (string.IsNullOrWhiteSpace(siteName) && Request?.Url != null)
            {
                siteName = Request.Url.Host;
            }

            string issuer = AuthenticatorHelper.NormalizeIssuer(siteName);
            string otpAuthUri = AuthenticatorHelper.GenerateOtpAuthUri(user.AuthenticatorKey, accountName, issuer);
            return new EnableAuthenticatorViewModel
            {
                SharedKey = AuthenticatorHelper.FormatKey(user.AuthenticatorKey),
                AuthenticatorUri = otpAuthUri,
                DisplayName = issuer + ":" + accountName,
                QrCodeImage = AuthenticatorHelper.GenerateQrCodeBase64(otpAuthUri)
            };
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            Logger.Debug("RemoveLogin action called with loginProvider: {0}, providerKey: {1}", loginProvider, providerKey);

            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                Logger.Info("Login removed successfully for provider: {0}", loginProvider);

                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                Logger.Error("Failed to remove login for provider: {0}", loginProvider);
                message = ManageMessageId.Error;
            }

            return RedirectToAction(IndexAction, "Home", new { area = "Customers", Message = message });
        }

        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            Logger.Debug("AddPhoneNumber action called. Number={0}", model.Number);

            if (!ModelState.IsValid)
            {
                Logger.Warn("ModelState is invalid for AddPhoneNumber.");
                return View(model);
            }

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
                Logger.Info("SMS sent to {0} with security code.", model.Number);
            }

            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            Logger.Debug("EnableTwoFactorAuthentication action called.");

            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                Logger.Info("Two-factor authentication enabled, user signed in.");
            }

            return RedirectToAction(IndexAction, "Manage");
        }

        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            Logger.Debug("DisableTwoFactorAuthentication action called.");

            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                Logger.Info("Two-factor authentication disabled, user signed in.");
            }

            return RedirectToAction(IndexAction, "Manage");
        }

        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            Logger.Debug("RemovePhoneNumber action called.");

            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                Logger.Error("Failed to remove phone number.");
                return RedirectToAction(IndexAction, new { Message = ManageMessageId.Error });
            }

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                Logger.Info("Phone number removed, user signed in.");
            }

            return RedirectToAction(IndexAction, new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        // Other methods follow the same pattern for adding logging...

        #region Helpers

        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private static string GetIndexStatusMessage(ManageMessageId? message)
        {
            if (message == ManageMessageId.ChangePasswordSuccess)
            {
                return "Your password has been changed.";
            }
            if (message == ManageMessageId.SetPasswordSuccess)
            {
                return "Your password has been set.";
            }
            if (message == ManageMessageId.SetTwoFactorSuccess)
            {
                return "Your two-factor authentication provider has been set.";
            }
            if (message == ManageMessageId.Error)
            {
                return "An error has occurred.";
            }
            if (message == ManageMessageId.AddPhoneSuccess)
            {
                return "Your phone number was added.";
            }
            if (message == ManageMessageId.RemovePhoneSuccess)
            {
                return "Your phone number was removed.";
            }

            return "";
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion Helpers
    }
}
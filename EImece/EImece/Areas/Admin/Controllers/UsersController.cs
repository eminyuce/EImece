using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using EImece.Domain.DependencyInjection;
using Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using static EImece.Controllers.ManageController;

namespace EImece.Areas.Admin.Controllers
{
    [DeleteAuthorize()]
    public class UsersController : BaseAdminController
    {
        private const string IndexAction = "Index";
        [Inject]
        public UsersService UsersService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public new ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "", String role = "", String twoFactor = "")
        {
            var staffUsers = (await UsersService.GetUsersAsync(string.Empty))
                .Where(r => !r.Role.Equals(Domain.Constants.CustomerRole, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(r => r.FirstName)
                .ToList();

            ViewBag.AvailableRoles = staffUsers
                .Select(r => r.Role)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(r => r)
                .ToList();

            IEnumerable<EditUserViewModel> query = staffUsers;
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = (await UsersService.GetUsersAsync(search))
                    .Where(r => !r.Role.Equals(Domain.Constants.CustomerRole, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(r => r.FirstName);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(r => r.Role.Equals(role.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }

            if (string.Equals(twoFactor, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(twoFactor, "1", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.AuthenticatorEnabled);
            }
            else if (string.Equals(twoFactor, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(twoFactor, "0", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => !r.AuthenticatorEnabled);
            }

            ViewBag.Search = search ?? string.Empty;
            ViewBag.Role = role ?? string.Empty;
            ViewBag.TwoFactor = twoFactor ?? string.Empty;

            return View(query.ToList());
        }

        public async Task<ActionResult> CustomerRoles(CancellationToken cancellationToken, String search = "")
        {
            List<EditUserViewModel> model = await UsersService.GetUsersAsync(search);
            model = model.Where(r => r.Role.Equals(Domain.Constants.CustomerRole, StringComparison.InvariantCultureIgnoreCase)).OrderBy(r => r.FirstName).ToList();

            var userIds = model.Select(m => m.Id).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            var customers = userIds.Count == 0
                ? new List<Customer>()
                : (await CustomerService.GetAllAsync())
                    .Where(c => !string.IsNullOrWhiteSpace(c.UserId) && userIds.Contains(c.UserId))
                    .ToList();
            var customerByUserId = customers
                .GroupBy(c => c.UserId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.UpdatedDate).First(), StringComparer.OrdinalIgnoreCase);

            foreach (var item in model)
            {
                EnrichCustomerDetailNote(item, customerByUserId);
            }

            return View(model);
        }

        private static void EnrichCustomerDetailNote(EditUserViewModel item, IDictionary<string, Customer> customerByUserId)
        {
            if (item == null)
            {
                return;
            }

            item.DetailNote = null;
            Customer customer = null;
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                customerByUserId.TryGetValue(item.Id, out customer);
            }

            if (customer == null)
            {
                item.AppendDetailLine(AdminResource.Roles, item.Role);
                item.AppendDetailBlock("Müşteri profil kaydı bulunamadı.");
                return;
            }

            item.AppendDetailLine(AdminResource.PhoneNumber, customer.GsmNumber);
            item.AppendDetailLine(AdminResource.Company, customer.Company);
            item.AppendDetailLine(AdminResource.IdentityNumber, customer.IdentityNumber);
            item.AppendDetailLine(AdminResource.City, customer.City);
            item.AppendDetailLine(AdminResource.Town, customer.Town);
            item.AppendDetailLine(AdminResource.District, customer.District);
            item.AppendDetailLine(AdminResource.Country, customer.Country);
            item.AppendDetailLine(AdminResource.ZipCode, customer.ZipCode);
            item.AppendDetailLine(AdminResource.CustomerOpenAddress, customer.Street);

            if (!string.IsNullOrWhiteSpace(customer.Description))
            {
                item.AppendDetailLine("Açıklama", customer.Description);
            }

            if (customer.CreatedDate != default(DateTime))
            {
                item.AppendDetailLine(AdminResource.CreatedDate, customer.CreatedDate.ToString("dd.MM.yyyy HH:mm"));
            }

            if (customer.UpdatedDate != default(DateTime))
            {
                item.AppendDetailLine(AdminResource.UpdatedDate, customer.UpdatedDate.ToString("dd.MM.yyyy HH:mm"));
            }

            if (string.IsNullOrWhiteSpace(item.DetailNote))
            {
                item.AppendDetailBlock("Ek müşteri bilgisi yok.");
            }
        }

        //[Authorize(Roles = "Admin")]
        public ActionResult Register()
        {
            var m = new RegisterViewModelForAdmin();
            return View(m);
        }

        [HttpPost]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModelForAdmin model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            if (ModelState.IsValid)
            {
                var user = model.GetUser();
                user.EmailConfirmed = true;
                var result = await UsersService.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    ///users/userroles/22dc301a-4661-4269-b5ba-88a5420bbcfa/
                    return RedirectToAction("userroles", "Users", new { id = user.Id });
                }
                else
                {
                    ModelState.AddModelError("", String.Join(", ", result.Errors.ToArray()));
                }
            }
            else
            {
                ModelState.AddModelError("", AdminResource.RequestIsNotValid);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<ActionResult> Edit(CancellationToken cancellationToken, string id, ManageMessageId? Message = null)
        {
            var user = await UsersService.GetUserAsync(id);
            var model = new EditUserViewModel();
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Email = user.Email;
            model.Id = user.Id;
            ViewBag.MessageId = Message;
            ViewBag.AuthenticatorEnabled = user.TwoFactorAuthenticatorEnabled;
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await ApplicationDbContext.Users.FirstAsync(u => u.Id == model.Id);
                // Update the user data:
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;

                ApplicationDbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                await ApplicationDbContext.SaveChangesAsync();
                return RedirectToAction(IndexAction);
            }
            else
            {
                // ModelState.AddModelError("", result.Errors.ToList().FirstOrDefault());
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<ActionResult> GenerateNewPassword(CancellationToken cancellationToken, string id = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.PasswordGenerated = false;
            return View(BuildEditUserViewModel(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateNewPasswordConfirm(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            if (user == null)
            {
                return HttpNotFound();
            }

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            string newPassword = GeneralHelper.GenerateRandomPassword();
            var result = await UserManager.ResetPasswordAsync(user.Id, code, newPassword);

            ViewBag.PasswordGenerated = true;
            if (result.Succeeded)
            {
                ViewBag.GenerationSucceeded = true;
                ViewBag.NewPassword = newPassword;
            }
            else
            {
                ViewBag.GenerationSucceeded = false;
                ViewBag.NewPassword = null;
                AddErrors(result);
            }

            return View("GenerateNewPassword", BuildEditUserViewModel(user));
        }

        private static EditUserViewModel BuildEditUserViewModel(ApplicationUser user)
        {
            return new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, string id)
        {
            var user = await ApplicationDbContext.Users.FirstAsync(u => u.Id == id, cancellationToken);
            ApplicationDbContext.Users.Remove(user);
            await ApplicationDbContext.SaveChangesAsync(cancellationToken);
            SetSuccessMessage();
            return ReturnIndexIfNotUrlReferrer(IndexAction);
        }

        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public async Task<ActionResult> UserRoles(CancellationToken cancellationToken, string id)
        {
            var user = await ApplicationDbContext.Users.FirstAsync(u => u.Id == id, cancellationToken);
            var model = new SelectUserRolesViewModel(user);
            model.SetAdminRoles(user);
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserRoles(SelectUserRolesViewModel model)
        {
            var user = await ApplicationDbContext.Users.FirstAsync(u => u.Id == model.Id);
            IdentityManager.ClearUserRoles(user.Id);
            foreach (var role in model.Roles)
            {
                if (role.Selected)
                {
                    IdentityManager.AddUserToRole(user.Id, role.RoleName);
                }
            }
            SetSuccessMessage();
            return RedirectToAction("index");
            // }
            // return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(CancellationToken cancellationToken, String id = "")
        {
            var model = new ForgotPasswordViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                var user = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Prefer Email; fall back to UserName for older accounts.
                model.Email = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : user.UserName;
                ViewBag.UserId = user.Id;
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName = user.LastName;
                ViewBag.HasTargetUser = true;
            }
            else
            {
                ViewBag.HasTargetUser = false;
            }

            return View(model);
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
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Users", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Users");
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
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
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
                return RedirectToAction("ResetPasswordConfirmation", "Users");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Users");
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

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        //
        // GET: /Manage/ChangePassword
        public async Task<ActionResult> ChangePassword()
        {
            var userId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(IndexAction, "Dashboard", new { area = "admin" });
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction(IndexAction, "Dashboard", new { area = "admin" });
            }
            ViewBag.CurrentUser = user;
            ViewBag.AuthenticatorEnabled = user.TwoFactorAuthenticatorEnabled;
            return View();
        }

        /// <summary>
        /// Setup TOTP authenticator for the currently logged-in admin (QR + confirm code).
        /// </summary>
        public async Task<ActionResult> EnableAuthenticator()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return HttpNotFound();
            }

            if (user.TwoFactorAuthenticatorEnabled)
            {
                SetSuccessMessage("İki faktörlü doğrulama zaten etkin.");
                return RedirectToAction("ChangePassword");
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
                ModelState.AddModelError("", "Authenticator anahtarı bulunamadı. Lütfen sayfayı yenileyin.");
                return View(await BuildEnableAuthenticatorViewModelAsync(user));
            }

            if (!ModelState.IsValid || !AuthenticatorHelper.VerifyCode(user.AuthenticatorKey, model?.Code))
            {
                ModelState.AddModelError("", "Geçersiz doğrulama kodu.");
                return View(await BuildEnableAuthenticatorViewModelAsync(user));
            }

            user.TwoFactorAuthenticatorEnabled = true;
            await UserManager.UpdateAsync(user);

            SetSuccessMessage("İki faktörlü doğrulama başarıyla etkinleştirildi.");
            return RedirectToAction("ChangePassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableAuthenticator(string id = null)
        {
            // Own account, or admin disabling another user's authenticator.
            string targetUserId = string.IsNullOrEmpty(id) ? User.Identity.GetUserId() : id;
            bool isSelf = string.Equals(targetUserId, User.Identity.GetUserId(), StringComparison.OrdinalIgnoreCase);
            if (!isSelf && !User.IsInRole(Domain.Constants.AdministratorRole))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var user = await UserManager.FindByIdAsync(targetUserId);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.TwoFactorAuthenticatorEnabled = false;
            user.AuthenticatorKey = null;
            await UserManager.UpdateAsync(user);

            SetSuccessMessage("İki faktörlü doğrulama kapatıldı.");
            if (isSelf)
            {
                return RedirectToAction("ChangePassword");
            }

            return RedirectToAction("Edit", new { id = targetUserId });
        }

        private async Task<EnableAuthenticatorViewModel> BuildEnableAuthenticatorViewModelAsync(ApplicationUser user)
        {
            string accountName = !string.IsNullOrEmpty(user.Email) ? user.Email : user.UserName;
            string siteName = await SettingService.GetSettingByKeyAsync(Domain.Constants.CompanyName);
            if (string.IsNullOrWhiteSpace(siteName) && Request?.Url != null)
            {
                siteName = Request.Url.Host;
            }

            string issuer = AuthenticatorHelper.NormalizeIssuer(siteName);
            string otpAuthUri = AuthenticatorHelper.GenerateOtpAuthUri(
                user.AuthenticatorKey ?? string.Empty,
                accountName,
                issuer);

            return new EnableAuthenticatorViewModel
            {
                SharedKey = AuthenticatorHelper.FormatKey(user.AuthenticatorKey),
                AuthenticatorUri = otpAuthUri,
                DisplayName = issuer + ":" + accountName,
                QrCodeImage = string.IsNullOrEmpty(user.AuthenticatorKey)
                    ? null
                    : AuthenticatorHelper.GenerateQrCodeBase64(otpAuthUri)
            };
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Message", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            var user2 = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.CurrentUser = user2;
            return View(model);
        }

        public ActionResult Message(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();

            return View();
        }
    }
}
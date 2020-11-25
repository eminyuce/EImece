using EImece.Domain.DbContext;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Ninject;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static EImece.Controllers.ManageController;

namespace EImece.Areas.Admin.Controllers
{
    [DeleteAuthorize()]
    public class UsersController : BaseAdminController
    {
        [Inject]
        public UsersService UsersService { get; set; }

        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        public ActionResult Index(String search = "")
        {
            List<EditUserViewModel> model = UsersService.GetUsers(search);
            model = model.Where(r => !r.Role.Equals(Domain.Constants.CustomerRole, StringComparison.InvariantCultureIgnoreCase)).OrderBy(r => r.FirstName).ToList();
            return View(model);
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

        public ActionResult Edit(string id, ManageMessageId? Message = null)
        {
            var user = UsersService.GetUser(id);
            var model = new EditUserViewModel();
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Email = user.Email;
            model.Id = user.Id;
            ViewBag.MessageId = Message;
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = ApplicationDbContext.Users.First(u => u.Id == model.Id);
                // Update the user data:
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;

                ApplicationDbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                await ApplicationDbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                // ModelState.AddModelError("", result.Errors.ToList().FirstOrDefault());
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult GenerateNewPassword(string id = null)
        {
            var user = ApplicationDbContext.Users.First(u => u.Id == id);
            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            string code = UserManager.GeneratePasswordResetToken(user.Id);
            String newPassWord = GeneralHelper.GenerateRandomPassword();
            var result = UserManager.ResetPassword(user.Id, code, newPassWord);

            if (result.Succeeded)
            {
                ViewBag.NewPassword = newPassWord;
            }
            else
            {
                ViewBag.NewPassword = result.Errors.ToArray()[0].ToStr() + " ERROR is occured while generating the new password for user:" + user.Email;
            }
            AddErrors(result);
            var model = new EditUserViewModel();
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Email = user.Email;
            model.Id = user.Id;

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = ApplicationDbContext.Users.First(u => u.Id == id);
            ApplicationDbContext.Users.Remove(user);
            ApplicationDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        public ActionResult UserRoles(string id)
        {
            var user = ApplicationDbContext.Users.First(u => u.Id == id);
            var model = new SelectUserRolesViewModel(user);
            model.SetAdminRoles(user);
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(Domain.Constants.AdministratorRole)]
        [ValidateAntiForgeryToken]
        public ActionResult UserRoles(SelectUserRolesViewModel model)
        {
            // if (ModelState.IsValid)
            // {
            var user = ApplicationDbContext.Users.First(u => u.Id == model.Id);
            IdentityManager.ClearUserRoles(user.Id);
            foreach (var role in model.Roles)
            {
                if (role.Selected)
                {
                    IdentityManager.AddUserToRole(user.Id, role.RoleName);
                }
            }
            return RedirectToAction("index");
            // }
            // return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword(String id = "")
        {
            var m = new ForgotPasswordViewModel();
            if (!String.IsNullOrEmpty(id))
            {
                var user = ApplicationDbContext.Users.First(u => u.Id == id);
                m.Email = user.UserName;
            }
            return View(m);
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
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            ViewBag.CurrentUser = user;
            return View();
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
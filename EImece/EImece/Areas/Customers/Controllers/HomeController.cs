﻿using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using Resources;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static EImece.Controllers.ManageController;

namespace EImece.Areas.Customers.Controllers
{
    [AuthorizeRoles(Domain.Constants.CustomerRole)]
    public class HomeController : Controller
    {
        private const string CaptchaCustomerEdit = "CaptchaCustomerEdit";

        [Inject]
        public IAuthenticationManager AuthenticationManager { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        public ApplicationUserManager UserManager { get; set; }

        public HomeController(ApplicationUserManager userManager)
        {
            this.UserManager = userManager;
        }

        // GET: Customers/Home
        public ActionResult Index()
        {
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var customer = CustomerService.GetUserId(user.Id);
            return View(customer);
        }

        [HttpPost]
        public ActionResult Index(Customer customer)
        {
            if (customer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (Session[CaptchaCustomerEdit] == null || !Session[CaptchaCustomerEdit].ToString().Equals(customer.Captcha, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("Captcha", Resources.Resource.ContactUsWrongSumForSecurityQuestion);
                return View("Index", customer);
            }
            else
            {
                var user = UserManager.FindByName(User.Identity.GetUserName());
                customer.UserId = user.Id;
                customer.Ip = GeneralHelper.GetIpAddress();
                customer =  CustomerService.SaveOrEditEntity(customer);
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                return View(customer);
            }
        }

        public ActionResult SendMessageToSeller()
        {
            return View();
        }

        public ActionResult CustomerOrders(string search = "")
        {
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var orders = OrderService.GetOrdersUserId(user.Id, search);
            return View(orders);
        }

        public ActionResult OrderDetail(string id)
        {
            var user = UserManager.FindByName(User.Identity.GetUserName());
            var order = OrderService.GetByOrderGuid(id);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home", new { @area = "" });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
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
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}
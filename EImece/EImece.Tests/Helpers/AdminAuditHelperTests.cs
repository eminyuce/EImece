using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class AdminAuditHelperTests
    {
        [TestMethod]
        public void FormatAuditDate_ValidDate_ShouldFormatAccordingToCulture()
        {
            var testDate = new DateTime(2026, 8, 26, 14, 30, 0);

            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                // Turkish culture
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                var trResult = AdminAuditHelper.FormatAuditDate(testDate);
                Assert.IsFalse(string.IsNullOrWhiteSpace(trResult));
                Assert.IsTrue(trResult.Contains("26") || trResult.Contains("2026"));

                // US English culture
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                var enResult = AdminAuditHelper.FormatAuditDate(testDate);
                Assert.IsFalse(string.IsNullOrWhiteSpace(enResult));
                Assert.IsTrue(enResult.Contains("26") || enResult.Contains("8") || enResult.Contains("2026"));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [TestMethod]
        public void FormatAuditDate_DefaultDate_ShouldReturnDash()
        {
            var result = AdminAuditHelper.FormatAuditDate(default(DateTime));
            Assert.AreEqual("-", result);
        }

        [TestMethod]
        public void GetUserDisplayName_EmptyOrNull_ShouldReturnUnknown()
        {
            var resultNull = AdminAuditHelper.GetUserDisplayName(null);
            var resultEmpty = AdminAuditHelper.GetUserDisplayName("");
            var resultWhitespace = AdminAuditHelper.GetUserDisplayName("   ");

            Assert.IsFalse(string.IsNullOrWhiteSpace(resultNull));
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultEmpty));
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultWhitespace));
        }

        [TestMethod]
        public void UserRoleHelper_IsDeletedEnableRoles_EditorRole_ShouldReturnFalse()
        {
            var identity = new GenericIdentity("editorUser");
            var principal = new GenericPrincipal(identity, new[] { Constants.EditorRole });

            var httpContext = new HttpContext(
                new HttpRequest(null, "http://localhost/", null),
                new HttpResponse(null)
            )
            {
                User = principal
            };

            HttpContext.Current = httpContext;

            try
            {
                var isEnabled = UserRoleHelper.IsDeletedEnableRoles();
                Assert.IsFalse(isEnabled, "Editor role must NOT be allowed to delete records");
            }
            finally
            {
                HttpContext.Current = null;
            }
        }

        [TestMethod]
        public void UserRoleHelper_IsDeletedEnableRoles_AdminRole_ShouldReturnTrue()
        {
            var identity = new GenericIdentity("adminUser");
            var principal = new GenericPrincipal(identity, new[] { Constants.AdministratorRole });

            var httpContext = new HttpContext(
                new HttpRequest(null, "http://localhost/", null),
                new HttpResponse(null)
            )
            {
                User = principal
            };

            HttpContext.Current = httpContext;

            try
            {
                var isEnabled = UserRoleHelper.IsDeletedEnableRoles();
                Assert.IsTrue(isEnabled, "Administrator role must be allowed to delete records");
            }
            finally
            {
                HttpContext.Current = null;
            }
        }

        [TestMethod]
        public void DeleteAuthorizeAttribute_EditorRole_ShouldDenyAccess()
        {
            var identity = new GenericIdentity("editorUser");
            var principal = new GenericPrincipal(identity, new[] { Constants.EditorRole });

            var httpContext = new FakeHttpContext(principal);
            var controllerContext = new ControllerContext(httpContext, new RouteData(), new FakeController());
            var actionDescriptor = new FakeActionDescriptor();
            var authContext = new AuthorizationContext(controllerContext, actionDescriptor);

            var attr = new DeleteAuthorizeAttribute();
            attr.OnAuthorization(authContext);

            Assert.IsNotNull(authContext.Result, "Editor role must be denied access by DeleteAuthorizeAttribute");
            var redirectResult = authContext.Result as RedirectToRouteResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Error", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("BadRequest", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void DeleteAuthorizeAttribute_AdminRole_ShouldAllowAccess()
        {
            var identity = new GenericIdentity("adminUser");
            var principal = new GenericPrincipal(identity, new[] { Constants.AdministratorRole });

            var httpContext = new FakeHttpContext(principal);
            var controllerContext = new ControllerContext(httpContext, new RouteData(), new FakeController());
            var actionDescriptor = new FakeActionDescriptor();
            var authContext = new AuthorizationContext(controllerContext, actionDescriptor);

            var attr = new DeleteAuthorizeAttribute();
            attr.OnAuthorization(authContext);

            Assert.IsNull(authContext.Result, "Administrator role must be allowed access by DeleteAuthorizeAttribute");
        }

        private class FakeHttpContext : HttpContextBase
        {
            private readonly IPrincipal _user;
            private readonly HttpRequestBase _request;

            public FakeHttpContext(IPrincipal user)
            {
                _user = user;
                _request = new FakeHttpRequest();
            }

            public override IPrincipal User => _user;
            public override HttpRequestBase Request => _request;
        }

        private class FakeHttpRequest : HttpRequestBase
        {
            public override bool IsAuthenticated => true;
        }

        private class FakeController : ControllerBase
        {
            protected override void ExecuteCore() { }
        }

        private class FakeActionDescriptor : ActionDescriptor
        {
            public override string ActionName => "Delete";
            public override ControllerDescriptor ControllerDescriptor => new FakeControllerDescriptor();
            public override object Execute(ControllerContext controllerContext, System.Collections.Generic.IDictionary<string, object> parameters) => null;
            public override ParameterDescriptor[] GetParameters() => new ParameterDescriptor[0];
        }

        private class FakeControllerDescriptor : ControllerDescriptor
        {
            public override Type ControllerType => typeof(FakeController);
            public override string ControllerName => "Fake";
            public override ActionDescriptor FindAction(ControllerContext controllerContext, string actionName) => new FakeActionDescriptor();
            public override ActionDescriptor[] GetCanonicalActions() => new ActionDescriptor[0];
        }
    }
}

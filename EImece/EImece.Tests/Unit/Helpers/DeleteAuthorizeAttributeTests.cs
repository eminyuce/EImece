using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EImece.Domain;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Tests.Unit.Helpers
{
    [TestClass]
    public class DeleteAuthorizeAttributeTests
    {
        [TestMethod]
        public void OnAuthorization_WhenAuthenticatedWithoutAdminRole_SetsBadRequestRedirect()
        {
            var identity = new GenericIdentity("editor@test.com");
            var principal = new GenericPrincipal(identity, new[] { "Editor" });
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.User).Returns(principal);
            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.IsAuthenticated).Returns(true);
            httpContext.Setup(c => c.Request).Returns(request.Object);

            var filterContext = new AuthorizationContext
            {
                HttpContext = httpContext.Object
            };

            new DeleteAuthorizeAttribute().OnAuthorization(filterContext);

            Assert.IsInstanceOfType(filterContext.Result, typeof(RedirectToRouteResult));
            var redirect = (RedirectToRouteResult)filterContext.Result;
            Assert.AreEqual("Error", redirect.RouteValues["controller"]);
            Assert.AreEqual("BadRequest", redirect.RouteValues["action"]);
        }

        [TestMethod]
        public void OnAuthorization_WhenAuthenticatedAsAdmin_LeavesResultNull()
        {
            var identity = new GenericIdentity("admin@test.com");
            var principal = new GenericPrincipal(identity, new[] { Constants.AdministratorRole });
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(c => c.User).Returns(principal);
            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.IsAuthenticated).Returns(true);
            httpContext.Setup(c => c.Request).Returns(request.Object);

            var filterContext = new AuthorizationContext
            {
                HttpContext = httpContext.Object
            };

            new DeleteAuthorizeAttribute().OnAuthorization(filterContext);

            Assert.IsNull(filterContext.Result);
        }

        [TestMethod]
        public void OnAuthorization_WhenAnonymous_DoesNotSetResult()
        {
            var httpContext = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.IsAuthenticated).Returns(false);
            httpContext.Setup(c => c.Request).Returns(request.Object);

            var filterContext = new AuthorizationContext
            {
                HttpContext = httpContext.Object
            };

            new DeleteAuthorizeAttribute().OnAuthorization(filterContext);

            Assert.IsNull(filterContext.Result);
        }
    }
}

using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EImece.Integration.Tests.Auth
{
    [TestClass]
    public class DeleteAuthorizeIntegrationTests
    {
        [TestMethod]
        public void DeleteAuthorize_WhenUserLacksRequiredRole_RedirectsToBadRequest()
        {
            var attr = new DeleteAuthorizeAttribute();
            var identity = new GenericIdentity("editor@test.com");
            var principal = new GenericPrincipal(identity, new[] { "Editor" });

            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.IsAuthenticated).Returns(true);
            var http = new Mock<HttpContextBase>();
            http.Setup(c => c.Request).Returns(request.Object);
            http.Setup(c => c.User).Returns(principal);

            var filterContext = new AuthorizationContext(
                new ControllerContext
                {
                    HttpContext = http.Object,
                    RouteData = new RouteData()
                },
                new Mock<ActionDescriptor>().Object);

            attr.OnAuthorization(filterContext);

            Assert.IsInstanceOfType(filterContext.Result, typeof(RedirectToRouteResult));
        }

        [TestMethod]
        public void DeleteAuthorize_WhenUserInAllDeleteRoles_AllowsRequest()
        {
            var roles = EImece.Domain.Helpers.UserRoleHelper.GetDeletedRoles();
            var attr = new DeleteAuthorizeAttribute();
            var identity = new GenericIdentity("admin@test.com");
            var principal = new GenericPrincipal(identity, roles);

            var request = new Mock<HttpRequestBase>();
            request.Setup(r => r.IsAuthenticated).Returns(true);
            var http = new Mock<HttpContextBase>();
            http.Setup(c => c.Request).Returns(request.Object);
            http.Setup(c => c.User).Returns(principal);

            var filterContext = new AuthorizationContext(
                new ControllerContext
                {
                    HttpContext = http.Object,
                    RouteData = new RouteData()
                },
                new Mock<ActionDescriptor>().Object);

            attr.OnAuthorization(filterContext);
            Assert.IsNull(filterContext.Result);
        }
    }
}

using EImece.Domain;
using EImece.Web.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class UnderConstAttributeTests
    {
        private class TestHttpContext : HttpContextBase
        {
            public IPrincipal CustomUser { get; set; }
            public override IPrincipal User
            {
                get => CustomUser;
                set => CustomUser = value;
            }

            public override HttpRequestBase Request => new TestHttpRequest();
            public override HttpResponseBase Response => new TestHttpResponse();
            public override HttpServerUtilityBase Server => new TestHttpServerUtility();
        }

        private class TestHttpRequest : HttpRequestBase
        {
            public override string UserHostAddress => "127.0.0.1";
        }

        private class TestHttpResponse : HttpResponseBase
        {
        }

        private class TestHttpServerUtility : HttpServerUtilityBase
        {
            public override string MapPath(string path) => @"C:\fake\App_Data\Offline.txt";
        }

        private class DummyController : ControllerBase
        {
            protected override void ExecuteCore() { }
        }

        private class DummyActionDescriptor : ActionDescriptor
        {
            private readonly string _actionName;
            private readonly ControllerDescriptor _controllerDescriptor;

            public DummyActionDescriptor(string actionName, ControllerDescriptor controllerDescriptor)
            {
                _actionName = actionName;
                _controllerDescriptor = controllerDescriptor;
            }

            public override string ActionName => _actionName;
            public override ControllerDescriptor ControllerDescriptor => _controllerDescriptor;
            public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters) => null;
            public override ParameterDescriptor[] GetParameters() => new ParameterDescriptor[0];
        }

        private class DummyControllerDescriptor : ControllerDescriptor
        {
            private readonly string _controllerName;

            public DummyControllerDescriptor(string controllerName)
            {
                _controllerName = controllerName;
            }

            public override string ControllerName => _controllerName;
            public override System.Type ControllerType => typeof(DummyController);
            public override ActionDescriptor FindAction(ControllerContext controllerContext, string actionName) => null;
            public override ActionDescriptor[] GetCanonicalActions() => new ActionDescriptor[0];
        }

        [TestMethod]
        public void OnActionExecuting_WhenAdminArea_AllowsAccessEvenWhenUnderConstruction()
        {
            // Arrange
            var filter = new UnderConstAttribute();
            var httpContext = new TestHttpContext();
            var routeData = new RouteData();
            routeData.DataTokens["area"] = "Admin";

            var controllerDesc = new DummyControllerDescriptor("Dashboard");
            var actionDesc = new DummyActionDescriptor("Index", controllerDesc);

            var controllerContext = new ControllerContext(httpContext, routeData, new DummyController());
            var actionExecutingContext = new ActionExecutingContext(controllerContext, actionDesc, new Dictionary<string, object>());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsNull(actionExecutingContext.Result, "Admin area requests should not be redirected by UnderConstAttribute.");
        }

        [TestMethod]
        public void OnActionExecuting_WhenAdminLoginAction_AllowsAccessEvenWhenUnderConstruction()
        {
            // Arrange
            var filter = new UnderConstAttribute();
            var httpContext = new TestHttpContext();
            var routeData = new RouteData();

            var controllerDesc = new DummyControllerDescriptor("Account");
            var actionDesc = new DummyActionDescriptor("AdminLogin", controllerDesc);

            var controllerContext = new ControllerContext(httpContext, routeData, new DummyController());
            var actionExecutingContext = new ActionExecutingContext(controllerContext, actionDesc, new Dictionary<string, object>());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsNull(actionExecutingContext.Result, "Account/AdminLogin should not be redirected by UnderConstAttribute.");
        }

        [TestMethod]
        public void OnActionExecuting_WhenUserIsAdministrator_AllowsAccessEvenWhenUnderConstruction()
        {
            // Arrange
            var filter = new UnderConstAttribute();
            var httpContext = new TestHttpContext();

            var identity = new ClaimsIdentity("ApplicationCookie");
            var principal = new GenericPrincipal(identity, new[] { Constants.AdministratorRole });
            httpContext.CustomUser = principal;

            var routeData = new RouteData();
            var controllerDesc = new DummyControllerDescriptor("Home");
            var actionDesc = new DummyActionDescriptor("Index", controllerDesc);

            var controllerContext = new ControllerContext(httpContext, routeData, new DummyController());
            var actionExecutingContext = new ActionExecutingContext(controllerContext, actionDesc, new Dictionary<string, object>());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsNull(actionExecutingContext.Result, "Authenticated Administrator should not be redirected.");
        }

        [TestMethod]
        public void OnActionExecuting_WhenUserIsEditor_AllowsAccessEvenWhenUnderConstruction()
        {
            // Arrange
            var filter = new UnderConstAttribute();
            var httpContext = new TestHttpContext();

            var identity = new ClaimsIdentity("ApplicationCookie");
            var principal = new GenericPrincipal(identity, new[] { Constants.EditorRole });
            httpContext.CustomUser = principal;

            var routeData = new RouteData();
            var controllerDesc = new DummyControllerDescriptor("Products");
            var actionDesc = new DummyActionDescriptor("Index", controllerDesc);

            var controllerContext = new ControllerContext(httpContext, routeData, new DummyController());
            var actionExecutingContext = new ActionExecutingContext(controllerContext, actionDesc, new Dictionary<string, object>());

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.IsNull(actionExecutingContext.Result, "Authenticated Editor should not be redirected.");
        }
    }
}

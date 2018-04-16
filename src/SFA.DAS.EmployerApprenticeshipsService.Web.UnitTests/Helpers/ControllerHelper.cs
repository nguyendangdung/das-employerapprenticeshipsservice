using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using SFA.DAS.EAS.Web.Helpers;

namespace SFA.DAS.EAS.Web.UnitTests.Helpers
{
    internal static class ControllerHelper
    {
        public static void SetupWithHttpContextAndUrlHelper(this Controller controller, string baseUrl,
            RouteCollection routes)
        {
            //setup request
            var requestContextMock = new Mock<HttpRequestBase>();
            requestContextMock.Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns("/");
            requestContextMock.Setup(r => r.ApplicationPath).Returns(baseUrl);

            //setup response
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(s => s.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(s => s);

            //setup context with request and response
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(h => h.Request).Returns(requestContextMock.Object);
            httpContextMock.Setup(h => h.Response).Returns(responseMock.Object);

            controller.ControllerContext =
                new ControllerContext(httpContextMock.Object, new RouteData(), controller);

            controller.Url = new UrlHelper(new RequestContext(httpContextMock.Object, new RouteData()), routes);
        }

        public static string SetUpTransfersIndexRoute(Controller controller)
        {
            const string baseUrl = "anyurl/";
            const string routeUrl = "Transfers/Index";

            var routes = new RouteCollection();
            routes.MapRoute(
                "TransfersDashboard",
                routeUrl,
                new
                {
                    controller = ControllerConstants.TransfersControllerName,
                    action = ControllerConstants.TransfersActionName
                });

            controller.SetupWithHttpContextAndUrlHelper(baseUrl, routes);

            return $"{baseUrl}{routeUrl}";
        }
    }
}
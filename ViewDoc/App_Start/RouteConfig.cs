using System.Web.Mvc;
using System.Web.Routing;

namespace DocumentViewer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {                        
            routes.MapRoute(
                name: "Default",
                url: "{action}/{docId}",
                defaults: new { controller = "DocumentViewer", action = "View", docId = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "View",
                url: "{docId}",
                defaults: new { controller = "DocumentViewer", action = "View", docId = UrlParameter.Optional }
            );
        }
    }
}
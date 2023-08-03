using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebNails
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //routes.MapMvcAttributeRoutes();

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute("", "",
               new
               {
                   controller = "Home",
                   action = "Index"
               });

            routes.MapRoute(
                "Index", "index.html",
                new
                {
                    controller = "Home",
                    action = "Index"
                });

            routes.MapRoute(
                "Contact", "contact.html",
                new
                {
                    controller = "Home",
                    action = "Contact"
                });

            routes.MapRoute(
                "About", "about-us.html",
                new
                {
                    controller = "Home",
                    action = "About"
                });

            routes.MapRoute(
                "Services", "services.html",
                new
                {
                    controller = "Home",
                    action = "Services"
                });

            routes.MapRoute(
                "Gifts", "e-gift.html",
                new
                {
                    controller = "Home",
                    action = "Gifts"
                });

            routes.MapRoute(
                "Prices", "price-list.html",
                new
                {
                    controller = "Home",
                    action = "Prices"
                });

            routes.MapRoute(
                "Payment", "payment.html",
                new
                {
                    controller = "Home",
                    action = "Payment"
                });

            routes.MapRoute(
                "Process", "process.html",
                new
                {
                    controller = "Home",
                    action = "Process"
                });

            routes.MapRoute(
                "Finish", "finish.html",
                new
                {
                    controller = "Home",
                    action = "Finish"
                });
        }
    }
}

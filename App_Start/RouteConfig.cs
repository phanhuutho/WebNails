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
                "Gallery", "gallery.html",
                new
                {
                    controller = "Home",
                    action = "Gallery"
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
                "PaymentResponse", "payment-response.html",
                new
                {
                    controller = "Home",
                    action = "PaymentResponse"
                });

            routes.MapRoute(
                "Finish", "finish.html",
                new
                {
                    controller = "Home",
                    action = "Finish"
                });

            routes.MapRoute(
                "Login", "login.html",
                new
                {
                    controller = "Home",
                    action = "Login"
                });

            routes.MapRoute(
                "GiftManage", "gift-manage.html",
                new
                {
                    controller = "Home",
                    action = "GiftManage"
                });

            routes.MapRoute(
                "Logout", "logout.html",
                new
                {
                    controller = "Home",
                    action = "Logout"
                });

            routes.MapRoute(
                "GetGiftManage", "get-gift-manage.html",
                new
                {
                    controller = "Home",
                    action = "GetGiftManage"
                });

            routes.MapRoute(
                "UpdateCompleted", "update-complete.html",
                new
                {
                    controller = "Home",
                    action = "UpdateCompleted"
                });

            routes.MapRoute(
                "SendMail", "send-mail.html",
                new
                {
                    controller = "Home",
                    action = "SendMail"
                });

        }
    }
}

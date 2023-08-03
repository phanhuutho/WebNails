using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace WebNails.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Services()
        {
            return View();
        }

        public ActionResult Gifts()
        {
            return View();
        }

        public ActionResult Prices()
        {
            return View();
        }

        public ActionResult Payment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Process(string amount, string stock, string email, string message)
        {
            var EmailPaypal = System.Web.Configuration.WebConfigurationManager.AppSettings["EmailPaypal"];
            ViewBag.EmailPaypal = EmailPaypal ?? "";
            ViewBag.Amount = amount ?? "1";
            ViewBag.Stock = stock ?? "";
            ViewBag.Email = email ?? "";
            ViewBag.Message = message ?? "";
            return View();
        }

        public ActionResult Finish(string amount, string stock, string email, string message)
        {
            return View();
        }
    }
}
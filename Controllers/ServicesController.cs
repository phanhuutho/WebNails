using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebNails.Controllers
{
    public class ServicesController : BaseController
    {
        // GET: Services
        public ActionResult Index(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Services", "Home");
            var strHTML = GetServiceDetail(name);
            return View(model: strHTML);
        }

        private string GetServiceDetail(string name)
        {
            var strHTML = "";
            if (System.IO.File.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/services/" + name + ".html")))
            {
                strHTML = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/services/" + name + ".html"));
            }
            return strHTML;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebNails.Controllers
{
    public class BaseController : Controller
    {
        private string HyperLinkTell = "tel:(316)2396349";
        private string TextTell = "(316)-239-6349";
        public BaseController()
        {
            ViewBag.HyperLinkTell = HyperLinkTell;
            ViewBag.TextTell = TextTell;
        }
    }
}
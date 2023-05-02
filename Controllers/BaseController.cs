using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebNails.Controllers
{
    public class BaseController : Controller
    {
        private string HyperLinkTell = "tel:(972)3556701";
        private string TextTell = "(972)-355-6701";
        public BaseController()
        {
            ViewBag.HyperLinkTell = HyperLinkTell;
            ViewBag.TextTell = TextTell;
        }
    }
}
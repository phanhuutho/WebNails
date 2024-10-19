using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNails.Models;

namespace WebNails.Utilities
{
    public class TokenAttribute : ActionFilterAttribute
    {
        private readonly string TokenKeyAPI = ConfigurationManager.AppSettings["TokenKeyAPI"];
        private readonly string SaltKeyAPI = ConfigurationManager.AppSettings["SaltKeyAPI"];
        private readonly string VectorKeyAPI = ConfigurationManager.AppSettings["VectorKeyAPI"];

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext != null && filterContext.HttpContext.Request.Cookies["TokenPage"] != null && !string.IsNullOrEmpty(filterContext.HttpContext.Request.Cookies["TokenPage"].Value))
            {
                var strEncryptToken = filterContext.HttpContext.Request.Cookies["TokenPage"].Value;
                var strDecryptToken = Sercurity.DecryptFromBase64(strEncryptToken, TokenKeyAPI, SaltKeyAPI, VectorKeyAPI);
                var objToken = JsonConvert.DeserializeObject<TokenPage>(strDecryptToken);

                if (objToken == null || string.IsNullOrEmpty(objToken.Token) || objToken.Token != filterContext.Controller.ViewBag.Token || string.IsNullOrEmpty(objToken.Domain) || objToken.Domain != filterContext.HttpContext.Request.Url.Host || objToken.TimeExpire == null || objToken.TimeExpire.Value < DateTime.Now)
                {
                    filterContext.Result = new RedirectResult("/");
                }    
                else
                {
                    HttpCookie cookie = filterContext.HttpContext.Request.Cookies["TokenPage"];
                    cookie.Expires = DateTime.Now.AddYears(-1);
                    filterContext.HttpContext.Response.Cookies.Add(cookie);
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("/");
            }
        }
    }
}
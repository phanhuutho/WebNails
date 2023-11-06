using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.Routing;
using WebNails.Models;

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

        [HttpPost]
        public ActionResult Contact(MessageModel item)
        {

            using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.Unicode), new MailAddress(ConfigurationManager.AppSettings["EmailContact"])))
            {
                mail.HeadersEncoding = System.Text.Encoding.Unicode;
                mail.SubjectEncoding = System.Text.Encoding.Unicode;
                mail.BodyEncoding = System.Text.Encoding.Unicode;
                mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                mail.Subject = item.Subject;
                mail.Body = $@"<p>Subject: {item.Subject}</p>
                               <p>Email: {item.YourEmail}</p>
                               <p>Name: {item.YourName}</p>
                               <p>Message: {item.YourMessage}</p>";

                SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                mySmtpClient.UseDefaultCredentials = false;
                mySmtpClient.Credentials = networkCredential;
                mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                mySmtpClient.Send(mail);
            }
            return Json(new { messages = "OK" }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Programs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult RegisterCoupon(string imgCoupon = "")
        {
            return View("register_coupon", new RegisterCouponModel { ImgCoupon = imgCoupon });
        }

        [HttpPost]
        public ActionResult RegisterCoupon(RegisterCouponModel item)
        {
            SendMailRegisterCoupon(item);
            return View("register_coupon_success");
        }

        public ActionResult Payment()
        {
            return View();
        }

        public ActionResult Gallery()
        {
            var Galleries = (List<GalleryModel>)ViewBag.Gallery ?? new List<GalleryModel>();
            return View(Galleries);
        }

        [HttpPost]
        public ActionResult Process(string amount, string stock, string email, string message)
        {
            var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
            ViewBag.EmailPaypal = EmailPaypal ?? "";
            ViewBag.Amount = amount ?? "1";
            ViewBag.Stock = stock ?? "";

            var cookieDataBefore = new HttpCookie("DataBefore");
            cookieDataBefore["Amount"] = amount;
            cookieDataBefore["Email"] = email;
            cookieDataBefore["Stock"] = stock;
            cookieDataBefore["Message"] = message;
            cookieDataBefore.Expires.Add(new TimeSpan(0, 60, 0));
            Response.Cookies.Add(cookieDataBefore);

            return View();
        }

        public ActionResult PaymentResponse()
        {
            var data = new RouteValueDictionary();
            foreach (var key in Request.Form.AllKeys)
            {
                data.Add(key, Request[key]);
            }
            foreach (var key in Request.QueryString.AllKeys)
            {
                data.Add(key, Request[key]);
            }
            foreach (var key in Request.Headers.AllKeys)
            {
                data.Add(key, Request[key]);
            }

            return RedirectToAction("Finish", data);
        }

        public ActionResult Finish()
        {
            string responseCode;
            string SecureHash;
            var strAmount = string.Empty;
            var strEmail = string.Empty;
            var strStock = string.Empty;
            var strMessage = string.Empty;

            HttpCookie cookieDataBefore = Request.Cookies["DataBefore"];
            if (cookieDataBefore != null)
            {
                strAmount = cookieDataBefore["Amount"];
                strEmail = cookieDataBefore["Email"];
                strStock = cookieDataBefore["Stock"];
                strMessage = cookieDataBefore["Message"];
            }

            if (Request.QueryString["st"] != null)
            {
                if (Request.QueryString["st"] == "Completed")
                {
                    SecureHash = "<font color='blue'><strong>CORRECT</strong></font>";

                    SendMailAfterPayment(strAmount, strStock, strEmail, strMessage);
                    responseCode = "0";
                }
                else
                {
                    SecureHash = "<font color='red'><strong>" + Request.QueryString["st"] + "</strong></font>";
                    responseCode = "-1";
                }
            }
            else
            {
                SecureHash = "<font color='red'><strong>ERROR</strong></font>";
                responseCode = "-1";
            }

            ViewBag.SecureHash = SecureHash;
            ViewBag.ResponseCode = responseCode;
            return View();
        }

        private void SendMailAfterPayment(string strAmount, string strStock, string strEmail, string strMessage)
        {
            if (!string.IsNullOrEmpty(strAmount) && !string.IsNullOrEmpty(strStock) && !string.IsNullOrEmpty(strEmail) && !string.IsNullOrEmpty(strMessage))
            {
                var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.Unicode), new MailAddress(EmailPaypal)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.Unicode;
                    mail.SubjectEncoding = System.Text.Encoding.Unicode;
                    mail.BodyEncoding = System.Text.Encoding.Unicode;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Checkout Paypal Gift Purcharse - " + strEmail;
                    mail.Body = $@"<p>Amount pay: {strAmount}</p>
					   <p>Receiver email: {strEmail}</p>
					   <p>Buyer email: {strStock}</p>
					   <p>Comment: {strMessage}</p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailRegisterCoupon(RegisterCouponModel item)
        {
            var EmailContact = ConfigurationManager.AppSettings["EmailContact"];
            var strBody = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/BodyRegisterCoupon.html"));
            strBody = strBody.Replace("{NailName}", ViewBag.Name);
            strBody = strBody.Replace("{ImgCoupon}", Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + item.ImgCoupon);
            strBody = strBody.Replace("{Email}", item.Email);
            strBody = strBody.Replace("{Name}", item.Name);
            strBody = strBody.Replace("{Phone}", item.Phone);
            strBody = strBody.Replace("{Birthday}", item.Birthday);

            using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ViewBag.Name, System.Text.Encoding.Unicode), new MailAddress(EmailContact)))
            {
                mail.HeadersEncoding = System.Text.Encoding.Unicode;
                mail.SubjectEncoding = System.Text.Encoding.Unicode;
                mail.BodyEncoding = System.Text.Encoding.Unicode;
                mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                mail.Subject = ViewBag.Name + " - Naperville";
                mail.Body = strBody;

                SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                mySmtpClient.UseDefaultCredentials = false;
                mySmtpClient.Credentials = networkCredential;
                mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                mySmtpClient.Send(mail);
            }

            using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ViewBag.Name, System.Text.Encoding.Unicode), new MailAddress(item.Email)))
            {
                mail.HeadersEncoding = System.Text.Encoding.Unicode;
                mail.SubjectEncoding = System.Text.Encoding.Unicode;
                mail.BodyEncoding = System.Text.Encoding.Unicode;
                mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                mail.Subject = ViewBag.Name + " - Naperville";
                mail.Body = strBody;

                SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                mySmtpClient.UseDefaultCredentials = false;
                mySmtpClient.Credentials = networkCredential;
                mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                mySmtpClient.Send(mail);
            }
        }
    }
}
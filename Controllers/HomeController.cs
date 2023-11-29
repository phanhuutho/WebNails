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
        public ActionResult Process(string amount, string stock, string email, string message, string img = "")
        {
            var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
            ViewBag.EmailPaypal = EmailPaypal ?? "";
            ViewBag.Amount = amount ?? "1";
            ViewBag.Stock = stock ?? "";
            ViewBag.Email = email ?? "";
            ViewBag.Img = img;

            var cookieDataBefore = new HttpCookie("DataBefore");
            cookieDataBefore["Amount"] = amount;
            cookieDataBefore["Email"] = email;
            cookieDataBefore["Stock"] = stock;
            cookieDataBefore["Message"] = message;
            cookieDataBefore["Img"] = img;
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

            TempData["PayerID"] = Request["PayerID"];
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
            var strImg = string.Empty;

            HttpCookie cookieDataBefore = Request.Cookies["DataBefore"];
            if (cookieDataBefore != null)
            {
                strAmount = cookieDataBefore["Amount"];
                strEmail = cookieDataBefore["Email"];
                strStock = cookieDataBefore["Stock"];
                strMessage = cookieDataBefore["Message"];
                strImg = cookieDataBefore["Img"];
            }

            if (Request.QueryString["PayerID"] != null && Request.QueryString["PayerID"] == string.Format("{0}", TempData["PayerID"]))
            {
                SecureHash = "<font color='blue'><strong>CORRECT</strong></font>";
                responseCode = "0";

                SendMailToOwner(strAmount, strStock, strEmail, strMessage, string.Format("{0}", TempData["PayerID"]), strImg);
                SendMailToBuyer(strAmount, strStock, strEmail, strMessage, string.Format("{0}", TempData["PayerID"]), strImg);
                SendMailToReceiver(strStock, strEmail, strAmount, string.Format("{0}", TempData["PayerID"]), strImg);
            }
            else
            {
                SecureHash = "<font color='red'><strong>FAIL</strong></font>";
                responseCode = "-1";
            }

            ViewBag.SecureHash = SecureHash;
            ViewBag.ResponseCode = responseCode;
            return View();
        }

        private void SendMailToOwner(string strAmount, string strStock, string strEmail, string strMessage, string strCode, string img = "")
        {
            if (!string.IsNullOrEmpty(strAmount) && !string.IsNullOrEmpty(strStock) && !string.IsNullOrEmpty(strEmail) && !string.IsNullOrEmpty(strMessage))
            {
                var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.UTF8), new MailAddress(EmailPaypal)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.UTF8;
                    mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Checkout Paypal Gift Purchase - " + strEmail;
                    mail.Body = $@"<p>Amount pay: {strAmount}</p>
					    <p>Receiver email: {strStock}</p>
					    <p>Buyer email: {strEmail}</p>
					    <p>Comment: {strMessage}</p>
                        <p>Code: <strong>{strCode}</strong></p>
                        <p><img width='320' src='{ Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + img}' width='360px' /></p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailToReceiver(string strEmailReceiver, string strEmailBuyer, string strAmount, string strCode, string img = "")
        {
            if (!string.IsNullOrEmpty(strEmailReceiver) && !string.IsNullOrEmpty(strEmailBuyer))
            {
                var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.UTF8), new MailAddress(strEmailReceiver)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.UTF8;
                    mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Gift For You";
                    mail.Body = $@"<p>Hello,</p><br/>
					    <p>You have a gift from  <strong>{strEmailBuyer}</strong>.</p>
                        <p>Please visit us at <strong>{ViewBag.Name}</strong> - Address: <strong>{ViewBag.Address}</strong> - Phone: <strong>{ViewBag.TextTell}</strong> to redeem your gift.</p>
                        <p>Amount: <strong>${strAmount} USD</strong>.</p>
                        <p>Code: <strong>{strCode}</strong></p><br/>
					    <p>Thank you!</p>
                        <p><img width='320' src='{ Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + img}' width='360px' /></p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailToBuyer(string strAmount, string strStock, string strEmail, string strMessage, string strCode, string img = "")
        {
            if (!string.IsNullOrEmpty(strAmount) && !string.IsNullOrEmpty(strStock) && !string.IsNullOrEmpty(strEmail) && !string.IsNullOrEmpty(strMessage))
            {
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.UTF8), new MailAddress(strEmail)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.UTF8;
                    mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Checkout Paypal Gift Purchase - " + strEmail;
                    mail.Body = $@"<p>Amount pay: {strAmount}</p>
					    <p>Receiver email: {strStock}</p>
					    <p>Buyer email: {strEmail}</p>
					    <p>Comment: {strMessage}</p>
                        <p>Code: <strong>{strCode}</strong></p>
                        <p><img width='320' src='{ Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + img}' width='360px' /></p>";

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

            using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ViewBag.Name, System.Text.Encoding.UTF8), new MailAddress(EmailContact)))
            {
                mail.HeadersEncoding = System.Text.Encoding.UTF8;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
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

            using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ViewBag.Name, System.Text.Encoding.UTF8), new MailAddress(item.Email)))
            {
                mail.HeadersEncoding = System.Text.Encoding.UTF8;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
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

        private static Random random = new Random();
        private string GenerateUniqueCode()
        {
            var result = string.Format("{0:ddd dd MMM yyyy HH mm ss}", DateTime.Now);
            result = String.Join("", result.Split(new char[] { ' ' }));
            result = new string(result.OrderBy(letter => random.Next()).ToArray()).ToUpper();
            return result;
        }
    }
}
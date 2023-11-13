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
            ViewBag.Email = email ?? "";

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

            HttpCookie cookieDataBefore = Request.Cookies["DataBefore"];
            if (cookieDataBefore != null)
            {
                strAmount = cookieDataBefore["Amount"];
                strEmail = cookieDataBefore["Email"];
                strStock = cookieDataBefore["Stock"];
                strMessage = cookieDataBefore["Message"];
            }

            if (Request.QueryString["PayerID"] != null && Request.QueryString["PayerID"] == string.Format("{0}", TempData["PayerID"]))
            {
                SecureHash = "<font color='blue'><strong>CORRECT</strong></font>";

                responseCode = "0";

                SendMailToOwner(strAmount, strStock, strEmail, strMessage, string.Format("{0}", TempData["PayerID"]));
                SendMailToBuyer(strAmount, strStock, strEmail, strMessage, string.Format("{0}", TempData["PayerID"]));
                SendMailToReceiver(strStock, strEmail, strAmount, string.Format("{0}", TempData["PayerID"]));
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

        private void SendMailToOwner(string strAmount, string strStock, string strEmail, string strMessage, string strCode)
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
                    mail.Subject = "Checkout Paypal Gift Purchase - " + strEmail;
                    mail.Body = $@"<p>Amount pay: {strAmount}</p>
					   <p>Receiver email: {strStock}</p>
					   <p>Buyer email: {strEmail}</p>
					   <p>Comment: {strMessage}</p>
                       <p>Code: <strong>{strCode}</strong></p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailToReceiver(string strEmailReceiver, string strEmailBuyer, string strAmount, string strCode)
        {
            if (!string.IsNullOrEmpty(strEmailReceiver) && !string.IsNullOrEmpty(strEmailBuyer))
            {
                var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.Unicode), new MailAddress(strEmailReceiver)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.Unicode;
                    mail.SubjectEncoding = System.Text.Encoding.Unicode;
                    mail.BodyEncoding = System.Text.Encoding.Unicode;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Gift For You";
                    mail.Body = $@"<p>Hello,</p><br/>
					   <p>You have a gift from  <strong>{strEmailBuyer}</strong>.</p>
                       <p>Please visit us at <strong>{ViewBag.Name}</strong> - Address: <strong>{ViewBag.Address}</strong> - Phone: <strong>{ViewBag.TextTell}</strong> to redeem your gift.</p>
                       <p>Amount: <strong>${strAmount} USD</strong>.</p>
                       <p>Code: <strong>{strCode}</strong></p><br/>
					   <p>Thank you!</p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailToBuyer(string strAmount, string strStock, string strEmail, string strMessage, string strCode)
        {
            if (!string.IsNullOrEmpty(strAmount) && !string.IsNullOrEmpty(strStock) && !string.IsNullOrEmpty(strEmail) && !string.IsNullOrEmpty(strMessage))
            {
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.Unicode), new MailAddress(strEmail)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.Unicode;
                    mail.SubjectEncoding = System.Text.Encoding.Unicode;
                    mail.BodyEncoding = System.Text.Encoding.Unicode;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Checkout Paypal Gift Purchase - " + strEmail;
                    mail.Body = $@"<p>Amount pay: {strAmount}</p>
					   <p>Receiver email: {strStock}</p>
					   <p>Buyer email: {strEmail}</p>
					   <p>Comment: {strMessage}</p>
                       <p>Code: <strong>{strCode}</strong></p>";

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
}
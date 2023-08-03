using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Configuration;
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
            var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
            ViewBag.EmailPaypal = EmailPaypal ?? "";
            ViewBag.Amount = amount ?? "1";
            ViewBag.Stock = stock ?? "";
            ViewBag.Email = email ?? "";
            ViewBag.Message = message ?? "";
            return View();
        }

        public ActionResult Finish(string amount, string stock, string email, string message)
        {
            if(!string.IsNullOrEmpty(amount) && !string.IsNullOrEmpty(stock) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(message))
            {
                var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.Unicode), new MailAddress(EmailPaypal)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.Unicode;
                    mail.SubjectEncoding = System.Text.Encoding.Unicode;
                    mail.BodyEncoding = System.Text.Encoding.Unicode;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Checkout Paypal Gift Purcharse - " + email;
                    mail.Body = $@"<p>Amount pay: {amount}</p>
					   <p>Receiver email: {email}</p>
					   <p>Buyer email: {stock}</p>
					   <p>Comment: {message}</p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }   
            return View();
        }
    }
}
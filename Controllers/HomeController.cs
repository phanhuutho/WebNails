using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Web.Mvc;
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

            //using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.Unicode), new MailAddress(ViewBag.Email)))
            //{
            //    mail.HeadersEncoding = System.Text.Encoding.Unicode;
            //    mail.SubjectEncoding = System.Text.Encoding.Unicode;
            //    mail.BodyEncoding = System.Text.Encoding.Unicode;
            //    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
            //    mail.Subject = item.Subject;
            //    mail.Body = $@"<p>Subject: {item.Subject}</p>
            //                   <p>Email: {item.YourEmail}</p>
            //                   <p>Name: {item.YourName}</p>
            //                   <p>Message: {item.YourMessage}</p>";

            //    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
            //    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
            //    mySmtpClient.UseDefaultCredentials = false;
            //    mySmtpClient.Credentials = networkCredential;
            //    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
            //    mySmtpClient.Send(mail);
            //}
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

        public ActionResult Gallery()
        {
            var Galleries = (List<GalleryModel>)ViewBag.Galleries ?? new List<GalleryModel>();
            var intCountPage = Galleries.Count / 12;
            if(Galleries.Count % 12 > 0)
            {
                intCountPage = intCountPage + 1;
            }
            ViewBag.CountPage = intCountPage;
            return View(Galleries.Skip(0).Take(12));
        }

        public ActionResult GalleryMore(int indexPage)
        {
            var Galleries = (List<GalleryModel>)ViewBag.Galleries ?? new List<GalleryModel>();
            return PartialView("_gallery_more", Galleries.Skip((indexPage - 1) * 12).Take(12));
        }
    }
}
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
using System.Data.SqlClient;
using Dapper;
using System.Data;
using System.Web.Security;

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
            var strID = Guid.NewGuid();
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
            cookieDataBefore["Guid"] = strID.ToString();
            cookieDataBefore.Expires.Add(new TimeSpan(0, 60, 0));
            Response.Cookies.Add(cookieDataBefore);

            using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
            {
                var Domain = Request.Url.Host;
                var Transactions = GenerateUniqueCode();
                var objResult = sqlConnect.Execute("spInfoPaypal_InsertBefore", new
                {
                    strID = strID,
                    strDomain = Domain,
                    strTransactions = Transactions,
                    strCode = Transactions,
                    strOwner = EmailPaypal,
                    strStock = stock,
                    strEmail = email,
                    intAmount = int.Parse(amount),
                    strMessage = message
                }, commandType: CommandType.StoredProcedure);
            }

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
            var strID = new Guid();

            HttpCookie cookieDataBefore = Request.Cookies["DataBefore"];
            if (cookieDataBefore != null)
            {
                strAmount = cookieDataBefore["Amount"];
                strEmail = cookieDataBefore["Email"];
                strStock = cookieDataBefore["Stock"];
                strMessage = cookieDataBefore["Message"];
                strID = Guid.Parse(cookieDataBefore["Guid"]);
            }

            if (Request.QueryString["PayerID"] != null && Request.QueryString["PayerID"] == string.Format("{0}", TempData["PayerID"]))
            {
                SecureHash = "<font color='blue'><strong>CORRECT</strong></font>";

                responseCode = "0";

                using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
                {
                    var info = sqlConnect.Query<InfoPaypal>("spInfoPaypal_GetInfoPaypalByID", new { strID = strID }, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    if(info != null)
                    {
                        var objResult = sqlConnect.Execute("spInfoPaypal_UpdateIsUsed", new { strID = strID, bitIsUsed = true }, commandType: CommandType.StoredProcedure);

                        if(objResult > 0)
                        {
                            SendMailToOwner(string.Format("{0}", strAmount), strStock, strEmail, strMessage, info.Code);
                            SendMailToBuyer(string.Format("{0}", strAmount), strStock, strEmail, strMessage, info.Code);
                            SendMailToReceiver(strStock, strEmail, string.Format("{0}", strAmount), info.Code);
                        }
                    }
                }    
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

        public ActionResult Login()
        {
            if (User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
            {
                return RedirectToAction("GiftManage");
            }
            return View(new LoginModel());
        }

        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
            {
                var queryString = Request.UrlReferrer.Query;

                var queryDictionary = HttpUtility.ParseQueryString(queryString);

                var Domain = Request.Url.Host;

                var checklogin = sqlConnect.Query("spUserSite_GetByUsernameAndPassword", new { strUsername = model.Username, strPassword = model.Password, strDomain = Domain }, commandType: CommandType.StoredProcedure).Count() == 1;
                if (checklogin)
                {
                    var ticket = new FormsAuthenticationTicket(1, model.Username, System.DateTime.Now, System.DateTime.Now.AddHours(1), true, "UserLogged", FormsAuthentication.FormsCookiePath);
                    var strEncrypt = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strEncrypt);
                    Response.Cookies.Add(cookie);

                    if (queryDictionary.Count > 0)
                    {
                        return Json(new { ReturnUrl = queryDictionary.Get("ReturnUrl"), IsLogin = true, Message = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { ReturnUrl = "/gift-manage.html", IsLogin = true, Message = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { ReturnUrl = queryDictionary.Get("ReturnUrl"), IsLogin = false, Message = "Login Fail" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GiftManage()
        {
            if(User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
            {
                return View();
            }
            return RedirectToAction("Login");
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                cookie.Expires = System.DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Login");
        }

        public ActionResult GetGiftManage(string search = "")
        {
            using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
            {
                var Domain = Request.Url.Host;

                var intSkip = Utilities.PagingHelper.Skip;

                var param = new DynamicParameters();
                param.Add("@intSkip", intSkip);
                param.Add("@intTake", Utilities.PagingHelper.CountSort);
                param.Add("@strDomain", Domain);
                param.Add("@strValue", search);
                param.Add("@intTotalRecord", dbType: DbType.Int32, direction: ParameterDirection.Output);

                var objResult = sqlConnect.Query<InfoPaypal>("spInfoPaypal_GetInfoPaypalByNailDomain", param, commandType: CommandType.StoredProcedure);

                ViewBag.Count = param.Get<int>("@intTotalRecord");

                return PartialView("_GetTable_GiftManage", objResult);
            }
        }

        [HttpPost]
        public ActionResult UpdateCompleted(Guid id)
        {
            using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
            {
                var objResult = sqlConnect.Execute("spInfoPaypal_UpdateIsUsed", new { strID = id, bitIsUsed = true}, commandType: CommandType.StoredProcedure);

                if(objResult > 0)
                {
                    return Json(new { Message = "Update completed success !" });
                }
                else
                {
                    return Json(new { Message = "Update completed fail !" });
                }
            }
        }

        [HttpPost]
        public ActionResult SendMail(Guid id)
        {
            using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
            {
                var info = sqlConnect.Query<InfoPaypal>("spInfoPaypal_GetInfoPaypalByID", new { strID = id }, commandType: CommandType.StoredProcedure).FirstOrDefault();

                if(info != null)
                {
                    var objResult = sqlConnect.Execute("spInfoPaypal_UpdateStatus", new { strID = id, intStatus = (int)PaymentStatus.Success }, commandType: CommandType.StoredProcedure);

                    if(objResult > 0)
                    {
                        SendMailToOwner(string.Format("{0}", info.Amount), info.Stock, info.Email, info.Message, info.Code);
                        SendMailToBuyer(string.Format("{0}", info.Amount), info.Stock, info.Email, info.Message, info.Code);
                        SendMailToReceiver(info.Stock, info.Email, string.Format("{0}", info.Amount), info.Code);
                    }

                    return Json(new { Message = "Send mail success !" });
                }
                else
                {
                    return Json(new { Message = "Send mail fail !" });
                }
            }
        }

        private static Random random = new Random();
        private string GenerateUniqueCode()
        {
            var strYear = string.Format("{0:yyyy}", DateTime.Now);
            var strDay = string.Format("{0:ddd dd MMM}", DateTime.Now);
            strDay = String.Join("", strDay.Split(new char[] { ' ' }));
            string strReverse = string.Empty;
            for (int i = strDay.Length - 1; i >= 0; i--)
            {
                strReverse += strDay[i];
            }
            var strTimes = string.Format("{0:HHmmss}", DateTime.Now);

            var result = string.Format("{0}{1}{2}", strYear, strReverse, strTimes).ToUpper();
            return result;
        }
    }
}
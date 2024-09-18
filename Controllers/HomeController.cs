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
using System.Threading.Tasks;

namespace WebNails.Controllers
{
    public class HomeController : BaseController
    {
        private readonly string ApiPayment = ConfigurationManager.AppSettings["ApiPayment"];

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
            //SendMailRegisterCoupon(item);
            return View("register_coupon_success");
        }

        public ActionResult Payment(string img = "")
        {
            ViewBag.Img = img;
            return View();
        }

        public ActionResult Gallery(int TabIndex = 1)
        {
            var Galleries = (List<GalleryModel>)ViewBag.Gallery ?? new List<GalleryModel>();

            ViewBag.TabIndex = TabIndex;
            ViewBag.ShowMore = Galleries.Where(x => x.TabIndex == TabIndex).Skip(16).Take(16).Count() > 0;

            Galleries = Galleries.Where(x => x.TabIndex == TabIndex).Skip(0).Take(16).ToList();
            return View(Galleries);
        }

        public ActionResult GalleryLoadMore(int TabIndex = 1, int Page = 1)
        {
            var dataGalleries = (List<GalleryModel>)ViewBag.Gallery ?? new List<GalleryModel>();
            var ShowMore = dataGalleries.Where(x => x.TabIndex == TabIndex).Skip(Page * 16).Take(16).Count() > 0;
            var Galleries = dataGalleries.Where(x => x.TabIndex == TabIndex).Select(x => new { x.Src }).Skip((Page - 1) * 16).Take(16).ToList();
            return Json(new { Galleries, ShowMore });
        }

        [HttpPost]
        public async Task<ActionResult> Process(string amount, string stock, string email, string message, string name_receiver, string name_buyer, string img = "", string codesale = "")
        {
            var strID = Guid.NewGuid();
            var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];
            var Token = ViewBag.Token;
            var Cost = float.Parse(amount);
            var Domain = Request.Url.Host;
            var Transactions = GenerateUniqueCode();

            var dataJson = new
            {
                EmailPaypal,
                strID,
                Transactions,
                amount,
                stock,
                email,
                message,
                name_receiver,
                name_buyer,
                codesale
            };
            var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Payment/Process?token={1}", ApiPayment, Token, Domain), JsonConvert.SerializeObject(dataJson));
            var AmountResult = JsonConvert.DeserializeObject(result).Amount;

            ViewBag.EmailPaypal = EmailPaypal ?? "";
            ViewBag.Amount = string.Format("{0}", AmountResult) ?? string.Format("{0}", "1");
            ViewBag.Stock = stock ?? "";
            ViewBag.Email = email ?? "";
            ViewBag.NameReceiver = name_receiver ?? "";
            ViewBag.NameBuyer = name_buyer ?? "";
            ViewBag.Img = img;
            ViewBag.Cost = Cost;
            ViewBag.CodeSaleOff = codesale;

            var cookieDataBefore = new HttpCookie("DataBefore");
            cookieDataBefore["Amount"] = string.Format("{0}", AmountResult);
            cookieDataBefore["Email"] = email;
            cookieDataBefore["Stock"] = stock;
            cookieDataBefore["Message"] = message;
            cookieDataBefore["NameReceiver"] = name_receiver;
            cookieDataBefore["NameBuyer"] = name_buyer;
            cookieDataBefore["Img"] = img;
            cookieDataBefore["Guid"] = strID.ToString();
            cookieDataBefore["Cost"] = string.Format("{0:N2}", Cost);
            cookieDataBefore["CodeSaleOff"] = codesale;
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

        public async Task<ActionResult> Finish()
        {
            var Token = ViewBag.Token;
            var Domain = Request.Url.Host;
            string responseCode;
            string SecureHash;
            var strAmount = string.Empty;
            var strCost = string.Empty;
            var strCodeSaleOff = string.Empty;
            var strEmail = string.Empty;
            var strStock = string.Empty;
            var strNameReceiver = string.Empty;
            var strNameBuyer = string.Empty;
            var strMessage = string.Empty;
            var strImg = string.Empty;
            var strID = new Guid();

            HttpCookie cookieDataBefore = Request.Cookies["DataBefore"];
            if (cookieDataBefore != null)
            {
                strAmount = cookieDataBefore["Amount"];
                strCost = cookieDataBefore["Cost"];
                strCodeSaleOff = cookieDataBefore["CodeSaleOff"];
                strEmail = cookieDataBefore["Email"];
                strStock = cookieDataBefore["Stock"];
                strNameReceiver = cookieDataBefore["NameReceiver"];
                strNameBuyer = cookieDataBefore["NameBuyer"];
                strMessage = cookieDataBefore["Message"];
                strImg = cookieDataBefore["Img"];
                strID = Guid.Parse(cookieDataBefore["Guid"]);
            }

            if (Request.QueryString["PayerID"] != null && Request.QueryString["PayerID"] == string.Format("{0}", TempData["PayerID"]))
            {
                SecureHash = "<font color='blue'><strong>CORRECT</strong></font>";

                responseCode = "0";

                var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Payment/Process?token={1}", ApiPayment, Token, Domain), JsonConvert.SerializeObject(new { strID }));
                var objResult = JsonConvert.DeserializeObject<dynamic>(result);
                var count = int.Parse(string.Format(objResult.count));
                var info = JsonConvert.DeserializeObject<InfoPaypal>(objResult.info);

                if (count > 0)
                {
                    SendMailToOwner(string.Format("{0:N2}", strAmount), strStock, strEmail, strMessage, info.Code, strNameReceiver, strNameBuyer, strImg, string.Format("{0:N2}", strCost), strCodeSaleOff);
                    SendMailToBuyer(string.Format("{0:N2}", strAmount), strStock, strEmail, strMessage, info.Code, strNameReceiver, strNameBuyer, strImg, string.Format("{0:N2}", strCost), strCodeSaleOff);
                    SendMailToReceiver(strStock, strEmail, string.Format("{0:N2}", strCost), info.Code, strNameReceiver, strNameBuyer, strImg);
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

        private void SendMailToOwner(string strAmount, string strStock, string strEmail, string strMessage, string strCode, string strNameReceiver, string strNameBuyer, string img = "", string strCost = "", string strCodeSaleOff = "")
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
                    mail.Body = $@"<p>Amount pay: <strong>${strAmount} USD</strong></p>
					    {(!string.IsNullOrEmpty(strCost) ? $"<p>Cost: {strCost}</p>" : "")}
					    {(!string.IsNullOrEmpty(strCodeSaleOff) ? $"<p>Code Sale Off: {strCodeSaleOff}</p>" : "")}
					    <p>Receiver name: {strNameReceiver}</p>
					    <p>Receiver email: {strStock}</p>
					    <p>Buyer name: {strNameBuyer}</p>
					    <p>Buyer email: {strEmail}</p>
					    <p>Comment: {strMessage}</p>
                        <p>Code: <strong>{strCode}</strong></p> 
                        <p><img width='320' src='{Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + img}' width='360px' /></p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailToReceiver(string strEmailReceiver, string strEmailBuyer, string strAmount, string strCode, string strNameReceiver, string strNameBuyer, string img = "")
        {
            if (!string.IsNullOrEmpty(strEmailReceiver) && !string.IsNullOrEmpty(strEmailBuyer))
            {
                using (MailMessage mail = new MailMessage(new MailAddress(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["EmailName"], System.Text.Encoding.UTF8), new MailAddress(strEmailReceiver, strNameReceiver, System.Text.Encoding.UTF8)))
                {
                    mail.HeadersEncoding = System.Text.Encoding.UTF8;
                    mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    mail.IsBodyHtml = bool.Parse(ConfigurationManager.AppSettings["IsBodyHtmlEmailSystem"]);
                    mail.Subject = "Gift For You";
                    mail.Body = $@"<p>Hello,</p><br/>
					    <p>You have a gift from <strong>{strNameBuyer} - ({strEmailBuyer})</strong>.</p>
                        <p>Please visit us at <strong>{ViewBag.Name}</strong> - Address: <strong>{ViewBag.Address}</strong> - Phone: <strong>{ViewBag.TextTell}</strong> to redeem your gift.</p>
                        <p>Amount: <strong>${strAmount} USD</strong>.</p>
                        <p>Code: <strong>{strCode}</strong></p><br/>
					    <p>Thank you!</p> 
                        <p><img width='320' src='{Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + img}' /></p>";

                    SmtpClient mySmtpClient = new SmtpClient(ConfigurationManager.AppSettings["HostEmailSystem"], int.Parse(ConfigurationManager.AppSettings["PortEmailSystem"]));
                    NetworkCredential networkCredential = new NetworkCredential(ConfigurationManager.AppSettings["EmailSystem"], ConfigurationManager.AppSettings["PasswordEmailSystem"]);
                    mySmtpClient.UseDefaultCredentials = false;
                    mySmtpClient.Credentials = networkCredential;
                    mySmtpClient.EnableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSslEmailSystem"]);
                    mySmtpClient.Send(mail);
                }
            }
        }

        private void SendMailToBuyer(string strAmount, string strStock, string strEmail, string strMessage, string strCode, string strNameReceiver, string strNameBuyer, string img = "", string strCost = "", string strCodeSaleOff = "")
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
					    {(!string.IsNullOrEmpty(strCost) ? $"<p>Cost: {strCost}</p>" : "")}
					    {(!string.IsNullOrEmpty(strCodeSaleOff) ? $"<p>Code Sale Off: {strCodeSaleOff}</p>" : "")}
					    <p>Receiver name: {strNameReceiver}</p>
					    <p>Receiver email: {strStock}</p>
					    <p>Buyer name: {strNameBuyer}</p>
					    <p>Buyer email: {strEmail}</p>
					    <p>Comment: {strMessage}</p>
                        <p>Code: <strong>{strCode}</strong></p> 
                        <p><img width='320' src='{Url.RequestContext.HttpContext.Request.Url.Scheme + "://" + Url.RequestContext.HttpContext.Request.Url.Authority + img}' /></p>";

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
        
        public ActionResult Login()
        {
            if (User != null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
            {
                return RedirectToAction("GiftManage");
            }
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            var Token = ViewBag.Token;

            var queryString = Request.UrlReferrer.Query;

            var queryDictionary = HttpUtility.ParseQueryString(queryString);

            var Domain = Request.Url.Host;

            var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Home/Login?token={1}", ApiPayment, Token, Domain), JsonConvert.SerializeObject(model));
            var objResult = JsonConvert.DeserializeObject<LoginResult>(result);
            var IsLogin = objResult.IsLogin;

            if (IsLogin)
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

        public async Task<ActionResult> GetGiftManage(string search = "")
        {
            var Token = ViewBag.Token;

            var Domain = Request.Url.Host;

            var intSkip = Utilities.PagingHelper.Skip;

            var intCountSort = Utilities.PagingHelper.CountSort;

            var result = await GetStringJsonFromURL(string.Format("{0}/{2}/Home/GetGiftManage?token={1}&intSkip={3}&intCountSort={4}&search={5}", ApiPayment, Token, Domain, intSkip, intCountSort, search));
            var objResult = JsonConvert.DeserializeObject<GiftManagelResult>(result);

            var Count = objResult.Count;
            var data = objResult.Data;

            ViewBag.Count = Count;

            return PartialView("_GetTable_GiftManage", data);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCompleted(Guid id)
        {
            var Token = ViewBag.Token;

            var Domain = Request.Url.Host;

            var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Home/UpdateCompleted?token={1}", ApiPayment, Token, Domain), JsonConvert.SerializeObject(new { id }));
            var objResult = JsonConvert.DeserializeObject<int>(result);

            if (objResult > 0)
            {
                return Json(new { Message = "Update completed success !" });
            }
            else
            {
                return Json(new { Message = "Update completed fail !" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendMail(Guid id)
        {
            var Token = ViewBag.Token;

            var Domain = Request.Url.Host;

            var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Home/SendMail?token={1}", ApiPayment, Token, Domain), JsonConvert.SerializeObject(new { id }));
            var objResult = JsonConvert.DeserializeObject<SendMailResult>(result);

            var Count = objResult.Count;
            var info = objResult.Data;

            if (info != null)
            {
                if (Count > 0)
                {
                    SendMailToOwner(string.Format("{0:N2}", info.Amount), info.Stock, info.Email, info.Message, info.Code, info.NameReceiver, info.NameBuyer, string.Format("{0:N2}", info.AmountReal), info.CodeSaleOff);
                    SendMailToBuyer(string.Format("{0:N2}", info.Amount), info.Stock, info.Email, info.Message, info.Code, info.NameReceiver, info.NameBuyer, string.Format("{0:N2}", info.AmountReal), info.CodeSaleOff);
                    SendMailToReceiver(info.Stock, info.Email, string.Format("{0:N2}", info.Amount), info.Code, info.NameReceiver, info.NameBuyer);
                }

                return Json(new { Message = "Send mail success !" });
            }
            else
            {
                return Json(new { Message = "Send mail fail !" });
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

        [HttpPost]
        public async Task<ActionResult> CheckCodeSaleOff(string Code, int Amount)
        {
            var Token = ViewBag.Token;
            var Domain = Request.Url.Host;

            var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Home/CheckCodeSaleOff?token={1}", ApiPayment, Token, Domain), JsonConvert.SerializeObject(new { Code, Amount }));
            var objResult = JsonConvert.DeserializeObject<CheckCodeSaleResult>(result);

            var status = objResult.Status;
            var message = objResult.Message;

            return Json(new { Status = status, Message = message });
        }

        [HttpPost]
        public async Task<ActionResult> GetListNailCodeSaleByDomain()
        {
            var Token = ViewBag.Token;
            var Domain = Request.Url.Host;

            var result = await PostStringJsonFromURL(string.Format("{0}/{2}/Home/GetListNailCodeSaleByDomain?token={1}", ApiPayment, Token, Domain), "");
            var objResult = JsonConvert.DeserializeObject<List<NailCodeSale>>(result);

            return Json(objResult);
        }
    }
}
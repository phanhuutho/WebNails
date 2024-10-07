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
using System.Text;
using System.IO;
using System.Threading.Tasks;

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
        public ActionResult Process(string amount, string stock, string email, string message, string name_receiver, string name_buyer, string img = "", string codesale = "")
        {
            var strID = Guid.NewGuid();
            var EmailPaypal = ConfigurationManager.AppSettings["EmailPaypal"];

            var Cost = float.Parse(amount);

            ViewBag.Invoice = strID;
            ViewBag.EmailPaypal = EmailPaypal ?? "";
            ViewBag.Amount = string.Format("{0}", amount) ?? string.Format("{0}", "1");
            ViewBag.Stock = stock ?? "";
            ViewBag.Email = email ?? "";
            ViewBag.NameReceiver = name_receiver ?? "";
            ViewBag.NameBuyer = name_buyer ?? "";
            ViewBag.Img = img;
            ViewBag.Cost = Cost;
            ViewBag.CodeSaleOff = codesale;

            //insert InforPaypal, use store Insert InforPaypal Before
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("strID: " + strID);
            sb.AppendLine("EmailPaypal: " + EmailPaypal);
            sb.AppendLine("Cost: " + Cost);
            sb.AppendLine("ViewBag.Invoice: " + ViewBag.Invoice);
            sb.AppendLine("ViewBag.EmailPaypal: " + ViewBag.EmailPaypal);
            sb.AppendLine("ViewBag.Amount: " + ViewBag.Amount);
            sb.AppendLine("ViewBag.Stock: " + ViewBag.Stock);
            sb.AppendLine("ViewBag.Email: " + ViewBag.Email);
            sb.AppendLine("ViewBag.NameReceiver: " + ViewBag.NameReceiver);
            sb.AppendLine("ViewBag.NameBuyer: " + ViewBag.NameBuyer);
            sb.AppendLine("ViewBag.Img: " + ViewBag.Img);
            sb.AppendLine("ViewBag.Cost: " + ViewBag.Cost);
            sb.AppendLine("ViewBag.CodeSaleOff: " + ViewBag.CodeSaleOff);
            System.IO.File.AppendAllText(@"C:\\DataWeb\PaypalIPN\LogRequest.txt", sb.ToString());
            return View();
        }

        public ActionResult PaymentResponse()
        {
            TempData["PayerID"] = Request["PayerID"];
            return RedirectToAction("Finish");
        }

        public ActionResult Finish()
        {
            if (Request.QueryString["PayerID"] != null && Request.QueryString["PayerID"] == string.Format("{0}", TempData["PayerID"]))
            {
                ViewBag.SecureHash = "<font color='blue'><strong>CORRECT</strong></font>";
                ViewBag.ResponseCode = "0";
            }
            else
            {
                ViewBag.SecureHash = "<font color='red'><strong>FAIL</strong></font>";
                ViewBag.ResponseCode = "-1";
            }
            return View();
        }

        [HttpPost]
        public HttpStatusCodeResult PaypalIPN()
        {
            //Store the IPN received from PayPal
            LogRequest(Request);

            //Fire and forget verification task
            Task.Run(() => VerifyTask(Request));

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void LogRequest(HttpRequestBase request)
        {
            StringBuilder sb = new StringBuilder();
            var data = new RouteValueDictionary();
            foreach (var key in request.Form.AllKeys)
            {
                data.Add(key, request[key]);
                sb.AppendLine(key + ": " + request[key]);
            }
            foreach (var key in request.QueryString.AllKeys)
            {
                data.Add(key, request[key]);
                sb.AppendLine(key + ": " + request[key]);
            }
            foreach (var key in request.Headers.AllKeys)
            {
                data.Add(key, request[key]);
                sb.AppendLine(key + ": " + request[key]);
            }
            sb.AppendLine("data: " + JsonConvert.SerializeObject(data));
            System.IO.File.AppendAllText(@"C:\\DataWeb\PaypalIPN\LogRequest.txt", sb.ToString());
            sb.Clear();
        }

        private void VerifyTask(HttpRequestBase ipnRequest)
        {
            try
            {
                var verificationResponse = string.Empty;
                var verificationRequest = (HttpWebRequest)WebRequest.Create("https://www.paypal.com/cgi-bin/webscr");

                //Set values for the verification request
                verificationRequest.Method = "POST";
                verificationRequest.ContentType = "application/x-www-form-urlencoded";
                var param = Request.BinaryRead(ipnRequest.ContentLength);
                var strRequest = Encoding.ASCII.GetString(param);

                //Add cmd=_notify-validate to the payload
                strRequest = "cmd=_notify-validate&" + strRequest;
                verificationRequest.ContentLength = strRequest.Length;

                //Attach payload to the verification request
                var streamOut = new StreamWriter(verificationRequest.GetRequestStream(), Encoding.ASCII);
                streamOut.Write(strRequest);
                streamOut.Close();

                //Send the request to PayPal and get the response
                var streamIn = new StreamReader(verificationRequest.GetResponse().GetResponseStream());
                verificationResponse = streamIn.ReadToEnd();
                streamIn.Close();

                ProcessVerificationResponse(verificationResponse, ipnRequest);
            }
            catch
            {
                //Capture exception for manual investigation
            }
        }

        private void ProcessVerificationResponse(string verificationResponse, HttpRequestBase ipnRequest)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("verificationResponse: " + verificationResponse);
            if (verificationResponse.Equals("VERIFIED"))
            {
                // check that Payment_status=Completed
                // check that Txn_id has not been previously processed
                // check that Receiver_email is your Primary PayPal email
                // check that Payment_amount/Payment_currency are correct
                // process payment
                if (ipnRequest.HttpMethod == "POST" && ipnRequest["payment_status"] == "Completed" && ipnRequest["receiver_email"] == ConfigurationManager.AppSettings["EmailPaypal"])
                {
                    sb.AppendLine("VERIFIED: OK");
                }
            }
            else if (verificationResponse.Equals("INVALID"))
            {
                //Log for manual investigation
            }
            else
            {
                //Log error
            }
            System.IO.File.AppendAllText(@"C:\\DataWeb\PaypalIPN\ProcessVerificationResponse.txt", sb.ToString());
            sb.Clear();
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
        public ActionResult CheckCodeSaleOff(string Code, int Amount)
        {
            var result = false;
            var message = "";
            if (!string.IsNullOrEmpty(Code))
            {
                using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
                {
                    var Domain = Request.Url.Host;
                    var objNailCodeSale = sqlConnect.Query<NailCodeSale>("spNailCodeSale_GetNailCodeSaleByCode", new { strCode = Code, strDomain = Domain, strDateNow = DateTime.Now }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    result = objNailCodeSale != null;
                    if (result)
                    {
                        if(Amount < objNailCodeSale.MinAmountSaleOff)
                        {
                            result = false;
                            message = $"Amount payment less than {string.Format("{0:N0}", objNailCodeSale.MinAmountSaleOff)}. Code sale off not available.";
                        }
                    }
                    else
                    {
                        message = "Code sale off incorrect";
                    }
                }
            }
            return Json(new { Status = result, Message = message });
        }

        [HttpPost]
        public ActionResult GetListNailCodeSaleByDomain()
        {
            using (var sqlConnect = new SqlConnection(ConfigurationManager.ConnectionStrings["ContextDatabase"].ConnectionString))
            {
                var Domain = Request.Url.Host;
                var data = sqlConnect.Query<NailCodeSale>("spNailCodeSale_GetNailCodeSalesByDomain", new { @strDomain = Domain, strDateNow = DateTime.Now }, commandType: CommandType.StoredProcedure).ToList();
                return Json(data);
            }
        }
    }
}
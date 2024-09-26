using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class GiftManagelResult
    {
        public int Count { get; set; }
        public List<InfoPaypal> Data { get; set; }
    }

    public class SendMailResult
    {
        public int Count { get; set; }
        public InfoPaypal Data { get; set; }
    }

    public class PaypalResult
    {
        public int Count { get; set; }
        public InfoPaypal Data { get; set; }
    }

    public class CheckCodeSaleResult
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class LoginResult
    {
        public bool IsLogin { get; set; }
    }
}
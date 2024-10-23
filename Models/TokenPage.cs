using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class TokenPage
    {
        public string Token { get; set; }
        public string Domain { get; set; }
        public DateTime? TimeExpire { get; set; }
    }
}
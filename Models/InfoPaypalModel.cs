using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class InfoPaypalModel
    {
        public string transaction { get; set; }
        public string owner { get; set; } //Email Owner
        public int amount { get; set; }
        public string stock { get; set; } //Email Receiver
        public string email { get; set; } //Email Buyer
        public string message { get; set; }
    }
}
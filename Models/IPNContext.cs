using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class IPNContext
    {
        public HttpRequestBase IPNRequest { get; set; }

        public string RequestBody { get; set; }

        public string Verification { get; set; } = String.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class PageModel
    {
        public string Page { get; set; }
        public List<string> RenderPartial { get; set; }
    }
}
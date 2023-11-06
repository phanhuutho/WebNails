using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class ReviewModel
    {
        public string Name { get; set; }
        public string Moment { get; set; }
        public int Star { get; set; }
        public string Text { get; set; }
    }
}
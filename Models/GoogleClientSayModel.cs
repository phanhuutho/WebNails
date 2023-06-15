using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class GoogleClientSayModel
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
        public int Position { get; set; }
    }
}
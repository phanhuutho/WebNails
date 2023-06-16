using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class GalleryModel
    {
        public string Image { get; set; }
        public string UrlRedirect { get; set; }
        public bool IsVideo { get; set; }
        public bool IsAlbums { get; set; }
    }
}
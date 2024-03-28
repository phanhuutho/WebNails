using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebNails.Models;

namespace WebNails.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            var jsonText = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/info.json"));
            var dataInfo = JsonConvert.DeserializeObject<InfoModel>(jsonText);
            ViewBag.HyperLinkTell = dataInfo.HyperLinkTell;
            ViewBag.TextTell = dataInfo.TextTell;
            ViewBag.Name = dataInfo.Name;
            ViewBag.Logo = dataInfo.Logo ?? "";
            ViewBag.LinkBookingAppointment = dataInfo.LinkBookingAppointment ?? "";
            ViewBag.Address = dataInfo.Address;
            ViewBag.GooglePlus = dataInfo.GooglePlus;
            ViewBag.LinkGoogleMapAddress = dataInfo.LinkGoogleMapAddress;
            ViewBag.LinkIFrameGoogleMap = dataInfo.LinkIFrameGoogleMap;
            ViewBag.ShowCoupon = dataInfo.ShowCoupon;
            ViewBag.Coupons = dataInfo.Coupons ?? new List<CouponModel>();
            ViewBag.Prices = dataInfo.Prices ?? new List<PricesModel>();
            ViewBag.Telegram = dataInfo.Telegram ?? new SocialModel();
            ViewBag.Facebook = dataInfo.Facebook ?? new SocialModel();
            ViewBag.Instagram = dataInfo.Instagram ?? new SocialModel();
            ViewBag.Twitter = dataInfo.Twitter ?? new SocialModel();
            ViewBag.Youtube = dataInfo.Youtube ?? new SocialModel();

            var txtBusinessHours = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/business-hours.txt"));
            ViewBag.BusinessHours = txtBusinessHours;

            var txtHomeAboutUs = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/home-about-us.txt"));
            ViewBag.HomeAboutUs = txtHomeAboutUs;

            var txtAboutUs = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/about-us.txt"));
            ViewBag.AboutUs = txtAboutUs;

            var jsonHomeGallery = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/home-data-gallery.json"));
            var dataHomeGallery = JsonConvert.DeserializeObject<List<GalleryModel>>(jsonHomeGallery);
            ViewBag.HomeGallery = dataHomeGallery;

            var jsonGallery = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/data-gallery.json"));
            var dataGallery = JsonConvert.DeserializeObject<List<GalleryModel>>(jsonGallery);
            ViewBag.Gallery = dataGallery.OrderBy(x => x.Position).ToList();

            var jsonCouponCallback = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/callback-coupon.json"));
            ViewBag.CouponCallback = JsonConvert.DeserializeObject<List<CouponCallbackModel>>(jsonCouponCallback); ;

            var jsonSetting = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/setting.json"));
            var dataSetting = JsonConvert.DeserializeObject<dynamic>(jsonSetting);
            ViewBag.IsPayment = bool.Parse(string.Format("{0}", dataSetting.IsPayment));
            ViewBag.IsGallery = bool.Parse(string.Format("{0}", dataSetting.IsGallery));
            ViewBag.IsFade_HomeBanner = bool.Parse(string.Format("{0}", dataSetting.IsFade_HomeBanner));
            ViewBag.IsFade_PageBanner = bool.Parse(string.Format("{0}", dataSetting.IsFade_PageBanner));
            ViewBag.LogoTop_Rounded = string.Format("{0}", dataSetting.LogoTop_Rounded);
            ViewBag.LogoBottom_Rounded = string.Format("{0}", dataSetting.LogoBottom_Rounded);
            ViewBag.ImageServices_Home_Rounded = string.Format("{0}", dataSetting.ImageServices_Home_Rounded);
            ViewBag.ImageServices_Home_Text_ItemAlign = string.Format("{0}", dataSetting.ImageServices_Home_Text_ItemAlign);
            ViewBag.ImageContact_Home_Rounded = string.Format("{0}", dataSetting.ImageContact_Home_Rounded);
            ViewBag.Button_Contact_Rounded = string.Format("{0}", dataSetting.Button_Contact_Rounded);
            ViewBag.Image_Contact_Rounded = string.Format("{0}", dataSetting.Image_Contact_Rounded);
            ViewBag.Image_Service_Rounded = string.Format("{0}", dataSetting.Image_Service_Rounded);
            ViewBag.Image_Service_Text_ItemAlign = string.Format("{0}", dataSetting.Image_Service_Text_ItemAlign);
            ViewBag.Image_Gallery_Rounded = string.Format("{0}", dataSetting.Image_Gallery_Rounded);
            ViewBag.Image_GiftCard_Rounded = string.Format("{0}", dataSetting.Image_GiftCard_Rounded);

            var BannerHome = (List<dynamic>)JsonConvert.DeserializeObject<List<dynamic>>(Convert.ToString(dataSetting.Banner_Home));
            var BannerPage = (List<dynamic>)JsonConvert.DeserializeObject<List<dynamic>>(Convert.ToString(dataSetting.Banner_Page));
            var BannerContact = (List<dynamic>)JsonConvert.DeserializeObject<List<dynamic>>(Convert.ToString(dataSetting.Banner_Contact));

            List<Dictionary<string, string>> Banner_Homes = new List<Dictionary<string, string>>();
            foreach(var item in BannerHome.OrderBy(x => x.Position))
            {
                var Text_ItemAlign = string.Format("align-items-xl-{0}", item.Text_ItemAlign.Large);
                Text_ItemAlign += string.Format(" align-items-lg-{0}", item.Text_ItemAlign.Laptop);
                Text_ItemAlign += string.Format(" align-items-md-{0}", item.Text_ItemAlign.Tablet);
                Text_ItemAlign += string.Format(" align-items-{0}", item.Text_ItemAlign.Mobile);

                var Text_JustifyContent = string.Format("justify-content-xl-{0}", item.Text_JustifyContent.Large);
                Text_JustifyContent += string.Format(" justify-content-lg-{0}", item.Text_JustifyContent.Laptop);
                Text_JustifyContent += string.Format(" justify-content-md-{0}", item.Text_JustifyContent.Tablet);
                Text_JustifyContent += string.Format(" justify-content-{0}", item.Text_JustifyContent.Mobile);

                var Banner_Item = new Dictionary<string, string>();
                Banner_Item.Add("Text_ItemAlign", Text_ItemAlign);
                Banner_Item.Add("Text_JustifyContent", Text_JustifyContent);

                Banner_Homes.Add(Banner_Item);
            }
            ViewBag.Banner_Home = Banner_Homes;

            List<Dictionary<string, string>> Banner_Pages = new List<Dictionary<string, string>>();
            foreach (var item in BannerPage.OrderBy(x => x.Position))
            {
                var Text_ItemAlign = string.Format("align-items-xl-{0}", item.Text_ItemAlign.Large);
                Text_ItemAlign += string.Format(" align-items-lg-{0}", item.Text_ItemAlign.Laptop);
                Text_ItemAlign += string.Format(" align-items-md-{0}", item.Text_ItemAlign.Tablet);
                Text_ItemAlign += string.Format(" align-items-{0}", item.Text_ItemAlign.Mobile);

                var Text_JustifyContent = string.Format("justify-content-xl-{0}", item.Text_JustifyContent.Large);
                Text_JustifyContent += string.Format(" justify-content-lg-{0}", item.Text_JustifyContent.Laptop);
                Text_JustifyContent += string.Format(" justify-content-md-{0}", item.Text_JustifyContent.Tablet);
                Text_JustifyContent += string.Format(" justify-content-{0}", item.Text_JustifyContent.Mobile);

                var Banner_Item = new Dictionary<string, string>();
                Banner_Item.Add("Text_ItemAlign", Text_ItemAlign);
                Banner_Item.Add("Text_JustifyContent", Text_JustifyContent);

                Banner_Pages.Add(Banner_Item);
            }
            ViewBag.Banner_Page = Banner_Pages;

            List<Dictionary<string, string>> Banner_Contacts = new List<Dictionary<string, string>>();
            foreach (var item in BannerContact.OrderBy(x => x.Position))
            {
                var Text_ItemAlign = string.Format("align-items-xl-{0}", item.Text_ItemAlign.Large);
                Text_ItemAlign += string.Format(" align-items-lg-{0}", item.Text_ItemAlign.Laptop);
                Text_ItemAlign += string.Format(" align-items-md-{0}", item.Text_ItemAlign.Tablet);
                Text_ItemAlign += string.Format(" align-items-{0}", item.Text_ItemAlign.Mobile);

                var Text_JustifyContent = string.Format("justify-content-xl-{0}", item.Text_JustifyContent.Large);
                Text_JustifyContent += string.Format(" justify-content-lg-{0}", item.Text_JustifyContent.Laptop);
                Text_JustifyContent += string.Format(" justify-content-md-{0}", item.Text_JustifyContent.Tablet);
                Text_JustifyContent += string.Format(" justify-content-{0}", item.Text_JustifyContent.Mobile);

                var Banner_Item = new Dictionary<string, string>();
                Banner_Item.Add("Text_ItemAlign", Text_ItemAlign);
                Banner_Item.Add("Text_JustifyContent", Text_JustifyContent);

                Banner_Contacts.Add(Banner_Item);
            }
            ViewBag.Banner_Contact = Banner_Contacts;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class InfoModel
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public string TextTell { get; set; }
        public string HyperLinkTell { get; set; }
        public string LinkBookingAppointment { get; set; }
        public string GooglePlus { get; set; }
        public string LinkGoogleMapAddress { get; set; }
        public string LinkIFrameGoogleMap { get; set; }
        public string BusinessHours { get; set; }
        public bool ShowCoupon { get; set; }
        public int MinAmountSaleOff { get; set; }
        public List<CouponModel> Coupons { get; set; }
        public List<PricesModel> Prices { get; set; }
        public SocialModel Telegram { get; set; }
        public SocialModel Facebook { get; set; }
        public SocialModel Instagram { get; set; }
        public SocialModel Twitter { get; set; }
        public SocialModel Youtube { get; set; }
    }
}
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            ViewBag.Address = dataInfo.Address;
            ViewBag.GooglePlus = dataInfo.GooglePlus;
            ViewBag.LinkGoogleMapAddress = dataInfo.LinkGoogleMapAddress;
            ViewBag.LinkIFrameGoogleMap = dataInfo.LinkIFrameGoogleMap ;

            var txtBusinessHours = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/business-hours.txt"));
            ViewBag.BusinessHours = txtBusinessHours;

            var txtHomeAboutUs = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/home-about-us.txt"));
            ViewBag.HomeAboutUs = txtHomeAboutUs;

            var txtAboutUs = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/about-us.txt"));
            ViewBag.AboutUs = txtAboutUs;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var webGen = new WebGenerator();
            var azureRegion = webGen.GetRegions();
            var o365Service = webGen.GetO365Services();

            ViewData["AppVersion"] = ConfigurationManager.AppSettings["AppVersion"];
            ViewData["Regions"] = azureRegion;
            ViewData["O365services"] = o365Service;

            return View();
        }
    }
}
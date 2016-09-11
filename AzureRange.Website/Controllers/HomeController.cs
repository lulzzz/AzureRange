using Newtonsoft.Json;
using StackExchange.Redis;
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
            var webGen = new WebGenerator(CacheConnection);
            // replace this call with http://mscloudips.azurewebsites.net/api/azureips/operation/listregions

            var azureRegion = webGen.GetRegions();
            //List<string> _regionList = azureRegion.Select(r => r.Id).ToList();
            //var result = webGen.GetComplementPrefixList(_regionList);
            //ViewData["IPPrefixInput"] = webGen.CachedList;

            ViewData["AppVersion"] = ConfigurationManager.AppSettings["AppVersion"];
            ViewData["Regions"] = azureRegion;
            //return View(result);
            return View();
        }
    }
}
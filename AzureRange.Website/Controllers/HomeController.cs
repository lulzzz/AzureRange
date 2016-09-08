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
            var result = webGen.GetComplementPrefixList(webGen.GetRegions().Select(r=>r.Id).ToList());

            ViewData["IPPrefixInput"] = webGen.CachedList;
            ViewData["Regions"] = webGen.GetRegions();
            return View(result);
        }
    }
}
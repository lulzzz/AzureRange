using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class HomeController : Controller
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(cacheConnection);
        });
        public static ConnectionMultiplexer Connection
        {
            get
            {
                //throw ("Test");

                return lazyConnection.Value;
            }
        }
        public ActionResult Index()
        {
            var db = Connection.GetDatabase();
            string rangesRaw = db.StringGet("ranges");

            if (string.IsNullOrEmpty(rangesRaw))
            {
                var rangesList = Downloader.Download();
                rangesRaw = JsonConvert.SerializeObject(rangesList);
                db.StringSet("ranges", rangesRaw);
            }

            var ranges = JsonConvert.DeserializeObject<List<IPPrefix>>(rangesRaw);
            var results = Generator.Not(ranges);

            ViewData["ranges"] = ranges;
            return View(results);
        }
    }
}
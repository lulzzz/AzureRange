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
        public static IConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
        public ActionResult Index()
        {
            var db = Connection.GetDatabase();
            var jsonIpPrefixList = string.Empty;
            List<IPPrefix> ipPPrefixesInput = null, ipPrefixesOutput = null;
            jsonIpPrefixList = db.StringGet("ranges");

            if (string.IsNullOrEmpty(jsonIpPrefixList))
            {
                // Load into IPPrefixesInput the list of prefixes to find complement for.
                ipPPrefixesInput = Downloader.Download();
                ipPPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
                ipPPrefixesInput.Add(new IPPrefix("10.0.0.0/8"));
                ipPPrefixesInput.Add(new IPPrefix("172.16.0.0/12"));
                ipPPrefixesInput.Add(new IPPrefix("169.254.0.0/16"));
                ipPPrefixesInput.Add(new IPPrefix("192.168.0.0/16"));
                ipPPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));

                jsonIpPrefixList = JsonConvert.SerializeObject(ipPPrefixesInput);
                db.StringSet("ranges", jsonIpPrefixList, TimeSpan.FromHours(1));
            }

            var ranges = JsonConvert.DeserializeObject<List<IPPrefix>>(jsonIpPrefixList);
            ipPrefixesOutput = Generator.Not(ipPPrefixesInput); 

            ViewData["IPPrefixInput"] = ipPrefixesOutput;
            return View(ipPrefixesOutput);
        }
    }
}
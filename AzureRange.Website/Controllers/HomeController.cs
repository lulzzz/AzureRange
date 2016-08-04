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
            var jsoIpPrefixList = string.Empty;
            List<IPPrefix> ipPPrefixesInput = null, ipPrefixesOutput = null;

#if debug

#else
            jsoIpPrefixList = db.StringGet("ranges");
#endif

            if (string.IsNullOrEmpty(jsoIpPrefixList))
            {
                // Load into IPPrefixesInput the list of prefixes to find complement for.
                ipPPrefixesInput = Downloader.Download();
                ipPPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
                ipPPrefixesInput.Add(new IPPrefix("10.0.0.0/8"));
                ipPPrefixesInput.Add(new IPPrefix("172.16.0.0/12"));
                ipPPrefixesInput.Add(new IPPrefix("169.254.0.0/16"));
                ipPPrefixesInput.Add(new IPPrefix("192.168.0.0/16"));
                ipPPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));

                jsoIpPrefixList = JsonConvert.SerializeObject(ipPPrefixesInput);
#if debug
                db.StringSet("ranges", strJsonIPPrefixList, TimeSpan.FromHours(1));
#endif
            }

            var ranges = JsonConvert.DeserializeObject<List<IPPrefix>>(jsoIpPrefixList);
            ipPrefixesOutput = Generator.Not(ipPPrefixesInput); 

            ViewData["IPPrefixInput"] = ipPrefixesOutput;
            return View(ipPrefixesOutput);
        }
    }
}
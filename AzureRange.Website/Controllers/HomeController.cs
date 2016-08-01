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
            string strJsonIPPrefixList;// = db.StringGet("ranges");
            //List<IPPrefix> IPPrefixList;
            List<IPPrefix> IPPrefixesInput;
            List<IPPrefix> IPPrefixesOutput;

            //if (string.IsNullOrEmpty(strJsonIPPrefixList))
            if (true)
            {
                IPPrefixesInput = Downloader.Download();
                IPPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
                IPPrefixesInput.Add(new IPPrefix("10.0.0.0/8"));
                IPPrefixesInput.Add(new IPPrefix("172.16.0.0/12"));
                IPPrefixesInput.Add(new IPPrefix("169.254.0.0/16"));
                IPPrefixesInput.Add(new IPPrefix("192.168.0.0/16"));
                IPPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));

                strJsonIPPrefixList = JsonConvert.SerializeObject(IPPrefixesInput);
                //db.StringSet("ranges", strJsonIPPrefixList);
            }

            var ranges = JsonConvert.DeserializeObject<List<IPPrefix>>(strJsonIPPrefixList);
            IPPrefixesOutput = Generator.Not(ranges);

            ViewData["IPPrefix"] = ranges;
            return View(IPPrefixesOutput);
        }
    }
}
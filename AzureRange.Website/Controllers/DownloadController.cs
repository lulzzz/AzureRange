using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;

namespace AzureRange.Website.Controllers
{
    public class DownloadController : BaseController
    {
        public ActionResult Index(string[] region, string outputformat, string command, bool complement = false)
        {
            int resultCount;
            var resultString = GenerationHelper.Generate(region, outputformat, false, out resultCount);
            var outputFileName = "Results-UTC-" + DateTime.UtcNow + "-" + string.Join("-", region) + ".txt";
            
            return File(Encoding.UTF8.GetBytes(resultString), "application/octet-stream", outputFileName);
        }
    }
}

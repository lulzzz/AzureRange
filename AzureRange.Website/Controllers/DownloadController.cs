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
        public ActionResult Index(string[] region, string[] o365service, string outputformat, string command, bool complement = false)
        {
            int resultCount;
            var resultString = GenerationHelper.Generate(region, o365service, outputformat, false, out resultCount);
            var outputFileName = "Results-UTC-" + DateTime.UtcNow + "-" + (region==null? "": string.Join("-", region)) + "-" + (o365service==null? "" : string.Join("-",o365service)) + ".txt";
            
            return File(Encoding.UTF8.GetBytes(resultString), "application/octet-stream", outputFileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class GenerateController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get([FromUri] string[] region, [FromUri] string[] o365service, string outputformat, bool complement = false, bool summarize = false)
        {
            int resultCount;
            // validate if proprer region or o365 service (for hacking... not to break service);
            var resultString = GenerationHelper.Generate(region, o365service, outputformat, complement, summarize, out resultCount);
            return Json(new { count = resultCount, encodedResultString = WebUtility.HtmlEncode(resultString) });
        }
    }
}
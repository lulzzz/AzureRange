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
        public IHttpActionResult Get([FromUri] string[] region, [FromUri] string[] o365service, string outputformat, bool complement = false)
        {
            int resultCount;
            var resultString = GenerationHelper.Generate(region, o365service, outputformat, complement, out resultCount);
            return Json(new { count = resultCount, encodedResultString = WebUtility.HtmlEncode(resultString) });
        }
    }
}
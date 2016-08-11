using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class GenerateController : BaseController
    {
        private const string _ciscoPrefix = @"router bgp <YOUR ASN>
 bgp log-neighbor-changes
 neighbor <PRIVATE_PEERING_EXPRESSROUTE_IP> remote-as 12076 ! ASN for ExpressRoute = 12076
 !
 address-family ipv4
  redistribute static ! this is one way to inject the networks below
  neighbor <PRIVATE_PEERING_EXPRESSROUTE_IP> activate
  no auto-summary
  no synchronization
 exit-address-family
!";


        public FileResult Index(string[] region, string outputformat)
        {
            var resultString = string.Empty;

            if (region != null)
            {
                var webGen = new WebGenerator(CacheConnection);
                var result = webGen.Generate(region.ToList());

                if (outputformat == "cisco")
                    resultString = _ciscoPrefix + Environment.NewLine + 
                        string.Join(string.Empty, 
                        result.Select(r => "ip route " + r.ToStringLongMask() + " null0" + Environment.NewLine
                        ).ToArray());

                if(outputformat == "subnets")
                    resultString = string.Join(string.Empty, result.Select(r =>  r.ToString() + Environment.NewLine).ToArray());
            }

            if (string.IsNullOrEmpty(resultString))
            {
                return File(Encoding.ASCII.GetBytes("No region selected."), System.Net.Mime.MediaTypeNames.Application.Octet, "Error.txt");
            }
            else
            {
                return File(Encoding.ASCII.GetBytes(resultString), System.Net.Mime.MediaTypeNames.Application.Octet, "AzureRange.txt");
            }
        }
    }
}
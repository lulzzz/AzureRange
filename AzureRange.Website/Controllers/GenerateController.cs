using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class GenerateController : BaseController
    {

        #region const_definition
        private const string _ciscoIOSPrefix = @"! CODE FOR IOS - CHECK IN A LAB BEFORE EXECUTING IN PRODUCTION!
router bgp <YOUR ASN>
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
        private const string _ciscoASAPrefix = @"! CODE FOR Cisco ASA - CHECK IN A LAB BEFORE EXECUTING IN PRODUCTION!
router bgp <YOUR ASN>
 bgp log-neighbor-changes
 !
 address-family ipv4 unicast
  neighbor <PRIVATE_PEERING_EXPRESSROUTE_IP> remote-as <YOUR ASN>
  neighbor <PRIVATE_PEERING_EXPRESSROUTE_IP> description ExpressRoutePrivatePeering
  neighbor <PRIVATE_PEERING_EXPRESSROUTE_IP> activate
  neighbor <PRIVATE_PEERING_EXPRESSROUTE_IP> route-map AZURE-OUT out
  redistribute static
  no auto-summary
  no synchronization
 exit-address-family
!
route-map AZURE-OUT permit 10
 match ip address prefix-list AZURE-OUT
!
";
        #endregion

        public object Index(string[] regions, string outputformat, string command)
        {
            var resultString = string.Empty;

            if (regions != null)
            {
                var webGen = new WebGenerator(CacheConnection);
                var result = webGen.GetComplementPrefixList(regions.ToList());

                // Cisco IOS/IOS-XR
                if (outputformat == "cisco-ios")
                    resultString = _ciscoIOSPrefix + Environment.NewLine +
                        string.Join(string.Empty,
                        result.Select(r => "ip route " + r.ToStringLongMask() + " null0" + Environment.NewLine
                        ).ToArray());
                // Cisco ASA
                if (outputformat == "cisco-asa")
                {
                    resultString = _ciscoASAPrefix + Environment.NewLine;
                    resultString = resultString + string.Join(string.Empty,
                        result.Select(r => "route <interface_name> " + r.ToStringLongMask() + " <interface_name_IP>" + Environment.NewLine
                        ).ToArray());
                    resultString = resultString
                        + "!" + Environment.NewLine
                        + "! Prefix-List to filter outgoing update to be restricted to the list below" + Environment.NewLine
                        + "!" + Environment.NewLine;
                    var prefixSeqNumber = 10;
                    resultString = resultString + string.Join(string.Empty, result.Select(r => "prefix-list AZURE-OUT seq " + prefixSeqNumber++ * 10 + " permit "
                         + r.ReadableIP + "/" + r.Mask + Environment.NewLine).ToArray());
                }

                if (outputformat == "list-subnet-masks")
                    resultString = string.Join(string.Empty, result.Select(r => r.ToStringLongMask() + Environment.NewLine).ToArray());
                if (outputformat == "list-cidr")
                    resultString = string.Join(string.Empty, result.Select(r => r.ReadableIP + "/" + r.Mask + Environment.NewLine).ToArray());
                if (outputformat == "csv-subnet-masks")
                {
                    resultString = string.Join(string.Empty, result.Select(r => "\"" + r.ToStringLongMask() + "\",").ToArray());
                    // need to remove the last ","
                    resultString = resultString.Substring(0, resultString.Length - 1);
                }
                if (outputformat == "csv-cidr")
                {
                    resultString = string.Join(string.Empty, result.Select(r => "\"" + r.ReadableIP + "/" + r.Mask + "\",").ToArray());
                    // remove the last "\","
                    resultString = resultString.Substring(0, resultString.Length - 1);
                }

                // Now deal with the command
                if (command == "download")
                {
                    //Generate filename
                    var outputFileName = "Results-UTC-" + DateTime.UtcNow + "-";
                    foreach (string region in regions)
                    {
                        outputFileName = outputFileName + region + "-";
                    }
                    outputFileName = outputFileName.Substring(0, outputFileName.Length - 1) + ".txt";
                    return Json(new {count=result.Count, encodedResultString = resultString, fileName = outputFileName },JsonRequestBehavior.AllowGet);
                }
                else if (command == "show")
                {
                    return Json(new {count=result.Count, encodedResultString = WebUtility.HtmlEncode(resultString) },JsonRequestBehavior.AllowGet); 
                }
                else
                {
                    return WebUtility.HtmlEncode("Unexpected operation command.");
                }
            }
            else // command == "Generate"
            {
                return null;
            }
        }
    }
}

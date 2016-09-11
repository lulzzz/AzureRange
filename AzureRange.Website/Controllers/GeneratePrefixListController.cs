using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class GeneratePrefixListController : BaseController
    {

        #region const_definition
        private const string _ciscoIOSPrefixBGPConfig = 
@"! CODE FOR IOS - CHECK IN A LAB BEFORE EXECUTING IN PRODUCTION!
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
        private const string _ciscoASAPrefixBGPConfig = 
@"! CODE FOR Cisco ASA - CHECK IN A LAB BEFORE EXECUTING IN PRODUCTION!
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
        private const string _ciscoIOSPrefixRouteConfig =
@"! CODE FOR IOS - CHECK IN A LAB BEFORE EXECUTING IN PRODUCTION!
! ip route statements to redirect traffic out a specific interface
!";
        private const string _ciscoIOSPrefixACLConfig =
@"! CODE FOR IOS - CHECK IN A LAB BEFORE EXECUTING IN PRODUCTION!
! ACL statements to permit traffic towards calculated prefixes
!
interface GigabitEthernet1
 ip access-group AzurePublicServicesACL in
 exit
!
ip access-list extended AzurePublicServicesACL
";
        #endregion

        public object Index(string[] regions, string outputformat, string command, bool complement = false)
        {
            var resultString = string.Empty;

            if (regions != null)
            {
                var webGen = new WebGenerator(CacheConnection);
                var result = webGen.GetComplementPrefixList(regions.ToList(),complement);
                switch (outputformat)
                {
                    case "cisco-ios":
                        resultString = _ciscoIOSPrefixBGPConfig + Environment.NewLine +
                            string.Join(string.Empty,
                            result.Select(r => "ip route " + r.ToStringLongMask() + " null0" + Environment.NewLine
                            ).ToArray());
                        break;

                    case "cisco-asa":
                        resultString = _ciscoASAPrefixBGPConfig + Environment.NewLine;
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
                        break;
                    case "list-subnet-masks":
                        resultString = string.Join(string.Empty, result.Select(r => r.ToStringLongMask() + Environment.NewLine).ToArray());
                        break;
                    case "list-cidr":
                        resultString = string.Join(string.Empty, result.Select(r => r.ReadableIP + "/" + r.Mask + Environment.NewLine).ToArray());
                        break;
                    case "csv-subnet-masks":
                        resultString = string.Join(string.Empty, result.Select(r => "\"" + r.ToStringLongMask() + "\",").ToArray());
                        // need to remove the last ","
                        resultString = resultString.Substring(0, resultString.Length - 1);
                        break;
                    case "csv-cidr":
                        resultString = string.Join(string.Empty, result.Select(r => "\"" + r.ReadableIP + "/" + r.Mask + "\",").ToArray());
                        // remove the last "\","
                        resultString = resultString.Substring(0, resultString.Length - 1);
                        break;
                    case "cisco-ios-route-list":
                        resultString = _ciscoIOSPrefixRouteConfig + Environment.NewLine +
                            string.Join(string.Empty,
                            result.Select(r => "ip route " + r.ToStringLongMask() + " <selected_interface>" + Environment.NewLine
                            ).ToArray());
                        break;
                    case "cisco-ios-acl-list":
                        resultString = _ciscoIOSPrefixACLConfig + string.Join(string.Empty,
                            result.Select(r => " access-list permit ip <any or your VNet address space> " + r.ToStringLongMask() +
                            Environment.NewLine).ToArray());
                        break;
                    default:
                        break;
                }
                // Now the command
                switch (command)
                {
                    case "download":
                        //Generate filename
                        var outputFileName = "Results-UTC-" + DateTime.UtcNow + "-";
                        foreach (string region in regions)
                        {
                            outputFileName = outputFileName + region + "-";
                        }
                        outputFileName = outputFileName.Substring(0, outputFileName.Length - 1) + ".txt";
                        return Json(new { count = result.Count, encodedResultString = resultString, fileName = outputFileName }, JsonRequestBehavior.AllowGet);
                    case "show":
                        return Json(new { count = result.Count, encodedResultString = WebUtility.HtmlEncode(resultString) }, JsonRequestBehavior.AllowGet);
                     default:
                        return WebUtility.HtmlEncode("Unexpected operation command.");
                }
            }
            else // regions = null?
            {
                return null;
            }
        }
    }
}

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class GenerationHelper
    {
        #region Const Definitions

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

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(cacheConnection);
        });
        protected static IConnectionMultiplexer CacheConnection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        internal static string Generate(string[] regions, string[] o365services, string outputformat, bool complement, out int resultCount)
        {
            var resultString = string.Empty;
            resultCount = 0;

            var webGen = new WebGenerator(CacheConnection);

            var regionsAndServices = new string[(regions == null ? 0 : regions.Length) + (o365services == null ? 0 : o365services.Length)];

            if (regions != null)
                // add regions to the array
                Array.Copy(regions, 0, regionsAndServices, 0, regions.Length);
            if (o365services != null)
                // Add o365 services to the array
                Array.Copy(o365services, 0, regionsAndServices, regions == null ? 0 : regions.Length, o365services.Length);
            
            var result = webGen.GetPrefixList(regionsAndServices.ToList(), complement);

            resultCount = result.Count();

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
            return resultString;
        }
    }
}
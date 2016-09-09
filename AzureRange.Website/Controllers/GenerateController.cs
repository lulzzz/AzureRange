using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureRange.Website.Controllers
{
    public class GenerateController : Controller
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

            return null;
        }
    }
}
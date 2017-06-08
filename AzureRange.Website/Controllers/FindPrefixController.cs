using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web.Http;

namespace AzureRange.Website.Controllers
{
    public class FindPrefixController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get([FromUri] string inputIP)
        {

            try
            {
                // try to resolve the IP address
                IPAddress[] ipAddList = Dns.GetHostAddresses(inputIP);

                // String to build output
                var resultstring = inputIP + " resolves to ..." + System.Environment.NewLine;
                // Look for each IP under the hostname...
                foreach (var ipAdd in ipAddList)
                {
                    var inputPrefix = new IPPrefix(ipAdd.ToString() + "/32");
                    var outputPrefix = FindPrefixHelper.FindPrefix(inputPrefix);

                    if (outputPrefix != null)
                    {
                        string prefixLocation = string.Empty;
                        // Check if it's part of an Azure region or O365 service
                        if (outputPrefix.Region != null)
                            prefixLocation = " in region " + outputPrefix.Region;
                        else
                            prefixLocation = " in Office 365 service " + outputPrefix.O365Service;

                        //Build the string result
                        resultstring += (inputPrefix.ToString() + " is part of " + outputPrefix.ToString() + prefixLocation + System.Environment.NewLine);
                    }
                    else
                        resultstring += (inputPrefix.ToString() + " but isn't part of our prefix lists." + System.Environment.NewLine);

                }
                return new string[] { resultstring };
            }
            catch (Exception ex)
            {
                return new string[] { "Invalid IP address or hostname : " + ex.Message  };
            }
            
        }
    }
}
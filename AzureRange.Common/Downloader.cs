using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureRange
{
    public class Downloader
    {
        public static List<IPPrefix> Download()
        {
            string downloadPageAzureCloud = "https://www.microsoft.com/en-ca/download/confirmation.aspx?id=41653";
            string downloadPageAzureChinaCloud = "https://www.microsoft.com/en-ca/download/confirmation.aspx?id=42064";
            string downloadPageO365Cloud = "http://go.microsoft.com/fwlink/?LinkId=533185";

            List<IPPrefix> ipPrefixes = new List<IPPrefix>();

            ipPrefixes.AddRange(AddXMLMSInputFileAzure(downloadPageAzureCloud, IpPrefixType.Azure));
            ipPrefixes.AddRange(AddXMLMSInputFileAzure(downloadPageAzureChinaCloud, IpPrefixType.AzureChina));
            ipPrefixes.AddRange(AddXMLMSInputFileO365(downloadPageO365Cloud));
            

            return ipPrefixes;
        }
        
        //public static void Merge(List<IPPrefix> ipPrefixes)
        //{
        //    var duplicates = new List<IPPrefix>();
        //    IPPrefix previousRange = null;
        //    foreach (var range in ipPrefixes.OrderBy(t => t.FirstIP))
        //    {
        //        if (previousRange != null)
        //        {
        //            if (range.FirstIP >= previousRange.FirstIP
        //                &&
        //                range.LastIP > previousRange.LastIP)
        //            {
        //                previousRange.LastIP = range.LastIP;
        //                duplicates.Add(range);
        //            }
        //        }
        //        previousRange = range;
        //    }

        //    duplicates.ForEach(ipToRemove => ipPrefixes.Remove(ipToRemove));
        //}

        

        private static List<IPPrefix> AddXMLMSInputFileAzure(string downloadURL, IpPrefixType type)
        {
            string dlUrl = string.Empty;
            string dlContent = string.Empty;
            List<IPPrefix> IPPrefixes = new List<IPPrefix>();

            using (var wc = new WebClient())
            {
                dlUrl = wc.DownloadString(downloadURL);
                var result = Regex.Match(dlUrl, "url=(.*)\"");
                dlUrl = result.Groups[1].Value;
                dlContent = wc.DownloadString(dlUrl);
            }

            //For using when testint offline
            //using (streamReader = new StreamReader(@"c:\Users\omartin2\Downloads\PublicIPs_20160719.xml", Encoding.UTF8))
            //{
            //    dlContent = streamReader.ReadToEnd();
            //}

            var xContent = XDocument.Load(new StringReader(dlContent));     // XML document containing the list 

            // Looing in the document sections
            foreach (var xRegion in xContent.Elements().First().Elements())
            {
                foreach (var xIPPrefix in xRegion.Elements())
                {
                    var prefix = new IPPrefix(
                        type,
                        xRegion.Attributes("Name").First().Value,
                        xIPPrefix.Attributes("Subnet").First().Value
                    );
                    IPPrefixes.Add(prefix);
                }
            }
            return IPPrefixes;
        }
        private static List<IPPrefix> AddXMLMSInputFileO365(string downloadURL)
        {
            string dlUrl = string.Empty;
            string dlContent = string.Empty;
            List<IPPrefix> IPPrefixes = new List<IPPrefix>();

            using (var wc = new WebClient())
            {
                dlContent = wc.DownloadString(downloadURL);
            }

            var xContent = XDocument.Load(new StringReader(dlContent));     // XML document containing the list 

            // Looping in the document sections
            foreach (var xO365ProductName in xContent.Elements().First().Elements())
            {
                foreach (var xAddressType in xO365ProductName.Elements())
                {
                    if (xAddressType.FirstAttribute.Value == "IPv4")
                    {
                        foreach (var xIPPrefix in xAddressType.Elements())
                        {
                            var prefix = 
                                new IPPrefix(IpPrefixType.Office365, xO365ProductName.FirstAttribute.Value,xIPPrefix.Value);
                            IPPrefixes.Add(prefix);
                        }
                    }
                }
            }
            return IPPrefixes;
        }
    }
}

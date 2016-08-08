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
            string downloadPage = "https://www.microsoft.com/en-ca/download/confirmation.aspx?id=41653";
            string dlUrl = string.Empty;
            string dlContent = string.Empty;
            List<IPPrefix> IPPrefixes = new List<IPPrefix>();

            using (var wc = new WebClient())
            {
                dlUrl = wc.DownloadString(downloadPage);
                var result = Regex.Match(dlUrl, "url=(.*)\"");
                dlUrl = result.Groups[1].Value;
                dlContent = wc.DownloadString(dlUrl);
            }

            /*
            using (streamReader = new StreamReader(@"c:\Users\omartin2\Downloads\PublicIPs_20160719.xml", Encoding.UTF8))
            {
                dlContent = streamReader.ReadToEnd();
            }*/
            
            var xContent = XDocument.Load(new StringReader(dlContent));     // XML document containing the list 

            // Looing in the document sections
            foreach (var xRegion in xContent.Elements().First().Elements())
            {
                foreach (var xIPPrefix in xRegion.Elements())
                {
                    var prefix = new IPPrefix(
                        xRegion.Attributes("Name").First().Value,
                        xIPPrefix.Attributes("Subnet").First().Value
                    );
                    IPPrefixes.Add(prefix);
                }
            }

            return IPPrefixes;
        }
    }
}

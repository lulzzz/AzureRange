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
        public static List<IpRange> Download()
        {
            string downloadPage = "https://www.microsoft.com/en-ca/download/confirmation.aspx?id=41653";
            string dlUrl = string.Empty;
            string dlContent = string.Empty;

            using (var wc = new WebClient())
            {
                dlUrl = wc.DownloadString(downloadPage);
                var result = Regex.Match(dlUrl, "url=(.*)\"");
                dlUrl = result.Groups[1].Value;
                dlContent = wc.DownloadString(dlUrl);
            }

            var ranges = new List<IpRange>();

            var xContent = XDocument.Load(new StringReader(dlContent));

            foreach (var xRegion in xContent.Elements().First().Elements())
            {
                foreach (var xRange in xRegion.Elements())
                {
                    var range = new IpRange(
                        xRegion.Attributes("Name").First().Value,
                        xRange.Attributes("Subnet").First().Value
                    );
                    ranges.Add(range);
                }
            }

            return ranges;
        }
    }
}

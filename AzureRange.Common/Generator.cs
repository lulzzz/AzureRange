using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRange
{
    public class Generator
    {
        public static List<IpRange> Not(List<IpRange> ranges)
        {
            var compRange = new List<IpRange>();
            IpRange previousRange = ranges.OrderBy(r => r.NetworkDecimal).First();
            foreach (var range in ranges.OrderBy(r => r.NetworkDecimal))
            {
                if (previousRange != null)
                {
                    var result = Generator.ProcessGap(previousRange, range);
                    if (result != null)
                        compRange.AddRange(result);
                }
                previousRange = range;
            }
            return compRange.OrderBy(r => r.NetworkDecimal).ToList();
        }
        public static List<IpRange> ProcessGap(IpRange previousRange, IpRange range)
        {
            var gapSubnets = new List<IpRange>();

            var subnet = GetSubnetsBetween(previousRange, range);
            if (subnet == null)
                return null;

            gapSubnets.Add(subnet);
            var innerRanges = ProcessGap(previousRange, subnet);
            if (innerRanges != null)
                gapSubnets.AddRange(innerRanges);

            return gapSubnets;
        }

        public static IpRange GetSubnetsBetween(IpRange lowerBound, IpRange upperBound)
        {
            var lastIpInBetween = upperBound.NetworkDecimal - 1;
            IpRange lastNetwork = null;
            for (var i = 32; i > 0; i--)
            {
                var mask = (long)Math.Pow(2, i) - 1;
                var shiftedMask = mask << 32 - i;
                var network = new IpRange(lastIpInBetween & shiftedMask, i);

                if (!(
                    network.NetworkDecimal > lowerBound.LastIp &&
                    network.LastIp < upperBound.NetworkDecimal
                    )
                    )
                {
                    if (lastNetwork != null)
                        return lastNetwork;
                    else
                        return null;
                }

                lastNetwork = network;
            }

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureRange
{
    public class IpRange: IEquatable<IpRange>
    {
        public IpRange()
        {

        }
        public IpRange(long network, int i)
        {
            NetworkDecimal = network;
            Mask = i;
        }

        public IpRange (string region, string rawRange)
        {
            Region = region;
            RawRange = rawRange;
            RawRangeSubnet = RawRange.Substring(0, RawRange.IndexOf("/"));

            Mask = Convert.ToInt32(RawRange.Substring(RawRange.IndexOf("/")+1));

            var subnetParts = RawRangeSubnet.Split('.');
            long subnetDecimal = (long)Convert.ToInt32(subnetParts[0]) * 256 * 256 * 256;
            subnetDecimal += (long)Convert.ToInt32(subnetParts[1]) * 256 * 256;
            subnetDecimal += (long)Convert.ToInt32(subnetParts[2]) * 256;
            subnetDecimal += (long)Convert.ToInt32(subnetParts[3]);
            NetworkDecimal = subnetDecimal;
        }

        public string Region { get; set; }
        public string RawRange { get; set; }
        public string RawRangeSubnet { get; set; }
        public long NetworkDecimal { get; set; }
        public int Mask { get; set; }

        public long LastIp
        {
            get
            {
                return NetworkDecimal + (long)Math.Pow(2, (32 - Mask)) - 1;
            }
        }

        public string ReadableLastIP
        {
            get
            {
                return IPAddress.Parse(LastIp.ToString()).ToString();
            }
        }

        public string ReadableIP
        {
            get
            {
                return IPAddress.Parse(NetworkDecimal.ToString()).ToString();
            }
        }
        public override string ToString()
        {
            return ReadableIP + "/" + Mask;
        }

        public bool Equals(IpRange other)
        {
            return other.NetworkDecimal.Equals(this.NetworkDecimal) && other.Mask.Equals(this.Mask);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureRange
{
    public class IPPrefix: IEquatable<IPPrefix>
    {
        public IPPrefix()
        {
            //Pourquoi ce constructeur
        }
        public IPPrefix(long pLongNetwork, int pIntMask)
        {
            FirstIP = pLongNetwork;
            Mask = pIntMask;
        }

        public IPPrefix (string pStrRegion, string pStrRawPrefix)
        {
            Region = pStrRegion;
            RawPrefix = pStrRawPrefix;
            RawPrefixSubnet = RawPrefix.Substring(0, RawPrefix.IndexOf("/"));

            Mask = Convert.ToInt32(RawPrefix.Substring(RawPrefix.IndexOf("/")+1));

            var subnetParts = RawPrefixSubnet.Split('.');
            long subnetDecimal = (long)Convert.ToInt32(subnetParts[0]) * 256 * 256 * 256;
            subnetDecimal += (long)Convert.ToInt32(subnetParts[1]) * 256 * 256;
            subnetDecimal += (long)Convert.ToInt32(subnetParts[2]) * 256;
            subnetDecimal += (long)Convert.ToInt32(subnetParts[3]);
            FirstIP = subnetDecimal;
        }

        public string Region { get; set; }
        public string RawPrefix { get; set; }
        public string RawPrefixSubnet { get; set; }
        public long FirstIP { get; set; }
        public int Mask { get; set; }

        public long LastIP
        {
            get
            {
                return FirstIP + (long)Math.Pow(2, (32 - Mask)) - 1;
            }
        }

        public string ReadableLastIP
        {
            get
            {
                return IPAddress.Parse(LastIP.ToString()).ToString();
            }
        }

        public string ReadableIP
        {
            get
            {
                return IPAddress.Parse(FirstIP.ToString()).ToString();
            }
        }
        public override string ToString()
        {
            return ReadableIP + "/" + Mask;
        }

        public bool Equals(IPPrefix other)
        {
            return other.FirstIP.Equals(this.FirstIP) && other.Mask.Equals(this.Mask);
        }
    }
}

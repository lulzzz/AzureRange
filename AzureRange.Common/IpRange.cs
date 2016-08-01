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
        public IPPrefix(UInt32 pUInt32_Network, int pInt_Mask)
        {
            FirstIP = pUInt32_Network;
            Mask = pInt_Mask;
        }

        public IPPrefix (string pStrRawPrefix)
        {
            RawPrefix = pStrRawPrefix;
            RawPrefixSubnet = RawPrefix.Substring(0, RawPrefix.IndexOf("/"));

            Mask = Convert.ToInt32(RawPrefix.Substring(RawPrefix.IndexOf("/") + 1));

            var subnetParts = RawPrefixSubnet.Split('.');
            UInt32 subnetDecimal = (UInt32)Convert.ToInt32(subnetParts[0]) * 256 * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[1]) * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[2]) * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[3]);
            FirstIP = subnetDecimal;
        }
        public IPPrefix (string pStrRegion, string pStrRawPrefix)
        {
            Region = pStrRegion;
            RawPrefix = pStrRawPrefix;
            RawPrefixSubnet = RawPrefix.Substring(0, RawPrefix.IndexOf("/"));

            Mask = Convert.ToInt32(RawPrefix.Substring(RawPrefix.IndexOf("/")+1));

            var subnetParts = RawPrefixSubnet.Split('.');
            UInt32 subnetDecimal = (UInt32)Convert.ToInt32(subnetParts[0]) * 256 * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[1]) * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[2]) * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[3]);
            FirstIP = subnetDecimal;
        }

        public string Region { get; set; }
        public string RawPrefix { get; set; }
        public string RawPrefixSubnet { get; set; }
        public UInt32 FirstIP { get; set; }
        public int Mask { get; set; }

        public UInt32 LastIP
        {
            get
            {
                return FirstIP + (UInt32)Math.Pow(2, (32 - Mask)) - 1;
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

        public string ToStringLongMask()
        {
            //return format 10.10.10.0 255.255.255.0
            return ReadableIP + " " + "255.255.255.0";
        }

        public bool Equals(IPPrefix other)
        {
            return other.FirstIP.Equals(this.FirstIP) && other.Mask.Equals(this.Mask);
        }
    }
}

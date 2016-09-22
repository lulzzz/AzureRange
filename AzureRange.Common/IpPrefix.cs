using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureRange
{
    public class IPPrefix: IEquatable<IPPrefix>, ICloneable
    {
        public IPPrefix()
        {
        }
        public IPPrefix(UInt32 pUInt32_Network, int pInt_Mask)
        {
            FirstIP = pUInt32_Network;
            Mask = pInt_Mask;
        }
        public IPPrefix (string RawPrefix)
        {
            this.RawPrefix = RawPrefix;
            RawPrefixSubnet = this.RawPrefix.Substring(0, this.RawPrefix.IndexOf("/"));

            Mask = Convert.ToInt32(this.RawPrefix.Substring(this.RawPrefix.IndexOf("/") + 1));

            #region FirstIp conversion

            var subnetParts = RawPrefixSubnet.Split('.');
            UInt32 subnetDecimal = (UInt32)Convert.ToInt32(subnetParts[0]) * 256 * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[1]) * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[2]) * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[3]);
            FirstIP = subnetDecimal;

            #endregion
        }
        public IPPrefix (string RegionOrO365Service, string pStrRawPrefix, bool isAzure)
        {
            if (isAzure)
            {
                Region = RegionOrO365Service;
                O365Service = null;
            }
            else
            {
                Region = null;
                O365Service = RegionOrO365Service;
            }
            RawPrefix = pStrRawPrefix;

            // if no / is present, add /32
            if (RawPrefix.IndexOf("/") == -1)
                RawPrefix = RawPrefix + "/32";

            RawPrefixSubnet = RawPrefix.Substring(0, RawPrefix.IndexOf("/"));
            Mask = Convert.ToInt32(RawPrefix.Substring(RawPrefix.IndexOf("/")+1));

            var subnetParts = RawPrefixSubnet.Split('.');
            UInt32 subnetDecimal = (UInt32)Convert.ToInt32(subnetParts[0]) * 256 * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[1]) * 256 * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[2]) * 256;
            subnetDecimal += (UInt32)Convert.ToInt32(subnetParts[3]);
            FirstIP = subnetDecimal;
        }

        public string O365Service { get; set; }
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

        public string ReadableMask
        {
            get
            {
                // If mask == 32, does it overloads to 0...
                var mask = Mask < 32 ? ~(0xFFFFFFFF >> Mask) : 0xFFFFFFFF;
                return IPAddress.Parse(mask.ToString()).ToString();
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
            return ReadableIP + " " + ReadableMask;
        }

        public bool Equals(IPPrefix other)
        {
            return other.FirstIP.Equals(this.FirstIP) && other.Mask.Equals(this.Mask);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

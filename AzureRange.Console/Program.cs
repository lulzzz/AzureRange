using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AzureRange.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Load the XML file into ranges
            var ipPrefixes = Downloader.Download();
            // ADD THE BASIC PRIVATE NETWORKS to the list, 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, 224.0.0.0/3
            // Order the ranges by increasing network ID
            var results = Generator.Not(ipPrefixes);
            // Extract a gap
            

            Debugger.Break();
        }
    }
}

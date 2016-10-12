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

namespace AzureRange.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Variable declaration 
            List<IPPrefix> ipPrefixesInput;
            List<IPPrefix> ipPrefixesOutput;

            // Load the XML file into ranges
            ipPrefixesInput = Downloader.Download();
            // add default private network prefixes
            ipPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
            ipPrefixesInput.Add(new IPPrefix("10.0.0.0/8"));
            ipPrefixesInput.Add(new IPPrefix("172.16.0.0/12"));
            ipPrefixesInput.Add(new IPPrefix("169.254.0.0/16"));
            ipPrefixesInput.Add(new IPPrefix("192.168.0.0/16"));
            ipPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));

            Generator.Dedupe(ipPrefixesInput);

            // Order the ranges by increasing network ID
            ipPrefixesOutput = Generator.Not(ipPrefixesInput);
            foreach (IPPrefix l_currentPrefix in ipPrefixesOutput)
            {
                Console.Write(String.Concat(l_currentPrefix.ReadableIP,"/",l_currentPrefix.Mask,"\n"));
            }
            Console.Write("Done!\n");
            Debugger.Break();
        }
    }
}

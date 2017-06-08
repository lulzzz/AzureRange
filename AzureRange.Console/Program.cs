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
            List<IPPrefix> ipPrefixesInput = new List<IPPrefix>();
            List<IPPrefix> ipPrefixesOutput = new List<IPPrefix>();

            // Load the XML file into ranges
            ipPrefixesInput = Downloader.Download();
            // add default private network prefixes
            //ipPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
            //ipPrefixesInput.Add(new IPPrefix("10.0.0.0/8"));
            //ipPrefixesInput.Add(new IPPrefix("172.16.0.0/12"));
            //ipPrefixesInput.Add(new IPPrefix("169.254.0.0/16"));
            //ipPrefixesInput.Add(new IPPrefix("192.168.0.0/16"));
            //ipPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));

            //Generator.Dedupe(ipPrefixesInput);
            //ipPrefixesOutput 
            ipPrefixesOutput = Generator.Summarize(ipPrefixesInput);
            var testPrefix = new IPPrefix("216.32.180.38/31");
            var solutionPrefix = Generator.GetContainingPrefix(testPrefix,ipPrefixesOutput);

            // Order the ranges by increasing network ID
            //ipPrefixesOutput = Generator.Not(ipPrefixesInput);

            //foreach (IPPrefix l_currentPrefix in ipPrefixesOutput)
            //{
            //    Console.Write(String.Concat(l_currentPrefix.ReadableIP,"/",l_currentPrefix.Mask,"\n"));
            //}
            if (solutionPrefix!=null)
            {
                string prefixLocation;
                if (solutionPrefix.Region != null)
                    prefixLocation = solutionPrefix.Region;
                else 
                    prefixLocation = solutionPrefix.O365Service;

                Console.Write("Found " + testPrefix.ToString() + " in : \n" + solutionPrefix.ToString() + ";" + prefixLocation + "\n");
            }
            else Console.Write("Unable to find prefix " + testPrefix.ToString() + " in list.\n");

            Console.Write("Done!\n");
            Console.ReadKey();
            //Debugger.Break();
        }
    }
}

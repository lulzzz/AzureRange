using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AzureRange.Website;
using Microsoft.Azure.WebJobs.Extensions.Timers;


namespace AzureRange.WebJob
{
    public class Functions
    {
        public static void TimerJob([TimerTrigger("01:00:00")] TimerInfo timerInfo) //, TextWriter log)
        {
            Console.WriteLine("Startup of background refresh task... \n");

            var webGen = new WebGenerator();
            var prefixList = new List<IPPrefix>();

            // Delete temp files...
            var filepath = Path.GetTempPath() + "\\ranges.txt";
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                Console.WriteLine("Deleted " + filepath + " succesfully");
            }
            filepath = Path.GetTempPath() + "\\AzureRegions.txt";
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                Console.WriteLine("Deleted " + filepath + " succesfully");
            }
            filepath = Path.GetTempPath() + "\\O365Services.txt";
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
                Console.WriteLine("Deleted " + filepath + " succesfully");
            }
            // retrieves, downloads and save the prefix list on temp storage (local disk)
            Console.WriteLine("Retreiving prefixes...");
            prefixList = webGen.CachedList;
            Console.WriteLine("Retreiving Azure regions...");
            webGen.GetRegions(); // will save region file on disk
            Console.WriteLine("Retreiving o365 services...");
            webGen.GetO365Services(); // will save o365 file on disk
            Console.WriteLine("Background refresh tasks done for now. Will restart soon.");
        }
    }
}

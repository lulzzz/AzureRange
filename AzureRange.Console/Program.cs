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
            var ranges = Downloader.Download();
            var results = Generator.Not(ranges);

            Debugger.Break();
        }
    }
}

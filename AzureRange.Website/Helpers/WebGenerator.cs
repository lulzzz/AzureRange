using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace AzureRange.Website
{
    public class WebGenerator
    {
        private TelemetryClient _telemetry;
        private List<IPPrefix> _localList;
        private List<IPPrefix> GetDefaultSubnets()
        {
            var ipPPrefixesInput = new List<IPPrefix>();
            ipPPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
            ipPPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));
            return ipPPrefixesInput;
        }
        public WebGenerator()
        {
            _telemetry = new Microsoft.ApplicationInsights.TelemetryClient();
        }
        public List<IPPrefix> CachedList
        {
            get
            {
                if (_localList != null)
                    return _localList;

                var jsonIpPrefixList = string.Empty;
                
                // Validate if ranges already exists and is recent enough (should be updated by WebJob)
                var filepath = Path.GetTempPath() + "\\ranges.txt";
                if (File.Exists(filepath) && (DateTime.Now - File.GetLastWriteTime(filepath)).TotalHours < 1)
                    jsonIpPrefixList = File.ReadAllText(filepath);

                if (!string.IsNullOrEmpty(jsonIpPrefixList))
                {
                    return JsonConvert.DeserializeObject<List<IPPrefix>>(jsonIpPrefixList);
                }
                else
                {
                    var innerStopWatch = Stopwatch.StartNew();
                    CachedList = Downloader.Download();
                    _telemetry.TrackDependency("ExternalWebsite", "Download", DateTime.Now, innerStopWatch.Elapsed, true);

                    return _localList;
                }
            }
            private set
            {
                var list = JsonConvert.SerializeObject(value);
                var filepath = Path.GetTempPath() + "\\ranges.txt";
                File.WriteAllText(filepath, JsonConvert.SerializeObject(value));

                _localList = value;
            }
        }
        public List<IPPrefix> GetPrefixList(List<string> regionsAndO365Service, bool complement, bool summarize)
        {
            var stopWatch = Stopwatch.StartNew();

            #region TempFileNameGeneration

            // create string of all regions and O365 services to hash
            var unhashedfilename = string.Join(".", regionsAndO365Service.ToArray()) + complement.ToString() + summarize.ToString() + ".txt";
            // hash it - otherwise temp file name too long
            var sha256 = new SHA256CryptoServiceProvider();
            var hashedfilename = new StringBuilder();
            byte[] byte_hashedfilename = sha256.ComputeHash(Encoding.UTF8.GetBytes(unhashedfilename), 0, Encoding.UTF8.GetByteCount(unhashedfilename));

            foreach (byte theByte in byte_hashedfilename)
            {
                hashedfilename.Append(theByte.ToString("x2"));
            }
            var filepath = Path.GetTempPath() + hashedfilename.ToString() + ".txt";

            #endregion TempFileNameGeneration
            
            var cachedResult = string.Empty;
            List<IPPrefix> result = null;

            if (File.Exists(filepath) && (DateTime.Now - File.GetCreationTime(filepath)).TotalHours < 8)
                cachedResult = File.ReadAllText(filepath);

            if (!string.IsNullOrEmpty(cachedResult))
            {
                result = JsonConvert.DeserializeObject<List<IPPrefix>>(cachedResult);
            }
            else
            {
                var localList = (List<IPPrefix>)CachedList.Clone();

                localList.RemoveAll(m => !regionsAndO365Service.Contains(m.Region) && !regionsAndO365Service.Contains(m.O365Service));

                // Remove duplicates between Azure and Office 365 regions
                Generator.Dedupe(localList);

                // Return the complement of Azure Subnets
                if (complement)
                {
                    // Add default subnets - mandatory to exclude 0.0.0.0/8 and class E IP addresses
                    localList.AddRange(GetDefaultSubnets());
                    // Calculate the complement and return it
                    result = Generator.Not(localList);
                    _telemetry.TrackMetric("GenerateComplement", stopWatch.Elapsed.TotalMilliseconds);
                }
                else
                {
                    // Generate the list of networks for the region(s) and reorder it for return 
                    result = localList.OrderBy(r => r.FirstIP).ToList();
                    _telemetry.TrackMetric("GenerateNoComplement", stopWatch.Elapsed.TotalMilliseconds);
                }

                // Do we summarize?
                if (summarize)
                {
                    _telemetry.TrackMetric("GenerateSummarized", stopWatch.Elapsed.TotalMilliseconds);
                    result = Generator.Summarize(result);
                }

                File.WriteAllText(filepath, JsonConvert.SerializeObject(result));
            }
            return result;
        }
        public List<AzureRegion> GetRegions()
        {
            var jsonRegion = string.Empty;
            List<AzureRegion> azureRegion = new List<AzureRegion>();
            
            var filepath = Path.GetTempPath() + "\\AzureRegions.txt";

            if (File.Exists(filepath) && (DateTime.Now - File.GetLastWriteTime(filepath)).TotalHours < 1)
                jsonRegion = File.ReadAllText(filepath);

            if (!string.IsNullOrEmpty(jsonRegion))
            {
                azureRegion = JsonConvert.DeserializeObject<List<AzureRegion>>(jsonRegion);
            }
            else
            {
                var regionList = CachedList.Select(f => f.Region).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().OrderBy(t => t).ToList();
                var regionManager = new RegionAndO365ServiceManager();
                azureRegion = regionManager.GetAzureRegions(regionList);
                
                File.WriteAllText(filepath, JsonConvert.SerializeObject(azureRegion));

            }
            return azureRegion;
        }
        public List<O365Service> GetO365Services()
        {
            var filepath = Path.GetTempPath() + "\\O365Services.txt";
            var jsonO365Service = string.Empty;
            List<O365Service> o365Services = new List<O365Service>();

            if (File.Exists(filepath) && (DateTime.Now - File.GetLastWriteTime(filepath)).TotalHours < 1)
            {
                jsonO365Service = File.ReadAllText(filepath);
            }

            if (!string.IsNullOrEmpty(jsonO365Service))
            {
                o365Services = JsonConvert.DeserializeObject<List<O365Service>>(jsonO365Service);
            }
            else
            {
                var o365serviceList = CachedList.Select(f => f.O365Service).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().OrderBy(t => t).ToList();
                var o365Manager = new RegionAndO365ServiceManager();
                o365Services = o365Manager.GetO365Services(o365serviceList);

                File.WriteAllText(filepath, JsonConvert.SerializeObject(o365Services));
            }
            return o365Services;
        }



    }
}
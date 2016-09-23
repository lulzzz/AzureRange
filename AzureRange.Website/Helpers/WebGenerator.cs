using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public WebGenerator(IConnectionMultiplexer connection)
        {
            RedisCache = connection;
            _telemetry = new Microsoft.ApplicationInsights.TelemetryClient();
        }
        public IConnectionMultiplexer RedisCache
        {
            get;
            private set;
        }
        public List<IPPrefix> CachedList
        {
            get
            {
                if (_localList != null)
                    return _localList;

                var stopWatch = Stopwatch.StartNew();

                var db = RedisCache.GetDatabase();
                var jsonIpPrefixList = string.Empty;
                try
                {
                    jsonIpPrefixList = db.StringGet("ranges");
                }
                catch (TimeoutException)
                {
                }

                _telemetry.TrackDependency("Redis", "GetRanges", DateTime.Now, stopWatch.Elapsed, true);

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

                var stopWatch = Stopwatch.StartNew();
                var db = RedisCache.GetDatabase();
                try
                {
                    db.StringSet("ranges", list, TimeSpan.FromHours(1));
                }
                catch (TimeoutException)
                { }

                _telemetry.TrackDependency("Redis", "PutRanges", DateTime.Now, stopWatch.Elapsed, true);
                _localList = value;
            }
        }
        public List<AzureRegion> GetRegions()
        {
            var db = RedisCache.GetDatabase();
            var jsonRegion = string.Empty;
            List<AzureRegion> azureRegion = new List<AzureRegion>();

            try
            {
#if DEBUG
                jsonRegion = db.StringGet("AzureRegions");
#endif
            }
            catch (TimeoutException)
            {
            }

            if (!string.IsNullOrEmpty(jsonRegion))
            {
                azureRegion = JsonConvert.DeserializeObject<List<AzureRegion>>(jsonRegion);
            }
            else
            {
                var regionList = CachedList.Select(f => f.Region).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().OrderBy(t => t).ToList();
                var regionManager = new RegionAndO365ServiceManager();
                azureRegion = regionManager.GetAzureRegions(regionList);
                try
                {
#if DEBUG
                    db.StringSet("AzureRegions", JsonConvert.SerializeObject(azureRegion), TimeSpan.FromHours(1));
#endif
                }
                catch (TimeoutException)
                {
                }
            }
            return azureRegion;
        }

        public List<O365Service> GetO365Services()
        {
            var db = RedisCache.GetDatabase();
            var jsonO365Service = string.Empty;
            List<O365Service> o365Services = new List<O365Service>();

            try
            {
#if DEBUG
                jsonO365Service = db.StringGet("O365Services");
#endif
            }
            catch (TimeoutException)
            {
            }

            if (!string.IsNullOrEmpty(jsonO365Service))
            {
                o365Services = JsonConvert.DeserializeObject<List<O365Service>>(jsonO365Service);
            }
            else
            {


                var o365serviceList = CachedList.Select(f => f.O365Service).Where(f => !string.IsNullOrWhiteSpace(f)).Distinct().OrderBy(t => t).ToList();
                // replace with api call to or cache list using redis http://mscloudips.azurewebsites.net/api/azureips/operation/listregions
                var o365Manager = new RegionAndO365ServiceManager();
                o365Services = o365Manager.GetO365Services(o365serviceList);

                try
                {
#if DEBUG
                    db.StringSet("O365Services", JsonConvert.SerializeObject(o365Services), TimeSpan.FromHours(1));
#endif
                }
                catch (TimeoutException)
                {
                }
            }
            return o365Services;
        }

        public List<IPPrefix> GetPrefixList(List<string> regionsAndO365Service, bool complement)
        {
            var stopWatch = Stopwatch.StartNew();

            var cachedResult = string.Empty;
            List<IPPrefix> result = null;
            var db = RedisCache.GetDatabase();
            // Create the key for RedisCache
            var key = string.Join("|",regionsAndO365Service.ToArray()) + complement.ToString();

            try
            {
                // See if results for this query were calculated before
                cachedResult = db.StringGet(key);
            }
            catch (TimeoutException){}
   
            if (!string.IsNullOrEmpty(cachedResult))
            {
                result = JsonConvert.DeserializeObject<List<IPPrefix>>(cachedResult);
            }
            else
            { 
                var localList = (List<IPPrefix>)CachedList.Clone();

                localList.RemoveAll(m => !regionsAndO365Service.Contains(m.Region) && !regionsAndO365Service.Contains(m.O365Service));
                
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
                try
                {
                    db.StringSet(key, JsonConvert.SerializeObject(result), TimeSpan.FromHours(1));
                }
                catch (TimeoutException) { }
            }
            return result;
        }
    }
}
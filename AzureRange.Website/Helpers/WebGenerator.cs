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
        private List<IPPrefix> _localList;
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
            var list = CachedList.Select(f => f.Region).Where(f=>!string.IsNullOrWhiteSpace(f)).Distinct().OrderBy(t=>t).ToList();

            var regionManager = new RegionManager();
            var regions = regionManager.GetRegions(list);
            //regions.Add(new AzureRegion { Id = "private", Name = "Private IP Address Space", Location = "n/a" });
            return regions;
        }
        public List<IPPrefix> GetComplementPrefixList(List<string> regions)
        {
            var stopWatch = Stopwatch.StartNew();

            var cachedResult = string.Empty;
            List<IPPrefix> result = null;
            var db = RedisCache.GetDatabase();
            
            var key = string.Join("|",regions.ToArray());
            try
            {
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

                localList.RemoveAll(m => !regions.Contains(m.Region));

                // Add default subnets - mandatory to exclude 0.0.0.0/8 and class E IP addresses
                localList.AddRange(GetDefaultSubnets());
                
                // Return the complement of Azure Subnets
                result = Generator.Not(localList);
                try
                {
                    db.StringSet(key, JsonConvert.SerializeObject(result), TimeSpan.FromHours(1));
                }
                catch (TimeoutException) { }
            }

            _telemetry.TrackMetric("Generate", stopWatch.Elapsed.TotalMilliseconds);
            //_telemetry.TrackMetric("SourceIP", ...);
            // _telemetry.TrackMetric ("REQUESTING IP!!", client.IP);

            return result;
        }
        //*Don't know if I should use another private member of the class...*/
        public List<IPPrefix> GetPrefixList(List<string> regions)
        {
            var stopWatch = Stopwatch.StartNew();

            var cachedResult = string.Empty;
            List<IPPrefix> result = null;
            var db = RedisCache.GetDatabase();

            var key = string.Join("|", regions.ToArray());
            try
            {
                cachedResult = db.StringGet(key);
            }
            catch (TimeoutException) { }

            if (!string.IsNullOrEmpty(cachedResult))
            {
                result = JsonConvert.DeserializeObject<List<IPPrefix>>(cachedResult);
            }
            else
            {
                var localList = (List<IPPrefix>)CachedList.Clone();

                localList.RemoveAll(m => !regions.Contains(m.Region));

                // Add default subnets - mandatory to exclude 0.0.0.0/8 and class E IP addresses
                localList.AddRange(GetDefaultSubnets());

                // Return the complement of Azure Subnets
                result = Generator.Not(localList);
                try
                {
                    db.StringSet(key, JsonConvert.SerializeObject(result), TimeSpan.FromHours(1));
                }
                catch (TimeoutException) { }
            }

            _telemetry.TrackMetric("Generate", stopWatch.Elapsed.TotalMilliseconds);
            //_telemetry.TrackMetric("SourceIP", ...);
            // _telemetry.TrackMetric ("REQUESTING IP!!", client.IP);

            return result;
        }

        private List<IPPrefix> GetDefaultSubnets()
        {
            var ipPPrefixesInput = new List<IPPrefix>();
            ipPPrefixesInput.Add(new IPPrefix("0.0.0.0/8"));
            ipPPrefixesInput.Add(new IPPrefix("224.0.0.0/3"));
            return ipPPrefixesInput;
        }
    }
}
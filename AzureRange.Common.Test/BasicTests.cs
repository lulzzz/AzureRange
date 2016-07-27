using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AzureRange.Common.Test
{
    [TestClass]
    public class BasicTests
    {
        private List<IpRange> _downloadedContent;

        [TestInitialize]
        public void Initialize()
        {
            _downloadedContent = Downloader.Download();
        }

        [TestMethod]
        public void IpRangeTest()
        {
            var testIpRange = new IpRange("NA", "192.168.0.1/24");
            Assert.AreEqual(testIpRange.ReadableIP, "192.168.0.1");
            Assert.AreEqual(testIpRange.Mask, 24);
        }

        [TestMethod]
        public void BetweenSimpleTest()
        {
            var lowerBound = new IpRange("na", "192.168.0.0/24");
            var upperBound = new IpRange("na", "192.168.2.0/24");

            var result = Generator.GetSubnetsBetween(lowerBound, upperBound);
            Assert.AreEqual(result.ReadableIP, "192.168.1.0");
            Assert.AreEqual(result.Mask, 24);
        }

        [TestMethod]
        public void BetweenTwoSubnetTest()
        {
            var lowerBound = new IpRange("na", "192.168.0.0/24");
            var upperBound = new IpRange("na", "192.168.3.0/24");

            var result = Generator.GetSubnetsBetween(lowerBound, upperBound);
            Assert.AreEqual(result.ReadableIP, "192.168.2.0");
            Assert.AreEqual(result.Mask, 24);

            result = Generator.GetSubnetsBetween(lowerBound, result);
            Assert.AreEqual(result.ReadableIP, "192.168.1.0");
            Assert.AreEqual(result.Mask, 24);

            result = Generator.GetSubnetsBetween(lowerBound, result);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void BetweenAllDistinctTest()
        {
            var results = Generator.Not(_downloadedContent);

            var r = results.Distinct().ToList();
            results.RemoveAll(t => r.Contains(t));
            Assert.IsTrue(results.Count == 0);
        }

        [TestMethod]
        public void BackToSourceTest()
        {
            var results = Generator.Not(_downloadedContent);
            results.AddRange(_downloadedContent);


            var gaps = Generator.Not(results);
            Assert.AreEqual(gaps.Distinct().Count(), gaps.Count);
            Assert.IsTrue(gaps.Count == 0);
        }

        [TestMethod]
        public void ValidateNoGapsTest()
        {
            var results = Generator.Not(_downloadedContent);
            results.AddRange(_downloadedContent);

            IpRange previousRange = null;
            foreach(var range in results.OrderBy(t => t.NetworkDecimal))
            {
                if (previousRange != null)
                {
                    Assert.AreEqual(previousRange.LastIp + 1, range.NetworkDecimal);
                }
                previousRange = range;
            }
        }
    }
}
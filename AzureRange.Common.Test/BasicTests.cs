using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AzureRange.Website;

namespace AzureRange.Common.Test
{
    [TestClass]
    public class BasicTests
    {
        private List<IPPrefix> _downloadedContent;

        [TestInitialize]
        public void Initialize()
        {
            _downloadedContent = Downloader.Download();
        }

        [TestMethod]
        public void IPPrefixTest()
        {
            var testIPPrefix = new IPPrefix(IpPrefixType.Azure, "NA", "192.168.0.1/24");
            Assert.AreEqual(testIPPrefix.ReadableIP, "192.168.0.1");
            Assert.AreEqual(testIPPrefix.Mask, 24);
        }

        [TestMethod]
        public void BetweenSimpleTest()
        {
            var lowerBound = new IPPrefix(IpPrefixType.Azure, "na", "192.168.0.0/24");
            var upperBound = new IPPrefix(IpPrefixType.Azure, "na", "192.168.2.0/24");

            var result = Generator.GetPrefixesBetween(lowerBound, upperBound);
            Assert.AreEqual(result.ReadableIP, "192.168.1.0");
            Assert.AreEqual(result.Mask, 24);
        }

        [TestMethod]
        public void BetweenTwoSubnetTest()
        {
            var lowerBound = new IPPrefix(IpPrefixType.Azure, "na", "192.168.0.0/24");
            var upperBound = new IPPrefix(IpPrefixType.Azure, "na", "192.168.3.0/24");

            var result = Generator.GetPrefixesBetween(lowerBound, upperBound);
            Assert.AreEqual(result.ReadableIP, "192.168.2.0");
            Assert.AreEqual(result.Mask, 24);

            result = Generator.GetPrefixesBetween(lowerBound, result);
            Assert.AreEqual(result.ReadableIP, "192.168.1.0");
            Assert.AreEqual(result.Mask, 24);
            Assert.AreEqual(result.ReadableMask, "255.255.255.0");

            result = Generator.GetPrefixesBetween(lowerBound, result);
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
        public void ValidateNoOverlap()
        {
            var copy = _downloadedContent.Select(item => (IPPrefix)item.Clone()).ToList();

            Generator.Dedupe(copy);

            IPPrefix previousRange = null;
            foreach (var range in copy.OrderBy(t => t.FirstIP))
            {
                if (previousRange != null)
                {
                    Assert.IsTrue(previousRange.LastIP < range.FirstIP, range.O365Service + ": " + previousRange.LastIP + " > " + range.FirstIP);
                }
                previousRange = range;
            }
        }

        [TestMethod]
        public void ValidateNoGapsTest()
        {
            var copy = _downloadedContent.Select(item => (IPPrefix)item.Clone()).ToList();
            Generator.Dedupe(copy);

            var results = Generator.Not(copy);
            results.AddRange(copy);

            IPPrefix previousRange = null;
            foreach(var range in results.OrderBy(t => t.FirstIP))
            {
                if (previousRange != null)
                {
                    Assert.AreEqual(previousRange.LastIP + 1, range.FirstIP);
                }
                previousRange = range;
            }
        }
    }
}
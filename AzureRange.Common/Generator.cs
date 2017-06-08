using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRange
{
    public class Generator
    {
        public static List<IPPrefix> Not(List<IPPrefix> PrefixList)
        {
            List<IPPrefix> complementPrefixList = new List<IPPrefix>();
            
            // Order the prefix list in numeric order
            IPPrefix previousPrefix = PrefixList.OrderBy(r => r.FirstIP).First();
            
            // For each prefix, find the gap... 
            foreach (IPPrefix currentPrefix in PrefixList.OrderBy(r => r.FirstIP))
            {
                var l_complementPrefix = ProcessGap(previousPrefix, currentPrefix);
                if (l_complementPrefix != null)
                    complementPrefixList.AddRange(l_complementPrefix);

                previousPrefix = currentPrefix;
            }
            return complementPrefixList.OrderBy(r => r.FirstIP).ToList();
        }
        public static List<IPPrefix> ProcessGap(IPPrefix p_Prefix_PreviousPrefix, IPPrefix p_CurrentPrefix)
        {
            var l_PrefixListGap = new List<IPPrefix>();

            // Locate 1st GAP to fill
            var l_PrefixGap = GetPrefixesBetween(p_Prefix_PreviousPrefix, p_CurrentPrefix);

            // If no prefix, null
            if (l_PrefixGap == null)
                return null;

            // Otherwise, add to list
            l_PrefixListGap.Add(l_PrefixGap);

            // Launch recursive loop to find solution 
            var innerRanges = ProcessGap(p_Prefix_PreviousPrefix, l_PrefixGap);
            if (innerRanges != null)
                l_PrefixListGap.AddRange(innerRanges);

            return l_PrefixListGap;
        }
        public static IPPrefix GetPrefixesBetween(IPPrefix p_Prefix_LowerBound, IPPrefix p_Prefix_UpperBound)
        {
            UInt32 l_int_lastIPInBetween = p_Prefix_UpperBound.FirstIP - 1;
            IPPrefix l_Prefix_lastNetwork = null;               // variable used to identify the subnet searched

            // validate for each mask (/32, /31, /30, etc.)
            for (short i = 32; i > 0; i--)
            {
                UInt32 l_long_maskBits = ((UInt32)Math.Pow(2, i) - 1) << (32 - i);
                var l_IPPrefix_TempNetwork = new IPPrefix(l_int_lastIPInBetween & l_long_maskBits, i);

                // Check if temporary prefix is good 
                if (!(
                    l_IPPrefix_TempNetwork.FirstIP > p_Prefix_LowerBound.LastIP &&
                    l_IPPrefix_TempNetwork.LastIP < p_Prefix_UpperBound.FirstIP
                    )
                    )
                {
                    if (l_Prefix_lastNetwork != null)
                        return l_Prefix_lastNetwork;
                    else
                        return null;
                }

                l_Prefix_lastNetwork = l_IPPrefix_TempNetwork;
            }

            return null;
        }
        public static List<IPPrefix> Summarize(List<IPPrefix> ipPrefixes)
        {
            List<IPPrefix> summarizedPrefixList = new List<IPPrefix>();

            for (var indexCount = 0; indexCount < ipPrefixes.Count(); indexCount++)
            {
                // Add current prefix to the list
                summarizedPrefixList.Add(ipPrefixes.ElementAt(indexCount));
                if (indexCount < (ipPrefixes.Count()-1))
                {
                    if( (ipPrefixes.ElementAt(indexCount).FirstIP ^ ipPrefixes.ElementAt(indexCount+1).FirstIP) == 0 )
                    {
                        summarizedPrefixList.Last().Mask = Math.Min(ipPrefixes.ElementAt(indexCount).Mask, ipPrefixes.ElementAt(indexCount + 1).Mask);
                        indexCount++;
                    }
                    if ((ipPrefixes.ElementAt(indexCount).FirstIP ^ ipPrefixes.ElementAt(indexCount + 1).FirstIP) 
                            == Math.Pow(2,32-ipPrefixes.ElementAt(indexCount).Mask) 
                            && (ipPrefixes.ElementAt(indexCount).Mask == ipPrefixes.ElementAt(indexCount+1).Mask))
                            // CAN BE SUMMARIZED!
                    {
                        // Modify last added element
                        summarizedPrefixList.Last().Mask -= 1;                        
                        indexCount++;
                    }
                }
            }
            if (ipPrefixes.Count() != summarizedPrefixList.Count())
                return Summarize(summarizedPrefixList);
            else
                return summarizedPrefixList;

        }
        public static IPPrefix GetContainingPrefix(IPPrefix p_Prefix,List<IPPrefix> ipPrefixList)
        {
            // Function used to look for which prefix in the list contains the prefix in parameter
            // input :  p_prefix : /32 prefix we're looking for
            //          ipPrefixList : list of prefixes for the lookup, ordered in numeric order
            // output : prefix containing p_prefix
            var resultPrefix = new IPPrefix();

            // Look in each prefix of the list
            foreach(var indexPrefix in ipPrefixList)
            {
                // see if our prefix is in between that list
                if ((indexPrefix.FirstIP <= p_Prefix.FirstIP) & (indexPrefix.LastIP >= p_Prefix.LastIP))
                {
                    resultPrefix = indexPrefix;
                    return resultPrefix;
                }
            }
            return null;
        }
        public static void Dedupe(List<IPPrefix> ipPrefixes)
        {
            var duplicates = new List<IPPrefix>();
            IPPrefix previousRange = null;
            foreach (var range in ipPrefixes.OrderBy(t => t.FirstIP))
            {
                bool isDup = false;
                if (previousRange != null)
                {
                    if (range.FirstIP >= previousRange.FirstIP
                        &&
                        range.LastIP <= previousRange.LastIP)
                    {
                        isDup = true;
                    }

                    if (range.FirstIP == previousRange.FirstIP
                        &&
                        range.LastIP > previousRange.LastIP)
                    {
                        duplicates.Add(previousRange);
                    }
                }

                if (isDup)
                    duplicates.Add(range);
                else
                    previousRange = range;
            }

            duplicates.ForEach(ipToRemove => ipPrefixes.Remove(ipToRemove));
        }
    }
}

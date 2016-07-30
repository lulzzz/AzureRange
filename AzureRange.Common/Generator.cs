using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRange
{
    public class Generator
    {
        public static List<IPPrefix> Not(List<IPPrefix> p_PrefixList)
        {
            var l_complementPrefixList = new List<IPPrefix>();
            // Order the prefix list in numeric order
            IPPrefix l_previousPrefix = p_PrefixList.OrderBy(r => r.FirstIP).First();
            // For each prefix, find the gap... 
            foreach (var l_prefix in p_PrefixList.OrderBy(r => r.FirstIP))
            {
                if (l_previousPrefix != null)
                {
                    var l_complementPrefix = ProcessGap(l_previousPrefix, l_prefix);
                    if (l_complementPrefix != null)
                        l_complementPrefixList.AddRange(l_complementPrefix);
                }
                l_previousPrefix = l_prefix;
            }
            return l_complementPrefixList.OrderBy(r => r.FirstIP).ToList();
        }
        public static List<IPPrefix> ProcessGap(IPPrefix p_Prefix_PreviousPrefix, IPPrefix p_Prefix)
        {
            var l_PrefixListGap = new List<IPPrefix>();

            // Trouver le premier gap valable pour l'écart à combler
            var l_PrefixGap = GetPrefixesBetween(p_Prefix_PreviousPrefix, p_Prefix);

            // Si aucun prefixe de trouve, on retourne null
            if (l_PrefixGap == null)
                // ne rien retourner
                return null;

            // sinon ajouter le prefixe a la solution
            l_PrefixListGap.Add(l_PrefixGap);

            // Chercher la solution entre le prefixe précédent et celui qui vient d'être trouvé
            var innerRanges = ProcessGap(p_Prefix_PreviousPrefix, l_PrefixGap);
            if (innerRanges != null)
                l_PrefixListGap.AddRange(innerRanges);

            return l_PrefixListGap;
        }

        public static IPPrefix GetPrefixesBetween(IPPrefix p_Prefix_LowerBound, IPPrefix p_Prefix_UpperBound)
        {
            var l_int_lastIPInBetween = p_Prefix_UpperBound.FirstIP - 1;
            IPPrefix l_Prefix_lastNetwork = null;               // variable used to identify the subnet searched

            // Validation pour chaque masque potentiel (/32, /31, /30, etc.)
            for (var i = 32; i > 0; i--)
            {
                var l_long_mask = (long)Math.Pow(2, i) - 1;
                var l_long_shiftedMask = l_long_mask << 32 - i;
                var l_IPPrefix_TempNetwork = new IPPrefix(l_int_lastIPInBetween & l_long_shiftedMask, i);

                // Valide si le prefixe temporaire trouve d
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
    }
}

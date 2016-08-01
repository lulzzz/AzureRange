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
            //variable declaration
            List<IPPrefix> l_complementPrefixList = new List<IPPrefix>();
            IPPrefix l_previousPrefix;
            
            // Order the prefix list in numeric order
            l_previousPrefix = p_PrefixList.OrderBy(r => r.FirstIP).First();
            
            // For each prefix, find the gap... 
            foreach (IPPrefix l_currentPrefix in p_PrefixList.OrderBy(r => r.FirstIP))
            {
                if (l_previousPrefix != null)
                {
                    var l_complementPrefix = ProcessGap(l_previousPrefix, l_currentPrefix);
                    if (l_complementPrefix != null)
                        l_complementPrefixList.AddRange(l_complementPrefix);
                }
                l_previousPrefix = l_currentPrefix;
            }
            return l_complementPrefixList.OrderBy(r => r.FirstIP).ToList();
        }
        public static List<IPPrefix> ProcessGap(IPPrefix p_Prefix_PreviousPrefix, IPPrefix p_CurrentPrefix)
        {
            var l_PrefixListGap = new List<IPPrefix>();

            // Trouver le premier gap valable pour l'écart à combler
            var l_PrefixGap = GetPrefixesBetween(p_Prefix_PreviousPrefix, p_CurrentPrefix);

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
            UInt32 l_int_lastIPInBetween = p_Prefix_UpperBound.FirstIP - 1;
            IPPrefix l_Prefix_lastNetwork = null;               // variable used to identify the subnet searched

            // Validation pour chaque masque potentiel (/32, /31, /30, etc.)
            for (short i = 32; i > 0; i--)
            {
                //var l_long_maskBits = (long)Math.Pow(2, i) - 1;
                UInt32 l_long_maskBits = ((UInt32)Math.Pow(2, i) - 1) << (32 - i);
                var l_IPPrefix_TempNetwork = new IPPrefix(l_int_lastIPInBetween & l_long_maskBits, i);
                //var int toto = sizeof(int);

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

using System;
using System.Collections.Generic;

namespace BusterWood.Monies
{
    public static class Currencies
    {
        static readonly Dictionary<string, int?> decimalPlaces = new Dictionary<string, int?>
        {
            { "BHD", 3 },
            { "BIF", 0 },
            { "BYR", 0 },
            { "CLP", 0 },
            { "CVE", 0 },
            { "DJF", 0 },
            { "GNF", 0 },
            { "ISK", 0 },
            { "JOD", 3 },
            { "JPY", 0 },
            { "KMF", 0 },
            { "KRW", 0 },
            { "KWD", 3 },
            { "LYD", 3 },
            { "MGA", 1 },
            { "MRO", 1 },
            { "OMR", 3 },
            { "PYG", 0 },
            { "RWF", 0 },
            { "TND", 3 },
            { "UGX", 0 },
            { "UYI", 0 },
            { "VND", 0 },
            { "VUV", 0 },
            { "XAF", 0 },
            { "XAG", default(int?) },
            { "XAU", default(int?) },
            { "XBA", default(int?) },
            { "XBB", default(int?) },
            { "XBC", default(int?) },
            { "XBD", default(int?) },
            { "XDR", default(int?) },
            { "XOF", 0 },
            { "XPD", default(int?) },
            { "XPF", 0 },
            { "XPT", default(int?) },
            { "XSU", default(int?) },
            { "XTS", default(int?) },
            { "XUA", default(int?) },
            { "XXX", default(int?) },
            { "BTC", 8 },
            { "XBT", 8 },
            { "XMR", 8 },
            { "XRP", 8 },
            { "XZP", 8 },
            { "ZEC", 8 },
        };

        /// <summary>
        /// Returns the number of decimal places used for differnt currency codes
        /// </summary>
        /// <param name="isoCode"></param>
        /// <returns></returns>
        public static int? DecimalPlaces(string isoCode)
        {
            int? dps;
            if (decimalPlaces.TryGetValue(isoCode, out dps))
                return dps;
            return 2; // the default, most currencies
        }
    }
}
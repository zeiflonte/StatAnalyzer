using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Stats.Controls
{
    static class CriticalValueParser
    {
        public static double[] criticalValuesTable = new double[30];

        public static void ParseCriticalValueTable()
        {
            string fileName = "../../critVal";

            using (StreamReader reader = new StreamReader(fileName))
            {
                string data = reader.ReadToEnd();
                {
                    string[] entries = data.Split('\n');
                    int i = 0;
                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    ci.NumberFormat.CurrencyDecimalSeparator = ".";
                    foreach (string entry in entries)
                    {
                        criticalValuesTable[i] = double.Parse(entry, NumberStyles.Any, ci);
                        i++;
                    }
                }
            }
        }
    }
}

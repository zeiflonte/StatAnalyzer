using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Stats.Controls
{
    class StatisticValue
    {
        private float value;
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }
        private float frequency;
        public float Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                frequency = value;
            }
        }

        public StatisticValue(float val, float freq)
        {
            this.value = val;
            this.frequency = freq;
        }


    }

    class DiscreteAnalyser
    {
        public DiscreteAnalyser()
        {

        }

        private float calculateDx(float Mx, List<float> data)
        {
            float sum = 0;
            foreach (float value in data)
            {
                sum += (float)Math.Pow(value - Mx, 2);
            }

            return sum / (float)data.Count;
        }

        private int entryCount(float value, float[] array)
        {
            int res = 0;
            foreach (float elem in array)
            {
                if (elem == value)
                    res++;
            }

            return res;
        }

        public void outputTable(string name, StatisticValue[] table)
        {
            using (StreamWriter sw = new StreamWriter(name, false, System.Text.Encoding.Default))
            {
                //sw.WriteLine("<table>");
                foreach (StatisticValue value in table)
                {
                    sw.WriteLine("Значение: {0}   Частота: {1}", value.Value, value.Frequency);
                }
                //sw.WriteLine("</table>");
            }
        }

        //bool PyassonCheck()

        public void Analyse(List<float> data)
        {
            float Mx_real = data.Sum() / data.Count;
            float Dx_real = calculateDx(Mx_real, data);

            float[] unique = data.Distinct().ToArray();
            Array.Sort(unique);

            StatisticValue[] table = new StatisticValue[unique.Length];
            int index = 0;
            foreach(float element in unique)
            {
                float elFreq = entryCount(element, data.ToArray()) / (float)data.Count;
                table[index] = new StatisticValue(element, elFreq);
                index++;
            }

            outputTable("output.txt", table);
        }
    }
}

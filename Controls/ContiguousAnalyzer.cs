using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Stats.Controls
{
    class СontiguousAnalyzer
    {
        private double h;
        double H
        {
            get
            {
                return h;
            }
            set
            {
                h = value;
            }
        }
        private double mx;
        double Mx
        {
            get
            {
                return mx;
            }
            set
            {
                mx = value;
            }
        }
        private double dx;
        double Dx
        {
            get
            {
                return dx;
            }
            set
            {
                dx = value;
            }
        }
        private double sd;
        double Sd
        {
            get
            {
                return sd;
            }
            set
            {
                sd = value;
            }
        }
        private double sumFrequencies;
        double SumFrequencies
        {
            get
            {
                return sumFrequencies;
            }
            set
            {
                sumFrequencies = value;
            }
        }

        struct intervalStruct
        {
            public double startPoint;
            public double endPoint;
            public double sumFrequencies;
            public double middle;
        }
        private List<intervalStruct> intervals = new List<intervalStruct>();

        public СontiguousAnalyzer(List<float> data)
        {
            H = calculateH(data);
            getIntervals(data);
            Mx = calculateMx();
            Dx = calculateDx();

            Analyse();
        }

        double calculateH(List<float> data)
        {
            double maxValue = data.Max();
            double minValue = data.Min();
            double H = (maxValue - minValue) / (1 + 3.322 * Math.Log(data.Count));
            return H;
        }

        void getIntervals(List<float> data)
        {
            double start;
            double end;
            double delta = 0.00001;

            // Set up end value for the first iteration 
            end = data.Min();
            do
            {
                start = end;
                end = start + H;

                intervalStruct interval = new intervalStruct();
                interval.startPoint = start;
                interval.endPoint = end;
                interval.middle = (start + end) / 2;
                interval.sumFrequencies = data
                    .Where(x => (x > start - delta || x > start + delta) &&
                          (x < end - delta || x < end + delta)).Sum();

                intervals.Add(interval);
            } while (end < data.Max());
        }

        double calculateMx()
        {
            double Mx = 0;
            double sumFrequencies = 0;
            foreach (intervalStruct interval in intervals)
            {
                Mx += interval.sumFrequencies * interval.middle;
                sumFrequencies += interval.sumFrequencies;
            }
            Mx /= sumFrequencies;
            this.SumFrequencies = sumFrequencies;
            return Mx;
        }

        double calculateDx()
        {
            double Dx = 0;
            foreach (intervalStruct interval in intervals)
            {
                Dx = Dx + Math.Pow(interval.middle - Mx, 2) * interval.sumFrequencies;
            }
            Dx /= SumFrequencies;
            return Dx;
        }

        double calculateSd()
        {
            return Math.Sqrt(Dx);
        }

        public void Analyse()
        {
            outputTable("output.txt");
        }

        public void outputTable(string name)
        {
            int i = 1;
            using (StreamWriter sw = new StreamWriter(name, false, System.Text.Encoding.Default))
            {
                //sw.WriteLine("<table>");
                foreach (intervalStruct interval in intervals)
                {
                    sw.WriteLine("{0} {1}-{2} {3} {4}", i, interval.startPoint, interval.endPoint, interval.sumFrequencies, interval.middle);
                    i++;
                }
                //sw.WriteLine("</table>");
            }
        }
    }
}

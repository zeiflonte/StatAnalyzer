using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Stats.Controls
{
    class intervalStruct
    {
        public double startPoint;
        public double endPoint;
        public int sumFrequencies;
        public double theorFrequencies;
        public double middle;
        public double x;
        public double f;
    }
    class СontiguousAnalyzer
    {
        static double[] fiTable = new double[401];
        Canvas canvas;
        private double xAverage;
        double Average
        {
            get
            {
                return xAverage;
            }
            set
            {
                xAverage = value;
            }
        }
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

        private List<intervalStruct> intervals = new List<intervalStruct>();

        public СontiguousAnalyzer(List<float> data, Canvas canvas)
        {
            H = calculateH(data);
            getIntervals(data);
            Mx = calculateMx();
            Dx = calculateDx();
            Sd = calculateSd();

            this.canvas = canvas;
            Analyse(data);
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

            double amountOfVariants = data.Count;

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
                    .Count(x => (x > start - delta || x > start + delta) &&
                          (x < end - delta || x < end + delta));
                interval.x = interval.middle;
                interval.f = interval.sumFrequencies / (amountOfVariants * H);

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

        private void draw()
        {
           // int offsetX = (int)canvas.ActualHeight;
            int offsetY = (int)canvas.ActualHeight * 2 / 3;
            int offset = 24;
            //int X1 = 0;
            //int Y1 = (int)canvas.ActualHeight;
            int X2 = 0;
            int Y2 = offsetY;
            foreach (intervalStruct interval in intervals)
            {
                Line line = new Line();
                line.X1 = X2;
                line.Y1 = Y2;

                X2 += offset;
                Y2 = offsetY - interval.sumFrequencies * 2; //!!!!!!
                
                line.X2 = X2;
                line.Y2 = Y2;

                line.VerticalAlignment = VerticalAlignment.Top;
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Green;
                canvas.Children.Add(line);
            }

        }

        public void Analyse(List<float> data)
        {
            draw();

            outputTable("output.txt");

            DispersionMultiplication();

            // Normal distribution check

            if (NormalDistributionCheck(data))
            {
                MessageBox.Show("Normal distribution");
            }

            CountFrequencies(data);
            int d = CountDegreesOfFreedom();
            string message;
            double X2obs = CountX2obs();
            message = ComparePirson(X2obs, CountX2da(d)) + "\n";
            message += CompareRomanovsky(X2obs, d) + "\n";
            message += CompareYastremsky(X2obs, d);
            MessageBox.Show(message, "Normal distribution");
        }

        void DispersionMultiplication()
        {
            double DxCommon = 0;
            foreach (intervalStruct interval in intervals)
            {
                DxCommon += Math.Pow(interval.x - xAverage, 2);
            }
            DxCommon /= intervals.Count;

            double GroupDispersionAverage;
            double numenator = 0;
            double denominator = 0;
            foreach (intervalStruct interval in intervals)
            {
                numenator += Sd * interval.f;
                denominator += interval.f;
            }
            GroupDispersionAverage = numenator / denominator;

            double InterGroupDispersion;
            numenator = 0;
            denominator = 0;
            foreach (intervalStruct interval in intervals)
            {
                numenator += Math.Pow(interval.x - xAverage, 2) * interval.f;
                denominator += interval.f;
            }
            InterGroupDispersion = numenator / denominator;

            if (CompareDispersionMultiplication(DxCommon, GroupDispersionAverage, InterGroupDispersion))
            {
                MessageBox.Show("Dispersions multiplication theorem: HIGH quality");
            }
            else
            {
                MessageBox.Show("Dispersions multiplication theorem: LOW quality");
            }

            //MessageBox.Show(DxCommon.ToString() + " = " + GroupDispersionAverage.ToString() + " + " + InterGroupDispersion.ToString());
        }

        bool CompareDispersionMultiplication(double DxCommon, double GroupDispersionAverage, double InterGroupDispersion)
        {
            if (GroupDispersionAverage + InterGroupDispersion > 0.9 * DxCommon &&
                GroupDispersionAverage + InterGroupDispersion < 1.1 * DxCommon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool NormalDistributionCheck(List<float> data)
        {
            double As;
            double Ex;

            // Count xAverage
            double numerator = 0; // числитель в xAverage
            double denominator = 0;
            foreach (intervalStruct interval in intervals)
            {
                numerator += interval.x * interval.f;
                denominator += interval.f;
            }
            xAverage = numerator / denominator;

            // Count nu3
            double nu3;
            int m = 3;
            numerator = 0;
            denominator = 0;
            foreach (intervalStruct interval in intervals)
            {
                numerator += Math.Pow(interval.x - xAverage, m) * interval.f;
                denominator += interval.f;
            }
            nu3 = numerator / denominator;

            // Count As
            double sigma3 = Dx * Sd;
            As = nu3 / sigma3;

            // Count nu4
            double nu4;
            m = 4;
            numerator = 0;
            denominator = 0;
            foreach (intervalStruct interval in intervals)
            {
                numerator += Math.Pow(interval.x - xAverage, m) * interval.f;
                denominator += interval.f;
            }
            nu4 = numerator / denominator;

            // Count Ex
            double sigma4 = Dx * Dx;
            Ex = nu4 / sigma4 - 3;

            // Compare with null
            return CompareWithNull(As) && CompareWithNull(Ex);
        }

        bool CompareWithNull(double parameter)
        {
            return (parameter < 0.5 && parameter > -0.5);
        }

        void CountFrequencies(List<float> data)
        {
            //string output = "";
            foreach (intervalStruct interval in intervals)
            {
                double t = (interval.x - xAverage) / Sd;
                //output += t.ToString() + "\n";
                int index = (int)Math.Abs(Math.Round(t * 100));
                double fi = fiTable[index];
                interval.theorFrequencies = data.Count * H * fi / Sd;
            }
            //MessageBox.Show(output);
        }

        int CountDegreesOfFreedom()
        {
            int r = 2; // amount of parameters for normal distribution
            int d = intervals.Count - r - 1;
            return d;
        }

        double CountX2obs()
        {
            double sum = 0;
            foreach (intervalStruct interval in intervals)
            {
                sum += Math.Pow(interval.sumFrequencies - interval.theorFrequencies, 2) / interval.theorFrequencies;
            }
            return sum;
        }

        double CountX2da(int d)
        {
            // alpha = 0.05;
            double criticalValue = CriticalValueParser.criticalValuesTable[d - 1];
            return criticalValue;
        }

        string ComparePirson(double X2obs, double X2da)
        {
            string message;
            if (X2obs > X2da)
            {
                message = "Pirson: Hypothesis has been denied";
            }
            else
            {
                message = "Pirson: Hypothesis has been accepted";
            }
            return message;
        }

        string CompareRomanovsky(double X2obs, int d)
        {
            string message;
            double R = Math.Abs(X2obs - d) / Math.Sqrt(2 * d);
            if (R >= 3)
            {
                message = "Romanovsky: Hypothesis has been denied";
            }
            else
            {
                message = "Romanovsky: Hypothesis has been accepted";
            }
            return message;
        }

        string CompareYastremsky(double X2obs, int d)
        {
            string message;
            double J = Math.Abs(X2obs - d) / Math.Sqrt(2 * intervals.Count + 2.4);
            if (J >= 3)
            {
                message = "Yastremsky: Hypothesis has been denied";
            }
            else
            {
                message = "Yastremsky: Hypothesis has been accepted";
            }
            return message;
        }

        public static void ParseFiTable()
        {
            string fileName = "../../fi";

            using (StreamReader reader = new StreamReader(fileName))
            {
                string data = reader.ReadToEnd();
                {
                    string[] cells = data.Split('\t');
                    int i = 0;
                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    ci.NumberFormat.CurrencyDecimalSeparator = ".";
                    foreach (string entry in cells)
                    {
                        string convertedString = "0." + entry;
                        double convertedFloat = double.Parse(convertedString, NumberStyles.Any, ci);
                        fiTable[i] = convertedFloat;
                        i++;
                    }
                }
            }
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

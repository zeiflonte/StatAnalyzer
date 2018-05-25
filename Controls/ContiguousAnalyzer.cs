using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Stats.Controls
{
    class СontiguousAnalyzer
    {
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

        struct intervalStruct
        {
            public double startPoint;
            public double endPoint;
            public int sumFrequencies;
            public double middle;
            public double x;
            public double f;
        }
        private List<intervalStruct> intervals = new List<intervalStruct>();

        public СontiguousAnalyzer(List<float> data, Canvas canvas)
        {
            H = calculateH(data);
            getIntervals(data);
            Mx = calculateMx();
            Dx = calculateDx();

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

            if (NormalSpreadingCheck(data))
            {
                MessageBox.Show("Normal spreading");
            }
        }

        public bool NormalSpreadingCheck(List<float> data)
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

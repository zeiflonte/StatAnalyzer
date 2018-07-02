using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Stats;

namespace Stats.Controls
{
    class intervalDiscrete
    {
        public double startPoint;
        public double endPoint;
        public double sumFrequencies;
        public int countFrequencies;
        public int frequency;
        public double theorFrequencies;
        public double middle;
        public double x;
        public double f;
    }

    class StatisticValue
    {
        private double value;
        private double theorFrequencies;
        public double TheorFrequencies
        {
            get
            {
                return theorFrequencies;
            }
            set
            {
                theorFrequencies = value;
            }
        }
        public double Value
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
        private double frequency;
        public double Frequency
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

        public StatisticValue(double val, double freq)
        {
            this.value = val;
            this.frequency = freq;
        }


    }

    class DiscreteAnalyser
    {
        private List<intervalDiscrete> intervals = new List<intervalDiscrete>();
        double H;
        Canvas field;
        Canvas funcCanvas;
        double AverageX;
        double Average
        {
            get
            {
                return AverageX;
            }
            set
            {
                AverageX = value;
            }
        }


        public DiscreteAnalyser(Canvas c, Canvas funcCanvas)
        {
            field = c;
            this.funcCanvas = funcCanvas;
        }


        private void draw(Canvas canvas, StatisticValue[] table)
        {
            int startPoint = (int)canvas.ActualHeight / 2;
            int startY = (int)canvas.ActualHeight;
            int offset = 0;
            foreach (StatisticValue value in table)
            {
                Ellipse el = new Ellipse();
                el.Width = 5;
                el.Height = 5;
                el.VerticalAlignment = VerticalAlignment.Top;
                el.StrokeThickness = 3;
                el.Stroke = System.Windows.Media.Brushes.Green;
                canvas.Children.Add(el);
                Canvas.SetLeft(el, value.Value + offset);
                Canvas.SetTop(el,(startPoint - value.Frequency*100));
                offset += 5;
            }
        }

        private void drawFunction(Canvas canvas, StatisticValue[] table)
        {
            int offsetY = (int)canvas.ActualHeight * 2 / 3;
            int offset = 6;
            int X2 = 0;
            int Y2 = offsetY;
            foreach (StatisticValue value in table)
            {
                Line line = new Line();
                line.X1 = X2;
                line.Y1 = Y2;

                X2 += offset;
                Y2 = offsetY - (int)(value.Value) * 2;

                line.X2 = X2;
                line.Y2 = Y2;

                line.VerticalAlignment = VerticalAlignment.Top;
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Green;
                canvas.Children.Add(line);
            }
        }

        double CalculateAverage(List<float> table)
        {
            float count = table.Count();
            double sum = 0;

            foreach (float value in table)
            {
                sum += value;
            }

            return sum / count;
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

        
        /*bool BinomialSpreadingCheck(int n)
        {
            double q = Dx / Mx;
            double p = (1 - q);
        }*/


        public void Analyse(List<float> data, TextBox results)
        {
            float Mx_real = data.Sum() / data.Count;
            float Dx_real = calculateDx(Mx_real, data);
            Average = CalculateAverage(data);

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

            draw(field, table);
            drawFunction(funcCanvas, table);

            string message = "";

            if (PyassonCheck(table, data, ref message))
            {
                MessageBox.Show("It is the puasson distribution");
            }
            else
            {
                MessageBox.Show("Primary checking on the puasson distribution: false");
            }

            outputTable("../../discrete.txt", table);
            
            H = getH(data);
            getIntervals(data, table);
           // CountTheorFrequencies();

            int d = CountDegreesOfFreedom();
            double X2obs = CountX2obs();
            message += "Mx = " + Mx_real + "; " + "Dx = " + Dx_real + "; " + "Sd = " + Math.Sqrt(Dx_real) + "\n";
            message += ComparePirson(X2obs, CountX2da(d)) + "\n";
            message += CompareRomanovsky(X2obs, d) + "\n";
            message += CompareYastremsky(X2obs, d, table);

            results.Text = message;
        }

        public int factorial(int number)
        {
            if (number == 0)
            {
                number = 1;
            }
            int result = 1;
            while (number != 1)
            {
                result = result * number;
                number = number - 1;
            }
            return result;
        }

        double getH(List<float> data)
        {
            double maxValue = data.Max();
            double minValue = data.Min();
            double H = (maxValue - minValue) / (1 + 3.322 * Math.Log(data.Count));
            H = 17;
            return H;
        }

        void getIntervals(List<float> data, StatisticValue[] table)
        {
            double start;
            double end;
            // double delta = 0.00001;

            double amountOfVariants = data.Count;
            string mes = "";
            // Set up end value for the first iteration 
            end = (int)Math.Round(data.Min());

            do
            {
                start = end;
                end = start + (int)Math.Round(H);

                intervalDiscrete interval = new intervalDiscrete();
                interval.startPoint = start;
                interval.endPoint = end;
                interval.middle = (start + end) / 2;

                StatisticValue[] arr = Array.FindAll(table, x => (x.Value >= start) && (x.Value < end));
                interval.sumFrequencies = arr.Sum(x => x.Frequency);
                interval.frequency = arr.Count();
                interval.theorFrequencies = arr.Count() * Math.Pow(AverageX, interval.x) * Math.Pow(Math.E, -AverageX) /factorial((int)Math.Round(interval.x));
                /* interval.countFrequencies = data
                    .Count(x => (x > start - delta || x > start + delta) &&
                          (x < end - delta || x < end + delta)); */

                interval.x = interval.middle;
                interval.f = interval.sumFrequencies / (amountOfVariants * H);
                mes += /*interval.startPoint + " - " + interval.endPoint + " " +*/ interval.frequency + " " + interval.sumFrequencies + "\n";
                intervals.Add(interval);
            } while (end < data.Max());
            //MessageBox.Show(mes);
        }     

        void CountTheorFrequencies()
        {
            string output = "";
            foreach (intervalDiscrete interval in intervals)
            {
                //interval.theorFrequencies = intervals.Count() * (Math.Pow(AverageX, interval.x) / (factorial((int)(Math.Round(interval.x))) * Math.Pow(Math.E, -AverageX)));
                interval.theorFrequencies = Math.Pow(interval.sumFrequencies, 2);
                output += interval.theorFrequencies.ToString() + "\n";
            }
            //MessageBox.Show(output);
        }

        int CountDegreesOfFreedom()
        {
            int r = 1; // amount of parameters for puasson distribution
            int d = intervals.Count() - r - 1;
            return d;
        }

        double CountX2obs()
        {
            double sum = 0;
            foreach (intervalDiscrete interval in intervals)
            {
                if (interval.theorFrequencies != 0)
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
            message += "\nX2obs = " + X2obs + "; " + "X2da = " + X2da;
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
            message += "\nR = " + R;
            return message;
        }

        string CompareYastremsky(double X2obs, int d, StatisticValue[] table)
        {
            string message;
            double J = Math.Abs(X2obs - d) / Math.Sqrt(2 * table.Count() + 2.4);
            if (J >= 3)
            {
                message = "Yastremsky: Hypothesis has been denied";
            }
            else
            {
                message = "Yastremsky: Hypothesis has been accepted";
            }
            message += "\nJ = " + J;
            return message;
        }

        bool PyassonCheck(StatisticValue[] table, List<float> data, ref string message)
        {
            double delta = 0.1;
            double Sv = SampleVariance(table, data.Count);
            double Xv = calculateXv(ref table, data.Count);
            message += "Sv = " + Sv + "; Xv = " + Xv + "\n";
            return (Sv * (1 - delta) < Xv && Xv < Sv * (1 + delta));
        }  

        double calculateXv(ref StatisticValue[] table, int n)
        {
            double numerator = 0;
            foreach (StatisticValue value in table)
            {
                numerator += value.Value * value.Frequency;
            }

            return numerator / (float)n;
        }

        double SampleVariance(StatisticValue[] table ,int n)
        {
            double numerator = 0;
            foreach (StatisticValue value in table)
            {
                numerator += Math.Pow(value.Value - Average, 2);
            }

            return numerator / n;
        }
    }
}

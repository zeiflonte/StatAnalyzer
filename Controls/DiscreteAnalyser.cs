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
        Canvas field;
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


        public DiscreteAnalyser(Canvas c)
        {
            field = c;
        }


        private void draw(Canvas canvas, StatisticValue[] table)
        {
            int startPoint = (int)canvas.ActualHeight / 2;
            int startY = (int)canvas.ActualHeight;
            int offset = 0;
            foreach (StatisticValue value in table)
            {
                Ellipse el = new Ellipse();
                el.Width = 10;
                el.Height = 10;
                el.VerticalAlignment = VerticalAlignment.Top;
                el.StrokeThickness = 3;
                el.Stroke = System.Windows.Media.Brushes.Green;
                canvas.Children.Add(el);
                Canvas.SetLeft(el, value.Value + offset);
                Canvas.SetTop(el,(startPoint - value.Frequency*100));
                offset += 20;
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


        public void Analyse(List<float> data)
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

            if (PyassonCheck(table, data))
            {
                MessageBox.Show("Puasson spreading");
            }

            outputTable("output.txt", table);
        }

       // void Find

        bool PyassonCheck(StatisticValue[] table, List<float> data)
        {
            double delta = 0.1;
            double Sv = SampleVariance(table, data.Count);
            double Xv = calculateXv(ref table, data.Count);

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
        /*double AverageArifmethikValue()
        {
          double numerator = 0;

          foreach (intervalStruct interval in intervals)
          {
              numerator += 
          }
        }*/

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

using Microsoft.Win32;
using System.IO;
using System.Windows;
using Stats.Controls;
using System.Collections.Generic;
using System.Globalization;

namespace Stats
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool[] isDiscret = new bool[3];

        public MainWindow()
        {
            InitializeComponent();
        }

        //float[,] values = new float[360,3];
        List<float> value1 = new List<float>();
        List<float> value2 = new List<float>();
        List<float> value3 = new List<float>();

        private void butLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileOpen = new OpenFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All Files (*.*)|*.*",
                RestoreDirectory = true,
            };
            if (fileOpen.ShowDialog() == true)
            {
                string fileName = fileOpen.FileName;

                using (StreamReader stream = new StreamReader(fileName))
                {
                    string data = stream.ReadToEnd();
                    {
                        string[] dataArray = data.Split('\n');
                        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                        ci.NumberFormat.CurrencyDecimalSeparator = ".";
                        foreach (string entry in dataArray)
                        {
                            string[] entryArray = entry.Split(' ');
                            value1.Add(float.Parse(entryArray[0], NumberStyles.Any, ci));
                            value2.Add(float.Parse(entryArray[1], NumberStyles.Any, ci));
                            value3.Add(float.Parse(entryArray[2], NumberStyles.Any, ci));
                        }
                    }
                }
                fileOpen = null;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            DataTypeIdentifyer identifyer = new DataTypeIdentifyer(value1);
            identifyer.Criteria = 0.5f;
            identifyer.Identify();

            identifyer = new DataTypeIdentifyer(value3);
            if (identifyer.Identify())
            {
                DiscreteAnalyser analyser = new DiscreteAnalyser();
                analyser.Analyse(value3);
            }

            СontiguousAnalyzer contiguousAnalyzer = new СontiguousAnalyzer(value1);
        }
    }
}

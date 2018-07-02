using Microsoft.Win32;
using System.IO;
using System.Windows;
using Stats.Controls;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

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

        private void Initialize(List<float> value, Canvas canvas, Canvas funcCanvas, TextBox results, string distribution)
        {
            DataTypeIdentifyer identifyer = new DataTypeIdentifyer(value);
            identifyer.Criteria = 0.5f;

            CriticalValueParser.ParseCriticalValueTable();
            if (identifyer.Identify())
            {
                // Discrete
                DiscreteAnalyser analyser = new DiscreteAnalyser(canvas, funcCanvas);
                analyser.Analyse(value, results);
            }
            else
            {
                СontiguousAnalyzer.ParseFiTable();
                СontiguousAnalyzer contiguousAnalyzer = new СontiguousAnalyzer(value, canvas, funcCanvas, results, distribution);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Initialize(value1, canvasContig, funcCanvasContig, results1, "normal");
            Initialize(value1, canvasContig, funcCanvasContig, results12, "uniform");
            Initialize(value2, canvasContig2, funcCanvasContig2, results2, "normal");
            Initialize(value2, canvasContig2, funcCanvasContig2, results22, "uniform");
            Initialize(value3, canvas, funcCanvas, results3, "");
        }

        private void btnClearData_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileOpen = new OpenFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All Files (*.*)|*.*",
                RestoreDirectory = true,
            };
            if (fileOpen.ShowDialog() == true)
            {
                string fileName = fileOpen.FileName;
                string[] parcedArray;

                // Reading and processing raw data
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string data = reader.ReadToEnd();
                    {
                        string[] dataArray = data.Split('\n');
                        parcedArray = new string[dataArray.Length / 5];
                        int i = 1, // skip 1st entry
                            j = 0;
                        while (i < dataArray.Length)
                        {
                            // 2nd entry - add min temperature
                            parcedArray[j] = dataArray[i];

                            i += 2; // 3nd entry - just skip

                            // 4nd entry - add max temperature
                            parcedArray[j] += ' ' + dataArray[i];
                            i++;

                            //5th entry - add rainfall
                            parcedArray[j] += ' ' + dataArray[i];

                            i += 2; // 1nd entry - just skip

                            // Next parcedArray entry
                            j++;
                        }
                    }
                }

                // Writing raw data
                SaveFileDialog fileSave = new SaveFileDialog
                {
                    Filter = "Text file (*.txt)|*.txt|All Files (*.*)|*.*",
                    RestoreDirectory = true,
                };
                if (fileSave.ShowDialog() == true)
                {
                    fileName = fileSave.FileName;
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        int i = 0;
                        while (i < parcedArray.Length - 1)
                        {
                            writer.WriteLine(parcedArray[i]);
                            i++;
                        }
                        // Avoiding a new line in the end of file
                        writer.Write(parcedArray[i]);             
                    }
                    fileSave = null;
                }
                fileOpen = null;
            }
        }
    }
}

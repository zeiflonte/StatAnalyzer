using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Stats
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        float[,] values = new float[360,3];

        private void butLoad_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
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
                        foreach (string entry in dataArray)
                        {
                            string[] entryArray = entry.Split(' ');
                            float.TryParse(entryArray[0], out values[i,0]);
                            float.TryParse(entryArray[1], out values[i,1]);
                            float.TryParse(entryArray[2], out values[i,2]);
                            i++;
                        }
                    }
                }
                fileOpen = null;
            }
        }
    }
}

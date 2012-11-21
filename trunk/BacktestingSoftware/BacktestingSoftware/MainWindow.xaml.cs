using System;
using System.ComponentModel;
using System.Windows;

namespace BacktestingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _algorithmFileName;

        public string AlgorithmFileName
        {
            get { return _algorithmFileName; }
            set
            {
                if (value != _algorithmFileName)
                {
                    _algorithmFileName = value;
                    OnPropertyChanged("AlgorithmFileName");
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AlgorithmButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "Algorithm.dll"; // Default file name
            dlg.DefaultExt = ".dll"; // Default file extension
            dlg.Filter = "Algorithm File (.dll)|*.dll"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            this.AlgorithmFileName = dlg.FileName;

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.AlgorithmFileName = dlg.FileName;
            }
        }
    }
}
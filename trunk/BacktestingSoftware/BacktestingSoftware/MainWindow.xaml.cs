using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace BacktestingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<DateTime, decimal> value;

        public MainWindow()
        {
            InitializeComponent();
            this.mainViewModel.StartDate = new DateTime(1970, 1, 1);
            this.mainViewModel.EndDate = DateTime.Now;
            this.mainViewModel.Orders = new List<Order>();
            this.mainViewModel.Signals = new List<int>();

            this.orders.DataContext = this.mainViewModel.Orders;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
        }

        private void AlgorithmButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "Algorithm.dll"; // Default file name
            dlg.DefaultExt = ".dll"; // Default file extension
            dlg.Filter = "Algorithm File (.dll)|*.dll"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.mainViewModel.AlgorithmFileName = dlg.FileName;
            }
        }

        private void DataButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "Data.csv"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "Data File (.csv)|*.csv"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.mainViewModel.DataFileName = dlg.FileName;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();

            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                // report the progress
                b.ReportProgress(0, "Starting Calculation...");

                Calculator c = new Calculator(this.mainViewModel);

                // report the progress
                b.ReportProgress(5, "Reading File...");

                c.ReadFile();

                // report the progress
                b.ReportProgress(40, "Calculating Signals...");

                c.CalculateSignals();

                // report the progress
                b.ReportProgress(70, "Calculating Performance...");

                c.CalculateNumbers();
            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                this.StatusLabel.Text = String.Empty + args.UserState.ToString();
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                this.StatusLabel.Text = "Drawing Chart...";
                this.LoadLineChartData();
                this.StatusLabel.Text = "Finished!";
            });

            bw.RunWorkerAsync();
        }

        private void LoadLineChartData()
        {
            value = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < this.mainViewModel.BarList.Count; i++)
                value.Add(this.mainViewModel.BarList[i].Item1, this.mainViewModel.BarList[i].Item5);

            Chart chart = this.FindName("MyWinformChart") as Chart;
            chart.DataSource = value;
            chart.Series["series"].XValueMember = "Key";
            chart.Series["series"].YValueMembers = "Value";
            chart.DataBind();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
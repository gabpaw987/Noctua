using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BacktestingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bw;

        public MainWindow()
        {
            InitializeComponent();
            this.mainViewModel.Orders = new List<Order>();
            this.mainViewModel.Signals = new List<int>();
            this.mainViewModel.BarList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();

            this.mainViewModel.SaveFileName = String.Empty;
            this.mainViewModel.LoadFileName = String.Empty;

            this.mainViewModel.AlgorithmFileName = Properties.Settings.Default.AlgorithmFileName;
            this.mainViewModel.DataFileName = Properties.Settings.Default.DataFileName;

            if (Properties.Settings.Default.StartDate != new DateTime() && Properties.Settings.Default.EndDate != new DateTime())
            {
                this.mainViewModel.StartDate = Properties.Settings.Default.StartDate;
                this.mainViewModel.EndDate = Properties.Settings.Default.EndDate;
            }
            else
            {
                this.mainViewModel.StartDate = new DateTime(1970, 1, 1);
                this.mainViewModel.EndDate = DateTime.Now;
            }

            this.mainViewModel.ValueOfSliderOne = Properties.Settings.Default.ValueOfSliderOne;
            this.mainViewModel.ValueOfSliderTwo = Properties.Settings.Default.ValueOfSliderTwo;
            this.mainViewModel.ValueOfSliderThree = Properties.Settings.Default.ValueOfSliderThree;
            this.mainViewModel.ValueOfSliderFour = Properties.Settings.Default.ValueOfSliderFour;
            this.mainViewModel.ValueOfSliderFive = Properties.Settings.Default.ValueOfSliderFive;
            this.mainViewModel.ValueOfSliderSix = Properties.Settings.Default.ValueOfSliderSix;

            this.mainViewModel.RoundLotSize = Properties.Settings.Default.RoundLotSize;
            this.mainViewModel.Capital = Properties.Settings.Default.Capital;

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
            this.resetCalculation();

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;

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
                this.ProgressBar.Value = args.ProgressPercentage;
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                this.StatusLabel.Text = "Drawing Chart...";
                this.LoadLineChartData();
                this.StatusLabel.Text = "Finished!";
                this.ProgressBar.Value = 100;
                this.orders.Items.Refresh();
            });

            bw.RunWorkerAsync();
        }

        private void LoadLineChartData()
        {
            Chart chart = this.FindName("MyWinformChart") as Chart;

            Series series = new Series("Data"); // <<== make sure to name the series "price"
            chart.Series.Add(series);

            // Set series chart type
            chart.Series["Data"].ChartType = SeriesChartType.Candlestick;

            //chart.DataSource = this.mainViewModel.BarList;
            chart.Series["Data"].XValueMember = "DateStamp";
            chart.Series["Data"].XValueType = ChartValueType.DateTime;
            chart.Series["Data"].YValueMembers = "HighPrice, LowPrice, OpenPrice, ClosePrice";

            // Set the style of the open-close marks
            chart.Series["Data"]["OpenCloseStyle"] = "Triangle";

            // Show both open and close marks
            chart.Series["Data"]["ShowOpenClose"] = "Both";

            // Set point width
            chart.Series["Data"]["PointWidth"] = "0.5";

            // Set colors bars
            chart.Series["Data"]["PriceUpColor"] = "Green"; // <<== use text indexer for series
            chart.Series["Data"]["PriceDownColor"] = "Red"; // <<== use text indexer for series
            //chart.Series["Data"].BorderColor = System.Drawing.Color.Black;
            chart.Series["Data"].Color = System.Drawing.Color.Black;
            
            //Calculating Minimum and Maximum values for scaling of y axis
            decimal min = this.mainViewModel.BarList[0].Item4;
            decimal max = 0m;

            min = this.mainViewModel.BarList.Min(p => p.Item4);
            max = this.mainViewModel.BarList.Max(p => p.Item3);

            //Calculating Minimum and Maximum values for zooming to the last 100 values
            decimal min100 = this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 100].Item4;
            decimal max100 = 0m;
            for (int i = this.mainViewModel.BarList.Count - 100; i < this.mainViewModel.BarList.Count; i++)
            {
                if (this.mainViewModel.BarList[i].Item4 < min100)
                    min100 = this.mainViewModel.BarList[i].Item4;
                if (this.mainViewModel.BarList[i].Item3 > max100)
                    max100 = this.mainViewModel.BarList[i].Item3;
            }

            //chart.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = false;

            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;

            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            chart.ChartAreas[0].AxisX.ScaleView.Zoom(this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 100].Item1.ToOADate(),
                                         this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 1].Item1.ToOADate());
            decimal margin100 = (max100 - min100) * 5 / 100;
            chart.ChartAreas[0].AxisY.ScaleView.Zoom(Math.Round(Convert.ToDouble(min100 - margin100)), Math.Round(Convert.ToDouble(max100 + margin100)));

            chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Chart_MouseClick);

            decimal margin = (max - min) * 5 / 100;
            //chart.ChartAreas[0].AxisY.Minimum = Math.Round(Convert.ToDouble(min - margin));
            chart.ChartAreas[0].AxisY.Maximum = Math.Round(Convert.ToDouble(max + margin));
            chart.ChartAreas[0].AxisY.Minimum = 0;
            for (int i = 0; i < this.mainViewModel.BarList.Count; i++)
            {
                // adding date and high
                chart.Series["Data"].Points.AddXY(this.mainViewModel.BarList[i].Item1, Convert.ToDouble(this.mainViewModel.BarList[i].Item3));
                // adding low
                chart.Series["Data"].Points[i].YValues[1] = Convert.ToDouble(this.mainViewModel.BarList[i].Item4);
                // adding open
                chart.Series["Data"].Points[i].YValues[2] = Convert.ToDouble(this.mainViewModel.BarList[i].Item2);
                // adding close
                chart.Series["Data"].Points[i].YValues[3] = Convert.ToDouble(this.mainViewModel.BarList[i].Item5);
            }

            Series series2 = new Series("FinFor");
            chart.Series.Add(series2);
            Series series3 = new Series("FinFor2");
            chart.Series.Add(series3);

            chart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverageConvergenceDivergence, "12,26", "Data:Y3", "FinFor");
            chart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, "90", "Data:Y3", "FinFor2");
            chart.Series["FinFor"].ChartType = SeriesChartType.FastLine;
            chart.Series["FinFor2"].ChartType = SeriesChartType.FastLine;

            chart.Series["FinFor"].Color = System.Drawing.Color.CornflowerBlue;
            chart.Series["FinFor2"].Color = System.Drawing.Color.DarkGoldenrod;

            chart.DataBind();

            // draw!
            chart.Invalidate();

            //chart.Series["FinFor"].IsXValueIndexed = true;
        }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Chart chart = this.FindName("MyWinformChart") as Chart;
                chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
                chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();
            }
        }

        public void resetCalculation()
        {
            this.mainViewModel.Orders.Clear();
            this.orders.Items.Refresh();
            this.mainViewModel.Signals.Clear();
            this.mainViewModel.BarList.Clear();
            this.mainViewModel.GainLossPercent = 0;
            this.mainViewModel.GainPercent = 0;
            this.mainViewModel.GtBtRatio = 0;
            this.mainViewModel.NoOfBadTrades = 0;
            this.mainViewModel.NoOfGoodTrades = 0;
            this.mainViewModel.StdDevOfPEquityPrice = 0;
            this.mainViewModel.StdDevOfProfit = 0;
            this.mainViewModel.LossPercent = 0;

            Chart chart = this.FindName("MyWinformChart") as Chart;
            chart.Series.Clear();

            this.StatusLabel.Text = "Ready";
            this.ProgressBar.Value = 0;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.bw.IsBusy)
            {
                this.bw.CancelAsync();

                this.resetCalculation();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.AlgorithmFileName = this.mainViewModel.AlgorithmFileName;
            Properties.Settings.Default.DataFileName = this.mainViewModel.DataFileName;
            Properties.Settings.Default.StartDate = this.mainViewModel.StartDate;
            Properties.Settings.Default.EndDate = this.mainViewModel.EndDate;

            Properties.Settings.Default.ValueOfSliderOne = this.mainViewModel.ValueOfSliderOne;
            Properties.Settings.Default.ValueOfSliderTwo = this.mainViewModel.ValueOfSliderTwo;
            Properties.Settings.Default.ValueOfSliderThree = this.mainViewModel.ValueOfSliderThree;
            Properties.Settings.Default.ValueOfSliderFour = this.mainViewModel.ValueOfSliderFour;
            Properties.Settings.Default.ValueOfSliderFive = this.mainViewModel.ValueOfSliderFive;
            Properties.Settings.Default.ValueOfSliderSix = this.mainViewModel.ValueOfSliderSix;

            Properties.Settings.Default.RoundLotSize = this.mainViewModel.RoundLotSize;
            Properties.Settings.Default.Capital = this.mainViewModel.Capital;

            Properties.Settings.Default.Save();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.mainViewModel.SaveFileName.Length == 0)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                dlg.FileName = "PerformanceData"; // Default file name
                dlg.DefaultExt = ".bts"; // Default file extension
                dlg.Filter = "Performance Data File (.bts)|*.bts"; // Filter files by extension

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    // Open document
                    this.mainViewModel.SaveFileName = dlg.FileName;
                }
            }

            if (this.mainViewModel.SaveFileName.Length != 0)
            {
                this.SaveToFile();
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.FileName = "PerformanceData"; // Default file name
            dlg.DefaultExt = ".bts"; // Default file extension
            dlg.Filter = "Performance Data File (.bts)|*.bts"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.mainViewModel.SaveFileName = dlg.FileName;
            }

            if (this.mainViewModel.SaveFileName.Length != 0)
            {
                this.SaveToFile();
            }
        }

        private void SaveToFile()
        {
            try
            {
                Stream stream = File.Open(this.mainViewModel.SaveFileName, FileMode.Create);
                BinaryFormatter bFormatter = new BinaryFormatter();
                bFormatter.Serialize(stream, this.mainViewModel.Orders);

                List<decimal> tempPerformanceList = new List<decimal>(new decimal[] {
                                   this.mainViewModel.GainLossPercent,
                                   this.mainViewModel.GainPercent,
                                   this.mainViewModel.LossPercent,
                                   this.mainViewModel.StdDevOfProfit,
                                   this.mainViewModel.StdDevOfPEquityPrice,
                                   this.mainViewModel.NoOfGoodTrades,
                                   this.mainViewModel.NoOfBadTrades,
                                   this.mainViewModel.GtBtRatio});
                bFormatter.Serialize(stream, tempPerformanceList);

                List<string> tempFileNameList = new List<string>(new string[] { this.mainViewModel.AlgorithmFileName,
                                                                            this.mainViewModel.DataFileName});
                bFormatter.Serialize(stream, tempFileNameList);

                List<DateTime> tempDateList = new List<DateTime>(new DateTime[] {this.mainViewModel.StartDate,
                                                                              this.mainViewModel.EndDate});
                bFormatter.Serialize(stream, tempDateList);

                stream.Close();
            }
            catch (Exception)
            {
                this.StatusLabel.Text = "Error occured while saving";
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "PerformanceData"; // Default file name
            dlg.DefaultExt = ".bts"; // Default file extension
            dlg.Filter = "Performance Data File (.bts)|*.bts"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.mainViewModel.LoadFileName = dlg.FileName;
            }

            if (this.mainViewModel.LoadFileName.Length != 0)
            {
                this.LoadFromFile();
            }
        }

        private void LoadFromFile()
        {
            try
            {
                Stream stream = File.Open(this.mainViewModel.LoadFileName, FileMode.Open);
                BinaryFormatter bFormatter = new BinaryFormatter();

                this.resetCalculation();

                List<Order> tempOrderList = (List<Order>)bFormatter.Deserialize(stream);
                foreach (Order order in tempOrderList)
                {
                    this.mainViewModel.Orders.Add(order);
                }
                this.orders.Items.Refresh();

                List<decimal> tempPerfomanceList = (List<decimal>)bFormatter.Deserialize(stream);
                this.mainViewModel.GainLossPercent = tempPerfomanceList[0];
                this.mainViewModel.GainPercent = tempPerfomanceList[1];
                this.mainViewModel.LossPercent = tempPerfomanceList[2];
                this.mainViewModel.StdDevOfProfit = tempPerfomanceList[3];
                this.mainViewModel.StdDevOfPEquityPrice = tempPerfomanceList[4];
                this.mainViewModel.NoOfGoodTrades = tempPerfomanceList[5];
                this.mainViewModel.NoOfBadTrades = tempPerfomanceList[6];
                this.mainViewModel.GtBtRatio = tempPerfomanceList[7];

                List<string> tempFileNameList = (List<string>)bFormatter.Deserialize(stream);
                this.mainViewModel.AlgorithmFileName = tempFileNameList[0];
                this.mainViewModel.DataFileName = tempFileNameList[1];

                List<DateTime> tempDateList = (List<DateTime>)bFormatter.Deserialize(stream);
                this.mainViewModel.StartDate = tempDateList[0];
                this.mainViewModel.EndDate = tempDateList[1];

                this.mainViewModel.SaveFileName = this.mainViewModel.LoadFileName;

                stream.Close();
            }
            catch (Exception)
            {
                this.StatusLabel.Text = "Error occured while loading";
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (e.Source == this.GeneralSettingsTabSelector)
                this.GeneralSettingsTab.IsSelected = true;
            else if (e.Source == this.OrdersSettingsTabSelector)
                this.OrdersSettingsTab.IsSelected = true;
        }
    }
}
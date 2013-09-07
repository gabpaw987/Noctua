using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Markup;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using Xceed.Wpf.Toolkit;

namespace BacktestingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bw;
        string ErrorMessage;
        Calculator c;

        bool iscalculating = false;

        private IBInput ibClient;
        private IBInput historicalDataClient;
        public Thread realTimeThread;
        public bool isRealTimeThreadRunning;
        public int curBarCount;

        private int selectedArrowIndex;

        public MainWindow()
        {
            InitializeComponent();

            this.ErrorMessage = "";

            this.mainViewModel.Orders = new List<Order>();
            this.mainViewModel.Signals = new List<int>();
            this.mainViewModel.BarList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();
            this.mainViewModel.IndicatorDictionary = new Dictionary<string, List<decimal>>();
            this.mainViewModel.OscillatorDictionary = new Dictionary<string, List<decimal>>();
            this.mainViewModel.CalculationResultSets = new Dictionary<string, CalculationResultSet>();

            this.mainViewModel.SaveFileName = string.Empty;
            this.mainViewModel.LoadFileName = string.Empty;

            this.mainViewModel.AdditionalParameters = string.Empty;

            this.mainViewModel.AlgorithmFileName = Properties.Settings.Default.AlgorithmFileName;
            this.mainViewModel.IsAlgorithmUsingMaps = Properties.Settings.Default.IsAlgorithmUsingMaps;
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

            this.mainViewModel.IsRealTimeEnabled = Properties.Settings.Default.IsRealTimeEnabled;
            this.mainViewModel.StockSymbolForRealTime = Properties.Settings.Default.StockSymbolForRealTime;
            this.mainViewModel.Barsize = Properties.Settings.Default.Barsize;

            this.mainViewModel.ValueOfSliderOne = Properties.Settings.Default.ValueOfSliderOne;
            this.mainViewModel.ValueOfSliderTwo = Properties.Settings.Default.ValueOfSliderTwo;
            this.mainViewModel.ValueOfSliderThree = Properties.Settings.Default.ValueOfSliderThree;
            this.mainViewModel.ValueOfSliderFour = Properties.Settings.Default.ValueOfSliderFour;
            this.mainViewModel.ValueOfSliderFive = Properties.Settings.Default.ValueOfSliderFive;
            this.mainViewModel.ValueOfSliderSix = Properties.Settings.Default.ValueOfSliderSix;

            this.mainViewModel.RoundLotSize = Properties.Settings.Default.RoundLotSize;
            this.mainViewModel.Capital = Properties.Settings.Default.Capital;

            this.mainViewModel.AbsTransactionFee = Properties.Settings.Default.AbsTransactionFee;
            this.mainViewModel.RelTransactionFee = Properties.Settings.Default.RelTransactionFee;
            this.mainViewModel.PricePremium = Properties.Settings.Default.PricePremium;

            this.mainViewModel.SaveFileName = Properties.Settings.Default.SaveFileName;

            this.mainViewModel.AdditionalParameters = Properties.Settings.Default.AdditionalParameters;

            this.mainViewModel.IsDataFutures = Properties.Settings.Default.IsDataFutures;
            this.mainViewModel.InnerValue = Properties.Settings.Default.InnerValue;

            this.mainViewModel.IndicatorPanels = new List<StackPanel>();
            if (Properties.Settings.Default.IndicatorPanels != null)
            {
                try
                {
                    if (Properties.Settings.Default.IndicatorPanels.Count != 0)
                    {
                        this.mainViewModel.IndicatorPanels = this.restoreIndicatorStackPanels(Properties.Settings.Default.IndicatorPanels);
                    }
                }
                catch (Exception)
                {
                    this.mainViewModel.IndicatorPanels = new List<StackPanel>();
                    Properties.Settings.Default.IndicatorPanels = new System.Collections.Specialized.StringCollection();
                }
            }
            else
            {
                Properties.Settings.Default.IndicatorPanels = new System.Collections.Specialized.StringCollection();
            }

            this.refreshIndicatorList();

            this.orders.DataContext = this.mainViewModel.Orders;

            this.bw = new BackgroundWorker();
            this.isRealTimeThreadRunning = false;
            this.curBarCount = 0;

            this.selectedArrowIndex = -1;
        }

        public List<StackPanel> restoreIndicatorStackPanels(StringCollection strings)
        {
            List<StackPanel> newList = new List<StackPanel>();

            for (int i = 0; i < strings.Count; i += 2)
            {
                newList.Add((StackPanel)XamlReader.Parse(strings[i]));

                for (int j = 0; j < newList[i / 2].Children.Count; j++)
                {
                    if (newList[i / 2].Children[j] is System.Windows.Controls.TextBox)
                        ((System.Windows.Controls.TextBox)newList[i / 2].Children[j]).PreviewTextInput += NumericOnly;
                }

                string[] argb = strings[i + 1].Split(';');
                ColorPicker cp = this.AddColorPicker();
                cp.SelectedColor = System.Windows.Media.Color.FromArgb(Convert.ToByte(argb[0]), Convert.ToByte(argb[1]), Convert.ToByte(argb[2]), Convert.ToByte(argb[3]));
                newList[i / 2].Children.Add(cp);

                ((System.Windows.Controls.Button)newList[i / 2].Children[0]).Click += RemoveIndicatorButton_Click;
            }

            return newList;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "Developers:\n\n"
                             + "Peer Nagy\npeer@gmx.at\n\n"
                             + "Gabriel Pawlowsky\ngabriel_pawlowsky@yahoo.de\n\n"
                             + "Josef Sochovsky\njosef.nikolaus@sochovsky.at";
            System.Windows.MessageBox.Show(message);
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "Backtestingsoftware Noctua\n" +
                             "Version 1.0\n\n" +
                             "This software has been developed within the scope of the \"Projects and Projectmanagement\" education " +
                             "as part of a diploma project at the Technologischen Gewerbemusem Wien. It tests algorithms for technical" +
                             "analysis in .dll-Files with historical stock market data stored in .csv-Files. Additionally " +
                             "an interface for creating such algorithm is provided to developers.";
            System.Windows.MessageBox.Show(message);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string algorithmUsed = this.mainViewModel.AlgorithmFileName.Split(new char[] { '/', '\\' }).Last();
            string datafileUsed = this.mainViewModel.DataFileName.Split(new char[] { '/', '\\' }).Last();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            try
            {
                dlg.FileName = algorithmUsed.Split(new char[] { '.' })[algorithmUsed.Split(new char[] { '.' }).Length - 2]
                           + "#" +
                           datafileUsed.Split(new char[] { '.' })[datafileUsed.Split(new char[] { '.' }).Length - 2]; // Default file name
            }
            catch (Exception)
            {
                dlg.FileName = "ExportedFile";
            }

            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Performance Data File (.txt)|*.txt"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string[] lines = new string[]
                {
                    "Algorithm used: " + algorithmUsed,
                    "Data-File used: " + datafileUsed,
                    "",
                    "Net Worth:                                    " + (this.mainViewModel.NetWorth < 0 ? "" : " ")                     + this.mainViewModel.NetWorthToDisplay,
                    "Portfolio Performance [%]:                    " + (this.mainViewModel.PortfolioPerformancePercent < 0 ? "" : " ")  + this.mainViewModel.PortfolioPerformancePercent,
                    "Sharpe Ratio:                                 " + (this.mainViewModel.SharpeRatio < 0 ? "" : " ")                  + this.mainViewModel.SharpeRatio,
                    "Mean Deviation of Portfolio Performance [%]:  " + (this.mainViewModel.StdDevOfProfit < 0 ? "" : " ")               + this.mainViewModel.StdDevOfProfit,
                    "Mean Deviation of Equity Price:               " + (this.mainViewModel.StdDevOfPEquityPrice < 0 ? "" : " ")         + this.mainViewModel.StdDevOfPEquityPrice,
                    "Return on Investment [%]:                     " + (this.mainViewModel.GainLossPercent < 0 ? "" : " ")              + this.mainViewModel.GainLossPercent,
                    "Number of Good Trades:                        " + (this.mainViewModel.NoOfGoodTrades < 0 ? "" : " ")               + this.mainViewModel.NoOfGoodTrades,
                    "Gain From Good Trades [%]:                    " + (this.mainViewModel.GainPercent < 0 ? "" : " ")                  + this.mainViewModel.GainPercent,
                    "Number of Bad Trades:                         " + (this.mainViewModel.NoOfBadTrades < 0 ? "" : " ")                + this.mainViewModel.NoOfBadTrades,
                    "Loss From Bad Trades [%]:                     " + (this.mainViewModel.LossPercent < 0 ? "" : " ")                  + this.mainViewModel.LossPercent,
                    "Ratio of Good Trades - Bad Trades:            " + (this.mainViewModel.GtBtRatio < 0 ? "" : " ")                    + this.mainViewModel.GtBtRatio,
                    "Highest Daily Portfolio Performance:          " + (this.mainViewModel.HighestDailyProfit < 0 ? "" : " ")           + this.mainViewModel.HighestDailyProfit,
                    "Lowest Daily Portfolio Performance:           " + (this.mainViewModel.HighestDailyLoss < 0 ? "" : " ")             + this.mainViewModel.HighestDailyLoss,
                    "Portfolio Performance of the Current Day:     " + (this.mainViewModel.HighestDailyProfit < 0 ? "" : " ")           + this.mainViewModel.HighestDailyProfit,
                };

                // Open document
                System.IO.File.WriteAllLines(dlg.FileName, lines);
            }
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
            if (!this.iscalculating)
            {
                this.iscalculating = true;

                this.StopButton_Click(null, null);

                this.resetCalculation(true);

                if (this.isRealTimeThreadRunning)
                {
                    ibClient.Disconnect();
                    this.isRealTimeThreadRunning = false;
                }
                if (this.mainViewModel.IsRealTimeEnabled)
                {
                    if (this.mainViewModel.Barsize.Equals("Minute"))
                    {
                        this.ibClient = new IBInput(1, this.mainViewModel.BarList, new Equity(this.mainViewModel.StockSymbolForRealTime), BarSize.OneMinute);
                        this.historicalDataClient = new IBInput(2, this.mainViewModel.BarList, new Equity(this.mainViewModel.StockSymbolForRealTime), BarSize.OneMinute);
                    }
                    else if (this.mainViewModel.Barsize.Equals("Daily"))
                    {
                        this.ibClient = new IBInput(1, this.mainViewModel.BarList, new Equity(this.mainViewModel.StockSymbolForRealTime), BarSize.OneDay);
                        this.historicalDataClient = new IBInput(2, this.mainViewModel.BarList, new Equity(this.mainViewModel.StockSymbolForRealTime), BarSize.OneDay);
                    }

                    this.ibClient.hadFirst = false;
                    this.ErrorMessage = this.ibClient.Connect();
                }

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

                    this.c = new Calculator(this.mainViewModel);

                    // report the progress
                    if (this.mainViewModel.IsRealTimeEnabled)
                        b.ReportProgress(5, "Reading From IB...");
                    else
                        b.ReportProgress(5, "Reading File...");

                    if (!this.isRealTimeThreadRunning)
                    {
                        try
                        {
                            if (this.ErrorMessage.Length == 0)
                            {
                                if (this.mainViewModel.IsRealTimeEnabled)
                                {
                                    ibClient.SubscribeForRealTimeBars();

                                    //Wait for first 5sec bar
                                    while (ibClient.RealTimeBarList.Count <= 1)
                                    {
                                        System.Threading.Thread.Sleep(100);
                                        if (!this.iscalculating)
                                        {
                                            break;
                                        }
                                    }

                                    //wait until minute is full
                                    if (this.iscalculating)
                                    {
                                        if (this.mainViewModel.Barsize.Equals("Minute"))
                                        {
                                            while (ibClient.RealTimeBarList.Count != 0)
                                            {
                                                System.Threading.Thread.Sleep(100);
                                                if (!this.iscalculating)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        //wait until day is full
                                        else if (this.mainViewModel.Barsize.Equals("Daily"))
                                        {
                                            while (ibClient.RealTimeBarList.Count != 0)
                                            {
                                                System.Threading.Thread.Sleep(1000);
                                                if (!this.iscalculating)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (this.iscalculating)
                                    {
                                        this.ErrorMessage = this.historicalDataClient.Connect();
                                    }

                                    if (this.iscalculating)
                                    {
                                        if (this.ErrorMessage.Length == 0)
                                            this.historicalDataClient.GetHistoricalDataBars();
                                    }

                                    if (this.iscalculating)
                                    {
                                        while (this.mainViewModel.BarList.Count < this.historicalDataClient.totalHistoricalBars ||
                                               this.historicalDataClient.totalHistoricalBars == 0)
                                        {
                                            System.Threading.Thread.Sleep(100);
                                            if (!this.iscalculating)
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    if (this.iscalculating)
                                    {
                                        this.curBarCount = this.mainViewModel.BarList.Count;

                                        this.historicalDataClient.Disconnect();
                                    }
                                }
                                else
                                {
                                    c.ReadFile();
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (this.ErrorMessage.Length == 0 && this.iscalculating)
                                this.ErrorMessage = "An error with the Data-File occured.";
                        }
                    }

                    // report the progress
                    b.ReportProgress(40, "Calculating Signals...");

                    if (this.mainViewModel.AdditionalParameters.Length == 0)
                    {
                        try
                        {
                            if (this.ErrorMessage.Length == 0 && this.iscalculating)
                            {
                                Type t = c.LoadAlgorithmFile();
                                c.CalculateSignals(t, null);
                            }
                        }
                        catch (Exception)
                        {
                            this.ErrorMessage = "An error with the Algorithm-File occured.";
                        }

                        // report the progress
                        b.ReportProgress(70, "Calculating Performance...");

                        try
                        {
                            if (this.ErrorMessage.Length == 0 && this.iscalculating)
                                this.ErrorMessage = c.CalculateNumbers(string.Empty);

                            if (this.ErrorMessage.Length == 0 && this.iscalculating)
                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    this.ResultSelectionComboBox.SelectedIndex = 0;
                                }));
                        }
                        catch (Exception)
                        {
                            this.ErrorMessage = "An error while calculating performance data occured.";
                        }
                    }
                    else
                    {
                        try
                        {
                            Dictionary<string, List<decimal>> parameters = new Dictionary<string, List<decimal>>();
                            //split AdditionalParameters string
                            string[] separatedAdditionalParameters = this.mainViewModel.AdditionalParameters.Split(';');
                            foreach (string parameter in separatedAdditionalParameters)
                            {
                                string[] separatedParameter = parameter.Split(',');
                                List<decimal> decimalParameter = new List<decimal>();
                                for (int i = 1; i < separatedParameter.Length; i++)
                                {
                                    decimalParameter.Add(decimal.Parse(separatedParameter[i], CultureInfo.InvariantCulture));
                                }
                                parameters.Add(separatedParameter[0], decimalParameter);
                            }

                            List<List<decimal>> valueSets = new List<List<decimal>>();
                            List<List<decimal>> parameterRanges = new List<List<decimal>>();

                            //fill parameterRanges list
                            foreach (List<decimal> informations in parameters.Values)
                            {
                                List<decimal> col = new List<decimal>();
                                for (decimal i = informations[0]; i <= informations[1]; i += informations[2])
                                {
                                    col.Add(i);
                                }
                                parameterRanges.Add(col);
                                valueSets.Add(new List<decimal>());
                            }
                            Calculator.mesh(0, parameterRanges, valueSets);

                            decimal progress = 40m;
                            Type t = c.LoadAlgorithmFile();

                            for (int i = 0; i < valueSets[0].Count; i++)
                            {
                                string description = string.Empty;

                                this.mainViewModel.OscillatorDictionary.Clear();
                                this.mainViewModel.IndicatorDictionary.Clear();
                                this.mainViewModel.Signals.Clear();
                                this.mainViewModel.Orders.Clear();

                                Dictionary<string, decimal> valueSet = new Dictionary<string, decimal>();
                                for (int j = 0; j < valueSets.Count; j++)
                                {
                                    valueSet.Add(parameters.Keys.ToList()[j], valueSets[j][i]);

                                    if (description.Length != 0)
                                    {
                                        description += ",";
                                    }
                                    description += parameters.Keys.ToList()[j] + ": " + valueSets[j][i];
                                }

                                progress += (60m / (2 * valueSets[0].Count));

                                // report the progress
                                b.ReportProgress((int)Math.Round(progress, 0, MidpointRounding.AwayFromZero), "Calculating Signals...");

                                try
                                {
                                    if (this.ErrorMessage.Length == 0 && this.iscalculating)
                                    {
                                        c.CalculateSignals(t, valueSet);
                                    }
                                }
                                catch (Exception)
                                {
                                    this.ErrorMessage = "An error with the Algorithm-File occured.";
                                }

                                progress += (60m / (2 * valueSets[0].Count));

                                // report the progress
                                b.ReportProgress((int)Math.Round(progress, 0, MidpointRounding.AwayFromZero), "Calculating Performance...");

                                try
                                {
                                    if (this.ErrorMessage.Length == 0 && this.iscalculating)
                                        this.ErrorMessage = c.CalculateNumbers(description);
                                }
                                catch (Exception)
                                {
                                    this.ErrorMessage = "An error while calculating performance data occured.";
                                }
                            }

                            if (this.ErrorMessage.Length == 0 && this.iscalculating)
                            {
                                CalculationResultSet highestPPResultSet = new CalculationResultSet();
                                CalculationResultSet lowestPPResultSet = new CalculationResultSet();
                                foreach (CalculationResultSet resultSet in this.mainViewModel.CalculationResultSets.Values)
                                {
                                    if (highestPPResultSet.PortfolioPerformancePercent < resultSet.PortfolioPerformancePercent)
                                    {
                                        highestPPResultSet = resultSet;
                                    }

                                    if (lowestPPResultSet.PortfolioPerformancePercent > resultSet.PortfolioPerformancePercent)
                                    {
                                        lowestPPResultSet = resultSet;
                                    }
                                }

                                string highestPPResultSetKey = this.mainViewModel.CalculationResultSets.FirstOrDefault(x => x.Value.Equals(highestPPResultSet)).Key;
                                this.mainViewModel.CalculationResultSets.Remove(highestPPResultSetKey);
                                highestPPResultSetKey += " [Best]";
                                this.mainViewModel.CalculationResultSets.Add(highestPPResultSetKey, highestPPResultSet);

                                string lowestPPResultSetKey = this.mainViewModel.CalculationResultSets.FirstOrDefault(x => x.Value.Equals(lowestPPResultSet)).Key;
                                this.mainViewModel.CalculationResultSets.Remove(lowestPPResultSetKey);
                                lowestPPResultSetKey += " [Worst]";
                                this.mainViewModel.CalculationResultSets.Add(lowestPPResultSetKey, lowestPPResultSet);

                                this.Dispatcher.Invoke((Action)(() =>
                                {
                                    this.ResultSelectionComboBox.Items.Refresh();

                                    this.ResultSelectionComboBox.SelectedItem = highestPPResultSetKey;
                                }));
                            }
                        }
                        catch (Exception)
                        {
                            this.ErrorMessage = "An error while evaluating the additional parameters occured";
                        }
                    }
                });

                // what to do when progress changed (update the progress bar for example)
                bw.ProgressChanged += new ProgressChangedEventHandler(
                delegate(object o, ProgressChangedEventArgs args)
                {
                    this.StatusLabel.Text = String.Empty + args.UserState.ToString();
                    this.ProgressBar.Value = args.ProgressPercentage;
                    if (args.ProgressPercentage == 0)
                        this.ProgressBar.Visibility = Visibility.Visible;
                });

                // what to do when worker completes its task (notify the user)
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate(object o, RunWorkerCompletedEventArgs args)
                {
                    this.StatusLabel.Text = "Drawing Chart...";
                    try
                    {
                        if (this.iscalculating)
                        {
                            this.LoadLineChartData();
                        }
                    }
                    catch (Exception)
                    {
                        if (this.ErrorMessage.Length == 0 && this.iscalculating)
                            this.ErrorMessage = "An error while drawing occured.";
                    }

                    this.orders.Items.Refresh();

                    if (this.ErrorMessage.Length == 0 && this.mainViewModel.IsRealTimeEnabled && !this.isRealTimeThreadRunning && this.iscalculating &&
                        this.mainViewModel.AdditionalParameters.Length == 0)
                    {
                        this.realTimeThread = new Thread(doRealTimeThreadWork);
                        this.realTimeThread.Start();
                    }

                    if (this.ErrorMessage.Length != 0)
                    {
                        this.StatusLabel.Text = this.ErrorMessage;
                        this.ErrorMessage = "";
                    }
                    else
                    {
                        this.StatusLabel.Text = "Finished!";
                    }

                    this.ProgressBar.Value = 100;
                    this.ProgressBar.Visibility = Visibility.Hidden;
                    this.iscalculating = false;
                });

                bw.RunWorkerAsync();
            }
        }

        public void doRealTimeThreadWork()
        {
            this.isRealTimeThreadRunning = true;

            while (isRealTimeThreadRunning)
            {
                while (this.curBarCount == this.mainViewModel.BarList.Count)
                {
                    System.Threading.Thread.Sleep(100);
                    if (!this.isRealTimeThreadRunning)
                    {
                        break;
                    }
                }

                this.curBarCount = this.mainViewModel.BarList.Count;

                if (this.isRealTimeThreadRunning)
                {
                    this.iscalculating = true;

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.resetCalculation(false);
                    }));

                    Console.WriteLine(this.ErrorMessage);
                    if (this.ErrorMessage.Length == 0)
                    {
                        Type t = c.LoadAlgorithmFile();
                        c.CalculateSignals(t, null);
                    }

                    if (this.ErrorMessage.Length == 0)
                        this.ErrorMessage = c.CalculateNumbers(string.Empty);

                    if (this.ErrorMessage.Length == 0)
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            this.LoadLineChartData();
                        }));
                    }

                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        this.orders.Items.Refresh();
                    }));

                    this.iscalculating = false;
                }

                Console.WriteLine(this.ErrorMessage);
            }
        }

        private void LoadLineChartData()
        {
            Chart chart = this.FindName("MyWinformChart") as Chart;

            Series series = new Series("Data");
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

            chart.ChartAreas[0].CursorY.Interval = 0.1;

            chart.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = false;

            //for right zooming of minute bars
            if (this.mainViewModel.Barsize.Equals("Minute"))
            {
                chart.ChartAreas[0].CursorX.Interval = 1 / 1440D;
                chart.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM/yy HH:mm:ss";
            }
            else if (this.mainViewModel.Barsize.Equals("Daily"))
            {
                chart.ChartAreas[0].CursorX.Interval = 1D;
                chart.ChartAreas[0].AxisX.LabelStyle.Format = "dd/MM/yy";
            }

            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            chart.ChartAreas[0].AxisX.ScaleView.Zoom(this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 100].Item1.ToOADate(),
                                         this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 1].Item1.ToOADate());
            decimal margin100 = (max100 - min100) * 5 / 100;
            chart.ChartAreas[0].AxisY.ScaleView.Zoom(Math.Round(Convert.ToDouble(min100 - margin100)), Math.Round(Convert.ToDouble(max100 + margin100)));

            chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Chart_MouseClick);

            decimal margin = (max - min) * 5 / 100;
            chart.ChartAreas[0].AxisY.Minimum = Math.Round(Convert.ToDouble(min - margin));
            chart.ChartAreas[0].AxisY.Maximum = Math.Round(Convert.ToDouble(max + margin));

            int k = 0;

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

                if (i != 0)
                {
                    if (this.mainViewModel.Signals[i - 1] != this.mainViewModel.Signals[i])
                    {
                        ArrowAnnotation la = new ArrowAnnotation();
                        la.Name = "Arrow-" + k++;

                        //la.AnchorDataPoint = chart.Series["Data"].Points[i];

                        la.Width = 0;
                        la.ClipToChartArea = chart.ChartAreas[0].Name;
                        la.ArrowSize = 3;
                        la.LineWidth = 2;

                        setArrowColor(la, this.mainViewModel.Signals[i]);

                        if (this.mainViewModel.Signals[i] < 0)
                        {
                            la.Height = -5;
                            la.LineColor = System.Drawing.Color.Black;

                            la.AnchorDataPoint = chart.Series["Data"].Points[i];
                            //indicates which one of the y values of the datapoint is used for the arrow
                            la.AnchorY = chart.Series["Data"].Points[i].YValues[0];
                            la.AnchorOffsetY = -2;
                        }
                        else if (this.mainViewModel.Signals[i] > 0)
                        {
                            la.Height = 5;
                            la.LineColor = System.Drawing.Color.Black;

                            la.AnchorDataPoint = chart.Series["Data"].Points[i];
                            //indicates which one of the y values of the datapoint is used for the arrow
                            la.AnchorY = chart.Series["Data"].Points[i].YValues[1];
                            la.AnchorOffsetY = 2;
                        }
                        else if (this.mainViewModel.Signals[i] == 0)
                        {
                            if (this.mainViewModel.Signals[i - 1] < 0)
                            {
                                la.Height = 5;
                                la.LineColor = System.Drawing.Color.Black;
                                la.BackColor = System.Drawing.Color.White;

                                la.AnchorDataPoint = chart.Series["Data"].Points[i];
                                //indicates which one of the y values of the datapoint is used for the arrow
                                la.AnchorY = chart.Series["Data"].Points[i].YValues[1];
                                la.AnchorOffsetY = 2;
                            }
                            else if (this.mainViewModel.Signals[i - 1] > 0)
                            {
                                la.Height = -5;
                                la.LineColor = System.Drawing.Color.Black;
                                la.BackColor = System.Drawing.Color.White;

                                la.AnchorDataPoint = chart.Series["Data"].Points[i];
                                //indicates which one of the y values of the datapoint is used for the arrow
                                la.AnchorY = chart.Series["Data"].Points[i].YValues[0];
                                la.AnchorOffsetY = -2;
                            }
                        }
                        chart.Annotations.Add(la);
                    }
                }
            }

            double min2 = 0;
            double max2 = 0;

            try
            {
                for (int i = 0; i < this.mainViewModel.IndicatorPanels.Count; i++)
                {
                    Series formula = new Series("FinFor" + i);
                    chart.Series.Add(formula);

                    if (((System.Windows.Controls.Label)this.mainViewModel.IndicatorPanels[i].Children[1]).Content.Equals("Simple Moving Average"))
                    {
                        chart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverage, ((System.Windows.Controls.TextBox)this.mainViewModel.IndicatorPanels[i].Children[3]).Text, "Data:Y3", "FinFor" + i);
                        chart.Series["FinFor" + i].ChartType = SeriesChartType.FastLine;
                    }
                    else if (((System.Windows.Controls.Label)this.mainViewModel.IndicatorPanels[i].Children[1]).Content.Equals("Weighted Moving Average"))
                    {
                        chart.DataManipulator.FinancialFormula(FinancialFormula.WeightedMovingAverage, ((System.Windows.Controls.TextBox)this.mainViewModel.IndicatorPanels[i].Children[3]).Text, "Data:Y3", "FinFor" + i);
                        chart.Series["FinFor" + i].ChartType = SeriesChartType.FastLine;
                    }
                    else if (((System.Windows.Controls.Label)this.mainViewModel.IndicatorPanels[i].Children[1]).Content.Equals("Exponential Moving Average"))
                    {
                        chart.DataManipulator.FinancialFormula(FinancialFormula.ExponentialMovingAverage, ((System.Windows.Controls.TextBox)this.mainViewModel.IndicatorPanels[i].Children[3]).Text, "Data:Y3", "FinFor" + i);
                        chart.Series["FinFor" + i].ChartType = SeriesChartType.FastLine;
                    }
                    else if (((System.Windows.Controls.Label)this.mainViewModel.IndicatorPanels[i].Children[1]).Content.Equals("Moving Average Convergence-Divergence"))
                    {
                        string shortLength = ((System.Windows.Controls.TextBox)this.mainViewModel.IndicatorPanels[i].Children[3]).Text;
                        string longLength = ((System.Windows.Controls.TextBox)this.mainViewModel.IndicatorPanels[i].Children[5]).Text;

                        if (shortLength.Length != 0 && longLength.Length != 0 &&
                            !shortLength.Length.Equals("0") && !longLength.Length.Equals("0"))
                        {
                            if (chart.ChartAreas.Count <= 1)
                                this.drawSecondChartArea(chart);

                            formula.ChartArea = "IndicatorArea";

                            chart.DataManipulator.FinancialFormula(FinancialFormula.MovingAverageConvergenceDivergence, shortLength + "," + longLength, "Data:Y3", "FinFor" + i);
                            chart.Series["FinFor" + i].ChartType = SeriesChartType.FastLine;

                            for (int j = 0; j < chart.Series["FinFor" + i].Points.Count; j++)
                            {
                                if (chart.Series["FinFor" + i].Points[j].YValues[0] < min2)
                                    min2 = chart.Series["FinFor" + i].Points[j].YValues[0];
                                else if (chart.Series["FinFor" + i].Points[j].YValues[0] > max2)
                                    max2 = chart.Series["FinFor" + i].Points[j].YValues[0];
                            }
                        }
                    }
                    else
                    {
                        chart.Series.Remove(formula);
                        break;
                    }

                    System.Windows.Media.Color tempColor = new System.Windows.Media.Color();

                    for (int j = 0; j < this.mainViewModel.IndicatorPanels[i].Children.Count; j++)
                    {
                        if (this.mainViewModel.IndicatorPanels[i].Children[j] is ColorPicker)
                            tempColor = ((ColorPicker)this.mainViewModel.IndicatorPanels[i].Children[j]).SelectedColor;
                    }

                    chart.Series["FinFor" + i].Color = System.Drawing.Color.FromArgb(tempColor.A, tempColor.R, tempColor.G, tempColor.B);
                }
            }
            catch (Exception)
            {
                this.ErrorMessage = "An error occured while drawing indicators!";

                //reset calculation
                this.resetCalculation(true);
            }

            try
            {
                int index = 0;

                foreach (string key in this.mainViewModel.IndicatorDictionary.Keys)
                {
                    if (this.mainViewModel.IndicatorDictionary[key].Count == this.mainViewModel.BarList.Count)
                    {
                        Series indicatorSeries = new Series("Indicator" + index);
                        chart.Series.Add(indicatorSeries);

                        for (int i = 0; i < this.mainViewModel.BarList.Count; i++)
                        {
                            chart.Series["Indicator" + index].Points.AddXY(this.mainViewModel.BarList[i].Item1, this.mainViewModel.IndicatorDictionary[key][i]);
                        }

                        foreach (decimal value in this.mainViewModel.IndicatorDictionary[key])
                        {
                            if (value < min && value != 0)
                            {
                                min = value;
                            }
                        }

                        max = this.mainViewModel.IndicatorDictionary[key].Max();

                        margin = (max - min) * 5 / 100;
                        if (Convert.ToDouble(min - margin) < chart.ChartAreas[0].AxisY.Minimum)
                        {
                            chart.ChartAreas[0].AxisY.Minimum = Math.Round(Convert.ToDouble(min - margin));
                        }
                        if (Convert.ToDouble(max + margin) > chart.ChartAreas[0].AxisY.Maximum)
                        {
                            chart.ChartAreas[0].AxisY.Maximum = Math.Round(Convert.ToDouble(max + margin));
                        }

                        // Set series chart type
                        chart.Series["Indicator" + index].ChartType = SeriesChartType.FastLine;

                        //chart.DataSource = this.mainViewModel.BarList;
                        chart.Series["Indicator" + index].XValueMember = "DateStamp";
                        chart.Series["Indicator" + index].XValueType = ChartValueType.DateTime;
                        chart.Series["Indicator" + index].YValueMembers = "Value";

                        //chart.Series["Data"].BorderColor = System.Drawing.Color.Black;
                        chart.Series["Indicator" + index].Color = System.Drawing.ColorTranslator.FromHtml(key.Split(';')[1]);

                        index++;
                    }
                }

                if (this.mainViewModel.OscillatorDictionary.Count > 0)
                {
                    if (chart.ChartAreas.Count <= 1)
                    {
                        this.drawSecondChartArea(chart);
                    }
                    foreach (string key in this.mainViewModel.OscillatorDictionary.Keys)
                    {
                        if (this.mainViewModel.OscillatorDictionary[key].Count == this.mainViewModel.BarList.Count)
                        {
                            Series indicatorSeries = new Series("Oscillator" + index);
                            indicatorSeries.ChartArea = "OscillatorArea";
                            chart.Series.Add(indicatorSeries);

                            for (int i = 0; i < this.mainViewModel.BarList.Count; i++)
                            {
                                chart.Series["Oscillator" + index].Points.AddXY(this.mainViewModel.BarList[i].Item1, this.mainViewModel.OscillatorDictionary[key][i]);
                            }

                            // Set series chart type
                            chart.Series["Oscillator" + index].ChartType = SeriesChartType.FastLine;

                            //chart.DataSource = this.mainViewModel.BarList;
                            chart.Series["Oscillator" + index].XValueMember = "DateStamp";
                            chart.Series["Oscillator" + index].XValueType = ChartValueType.DateTime;
                            chart.Series["Oscillator" + index].YValueMembers = "Value";

                            //chart.Series["Data"].BorderColor = System.Drawing.Color.Black;
                            chart.Series["Oscillator" + index].Color = System.Drawing.ColorTranslator.FromHtml(key.Split(';')[1]);

                            for (int j = 0; j < chart.Series["Oscillator" + index].Points.Count; j++)
                            {
                                if (chart.Series["Oscillator" + index].Points[j].YValues[0] < min2)
                                    min2 = chart.Series["Oscillator" + index].Points[j].YValues[0];
                                else if (chart.Series["Oscillator" + index].Points[j].YValues[0] > max2)
                                    max2 = chart.Series["Oscillator" + index].Points[j].YValues[0];
                            }

                            index++;
                        }
                    }
                }
            }
            catch (Exception)
            {
                this.ErrorMessage = "An error occured while drawing indicators!";

                //reset calculation
                this.resetCalculation(true);
            }

            if (chart.ChartAreas.Count > 1)
            {
                double margin2 = (max2 - min2) * 5 / 100;
                chart.ChartAreas[1].AxisY.Minimum = Math.Round(min2 - margin2);
                chart.ChartAreas[1].AxisY.Maximum = Math.Round(max2 + margin2);
            }

            if (chart.ChartAreas.Count <= 1)
                this.drawSecondEmptyChartArea(chart);

            chart.DataBind();

            // draw!
            chart.Invalidate();
        }

        private void setArrowColor(ArrowAnnotation la, int signal)
        {
            switch (signal)
            {
                case -3:
                    la.BackColor = System.Drawing.Color.FromArgb(102, 0, 0);
                    break;
                case -2:
                    la.BackColor = System.Drawing.Color.FromArgb(255, 0, 0);
                    break;
                case -1:
                    la.BackColor = System.Drawing.Color.FromArgb(255, 204, 204);
                    break;
                case 0:
                    la.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
                    break;
                case 1:
                    la.BackColor = System.Drawing.Color.FromArgb(0, 255, 51);
                    break;
                case 2:
                    la.BackColor = System.Drawing.Color.FromArgb(0, 153, 0);
                    break;
                case 3:
                    la.BackColor = System.Drawing.Color.FromArgb(0, 51, 0);
                    break;
            }
        }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            Chart chart = sender as Chart;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var pos = e.Location;
                var results = chart.HitTest(pos.X, pos.Y, false,
                                             ChartElementType.Annotation);
                foreach (var result in results)
                {
                    if (result.Object != null)
                    {
                        if (result.Object is ArrowAnnotation)
                        {
                            int selectedIndex = int.Parse(((ArrowAnnotation)result.Object).Name.Split('-')[1]);
                            this.orders.SelectedIndex = selectedIndex;
                            this.orders.ScrollIntoView(this.mainViewModel.Orders[selectedIndex]);
                        }
                    }
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
                chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();

                if (chart.ChartAreas.Count > 1)
                {
                    chart.ChartAreas[1].AxisX.ScaleView.ZoomReset();
                    chart.ChartAreas[1].AxisY.ScaleView.ZoomReset();
                }
            }
        }

        public void drawSecondChartArea(Chart chart)
        {
            ChartArea indicatorArea = new ChartArea("OscillatorArea");
            chart.ChartAreas.Add(indicatorArea);

            chart.ChartAreas[1].CursorY.IsUserEnabled = true;
            chart.ChartAreas[1].CursorY.IsUserSelectionEnabled = true;
            chart.ChartAreas[1].AxisY.ScaleView.Zoomable = true;
            chart.ChartAreas[1].AxisY.ScrollBar.IsPositionedInside = false;

            chart.ChartAreas[1].CursorX.IsUserEnabled = true;
            chart.ChartAreas[1].CursorX.IsUserSelectionEnabled = true;
            chart.ChartAreas[1].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas[1].AxisX.ScrollBar.IsPositionedInside = false;

            chart.ChartAreas[1].AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            chart.ChartAreas[1].AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;

            chart.ChartAreas[1].AlignWithChartArea = "MainArea";

            chart.ChartAreas[1].AxisX.Minimum = this.mainViewModel.BarList[0].Item1.ToOADate();
            chart.ChartAreas[1].AxisX.Maximum = this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 1].Item1.ToOADate();

            chart.ChartAreas[1].AxisX.ScaleView.Zoom(this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 100].Item1.ToOADate(),
                                 this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 1].Item1.ToOADate());

            chart.ChartAreas[0].Position.Width = 100;
            chart.ChartAreas[0].Position.X = 0;
            chart.ChartAreas[0].Position.Height = 70;
            chart.ChartAreas[0].Position.Y = 0;

            chart.ChartAreas[1].Position.Width = 100;
            chart.ChartAreas[1].Position.X = 0;
            chart.ChartAreas[1].Position.Height = 30;
            chart.ChartAreas[1].Position.Y = 70;
        }

        public void drawSecondEmptyChartArea(Chart chart)
        {
            ChartArea indicatorArea = new ChartArea("IndicatorArea");
            chart.ChartAreas.Add(indicatorArea);

            chart.ChartAreas[1].AlignWithChartArea = "MainArea";

            chart.ChartAreas[1].AxisX.ScaleView.Zoom(this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 100].Item1.ToOADate(),
                                 this.mainViewModel.BarList[this.mainViewModel.BarList.Count - 1].Item1.ToOADate());

            chart.ChartAreas[1].Position.Width = 100;
            chart.ChartAreas[1].Position.X = 0;
            chart.ChartAreas[1].Position.Height = 0;
            chart.ChartAreas[1].Position.Y = 100;
        }

        public void resetCalculation(bool resetBarList)
        {
            this.mainViewModel.Orders.Clear();
            this.orders.Items.Refresh();

            if (resetBarList)
            {
                this.mainViewModel.BarList.Clear();
            }

            this.mainViewModel.IndicatorDictionary.Clear();
            this.mainViewModel.OscillatorDictionary.Clear();

            this.mainViewModel.CalculationResultSets.Clear();
            this.ResultSelectionComboBox.Items.Refresh();

            this.mainViewModel.Signals.Clear();
            this.mainViewModel.GainLossPercent = 0;
            this.mainViewModel.GainPercent = 0;
            this.mainViewModel.GtBtRatio = 0;
            this.mainViewModel.NoOfBadTrades = 0;
            this.mainViewModel.NoOfGoodTrades = 0;
            this.mainViewModel.StdDevOfPEquityPrice = 0;
            this.mainViewModel.StdDevOfProfit = 0;
            this.mainViewModel.LossPercent = 0;
            this.mainViewModel.NetWorth = 0;
            this.mainViewModel.PortfolioPerformancePercent = 0;
            this.mainViewModel.SharpeRatio = 0;
            this.mainViewModel.HighestDailyProfit = 0;
            this.mainViewModel.HighestDailyLoss = 0;
            this.mainViewModel.LastDayProfitLoss = 0;

            Chart chart = this.FindName("MyWinformChart") as Chart;
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(new ChartArea("MainArea"));
            chart.Annotations.Clear();

            this.StatusLabel.Text = "Ready";
            this.ProgressBar.Value = 0;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.isRealTimeThreadRunning = false;

                if (this.bw.IsBusy)
                {
                    this.bw.CancelAsync();

                    this.resetCalculation(true);

                    this.iscalculating = false;
                }

                //Disconnect the IBInput clients
                if (this.ibClient != null)
                {
                    if (this.ibClient.IsConnected)
                    {
                        this.ibClient.Disconnect();
                    }
                }
                if (this.historicalDataClient != null)
                {
                    if (this.historicalDataClient.IsConnected)
                    {
                        this.historicalDataClient.Disconnect();
                    }
                }
            }
            catch (Exception)
            { }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.StopButton_Click(null, null);

            Properties.Settings.Default.AlgorithmFileName = this.mainViewModel.AlgorithmFileName;
            Properties.Settings.Default.IsAlgorithmUsingMaps = this.mainViewModel.IsAlgorithmUsingMaps;
            Properties.Settings.Default.DataFileName = this.mainViewModel.DataFileName;
            Properties.Settings.Default.StartDate = this.mainViewModel.StartDate;
            Properties.Settings.Default.EndDate = this.mainViewModel.EndDate;
            Properties.Settings.Default.IsRealTimeEnabled = this.mainViewModel.IsRealTimeEnabled;
            Properties.Settings.Default.StockSymbolForRealTime = this.mainViewModel.StockSymbolForRealTime;
            Properties.Settings.Default.Barsize = this.mainViewModel.Barsize;
            Properties.Settings.Default.AdditionalParameters = this.mainViewModel.AdditionalParameters;

            Properties.Settings.Default.ValueOfSliderOne = this.mainViewModel.ValueOfSliderOne;
            Properties.Settings.Default.ValueOfSliderTwo = this.mainViewModel.ValueOfSliderTwo;
            Properties.Settings.Default.ValueOfSliderThree = this.mainViewModel.ValueOfSliderThree;
            Properties.Settings.Default.ValueOfSliderFour = this.mainViewModel.ValueOfSliderFour;
            Properties.Settings.Default.ValueOfSliderFive = this.mainViewModel.ValueOfSliderFive;
            Properties.Settings.Default.ValueOfSliderSix = this.mainViewModel.ValueOfSliderSix;

            Properties.Settings.Default.AbsTransactionFee = this.mainViewModel.AbsTransactionFee;
            Properties.Settings.Default.RelTransactionFee = this.mainViewModel.RelTransactionFee;
            Properties.Settings.Default.PricePremium = this.mainViewModel.PricePremium;

            Properties.Settings.Default.RoundLotSize = this.mainViewModel.RoundLotSize;
            Properties.Settings.Default.Capital = this.mainViewModel.Capital;

            Properties.Settings.Default.SaveFileName = this.mainViewModel.SaveFileName;

            Properties.Settings.Default.IsDataFutures = this.mainViewModel.IsDataFutures;
            Properties.Settings.Default.InnerValue = this.mainViewModel.InnerValue;

            Properties.Settings.Default.IndicatorPanels = this.storeIndicatorStackPanels(this.mainViewModel.IndicatorPanels);

            Properties.Settings.Default.Save();
        }

        public StringCollection storeIndicatorStackPanels(List<StackPanel> stackPanels)
        {
            StringCollection strings = new StringCollection();
            if (stackPanels.Count != 0)
            {
                foreach (StackPanel sp in stackPanels)
                {
                    if (sp.Children[sp.Children.Count - 1].GetType().IsAssignableFrom((new ColorPicker()).GetType()))
                    {
                        ColorPicker cp = ((ColorPicker)sp.Children[sp.Children.Count - 1]);
                        sp.Children.Remove(cp);
                        strings.Add(XamlWriter.Save(sp));
                        strings.Add(cp.SelectedColor.A + ";" + cp.SelectedColor.R + ";" + cp.SelectedColor.G + ";" + cp.SelectedColor.B);
                        sp.Children.Add(cp);
                    }
                }
            }
            return strings;
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
                                   this.mainViewModel.GtBtRatio,
                                   this.mainViewModel.NetWorth,
                                   this.mainViewModel.PortfolioPerformancePercent,
                                   this.mainViewModel.SharpeRatio});
                bFormatter.Serialize(stream, tempPerformanceList);

                List<bool> tempBoolList = new List<bool>(new bool[] {
                                   this.mainViewModel.IsRealTimeEnabled,
                                   this.mainViewModel.IsAlgorithmUsingMaps,
                                   this.mainViewModel.IsDataFutures});
                bFormatter.Serialize(stream, tempBoolList);

                List<string> tempStringList = new List<string>(new string[] { this.mainViewModel.AlgorithmFileName,
                                                                            this.mainViewModel.DataFileName,
                                                                            this.mainViewModel.Capital,
                                                                            this.mainViewModel.AbsTransactionFee,
                                                                            this.mainViewModel.RelTransactionFee,
                                                                            this.mainViewModel.PricePremium,
                                                                            this.mainViewModel.StockSymbolForRealTime,
                                                                            this.mainViewModel.Barsize,
                                                                            this.mainViewModel.AdditionalParameters});
                bFormatter.Serialize(stream, tempStringList);

                List<DateTime> tempDateList = new List<DateTime>(new DateTime[] {this.mainViewModel.StartDate,
                                                                              this.mainViewModel.EndDate});
                bFormatter.Serialize(stream, tempDateList);

                List<int> tempIntList = new List<int>(new int[] {this.mainViewModel.ValueOfSliderOne,
                                                                 this.mainViewModel.ValueOfSliderTwo,
                                                                 this.mainViewModel.ValueOfSliderThree,
                                                                 this.mainViewModel.ValueOfSliderFour,
                                                                 this.mainViewModel.ValueOfSliderFive,
                                                                 this.mainViewModel.ValueOfSliderSix,
                                                                 this.mainViewModel.RoundLotSize,
                                                                 this.mainViewModel.InnerValue});
                bFormatter.Serialize(stream, tempIntList);

                StringCollection serializableStackPanels = new StringCollection();
                serializableStackPanels = this.storeIndicatorStackPanels(this.mainViewModel.IndicatorPanels);
                bFormatter.Serialize(stream, serializableStackPanels);

                bFormatter.Serialize(stream, this.mainViewModel.CalculationResultSets);

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

                this.resetCalculation(true);

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
                this.mainViewModel.NetWorth = tempPerfomanceList[8];
                this.mainViewModel.PortfolioPerformancePercent = tempPerfomanceList[9];
                this.mainViewModel.SharpeRatio = tempPerfomanceList[10];

                List<bool> tempBoolList = (List<bool>)bFormatter.Deserialize(stream);
                this.mainViewModel.IsRealTimeEnabled = tempBoolList[0];
                this.mainViewModel.IsAlgorithmUsingMaps = tempBoolList[1];
                this.mainViewModel.IsDataFutures = tempBoolList[2];

                List<string> tempStringList = (List<string>)bFormatter.Deserialize(stream);
                this.mainViewModel.AlgorithmFileName = tempStringList[0];
                this.mainViewModel.DataFileName = tempStringList[1];
                this.mainViewModel.Capital = tempStringList[2];
                this.mainViewModel.AbsTransactionFee = tempStringList[3];
                this.mainViewModel.RelTransactionFee = tempStringList[4];
                this.mainViewModel.PricePremium = tempStringList[5];
                this.mainViewModel.StockSymbolForRealTime = tempStringList[6];
                this.mainViewModel.Barsize = tempStringList[7];
                this.mainViewModel.AdditionalParameters = tempStringList[8];

                List<DateTime> tempDateList = (List<DateTime>)bFormatter.Deserialize(stream);
                this.mainViewModel.StartDate = tempDateList[0];
                this.mainViewModel.EndDate = tempDateList[1];

                List<int> tempIntList = (List<int>)bFormatter.Deserialize(stream);
                this.mainViewModel.ValueOfSliderOne = tempIntList[0];
                this.mainViewModel.ValueOfSliderTwo = tempIntList[1];
                this.mainViewModel.ValueOfSliderThree = tempIntList[2];
                this.mainViewModel.ValueOfSliderFour = tempIntList[3];
                this.mainViewModel.ValueOfSliderFive = tempIntList[4];
                this.mainViewModel.ValueOfSliderSix = tempIntList[5];
                this.mainViewModel.RoundLotSize = tempIntList[6];
                this.mainViewModel.InnerValue = tempIntList[7];

                StringCollection serializableStackPanels = (StringCollection)bFormatter.Deserialize(stream);
                this.mainViewModel.IndicatorPanels = this.restoreIndicatorStackPanels(serializableStackPanels);
                this.refreshIndicatorList();

                this.mainViewModel.CalculationResultSets = (Dictionary<string, CalculationResultSet>)bFormatter.Deserialize(stream);

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
            else if (e.Source == this.ChartSettingsTabSelector)
                this.ChartSettingsTab.IsSelected = true;
        }

        private void RemoveIndicatorButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.mainViewModel.IndicatorPanels.Count; i++)
            {
                if (((System.Windows.Controls.Button)this.mainViewModel.IndicatorPanels[i].Children[0]).Equals((System.Windows.Controls.Button)sender))
                {
                    this.mainViewModel.IndicatorPanels.RemoveAt(i);
                }
            }
            this.refreshIndicatorList();
        }

        private void AddIndicatorButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = System.Windows.Controls.Orientation.Horizontal;

            System.Windows.Controls.Button remBut = new System.Windows.Controls.Button();
            remBut.Content = "Remove";
            remBut.Margin = new Thickness(5, 5, 5, 5);
            remBut.Click += RemoveIndicatorButton_Click;
            sp.Children.Add(remBut);

            System.Windows.Controls.Label label = new System.Windows.Controls.Label();
            label.Content = ((ComboBoxItem)this.IndicatorComboBox.SelectedValue).Content;
            label.Margin = new Thickness(0, 0, 20, 0);
            label.Width = 300;
            sp.Children.Add(label);

            switch (this.IndicatorComboBox.SelectedIndex)
            {
                case 0:
                case 1:
                case 2:
                    sp.Children.Add(this.AddAttrLabel("Length"));
                    sp.Children.Add(this.AddTextBox(String.Empty));
                    sp.Children.Add(this.AddColorPicker());
                    break;
                case 3:
                    sp.Children.Add(this.AddAttrLabel("Short"));
                    sp.Children.Add(this.AddTextBox("Works like this: ShortLength"));
                    sp.Children.Add(this.AddAttrLabel("Long"));
                    sp.Children.Add(this.AddTextBox("Works like this: LongLength"));
                    sp.Children.Add(this.AddColorPicker());
                    break;
            }

            this.mainViewModel.IndicatorPanels.Add(sp);
            this.refreshIndicatorList();
        }

        public System.Windows.Controls.Label AddAttrLabel(string content)
        {
            System.Windows.Controls.Label label = new System.Windows.Controls.Label();
            label.Content = content;
            label.Width = 70;
            return label;
        }

        public System.Windows.Controls.TextBox AddTextBox(string tooltip)
        {
            System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();
            tb.PreviewTextInput += NumericOnly;
            tb.Margin = new Thickness(5, 5, 5, 5);
            tb.Text = "0";
            if (tooltip.Length != 0)
                tb.ToolTip = tooltip;
            tb.Width = 50;
            return tb;
        }

        public ColorPicker AddColorPicker()
        {
            ColorPicker cp = new ColorPicker();
            cp.DisplayColorAndName = true;
            cp.Width = 200;
            cp.Margin = new Thickness(5, 5, 5, 5);
            return cp;
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new Regex("[^0-9]");
            e.Handled = reg.IsMatch(e.Text);
        }

        private void NumericOnly_WithDecimalPlace(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new Regex("[^0-9,]");
            e.Handled = reg.IsMatch(e.Text);
        }

        public void refreshIndicatorList()
        {
            this.IndicatorStackPanel.Children.Clear();
            foreach (StackPanel panel in this.mainViewModel.IndicatorPanels)
            {
                this.IndicatorStackPanel.Children.Add(panel);
            }
        }

        private void orderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.iscalculating)
            {
                Chart chart = this.FindName("MyWinformChart") as Chart;

                if (this.selectedArrowIndex > -1)
                {
                    setArrowColor((ArrowAnnotation)chart.Annotations["Arrow-" + this.selectedArrowIndex],
                                   this.mainViewModel.Orders[this.selectedArrowIndex].Trendstrength);

                    ((ArrowAnnotation)(chart.Annotations["Arrow-" + this.selectedArrowIndex])).Height /= 2;
                    ((ArrowAnnotation)(chart.Annotations["Arrow-" + this.selectedArrowIndex])).ArrowSize -= 2;
                }

                this.selectedArrowIndex = ((System.Windows.Controls.DataGrid)sender).SelectedIndex;

                chart.Annotations["Arrow-" + this.selectedArrowIndex].BackColor = System.Drawing.Color.FromArgb(0, 0, 0);
                ((ArrowAnnotation)(chart.Annotations["Arrow-" + this.selectedArrowIndex])).Height *= 2;
                ((ArrowAnnotation)(chart.Annotations["Arrow-" + this.selectedArrowIndex])).ArrowSize += 2;
            }
            else
            {
                this.selectedArrowIndex = -1;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ResultSelectionComboBox.SelectedItem != null)
            {
                if (((string)(this.ResultSelectionComboBox.SelectedItem)).Length != 0 &&
                    this.mainViewModel.CalculationResultSets.ContainsKey((string)this.ResultSelectionComboBox.SelectedItem))
                {
                    CalculationResultSet resultSet = this.mainViewModel.CalculationResultSets[(string)this.ResultSelectionComboBox.SelectedItem];

                    this.mainViewModel.NetWorth = resultSet.NetWorth;
                    this.mainViewModel.PortfolioPerformancePercent = resultSet.PortfolioPerformancePercent;
                    this.mainViewModel.SharpeRatio = resultSet.SharpeRatio;
                    this.mainViewModel.StdDevOfProfit = resultSet.StdDevOfProfit;
                    this.mainViewModel.StdDevOfPEquityPrice = resultSet.StdDevOfPEquityPrice;
                    this.mainViewModel.GainLossPercent = resultSet.GainLossPercent;
                    this.mainViewModel.NoOfGoodTrades = resultSet.NoOfGoodTrades;
                    this.mainViewModel.GainPercent = resultSet.GainPercent;
                    this.mainViewModel.NoOfBadTrades = resultSet.NoOfBadTrades;
                    this.mainViewModel.LossPercent = resultSet.LossPercent;
                    this.mainViewModel.GtBtRatio = resultSet.GtBtRatio;
                    this.mainViewModel.HighestDailyProfit = resultSet.HighestDailyProfit;
                    this.mainViewModel.HighestDailyLoss = resultSet.HighestDailyLoss;
                    this.mainViewModel.LastDayProfitLoss = resultSet.LastDayProfitLoss;
                }
            }
        }
    }
}
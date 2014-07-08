using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace TradingSoftware
{
    /// <summary>
    /// Interaction logic for WorkerTab.xaml
    /// </summary>
    public partial class WorkerTab : TabItem
    {
        private Worker worker;
        private MainWindow mainWindow;

        public WorkerTab(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
        }

        public void setUpTabWorkerConnection(Worker worker)
        {
            this.worker = worker;
            this.workerGrid.DataContext = new List<WorkerViewModel> { this.workerViewModel };
            this.workerGrid.Items.Refresh();

            this.WorkerTabItem.Name = this.workerViewModel.EquityAsString;
            this.WorkerTabItem.Header = this.workerViewModel.EquityAsString;
        }

        private void ScrollToEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConsoleBoxScrollViewer.ScrollToEnd();
            SignalBoxScrollViewer.ScrollToEnd();
        }

        private void StopThisWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.Stop();
        }

        private void StartThisWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.workerViewModel.IsTrading = true;
            if (!this.worker.workerViewModel.IsThreadRunning)
            {
                this.worker.Start();
            }
        }

        private void StopTradingThisWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.StopTrading();
        }

        private void StopThisWorkerAfterSignalButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.StopTradingAfterSignal();

            if (this.worker.doesStopTradingAfterSignal())
            {
                this.StopTradingAfterSignalButton.Content = "Don't stop trading after signal";
            }
            else
            {
                this.StopTradingAfterSignalButton.Content = "Stop trading after signal";
            }
        }

        private void RemoveWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.mainWindow.RemoveWorker(this.worker);
        }
        private void ChangeWorkerSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ShowSettingsWindow(this.workerViewModel);
        }
    }
}

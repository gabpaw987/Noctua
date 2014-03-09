using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media;

namespace BacktestingSoftware
{
    /// <summary>
    /// Interaktionslogik für ComparePerformancesWindow.xaml
    /// </summary>
    public partial class ComparePerformancesWindow : Window
    {
        private MainWindow mainWindow;

        private SortedList<string, decimal> performancesList;

        private int SelectedDataGridIndex;

        public ComparePerformancesWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.performancesList = new SortedList<string, decimal>();
            this.SelectedDataGridIndex = -1;

            this.calculationResults.DataContext = this.performancesList;
            this.calculationResults.RowBackground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(this.mainWindow.ChartBGColor.A, this.mainWindow.ChartBGColor.R,
                                                                                                this.mainWindow.ChartBGColor.G, this.mainWindow.ChartBGColor.B));

            this.PrepareDictionary();
            this.SetUpChart();
        }

        private void PrepareDictionary()
        {
            if (this.mainWindow.mainViewModel.CalculationResultSets.Count > 0)
            {
                foreach (KeyValuePair<string, CalculationResultSet> entry in this.mainWindow.mainViewModel.CalculationResultSets)
                {
                    this.performancesList.Add(entry.Key, Math.Round(entry.Value.PortfolioPerformancePercent, 3));
                }
            }
        }

        private void SetUpChart()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                Chart chart = this.FindName("ComparePerformancesChart") as Chart;

                Series series = new Series("Performances");
                chart.Series.Add(series);

                chart.Series["Performances"].Color = System.Drawing.Color.Black;

                for (int i = 0; i < chart.Series.Count; i++)
                {
                    // Set series chart type
                    chart.Series[i].ChartType = SeriesChartType.Line;

                    chart.Series[i].XValueMember = "Index";
                    chart.Series[i].XValueType = ChartValueType.Int32;
                    chart.Series[i].YValueMembers = "Portfolio Performance";

                    // Set point width
                    chart.Series[i]["PointWidth"] = "0.5";
                }

                this.FormatChart(chart);

                //Calculate Minimum and Maximum values for Performances
                decimal min = this.performancesList.Values.Min();
                decimal max = this.performancesList.Values.Max();

                decimal margin = (max - min) * 5 / 100;
                chart.ChartAreas[0].AxisY.Minimum = Math.Round(Convert.ToDouble(min - margin), 2);
                chart.ChartAreas[0].AxisY.Maximum = Math.Round(Convert.ToDouble(max + margin), 2);

                chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Chart_MouseClick);

                int j = 0;
                foreach (KeyValuePair<string, decimal> entry in this.performancesList)
                {
                    chart.Series["Performances"].Points.AddXY(j, Convert.ToDouble(entry.Value));
                    chart.Series["Performances"].Points[j].MarkerSize = 8;
                    chart.Series["Performances"].Points[j].MarkerStyle = MarkerStyle.Circle;
                    chart.Series["Performances"].Points[j].ToolTip = entry.Key + ": " + entry.Value;
                    j++;
                }

                chart.DataBind();

                // draw!
                chart.Invalidate();
            }));
        }

        public void FormatChart(Chart chart)
        {
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

            chart.ChartAreas[0].AxisX.LabelStyle.Format = " ";

            //Color of the other Background System.Drawing.Color.FromArgb(213, 216, 221)
            System.Drawing.Color backgroundColor = this.mainWindow.ChartBGColor;

            chart.BackColor = backgroundColor;
            foreach (ChartArea area in chart.ChartAreas)
            {
                area.BackColor = backgroundColor;

                area.AxisX.MajorGrid.LineColor = System.Drawing.Color.LightGray;
                area.AxisY.MajorGrid.LineColor = System.Drawing.Color.LightGray;
            }
        }

        private void orderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Chart chart = this.FindName("ComparePerformancesChart") as Chart;

            if (this.SelectedDataGridIndex > -1)
            {
                chart.Series["Performances"].Points[this.SelectedDataGridIndex].Color = System.Drawing.Color.Black;
            }

            this.SelectedDataGridIndex = ((System.Windows.Controls.DataGrid)sender).SelectedIndex;

            chart.Series["Performances"].Points[this.SelectedDataGridIndex].Color = System.Drawing.Color.Red;
        }

        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            Chart chart = sender as Chart;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var pos = e.Location;
                var results = chart.HitTest(pos.X, pos.Y, false,
                                             ChartElementType.DataPoint);
                foreach (var result in results)
                {
                    if (result.Object != null)
                    {
                        if (result.Object is DataPoint)
                        {
                            int selectedDataGridIndex = (int)((DataPoint)result.Object).XValue;
                            this.calculationResults.SelectedIndex = selectedDataGridIndex;
                            this.calculationResults.ScrollIntoView(this.calculationResults.Items[selectedDataGridIndex]);
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }
    }
}
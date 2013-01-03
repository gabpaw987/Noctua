using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BacktestingSoftware
{
    internal class Calculator
    {
        private MainViewModel mainViewModel { get; set; }

        public List<Tuple<DateTime, decimal, decimal, decimal, decimal>> barList { get; private set; }

        public List<int> signals { get; private set; }

        public Calculator(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.signals = new List<int>();
        }

        public void Start()
        {
            this.barList = CSVReader.EnumerateExcelFile(this.mainViewModel.DataFileName, this.mainViewModel.StartDate, this.mainViewModel.EndDate).ToList();

            Assembly assembly = Assembly.LoadFrom(this.mainViewModel.AlgorithmFileName);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            Type t = assembly.GetType("Algorithm.DecisionCalculator");

            Object[] oa = { 90, barList, this.signals };

            t.GetMethod("startCalculation").Invoke(null, oa);

            this.CalculateNumbers();
        }

        private void CalculateNumbers()
        {
            if (barList.Count == signals.Count)
            {
                List<double> profitsForStdDev = new List<double>();
                List<double> EquityPricesForStdDev = new List<double>();
                this.mainViewModel.GainLossPercent = 0;
                this.mainViewModel.GainPercent = 0;
                this.mainViewModel.LossPercent = 0;
                decimal valueOfLastTrade = 0m;
                for (int i = 1; i < barList.Count; i++)
                {
                    if (signals[i - 1] != signals[i] && signals[i] != 0 && signals[i - 1] != 0)
                    {
                        if (valueOfLastTrade == 0m)
                        {
                            //TODO:Weighting
                            valueOfLastTrade = barList[i].Item5 * Math.Abs(signals[i]);
                        }
                        else
                        {
                            //TODO: Weighting
                            decimal valueOfThisTrade = barList[i].Item5 * Math.Abs(signals[i]);
                            decimal percentageOfThisTrade = 0;
                            if (signals[i] > 0)
                                percentageOfThisTrade = ((valueOfLastTrade - valueOfThisTrade) / valueOfThisTrade) * 100;
                            else if (signals[i] < 0)
                                percentageOfThisTrade = ((valueOfThisTrade - valueOfLastTrade) / valueOfLastTrade) * 100;

                            this.mainViewModel.GainLossPercent += percentageOfThisTrade;
                            profitsForStdDev.Add((double)percentageOfThisTrade);

                            if (percentageOfThisTrade > 0)
                            {
                                this.mainViewModel.NoOfGoodTrades++;
                                this.mainViewModel.GainPercent += percentageOfThisTrade;
                            }
                            else if (percentageOfThisTrade < 0)
                            {
                                this.mainViewModel.NoOfBadTrades++;
                                this.mainViewModel.LossPercent += percentageOfThisTrade;
                            }

                            this.mainViewModel.Orders.Add(new Order(barList[i].Item1, signals[i], 0, barList[i].Item5, percentageOfThisTrade, this.mainViewModel.GainLossPercent));

                            valueOfLastTrade = valueOfThisTrade;
                        }
                    }
                    EquityPricesForStdDev.Add((double)barList[i].Item5);
                }
                this.mainViewModel.GtBtRatio = this.mainViewModel.NoOfGoodTrades / this.mainViewModel.NoOfBadTrades;
                this.mainViewModel.StdDevOfProfit = (decimal)this.CalculateStdDevs(profitsForStdDev);
                this.mainViewModel.StdDevOfPEquityPrice = (decimal)this.CalculateStdDevs(EquityPricesForStdDev);
            }
        }

        private double CalculateStdDevs(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
    }
}
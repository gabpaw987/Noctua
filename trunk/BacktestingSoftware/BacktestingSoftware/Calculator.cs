using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BacktestingSoftware
{
    internal class Calculator
    {
        private MainViewModel mainViewModel { get; set; }

        public Calculator(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.mainViewModel.Signals = new List<int>();
        }

        public void Start()
        {
            this.ReadFile();
            this.CalculateSignals();
            this.CalculateNumbers();
        }

        public void ReadFile()
        {
            this.mainViewModel.BarList = CSVReader.EnumerateExcelFile(this.mainViewModel.DataFileName, this.mainViewModel.StartDate, this.mainViewModel.EndDate).ToList();
        }

        public void CalculateSignals()
        {
            Assembly assembly = Assembly.LoadFrom(this.mainViewModel.AlgorithmFileName);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            Type t = assembly.GetType("Algorithm.DecisionCalculator");

            Object[] oa = { 90, this.mainViewModel.BarList, this.mainViewModel.Signals };
            t.GetMethod("startCalculation").Invoke(null, oa);
        }

        public void CalculateNumbers()
        {
            if (this.mainViewModel.BarList.Count == this.mainViewModel.Signals.Count)
            {
                List<double> profitsForStdDev = new List<double>();
                List<double> EquityPricesForStdDev = new List<double>();
                this.mainViewModel.GainLossPercent = 0;
                this.mainViewModel.GainPercent = 0;
                this.mainViewModel.LossPercent = 0;
                decimal valueOfLastTrade = 0m;
                for (int i = 1; i < this.mainViewModel.BarList.Count; i++)
                {
                    if (this.mainViewModel.Signals[i - 1] != this.mainViewModel.Signals[i] && this.mainViewModel.Signals[i] != 0 && this.mainViewModel.Signals[i - 1] != 0)
                    {
                        if (valueOfLastTrade == 0m)
                        {
                            //TODO:Weighting
                            valueOfLastTrade = this.mainViewModel.BarList[i].Item5 * Math.Abs(this.mainViewModel.Signals[i]);
                        }
                        else
                        {
                            //TODO: Weighting
                            decimal valueOfThisTrade = this.mainViewModel.BarList[i].Item5 * Math.Abs(this.mainViewModel.Signals[i]);
                            decimal percentageOfThisTrade = 0;
                            if (this.mainViewModel.Signals[i] > 0)
                                percentageOfThisTrade = ((valueOfLastTrade - valueOfThisTrade) / valueOfThisTrade) * 100;
                            else if (this.mainViewModel.Signals[i] < 0)
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

                            this.mainViewModel.Orders.Add(new Order(this.mainViewModel.BarList[i].Item1, this.mainViewModel.Signals[i], 0, this.mainViewModel.BarList[i].Item5, percentageOfThisTrade, this.mainViewModel.GainLossPercent));

                            valueOfLastTrade = valueOfThisTrade;
                        }
                    }
                    EquityPricesForStdDev.Add((double)this.mainViewModel.BarList[i].Item5);
                }

                if (this.mainViewModel.NoOfBadTrades > 0)
                    this.mainViewModel.GtBtRatio = this.mainViewModel.NoOfGoodTrades / this.mainViewModel.NoOfBadTrades;
                else
                    this.mainViewModel.GtBtRatio = 0;

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
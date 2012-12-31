using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BacktestingSoftware
{
    internal class Calculator
    {
        private MainViewModel mainViewModel { get; set; }

        public List<Tuple<DateTime, double, double, double, double>> barList { get; private set; }

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

            this.calculateNumbers();
        }

        private void calculateNumbers()
        {
            if (barList.Count == signals.Count)
            {
                this.mainViewModel.GainLossPercent = 0;
                double valueOfLastTrade = 0.0;
                for (int i = 1; i < barList.Count; i++)
                {
                    if (signals[i - 1] != signals[i] && signals[i] != 0 && signals[i - 1] != 0)
                    {
                        if (valueOfLastTrade == 0.0)
                        {
                            //TODO:Weighting
                            valueOfLastTrade = barList[i].Item5 * Math.Abs(signals[i]);
                        }
                        else
                        {
                            //TODO: Weighting
                            double valueOfThisTrade = barList[i].Item5 * Math.Abs(signals[i]);
                            double percentageOfThisTrade = 0;
                            if (signals[i] > 0)
                                percentageOfThisTrade = ((valueOfLastTrade - valueOfThisTrade) / valueOfThisTrade) * 100;
                            else if (signals[i] < 0)
                                percentageOfThisTrade = ((valueOfThisTrade - valueOfLastTrade) / valueOfLastTrade) * 100;

                            this.mainViewModel.GainLossPercent += percentageOfThisTrade;

                            if (percentageOfThisTrade > 0)
                                this.mainViewModel.NoOfGoodTrades++;
                            else if (percentageOfThisTrade < 0)
                                this.mainViewModel.NoOfBadTrades++;

                            this.mainViewModel.Orders.Add(new Order(barList[i].Item1, signals[i], 0, barList[i].Item5, percentageOfThisTrade, this.mainViewModel.GainLossPercent));

                            valueOfLastTrade = valueOfThisTrade;
                        }
                    }
                }
                this.mainViewModel.GtBtRatio = this.mainViewModel.NoOfGoodTrades / this.mainViewModel.NoOfBadTrades;
            }
        }
    }
}
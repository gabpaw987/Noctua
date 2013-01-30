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
                decimal priceOfLastTrade = 0m;
                decimal absCumGainLoss = 0m;
                this.mainViewModel.NetWorth = decimal.Parse(this.mainViewModel.Capital);

                int RoundLotSize = 0;
                switch (this.mainViewModel.RoundLotSize)
                {
                    case 0:
                        RoundLotSize = 10;
                        break;
                    case 1:
                        RoundLotSize = 50;
                        break;
                    case 2:
                        RoundLotSize = 100;
                        break;
                }

                for (int i = 1; i < this.mainViewModel.BarList.Count; i++)
                {
                    if (this.mainViewModel.Signals[i - 1] != this.mainViewModel.Signals[i])
                    {
                        if (priceOfLastTrade == 0m)
                        {
                            priceOfLastTrade = this.mainViewModel.BarList[i].Item5;

                            this.mainViewModel.Orders.Add(new Order(this.mainViewModel.BarList[i].Item1, this.mainViewModel.Signals[i], this.getWeightingMultiplier(i), this.mainViewModel.BarList[i].Item5, this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.getWeightingMultiplier(i), 0, 0, 0, 0, 0, absCumGainLoss, this.mainViewModel.NetWorth));
                        }
                        else
                        {
                            int oldWeightingMultiplier = this.getWeightingMultiplier(i - 1);
                            int WeightingMultiplier = this.getWeightingMultiplier(i);

                            decimal priceOfThisTrade = this.mainViewModel.BarList[i].Item5;

                            decimal currentGainLoss = 0;
                            currentGainLoss = (priceOfThisTrade - priceOfLastTrade) * oldWeightingMultiplier * RoundLotSize;

                            decimal portfolioPerformance = 0;
                            portfolioPerformance = currentGainLoss / this.mainViewModel.NetWorth * 100;

                            this.mainViewModel.PortfolioPerformancePercent += (currentGainLoss / decimal.Parse(this.mainViewModel.Capital) * 100);

                            decimal percentageOfThisTrade = 0;
                            if ((oldWeightingMultiplier == 0) ||
                                (Math.Sign(oldWeightingMultiplier) == Math.Sign(WeightingMultiplier) &&
                                (WeightingMultiplier - oldWeightingMultiplier) == Math.Sign(WeightingMultiplier)))
                            {
                                percentageOfThisTrade = 0;
                            }
                            else if (oldWeightingMultiplier != 0)
                            {
                                percentageOfThisTrade = ((oldWeightingMultiplier * (priceOfThisTrade - priceOfLastTrade)) / priceOfLastTrade) * 100;
                            }

                            this.mainViewModel.GainLossPercent += percentageOfThisTrade;
                            profitsForStdDev.Add((double)portfolioPerformance);

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

                            absCumGainLoss += currentGainLoss;
                            this.mainViewModel.NetWorth += currentGainLoss;

                            this.mainViewModel.Orders.Add(new Order(this.mainViewModel.BarList[i].Item1, this.mainViewModel.Signals[i], this.getWeightingMultiplier(i), this.mainViewModel.BarList[i].Item5, this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.getWeightingMultiplier(i), percentageOfThisTrade, this.mainViewModel.GainLossPercent, portfolioPerformance, this.mainViewModel.PortfolioPerformancePercent, currentGainLoss, absCumGainLoss, this.mainViewModel.NetWorth));

                            priceOfLastTrade = priceOfThisTrade;
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

        private int getWeightingMultiplier(int index)
        {
            switch (this.mainViewModel.Signals[index])
            {
                case -3:
                    return this.mainViewModel.ValueOfSliderOne * -1;
                case -2:
                    return this.mainViewModel.ValueOfSliderTwo * -1;
                case -1:
                    return this.mainViewModel.ValueOfSliderThree * -1;
                case 1:
                    return this.mainViewModel.ValueOfSliderFour;
                case 2:
                    return this.mainViewModel.ValueOfSliderFive;
                case 3:
                    return this.mainViewModel.ValueOfSliderSix;
            }
            return 0;
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
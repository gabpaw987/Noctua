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
            Assembly assembly = Assembly.LoadFile(this.mainViewModel.AlgorithmFileName);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            Type t = assembly.GetType("Algorithm.DecisionCalculator");

            Object[] oa = { this.mainViewModel.BarList, this.mainViewModel.Signals };
            t.GetMethod("startCalculation").Invoke(null, oa);
        }

        public String CalculateNumbers()
        {
            if (this.mainViewModel.BarList.Count == this.mainViewModel.Signals.Count)
            {
                if (this.mainViewModel.Signals.Count<int>(n => n == 0) != this.mainViewModel.Signals.Count)
                {
                    List<double> profitsForStdDev = new List<double>();
                    List<double> EquityPricesForStdDev = new List<double>();
                    this.mainViewModel.GainLossPercent = 0;
                    this.mainViewModel.GainPercent = 0;
                    this.mainViewModel.LossPercent = 0;
                    decimal priceOfLastTrade = 0m;
                    decimal absCumGainLoss = 0m;
                    this.mainViewModel.NetWorth = decimal.Parse(this.mainViewModel.Capital);

                    if (this.mainViewModel.NetWorth == 0)
                    {
                        return "Capital is 0!";
                    }

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

                    //To remove all the remaining positions in the end
                    this.mainViewModel.Signals[this.mainViewModel.Signals.Count - 1] = 0;

                    for (int i = 1; i < this.mainViewModel.BarList.Count; i++)
                    {
                        if (this.mainViewModel.Signals[i - 1] != this.mainViewModel.Signals[i] && this.mainViewModel.BarList[i].Item2 != 0
                            && this.mainViewModel.BarList[i].Item3 != 0 && this.mainViewModel.BarList[i].Item4 != 0 && this.mainViewModel.BarList[i].Item5 != 0)
                        {
                            decimal addableFee = 0;
                            if (this.mainViewModel.Signals[i] != 0 || (this.mainViewModel.Signals[i - 1] != 0 && this.mainViewModel.Signals[i] == 0))
                                addableFee = Math.Abs(decimal.Parse(this.mainViewModel.RelTransactionFee) / 100 * (this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.GetAbsWeightedSignalDifference(i))) + decimal.Parse(this.mainViewModel.AbsTransactionFee);

                            int oldWeightingMultiplier = this.GetWeightingMultiplier(i - 1);
                            int WeightingMultiplier = this.GetWeightingMultiplier(i);

                            decimal priceOfThisTrade = this.mainViewModel.BarList[i].Item5;

                            //if buy
                            if (WeightingMultiplier > oldWeightingMultiplier)
                            {
                                priceOfThisTrade += decimal.Parse(this.mainViewModel.PricePremium);
                            }
                            //else if sell
                            else if (WeightingMultiplier < oldWeightingMultiplier)
                            {
                                priceOfThisTrade -= decimal.Parse(this.mainViewModel.PricePremium);
                            }

                            decimal currentGainLoss = 0;
                            currentGainLoss = (priceOfThisTrade - priceOfLastTrade) * oldWeightingMultiplier * RoundLotSize;
                            currentGainLoss -= addableFee;

                            decimal portfolioPerformance = 0;
                            if (this.mainViewModel.NetWorth == 0)
                            {
                                portfolioPerformance = decimal.Zero;
                            }
                            else
                            {
                                portfolioPerformance = currentGainLoss / this.mainViewModel.NetWorth * 100;
                            }

                            //This is the portfolioperformance relative to the whole capital
                            //not the net worth; only for cumulative and good trades and bad trades
                            decimal partialPortfolioPerformancePercent = (currentGainLoss / decimal.Parse(this.mainViewModel.Capital) * 100);
                            this.mainViewModel.PortfolioPerformancePercent += partialPortfolioPerformancePercent;

                            // strengthening the signal or first trade
                            decimal percentageOfThisTrade = 0;
                            if ((oldWeightingMultiplier == 0) ||
                                (Math.Sign(oldWeightingMultiplier) == Math.Sign(WeightingMultiplier) &&
                                (WeightingMultiplier - oldWeightingMultiplier) == Math.Sign(WeightingMultiplier)))
                            {
                                percentageOfThisTrade = (-addableFee / (this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.GetAbsWeightedSignalDifference(i))) * 100;
                            }
                            // if not strengthening the signal
                            else if (oldWeightingMultiplier != 0)
                            {
                                if (priceOfLastTrade == 0)
                                {
                                    percentageOfThisTrade = (-addableFee / (this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.GetAbsWeightedSignalDifference(i))) * 100;
                                }
                                else
                                {
                                    //This is to be able to only show realised profit
                                    if (Math.Sign(oldWeightingMultiplier) == Math.Sign(WeightingMultiplier))
                                    {
                                        percentageOfThisTrade = (((oldWeightingMultiplier * (priceOfThisTrade - priceOfLastTrade)) / priceOfLastTrade) * 100) - (addableFee / (this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.GetAbsWeightedSignalDifference(i)));
                                    }
                                    else
                                    {
                                        percentageOfThisTrade = (((this.GetAbsWeightedSignalDifference(i) * (priceOfThisTrade - priceOfLastTrade)) / priceOfLastTrade) * 100) - (addableFee / (this.mainViewModel.BarList[i].Item5 * RoundLotSize * this.GetAbsWeightedSignalDifference(i)));
                                    }
                                }
                            }

                            this.mainViewModel.GainLossPercent += percentageOfThisTrade;
                            profitsForStdDev.Add((double)portfolioPerformance);

                            absCumGainLoss += currentGainLoss;
                            this.mainViewModel.NetWorth += currentGainLoss;

                            decimal transactionPriceToDisplay = (this.mainViewModel.BarList[i].Item5 * RoundLotSize * (this.GetAbsWeightedSignalDifference(i) * WeightingMultiplier > oldWeightingMultiplier ? 1 : -1)) + addableFee;
                            this.mainViewModel.Orders.Add(new Order(this.mainViewModel.BarList[i].Item1, this.mainViewModel.Signals[i], this.GetWeightingMultiplier(i), priceOfThisTrade, transactionPriceToDisplay, addableFee, percentageOfThisTrade, this.mainViewModel.GainLossPercent, portfolioPerformance, this.mainViewModel.PortfolioPerformancePercent, currentGainLoss, absCumGainLoss, this.mainViewModel.NetWorth));

                            priceOfLastTrade = priceOfThisTrade;
                        }
                        EquityPricesForStdDev.Add((double)this.mainViewModel.BarList[i].Item5);
                    }

                    //Calculation of GainPrecent and LossPercent, without losing unrealised profit.
                    //This has to be done after the rest of the calculation in order to be able to catch the
                    //unrealised profit too.
                    for (int i = 1; i < this.mainViewModel.Orders.Count; i++)
                    {
                        Order order = this.mainViewModel.Orders[i];

                        decimal ZeroWithFeePaid = Math.Round((-order.PaidFee / (order.Price * RoundLotSize * this.GetAbsWeightedSignalDifferenceForOrders(i))) * 100, 3);

                        Console.WriteLine(ZeroWithFeePaid + "   " + order.GainLossPercent);

                        //Get all the normal profit and dont take fee into account for good trades or bad trades
                        if (order.GainLossPercent > ZeroWithFeePaid)
                        {
                            this.mainViewModel.NoOfGoodTrades++;
                            this.mainViewModel.GainPercent += (order.CumulativePortfolioPerformance -
                                                               this.mainViewModel.Orders[i - 1].CumulativePortfolioPerformance);
                        }
                        else if (order.GainLossPercent < ZeroWithFeePaid)
                        {
                            this.mainViewModel.NoOfBadTrades++;
                            this.mainViewModel.LossPercent += (order.CumulativePortfolioPerformance -
                                                               this.mainViewModel.Orders[i - 1].CumulativePortfolioPerformance);
                        }
                        else if (order.GainLossPercent == ZeroWithFeePaid)
                        {
                            decimal portfolioPerformanceRelativeToCapital = (order.CumulativePortfolioPerformance -
                                                                             this.mainViewModel.Orders[i - 1].CumulativePortfolioPerformance);
                            //Get all the unrealised profit
                            if (portfolioPerformanceRelativeToCapital > ZeroWithFeePaid)
                            {
                                this.mainViewModel.NoOfGoodTrades++;
                                this.mainViewModel.GainPercent += portfolioPerformanceRelativeToCapital;
                            }
                            else if (portfolioPerformanceRelativeToCapital < ZeroWithFeePaid)
                            {
                                this.mainViewModel.NoOfBadTrades++;
                                this.mainViewModel.LossPercent += portfolioPerformanceRelativeToCapital;
                            }
                            //if 0 do nothing, becuase there is not even unrealised profit
                        }
                    }

                    if (this.mainViewModel.NoOfBadTrades > 0)
                        this.mainViewModel.GtBtRatio = this.mainViewModel.NoOfGoodTrades / this.mainViewModel.NoOfBadTrades;
                    else
                        this.mainViewModel.GtBtRatio = 0;

                    this.mainViewModel.StdDevOfProfit = (decimal)this.CalculateStdDevs(profitsForStdDev);
                    this.mainViewModel.StdDevOfPEquityPrice = (decimal)this.CalculateStdDevs(EquityPricesForStdDev);

                    //assume 365.25 days per year and get the length of the historical data in years
                    decimal years = ((this.mainViewModel.BarList.Last().Item1 - this.mainViewModel.BarList[0].Item1).Days) / 365.25m;

                    //portfolioperformance without the risk free rate you would have earned via a formula from http://www.frustfrei-lernen.de/mathematik/zinseszins.html
                    double interestRate = 1.5;
                    decimal netWorthWithRiskFreeRate = Convert.ToDecimal(Convert.ToDouble(this.mainViewModel.Capital) * Math.Pow(
                                    (1 + (interestRate / 100)),
                                    Convert.ToDouble(years)));
                    decimal portfolioPerformanceWithRiskFreeRate = (netWorthWithRiskFreeRate - Convert.ToDecimal(this.mainViewModel.Capital)) / Convert.ToDecimal(this.mainViewModel.Capital) * 100;

                    this.mainViewModel.SharpeRatio = (this.mainViewModel.PortfolioPerformancePercent - portfolioPerformanceWithRiskFreeRate) / this.mainViewModel.StdDevOfProfit;

                    return string.Empty;
                }
                else
                {
                    return "Algorithm File returning only 0s!";
                }
            }
            else
            {
                return "Algorithm File is wrong!";
            }
        }

        private int GetAbsWeightedSignalDifferenceForOrders(int index)
        {
            for (int i = 0; i < this.mainViewModel.BarList.Count; i++)
            {
                if (this.mainViewModel.Orders[index].Timestamp.Equals(this.mainViewModel.BarList[i].Item1))
                {
                    return this.GetAbsWeightedSignalDifference(i);
                }
            }
            return 0;
        }

        private int GetAbsWeightedSignalDifference(int index)
        {
            int oldSignal = this.mainViewModel.Signals[index - 1];
            int newSignal = this.mainViewModel.Signals[index];

            int toZero = 0;
            int fromZero = 0;

            if ((newSignal > 0 && oldSignal < 0) ||
                (newSignal < 0 && oldSignal > 0))
            {
                toZero = 0 - oldSignal;
                fromZero = newSignal;
            }
            else if (newSignal > oldSignal)
            {
                //kaufen newSignal - oldSignal
                toZero = newSignal - oldSignal;
            }
            else if (newSignal < oldSignal)
            {
                //verkaufen newSignal - oldSignal
                toZero = -(newSignal - oldSignal);
            }
            else if (newSignal == 0)
            {
                toZero = 0 - oldSignal;
            }

            return (Math.Abs(GetWeightingMultiplierFromSignal(toZero)) + Math.Abs(GetWeightingMultiplierFromSignal(fromZero)));
        }

        private int GetWeightingMultiplier(int index)
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

        private int GetWeightingMultiplierFromSignal(int signal)
        {
            switch (signal)
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
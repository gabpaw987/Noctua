using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (this.mainViewModel.AdditionalParameters.Length == 0)
            {
                Type t = this.LoadAlgorithmFile();
                this.CalculateSignals(t, null);
                this.CalculateNumbers(string.Empty);
            }
            else
            {
                try
                {
                    Dictionary<string, List<decimal>> parameters = new Dictionary<string, List<decimal>>();
                    //split AdditionalParameters string
                    string[] separatedAdditionalParameters = this.mainViewModel.AdditionalParameters.Split('\n');
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
                    mesh(0, parameterRanges, valueSets);

                    Type t = this.LoadAlgorithmFile();

                    for (int i = 0; i < valueSets[0].Count; i++)
                    {
                        string description = string.Empty;

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

                        this.CalculateSignals(t, valueSet);
                        this.CalculateNumbers(description);
                    }
                }
                catch (Exception)
                {
                    //if this method was used array access exceptions would have to be handled
                }
            }
        }

        // when mesh is called from  outside, listNo always has to start with 0
        // the list valueSets has to be initialised with as many empty lists as there are lists in the parameterRanges
        // when called from outside
        public static void mesh(int listNo, List<List<decimal>> parameterRanges, List<List<decimal>> valueSets)
        {
            for (int i = 0; i < parameterRanges[listNo].Count; i++)
            {
                if (listNo == 0)
                {
                    valueSets[listNo].Add(parameterRanges[listNo][i]);
                    mesh(listNo + 1, parameterRanges, valueSets);
                }
                else
                {
                    valueSets[listNo].Add(parameterRanges[listNo][i]);

                    for (int j = 0; j < listNo && i != 0; j++)
                    {
                        valueSets[j].Add(valueSets[j].Last());
                    }

                    if (listNo != parameterRanges.Count - 1)
                    {
                        mesh(listNo + 1, parameterRanges, valueSets);
                    }
                }
            }
        }

        public void ReadFile()
        {
            this.mainViewModel.BarList = CSVReader.EnumerateExcelFile(this.mainViewModel.DataFileName,
                                                                      this.mainViewModel.StartDate,
                                                                      this.mainViewModel.EndDate,
                                                                      this.mainViewModel.IsDataFutures,
                                                                      this.switchRoundLotSizeOrInnerValue(this.mainViewModel.InnerValue)).ToList();
        }

        public Type LoadAlgorithmFile()
        {
            Assembly assembly = Assembly.LoadFile(this.mainViewModel.AlgorithmFileName);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            return assembly.GetType("Algorithm.DecisionCalculator");
        }

        public void CalculateSignals(Type t, Dictionary<string, decimal> parametersValueSet)
        {
            if (this.mainViewModel.IsAlgorithmUsingMaps && parametersValueSet != null)
            {
                Object[] oa = { this.mainViewModel.BarList, this.mainViewModel.Signals, this.mainViewModel.IndicatorDictionary, this.mainViewModel.OscillatorDictionary, parametersValueSet };
                t.GetMethod("startCalculation").Invoke(null, oa);
            }
            else if (parametersValueSet != null && !this.mainViewModel.IsAlgorithmUsingMaps)
            {
                Object[] oa = { this.mainViewModel.BarList, this.mainViewModel.Signals, parametersValueSet };
                t.GetMethod("startCalculation").Invoke(null, oa);
            }
            else if (this.mainViewModel.IsAlgorithmUsingMaps && parametersValueSet == null)
            {
                Object[] oa = { this.mainViewModel.BarList, this.mainViewModel.Signals, this.mainViewModel.IndicatorDictionary, this.mainViewModel.OscillatorDictionary };
                t.GetMethod("startCalculation").Invoke(null, oa);
            }
            else
            {
                Object[] oa = { this.mainViewModel.BarList, this.mainViewModel.Signals };
                t.GetMethod("startCalculation").Invoke(null, oa);
            }
        }

        public String CalculateNumbers(string parametersUsed)
        {
            if (this.mainViewModel.BarList.Count == this.mainViewModel.Signals.Count)
            {
                if (this.mainViewModel.Signals.Count<int>(n => n == 0) != this.mainViewModel.Signals.Count)
                {
                    CalculationResultSet resultSet = new CalculationResultSet();

                    List<double> profitsForStdDev = new List<double>();
                    List<double> EquityPricesForStdDev = new List<double>();
                    resultSet.GainLossPercent = 0;
                    resultSet.GainPercent = 0;
                    resultSet.LossPercent = 0;
                    decimal priceOfLastTrade = 0m;
                    decimal absCumGainLoss = 0m;
                    List<decimal> dailyPortfolioPerformances = new List<decimal>();
                    //For calculation of daily Portfolio Performances
                    DateTime currentDay = new DateTime();
                    decimal currentDayPortfolioPerformance = 0m;

                    resultSet.NetWorth = decimal.Parse(this.mainViewModel.Capital);

                    if (resultSet.NetWorth == 0)
                    {
                        return "Capital is 0!";
                    }

                    int RoundLotSize = this.switchRoundLotSizeOrInnerValue(this.mainViewModel.RoundLotSize);
                    int InnerValue = this.switchRoundLotSizeOrInnerValue(this.mainViewModel.InnerValue);

                    //To remove all the remaining positions in the end
                    this.mainViewModel.Signals[this.mainViewModel.Signals.Count - 1] = 0;

                    decimal paidForGainLoss = 0m;

                    for (int i = 1; i < this.mainViewModel.BarList.Count; i++)
                    {
                        if (this.mainViewModel.Signals[i - 1] != this.mainViewModel.Signals[i] && this.mainViewModel.BarList[i].Item2 != 0
                            && this.mainViewModel.BarList[i].Item3 != 0 && this.mainViewModel.BarList[i].Item4 != 0 && this.mainViewModel.BarList[i].Item5 != 0)
                        {
                            decimal addableFee = 0;
                            if (this.mainViewModel.Signals[i] != 0 || (this.mainViewModel.Signals[i - 1] != 0 && this.mainViewModel.Signals[i] == 0))
                                addableFee = Math.Abs(decimal.Parse(this.mainViewModel.RelTransactionFee) / 100 *
                                    (this.mainViewModel.BarList[i].Item5 * InnerValue / this.mainViewModel.MiniContractDenominator * RoundLotSize * this.GetAbsWeightedSignalDifference(i, true))) +
                                    decimal.Parse(this.mainViewModel.AbsTransactionFee);

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

                            //Calculation of absolute portfolio performance
                            decimal currentGainLoss = 0;
                            currentGainLoss = (priceOfThisTrade - priceOfLastTrade) * oldWeightingMultiplier * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator;
                            currentGainLoss -= addableFee;

                            //Calculation of portfolio Performance
                            decimal portfolioPerformance = 0;
                            if (resultSet.NetWorth == 0)
                            {
                                portfolioPerformance = decimal.Zero;
                            }
                            else
                            {
                                portfolioPerformance = currentGainLoss / resultSet.NetWorth * 100;
                            }

                            //This is the portfolioperformance relative to the whole capital
                            //not the net worth; only for cumulative and good trades and bad trades
                            decimal partialPortfolioPerformancePercent = (currentGainLoss / decimal.Parse(this.mainViewModel.Capital) * 100);
                            resultSet.PortfolioPerformancePercent += partialPortfolioPerformancePercent;

                            //if strengthening the signal or first trade
                            decimal percentageOfThisTrade = 0;
                            if ((oldWeightingMultiplier == 0) ||
                                (Math.Sign(oldWeightingMultiplier) == Math.Sign(WeightingMultiplier) &&
                                (Math.Abs(WeightingMultiplier) - Math.Abs(oldWeightingMultiplier)) > 0))
                            {
                                percentageOfThisTrade = (-addableFee / (priceOfThisTrade * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator *
                                                                        this.GetAbsWeightedSignalDifference(i, true))) * 100;
                                paidForGainLoss += (priceOfThisTrade * this.GetAbsWeightedSignalDifference(i, true));
                            }
                            // if not strengthening the signal
                            else if (oldWeightingMultiplier != 0)
                            {
                                //if there was no last trade... should not happen
                                if (priceOfLastTrade == 0)
                                {
                                    percentageOfThisTrade = (-addableFee / (priceOfThisTrade * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator *
                                                                            this.GetAbsWeightedSignalDifference(i, true))) * 100;
                                    paidForGainLoss += (priceOfThisTrade * this.GetAbsWeightedSignalDifference(i, true));
                                }
                                else
                                {
                                    //This is needed to show only realised profit
                                    if (Math.Sign(oldWeightingMultiplier) == Math.Sign(WeightingMultiplier))
                                    {
                                        decimal partOfPaidForGainLoss = (paidForGainLoss / Math.Abs(oldWeightingMultiplier)) * this.GetAbsWeightedSignalDifference(i, true);
                                        percentageOfThisTrade = ((((this.GetAbsWeightedSignalDifference(i, true) * priceOfThisTrade) - partOfPaidForGainLoss) / partOfPaidForGainLoss) * 100) -
                                                                (addableFee / (priceOfThisTrade * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator * this.GetAbsWeightedSignalDifference(i, true)));

                                        //The calculation process works without signs, therefore checking if the percentage of this trade
                                        //is positive or negative (because you could have bought or sold stocks) is very important
                                        //If within the current trade stocks are bought, changes the sign of the percentage
                                        if (Math.Sign(this.GetAbsWeightedSignalDifference(i, false)) == 1)
                                        {
                                            percentageOfThisTrade = percentageOfThisTrade * (-1);
                                        }

                                        paidForGainLoss -= partOfPaidForGainLoss;
                                    }
                                    //if signs unequal
                                    else
                                    {
                                        //TODO: DivideByZeroException at paidForGainLoss if weighting multipliers at signal 3 are smaller than at 2 and so on
                                        percentageOfThisTrade = ((((Math.Abs(oldWeightingMultiplier) * priceOfThisTrade) - paidForGainLoss) / paidForGainLoss) * 100) -
                                                                (addableFee / (priceOfThisTrade * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator * this.GetAbsWeightedSignalDifference(i, true)));

                                        //If within the current trade stocks are bought, changes the sign of the percentage
                                        if (Math.Sign(this.GetAbsWeightedSignalDifference(i, false)) == 1)
                                        {
                                            percentageOfThisTrade = percentageOfThisTrade * (-1);
                                        }

                                        paidForGainLoss = (priceOfThisTrade * Math.Abs(WeightingMultiplier));
                                    }
                                }
                            }

                            resultSet.GainLossPercent += percentageOfThisTrade;
                            profitsForStdDev.Add((double)portfolioPerformance);

                            if ((currentDay != this.mainViewModel.BarList[i].Item1.Date) || (i == this.mainViewModel.BarList.Count))
                            {
                                currentDay = this.mainViewModel.BarList[i].Item1.Date;

                                dailyPortfolioPerformances.Add(currentDayPortfolioPerformance);
                                currentDayPortfolioPerformance = portfolioPerformance;
                            }
                            else
                            {
                                currentDayPortfolioPerformance += portfolioPerformance;
                            }

                            absCumGainLoss += currentGainLoss;
                            resultSet.NetWorth += currentGainLoss;

                            decimal transactionPriceToDisplay = (this.mainViewModel.BarList[i].Item5 * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator * (this.GetAbsWeightedSignalDifference(i, true) * WeightingMultiplier > oldWeightingMultiplier ? 1 : -1)) + addableFee;
                            this.mainViewModel.Orders.Add(new Order(this.mainViewModel.BarList[i].Item1, this.mainViewModel.Signals[i], this.GetWeightingMultiplier(i), priceOfThisTrade, transactionPriceToDisplay, addableFee, percentageOfThisTrade, resultSet.GainLossPercent, portfolioPerformance, resultSet.PortfolioPerformancePercent, currentGainLoss, absCumGainLoss, resultSet.NetWorth));

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

                        decimal ZeroWithFeePaid = Math.Round((-order.PaidFee / (order.Price * RoundLotSize * InnerValue / this.mainViewModel.MiniContractDenominator * this.GetAbsWeightedSignalDifferenceForOrders(i))) * 100, 3);

                        //Get all the normal profit and dont take fee into account for good trades or bad trades
                        if (order.GainLossPercent > ZeroWithFeePaid)
                        {
                            resultSet.NoOfGoodTrades++;
                            resultSet.GainPercent += (order.CumulativePortfolioPerformance -
                                                      this.mainViewModel.Orders[i - 1].CumulativePortfolioPerformance);
                        }
                        else if (order.GainLossPercent < ZeroWithFeePaid)
                        {
                            resultSet.NoOfBadTrades++;
                            resultSet.LossPercent += (order.CumulativePortfolioPerformance -
                                                      this.mainViewModel.Orders[i - 1].CumulativePortfolioPerformance);
                        }
                        else if (order.GainLossPercent == ZeroWithFeePaid)
                        {
                            decimal portfolioPerformanceRelativeToCapital = (order.CumulativePortfolioPerformance -
                                                                             this.mainViewModel.Orders[i - 1].CumulativePortfolioPerformance);
                            //Get all the unrealised profit
                            if (portfolioPerformanceRelativeToCapital > ZeroWithFeePaid)
                            {
                                resultSet.NoOfGoodTrades++;
                                resultSet.GainPercent += portfolioPerformanceRelativeToCapital;
                            }
                            else if (portfolioPerformanceRelativeToCapital < ZeroWithFeePaid)
                            {
                                resultSet.NoOfBadTrades++;
                                resultSet.LossPercent += portfolioPerformanceRelativeToCapital;
                            }
                            //if 0 do nothing, becuase there is not even unrealised profit
                        }
                    }

                    if (resultSet.NoOfBadTrades > 0)
                        resultSet.GtBtRatio = resultSet.NoOfGoodTrades / resultSet.NoOfBadTrades;
                    else
                        resultSet.GtBtRatio = 0;

                    resultSet.StdDevOfProfit = (decimal)this.CalculateStdDevs(profitsForStdDev);
                    resultSet.StdDevOfPEquityPrice = (decimal)this.CalculateStdDevs(EquityPricesForStdDev);

                    //assume 365.25 days per year and get the length of the historical data in years
                    decimal years = ((this.mainViewModel.BarList.Last().Item1 - this.mainViewModel.BarList[0].Item1).Days) / 365.25m;

                    //portfolioperformance without the risk free rate you would have earned via a formula from http://www.frustfrei-lernen.de/mathematik/zinseszins.html
                    double interestRate = 1.5;
                    decimal netWorthWithRiskFreeRate = Convert.ToDecimal(Convert.ToDouble(this.mainViewModel.Capital) * Math.Pow(
                                    (1 + (interestRate / 100)),
                                    Convert.ToDouble(years)));
                    decimal portfolioPerformanceWithRiskFreeRate = (netWorthWithRiskFreeRate - Convert.ToDecimal(this.mainViewModel.Capital)) / Convert.ToDecimal(this.mainViewModel.Capital) * 100;

                    resultSet.SharpeRatio = (resultSet.PortfolioPerformancePercent - portfolioPerformanceWithRiskFreeRate) / resultSet.StdDevOfProfit;

                    resultSet.HighestDailyProfit = dailyPortfolioPerformances.Max();
                    resultSet.HighestDailyLoss = dailyPortfolioPerformances.Min();
                    resultSet.LastDayProfitLoss = dailyPortfolioPerformances.Last();

                    if (parametersUsed.Length == 0)
                    {
                        this.mainViewModel.CalculationResultSets.Add("-", resultSet);
                    }
                    else
                    {
                        this.mainViewModel.CalculationResultSets.Add(parametersUsed, resultSet);
                    }

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
                    return this.GetAbsWeightedSignalDifference(i, true);
                }
            }
            return 0;
        }

        private int GetAbsWeightedSignalDifference(int index, bool returnAbs)
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

            if (returnAbs)
            {
                return (Math.Abs(GetWeightingMultiplierFromSignal(toZero)) + Math.Abs(GetWeightingMultiplierFromSignal(fromZero)));
            }
            else
            {
                if (newSignal > oldSignal)
                {
                    return (Math.Abs(GetWeightingMultiplierFromSignal(toZero)) + Math.Abs(GetWeightingMultiplierFromSignal(fromZero)));
                }
                else
                {
                    return (Math.Abs(GetWeightingMultiplierFromSignal(toZero)) + Math.Abs(GetWeightingMultiplierFromSignal(fromZero))) * -1;
                }
            }
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

        private int switchRoundLotSizeOrInnerValue(int value)
        {
            switch (value)
            {
                case -1:
                    return 1;
                case 0:
                    return 10;
                case 1:
                    return 50;
                case 2:
                    return 100;
            }

            return 1;
        }
    }
}
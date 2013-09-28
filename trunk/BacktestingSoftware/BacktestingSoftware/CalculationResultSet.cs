using System;

namespace BacktestingSoftware
{
    [Serializable()]
    internal class CalculationResultSet
    {
        private decimal _netWorth;
        private decimal _portfolioPerformancePercent;
        private decimal _annualizedPortfolioPerformancePercent;
        private decimal _timeInMarket;
        private decimal _sharpeRatio;
        private decimal _stdDevOfProfit;
        private decimal _stdDevOfPEquityPrice;
        private decimal _gainLossPercent;
        private decimal _annualizedGainLossPercent;
        private decimal _noOfGoodTrades;
        private decimal _gainPercent;
        private decimal _noOfBadTrades;
        private decimal _lossPercent;
        private decimal _gtBtRatio;
        private string _highestDailyProfit;
        private string _highestDailyLoss;
        private string _lastDayProfitLoss;

        public decimal NetWorth
        {
            get { return _netWorth; }
            set { _netWorth = value; }
        }

        public decimal PortfolioPerformancePercent
        {
            get { return _portfolioPerformancePercent; }
            set { _portfolioPerformancePercent = value; }
        }

        public decimal AnnualizedPortfolioPerformancePercent
        {
            get { return _annualizedPortfolioPerformancePercent; }
            set { _annualizedPortfolioPerformancePercent = value; }
        }

        public decimal TimeInMarket
        {
            get { return _timeInMarket; }
            set { _timeInMarket = value; }
        }

        public decimal SharpeRatio
        {
            get { return _sharpeRatio; }
            set { _sharpeRatio = value; }
        }

        public decimal StdDevOfProfit
        {
            get { return _stdDevOfProfit; }
            set { _stdDevOfProfit = value; }
        }

        public decimal StdDevOfPEquityPrice
        {
            get { return _stdDevOfPEquityPrice; }
            set { _stdDevOfPEquityPrice = value; }
        }

        public decimal GainLossPercent
        {
            get { return _gainLossPercent; }
            set { _gainLossPercent = value; }
        }

        public decimal AnnualizedGainLossPercent
        {
            get { return _annualizedGainLossPercent; }
            set { _annualizedGainLossPercent = value; }
        }

        public decimal NoOfGoodTrades
        {
            get { return _noOfGoodTrades; }
            set { _noOfGoodTrades = value; }
        }

        public decimal GainPercent
        {
            get { return _gainPercent; }
            set { _gainPercent = value; }
        }

        public decimal NoOfBadTrades
        {
            get { return _noOfBadTrades; }
            set { _noOfBadTrades = value; }
        }

        public decimal LossPercent
        {
            get { return _lossPercent; }
            set { _lossPercent = value; }
        }

        public decimal GtBtRatio
        {
            get { return _gtBtRatio; }
            set { _gtBtRatio = value; }
        }

        public string HighestDailyProfit
        {
            get { return _highestDailyProfit; }
            set { _highestDailyProfit = value; }
        }

        public string HighestDailyLoss
        {
            get { return _highestDailyLoss; }
            set { _highestDailyLoss = value; }
        }

        public string LastDayProfitLoss
        {
            get { return _lastDayProfitLoss; }
            set { _lastDayProfitLoss = value; }
        }
    }
}
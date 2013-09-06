using System;

namespace BacktestingSoftware
{
    [Serializable()]
    internal class CalculationResultSet
    {
        private decimal _netWorth;
        private decimal _portfolioPerformancePercent;
        private decimal _sharpeRatio;
        private decimal _stdDevOfProfit;
        private decimal _stdDevOfPEquityPrice;
        private decimal _gainLossPercent;
        private decimal _noOfGoodTrades;
        private decimal _gainPercent;
        private decimal _noOfBadTrades;
        private decimal _lossPercent;
        private decimal _gtBtRatio;
        private decimal _highestDailyProfit;
        private decimal _highestDailyLoss;
        private decimal _lastDayProfitLoss;

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

        public decimal HighestDailyProfit
        {
            get { return _highestDailyProfit; }
            set { _highestDailyProfit = value; }
        }

        public decimal HighestDailyLoss
        {
            get { return _highestDailyLoss; }
            set { _highestDailyLoss = value; }
        }

        public decimal LastDayProfitLoss
        {
            get { return _lastDayProfitLoss; }
            set { _lastDayProfitLoss = value; }
        }
    }
}
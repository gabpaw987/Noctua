using System;
using System.ComponentModel;

namespace BacktestingSoftware
{
    [Serializable()]
    internal class Order
    {
        private DateTime _timestamp;
        private int _trendstrength;
        private decimal _quantityMultiplier;
        private decimal _price;
        private decimal _transactionPrice;
        private decimal _gainLossPercent;
        private decimal _cumulativeGainLossPercent;
        private decimal _portfolioPerformance;
        private decimal _cumulativePortfolioPerformance;
        private decimal _absGainLoss;
        private decimal _absCumulativeGainLoss;
        private decimal _currentCapital;

        public Order(DateTime timestamp, int trendstrength, decimal quantityMultiplier, decimal price, decimal transactionPrice, decimal gainLossPercent, decimal cumulativeGainLossPercent, decimal portfolioPerformance, decimal cumulativePortfolioPerformance, decimal absGainLoss, decimal absCumulativeGainLoss, decimal currentCapital)
        {
            _timestamp = timestamp;
            _trendstrength = trendstrength;
            _quantityMultiplier = quantityMultiplier;
            _price = price;
            _transactionPrice = transactionPrice;
            _gainLossPercent = gainLossPercent;
            _cumulativeGainLossPercent = cumulativeGainLossPercent;
            _portfolioPerformance = portfolioPerformance;
            _cumulativePortfolioPerformance = cumulativePortfolioPerformance;
            _absGainLoss = absGainLoss;
            _absCumulativeGainLoss = absCumulativeGainLoss;
            _currentCapital = currentCapital;
        }

        [DisplayName("Time")]
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        [DisplayName("Signal Strength")]
        public int Trendstrength
        {
            get { return _trendstrength; }
            set { _trendstrength = value; }
        }

        [DisplayName("Position")]
        public decimal QuantityMultiplier
        {
            get { return _quantityMultiplier; }
            set { _quantityMultiplier = value; }
        }

        [DisplayName("Price")]
        public decimal Price
        {
            get { return Math.Round(_price, 3); }
            set { _price = value; }
        }

        [DisplayName("Transaction Price")]
        public decimal TransactionPrice
        {
            get { return Math.Round(_transactionPrice, 3); }
            set { _transactionPrice = value; }
        }

        [DisplayName("Gain/Loss [%]")]
        public decimal GainLossPercent
        {
            get { return Math.Round(_gainLossPercent, 3); }
            set { _gainLossPercent = value; }
        }

        [DisplayName("Cumulative Gain/Loss [%]")]
        public decimal CumulativeGainLossPercent
        {
            get { return Math.Round(_cumulativeGainLossPercent, 3); }
            set { _cumulativeGainLossPercent = value; }
        }

        [DisplayName("Portfolio Performance [%]")]
        public decimal PortfolioPerformance
        {
            get { return Math.Round(_portfolioPerformance, 3); }
            set { _portfolioPerformance = value; }
        }

        [DisplayName("Cumulative Portfolio Performance [%]")]
        public decimal CumulativePortfolioPerformance
        {
            get { return Math.Round(_cumulativePortfolioPerformance, 3); }
            set { _cumulativePortfolioPerformance = value; }
        }

        [DisplayName("Absolute Portfolio Performance")]
        public decimal AbsGainLoss
        {
            get { return Math.Round(_absGainLoss, 3); }
            set { _absGainLoss = value; }
        }

        [DisplayName("Absolute Cumulative Portfolio Performance")]
        public decimal AbsCumulativeGainLoss
        {
            get { return Math.Round(_absCumulativeGainLoss, 3); }
            set { _absCumulativeGainLoss = value; }
        }

        [DisplayName("Net Worth")]
        public decimal CurrentCapital
        {
            get { return Math.Round(_currentCapital, 3); }
            set { _currentCapital = value; }
        }
    }
}
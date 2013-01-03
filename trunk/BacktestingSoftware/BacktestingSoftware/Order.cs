using System;

namespace BacktestingSoftware
{
    internal class Order
    {
        private DateTime _timestamp;
        private int _trendstrength;
        private decimal _quantityMultiplier;
        private decimal _price;
        private decimal _gainLossPercent;
        private decimal _cumulativeGainLossPercent;

        public Order(DateTime timestamp, int trendstrength, decimal quantityMultiplier, decimal price, decimal gainLossPercent, decimal cumulativeGainLossPercent)
        {
            _timestamp = timestamp;
            _trendstrength = trendstrength;
            _quantityMultiplier = quantityMultiplier;
            _price = price;
            _gainLossPercent = gainLossPercent;
            _cumulativeGainLossPercent = cumulativeGainLossPercent;
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { _timestamp = value; }
        }

        public int Trendstrength
        {
            get { return _trendstrength; }
            set { _trendstrength = value; }
        }

        public decimal QuantityMultiplier
        {
            get { return _quantityMultiplier; }
            set { _quantityMultiplier = value; }
        }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public decimal GainLossPercent
        {
            get { return _gainLossPercent; }
            set { _gainLossPercent = value; }
        }

        public decimal CumulativeGainLossPercent
        {
            get { return _cumulativeGainLossPercent; }
            set { _cumulativeGainLossPercent = value; }
        }
    }
}
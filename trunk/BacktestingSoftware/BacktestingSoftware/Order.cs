using System;

namespace BacktestingSoftware
{
    internal class Order
    {
        private DateTime _timestamp;
        private int _trendstrength;
        private double _quantityMultiplier;
        private double _price;
        private double _gainLossPercent;
        private double _cumulativeGainLossPercent;

        public Order(DateTime timestamp, int trendstrength, double quantityMultiplier, double price, double gainLossPercent, double cumulativeGainLossPercent)
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

        public double QuantityMultiplier
        {
            get { return _quantityMultiplier; }
            set { _quantityMultiplier = value; }
        }

        public double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public double GainLossPercent
        {
            get { return _gainLossPercent; }
            set { _gainLossPercent = value; }
        }

        public double CumulativeGainLossPercent
        {
            get { return _cumulativeGainLossPercent; }
            set { _cumulativeGainLossPercent = value; }
        }
    }
}
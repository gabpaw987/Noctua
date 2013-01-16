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

        [DisplayName("Quantity Multiplier")]
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
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BacktestingSoftware
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _algorithmFileName;

        public string AlgorithmFileName
        {
            get
            {
                return _algorithmFileName;
            }
            set
            {
                if (value != _algorithmFileName)
                {
                    _algorithmFileName = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AlgorithmFileName"));
                }
            }
        }

        private string _dataFileName;

        public string DataFileName
        {
            get
            {
                return _dataFileName;
            }
            set
            {
                if (value != _dataFileName)
                {
                    _dataFileName = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("DataFileName"));
                }
            }
        }

        private DateTime _startDate;

        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                if (!value.Equals(_startDate))
                {
                    _startDate = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StartDate"));
                }
            }
        }

        private DateTime _endDate;

        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                if (!value.Equals(_endDate))
                {
                    _endDate = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("EndDate"));
                }
            }
        }

        private List<Order> _orders;

        public List<Order> Orders
        {
            get
            {
                return _orders;
            }
            set
            {
                if (!value.Equals(_orders))
                {
                    _orders = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Orders"));
                }
            }
        }

        private double _gainLossPercent;

        public double GainLossPercent
        {
            get
            {
                return _gainLossPercent;
            }
            set
            {
                if (!value.Equals(_gainLossPercent))
                {
                    _gainLossPercent = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("GainLossPercent"));
                }
            }
        }

        private double _noOfGoodTrades;

        public double NoOfGoodTrades
        {
            get
            {
                return _noOfGoodTrades;
            }
            set
            {
                if (!value.Equals(_noOfGoodTrades))
                {
                    _noOfGoodTrades = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("NoOfGoodTrades"));
                }
            }
        }

        private double _noOfBadTrades;

        public double NoOfBadTrades
        {
            get
            {
                return _noOfBadTrades;
            }
            set
            {
                if (!value.Equals(_noOfBadTrades))
                {
                    _noOfBadTrades = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("NoOfBadTrades"));
                }
            }
        }

        private double _gtBtRatio;

        public double GtBtRatio
        {
            get
            {
                return _gtBtRatio;
            }
            set
            {
                if (!value.Equals(_gtBtRatio))
                {
                    _gtBtRatio = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("GtBtRatio"));
                }
            }
        }
    }
}
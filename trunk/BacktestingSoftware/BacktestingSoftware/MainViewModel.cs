using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace BacktestingSoftware
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<int> _signals;

        public List<int> Signals
        {
            get
            {
                return _signals;
            }
            set
            {
                if (value != _signals)
                {
                    _signals = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Signals"));
                }
            }
        }

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> _barList;

        public List<Tuple<DateTime, decimal, decimal, decimal, decimal>> BarList
        {
            get
            {
                return _barList;
            }
            set
            {
                if (value != _barList)
                {
                    _barList = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BarList"));
                        PropertyChanged(this, new PropertyChangedEventArgs("TimeSpanToDisplay"));
                        PropertyChanged(this, new PropertyChangedEventArgs("DatesToDisplay"));
                    }
                }
            }
        }

        private Dictionary<string, List<decimal>> _indicatorDictionary;

        public Dictionary<string, List<decimal>> IndicatorDictionary
        {
            get
            {
                return _indicatorDictionary;
            }
            set
            {
                if (value != _indicatorDictionary)
                {
                    _indicatorDictionary = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IndicatorDictionary"));
                }
            }
        }

        private Dictionary<string, List<decimal>> _oscillatorDictionary;

        public Dictionary<string, List<decimal>> OscillatorDictionary
        {
            get
            {
                return _oscillatorDictionary;
            }
            set
            {
                if (value != _oscillatorDictionary)
                {
                    _oscillatorDictionary = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("OscillatorDictionary"));
                }
            }
        }

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

        private string _stockSymbolForRealTime;

        public string StockSymbolForRealTime
        {
            get
            {
                return _stockSymbolForRealTime;
            }
            set
            {
                if (value != _stockSymbolForRealTime)
                {
                    _stockSymbolForRealTime = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StockSymbolForRealTime"));
                }
            }
        }

        private string _barsize;

        public string Barsize
        {
            get
            {
                return _barsize;
            }
            set
            {
                if (value != _barsize)
                {
                    _barsize = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Barsize"));
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

        private decimal _gainLossPercent;

        public decimal GainLossPercent
        {
            get
            {
                return Math.Round(_gainLossPercent, 3);
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

        private decimal _annualizedGainLossPercent;

        public decimal AnnualizedGainLossPercent
        {
            get
            {
                return Math.Round(_annualizedGainLossPercent, 3);
            }
            set
            {
                if (!value.Equals(_annualizedGainLossPercent))
                {
                    _annualizedGainLossPercent = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AnnualizedGainLossPercent"));
                }
            }
        }

        private decimal _noOfGoodTrades;

        public decimal NoOfGoodTrades
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

        private decimal _noOfBadTrades;

        public decimal NoOfBadTrades
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

        private decimal _gtBtRatio;

        public decimal GtBtRatio
        {
            get
            {
                return Math.Round(_gtBtRatio, 3);
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

        private decimal _gainPercent;

        public decimal GainPercent
        {
            get
            {
                return Math.Round(_gainPercent, 3);
            }
            set
            {
                if (!value.Equals(_gainPercent))
                {
                    _gainPercent = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("GainPercent"));
                }
            }
        }

        private decimal _lossPercent;

        public decimal LossPercent
        {
            get
            {
                return Math.Round(_lossPercent, 3);
            }
            set
            {
                if (!value.Equals(_lossPercent))
                {
                    _lossPercent = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("LossPercent"));
                }
            }
        }

        private decimal _stdDevOfProfit;

        public decimal StdDevOfProfit
        {
            get
            {
                return Math.Round(_stdDevOfProfit, 3);
            }
            set
            {
                if (!value.Equals(_stdDevOfProfit))
                {
                    _stdDevOfProfit = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StdDevOfProfit"));
                }
            }
        }

        private decimal _stdDevOfEquityPrice;

        public decimal StdDevOfPEquityPrice
        {
            get
            {
                return Math.Round(_stdDevOfEquityPrice, 3);
            }
            set
            {
                if (!value.Equals(_stdDevOfEquityPrice))
                {
                    _stdDevOfEquityPrice = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StdDevOfPEquityPrice"));
                }
            }
        }

        private string _saveFileName;

        public string SaveFileName
        {
            get
            {
                return _saveFileName;
            }
            set
            {
                if (!value.Equals(_saveFileName))
                {
                    _saveFileName = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SaveFileName"));
                }
            }
        }

        private string _loadFileName;

        public string LoadFileName
        {
            get
            {
                return _loadFileName;
            }
            set
            {
                if (!value.Equals(_loadFileName))
                {
                    _loadFileName = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("LoadFileName"));
                }
            }
        }

        private int _valueOfSliderOne;

        public int ValueOfSliderOne
        {
            get
            {
                return _valueOfSliderOne;
            }
            set
            {
                if (!value.Equals(_valueOfSliderOne))
                {
                    _valueOfSliderOne = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueOfSliderOne"));
                }
            }
        }

        private int _valueOfSliderTwo;

        public int ValueOfSliderTwo
        {
            get
            {
                return _valueOfSliderTwo;
            }
            set
            {
                if (!value.Equals(_valueOfSliderTwo))
                {
                    _valueOfSliderTwo = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueOfSliderTwo"));
                }
            }
        }

        private int _valueOfSliderThree;

        public int ValueOfSliderThree
        {
            get
            {
                return _valueOfSliderThree;
            }
            set
            {
                if (!value.Equals(_valueOfSliderThree))
                {
                    _valueOfSliderThree = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueOfSliderThree"));
                }
            }
        }

        private int _valueOfSliderFour;

        public int ValueOfSliderFour
        {
            get
            {
                return _valueOfSliderFour;
            }
            set
            {
                if (!value.Equals(_valueOfSliderFour))
                {
                    _valueOfSliderFour = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueOfSliderFour"));
                }
            }
        }

        private int _valueOfSliderFive;

        public int ValueOfSliderFive
        {
            get
            {
                return _valueOfSliderFive;
            }
            set
            {
                if (!value.Equals(_valueOfSliderFive))
                {
                    _valueOfSliderFive = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueOfSliderFive"));
                }
            }
        }

        private int _valueOfSliderSix;

        public int ValueOfSliderSix
        {
            get
            {
                return _valueOfSliderSix;
            }
            set
            {
                if (!value.Equals(_valueOfSliderSix))
                {
                    _valueOfSliderSix = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ValueOfSliderSix"));
                }
            }
        }

        private int _roundLotSize;

        public int RoundLotSize
        {
            get
            {
                return _roundLotSize;
            }
            set
            {
                if (!value.Equals(_roundLotSize))
                {
                    _roundLotSize = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RoundLotSize"));
                }
            }
        }

        private int _capital;

        public String Capital
        {
            get
            {
                return _capital.ToString();
            }
            set
            {
                try
                {
                    if (!(int.Parse(value)).Equals(_capital))
                    {
                        _capital = int.Parse(value);
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("Capital"));
                    }
                }
                catch (Exception)
                {
                    _capital = 0;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Capital"));
                }
            }
        }

        private decimal _absTransactionFee;

        public String AbsTransactionFee
        {
            get
            {
                return _absTransactionFee.ToString();
            }
            set
            {
                try
                {
                    if (!(decimal.Parse(value)).Equals(_absTransactionFee))
                    {
                        _absTransactionFee = decimal.Parse(value);
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("AbsTransactionFee"));
                    }
                }
                catch (Exception)
                {
                    _absTransactionFee = 0;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AbsTransactionFee"));
                }
            }
        }

        private decimal _relTransactionFee;

        public String RelTransactionFee
        {
            get
            {
                return Convert.ToString(_relTransactionFee);
            }
            set
            {
                try
                {
                    if (!(decimal.Parse(value)).Equals(_relTransactionFee))
                    {
                        _relTransactionFee = decimal.Parse(value);
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("RelTransactionFee"));
                    }
                }
                catch (Exception)
                {
                    _relTransactionFee = 0;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RelTransactionFee"));
                }
            }
        }

        private decimal _pricePremium;

        public String PricePremium
        {
            get
            {
                return _pricePremium.ToString();
            }
            set
            {
                try
                {
                    if (!(decimal.Parse(value)).Equals(_pricePremium))
                    {
                        _pricePremium = decimal.Parse(value);
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("PricePremium"));
                    }
                }
                catch (Exception)
                {
                    _pricePremium = 0;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("PricePremium"));
                }
            }
        }

        private decimal _netWorth;

        public decimal NetWorth
        {
            get
            {
                return _netWorth;
            }
            set
            {
                if (!value.Equals(_netWorth))
                {
                    _netWorth = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("NetWorth"));
                        PropertyChanged(this, new PropertyChangedEventArgs("NetWorthToDisplay"));
                    }
                }
            }
        }

        public string NetWorthToDisplay
        {
            get
            {
                if (Math.Sign(_netWorth - _capital) == 1)
                    return Math.Round(_netWorth, 3) + " (+" + Math.Round(_netWorth - _capital, 3) + ")";
                else
                    return Math.Round(_netWorth, 3) + " (" + Math.Round(_netWorth - _capital, 3) + ")";
            }
        }

        public decimal _portfolioPerformancePercent;

        public decimal PortfolioPerformancePercent
        {
            get
            {
                return Math.Round(_portfolioPerformancePercent, 3);
            }
            set
            {
                _portfolioPerformancePercent = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("PortfolioPerformancePercent"));
            }
        }

        private decimal _annualizedPortfolioPerformancePercent;

        public decimal AnnualizedPortfolioPerformancePercent
        {
            get
            {
                return Math.Round(_annualizedPortfolioPerformancePercent, 3);
            }
            set
            {
                _annualizedPortfolioPerformancePercent = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("AnnualizedPortfolioPerformancePercent"));
            }
        }

        private decimal _sharpeRatio;

        public decimal SharpeRatio
        {
            get
            {
                return Math.Round(_sharpeRatio, 3);
            }
            set
            {
                if (!value.Equals(_sharpeRatio))
                {
                    _sharpeRatio = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SharpeRatio"));
                }
            }
        }

        private List<StackPanel> _indicatorPanels;

        public List<StackPanel> IndicatorPanels
        {
            get
            {
                return _indicatorPanels;
            }
            set
            {
                if (!value.Equals(_indicatorPanels))
                {
                    _indicatorPanels = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IndicatorPanels"));
                }
            }
        }

        private bool _isRealTimeEnabled;

        public bool IsRealTimeEnabled
        {
            get
            {
                return _isRealTimeEnabled;
            }
            set
            {
                if (!value.Equals(_isRealTimeEnabled))
                {
                    _isRealTimeEnabled = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsRealTimeEnabled"));
                }
            }
        }

        private bool _isAlgorithmUsingMaps;

        public bool IsAlgorithmUsingMaps
        {
            get
            {
                return _isAlgorithmUsingMaps;
            }
            set
            {
                if (!value.Equals(_isAlgorithmUsingMaps))
                {
                    _isAlgorithmUsingMaps = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsAlgorithmUsingMaps"));
                }
            }
        }

        private string _highestDailyProfit;

        public string HighestDailyProfit
        {
            get
            {
                return _highestDailyProfit;
            }
            set
            {
                if (!value.Equals(_highestDailyProfit))
                {
                    _highestDailyProfit = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("HighestDailyProfit"));
                }
            }
        }

        private string _highestDailyLoss;

        public string HighestDailyLoss
        {
            get
            {
                return _highestDailyLoss;
            }
            set
            {
                if (!value.Equals(_highestDailyLoss))
                {
                    _highestDailyLoss = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("HighestDailyLoss"));
                }
            }
        }

        private string _lastDayProfitLoss;

        public string LastDayProfitLoss
        {
            get
            {
                return _lastDayProfitLoss;
            }
            set
            {
                if (!value.Equals(_lastDayProfitLoss))
                {
                    _lastDayProfitLoss = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("LastDayProfitLoss"));
                }
            }
        }

        private string _additionalParameters;

        public string AdditionalParameters
        {
            get
            {
                return _additionalParameters;
            }
            set
            {
                if (!value.Equals(_additionalParameters))
                {
                    _additionalParameters = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AdditionalParameters"));
                }
            }
        }

        private SortedDictionary<string, CalculationResultSet> _calculationResultSets;

        public SortedDictionary<string, CalculationResultSet> CalculationResultSets
        {
            get
            {
                return _calculationResultSets;
            }
            set
            {
                if (!value.Equals(_calculationResultSets))
                {
                    _calculationResultSets = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CalculationResultSets"));
                }
            }
        }

        private bool _isDataFutures;

        public bool IsDataFutures
        {
            get
            {
                return _isDataFutures;
            }
            set
            {
                if (!value.Equals(_isDataFutures))
                {
                    if (value)
                    {
                        this.InnerValue = this.RoundLotSize;
                        this.RoundLotSize = -1;

                        if (this.IsMiniContract)
                        {
                            this.MiniContractDenominator = 5;
                        }
                        else
                        {
                            this.MiniContractDenominator = 1;
                        }
                    }
                    else
                    {
                        this.RoundLotSize = this.InnerValue;
                        this.InnerValue = -1;
                        this.MiniContractDenominator = 1;
                    }

                    _isDataFutures = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsDataFutures"));
                }
            }
        }

        private int _innerValue;

        public int InnerValue
        {
            get
            {
                return _innerValue;
            }
            set
            {
                if (!value.Equals(_innerValue))
                {
                    _innerValue = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("InnerValue"));
                }
            }
        }

        private bool _isMiniContract;

        public bool IsMiniContract
        {
            get
            {
                return _isMiniContract;
            }
            set
            {
                if (!value.Equals(_isMiniContract))
                {
                    if (value)
                    {
                        MiniContractDenominator = 5;
                    }
                    else
                    {
                        MiniContractDenominator = 1;
                    }

                    _isMiniContract = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsMiniContract"));
                }
            }
        }

        private int _miniContractDenominator;

        public int MiniContractDenominator
        {
            get
            {
                return _miniContractDenominator;
            }
            set
            {
                if (!value.Equals(_miniContractDenominator))
                {
                    _miniContractDenominator = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("MiniContractDenominator"));
                }
            }
        }

        private decimal _timeInMarket;

        public decimal TimeInMarket
        {
            get
            {
                return Math.Round(_timeInMarket, 3);
            }
            set
            {
                if (!value.Equals(_timeInMarket))
                {
                    _timeInMarket = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("TimeInMarket"));
                }
            }
        }

        private int _calculationThreadCount;

        public int CalculationThreadCount
        {
            get
            {
                return _calculationThreadCount;
            }
            set
            {
                if (!value.Equals(_calculationThreadCount))
                {
                    if (value < 0)
                    {
                        _calculationThreadCount = 1;
                    }
                    else
                    {
                        _calculationThreadCount = value;
                    }

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CalculationThreadCount"));
                }
            }
        }

        private bool _isNetWorthChartInPercentage;

        public bool IsNetWorthChartInPercentage
        {
            get
            {
                return _isNetWorthChartInPercentage;
            }
            set
            {
                if (!value.Equals(_isNetWorthChartInPercentage))
                {
                    _isNetWorthChartInPercentage = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsNetWorthChartInPercentage"));
                }
            }
        }

        private List<decimal> _performanceFromPrice;

        public List<decimal> PerformanceFromPrice
        {
            get
            {
                return _performanceFromPrice;
            }
            set
            {
                if (!value.Equals(_performanceFromPrice))
                {
                    _performanceFromPrice = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("PerformanceFromPrice"));
                }
            }
        }

        public string TimeSpanToDisplay
        {
            get
            {
                if (this.BarList != null)
                {
                    if (this.BarList.Count > 1)
                    {
                        return (this.BarList.Last().Item1 - this.BarList[0].Item1).ToString(@"%d\.hh\:mm\:ss").Replace(".", " days ") + " hours";
                    }
                    else
                    {
                        return "0";
                    }
                }
                else
                {
                    return "0";
                }
            }
        }

        public string DatesToDisplay
        {
            get
            {
                if (this.BarList != null)
                {
                    if (this.BarList.Count > 1)
                    {
                        return this.BarList[0].Item1 + " - " + this.BarList.Last().Item1;
                    }
                    else
                    {
                        return "-";
                    }
                }
                else
                {
                    return "-";
                }
            }
        }
    }
}
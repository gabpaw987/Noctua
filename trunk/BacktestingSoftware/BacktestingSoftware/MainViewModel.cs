﻿using System;
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

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> _barList;

        public List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> BarList
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

                    Properties.Settings.Default.AlgorithmFileName = this.AlgorithmFileName;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.DataFileName = this.DataFileName;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.StartDate = this.StartDate;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.StockSymbolForRealTime = this.StockSymbolForRealTime;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.Barsize = this.Barsize;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.EndDate = this.EndDate;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.SaveFileName = this.SaveFileName;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.ValueOfSliderOne = this.ValueOfSliderOne;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.ValueOfSliderTwo = this.ValueOfSliderTwo;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.ValueOfSliderThree = this.ValueOfSliderThree;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.ValueOfSliderFour = this.ValueOfSliderFour;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.ValueOfSliderFive = this.ValueOfSliderFive;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.ValueOfSliderSix = this.ValueOfSliderSix;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.RoundLotSize = this.RoundLotSize;
                    Properties.Settings.Default.Save();

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
                finally
                {
                    Properties.Settings.Default.Capital = this.Capital;
                    Properties.Settings.Default.Save();
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
                finally
                {
                    Properties.Settings.Default.AbsTransactionFee = this.AbsTransactionFee;
                    Properties.Settings.Default.Save();
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
                finally
                {
                    Properties.Settings.Default.RelTransactionFee = this.RelTransactionFee;
                    Properties.Settings.Default.Save();
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
                finally
                {
                    Properties.Settings.Default.PricePremium = this.PricePremium;
                    Properties.Settings.Default.Save();
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

                    Properties.Settings.Default.IsRealTimeEnabled = this.IsRealTimeEnabled;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsRealTimeEnabled"));
                }
            }
        }

        private bool _shallDrawIndicatorMap;

        public bool ShallDrawIndicatorMap
        {
            get
            {
                return _shallDrawIndicatorMap;
            }
            set
            {
                if (!value.Equals(_shallDrawIndicatorMap))
                {
                    _shallDrawIndicatorMap = value;

                    Properties.Settings.Default.ShallDrawIndicatorMap = this.ShallDrawIndicatorMap;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ShallDrawIndicatorMap"));
                }
            }
        }

        private bool _shallDrawOscillatorMap;

        public bool ShallDrawOscillatorMap
        {
            get
            {
                return _shallDrawOscillatorMap;
            }
            set
            {
                if (!value.Equals(_shallDrawOscillatorMap))
                {
                    _shallDrawOscillatorMap = value;

                    Properties.Settings.Default.ShallDrawOscillatorMap = this.ShallDrawOscillatorMap;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ShallDrawOscillatorMap"));
                }
            }
        }

        private bool _shallDrawVolume;

        public bool ShallDrawVolume
        {
            get
            {
                return _shallDrawVolume;
            }
            set
            {
                if (!value.Equals(_shallDrawVolume))
                {
                    _shallDrawVolume = value;

                    Properties.Settings.Default.ShallDrawVolume = this.ShallDrawVolume;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ShallDrawVolume"));
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

                    Properties.Settings.Default.AdditionalParameters = this.AdditionalParameters;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AdditionalParameters"));
                }
            }
        }

        private Dictionary<string, CalculationResultSet> _calculationResultSets;

        public Dictionary<string, CalculationResultSet> CalculationResultSets
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

        private string _performancePageMargin;

        public string PerformancePageMargin
        {
            get
            {
                return _performancePageMargin;
            }
            set
            {
                if (!value.Equals(_performancePageMargin))
                {
                    _performancePageMargin = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PerformancePageMargin"));
                        PropertyChanged(this, new PropertyChangedEventArgs("PerformancePageMarginLarge"));
                        PropertyChanged(this, new PropertyChangedEventArgs("PerformancePageMarginIntLarge"));
                        PropertyChanged(this, new PropertyChangedEventArgs("PerformancePageMarginSeparatorLarge"));
                        PropertyChanged(this, new PropertyChangedEventArgs("PerformancePageMarginFirstSeparatorLarge"));
                    }
                }
            }
        }

        public string PerformancePageMarginLarge
        {
            get
            {
                return "0,0,0," + this.PerformancePageMarginIntLarge;
            }
        }

        public string PerformancePageMarginSeparatorLarge
        {
            get
            {
                return "0,0,100," + this.PerformancePageMarginIntLarge;
            }
        }

        public string PerformancePageMarginFirstSeparatorLarge
        {
            get
            {
                return "5,0,105," + this.PerformancePageMarginIntLarge;
            }
        }

        public string PerformancePageMarginIntLarge
        {
            get
            {
                if (this.PerformancePageMargin != null)
                {
                    int margin = int.Parse(this.PerformancePageMargin.Substring(6));
                    if (margin <= 0)
                        return "5";
                    else
                    {
                        return "15";
                    }
                }
                else
                {
                    return "5";
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
                        if (this.RoundLotSize != -1)
                        {
                            this.InnerValue = this.RoundLotSize;
                            this.RoundLotSize = -1;
                        }
                    }
                    else
                    {
                        this.RoundLotSize = this.InnerValue;
                        this.InnerValue = -1;
                        this.IsFullFuturePriceData = false;
                        this.IsMiniContract = false;
                    }

                    _isDataFutures = value;

                    Properties.Settings.Default.IsDataFutures = this.IsDataFutures;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.InnerValue = this.InnerValue;
                    Properties.Settings.Default.Save();

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
                    _isMiniContract = value;

                    Properties.Settings.Default.IsMiniContract = this.IsMiniContract;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsMiniContract"));
                }
            }
        }

        private int _miniContractFactor;

        public int MiniContractFactor
        {
            get
            {
                return _miniContractFactor;
            }
            set
            {
                if (!value.Equals(_miniContractFactor))
                {
                    _miniContractFactor = value;

                    Properties.Settings.Default.MiniContractFactor = this.MiniContractFactor;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("MiniContractFactor"));
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

                    Properties.Settings.Default.CalculationThreadCount = this.CalculationThreadCount;
                    Properties.Settings.Default.Save();

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

                    Properties.Settings.Default.IsNetWorthChartInPercentage = this.IsNetWorthChartInPercentage;
                    Properties.Settings.Default.Save();

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

        private int _noOfGoodDays;

        public int NoOfGoodDays
        {
            get
            {
                return _noOfGoodDays;
            }
            set
            {
                if (!value.Equals(_noOfGoodDays))
                {
                    _noOfGoodDays = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("NoOfGoodDays"));
                }
            }
        }

        private int _noOfBadDays;

        public int NoOfBadDays
        {
            get
            {
                return _noOfBadDays;
            }
            set
            {
                if (!value.Equals(_noOfBadDays))
                {
                    _noOfBadDays = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("NoOfBadDays"));
                }
            }
        }

        private decimal _goodDayBadDayRatio;

        public decimal GoodDayBadDayRatio
        {
            get
            {
                return Math.Round(_goodDayBadDayRatio, 3);
            }
            set
            {
                if (!value.Equals(_goodDayBadDayRatio))
                {
                    _goodDayBadDayRatio = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("GoodDayBadDayRatio"));
                }
            }
        }

        private string _themeName;

        public string ThemeName
        {
            get
            {
                return _themeName;
            }
            set
            {
                if (!value.Equals(_themeName))
                {
                    _themeName = value;

                    Properties.Settings.Default.ThemeName = this.ThemeName;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ThemeName"));
                }
            }
        }

        private bool _useRegularTradingHours;

        public bool UseRegularTradingHours
        {
            get
            {
                return _useRegularTradingHours;
            }
            set
            {
                if (!value.Equals(_useRegularTradingHours))
                {
                    _useRegularTradingHours = value;

                    Properties.Settings.Default.UseRegularTradingHours = this.UseRegularTradingHours;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("UseRegularTradingHours"));
                }
            }
        }

        private bool _isFullFuturePriceData;

        public bool IsFullFuturePriceData
        {
            get
            {
                return _isFullFuturePriceData;
            }
            set
            {
                if (!value.Equals(_isFullFuturePriceData))
                {
                    _isFullFuturePriceData = value;

                    Properties.Settings.Default.IsFullFuturePriceData = this.IsFullFuturePriceData;
                    Properties.Settings.Default.Save();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsFullFuturePriceData"));
                }
            }
        }
    }
}
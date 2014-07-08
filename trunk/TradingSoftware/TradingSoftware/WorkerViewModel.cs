using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TradingSoftware
{
    public class WorkerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isThreadRunning;

        public bool IsThreadRunning
        {
            get
            {
                return _isThreadRunning;
            }
            set
            {
                if (value != _isThreadRunning)
                {
                    _isThreadRunning = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsThreadRunning"));
                }
            }
        }

        private string _consoleText;

        public string ConsoleText
        {
            get
            {
                return _consoleText;
            }
            set
            {
                if (value != _consoleText)
                {
                    _consoleText = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ConsoleText"));
                }
            }
        }

        private string _signalText;

        public string SignalText
        {
            get
            {
                return _signalText;
            }
            set
            {
                if (value != _signalText)
                {
                    _signalText = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SignalText"));
                }
            }
        }

        public bool _isTrading;

        [DisplayName("Is trading?")]
        public bool IsTrading
        {
            get 
            {
                bool newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "isTrading").Equals("true") ? true : false;
                if (!_isTrading.Equals(newValue))
                {
                    this.IsTrading = newValue;
                }
                return _isTrading;
            }
            set
            {
                if (value != _isTrading)
                {
                    _isTrading = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "isTrading", value.Equals(true) ? "true" : "false");

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsTrading"));
                }
            }
        }

        private Contract _equity;
        
        public Contract EquityAsContract
        {
            get
            {
                string tmpEquity = this.EquityAsString;
                return _equity;
            }
            set
            {
                string newEquitySymbol = "";

                if (this.IsFutureTrading)
                {
                    newEquitySymbol = ConvertFutureToString((Future)_equity);
                }
                else
                {
                    newEquitySymbol = _equity.Symbol;
                }

                this.EquityAsString = newEquitySymbol;
            }
        }

        [DisplayName("Symbol")]
        public string EquityAsString
        {
            get
            {
                string currentValue = "";

                if (_equity != null)
                {
                    //To not get into an infinite loop
                    if (this._isFutureTrading)
                    {
                        currentValue = ConvertFutureToString((Future)_equity);
                    }
                    else
                    {
                        currentValue = _equity.Symbol;
                    }

                    string newValue = XMLHandler.ReadValueFromXML(currentValue, "symbol"); 
                    
                    if (!currentValue.Equals(newValue) && newValue.Length != 0)
                    {
                        this.EquityAsString = newValue;
                    }
                }

                if (this._isFutureTrading)
                {
                    return ConvertFutureToString((Future)_equity);
                }
                else
                {
                    return _equity.Symbol;
                }
            }
            set
            {
                if (value.Length != 0)
                {
                    string currentValue = "";

                    if (_equity != null)
                    {

                        if (this._isFutureTrading)
                        {
                            currentValue = ConvertFutureToString((Future)_equity);
                        }
                        else
                        {
                            currentValue = _equity.Symbol;
                        }
                    }

                    if (currentValue != value)
                    {
                        XMLHandler.WriteValueToXML(_equity == null ? value : currentValue, "symbol", value);

                        if (this._isFutureTrading)
                        {
                            _equity = ConvertToFutures(value);
                        }
                        else
                        {
                            _equity = new Equity(value);
                        }

                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("EquityAsString"));
                    }
                }
            }
        }

        private BarSize _barsize;

        [DisplayName("Barsize")]
        public BarSize BarsizeAsObject
        {
            get
            {
                string tmpBarSize = this.BarsizeAsString;
                return _barsize;
            }
            set
            {
                if (!value.Equals(_barsize))
                {
                    this.BarsizeAsString = value.ToString();
                }
            }
        }

        [DisplayName("Barsize")]
        public string BarsizeAsString
        {
            get
            {
                string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "barsize");
                if (!_barsize.Equals(newValue))
                {
                    this.BarsizeAsString = newValue;
                }

                if (_barsize.ToString().Equals("OneMinute"))
                {
                    return "Minute";
                }
                else if (_barsize.ToString().Equals("OneDay"))
                {
                    return "Daily";
                }
                else
                {
                    return "Not supported";
                }
            }
            set
            {
                if (value.Equals("mBar") || value.Equals("Minute") || value.Equals("OneMinute"))
                {
                    value = BarSize.OneMinute.ToString();
                }
                else if (value.Equals("dBar") || value.Equals("Daily") || value.Equals("OneDay"))
                {
                    value = BarSize.OneDay.ToString();
                }

                if (!value.Equals(_barsize.ToString()))
                {
                    if (value.Equals("OneMinute"))
                    {
                        _barsize = BarSize.OneMinute;
                        XMLHandler.WriteValueToXML(this.EquityAsString, "barsize", "Minute");
                    }
                    else if (value.Equals("OneDay"))
                    {
                        _barsize = BarSize.OneDay;
                        XMLHandler.WriteValueToXML(this.EquityAsString, "barsize", "Daily");
                    }

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsString"));
                        PropertyChanged(this, new PropertyChangedEventArgs("BarsizeAsObject"));
                    }
                }
            }
        }

        public HistoricalDataType _historicalType;
        public RealTimeBarType _realtimeType;

        [DisplayName("Data type")]
        public string DataType
        {
            get
            {
                string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "dataType");
                if (!_historicalType.ToString().Equals(newValue) || !_realtimeType.ToString().Equals(newValue))
                {
                    this.DataType = newValue;
                }

                if (_historicalType.ToString().Equals(_realtimeType.ToString()))
                {
                    if (_historicalType.ToString().Equals("Trades"))
                    {
                        return "Last";
                    }
                    else
                    {
                        return _historicalType.ToString();
                    }
                }
                else
                {
                    return "inconsistent";
                }
            }
            set
            {
                if (value.Equals("Last"))
                {
                    value = "Trades";
                }
                if (!_historicalType.ToString().Equals(value) || !_realtimeType.ToString().Equals(value))
                {
                    if (value.Equals("Bid"))
                    {
                        _historicalType = HistoricalDataType.Bid;
                        _realtimeType = RealTimeBarType.Bid;
                    }
                    else if (value.Equals("Ask"))
                    {
                        _historicalType = HistoricalDataType.Ask;
                        _realtimeType = RealTimeBarType.Ask;
                    }
                    else if (value.Equals("Last") || value.Equals("Trades"))
                    {
                        _historicalType = HistoricalDataType.Trades;
                        _realtimeType = RealTimeBarType.Trades;
                    }
                    else if (value.Equals("Midpoint"))
                    {
                        _historicalType = HistoricalDataType.Midpoint;
                        _realtimeType = RealTimeBarType.Midpoint;
                    }

                    XMLHandler.WriteValueToXML(this.EquityAsString, "dataType", value);

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("DataType"));
                }
            }
        }

        public decimal _pricePremiumPercentage;

        [DisplayName("Price premium [%]")]
        public decimal PricePremiumPercentage
        {
            get
            {
                decimal newValue = Decimal.Parse(XMLHandler.ReadValueFromXML(this.EquityAsString, "pricePremiumPercentage"), CultureInfo.InvariantCulture);
                
                if (!_pricePremiumPercentage.Equals(newValue))
                {
                    this.PricePremiumPercentage = newValue;
                }

                return _pricePremiumPercentage;
            }
            set
            {
                if (_pricePremiumPercentage != value)
                {
                    _pricePremiumPercentage = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "pricePremiumPercentage", value.ToString(CultureInfo.InvariantCulture));

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("PricePremiumPercentage"));
                }
            }
        }

        public int _currentPosition;

        [DisplayName("Cur. Position")]
        public int CurrentPosition
        {
            get
            {
                int newValue = int.Parse(XMLHandler.ReadValueFromXML(this.EquityAsString, "currentPosition"), CultureInfo.InvariantCulture);
                
                if (!_currentPosition.Equals(newValue))
                {
                    this.CurrentPosition = newValue;
                }

                return _currentPosition;
            }
            set
            {
                if (_currentPosition != value)
                {
                    _currentPosition = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "currentPosition", value.ToString(CultureInfo.InvariantCulture));

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentPosition"));
                }
            }
        }

        public int _roundLotSize;

        [DisplayName("Round Lot Size")]
        public int RoundLotSize
        {
            get
            {
                int newValue = int.Parse(XMLHandler.ReadValueFromXML(this.EquityAsString, "roundLotSize"), CultureInfo.InvariantCulture);
                
                if (!_roundLotSize.Equals(newValue))
                {
                    this.RoundLotSize = newValue;
                }

                return _roundLotSize;
            }
            set
            {
                if (_roundLotSize != value)
                {
                    _roundLotSize = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "roundLotSize", value.ToString(CultureInfo.InvariantCulture));

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("RoundLotSize"));
                }
            }
        }

        public bool _isFutureTrading;

        [DisplayName("FutureTrading")]
        public bool IsFutureTrading
        {
            get 
            {
                bool newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "isFutureTrading").Equals("true") ? true : false;
                if (!_isFutureTrading.Equals(newValue))
                {
                    this.IsFutureTrading = newValue;
                }
                return _isFutureTrading;
            }
            set
            {
                if (value != _isFutureTrading)
                {
                    string equityString = this.EquityAsString;
                    this._equity = null;

                    _isFutureTrading = value;

                    this.EquityAsString = equityString;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "isFutureTrading", value.Equals(true) ? "true" : "false");

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsFutureTrading"));
                }
            }
        }

        public bool _shallIgnoreFirstSignal;

        [DisplayName("Shall Ignore First Signal")]
        public bool ShallIgnoreFirstSignal
        {
            get 
            {
                bool newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "shallIgnoreFirstSignal").Equals("true") ? true : false;
                if (!_shallIgnoreFirstSignal.Equals(newValue))
                {
                    this.ShallIgnoreFirstSignal = newValue;
                }
                return _shallIgnoreFirstSignal;
            }
            set
            {
                if (value != _shallIgnoreFirstSignal)
                {
                    _shallIgnoreFirstSignal = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "shallIgnoreFirstSignal", value.Equals(true) ? "true" : "false");

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ShallIgnoreFirstSignal"));
                }
            }
        }

        public bool _hasAlgorithmParameters;

        [DisplayName("Algorithm with parameters?")]
        public bool HasAlgorithmParameters
        {
            get 
            {
                bool newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "hasAlgorithmParameters").Equals("true") ? true : false;
                if (!_hasAlgorithmParameters.Equals(newValue))
                {
                    this.HasAlgorithmParameters = newValue;
                }
                return _hasAlgorithmParameters;
            }
            set
            {
                if (value != _hasAlgorithmParameters)
                {
                    _hasAlgorithmParameters = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "hasAlgorithmParameters", value.Equals(true) ? "true" : "false");

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("HasAlgorithmParameters"));
                }
            }
        }


        public Dictionary<string, decimal> _parsedAlgorithmParameters;

        public Dictionary<string, decimal> ParsedAlgorithmParameters
        {
            get 
            {
                string tmpAlgorithmParameters = this.AlgorithmParameters;
                return _parsedAlgorithmParameters;
            }
        }

        public string _algorithmParameters;

        public string AlgorithmParameters
        {
            get 
            {
                string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "algorithmParameters");
                if (!_algorithmParameters.Equals(newValue))
                {
                    this.AlgorithmParameters = newValue;
                }
                return _algorithmParameters;
            }
            set
            {
                if(value == null)
                {
                    value = "";
                }
                if (!value.Equals(_algorithmParameters))
                {
                    _algorithmParameters = value;

                    if (value != null)
                    {
                        if (value.Length != 0)
                        {
                            _parsedAlgorithmParameters = this.parseAlgorithmParameters(value);
                        }
                    }

                    XMLHandler.WriteValueToXML(this.EquityAsString, "algorithmParameters", value);

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("AlgorithmParameters"));
                        PropertyChanged(this, new PropertyChangedEventArgs("ParsedAlgorithmParameters"));
                    }
                }
            }
        }

        private string _algorithmFilePath;

        public string AlgorithmFilePath
        {
            get 
            {
                string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "algorithmFilePath");
                if (!_algorithmFilePath.Equals(newValue))
                {
                    this.AlgorithmFilePath = newValue;
                }
                return _algorithmFilePath;
            }
            set
            {
                if (!value.Equals(_algorithmFilePath))
                {
                    _algorithmFilePath = value;

                    XMLHandler.WriteValueToXML(this.EquityAsString, "algorithmFilePath", value);

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("AlgorithmFilePath"));
                }
            }
        }

        private string _exchange;

        public string Exchange
        {
            get
            {
                //if not first call
                if (this._equity != null)
                {
                    string newValue = XMLHandler.ReadValueFromXML(this.EquityAsString, "exchange");
                    if (!_exchange.Equals(newValue))
                    {
                        this.Exchange = newValue;
                    }
                }
                return _exchange;
            }
            set
            {
                //if not first call
                if (this._equity != null)
                {
                    if (!value.Equals(_exchange))
                    {
                        string equityString = this.EquityAsString;
                        this._equity = null;

                        _exchange = value;

                        this.EquityAsString = equityString;

                        XMLHandler.WriteValueToXML(this.EquityAsString, "exchange", value);

                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("Exchange"));
                    }
                }
                else
                {
                    _exchange = value;

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Exchange"));
                }
            }
        }

        public Future ConvertToFutures(string input)
        {
            string expiry = "201" + input.ElementAt(3);
            switch ((input.ElementAt(2) + "").ToUpper())
            {
                case ("Z"):
                    expiry += "12";
                    break;

                case ("Q"): case ("U"):
                    expiry += "09";
                    break;

                case ("H"):
                    expiry += "03";
                    break;

                case ("M"):
                    expiry += "06";
                    break;

                default:
                    break;
            }

            return new Future(input.ElementAt(0) + "" + input.ElementAt(1), this.Exchange, expiry);
        }

        public string ConvertFutureToString(Future input)
        {
            string displayableExpiry = "";
            string month = ("" + input.Expiry.ElementAt(4) + input.Expiry.ElementAt(5));
            switch (month)
            {
                case ("12"):
                    displayableExpiry += "Z";
                    break;

                case ("09"):
                    displayableExpiry += "Q";
                    break;

                case ("03"):
                    displayableExpiry += "H";
                    break;

                case ("06"):
                    displayableExpiry += "M";
                    break;

                default:
                    break;
            }

            displayableExpiry += input.Expiry.ElementAt(3);

            return input.Symbol + displayableExpiry;
        }

        public Dictionary<string, decimal> parseAlgorithmParameters(string rawAlgorithmParameters)
        {
            Dictionary<string, decimal> parameters = new Dictionary<string, decimal>();

            if (rawAlgorithmParameters.Length != 0)
            {
                try
                {
                    string[] separatedAlgorithmParameters = rawAlgorithmParameters.Split('\n');
                    for (int i = 0; i < separatedAlgorithmParameters.Length; i++ )
                    {
                        string[] separatedParameter = separatedAlgorithmParameters[i].Split(',');
                        parameters.Add(separatedParameter[0], decimal.Parse(separatedParameter[1], CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception)
                {
                    this.ConsoleText += this.EquityAsString + ": Exception while parsing parameters.";
                    parameters = null;
                }
            }

            return parameters;
        }
    }
}

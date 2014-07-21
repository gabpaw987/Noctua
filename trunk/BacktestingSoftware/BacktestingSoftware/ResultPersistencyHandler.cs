using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace BacktestingSoftware
{
    public static class ResultPersistencyHandler
    {
        public static string persistencyFilePath = "Results.bts";
        public static string persistencyBackupFilePath = "ResultsBackup.bts";
        private static object WriteResultSetLock = new Object();
        
        public static bool CreateNewPersistencyFile(MainWindow mainWindow)
        {
            lock (WriteResultSetLock)
            {
                try
                {

                    persistencyFilePath = "Results.bts";
                    persistencyBackupFilePath = "ResultsBackup.bts";

                    if (File.Exists(persistencyFilePath))
                    {
                        if (File.Exists(persistencyBackupFilePath))
                        {
                            File.Delete(persistencyBackupFilePath);
                        }
                        File.Move(persistencyFilePath, persistencyBackupFilePath);
                    }

                    Stream stream = File.Open(persistencyFilePath, FileMode.Create);
                    BinaryFormatter bFormatter = new BinaryFormatter();

                    bFormatter.Serialize(stream, mainWindow.mainViewModel.Orders);
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.Signals);
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.IndicatorDictionary);
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.OscillatorDictionary);
                    stream.Close();
                    stream = File.Open(persistencyFilePath, FileMode.Append);

                    List<decimal> tempPerformanceList = new List<decimal>(new decimal[] {
                                   mainWindow.mainViewModel.GainLossPercent,
                                   mainWindow.mainViewModel.GainPercent,
                                   mainWindow.mainViewModel.LossPercent,
                                   mainWindow.mainViewModel.StdDevOfProfit,
                                   mainWindow.mainViewModel.StdDevOfPEquityPrice,
                                   mainWindow.mainViewModel.NoOfGoodTrades,
                                   mainWindow.mainViewModel.NoOfBadTrades,
                                   mainWindow.mainViewModel.GtBtRatio,
                                   mainWindow.mainViewModel.NetWorth,
                                   mainWindow.mainViewModel.PortfolioPerformancePercent,
                                   mainWindow.mainViewModel.SharpeRatio,
                                   mainWindow.mainViewModel.TimeInMarket,
                                   mainWindow.mainViewModel.AnnualizedPortfolioPerformancePercent,
                                   mainWindow.mainViewModel.AnnualizedGainLossPercent,
                                   mainWindow.mainViewModel.GoodDayBadDayRatio});
                    bFormatter.Serialize(stream, tempPerformanceList);

                    List<bool> tempBoolList = new List<bool>(new bool[] {
                                   mainWindow.mainViewModel.IsRealTimeEnabled,
                                   mainWindow.mainViewModel.ShallDrawIndicatorMap,
                                   mainWindow.mainViewModel.ShallDrawOscillatorMap,
                                   mainWindow.mainViewModel.ShallDrawVolume,
                                   mainWindow.mainViewModel.IsDataFutures,
                                   mainWindow.mainViewModel.IsMiniContract,
                                   mainWindow.mainViewModel.IsNetWorthChartInPercentage,
                                   mainWindow.mainViewModel.UseRegularTradingHours,
                                   mainWindow.mainViewModel.IsFullFuturePriceData});
                    bFormatter.Serialize(stream, tempBoolList);

                    List<string> tempStringList = new List<string>(new string[] { 
                                   mainWindow.mainViewModel.AlgorithmFileName,
                                   mainWindow.mainViewModel.DataFileName,
                                   mainWindow.mainViewModel.Capital,
                                   mainWindow.mainViewModel.AbsTransactionFee,
                                   mainWindow.mainViewModel.RelTransactionFee,
                                   mainWindow.mainViewModel.PricePremium,
                                   mainWindow.mainViewModel.StockSymbolForRealTime,
                                   mainWindow.mainViewModel.Barsize,
                                   mainWindow.mainViewModel.AdditionalParameters});
                    bFormatter.Serialize(stream, tempStringList);

                    List<DateTime> tempDateList = new List<DateTime>(new DateTime[] {
                                   mainWindow.mainViewModel.StartDate,
                                   mainWindow.mainViewModel.EndDate});
                    bFormatter.Serialize(stream, tempDateList);

                    List<int> tempIntList = new List<int>(new int[] {
                                   mainWindow.mainViewModel.ValueOfSliderOne,
                                   mainWindow.mainViewModel.ValueOfSliderTwo,
                                   mainWindow.mainViewModel.ValueOfSliderThree,
                                   mainWindow.mainViewModel.ValueOfSliderFour,
                                   mainWindow.mainViewModel.ValueOfSliderFive,
                                   mainWindow.mainViewModel.ValueOfSliderSix,
                                   mainWindow.mainViewModel.RoundLotSize,
                                   mainWindow.mainViewModel.InnerValue,
                                   mainWindow.mainViewModel.MiniContractFactor,
                                   mainWindow.mainViewModel.CalculationThreadCount,
                                   mainWindow.mainViewModel.NoOfGoodDays,
                                   mainWindow.mainViewModel.NoOfBadDays});
                    bFormatter.Serialize(stream, tempIntList);

                    StringCollection serializableStackPanels = new StringCollection();
                    serializableStackPanels = mainWindow.storeIndicatorStackPanels(mainWindow.mainViewModel.IndicatorPanels);
                    bFormatter.Serialize(stream, serializableStackPanels);

                    stream.Close();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool UpdateBestSet(MainWindow mainWindow)
        {
            lock (WriteResultSetLock)
            {
                try
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    Stream stream = File.Open(persistencyFilePath, FileMode.Open);

                    var tmp = (List<Order>)bFormatter.Deserialize(stream);
                    var tmp1 = (List<int>)bFormatter.Deserialize(stream);
                    var tmp2 = (Dictionary<string, List<decimal>>)bFormatter.Deserialize(stream);
                    var tmp3 = (Dictionary<string, List<decimal>>)bFormatter.Deserialize(stream);

                    int bestSetOffset = (int)stream.Position;
                    stream.Close();

                    byte[] bytes = File.ReadAllBytes(persistencyFilePath);
                    File.Delete(persistencyFilePath);

                    stream = File.Open(persistencyFilePath, FileMode.Create);
                                        
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.Orders);
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.Signals);
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.IndicatorDictionary);
                    bFormatter.Serialize(stream, mainWindow.mainViewModel.OscillatorDictionary);

                    stream.Write(bytes, bestSetOffset, bytes.Length - bestSetOffset);

                    stream.Close();
                    
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool WriteResultSet(string key, CalculationResultSet resultSet)
        {
            lock (WriteResultSetLock)
            {
                try
                {
                    Stream stream = File.Open(persistencyFilePath, FileMode.Append);
                    BinaryFormatter bFormatter = new BinaryFormatter();

                    bFormatter.Serialize(stream, key);
                    bFormatter.Serialize(stream, resultSet);

                    stream.Close();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool SaveBackup(string newFilePath)
        {
            try
            {
                File.Copy(persistencyFilePath, newFilePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool LoadPersistencyFile(MainWindow mainWindow)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Do you want to apply the paths from this file?", "Apply Paths", MessageBoxButton.YesNo);

                Stream stream = File.Open(mainWindow.mainViewModel.LoadFileName, FileMode.Open);
                BinaryFormatter bFormatter = new BinaryFormatter();

                mainWindow.resetCalculation(true);

                mainWindow.mainViewModel.Orders = (List<Order>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.Signals = (List<int>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.IndicatorDictionary = (Dictionary<string, List<decimal>>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.OscillatorDictionary = (Dictionary<string, List<decimal>>)bFormatter.Deserialize(stream);

                List<decimal> tempPerformanceList = (List<decimal>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.GainLossPercent = tempPerformanceList[0];
                mainWindow.mainViewModel.GainPercent = tempPerformanceList[1];
                mainWindow.mainViewModel.LossPercent = tempPerformanceList[2];
                mainWindow.mainViewModel.StdDevOfProfit = tempPerformanceList[3];
                mainWindow.mainViewModel.StdDevOfPEquityPrice = tempPerformanceList[4];
                mainWindow.mainViewModel.NoOfGoodTrades = tempPerformanceList[5];
                mainWindow.mainViewModel.NoOfBadTrades = tempPerformanceList[6];
                mainWindow.mainViewModel.GtBtRatio = tempPerformanceList[7];
                mainWindow.mainViewModel.NetWorth = tempPerformanceList[8];
                mainWindow.mainViewModel.PortfolioPerformancePercent = tempPerformanceList[9];
                mainWindow.mainViewModel.SharpeRatio = tempPerformanceList[10];
                mainWindow.mainViewModel.TimeInMarket = tempPerformanceList[11];
                mainWindow.mainViewModel.AnnualizedPortfolioPerformancePercent = tempPerformanceList[12];
                mainWindow.mainViewModel.AnnualizedGainLossPercent = tempPerformanceList[13];
                mainWindow.mainViewModel.GoodDayBadDayRatio = tempPerformanceList[14];

                List<bool> tempBoolList = (List<bool>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.IsRealTimeEnabled = tempBoolList[0];
                mainWindow.mainViewModel.ShallDrawIndicatorMap = tempBoolList[1];
                mainWindow.mainViewModel.ShallDrawOscillatorMap = tempBoolList[2];
                mainWindow.mainViewModel.ShallDrawVolume = tempBoolList[3];
                mainWindow.mainViewModel.IsDataFutures = tempBoolList[4];
                mainWindow.mainViewModel.IsMiniContract = tempBoolList[5];
                mainWindow.mainViewModel.IsNetWorthChartInPercentage = tempBoolList[6];
                mainWindow.mainViewModel.UseRegularTradingHours = tempBoolList[7];
                mainWindow.mainViewModel.IsFullFuturePriceData = tempBoolList[8];


                List<string> tempStringList = (List<string>)bFormatter.Deserialize(stream);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    mainWindow.mainViewModel.AlgorithmFileName = tempStringList[0];
                    mainWindow.mainViewModel.DataFileName = tempStringList[1];
                }
                mainWindow.mainViewModel.Capital = tempStringList[2];
                mainWindow.mainViewModel.AbsTransactionFee = tempStringList[3];
                mainWindow.mainViewModel.RelTransactionFee = tempStringList[4];
                mainWindow.mainViewModel.PricePremium = tempStringList[5];
                mainWindow.mainViewModel.StockSymbolForRealTime = tempStringList[6];
                mainWindow.mainViewModel.Barsize = tempStringList[7];
                mainWindow.mainViewModel.AdditionalParameters = tempStringList[8];

                List<DateTime> tempDateList = (List<DateTime>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.StartDate = tempDateList[0];
                mainWindow.mainViewModel.EndDate = tempDateList[1];

                List<int> tempIntList = (List<int>)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.ValueOfSliderOne = tempIntList[0];
                mainWindow.mainViewModel.ValueOfSliderTwo = tempIntList[1];
                mainWindow.mainViewModel.ValueOfSliderThree = tempIntList[2];
                mainWindow.mainViewModel.ValueOfSliderFour = tempIntList[3];
                mainWindow.mainViewModel.ValueOfSliderFive = tempIntList[4];
                mainWindow.mainViewModel.ValueOfSliderSix = tempIntList[5];
                mainWindow.mainViewModel.RoundLotSize = tempIntList[6];
                mainWindow.mainViewModel.InnerValue = tempIntList[7];
                mainWindow.mainViewModel.MiniContractFactor = tempIntList[8];
                mainWindow.mainViewModel.CalculationThreadCount = tempIntList[9];
                mainWindow.mainViewModel.NoOfGoodDays = tempIntList[10];
                mainWindow.mainViewModel.NoOfBadDays = tempIntList[11];

                StringCollection serializableStackPanels = (StringCollection)bFormatter.Deserialize(stream);
                mainWindow.mainViewModel.IndicatorPanels = mainWindow.restoreIndicatorStackPanels(serializableStackPanels);
                mainWindow.refreshIndicatorList();

                Calculator c = new Calculator(mainWindow.mainViewModel);
                Tuple<Dictionary<string,List<Decimal>>, List<List<decimal>>> meshedParameters = c.ReadAndMeshParameters();

                while (stream.Position < stream.Length)
                {
                    mainWindow.StatusLabel.Text = "Loading... (" + mainWindow.mainViewModel.CalculationResultSets.Count + "/" + meshedParameters.Item2[0].Count + ")";

                    mainWindow.mainViewModel.CalculationResultSets.Add((string)bFormatter.Deserialize(stream),
                                                                       (CalculationResultSet)bFormatter.Deserialize(stream));
                }

                mainWindow.mainViewModel.SaveFileName = mainWindow.mainViewModel.LoadFileName;

                stream.Close();

                mainWindow.StatusLabel.Text = "Finished Loading... (" + mainWindow.mainViewModel.CalculationResultSets.Count + "/" + meshedParameters.Item2[0].Count + ")";

                //To draw the chart in case everything has been calculated already
                if (mainWindow.mainViewModel.CalculationResultSets.Count == meshedParameters.Item2[0].Count)
                {
                    mainWindow.StartButton_Click(null, null);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

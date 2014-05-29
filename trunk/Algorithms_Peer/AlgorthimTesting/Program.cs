using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AlgorthimTesting
{
    class Program
    {
        // SWITCHES BETWEEN ALL PRICES AT ONCE (false) OR REAL TIME DEMO (one value at a time) (true)
        public const bool realTimeDemo = false;

        static void Main(string[] args)
        {
            //"C:/noctua/trunk/Input_Data/NKD_1mBar_20110809.csv"
            //"C:/noctua/trunk/Input_Data/GOOG_1dBar_20130110.csv"
            //"C:/noctua/trunk/Input_Data/GOOG_1mBar_20130110.csv"
            //"C:/noctua/trunk/Input_Data/SPX_1dBar_20130220.csv"
            //"C:/noctua/trunk/Input_Data/INTC_1dBar_20130220.csv"
            //"C:/noctua/trunk/Input_Data/NQ_U3_EMINI_NASDAQ_100.csv"
            //NQ_U3_EMINI_NASDAQ_100_cropped
            //"C:/Dropbox/Diplomprojekt/CAD_1mBar_20110924.csv"
            Console.WriteLine("File einlesen");
            List<Tuple<DateTime, decimal, decimal, decimal, decimal>> prices;
            prices = CSVReader.EnumerateExcelFile("D:/noctua/trunk/Input_Data/NQ_U3_EMINI_NASDAQ_100_cropped.csv", new DateTime(), DateTime.Now).ToList();
            Console.WriteLine("Algorithmus starten");

            List<int> signals = new List<int>();

            // Trading Algo
            if (Program.realTimeDemo)
            {
                // first value
                Algorithm.DecisionCalculator.startCalculation(prices.GetRange(0, 1500), signals, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>());//, new System.Collections.Generic.Dictionary<string, decimal>());
                while (signals.Count < prices.Count)
                    Algorithm.DecisionCalculator.startCalculation(prices.GetRange(0, signals.Count + 1), signals, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>());//, new System.Collections.Generic.Dictionary<string, decimal>());
            }
            else
            {
                signals = Algorithm.DecisionCalculator.startCalculation(prices, signals, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>());//, new System.Collections.Generic.Dictionary<string, decimal>());
            }

            // BTS Algo
            //if (Program.realTimeDemo)
            //{
            //    // first value
            //    Algorithm.DecisionCalculator.startCalculation(prices.GetRange(0, 1500), signals, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, decimal>());
            //    while (signals.Count < prices.Count)
            //        Algorithm.DecisionCalculator.startCalculation(prices.GetRange(0, signals.Count + 1), signals, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, decimal>());
            //}
            //else
            //{
            //    signals = Algorithm.DecisionCalculator.startCalculation(prices, signals, new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>(), new System.Collections.Generic.Dictionary<string, decimal>());
            //}
            
            //for (int i = 0; i < test.Count; i++)
            //    Console.WriteLine(test.ElementAt(i));
            if (prices.Count == signals.Count)
                Console.WriteLine("Algorithmus passt");
            else
                Console.WriteLine("Passt gar nicht sollte sein:" + prices.Count + " ist aber:" + signals.Count + "");
            Console.Read();
        }
    }
}

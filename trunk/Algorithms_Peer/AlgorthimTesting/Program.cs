using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AlgorthimTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            //"C:/noctua/trunk/Input_Data/NKD_1mBar_20110809.csv"
            //"C:/noctua/trunk/Input_Data/GOOG_1dBar_20130110.csv"
            //"C:/noctua/trunk/Input_Data/SPX_1dBar_20130220.csv"
            //"C:/noctua/trunk/Input_Data/INTC_1dBar_20130220.csv"
            //"C:/Dropbox/Diplomprojekt/CAD_1mBar_20110924.csv"
            Console.WriteLine("File einlesen");
            List<Tuple<DateTime, decimal, decimal, decimal, decimal>> asd;
            asd = CSVReader.EnumerateExcelFile("C:/noctua/trunk/Input_Data/SPX_1dBar_20130220.csv", new DateTime(), DateTime.Now).ToList();
            Console.WriteLine("Algorithmus starten");
            // = new List<decimal>();
            // 
            List<int> test = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                //test.Add(1);
            }
            test = Algorithm.DecisionCalculator.startCalculation(asd, test);
            for (int i = 0; i < test.Count; i++)
                Console.WriteLine(test.ElementAt(i));
            if (asd.Count == test.Count)
                Console.WriteLine("Algorithmus passt");
            else
                Console.WriteLine("Passt gar nicht sollte sein:" + asd.Count + " ist aber:" + test.Count + "");
            Console.Read();
        }
    }
}

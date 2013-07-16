using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AlgorthimTesting
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/NKD_1mBar_20110809.csv"
            //"C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/GOOG_1dBar_20130110.csv"
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/SPX_1dBar_20130220.csv"
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/GOOG_1mBar_20130110.csv"
            //"C:\Users\Josefs\Documents\Schule\PPM\noctua\trunk\Input_Data\GOOG_1mBar_20130110.csv"
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/INTC_1dBar_20130220.csv"
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/bitcoins.csv"
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/INTC_1dBar_20130220kurz.csv"
            Console.WriteLine("File einlesen");
            List<Tuple<DateTime, decimal, decimal, decimal, decimal>> asd;
            asd = CSVReader.EnumerateExcelFile("C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/GOOG_1mBar_20130110.csv", new DateTime(), DateTime.Now).ToList();
            Console.WriteLine("Algorithmus starten");

            // = new List<decimal>();
            //
            List<int> test = new List<int>();
            test = Algorithm.DecisionCalculator.startCalculation(asd, test);

            //for (int i = 0; i < test.Count; i++)
            //{
            //    Console.WriteLine(test.ElementAt(i));
            //}
            //if (asd.Count == test.Count)
            //    Console.WriteLine("Algorithmus passt");

            //else
            //Console.WriteLine("Passt gar nicht sollte sein:" + asd.Count + " ist aber:" + test.Count + "");
            Console.Read();
        }
    }
}
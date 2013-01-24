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
            //"C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/NKD_1mBar_20110809.csv"
            //"C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/GOOG_1dBar_20130110.csv"
            Console.WriteLine("File einlesen");
            List<Tuple<DateTime, decimal, decimal, decimal, decimal>> asd;
            asd = CSVReader.EnumerateExcelFile("C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/GOOG_1dBar_20130110.csv", new DateTime(), DateTime.Now).ToList();
            Console.WriteLine("Algorithmus starten");
            // = new List<decimal>();
            // 
            List<int> test = new List<int>() ;
            test =  Algorithm.DecisionCalculator.startCalculation(2,asd,test);
            for (int i = 0; i < test.Count;i++ )
                Console.WriteLine(test.ElementAt(i));
            Console.Read();
            
        }
    }
}

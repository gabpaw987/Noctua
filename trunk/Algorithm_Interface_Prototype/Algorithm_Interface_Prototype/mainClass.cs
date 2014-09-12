using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Algorithm_Interface_Prototype
{
    internal static class mainClass
    {
        public static void startCalculation(string csvFileName, string algorithmFileName)
        {
            IEnumerable<Bar> barList = CSVReader.EnumerateExcelFile(csvFileName);

            List<double> dl = new List<double>();
            foreach (Bar b in barList)
            {
                dl.Add((double)b.Close);
            }
            Assembly assembly = Assembly.LoadFrom(algorithmFileName);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            Type t = assembly.GetType("NuTrade.Algorithm.DecisionCalculator");

            Object[] oa = { dl, 2, 4 };

            var signal = t.GetMethod("buyDecision").Invoke(null, oa);

            MessageBox.Show("" + signal);
        }
    }
}
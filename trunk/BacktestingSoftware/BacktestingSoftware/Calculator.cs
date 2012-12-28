using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BacktestingSoftware
{
    internal class Calculator
    {
        private MainViewModel mainViewModel { get; set; }

        public List<Tuple<DateTime, double, double, double, double>> barList { get; private set; }

        public List<int> signals { get; private set; }

        public Calculator(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.signals = new List<int>();
        }

        public void Start()
        {
            this.barList = CSVReader.EnumerateExcelFile(this.mainViewModel.DataFileName, this.mainViewModel.StartDate, this.mainViewModel.EndDate).ToList();

            Assembly assembly = Assembly.LoadFrom(this.mainViewModel.AlgorithmFileName);
            AppDomain.CurrentDomain.Load(assembly.GetName());
            Type t = assembly.GetType("Algorithm.DecisionCalculator");

            Object[] oa = { 90, barList, this.signals };

            var signals = t.GetMethod("startCalculation").Invoke(null, oa);
        }
    }
}
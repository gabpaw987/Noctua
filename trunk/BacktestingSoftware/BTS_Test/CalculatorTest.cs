using System;
using System.Collections.Generic;
using BacktestingSoftware;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BTS_Test
{
    [TestClass]
    public class CalculatorTest
    {
        Calculator target;

        public CalculatorTest()
        {
            MainViewModel mainViewModel = new MainViewModel();
            mainViewModel.Capital = "100000";
            mainViewModel.AlgorithmFileName = @"C:\Users\Gabriel\Documents\Schule\PPM\Noctua\trunk\Input_Data\wma1090.dll";
            mainViewModel.DataFileName = @"C:\Users\Gabriel\Documents\Schule\PPM\Noctua\trunk\Input_Data\GOOG_1dBar_20130110.csv";
            target = new Calculator(mainViewModel);
        }

        /// <summary>
        ///A test for "ReadFile"
        ///</summary>
        [TestMethod()]
        public void ReadFileTest()
        {
            target.ReadFile();
        }

        /// <summary>
        ///A test for "CalculateSignals"
        ///</summary>
        [TestMethod()]
        public void CalculateSignalsTest()
        {
            target.ReadFile();
            Type t = target.LoadAlgorithmFile();
            target.CalculateSignals(t, null);
        }

        /// <summary>
        ///A test for "CalculateNumbers"
        ///</summary>
        [TestMethod()]
        public void CalculateNumbersTest()
        {
            target.ReadFile();
            Type t = target.LoadAlgorithmFile();
            target.CalculateSignals(t, null);
            string expected = string.Empty;
            string actual;
            actual = target.CalculateNumbers(string.Empty);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for "CalculateStdDevs"
        ///</summary>
        [TestMethod()]
        [DeploymentItem("BacktestingSoftware.exe")]
        public void CalculateStdDevsTest()
        {
            PrivateObject param0 = null; // TODO: Passenden Wert initialisieren
            Calculator_Accessor target = new Calculator_Accessor(param0); // TODO: Passenden Wert initialisieren
            IEnumerable<double> values = null; // TODO: Passenden Wert initialisieren
            double expected = 0F; // TODO: Passenden Wert initialisieren
            double actual;
            actual = target.CalculateStdDevs(values);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Überprüfen Sie die Richtigkeit dieser Testmethode.");
        }
    }
}
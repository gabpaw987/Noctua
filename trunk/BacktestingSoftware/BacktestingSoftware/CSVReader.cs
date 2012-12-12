using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BacktestingSoftware
{
    /// <summary>
    /// This is a Reader that reads Historical Bar data from .csv files and adds them to n IEnumerable filled with Bars.
    /// </summary>
    /// <remarks></remarks>
    internal static class CSVReader
    {
        /// <summary>
        /// Enumerates the excel file, which contains the informations for the bars.
        /// </summary>
        /// <param name="filePath">The excel file path.</param>
        /// <returns>An enumeration of bars.</returns>
        /// <remarks></remarks>
        public static IEnumerable<Bar> EnumerateExcelFile(string filePath)
        {
            // Enumerate all lines, but skip the header
            return from line in File.ReadLines(filePath).Skip(1)
                   select line.Split(',')
                       into fields
                       let timeStamp = DateTime.ParseExact(fields[1] + fields[2], "d", new CultureInfo("en-US"))
                       let open = Decimal.Parse(fields[3])
                       let high = Decimal.Parse(fields[4])
                       let low = Decimal.Parse(fields[5])
                       let close = Decimal.Parse(fields[6])
                       select new Bar(timeStamp, open, high, low, close);
        }
    }
}
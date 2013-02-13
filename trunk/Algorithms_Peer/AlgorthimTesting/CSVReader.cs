using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AlgorthimTesting
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
        public static IEnumerable<Tuple<DateTime, decimal, decimal, decimal, decimal>> EnumerateExcelFile(string filePath, DateTime startDate, DateTime endDate)
        {
            // Enumerate all lines, but skip the header
            return from line in File.ReadLines(filePath).Skip(1)
                   select line.Split(',')
                       into fields
                       let timeStamp = DateTime.ParseExact(fields[1] + " " + fields[2], "MM/dd/yy HH:mm", new CultureInfo("en-US"))
                       let open = decimal.Parse(fields[3],CultureInfo.InvariantCulture)
                       let high = decimal.Parse(fields[4],CultureInfo.InvariantCulture)
                       let low = decimal.Parse(fields[5], CultureInfo.InvariantCulture)
                       let close = decimal.Parse(fields[6], CultureInfo.InvariantCulture)
                       where timeStamp.Date >= startDate.Date && timeStamp.Date <= endDate.Date
                       select new Tuple<DateTime, decimal, decimal, decimal, decimal>(timeStamp, open, high, low, close);
        }
    }
}
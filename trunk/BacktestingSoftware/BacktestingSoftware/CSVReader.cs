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
        public static IEnumerable<Tuple<DateTime, decimal, decimal, decimal, decimal>> EnumerateExcelFile(string filePath, DateTime startDate, DateTime endDate)
        {
            // Enumerate all lines, but skip the header
            return from line in File.ReadLines(filePath).Skip(1)
                   select line.Split(',')
                       into fields
                       let timeStamp = parseBarDateTime(fields[1], fields[2])
                       let open = decimal.Parse(fields[3], CultureInfo.InvariantCulture)
                       let high = decimal.Parse(fields[4], CultureInfo.InvariantCulture)
                       let low = decimal.Parse(fields[5], CultureInfo.InvariantCulture)
                       let close = decimal.Parse(fields[6], CultureInfo.InvariantCulture)
                       where timeStamp.Date >= startDate.Date && timeStamp.Date <= endDate.Date
                       select new Tuple<DateTime, decimal, decimal, decimal, decimal>(timeStamp, open, high, low, close);
        }

        public static DateTime parseBarDateTime(string date, string time)
        {
            string[] dateValues = date.Split('/');
            string[] timeValues = time.Split(':');

            DateTime timeOfBar = DateTime.MinValue;

            timeOfBar = timeOfBar.AddYears(int.Parse(dateValues[2]) - 1 + 2000);
            timeOfBar = timeOfBar.AddMonths(int.Parse(dateValues[0]) - 1);
            timeOfBar = timeOfBar.AddDays(int.Parse(dateValues[1]) - 1);

            timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]));
            timeOfBar = timeOfBar.AddMinutes(int.Parse(timeValues[1]));

            return timeOfBar;
        }

        //Currently not in use
        public static IEnumerable<Tuple<DateTime, decimal, decimal, decimal, decimal>> EnumerateExcelFile1(string filePath, DateTime startDate, DateTime endDate)
        {
            List<Tuple<DateTime, decimal, decimal, decimal, decimal>> resultList = new List<Tuple<DateTime, decimal, decimal, decimal, decimal>>();

            StreamReader reader = new StreamReader(File.OpenRead(filePath));
            string line = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                if (!String.IsNullOrWhiteSpace(line))
                {
                    string[] values = line.Split(',');
                    if (values != null)
                    {
                        string[] dateValues = values[1].Split('/');
                        string[] timeValues = values[2].Split(':');
                        DateTime timeOfBar = DateTime.MinValue;
                        timeOfBar = timeOfBar.AddYears(int.Parse(dateValues[2]) - 1 + 2000);
                        timeOfBar = timeOfBar.AddMonths(int.Parse(dateValues[0]) - 1);
                        timeOfBar = timeOfBar.AddDays(int.Parse(dateValues[1]) - 1);

                        timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]));
                        timeOfBar = timeOfBar.AddMinutes(int.Parse(timeValues[1]));

                        if (timeOfBar.Ticks > startDate.Ticks && timeOfBar.Ticks < endDate.Ticks)
                        {
                            resultList.Add(Tuple.Create(
                                timeOfBar,
                                decimal.Parse(values[3], CultureInfo.InvariantCulture),
                                decimal.Parse(values[4], CultureInfo.InvariantCulture),
                                decimal.Parse(values[5], CultureInfo.InvariantCulture),
                                decimal.Parse(values[6], CultureInfo.InvariantCulture)));
                        }
                    }
                }
            }
            return resultList;
        }
    }
}
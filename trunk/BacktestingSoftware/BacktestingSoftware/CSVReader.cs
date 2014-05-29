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
        public static IEnumerable<Tuple<DateTime, decimal, decimal, decimal, decimal>> EnumerateExcelFile(string filePath, DateTime startDate, DateTime endDate, bool isFullFuturePriceData, int innerValue, bool isDataFromESignal11)
        {
            // Enumerate all lines, but skip the header
            return from line in File.ReadLines(filePath).Skip(1)
                   select line.Split(',')
                       into fields
                       let timeStamp = parseBarDateTime(fields[isDataFromESignal11 ? 0 : 1], fields[isDataFromESignal11 ? 1 : 2], isDataFromESignal11)
                       let open = decimal.Parse(fields[isDataFromESignal11 ? 2 : 3], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let high = decimal.Parse(fields[isDataFromESignal11 ? 3 : 4], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let low = decimal.Parse(fields[isDataFromESignal11 ? 4 : 5], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let close = decimal.Parse(fields[isDataFromESignal11 ? 5 : 6], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       where timeStamp.Date >= startDate.Date && timeStamp.Date <= endDate.Date
                       select new Tuple<DateTime, decimal, decimal, decimal, decimal>(timeStamp, open, high, low, close);
        }

        public static DateTime parseBarDateTime(string date, string time, bool isDataFromESignal11)
        {
            string[] dateValues = date.Split('/');
            string[] timeValues = time.Split(':');

            DateTime timeOfBar = DateTime.MinValue;

            timeOfBar = timeOfBar.AddYears(int.Parse(dateValues[2]) - 1 + (isDataFromESignal11 ? 0 : 2000));
            timeOfBar = timeOfBar.AddMonths(int.Parse(dateValues[0]) - 1);
            timeOfBar = timeOfBar.AddDays(int.Parse(dateValues[1]) - 1);

            //Convert the eSignal 11's AM-PM 12 hour clock to our 24 hour clock
            if (isDataFromESignal11)
            {
                if (timeValues[2].ToCharArray()[3].Equals('P') && !timeValues[0].Equals("12"))
                {
                    timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]) + 12);
                }
                else if (timeValues[2].ToCharArray()[3].Equals('A') && timeValues[0].Equals("12"))
                {
                    timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]) - 12);                    
                }
                else
                {
                    timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]));
                }
            }
            else
            {
                timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]));

            }
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BacktestingSoftware
{
    /// <summary>
    /// This is a Reader that reads Historical Bar data from .csv files and adds them to an IEnumerable filled with Bars.
    /// </summary>
    /// <remarks></remarks>
    internal static class CSVReader
    {
        /// <summary>
        /// Enumerates the csv file, which contains the informations for the bars.
        /// </summary>
        /// <param name="filePath">The csv file path.</param>
        /// <returns>An enumeration of bars.</returns>
        /// <remarks></remarks>
        public static IEnumerable<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> EnumerateExcelFile(string filePath, DateTime startDate, DateTime endDate, bool isFullFuturePriceData, int innerValue)
        {
            char splitter = ',';
            //Indices: date, time, open, high, low, close, volume
            int[] indices = new int[7];
            for(int i = 0; i < indices.Length; i++)
            {
                indices[i] = -100;
            }
            bool isEsignal11DateTimeFormat = false;

            //get file structure
            StreamReader reader = new StreamReader(File.OpenRead(filePath));
            string header = reader.ReadLine();
            if (!String.IsNullOrWhiteSpace(header))
            {
                if (header.Count(c => c == ',') > header.Count(c => c == ';'))
                {
                    splitter = ',';
                }
                else if (header.Count(c => c == ';') > header.Count(c => c == ','))
                {
                    splitter = ';';
                }

                string[] headerValues = header.Split(splitter);

                for (int i = 0; i < headerValues.Length; i++)
                {
                    switch (headerValues[i].ToLower())
                    {
                        case "date":
                            indices[0] = i;
                            break;
                        case "time":
                            indices[1] = i;
                            break;
                        case "open":
                            indices[2] = i;
                            break;
                        case "high":
                            indices[3] = i;
                            break;
                        case "low":
                            indices[4] = i;
                            break;
                        case "close": case "schließen":
                            indices[5] = i;
                            break;
                        case "volume": case "vol":
                            indices[6] = i;
                            break;
                    }
                }
            }

            string firstLine = reader.ReadLine();
            if (!String.IsNullOrWhiteSpace(header))
            {
                string[] firstLineValues = firstLine.Split(splitter);
                if (firstLineValues[indices[1]].Contains("PM") || firstLineValues[indices[1]].Contains("AM"))
                {
                    isEsignal11DateTimeFormat = true;
                }
            }

            // Enumerate all lines, but skip the header
            return from line in File.ReadLines(filePath).Skip(1)
                   select line.Split(splitter)
                       into fields
                       let timeStamp = indices[2] == -100 ? new DateTime() : parseBarDateTime(fields[indices[0]], fields[indices[1]], isEsignal11DateTimeFormat)
                       let open = indices[2] == -100 ? 0 : decimal.Parse(fields[indices[2]], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let high = indices[3] == -100 ? 0 : decimal.Parse(fields[indices[3]], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let low = indices[4] == -100 ? 0 : decimal.Parse(fields[indices[4]], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let close = indices[5] == -100 ? 0 : decimal.Parse(fields[indices[5]], CultureInfo.InvariantCulture) / (isFullFuturePriceData ? innerValue : 1)
                       let volume = indices[6] == -100 ? 0 : long.Parse(fields[indices[6]], CultureInfo.InvariantCulture)
                       where timeStamp.Date >= startDate.Date && timeStamp.Date <= endDate.Date
                       select new Tuple<DateTime, decimal, decimal, decimal, decimal, long>(timeStamp, open, high, low, close, volume);
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
    }
}
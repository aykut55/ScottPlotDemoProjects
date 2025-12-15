using OoplesFinance.StockIndicators.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AlgoTradeWithScottPlot
{
    public struct StockData
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; } // Changed to long for integer representation
        public long Size { get; set; } // Represents the 'Lot' value
    }

    public class DataReader
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _readCount;

        public int ReadCount => _readCount;

        public void StartTimer()
        {
            _stopwatch.Start();
        }

        public void StopTimer()
        {
            _stopwatch.Stop();
        }

        public TimeSpan GetElapsedTime()
        {
            return _stopwatch.Elapsed;
        }

        public long GetElapsedTimeMsec()
        {
            return _stopwatch.ElapsedMilliseconds;
        }

        public void Clear()
        {
            _stopwatch.Reset();
            _readCount = 0;
        }

        public List<StockData> ReadData(string filePath)
        {
            var data = new List<StockData>();
            var culture = new CultureInfo("tr-TR"); // For parsing numbers with comma as decimal separator

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified data file was not found.", filePath);
            }

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // Skip header, comments, or empty lines
                if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#") || line.Trim().StartsWith("Id"))
                {
                    continue;
                }

                var parts = line.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

                // Expected format: Id ; Date Time ; Open ; High ; Low ; Close ; Volume ; Lot
                if (parts.Length < 8)
                {
                    // Log or ignore malformed lines
                    Console.WriteLine($"Skipping malformed line: {line}");
                    continue;
                }

                try
                {
                    var dateTimePart = parts[1];
                    
                    // Check if date and time are in separate fields
                    if (parts.Length >= 9 && parts[1].Length == 10 && parts[2].Contains(":"))
                    {
                        // Format: parts[1] = "2013.07.12", parts[2] = "09:30:00"
                        dateTimePart = $"{parts[1]} {parts[2]}";
                        // Shift other parts indices by 1
                        var newParts = new string[parts.Length - 1];
                        newParts[0] = parts[0]; // Id
                        newParts[1] = dateTimePart; // Combined datetime
                        Array.Copy(parts, 3, newParts, 2, parts.Length - 3); // Rest of the data
                        parts = newParts;
                    }
                    else if (dateTimePart.Contains(";"))
                    {
                        // Format: "2007.06.11;13:15:00"
                        dateTimePart = dateTimePart.Replace(";", " ");
                    }
                    
                    var dateTime = DateTime.ParseExact(dateTimePart, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    
                    var stockData = new StockData
                    {
                        Id = int.Parse(parts[0]),
                        DateTime = dateTime,
                        Date = dateTime.Date,
                        Time = dateTime.TimeOfDay,
                        Open = double.Parse(parts[2], culture),
                        High = double.Parse(parts[3], culture),
                        Low = double.Parse(parts[4], culture),
                        Close = double.Parse(parts[5], culture),
                        Volume = long.Parse(parts[6], NumberStyles.Any, culture), // Changed to long.Parse
                        Size = int.Parse(parts[7], NumberStyles.Any, culture)
                    };
                    data.Add(stockData);
                }
                catch (FormatException ex)
                {
                    // Log or handle parsing errors
                    Console.WriteLine($"Could not parse line: '{line}'. Error: {ex.Message}");
                }
            }

            _readCount = data.Count;
            return data;
        }

        public List<StockData> _rdFstFl(string filePath)
        {
            var culture = new CultureInfo("tr-TR");
            var bag = new System.Collections.Concurrent.ConcurrentBag<StockData>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified data file was not found.", filePath);
            }

            File.ReadLines(filePath)
                .AsParallel()
                .Where(line => !(string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#") || line.Trim().StartsWith("Id")))
                .ForAll(line =>
                {
                    var parts = line.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

                    if (parts.Length < 8)
                    {
                        return; // Skip malformed lines
                    }

                    try
                    {
                        var dateTimePart = parts[1];

                        // Check if date and time are in separate fields
                        if (parts.Length >= 9 && parts[1].Length == 10 && parts[2].Contains(":"))
                        {
                            // Format: parts[1] = "2013.07.12", parts[2] = "09:30:00"
                            dateTimePart = $"{parts[1]} {parts[2]}";
                            // Shift other parts indices by 1
                            var newParts = new string[parts.Length - 1];
                            newParts[0] = parts[0]; // Id
                            newParts[1] = dateTimePart; // Combined datetime
                            Array.Copy(parts, 3, newParts, 2, parts.Length - 3); // Rest of the data
                            parts = newParts;
                        }
                        else if (dateTimePart.Contains(";"))
                        {
                            // Format: "2007.06.11;13:15:00"
                            dateTimePart = dateTimePart.Replace(";", " ");
                        }

                        var dateTime = DateTime.ParseExact(dateTimePart, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);

                        var stockData = new StockData
                        {
                            Id = int.Parse(parts[0]),
                            DateTime = dateTime,
                            Date = dateTime.Date,
                            Time = dateTime.TimeOfDay,
                            Open = double.Parse(parts[2], culture),
                            High = double.Parse(parts[3], culture),
                            Low = double.Parse(parts[4], culture),
                            Close = double.Parse(parts[5], culture),
                            Volume = long.Parse(parts[6], NumberStyles.Any, culture),
                            Size = long.Parse(parts[7], NumberStyles.Any, culture)
                        };
                        bag.Add(stockData);
                    }
                    catch (FormatException ex)
                    {
                        // This might be noisy in parallel. Consider a different logging strategy for production.
                        Console.WriteLine($"Could not parse line: '{line}'. Error: {ex.Message}");
                    }
                });

            // Convert to list and sort to maintain original order, as parallel processing is non-deterministic.
            var sortedList = bag.ToList();
            sortedList.Sort((x, y) => x.Id.CompareTo(y.Id));
            _readCount = sortedList.Count;
            var stockData = sortedList[sortedList.Count - 1];
            return sortedList;
        }

        private static string ParseDateTimePart(string[] parts)
        {
            var dateTimePart = parts[1];
            
            // Check if date and time are in separate fields
            if (parts.Length >= 9 && parts[1].Length == 10 && parts[2].Contains(":"))
            {
                // Format: parts[1] = "2013.07.12", parts[2] = "09:30:00"
                dateTimePart = $"{parts[1]} {parts[2]}";
            }
            else if (dateTimePart.Contains(";"))
            {
                // Format: "2007.06.11;13:15:00"
                dateTimePart = dateTimePart.Replace(";", " ");
            }
            
            return dateTimePart;
        }

        private static StockData CreateStockData(string[] parts, CultureInfo culture)
        {
            var normalizedParts = NormalizeParts(parts);
            var dateTimePart = ParseDateTimePart(parts);
            var dateTime = DateTime.ParseExact(dateTimePart, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);

            return new StockData
            {
                Id = int.Parse(normalizedParts[0]),
                DateTime = dateTime,
                Date = dateTime.Date,
                Time = dateTime.TimeOfDay,
                Open = double.Parse(normalizedParts[2], culture),
                High = double.Parse(normalizedParts[3], culture),
                Low = double.Parse(normalizedParts[4], culture),
                Close = double.Parse(normalizedParts[5], culture),
                Volume = long.Parse(normalizedParts[6], NumberStyles.Any, culture),
                Size = long.Parse(normalizedParts[7], NumberStyles.Any, culture)
            };
        }

        private static string[] NormalizeParts(string[] parts)
        {
            // Check if date and time are in separate fields
            if (parts.Length >= 9 && parts[1].Length == 10 && parts[2].Contains(":"))
            {
                // Format: parts[1] = "2013.07.12", parts[2] = "09:30:00"
                var dateTimePart = $"{parts[1]} {parts[2]}";
                // Shift other parts indices by 1
                var newParts = new string[parts.Length - 1];
                newParts[0] = parts[0]; // Id
                newParts[1] = dateTimePart; // Combined datetime
                Array.Copy(parts, 3, newParts, 2, parts.Length - 3); // Rest of the data
                return newParts;
            }
            return parts;
        }

        public enum FilterMode
        {
            All,
            LastN,
            FirstN,
            IndexRange,
            AfterDateTime,
            BeforeDateTime,
            DateTimeRange
        }

        public List<StockData> ReadDataFast(string filePath, FilterMode mode = FilterMode.All, int n1 = 0, int n2 = 0, DateTime? dt1 = null, DateTime? dt2 = null)
        {
            var culture = new CultureInfo("tr-TR");
            var bag = new System.Collections.Concurrent.ConcurrentBag<StockData>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified data file was not found.", filePath);
            }

            File.ReadLines(filePath)
                .AsParallel()
                .Where(line => !(string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#") || line.Trim().StartsWith("Id")))
                .ForAll(line =>
                {
                    var parts = line.Split(';').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();

                    if (parts.Length < 8)
                    {
                        return; // Skip malformed lines
                    }

                    try
                    {
                        var stockData = CreateStockData(parts, culture);
                        bag.Add(stockData);
                    }
                    catch (FormatException ex)
                    {
                        // This might be noisy in parallel. Consider a different logging strategy for production.
                        Console.WriteLine($"Could not parse line: '{line}'. Error: {ex.Message}");
                    }
                });

            // Convert to list and sort to maintain original order, as parallel processing is non-deterministic.
            var allData = bag.ToList();
            allData.Sort((x, y) => x.Id.CompareTo(y.Id));
            
            // Apply filtering based on mode
            var filteredData = ApplyFilter(allData, mode, n1, n2, dt1, dt2);
            
            _readCount = filteredData.Count;
            return filteredData;
        }

        private static List<StockData> ApplyFilter(List<StockData> data, FilterMode mode, int n1, int n2, DateTime? dt1, DateTime? dt2)
        {
            switch (mode)
            {
                case FilterMode.All:
                    return data;

                case FilterMode.LastN:
                    return data.TakeLast(n1).ToList();

                case FilterMode.FirstN:
                    return data.Take(n1).ToList();

                case FilterMode.IndexRange:
                    if (n1 < 0 || n2 < 0 || n1 > n2 || n1 >= data.Count)
                        return new List<StockData>();
                    var endIndex = Math.Min(n2, data.Count - 1);
                    return data.Skip(n1).Take(endIndex - n1 + 1).ToList();

                case FilterMode.AfterDateTime:
                    if (!dt1.HasValue) return data;
                    return data.Where(x => x.DateTime >= dt1.Value).ToList();

                case FilterMode.BeforeDateTime:
                    if (!dt1.HasValue) return data;
                    return data.Where(x => x.DateTime <= dt1.Value).ToList();

                case FilterMode.DateTimeRange:
                    if (!dt1.HasValue || !dt2.HasValue) return data;
                    return data.Where(x => x.DateTime >= dt1.Value && x.DateTime <= dt2.Value).ToList();

                default:
                    return data;
            }
        }
    }
}

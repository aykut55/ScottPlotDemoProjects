using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using Microsoft.VisualBasic.Logging;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static AlgoTradeWithScottPlot.DataReader;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Fully qualify System.Drawing.Color to resolve ambiguity with ScottPlot.Color
using SysColor = System.Drawing.Color;


namespace AlgoTradeWithScottPlot
{
    public partial class Form1 : Form
    {
        private readonly DataReader dataReader;
        private List<StockData>? stockDataList = null;

        // Synchronization control
        private bool isUpdatingAxes = false; // Prevent infinite loops

        // RSI data
        private double[] rsiData;
        private bool rsiCalculated = false;

        private bool useAdaptiveData = false;

        public Form1()
        {
            InitializeComponent();

            // Instantiate the DataReader
            dataReader = new DataReader();

            panelTop.Visible = true;
            panelTop.BackColor = SysColor.LightBlue;

            panelLeft.Visible = true;
            panelLeft.BackColor = SysColor.LightGray;

            panelRight.Visible = false;
            panelRight.BackColor = SysColor.LightGreen;

            panelBottom.Visible = true;
            panelBottom.BackColor = SysColor.LightSalmon;

            panelCenter.Visible = true;

            // Setup plot synchronization
            SetupPlotSynchronization();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
                        var now = DateTime.Now;
                        var rnd = new Random();
                        var ohlcs = Enumerable.Range(0, 20)
                            .Select(i =>
                            {
                                double open = rnd.Next(90, 110);
                                double close = open + rnd.Next(-5, 5);
                                double high = Math.Max(open, close) + rnd.Next(1, 3);
                                double low = Math.Min(open, close) - rnd.Next(1, 3);
                                return new OHLC(open, high, low, close, now.AddMinutes(i), TimeSpan.FromMinutes(1));
                            }).ToArray();

                        formsPlot2.Plot.Clear();
                        formsPlot2.Plot.Add.Candlestick(ohlcs);
                        formsPlot2.Plot.Axes.DateTimeTicksBottom();
                        formsPlot2.Plot.Axes.AutoScale();
                        formsPlot2.Refresh();
                        */

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // This can be used for real-time updates in the future
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileDir = "D:\\sage1\\AlgoTradeWithPaythonWithGemini\\HistoricalData\\2025.09.21\\_Exports";
            string fileName = "";

            var fileNames = new List<string>
              {
                  "IMKBH'AKBNK_1.csv",
                  "IMKBH'AKBNK_5.csv",
                  "IMKBH'AKBNK_10.csv",
                  "IMKBH'AKBNK_15.csv",
                  "IMKBH'AKBNK_20.csv",
                  "IMKBH'AKBNK_30.csv",
                  "IMKBH'AKBNK_60.csv",
                  "IMKBH'AKBNK_120.csv",
                  "IMKBH'AKBNK_240.csv",
                  "IMKBH'AKBNK_G.csv",
                  "IMKBH'AKBNK_H.csv"
              };

            fileName = fileNames[0];                    // 0 : "IMKBH'AKBNK_1.csv",
            fileName = fileNames[fileNames.Count - 1];  // "IMKBH'AKBNK_H.csv"
            fileName = fileNames[0];
            string filePath = Path.Combine(fileDir, fileName);

            fileDir = "C:\\data\\VIP-X030-T";
            fileName = "VIP'VIP-X030-T_1.csv";
            filePath = Path.Combine(fileDir, fileName);

            fileDir = "C:\\data\\csvFiles\\VIP\\01";
            fileName = "VIP-X030-T.csv";
            filePath = Path.Combine(fileDir, fileName);

            FilterMode mode = FilterMode.All;

            try
            {
                toolStripStatusLabel1.Text = $"Loading data from : {filePath}";

                dataReader.Clear();

                dataReader.StartTimer();
                
                if (mode == FilterMode.All)
                {
                    stockDataList = dataReader.ReadDataFast(filePath);
                }
                else if (mode == FilterMode.LastN)
                {
                    stockDataList = dataReader.ReadDataFast(filePath, FilterMode.LastN, 300);
                }
                else if (mode == FilterMode.FirstN)
                {
                    stockDataList = dataReader.ReadDataFast(filePath, FilterMode.FirstN, 300);
                }
                else if (mode == FilterMode.IndexRange)
                {
                    stockDataList = dataReader.ReadDataFast(filePath, FilterMode.IndexRange, 1000, 2000);
                }
                else if (mode == FilterMode.AfterDateTime)
                {
                    stockDataList = dataReader.ReadDataFast(filePath, FilterMode.AfterDateTime, dt1: new DateTime(2025, 11, 12));
                }
                else if (mode == FilterMode.BeforeDateTime)
                {
                    stockDataList = dataReader.ReadDataFast(filePath, FilterMode.BeforeDateTime, dt1: new DateTime(2025, 11, 12));
                }
                else if (mode == FilterMode.DateTimeRange)
                {
                    stockDataList = dataReader.ReadDataFast(filePath, FilterMode.DateTimeRange, dt1: new DateTime(2025, 11, 12), dt2: new DateTime(2025, 11, 12, 23, 59, 59));
                }
                
                dataReader.StopTimer();

                if (stockDataList == null || !stockDataList.Any())
                {
                    MessageBox.Show("No valid data was read from the file.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                long t1 = dataReader.GetElapsedTimeMsec();
                int itemsCount = dataReader.ReadCount;
                toolStripStatusLabel1.Text = $"Data is loaded...Total count : {itemsCount}, Elapsed time : {t1} ms";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while reading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (stockDataList == null || stockDataList.Count < 2)
                {
                    MessageBox.Show("Not enough data to plot OHLC.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var displayData = stockDataList;
                toolStripStatusLabel1.Text = $"Plotting OHLC for {displayData.Count:N0} points...";

                // Clear both plots
                formsPlot2.Plot.Clear(); // Top plot for OHLC
                formsPlot1.Plot.Clear(); // Bottom plot for MAs

                // PLOT OHLC TO TOP PLOT
                PlotOHLCData(formsPlot1, displayData);
            
                // PLOT MA50,100,200 TO TOP PLOT WITH OHLC (if calculated)
                if (maCalculated)
                {
                    var topPlotMAs = new List<(double[] data, int period, ScottPlot.Color color, int width)>
                    {
                        (ma50Data, 50, ScottPlot.Colors.Blue, 3),
                        (ma100Data, 100, ScottPlot.Colors.Purple, 3),
                        (ma200Data, 200, ScottPlot.Colors.Black, 4)
                    };
                    PlotSpecificMovingAverages(formsPlot1, displayData, topPlotMAs);
                }

                // PLOT ALL MAs TO BOTTOM PLOT (if calculated) 
                if (maCalculated)
                {
                    var bottomPlotMAs = new List<(double[] data, int period, ScottPlot.Color color, int width)>
                    {
                        (ma5Data, 5, ScottPlot.Colors.Red, 2),
                        (ma8Data, 8, ScottPlot.Colors.Orange, 2),
                        (ma13Data, 13, ScottPlot.Colors.Yellow, 2),
                        (ma21Data, 21, ScottPlot.Colors.Green, 2),
                        (ma50Data, 50, ScottPlot.Colors.Blue, 3),
                        (ma100Data, 100, ScottPlot.Colors.Purple, 3),
                        (ma200Data, 200, ScottPlot.Colors.Black, 4)
                    };
                    PlotSpecificMovingAverages(formsPlot2, displayData, bottomPlotMAs);
                }

                // PLOT RSI TO THIRD PLOT (if calculated)
                if (rsiCalculated)
                {
                    PlotRsi(formsPlot3, displayData);
                }

                toolStripStatusLabel1.Text = $"Successfully plotted OHLC for {displayData.Count:N0} points";
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "Plotting failed";
                MessageBox.Show($"An error occurred while plotting the data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlotOHLCData(ScottPlot.WinForms.FormsPlot plot, List<StockData> data)
        {
            // PERFORMANCE OPTIMIZATIONS for OHLC
            plot.Plot.Benchmark.IsVisible = false;
            plot.Plot.RenderManager.EnableRendering = true;

            // Use more aggressive sampling to show full dataset in title but maintain performance  
            var maxCandlesForSmooth = 100000; // Increased for better representation
            var sampledData = useAdaptiveData ? GetAdaptiveOHLCData(data, maxCandlesForSmooth) : data;

            // Convert to OHLC array using real DateTime from data
            var ohlcs = new ScottPlot.OHLC[sampledData.Count];

            for (int i = 0; i < sampledData.Count; i++)
            {
                var sd = sampledData[i];
                if (sd.Low > sd.Open || sd.Low > sd.Close)
                {

                }
                else
                {
                    ohlcs[i] = new ScottPlot.OHLC(
                        sd.Open, sd.High, sd.Low, sd.Close,
                        sd.DateTime, // Use real DateTime from data
                        TimeSpan.FromMinutes(1)
                    );
                }
            }

            // Add OHLC candlestick plot
            var candlePlot = plot.Plot.Add.Candlestick(ohlcs);
            candlePlot.Sequential = true;
            candlePlot.RisingColor = ScottPlot.Colors.Green;
            candlePlot.FallingColor = ScottPlot.Colors.Red;

            // Custom DateTime axis formatting for OHLC plot
            plot.Plot.Axes.Bottom.Label.Text = "Time";
            plot.Plot.Axes.Left.Label.Text = "Price";

            // Use built-in DateTime ticks - should work with real DateTime values
            plot.Plot.Axes.DateTimeTicksBottom();
            plot.Plot.Title($"OHLC Candlesticks (Original: {data.Count:N0}, Displayed: {sampledData.Count:N0})");
            plot.Plot.Axes.AutoScale();
            plot.Plot.Axes.Margins(0.01, 0.02);

            plot.ContextMenuStrip = null;
            plot.Refresh();
        }

        private void PlotSpecificMovingAverages(ScottPlot.WinForms.FormsPlot plot, List<StockData> data, List<(double[] data, int period, ScottPlot.Color color, int width)> movingAverages)
        {
            // ULTRA PERFORMANCE OPTIMIZATIONS for specific MAs
            plot.Plot.Benchmark.IsVisible = false;
            plot.Plot.RenderManager.EnableRendering = true;

            if (movingAverages == null || movingAverages.Count == 0)
            {
                toolStripStatusLabel1.Text = "No valid MA data provided";
                return;
            }

            // Use adaptive sampling for performance 
            var maxPointsForSmooth = 100000; // Same as OHLC
            var sampledData = useAdaptiveData ? GetAdaptiveData(data, maxPointsForSmooth) : data;

            toolStripStatusLabel1.Text = $"Plotting selected MA lines (Original: {data.Count:N0}, Displayed: {sampledData.Count:N0})...";

            // Create filtered MA data arrays starting from index 200
            var validStartIndex = 200;
            var validLength = sampledData.Count - validStartIndex;

            if (validLength <= 0)
            {
                toolStripStatusLabel1.Text = "Not enough sampled data for MA plotting";
                return;
            }

            foreach (var (maData, period, color, width) in movingAverages)
            {
                var filteredData = new double[validLength];

                // Copy valid MA data (starting from where MA200 becomes available)
                for (int i = 0; i < validLength; i++)
                {
                    var sourceIndex = validStartIndex + i;
                    
                    // Map from sampled data back to original MA data using ID
                    var sampledDataPoint = sampledData[sourceIndex];
                    var originalIndex = sampledDataPoint.Id - 1; // Assuming ID starts from 1
                    
                    if (originalIndex >= 0 && originalIndex < maData.Length)
                    {
                        filteredData[i] = maData[originalIndex];
                    }
                    else
                    {
                        filteredData[i] = double.NaN;
                    }
                }

                // Add MA line to plot - Signal plot for performance
                var maPlot = plot.Plot.Add.Signal(filteredData);
                maPlot.Data.XOffset = validStartIndex;
                maPlot.Color = color;
                maPlot.LineWidth = width;
            }

            // Simple DateTime axis formatting for MA plot (Signal plot limitation)
            plot.Plot.Axes.Bottom.Label.Text = "Data Index";
            plot.Plot.Axes.Left.Label.Text = "MA Price";

            var periodText = string.Join(",", movingAverages.Select(ma => ma.period));
            plot.Plot.Title($"Moving Averages: MA{periodText} (Original: {data.Count:N0}, Displayed: {sampledData.Count:N0})");

            // Use AutoScale for proper zoom reset behavior
            plot.Plot.Axes.AutoScale();
            plot.Plot.Axes.Margins(0.01, 0.02);

            // Disable context menu for better performance
            plot.ContextMenuStrip = null;
            plot.Refresh();

            toolStripStatusLabel1.Text = $"Successfully plotted MA{periodText} for {data.Count:N0} points";
        }

        private void PlotRsi(ScottPlot.WinForms.FormsPlot plot, List<StockData> data)
        {
            // Clear plot first
            plot.Plot.Clear();

            if (!rsiCalculated)
            {
                toolStripStatusLabel1.Text = "Please calculate RSI first";
                return;
            }

            // Use adaptive sampling for performance 
            var maxPointsForSmooth = 100000; // Same as OHLC
            var sampledData = useAdaptiveData ? GetAdaptiveData(data, maxPointsForSmooth) : data;
            //sampledData = data;

            toolStripStatusLabel1.Text = $"Plotting RSI (Original: {data.Count:N0}, Displayed: {sampledData.Count:N0})...";

            // Filter out NaN values for proper plotting
            var validStartIndex = 14; // RSI starts at period 14
            var validLength = sampledData.Count - validStartIndex;
            
            if (validLength <= 0)
            {
                toolStripStatusLabel1.Text = "Not enough sampled data for RSI plotting";
                return;
            }

            var filteredRsiData = new double[validLength];

            for (int i = 0; i < validLength; i++)
            {
                var sourceIndex = validStartIndex + i;
                
                // Map from sampled data back to original RSI data using ID
                var sampledDataPoint = sampledData[sourceIndex];
                var originalIndex = sampledDataPoint.Id - 1; // Assuming ID starts from 1
                
                if (originalIndex >= 0 && originalIndex < rsiData.Length)
                {
                    filteredRsiData[i] = rsiData[originalIndex];
                }
                else
                {
                    filteredRsiData[i] = double.NaN;
                }
            }

            // Add RSI line to plot using Signal for performance
            var rsiPlot = plot.Plot.Add.Signal(filteredRsiData);
            rsiPlot.Data.XOffset = validStartIndex;
            rsiPlot.Color = ScottPlot.Colors.Blue;
            rsiPlot.LineWidth = 2;

            // Add reference lines at 30 and 70
            var overBoughtLine = plot.Plot.Add.HorizontalLine(70);
            overBoughtLine.Color = ScottPlot.Colors.Red;
            overBoughtLine.LineWidth = 1;

            var overSoldLine = plot.Plot.Add.HorizontalLine(30);
            overSoldLine.Color = ScottPlot.Colors.Green;
            overSoldLine.LineWidth = 1;

            var midLine = plot.Plot.Add.HorizontalLine(50);
            midLine.Color = ScottPlot.Colors.Gray;
            midLine.LineWidth = 1;

            // Setup axes
            plot.Plot.Axes.Bottom.Label.Text = "Data Index";
            plot.Plot.Axes.Left.Label.Text = "RSI";
            plot.Plot.Title($"RSI (14) (Original: {data.Count:N0}, Displayed: {sampledData.Count:N0})");

            // Set Y axis limits for RSI (0-100)
            plot.Plot.Axes.SetLimitsY(0, 100);
            plot.Plot.Axes.AutoScaleX();
            plot.Plot.Axes.Margins(0.01, 0.02);

            // Disable context menu for better performance
            plot.ContextMenuStrip = null;
            plot.Refresh();

            toolStripStatusLabel1.Text = $"Successfully plotted RSI for {data.Count:N0} points";
        }

        private void PlotMovingAveragesWithSync(ScottPlot.WinForms.FormsPlot plot, List<StockData> data)
        {
            // ULTRA PERFORMANCE OPTIMIZATIONS for MAs - Using Signal plot approach
            plot.Plot.Benchmark.IsVisible = false;
            plot.Plot.RenderManager.EnableRendering = true;

            // Clear plot first
            plot.Plot.Clear();

            if (!maCalculated)
            {
                toolStripStatusLabel1.Text = "Please calculate MAs first using 'Calculate MA' button";
                return;
            }

            toolStripStatusLabel1.Text = $"Plotting MA lines for {data.Count:N0} data points...";

            // DEBUG: Check if MA data exists and has values
            if (ma5Data == null || ma5Data.Length == 0)
            {
                toolStripStatusLabel1.Text = "MA data is empty - calculation may have failed";
                return;
            }

            // Create filtered MA data arrays without NaN values for proper auto-scaling
            // Start from index 200 where all MAs are valid
            var validStartIndex = 200;
            var validLength = data.Count - validStartIndex;

            var ma5Filtered = new double[validLength];
            var ma8Filtered = new double[validLength];
            var ma13Filtered = new double[validLength];
            var ma21Filtered = new double[validLength];
            var ma50Filtered = new double[validLength];
            var ma100Filtered = new double[validLength];
            var ma200Filtered = new double[validLength];

            // Copy valid MA data (starting from where MA200 becomes available)
            for (int i = 0; i < validLength; i++)
            {
                var sourceIndex = validStartIndex + i;
                ma5Filtered[i] = ma5Data[sourceIndex];
                ma8Filtered[i] = ma8Data[sourceIndex];
                ma13Filtered[i] = ma13Data[sourceIndex];
                ma21Filtered[i] = ma21Data[sourceIndex];
                ma50Filtered[i] = ma50Data[sourceIndex];
                ma100Filtered[i] = ma100Data[sourceIndex];
                ma200Filtered[i] = ma200Data[sourceIndex];
            }

            // ULTRA-FAST Signal plot approach with filtered data
            var ma5Plot = plot.Plot.Add.Signal(ma5Filtered);
            var ma8Plot = plot.Plot.Add.Signal(ma8Filtered);
            var ma13Plot = plot.Plot.Add.Signal(ma13Filtered);
            var ma21Plot = plot.Plot.Add.Signal(ma21Filtered);
            var ma50Plot = plot.Plot.Add.Signal(ma50Filtered);
            var ma100Plot = plot.Plot.Add.Signal(ma100Filtered);
            var ma200Plot = plot.Plot.Add.Signal(ma200Filtered);

            // Set starting offset for all signals to align with data index 200
            ma5Plot.Data.XOffset = validStartIndex;
            ma8Plot.Data.XOffset = validStartIndex;
            ma13Plot.Data.XOffset = validStartIndex;
            ma21Plot.Data.XOffset = validStartIndex;
            ma50Plot.Data.XOffset = validStartIndex;
            ma100Plot.Data.XOffset = validStartIndex;
            ma200Plot.Data.XOffset = validStartIndex;

            // Set colors and styles for each MA - make them more visible
            ma5Plot.Color = ScottPlot.Colors.Red; ma5Plot.LineWidth = 2;
            ma8Plot.Color = ScottPlot.Colors.Orange; ma8Plot.LineWidth = 2;
            ma13Plot.Color = ScottPlot.Colors.Yellow; ma13Plot.LineWidth = 2;
            ma21Plot.Color = ScottPlot.Colors.Green; ma21Plot.LineWidth = 2;
            ma50Plot.Color = ScottPlot.Colors.Blue; ma50Plot.LineWidth = 3;
            ma100Plot.Color = ScottPlot.Colors.Purple; ma100Plot.LineWidth = 3;
            ma200Plot.Color = ScottPlot.Colors.Black; ma200Plot.LineWidth = 4;

            // Simple axis setup for visibility
            plot.Plot.Axes.Bottom.Label.Text = "Data Index";
            plot.Plot.Axes.Left.Label.Text = "MA Price";
            plot.Plot.Title($"Moving Averages - {data.Count:N0} Points (MA200 starts at index 200)");

            // Use AutoScale like OHLC plot for proper zoom reset behavior
            plot.Plot.Axes.AutoScale();
            plot.Plot.Axes.Margins(0.01, 0.02); // Same margins as OHLC plot

            // Disable context menu for better performance
            plot.ContextMenuStrip = null;
            plot.Refresh();

            toolStripStatusLabel1.Text = $"Successfully plotted MA lines for {data.Count:N0} points";
        }

        private void PlotMovingAverages(ScottPlot.WinForms.FormsPlot plot, List<StockData> data)
        {
            // Legacy method - now redirects to synchronized version
            PlotMovingAveragesWithSync(plot, data);
        }

        private double[] CalculateMovingAverage(List<StockData> data, int period)
        {
            var result = new double[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                if (i < period - 1)
                {
                    result[i] = double.NaN; // Not enough data points
                }
                else
                {
                    var sum = 0.0;
                    for (int j = i - period + 1; j <= i; j++)
                    {
                        sum += data[j].Close;
                    }
                    result[i] = sum / period;
                }
            }

            return result;
        }

        private void CalculateRsi(List<StockData> data, int period = 14)
        {
            if (data == null || data.Count < period + 1)
            {
                rsiCalculated = false;
                return;
            }

            var gains = new double[data.Count];
            var losses = new double[data.Count];
            rsiData = new double[data.Count];

            // Calculate price changes
            for (int i = 1; i < data.Count; i++)
            {
                var change = data[i].Close - data[i - 1].Close;
                gains[i] = change > 0 ? change : 0;
                losses[i] = change < 0 ? -change : 0;
            }

            // Calculate initial averages
            var avgGain = 0.0;
            var avgLoss = 0.0;

            for (int i = 1; i <= period; i++)
            {
                avgGain += gains[i];
                avgLoss += losses[i];
            }

            avgGain /= period;
            avgLoss /= period;

            // Calculate RSI
            for (int i = 0; i < data.Count; i++)
            {
                if (i < period)
                {
                    rsiData[i] = double.NaN;
                }
                else if (i == period)
                {
                    var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                    rsiData[i] = 100 - (100 / (1 + rs));
                }
                else
                {
                    // Wilder's smoothing
                    avgGain = ((avgGain * (period - 1)) + gains[i]) / period;
                    avgLoss = ((avgLoss * (period - 1)) + losses[i]) / period;

                    var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
                    rsiData[i] = 100 - (100 / (1 + rs));
                }
            }

            rsiCalculated = true;
        }

        // Smart OHLC adaptive sampling - preserves important price action while maintaining performance
        private List<StockData> GetAdaptiveOHLCData(List<StockData> data, int maxPoints)
        {
            if (data.Count <= maxPoints)
                return data;

            var sampled = new List<StockData>(maxPoints);
            var step = (double)data.Count / maxPoints;

            // Always include first candle
            sampled.Add(data[0]);

            // Intelligent OHLC sampling that preserves critical price levels
            for (int i = 1; i < maxPoints - 1; i++)
            {
                var windowStart = (int)((i - 1) * step);
                var windowEnd = Math.Min((int)(i * step), data.Count - 1);

                if (windowStart >= data.Count) break;

                // For each window, create a consolidated OHLC candle
                var window = data.Skip(windowStart).Take(windowEnd - windowStart + 1).ToList();
                if (!window.Any()) continue;

                // Create representative OHLC from the window
                var consolidatedCandle = new StockData
                {
                    DateTime = window.First().DateTime, // Use first timestamp
                    Open = window.First().Open,         // First open
                    High = window.Max(x => x.High),     // Highest high
                    Low = window.Min(x => x.Low),       // Lowest low  
                    Close = window.Last().Close,        // Last close
                    Volume = window.Sum(x => x.Volume)  // Sum volumes
                };

                sampled.Add(consolidatedCandle);
            }

            // Always include last candle
            if (data.Count > 1)
                sampled.Add(data[data.Count - 1]);

            return sampled;
        }

        // General adaptive sampling for MA and RSI (simple uniform sampling)
        private List<StockData> GetAdaptiveData(List<StockData> data, int maxPoints)
        {
            if (data.Count <= maxPoints)
                return data;

            var sampled = new List<StockData>(maxPoints);
            var step = (double)data.Count / maxPoints;

            // Simple uniform sampling - keep original data structure
            for (int i = 0; i < maxPoints; i++)
            {
                var index = (int)(i * step);
                if (index >= data.Count) 
                    index = data.Count - 1;
                
                sampled.Add(data[index]);
            }

            return sampled;
        }

        // Legacy decimation method
        private List<StockData> DecimateData(List<StockData> data, int maxPoints)
        {
            return GetAdaptiveOHLCData(data, maxPoints);
        }

        // COMPREHENSIVE PLOT SYNCHRONIZATION METHODS
        private void SetupPlotSynchronization()
        {
            // FORMSPLOT2 (OHLC - TOP PLOT) -> ALL OTHER PLOTS
            formsPlot2.MouseWheel += (sender, e) => SynchronizeFromSource(formsPlot2);
            formsPlot2.MouseUp += (sender, e) => SynchronizeFromSource(formsPlot2);
            formsPlot2.MouseMove += (sender, e) => { if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) SynchronizeFromSource(formsPlot2); };
            formsPlot2.MouseDown += (sender, e) => SynchronizeFromSource(formsPlot2);

            // FORMSPLOT1 (MA - BOTTOM PLOT) -> ALL OTHER PLOTS  
            formsPlot1.MouseWheel += (sender, e) => SynchronizeFromSource(formsPlot1);
            formsPlot1.MouseUp += (sender, e) => SynchronizeFromSource(formsPlot1);
            formsPlot1.MouseMove += (sender, e) => { if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) SynchronizeFromSource(formsPlot1); };
            formsPlot1.MouseDown += (sender, e) => SynchronizeFromSource(formsPlot1);

            // FORMSPLOT3 (RSI - THIRD PLOT) -> ALL OTHER PLOTS
            formsPlot3.MouseWheel += (sender, e) => SynchronizeFromSource(formsPlot3);
            formsPlot3.MouseUp += (sender, e) => SynchronizeFromSource(formsPlot3);
            formsPlot3.MouseMove += (sender, e) => { if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) SynchronizeFromSource(formsPlot3); };
            formsPlot3.MouseDown += (sender, e) => SynchronizeFromSource(formsPlot3);
        }

        private void SynchronizeFromSource(ScottPlot.WinForms.FormsPlot sourcePlot)
        {
            if (isUpdatingAxes) return; // Prevent infinite loop

            // Use a small delay to allow the source plot to finish its interaction
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 30; // Reduced to 30ms for more responsive sync
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                timer.Dispose();
                SynchronizeAllPlotsFromSource(sourcePlot);
            };
            timer.Start();
        }

        private void SynchronizeOnInteraction(ScottPlot.WinForms.FormsPlot sourcePlot, ScottPlot.WinForms.FormsPlot targetPlot)
        {
            if (isUpdatingAxes) return; // Prevent infinite loop

            // Use a small delay to allow the source plot to finish its interaction
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 30; // Reduced to 30ms for more responsive sync
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                timer.Dispose();
                SynchronizeAllAxes(sourcePlot, targetPlot);
            };
            timer.Start();
        }

        private void SynchronizeAllPlotsFromSource(ScottPlot.WinForms.FormsPlot sourcePlot)
        {
            if (isUpdatingAxes) return; // Prevent infinite loop

            try
            {
                isUpdatingAxes = true;

                // Get X axis limits from source plot (only sync X-axis for performance)
                var sourceXMin = sourcePlot.Plot.Axes.Bottom.Min;
                var sourceXMax = sourcePlot.Plot.Axes.Bottom.Max;

                // Sync to all other plots except the source
                var allPlots = new[] { formsPlot1, formsPlot2, formsPlot3 };
                
                foreach (var targetPlot in allPlots)
                {
                    if (targetPlot != sourcePlot)
                    {
                        // Apply only X-axis limits to target plot (keep Y-axis independent)
                        targetPlot.Plot.Axes.SetLimitsX(sourceXMin, sourceXMax);
                        targetPlot.Refresh();
                    }
                }
            }
            catch (Exception)
            {
                // Ignore synchronization errors to prevent crashes
            }
            finally
            {
                isUpdatingAxes = false;
            }
        }

        private void SynchronizeAllAxes(ScottPlot.WinForms.FormsPlot sourcePlot, ScottPlot.WinForms.FormsPlot targetPlot)
        {
            if (isUpdatingAxes) return; // Prevent infinite loop

            try
            {
                isUpdatingAxes = true;

                // Get ALL axis limits from source plot
                var sourceXMin = sourcePlot.Plot.Axes.Bottom.Min;
                var sourceXMax = sourcePlot.Plot.Axes.Bottom.Max;
                var sourceYMin = sourcePlot.Plot.Axes.Left.Min;
                var sourceYMax = sourcePlot.Plot.Axes.Left.Max;

                // Apply BOTH X and Y axis limits to target plot for FULL synchronization
                targetPlot.Plot.Axes.SetLimits(sourceXMin, sourceXMax, sourceYMin, sourceYMax);

                // Refresh target plot to show fully synchronized view
                targetPlot.Refresh();
            }
            catch (Exception)
            {
                // Ignore synchronization errors to prevent crashes
            }
            finally
            {
                isUpdatingAxes = false;
            }
        }

        // Store calculated MA data for plotting
        private double[] ma5Data, ma8Data, ma13Data, ma21Data, ma50Data, ma100Data, ma200Data;
        private bool maCalculated = false;

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (stockDataList == null || stockDataList.Count < 200)
                {
                    MessageBox.Show("Not enough data to calculate MAs. Need at least 200 data points.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var displayData = stockDataList;
                toolStripStatusLabel1.Text = $"Calculating Moving Averages for {displayData.Count:N0} points...";

                // Calculate all Moving Averages and store them
                ma5Data = CalculateMovingAverage(displayData, 5);
                ma8Data = CalculateMovingAverage(displayData, 8);
                ma13Data = CalculateMovingAverage(displayData, 13);
                ma21Data = CalculateMovingAverage(displayData, 21);
                ma50Data = CalculateMovingAverage(displayData, 50);
                ma100Data = CalculateMovingAverage(displayData, 100);
                ma200Data = CalculateMovingAverage(displayData, 200);

                maCalculated = true;

                toolStripStatusLabel1.Text = $"Successfully calculated MA5,8,13,21,50,100 and 200 for {displayData.Count:N0} points";
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "MA calculation failed";
                MessageBox.Show($"An error occurred while calculating MAs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panelLeft_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (stockDataList == null || stockDataList.Count < 15)
                {
                    MessageBox.Show("Not enough data to calculate RSI. Need at least 15 data points.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var displayData = stockDataList;
                toolStripStatusLabel1.Text = $"Calculating RSI for {displayData.Count:N0} points...";

                // Calculate RSI
                CalculateRsi(displayData);

                toolStripStatusLabel1.Text = $"Successfully calculated RSI for {displayData.Count:N0} points";
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "RSI calculation failed";
                MessageBox.Show($"An error occurred while calculating RSI: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}





/*
 * 
 * 
 * 

> bence olmaz. deneme yanılma yaparak datalar ve plotlarına ait speklerini cıkaracağız. Mesala RSİ için bir sablonum
  var değil mi? aynısını stokastik için de kullanabiliriz. mesala SuperTrend indikatirü cıktıları var. Bunu da ma
  şablonunu kullanarak ister OHLC nin üzerinde ister ayrı bir plotta çizdirmke mümkün olurdu. Doğru muyum?

● Kesinlikle doğru! Çok akıllıca bir yaklaşım.

  Mevcut Şablonlarımız:

  1) RSI Şablonu (0-100 Osilatör)

  PlotRsi(plot, data)
  // Y-axis: 0-100 fixed
  // Reference lines: 30, 50, 70
  // Single line plot

  Kullanılabilir:
  - ✅ Stochastic (0-100, ref: 20,50,80)
  - ✅ Williams %R (0-100'e normalize et)
  - ✅ CCI (scale değiştir)

  2) Moving Average Şablonu (Fiyat Koreleli)

  PlotSpecificMovingAverages(plot, data, maList)
  // Y-axis: Auto price scale
  // Multiple lines with colors

  Kullanılabilir:
  - ✅ SuperTrend (OHLC üzerinde veya ayrı plot)
  - ✅ Bollinger Bands (3 line: upper/middle/lower)
  - ✅ Ichimoku lines (Tenkan/Kijun/etc)
  - ✅ VWAP

  3) OHLC + Overlay Şablonu

  PlotOHLCData() + PlotSpecificMovingAverages()

  Kullanılabilir:
  - ✅ SuperTrend overlays
  - ✅ Bollinger Bands overlay
  - ✅ Parabolic SAR dots

  Approach:

  1. Hesaplama metodları yaz (CalculateStochastic, CalculateSuperTrend...)
  2. Mevcut plot şablonlarını kullan
  3. Sadece parametreleri değiştir (Y-limits, colors, reference lines)

  Çok efficient ve maintainable olur! 

 * 
 */
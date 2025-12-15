using ScottPlot;
using ScottPlot.WinForms;

namespace ScottPlotOHLCWinForms;

public partial class Form1 : Form
{
    private readonly FormsPlot formsPlot1;
    private OHLC[] _fullData;
    private OHLC[] _dataL1; // Aggregated x100 = 20k points
    private OHLC[] _dataL2; // Aggregated x1000 = 2k points
    private OHLC[] _dataL3; // Aggregated x10000 = 200 points
    private OHLC[] _dataL4; // Aggregated x10 = 200k points
    
    // Keep references to all 5 plots
    private ScottPlot.Plottables.CandlestickPlot _plotL0;
    private ScottPlot.Plottables.CandlestickPlot _plotL1;
    private ScottPlot.Plottables.CandlestickPlot _plotL2;
    private ScottPlot.Plottables.CandlestickPlot _plotL3;
    private ScottPlot.Plottables.CandlestickPlot _plotL4;

    private int _currentLayer = -1; // Start invalid to force update

    // View mode controls
    private Panel _controlPanel;
    private RadioButton _rbFullData;
    private RadioButton _rbFitToScreen;
    private RadioButton _rbLastN;
    private RadioButton _rbFirstN;
    private RadioButton _rbRange;
    private TextBox _txtN;
    private TextBox _txtRangeStart;
    private TextBox _txtRangeEnd;
    private Button _btnApply;
    private RadioButton _rbReset;

    private bool _autoScaleY = true; // Flag to enable/disable automatic Y-axis scaling

    public Form1()
    {
        InitializeComponent();
        
        // Create plot panel first (will fill remaining space)
        Panel plotPanel = new Panel
        {
            Dock = DockStyle.Fill
        };
        this.Controls.Add(plotPanel);
        
        // Initialize FormsPlot in the plot panel
        formsPlot1 = new FormsPlot() { Dock = DockStyle.Fill };
        plotPanel.Controls.Add(formsPlot1);
        
        // Create control panel last (will dock to top)
        _controlPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = System.Drawing.Color.LightGray
        };
        this.Controls.Add(_controlPanel);
        
        // Row 1: Radio buttons
        _rbFullData = new RadioButton { Text = "Full Data", Location = new Point(10, 10), AutoSize = true, Checked = true };
        _rbFitToScreen = new RadioButton { Text = "Fit to Screen", Location = new Point(120, 10), AutoSize = true };
        _rbLastN = new RadioButton { Text = "Last N:", Location = new Point(250, 10), AutoSize = true };
        _rbFirstN = new RadioButton { Text = "First N:", Location = new Point(350, 10), AutoSize = true };
        _rbRange = new RadioButton { Text = "Range:", Location = new Point(450, 10), AutoSize = true };
        _rbReset = new RadioButton { Text = "Reset", Location = new Point(550, 10), AutoSize = true };
        
        _controlPanel.Controls.Add(_rbFullData);
        _controlPanel.Controls.Add(_rbFitToScreen);
        _controlPanel.Controls.Add(_rbLastN);
        _controlPanel.Controls.Add(_rbFirstN);
        _controlPanel.Controls.Add(_rbRange);
        _controlPanel.Controls.Add(_rbReset);
        
        // Row 2: Input controls
        _txtN = new TextBox { Location = new Point(250, 40), Width = 80, Text = "1000" };
        _txtRangeStart = new TextBox { Location = new Point(450, 40), Width = 80, Text = "0", PlaceholderText = "Start" };
        _txtRangeEnd = new TextBox { Location = new Point(540, 40), Width = 80, Text = "1000", PlaceholderText = "End" };
        _btnApply = new Button { Text = "Apply", Location = new Point(640, 38), Width = 80 };
        
        _controlPanel.Controls.Add(_txtN);
        _controlPanel.Controls.Add(_txtRangeStart);
        _controlPanel.Controls.Add(_txtRangeEnd);
        _controlPanel.Controls.Add(_btnApply);
        
        // Apply button click handler
        //_btnApply.Click += (s, e) => ApplyViewMode();

        // Generate 2M OHLC points
        int pointCount = 2_000_000;
        _fullData = GenerateOHLCs(pointCount);
        
        // Pre-calculate LOD layers
        _dataL1 = AggregateData(_fullData, 100);   // 20k points
        _dataL2 = AggregateData(_fullData, 1000);  // 2k points
        _dataL3 = AggregateData(_fullData, 10000); // 200 points
        _dataL4 = AggregateData(_fullData, 10);    // 200k points

        // Add ALL plots initially, but hide them
        // Layer 3 (Very Coarse - extreme zoom out)
        _plotL3 = formsPlot1.Plot.Add.Candlestick(_dataL3);
        _plotL3.Sequential = false;
        _plotL3.IsVisible = true; // Start visible

        // Layer 2 (Coarse)
        _plotL2 = formsPlot1.Plot.Add.Candlestick(_dataL2);
        _plotL2.Sequential = false;
        _plotL2.IsVisible = false;

        // Layer 1 (Medium)
        _plotL1 = formsPlot1.Plot.Add.Candlestick(_dataL1);
        _plotL1.Sequential = false;
        _plotL1.IsVisible = false;

        // Layer 4 (Fine - between L1 and L0)
        _plotL4 = formsPlot1.Plot.Add.Candlestick(_dataL4);
        _plotL4.Sequential = false;
        _plotL4.IsVisible = false;

        // Layer 0 (Full)
        _plotL0 = formsPlot1.Plot.Add.Candlestick(_fullData);
        _plotL0.Sequential = true; // Enable Sequential for full data
        _plotL0.IsVisible = false;

        _currentLayer = 3;

        // Mouse wheel zoom is already enabled by default for X axis
        // Left-click drag for pan is already enabled
        // We just need to add Ctrl+Wheel for Y axis zoom
        
        bool isCtrlPressed = false;
        
        this.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.ControlKey)
                isCtrlPressed = true;
        };
        
        this.KeyUp += (s, e) =>
        {
            if (e.KeyCode == Keys.ControlKey)
                isCtrlPressed = false;
        };

        // Custom Axis Formatter: Index -> Date (Dynamic format based on zoom level)
        ScottPlot.TickGenerators.NumericAutomatic tickGen = new ScottPlot.TickGenerators.NumericAutomatic();
        tickGen.LabelFormatter = (x) => 
        {
            int index = (int)x;
            if (index < 0 || index >= _fullData.Length) return "";
            
            // Get current visible range to determine appropriate format
            var axis = formsPlot1.Plot.Axes.Bottom;
            double visibleRange = axis.Max - axis.Min;
            
            DateTime dt = _fullData[index].DateTime;
            
            // Dynamic format based on visible range
            // If showing more than 1 day worth of data (1440 minutes), show only date
            // Otherwise show date + time
            if (visibleRange > 1440) // More than 1 day
            {
                return dt.ToString("yyyy.MM.dd");
            }
            else // Intraday - show time too
            {
                return dt.ToString("yyyy.MM.dd HH:mm:ss");
            }
        };
        formsPlot1.Plot.Axes.Bottom.TickGenerator = tickGen;

        // Detect Y-axis pan to disable auto-scaling
        double lastYMin = 0;
        double lastYMax = 0;
        
        formsPlot1.MouseDown += (s, e) =>
        {
            lastYMin = formsPlot1.Plot.Axes.Left.Min;
            lastYMax = formsPlot1.Plot.Axes.Left.Max;
        };
        
        formsPlot1.MouseUp += (s, e) =>
        {
            // Check if Y-axis changed (user panned Y-axis)
            double currentYMin = formsPlot1.Plot.Axes.Left.Min;
            double currentYMax = formsPlot1.Plot.Axes.Left.Max;
            
            if (Math.Abs(currentYMin - lastYMin) > 0.001 || Math.Abs(currentYMax - lastYMax) > 0.001)
            {
                // Y-axis was manually changed, disable auto-scaling
                _autoScaleY = false;
                System.Diagnostics.Debug.WriteLine("[Y-Axis] Manual pan detected, auto-scaling disabled");
            }
        };

        // Add Crosshair
        var crosshair = formsPlot1.Plot.Add.Crosshair(0, 0);
        crosshair.IsVisible = false;

        formsPlot1.MouseMove += (s, e) => 
        {
            if (_fullData == null) return; // Wait for data
            
            if (!crosshair.IsVisible) crosshair.IsVisible = true;
            var mouse = formsPlot1.Plot.GetCoordinates(e.X, e.Y);
            crosshair.X = mouse.X;
            crosshair.Y = mouse.Y;
            formsPlot1.Refresh();
        };

        // LOD Logic
        formsPlot1.Plot.RenderManager.RenderStarting += (s, e) =>
        {
            try
            {
                if (_fullData == null) return;

                var axis = formsPlot1.Plot.Axes.Bottom;
                double range = axis.Max - axis.Min;
                
                System.Diagnostics.Debug.WriteLine($"[LOD Debug] axis.Min={axis.Min:F2}, axis.Max={axis.Max:F2}, range={range:F2}");

                // Decide which layer to use based on visible range (zoom level)
                int targetLayer = _currentLayer;
                
                if (range > 1_000_000)
                {
                    targetLayer = 3; // 200 points
                }
                else if (range > 500_000)
                {
                    targetLayer = 2; // 2k points
                }
                else if (range > 20_000)
                {
                    targetLayer = 1; // 20k points
                }
                else if (range > 2_000)
                {
                    targetLayer = 4; // 200k points
                }
                else
                {
                    targetLayer = 0; // 2M points (full)
                }

                // Only switch if changed
                if (targetLayer != _currentLayer)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOD Switch] Range: {range:N0} -> Switching to Layer {targetLayer}");
                    
                    _plotL0.IsVisible = (targetLayer == 0);
                    _plotL1.IsVisible = (targetLayer == 1);
                    _plotL2.IsVisible = (targetLayer == 2);
                    _plotL3.IsVisible = (targetLayer == 3);
                    _plotL4.IsVisible = (targetLayer == 4);
                    
                    _currentLayer = targetLayer;
                }

                // Update Title with aggregation info
                int displayedCount = 0;
                string aggInfo = "";
                int logicalLayer = 0; // Logical layer number based on zoom level
                
                if (_currentLayer == 0) 
                {
                    displayedCount = _fullData.Length;
                    aggInfo = "Full Data";
                    logicalLayer = 0; // Most zoomed in
                }
                else if (_currentLayer == 4) 
                {
                    displayedCount = _dataL4.Length;
                    aggInfo = "x10";
                    logicalLayer = 1;
                }
                else if (_currentLayer == 1) 
                {
                    displayedCount = _dataL1.Length;
                    aggInfo = "x100";
                    logicalLayer = 2;
                }
                else if (_currentLayer == 2) 
                {
                    displayedCount = _dataL2.Length;
                    aggInfo = "x1000";
                    logicalLayer = 3;
                }
                else // _currentLayer == 3
                {
                    displayedCount = _dataL3.Length;
                    aggInfo = "x10000";
                    logicalLayer = 4; // Most zoomed out
                }

                formsPlot1.Plot.Title($"Total: {_fullData.Length:N0} | Showing: {displayedCount:N0} ({aggInfo}) | Layer: {logicalLayer}");
                
                // Auto-scale Y axis based on visible X range (only if auto-scaling is enabled)
                if (_autoScaleY)
                {
                    var xAxis = formsPlot1.Plot.Axes.Bottom;
                    int visibleStartIdx = Math.Max(0, (int)Math.Floor(xAxis.Min));
                    int visibleEndIdx = Math.Min(_fullData.Length, (int)Math.Ceiling(xAxis.Max));
                    
                    if (visibleStartIdx < visibleEndIdx && visibleEndIdx <= _fullData.Length)
                    {
                        double yMin = double.MaxValue;
                        double yMax = double.MinValue;
                        
                        for (int i = visibleStartIdx; i < visibleEndIdx; i++)
                        {
                            yMin = Math.Min(yMin, _fullData[i].Low);
                            yMax = Math.Max(yMax, _fullData[i].High);
                        }
                        
                        if (yMin < yMax)
                        {
                            // Add 5% padding to Y limits
                            double yPadding = (yMax - yMin) * 0.05;
                            formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CRASH] RenderStarting: {ex.Message}");
            }
        };

        formsPlot1.Plot.Title($"OHLC Chart with {pointCount:N0} Points (LOD)");
        formsPlot1.Refresh();
    }

    private OHLC[] GenerateOHLCs(int count)
    {
        System.Diagnostics.Debug.WriteLine($"[GenerateOHLCs] Starting generation of {count:N0} points...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        OHLC[] ohlcs = new OHLC[count];
        DateTime startDate = new DateTime(2020, 1, 1);
        double open = 100;
        Random rand = new Random(0);

        for (int i = 0; i < count; i++)
        {
            double change = (rand.NextDouble() - 0.5) * 2;
            double close = open + change;
            double high = Math.Max(open, close) + rand.NextDouble();
            double low = Math.Min(open, close) - rand.NextDouble();
            
            high = Math.Max(high, Math.Max(open, close));
            low = Math.Min(low, Math.Min(open, close));

            TimeSpan timeSpan = TimeSpan.FromMinutes(1);
            DateTime time = startDate + (timeSpan * i);

            // Use real time for proper date display on X-axis
            ohlcs[i] = new OHLC(open, high, low, close, time, timeSpan);
            open = close;
        }
        
        sw.Stop();
        System.Diagnostics.Debug.WriteLine($"[GenerateOHLCs] Completed in {sw.ElapsedMilliseconds}ms");
        return ohlcs;
    }

    private OHLC[] AggregateData(OHLC[] source, int factor)
    {
        System.Diagnostics.Debug.WriteLine($"[AggregateData] Aggregating factor {factor}...");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        int newCount = source.Length / factor;
        OHLC[] result = new OHLC[newCount];

        for (int i = 0; i < newCount; i++)
        {
            int baseIdx = i * factor;
            double o = source[baseIdx].Open;
            double c = source[baseIdx + factor - 1].Close;
            double h = double.MinValue;
            double l = double.MaxValue;

            for (int j = 0; j < factor; j++)
            {
                h = Math.Max(h, source[baseIdx + j].High);
                l = Math.Min(l, source[baseIdx + j].Low);
            }

            // X coordinate should be the center or start?
            // Let's use center to align nicely.
            // Center index = baseIdx + factor / 2.0
            double centerIndex = baseIdx + (factor / 2.0);
            
            result[i] = new OHLC(o, h, l, c, DateTime.FromOADate(centerIndex), TimeSpan.FromDays(factor));
        }

        sw.Stop();
        System.Diagnostics.Debug.WriteLine($"[AggregateData] Completed in {sw.ElapsedMilliseconds}ms. New count: {newCount:N0}");
        return result;
    }

    /// <summary>
    /// Calculates how many candlesticks can fit on the screen based on plot width.
    /// Each candlestick needs minimum space for body + wicks + spacing.
    /// </summary>
    private int CalculateFitToScreenCount()
    {
        try
        {
            // Get the plot area width in pixels
            var plotArea = formsPlot1.Plot.RenderManager.LastRender.DataRect;
            double plotWidthPixels = plotArea.Width;
            
            // Minimum pixels per candlestick (body + spacing)
            // Typical candlestick needs ~5-10 pixels to be readable
            // We'll use 8 pixels as a good balance
            const double minPixelsPerCandle = 8.0;
            
            // Calculate how many candlesticks can fit
            int count = (int)(plotWidthPixels / minPixelsPerCandle);
            
            // Ensure we have at least 50 and at most 2000 candlesticks
            count = Math.Max(50, Math.Min(2000, count));
            
            System.Diagnostics.Debug.WriteLine($"[FitToScreen] Plot width: {plotWidthPixels}px, Calculated count: {count}");
            
            return count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FitToScreen] Error calculating count: {ex.Message}");
            // Fallback to 500 if calculation fails
            return 500;
        }
    }

    private void ApplyViewMode()
    {
        if (_fullData == null) return;

        int totalCount = _fullData.Length;
        
        // Re-enable auto Y-scaling when applying a view mode
        _autoScaleY = true;
        
        try
        {
            if (_rbReset.Checked || _rbFullData.Checked)
            {
                // Show all data, let LOD decide which layer
                // Add 2% padding to X-axis
                double xPadding = totalCount * 0.02;
                formsPlot1.Plot.Axes.SetLimitsX(-xPadding, totalCount + xPadding);
                
                // Calculate Y limits from all data
                double yMin = double.MaxValue;
                double yMax = double.MinValue;
                for (int i = 0; i < _fullData.Length; i++)
                {
                    yMin = Math.Min(yMin, _fullData[i].Low);
                    yMax = Math.Max(yMax, _fullData[i].High);
                }
                
                // Add 5% padding to Y limits
                double yPadding = (yMax - yMin) * 0.05;
                formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                
                formsPlot1.Refresh();
            }
            else if (_rbFitToScreen.Checked)
            {
                // Get current X-axis limits
                var xAxis = formsPlot1.Plot.Axes.Bottom;
                double currentXMin = xAxis.Min;
                double currentXMax = xAxis.Max;
                double currentXRange = currentXMax - currentXMin;
                
                // Check if user has a custom view (zoomed/panned) or if showing full data
                bool isFullDataView = (currentXMin <= 0 && currentXMax >= totalCount - 1);
                
                if (isFullDataView)
                {
                    // Coming from Full Data/Reset - fit X-axis to show last N bars
                    int visibleCount = CalculateFitToScreenCount();
                    int startIdx = Math.Max(0, totalCount - visibleCount);
                    int endIdx = totalCount;
                    
                    // Add 2% padding to X-axis
                    double xRange = endIdx - startIdx;
                    double xPadding = xRange * 0.02;
                    formsPlot1.Plot.Axes.SetLimitsX(startIdx - xPadding, endIdx + xPadding);
                    
                    // Calculate Y limits from visible data
                    double yMin = double.MaxValue;
                    double yMax = double.MinValue;
                    for (int i = startIdx; i < endIdx && i < _fullData.Length; i++)
                    {
                        yMin = Math.Min(yMin, _fullData[i].Low);
                        yMax = Math.Max(yMax, _fullData[i].High);
                    }
                    
                    // Add 5% padding to Y limits
                    double yPadding = (yMax - yMin) * 0.05;
                    formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                    
                    System.Diagnostics.Debug.WriteLine($"[FitToScreen] Full data view - showing range: {startIdx} to {endIdx} ({visibleCount} bars), Y: {yMin:F2} to {yMax:F2}");
                }
                else
                {
                    // User has zoomed/panned - keep X-axis, only fit Y-axis
                    int visibleStartIdx = Math.Max(0, (int)Math.Floor(currentXMin));
                    int visibleEndIdx = Math.Min(_fullData.Length, (int)Math.Ceiling(currentXMax));
                    
                    // Calculate Y limits from currently visible data
                    double yMin = double.MaxValue;
                    double yMax = double.MinValue;
                    for (int i = visibleStartIdx; i < visibleEndIdx && i < _fullData.Length; i++)
                    {
                        yMin = Math.Min(yMin, _fullData[i].Low);
                        yMax = Math.Max(yMax, _fullData[i].High);
                    }
                    
                    // Add 5% padding to Y limits
                    double yPadding = (yMax - yMin) * 0.05;
                    formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                    
                    System.Diagnostics.Debug.WriteLine($"[FitToScreen] Custom view - keeping X-axis [{visibleStartIdx} to {visibleEndIdx}], fitting Y: {yMin:F2} to {yMax:F2}");
                }
                
                formsPlot1.Refresh();
            }
            else if (_rbLastN.Checked)
            {
                // Show last N points
                if (int.TryParse(_txtN.Text, out int n))
                {
                    int startIdx = Math.Max(0, totalCount - n);
                    
                    // Add 2% padding to X-axis
                    double xRange = totalCount - startIdx;
                    double xPadding = xRange * 0.02;
                    formsPlot1.Plot.Axes.SetLimitsX(startIdx - xPadding, totalCount + xPadding);
                    
                    // Calculate Y limits from visible data
                    double yMin = double.MaxValue;
                    double yMax = double.MinValue;
                    for (int i = startIdx; i < totalCount && i < _fullData.Length; i++)
                    {
                        yMin = Math.Min(yMin, _fullData[i].Low);
                        yMax = Math.Max(yMax, _fullData[i].High);
                    }
                    
                    // Add 5% padding to Y limits
                    double yPadding = (yMax - yMin) * 0.05;
                    formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                    
                    formsPlot1.Refresh();
                }
                else
                {
                    MessageBox.Show("Please enter a valid number for N", "Invalid Input");
                }
            }
            else if (_rbFirstN.Checked)
            {
                // Show first N points
                if (int.TryParse(_txtN.Text, out int n))
                {
                    int endIdx = Math.Min(totalCount, n);
                    
                    // Add 2% padding to X-axis
                    double xPadding = endIdx * 0.02;
                    formsPlot1.Plot.Axes.SetLimitsX(-xPadding, endIdx + xPadding);
                    
                    // Calculate Y limits from visible data
                    double yMin = double.MaxValue;
                    double yMax = double.MinValue;
                    for (int i = 0; i < endIdx && i < _fullData.Length; i++)
                    {
                        yMin = Math.Min(yMin, _fullData[i].Low);
                        yMax = Math.Max(yMax, _fullData[i].High);
                    }
                    
                    // Add 5% padding to Y limits
                    double yPadding = (yMax - yMin) * 0.05;
                    formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                    
                    formsPlot1.Refresh();
                }
                else
                {
                    MessageBox.Show("Please enter a valid number for N", "Invalid Input");
                }
            }
            else if (_rbRange.Checked)
            {
                // Show range [start, end]
                if (int.TryParse(_txtRangeStart.Text, out int start) && 
                    int.TryParse(_txtRangeEnd.Text, out int end))
                {
                    start = Math.Max(0, Math.Min(start, totalCount));
                    end = Math.Max(0, Math.Min(end, totalCount));
                    
                    if (start < end)
                    {
                        // Add 2% padding to X-axis
                        double xRange = end - start;
                        double xPadding = xRange * 0.02;
                        formsPlot1.Plot.Axes.SetLimitsX(start - xPadding, end + xPadding);
                        
                        // Calculate Y limits from visible data
                        double yMin = double.MaxValue;
                        double yMax = double.MinValue;
                        for (int i = start; i < end && i < _fullData.Length; i++)
                        {
                            yMin = Math.Min(yMin, _fullData[i].Low);
                            yMax = Math.Max(yMax, _fullData[i].High);
                        }
                        
                        // Add 5% padding to Y limits
                        double yPadding = (yMax - yMin) * 0.05;
                        formsPlot1.Plot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                        
                        formsPlot1.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("Start must be less than End", "Invalid Range");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter valid numbers for Start and End", "Invalid Input");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error applying view mode: {ex.Message}", "Error");
        }
    }
}

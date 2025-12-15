using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;

namespace ScottPlotOHLCWinForms
{
    public class GuiManager : IDisposable
    {
        private Form _parentForm;
        private FormsPlot _formsPlot;

        // Multiplot references
        private Plot _mainPlot;  // OHLC plot
        private Plot _smaPlot;   // SMA plot

        private OHLC[] _fullData;
        private OHLCLayer _ohlcLayer;
        private int _currentLayer = -1; // Start invalid to force update

        // Signal types
        public enum SignalType
        {
            None = 0,
            Buy = 1,
            Sell = 2,
            Flat = 3
        }

        // Trading signal data
        public class TradingSignal
        {
            public int Index { get; set; }
            public SignalType Type { get; set; }
            public double Price { get; set; }
        }

        // Signal configuration for adding signals to main plot
        public class SignalConfig
        {
            public string Name { get; set; } = "";
            public System.Drawing.Color Color { get; set; }
            public int LineWidth { get; set; } = 2;
            public double[] DataL0 { get; set; } = Array.Empty<double>();
            public double[] DataL1 { get; set; } = Array.Empty<double>();
            public double[] DataL2 { get; set; } = Array.Empty<double>();
            public double[] DataL3 { get; set; } = Array.Empty<double>();
            public double[] DataL4 { get; set; } = Array.Empty<double>();
        }

        // Internal tracking for signals added to main plot
        private class MainPlotSignal
        {
            public string Name = "";
            public ScottPlot.Plottables.Signal PlotL0;
            public ScottPlot.Plottables.Signal PlotL1;
            public ScottPlot.Plottables.Signal PlotL2;
            public ScottPlot.Plottables.Signal PlotL3;
            public ScottPlot.Plottables.Signal PlotL4;
        }

        // Trading signal markers with LOD support
        private class TradingMarkerLayer
        {
            public ScottPlot.Plottables.Scatter BuyMarkersL0, BuyMarkersL1, BuyMarkersL2, BuyMarkersL3, BuyMarkersL4;
            public ScottPlot.Plottables.Scatter SellMarkersL0, SellMarkersL1, SellMarkersL2, SellMarkersL3, SellMarkersL4;
            public ScottPlot.Plottables.Scatter FlatMarkersL0, FlatMarkersL1, FlatMarkersL2, FlatMarkersL3, FlatMarkersL4;
        }

        private TradingMarkerLayer? _tradingMarkers = null;

        // OHLC Layer structure
        private class OHLCLayer
        {
            public OHLC[] DataL0; // Full 2M points
            public OHLC[] DataL1; // x100 aggregation
            public OHLC[] DataL2; // x1000 aggregation
            public OHLC[] DataL3; // x10000 aggregation
            public OHLC[] DataL4; // x10 aggregation
            public ScottPlot.Plottables.CandlestickPlot PlotL0;
            public ScottPlot.Plottables.CandlestickPlot PlotL1;
            public ScottPlot.Plottables.CandlestickPlot PlotL2;
            public ScottPlot.Plottables.CandlestickPlot PlotL3;
            public ScottPlot.Plottables.CandlestickPlot PlotL4;
        }

        // SMA Layer structure
        private class SMALayer
        {
            public int Period;
            public System.Drawing.Color Color;
            public double[] DataL0; // Full 2M points
            public double[] DataL1; // x100 aggregation
            public double[] DataL2; // x1000 aggregation
            public double[] DataL3; // x10000 aggregation
            public double[] DataL4; // x10 aggregation
            public ScottPlot.Plottables.Signal PlotL0;
            public ScottPlot.Plottables.Signal PlotL1;
            public ScottPlot.Plottables.Signal PlotL2;
            public ScottPlot.Plottables.Signal PlotL3;
            public ScottPlot.Plottables.Signal PlotL4;
        }

        private List<SMALayer> _smaLayers;
        private List<MainPlotSignal> _mainPlotSignals;
        private int _currentSMALayer = -1;
        
        // MACD Layer structure
        private class MACDLayer
        {
            public double[] MacdLineL0, MacdLineL1, MacdLineL2, MacdLineL3, MacdLineL4;
            public double[] SignalLineL0, SignalLineL1, SignalLineL2, SignalLineL3, SignalLineL4;
            public double[] HistogramL0, HistogramL1, HistogramL2, HistogramL3, HistogramL4;
            public ScottPlot.Plottables.Signal MacdPlotL0, MacdPlotL1, MacdPlotL2, MacdPlotL3, MacdPlotL4;
            public ScottPlot.Plottables.Signal SignalPlotL0, SignalPlotL1, SignalPlotL2, SignalPlotL3, SignalPlotL4;
            public ScottPlot.Plottables.Signal HistogramPlotL0, HistogramPlotL1, HistogramPlotL2, HistogramPlotL3, HistogramPlotL4;
        }

        // RSI Layer structure
        private class RSILayer
        {
            public double[] DataL0, DataL1, DataL2, DataL3, DataL4;
            public ScottPlot.Plottables.Signal PlotL0, PlotL1, PlotL2, PlotL3, PlotL4;
        }

        private Plot _macdPlot;
        private Plot _rsiPlot;
        private MACDLayer _macdLayer;
        private RSILayer _rsiLayer;
        private int _currentMACDLayer = -1;
        private int _currentRSILayer = -1;

        // Crosshairs for each plot
        private ScottPlot.Plottables.Crosshair _mainCrosshair;
        private ScottPlot.Plottables.Crosshair _smaCrosshair;
        private ScottPlot.Plottables.Crosshair _macdCrosshair;
        private ScottPlot.Plottables.Crosshair _rsiCrosshair;

        // View mode controls
        private Panel _controlPanel;
        private RadioButton _rbLastFit;
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
        private RadioButton _rbAlignRight;
        private Button _btnAddRow;
        private Button _btnDeleteRow;
        private Button _btnSetupMultiplot;

        private bool _autoScaleY = true; // Flag to enable/disable automatic Y-axis scaling

        // Scrollbars
        private HScrollBar _hScrollBar;
        private VScrollBar _vScrollBar;
        private bool _isUpdatingScrollBars = false; // Prevent recursive updates

        // Global Data Limits
        private double _globalYMin = 0;
        private double _globalYMax = 100;
        private const int Y_SCROLL_RESOLUTION = 10000;

        public GuiManager(Form parentForm)
        {
            _parentForm = parentForm;
            _mainPlotSignals = new List<MainPlotSignal>();
        }

        public void Initialize()
        {
            // Create a container panel for plot and scrollbars
            Panel plotContainer = new Panel
            {
                Dock = DockStyle.Fill
            };
            _parentForm.Controls.Add(plotContainer);

            // Initialize FormsPlot - fills the container
            _formsPlot = new FormsPlot() { Dock = DockStyle.Fill };
            plotContainer.Controls.Add(_formsPlot);

            // Initialize VScrollBar - docked to right, standard width
            _vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Minimum = 0,
                Maximum = 10000,
                SmallChange = 1,
                LargeChange = 1000
            };
            plotContainer.Controls.Add(_vScrollBar);

            // Initialize HScrollBar - docked to bottom, standard height
            _hScrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                Minimum = 0,
                Maximum = 10000,
                SmallChange = 1,
                LargeChange = 1000
            };
            plotContainer.Controls.Add(_hScrollBar);

            // Scrollbar Events
            _hScrollBar.Scroll += (s, e) => OnHScroll();
            _vScrollBar.Scroll += (s, e) => OnVScroll();

            // Create control panel last (will dock to top)
            _controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = System.Drawing.Color.LightGray
            };
            _parentForm.Controls.Add(_controlPanel);

            // Row 1: Radio buttons
            _rbLastFit = new RadioButton { Text = "Last Fit", Location = new Point(10, 10), AutoSize = true };
            _rbFullData = new RadioButton { Text = "Full Data", Location = new Point(100, 10), AutoSize = true, Checked = true };
            _rbFitToScreen = new RadioButton { Text = "Fit to Screen", Location = new Point(200, 10), AutoSize = true };
            _rbLastN = new RadioButton { Text = "Last N:", Location = new Point(330, 10), AutoSize = true };
            _rbFirstN = new RadioButton { Text = "First N:", Location = new Point(430, 10), AutoSize = true };
            _rbRange = new RadioButton { Text = "Range:", Location = new Point(530, 10), AutoSize = true };
            _rbReset = new RadioButton { Text = "Reset", Location = new Point(630, 10), AutoSize = true };
            _rbAlignRight = new RadioButton { Text = "Align Right", Location = new Point(720, 10), AutoSize = true };

            _controlPanel.Controls.Add(_rbLastFit);
            _controlPanel.Controls.Add(_rbFullData);
            _controlPanel.Controls.Add(_rbFitToScreen);
            _controlPanel.Controls.Add(_rbLastN);
            _controlPanel.Controls.Add(_rbFirstN);
            _controlPanel.Controls.Add(_rbRange);
            _controlPanel.Controls.Add(_rbReset);
            _controlPanel.Controls.Add(_rbAlignRight);

            // Row 2: Input controls
            _txtN = new TextBox { Location = new Point(250, 40), Width = 80, Text = "1000" };
            _txtRangeStart = new TextBox { Location = new Point(450, 40), Width = 80, Text = "0", PlaceholderText = "Start" };
            _txtRangeEnd = new TextBox { Location = new Point(540, 40), Width = 80, Text = "1000", PlaceholderText = "End" };
            _btnApply = new Button { Text = "Apply", Location = new Point(640, 38), Width = 80 };

            _controlPanel.Controls.Add(_txtN);
            _controlPanel.Controls.Add(_txtRangeStart);
            _controlPanel.Controls.Add(_txtRangeEnd);
            _controlPanel.Controls.Add(_btnApply);// Subplot management buttons

            _btnAddRow = new Button { Text = "Add Row", Location = new Point(820, 38), Width = 80 };
            _btnDeleteRow = new Button { Text = "Delete Row", Location = new Point(910, 38), Width = 90 };
            _btnSetupMultiplot = new Button { Text = "Setup Multiplot", Location = new Point(1020, 38), Width = 110 };
            _controlPanel.Controls.Add(_btnSetupMultiplot);
            _controlPanel.Controls.Add(_btnAddRow);
            _controlPanel.Controls.Add(_btnDeleteRow);

            // Add Row button click handler
            _btnAddRow.Click += (s, e) =>
            {
                Plot plot = _formsPlot.Multiplot.AddPlot();
                plot.Axes.Left.LockSize(50);
                plot.Axes.Right.LockSize(20);
                _formsPlot.Multiplot.CollapseVertically();
                _formsPlot.Refresh();
            };

            // Delete Row button click handler
            _btnDeleteRow.Click += (s, e) =>
            {
                if (_formsPlot.Multiplot.Subplots.Count < 2)
                    return;

                Plot plotToRemove = _formsPlot.Multiplot.Subplots.GetPlots().Last();
                _formsPlot.Multiplot.RemovePlot(plotToRemove);

                Plot newBottomPlot = _formsPlot.Multiplot.Subplots.GetPlots().Last();
                newBottomPlot.Axes.Bottom.ResetSize();
                newBottomPlot.Axes.Bottom.TickGenerator = plotToRemove.Axes.Bottom.TickGenerator;

                _formsPlot.Refresh();
            };



            // Setup Multiplot button click handler
            _btnSetupMultiplot.Click += (s, e) =>
            {
                // Clear all existing plots
                _formsPlot.Multiplot.Reset();

                // Re-setup plots
                //SetupMultiplot();

                _formsPlot.Refresh();
            };

            // Apply button click handler
            _btnApply.Click += (s, e) => ApplyViewMode();

            SetupPlots();
        }

        private void SetupPlots()
        {
            // Setup Multiplot with 2 subplots (OHLC + SMA + MACD + RSI)
            _formsPlot.Multiplot.AddPlots(4);
            _formsPlot.Multiplot.CollapseVertically();

            // Get plot references
            _mainPlot = _formsPlot.Multiplot.GetPlot(0);
            _smaPlot = _formsPlot.Multiplot.GetPlot(1);
            _macdPlot = _formsPlot.Multiplot.GetPlot(2);
            _rsiPlot = _formsPlot.Multiplot.GetPlot(3);            

            // Standardize axes sizes for alignment (same as MultiplotDraggable example)
            foreach (Plot plot in _formsPlot.Multiplot.GetPlots())
            {
                //plot.Axes.Left.MinimumSize = 60;
                //plot.Axes.Left.MaximumSize = 60;
                //plot.Axes.Right.MinimumSize = 60;
                //plot.Axes.Right.MaximumSize = 60;
            }

            // use the same size for all right axes to ensure alignment regardless of tick label length
            foreach (Plot plot in _formsPlot.Multiplot.GetPlots())
            {
                plot.Axes.Left.LockSize(50);
                plot.Axes.Right.LockSize(20);
            }

            // Share X-axis between plots
            _formsPlot.Multiplot.SharedAxes.ShareX(new[] { _mainPlot, _smaPlot, _macdPlot, _rsiPlot });

            // Setup grids to use ticks from the bottom plot (SMA plot)
            _mainPlot.Grid.XAxis = _smaPlot.Axes.Bottom;
            _smaPlot.Grid.XAxis = _rsiPlot.Axes.Bottom;
            _macdPlot.Grid.XAxis = _rsiPlot.Axes.Bottom;   
            _rsiPlot.Grid.XAxis = _rsiPlot.Axes.Bottom;   

            // Setup individual plots
            SetupMainPlot();
            SetupSMAPlot();
            SetupMACDPlot();
            SetupRSIPlot();

            // Setup unified crosshair for all plots
            _formsPlot.MouseMove += (s, e) =>
            {
                if (_fullData == null) return; // Wait for data

                // Determine which plot the mouse is over
                var mousePixel = new ScottPlot.Pixel(e.X, e.Y);

                // Update all crosshairs with same X coordinate but different Y based on each plot's coordinates
                bool isOverAnyPlot = false;

                // Check main plot
                if (_mainPlot.RenderManager.LastRender.DataRect.Contains(mousePixel))
                {
                    var coords = _mainPlot.GetCoordinates(e.X, e.Y);
                    _mainCrosshair.Position = coords;
                    _mainCrosshair.IsVisible = true;
                    isOverAnyPlot = true;

                    // Update other plots with same X but their own Y coordinates
                    _smaCrosshair.Position = _smaPlot.GetCoordinates(e.X, e.Y);
                    _smaCrosshair.IsVisible = true;
                    _macdCrosshair.Position = _macdPlot.GetCoordinates(e.X, e.Y);
                    _macdCrosshair.IsVisible = true;
                    _rsiCrosshair.Position = _rsiPlot.GetCoordinates(e.X, e.Y);
                    _rsiCrosshair.IsVisible = true;
                }
                // Check SMA plot
                else if (_smaPlot.RenderManager.LastRender.DataRect.Contains(mousePixel))
                {
                    var coords = _smaPlot.GetCoordinates(e.X, e.Y);
                    _smaCrosshair.Position = coords;
                    _smaCrosshair.IsVisible = true;
                    isOverAnyPlot = true;

                    // Update other plots
                    _mainCrosshair.Position = _mainPlot.GetCoordinates(e.X, e.Y);
                    _mainCrosshair.IsVisible = true;
                    _macdCrosshair.Position = _macdPlot.GetCoordinates(e.X, e.Y);
                    _macdCrosshair.IsVisible = true;
                    _rsiCrosshair.Position = _rsiPlot.GetCoordinates(e.X, e.Y);
                    _rsiCrosshair.IsVisible = true;
                }
                // Check MACD plot
                else if (_macdPlot.RenderManager.LastRender.DataRect.Contains(mousePixel))
                {
                    var coords = _macdPlot.GetCoordinates(e.X, e.Y);
                    _macdCrosshair.Position = coords;
                    _macdCrosshair.IsVisible = true;
                    isOverAnyPlot = true;

                    // Update other plots
                    _mainCrosshair.Position = _mainPlot.GetCoordinates(e.X, e.Y);
                    _mainCrosshair.IsVisible = true;
                    _smaCrosshair.Position = _smaPlot.GetCoordinates(e.X, e.Y);
                    _smaCrosshair.IsVisible = true;
                    _rsiCrosshair.Position = _rsiPlot.GetCoordinates(e.X, e.Y);
                    _rsiCrosshair.IsVisible = true;
                }
                // Check RSI plot
                else if (_rsiPlot.RenderManager.LastRender.DataRect.Contains(mousePixel))
                {
                    var coords = _rsiPlot.GetCoordinates(e.X, e.Y);
                    _rsiCrosshair.Position = coords;
                    _rsiCrosshair.IsVisible = true;
                    isOverAnyPlot = true;

                    // Update other plots
                    _mainCrosshair.Position = _mainPlot.GetCoordinates(e.X, e.Y);
                    _mainCrosshair.IsVisible = true;
                    _smaCrosshair.Position = _smaPlot.GetCoordinates(e.X, e.Y);
                    _smaCrosshair.IsVisible = true;
                    _macdCrosshair.Position = _macdPlot.GetCoordinates(e.X, e.Y);
                    _macdCrosshair.IsVisible = true;
                }

                if (!isOverAnyPlot)
                {
                    // Hide all crosshairs if mouse is not over any plot
                    _mainCrosshair.IsVisible = false;
                    _smaCrosshair.IsVisible = false;
                    _macdCrosshair.IsVisible = false;
                    _rsiCrosshair.IsVisible = false;
                }

                _formsPlot.Refresh();
            };

            // Trading sinyallerini �ret ve ekle
            //var tradingSignals = GenerateBuySellFlatSignals(50, 200, "SMA", 5, -3);
            //AddTradingSignalsToPlot(tradingSignals);

            AddSignalsToMainPlot();

            // Apply the default view mode (Full Data)
            ApplyViewMode();
        }

        public Plot AddPlot()
        {
            Plot plot = _formsPlot.Multiplot.AddPlot();

            // Standardize axes sizes for alignment
            plot.Axes.Left.MinimumSize = 60;
            plot.Axes.Left.MaximumSize = 60;
            plot.Axes.Right.MinimumSize = 60;
            plot.Axes.Right.MaximumSize = 60;

            _formsPlot.Multiplot.CollapseVertically();
            _formsPlot.Refresh();

            return plot;
        }
        private void SetupMainPlot()
        {
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] ========== STARTING MAIN PLOT SETUP ==========");

            // Generate 2M OHLC points
            int pointCount = 2_000_000;
            _fullData = GenerateOHLCs(pointCount);
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] _fullData.Length = {_fullData.Length}");

            // Initialize OHLC layer
            _ohlcLayer = new OHLCLayer();

            // Pre-calculate LOD layers
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] ===== Calculating OHLC LOD Layers =====");
            _ohlcLayer.DataL0 = _fullData;                      // 2M points (Full)
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] OHLC L0: {_ohlcLayer.DataL0.Length} points");

            _ohlcLayer.DataL1 = AggregateData(_fullData, 100);   // 20k points
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] OHLC L1: {_ohlcLayer.DataL1.Length} points");

            _ohlcLayer.DataL2 = AggregateData(_fullData, 1000);  // 2k points
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] OHLC L2: {_ohlcLayer.DataL2.Length} points");

            _ohlcLayer.DataL3 = AggregateData(_fullData, 10000); // 200 points
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] OHLC L3: {_ohlcLayer.DataL3.Length} points");

            _ohlcLayer.DataL4 = AggregateData(_fullData, 10);    // 200k points
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] OHLC L4: {_ohlcLayer.DataL4.Length} points");

            // Add ALL plots initially, but hide them
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] Adding Candlestick plots...");

            // Layer 3 (Very Coarse - extreme zoom out) - visible by default
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] Adding L3 Candlestick plot...");
            _ohlcLayer.PlotL3 = _mainPlot.Add.Candlestick(_ohlcLayer.DataL3);
            _ohlcLayer.PlotL3.Sequential = false;
            _ohlcLayer.PlotL3.IsVisible = true; // Start visible
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] L3 plot added: Sequential=false, IsVisible=true");

            // Layer 2 (Coarse)
            _ohlcLayer.PlotL2 = _mainPlot.Add.Candlestick(_ohlcLayer.DataL2);
            _ohlcLayer.PlotL2.Sequential = false;
            _ohlcLayer.PlotL2.IsVisible = false;

            // Layer 1 (Medium)
            _ohlcLayer.PlotL1 = _mainPlot.Add.Candlestick(_ohlcLayer.DataL1);
            _ohlcLayer.PlotL1.Sequential = false;
            _ohlcLayer.PlotL1.IsVisible = false;

            // Layer 4 (Fine - between L1 and L0)
            _ohlcLayer.PlotL4 = _mainPlot.Add.Candlestick(_ohlcLayer.DataL4);
            _ohlcLayer.PlotL4.Sequential = false;
            _ohlcLayer.PlotL4.IsVisible = false;

            // Layer 0 (Full)
            _ohlcLayer.PlotL0 = _mainPlot.Add.Candlestick(_ohlcLayer.DataL0);
            _ohlcLayer.PlotL0.Sequential = true; // Enable Sequential for full data
            _ohlcLayer.PlotL0.IsVisible = false;

            _currentLayer = 3;
            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] Initial layer set to: {_currentLayer}");

            // Mouse wheel zoom is already enabled by default for X axis
            // Left-click drag for pan is already enabled
            // We just need to add Ctrl+Wheel for Y axis zoom

            bool isCtrlPressed = false;

            _parentForm.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.ControlKey)
                    isCtrlPressed = true;
            };

            _parentForm.KeyUp += (s, e) =>
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
                var axis = _mainPlot.Axes.Bottom;
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
            _mainPlot.Axes.Bottom.TickGenerator = tickGen;

            // Detect Y-axis pan to disable auto-scaling
            double lastYMin = 0;
            double lastYMax = 0;
            bool isDragging = false;
            System.Drawing.Point mouseDownLocation = System.Drawing.Point.Empty;

            _formsPlot.MouseDown += (s, e) =>
            {
                lastYMin = _mainPlot.Axes.Left.Min;
                lastYMax = _mainPlot.Axes.Left.Max;
                isDragging = true;
                mouseDownLocation = e.Location;
                System.Diagnostics.Debug.WriteLine($"[Y-Axis] MouseDown: Button={e.Button}, Location={e.Location}");
            };

            _formsPlot.MouseUp += (s, e) =>
            {
                if (!isDragging) return;
                isDragging = false;

                // Check if Y-axis changed (user panned Y-axis)
                double currentYMin = _mainPlot.Axes.Left.Min;
                double currentYMax = _mainPlot.Axes.Left.Max;

                // Calculate how much the mouse moved
                int deltaX = Math.Abs(e.Location.X - mouseDownLocation.X);
                int deltaY = Math.Abs(e.Location.Y - mouseDownLocation.Y);

                System.Diagnostics.Debug.WriteLine($"[Y-Axis] MouseUp: Button={e.Button}, DeltaX={deltaX}, DeltaY={deltaY}, YAxisChanged={Math.Abs(currentYMin - lastYMin) > 0.001}");

                // Only disable auto-scale if:
                // 1. Right mouse button was used (Y-axis pan)
                // 2. OR significant vertical movement detected with left button
                // 3. AND Y-axis actually changed
                bool yAxisChanged = Math.Abs(currentYMin - lastYMin) > 0.001 || Math.Abs(currentYMax - lastYMax) > 0.001;
                bool verticalMovement = deltaY > 10; // More than 10 pixels vertical movement
                bool rightButtonUsed = e.Button == MouseButtons.Right;

                if (yAxisChanged && (rightButtonUsed || verticalMovement))
                {
                    // Y-axis was manually changed, disable auto-scaling
                    _autoScaleY = false;
                    System.Diagnostics.Debug.WriteLine("[Y-Axis] Manual pan detected, auto-scaling disabled");
                }
            };

            // Handle middle mouse button click to reset view and re-enable auto-scale
            _formsPlot.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Middle)
                {
                    System.Diagnostics.Debug.WriteLine("[Y-Axis] Middle button clicked - re-enabling auto-scale");
                    _autoScaleY = true;
                }
            };

            // Add Crosshair to main plot
            _mainCrosshair = _mainPlot.Add.Crosshair(0, 0);
            _mainCrosshair.IsVisible = false;

            // Update Scrollbars on Render
            _mainPlot.RenderManager.RenderStarting += (s, e) =>
            {
                if (!_isUpdatingScrollBars && !_parentForm.IsDisposed)
                {
                    // Use BeginInvoke to avoid blocking the render thread and ensure UI thread execution
                    _parentForm.BeginInvoke((MethodInvoker)UpdateScrollBarsFromPlot);
                }
            };

            // LOD Logic
            _mainPlot.RenderManager.RenderStarting += (s, e) =>
            {
                try
                {
                    if (_fullData == null) return;

                    var axis = _mainPlot.Axes.Bottom;
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

                        _ohlcLayer.PlotL0.IsVisible = (targetLayer == 0);
                        _ohlcLayer.PlotL1.IsVisible = (targetLayer == 1);
                        _ohlcLayer.PlotL2.IsVisible = (targetLayer == 2);
                        _ohlcLayer.PlotL3.IsVisible = (targetLayer == 3);
                        _ohlcLayer.PlotL4.IsVisible = (targetLayer == 4);

                        // Switch signals added to main plot as well
                        foreach (var signal in _mainPlotSignals)
                        {
                            signal.PlotL0.IsVisible = (targetLayer == 0);
                            signal.PlotL1.IsVisible = (targetLayer == 1);
                            signal.PlotL2.IsVisible = (targetLayer == 2);
                            signal.PlotL3.IsVisible = (targetLayer == 3);
                            signal.PlotL4.IsVisible = (targetLayer == 4);
                        }

                        // Switch trading marker visibility as well
                        if (_tradingMarkers != null)
                        {
                            _tradingMarkers.BuyMarkersL0.IsVisible = (targetLayer == 0);
                            _tradingMarkers.BuyMarkersL1.IsVisible = (targetLayer == 1);
                            _tradingMarkers.BuyMarkersL2.IsVisible = (targetLayer == 2);
                            _tradingMarkers.BuyMarkersL3.IsVisible = (targetLayer == 3);
                            _tradingMarkers.BuyMarkersL4.IsVisible = (targetLayer == 4);

                            _tradingMarkers.SellMarkersL0.IsVisible = (targetLayer == 0);
                            _tradingMarkers.SellMarkersL1.IsVisible = (targetLayer == 1);
                            _tradingMarkers.SellMarkersL2.IsVisible = (targetLayer == 2);
                            _tradingMarkers.SellMarkersL3.IsVisible = (targetLayer == 3);
                            _tradingMarkers.SellMarkersL4.IsVisible = (targetLayer == 4);

                            _tradingMarkers.FlatMarkersL0.IsVisible = (targetLayer == 0);
                            _tradingMarkers.FlatMarkersL1.IsVisible = (targetLayer == 1);
                            _tradingMarkers.FlatMarkersL2.IsVisible = (targetLayer == 2);
                            _tradingMarkers.FlatMarkersL3.IsVisible = (targetLayer == 3);
                            _tradingMarkers.FlatMarkersL4.IsVisible = (targetLayer == 4);
                        }

                        _currentLayer = targetLayer;
                    }

                    // Update Title with aggregation info
                    int displayedCount = 0;
                    string aggInfo = "";
                    int logicalLayer = 0; // Logical layer number based on zoom level

                    if (_currentLayer == 0)
                    {
                        displayedCount = _ohlcLayer.DataL0.Length;
                        aggInfo = "Full Data";
                        logicalLayer = 0; // Most zoomed in
                    }
                    else if (_currentLayer == 4)
                    {
                        displayedCount = _ohlcLayer.DataL4.Length;
                        aggInfo = "x10";
                        logicalLayer = 1;
                    }
                    else if (_currentLayer == 1)
                    {
                        displayedCount = _ohlcLayer.DataL1.Length;
                        aggInfo = "x100";
                        logicalLayer = 2;
                    }
                    else if (_currentLayer == 2)
                    {
                        displayedCount = _ohlcLayer.DataL2.Length;
                        aggInfo = "x1000";
                        logicalLayer = 3;
                    }
                    else // _currentLayer == 3
                    {
                        displayedCount = _ohlcLayer.DataL3.Length;
                        aggInfo = "x10000";
                        logicalLayer = 4; // Most zoomed out
                    }

                    _mainPlot.Title($"Total: {_fullData.Length:N0} | Showing: {displayedCount:N0} ({aggInfo}) | Layer: {logicalLayer}");

                    // Auto-scale Y axis based on visible X range (only if auto-scaling is enabled)
                    if (_autoScaleY)
                    {
                        var xAxis = _mainPlot.Axes.Bottom;
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
                                _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CRASH] RenderStarting: {ex.Message}");
                }
            };

            _mainPlot.Title($"OHLC Chart with {pointCount:N0} Points (LOD)");

            // Set initial view to show last 500 bars for better visibility
            int initialVisibleCount = 500;
            int startIdx = Math.Max(0, pointCount - initialVisibleCount);
            int endIdx = pointCount;

            double xRange = endIdx - startIdx;
            double xPadding = xRange * 0.02;
            _mainPlot.Axes.SetLimitsX(startIdx - xPadding, endIdx + xPadding);

            // Calculate initial Y limits
            double yMin = double.MaxValue;
            double yMax = double.MinValue;
            for (int i = startIdx; i < endIdx && i < _fullData.Length; i++)
            {
                yMin = Math.Min(yMin, _fullData[i].Low);
                yMax = Math.Max(yMax, _fullData[i].High);
            }

            if (yMin < yMax)
            {
                double yPadding = (yMax - yMin) * 0.05;
                _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
            }

            System.Diagnostics.Debug.WriteLine($"[SetupMainPlot] Initial view set: X=[{startIdx}, {endIdx}], Y=[{yMin:F2}, {yMax:F2}]");

            // Initial Scrollbar Update
            UpdateScrollBarsFromPlot();

            _formsPlot.Refresh();
        }


        private void SetupSMAPlot()
        {
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] ========== STARTING SMA PLOT SETUP ==========");
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] _fullData.Length = {_fullData.Length}");
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] _smaPlot exists = {_smaPlot != null}");

            // Initialize SMA layers list
            _smaLayers = new List<SMALayer>();

            // Define SMA periods and colors
            var smaConfigs = new[]
            {
                //(Period: 20, Color: System.Drawing.Color.Blue),
                (Period: 50, Color: System.Drawing.Color.Orange),
                (Period: 200, Color: System.Drawing.Color.Red)
            };

            foreach (var config in smaConfigs)
            {
                var layer = new SMALayer
                {
                    Period = config.Period,
                    Color = config.Color
                };

                // Calculate SMA for each LOD level
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] ===== Calculating SMA{config.Period} =====");

                layer.DataL0 = IndicatorManager.CalculateSMA(_fullData, config.Period);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA{config.Period} L0: {layer.DataL0.Length} points");

                layer.DataL4 = IndicatorManager.CalculateSMA(_ohlcLayer.DataL4, config.Period);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA{config.Period} L4: {layer.DataL4.Length} points");

                layer.DataL1 = IndicatorManager.CalculateSMA(_ohlcLayer.DataL1, config.Period);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA{config.Period} L1: {layer.DataL1.Length} points");

                layer.DataL2 = IndicatorManager.CalculateSMA(_ohlcLayer.DataL2, config.Period);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA{config.Period} L2: {layer.DataL2.Length} points");

                layer.DataL3 = IndicatorManager.CalculateSMA(_ohlcLayer.DataL3, config.Period);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA{config.Period} L3: {layer.DataL3.Length} points");

                // Convert System.Drawing.Color to ScottPlot.Color
                var scottPlotColor = new ScottPlot.Color(config.Color.R, config.Color.G, config.Color.B, config.Color.A);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] Color: R={config.Color.R}, G={config.Color.G}, B={config.Color.B}");

                // Add Signal plots for each LOD level with Period and OffsetX
                // L3 (Most zoomed out) - visible by default
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] Adding L3 Signal plot...");
                layer.PlotL3 = _smaPlot.Add.Signal(layer.DataL3);
                layer.PlotL3.Color = scottPlotColor;
                layer.PlotL3.LineWidth = 2;
                layer.PlotL3.Data.Period = 10000; // Each point represents 10000 indices
                layer.PlotL3.Data.XOffset = 5000; // Center of first aggregation (10000/2)
                layer.PlotL3.IsVisible = true;
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] L3 plot added: Period=10000, Offset=5000");

                // L2
                layer.PlotL2 = _smaPlot.Add.Signal(layer.DataL2);
                layer.PlotL2.Color = scottPlotColor;
                layer.PlotL2.LineWidth = 2;
                layer.PlotL2.Data.Period = 1000;
                layer.PlotL2.Data.XOffset = 500;
                layer.PlotL2.IsVisible = false;

                // L1
                layer.PlotL1 = _smaPlot.Add.Signal(layer.DataL1);
                layer.PlotL1.Color = scottPlotColor;
                layer.PlotL1.LineWidth = 2;
                layer.PlotL1.Data.Period = 100;
                layer.PlotL1.Data.XOffset = 50;
                layer.PlotL1.IsVisible = false;

                // L4
                layer.PlotL4 = _smaPlot.Add.Signal(layer.DataL4);
                layer.PlotL4.Color = scottPlotColor;
                layer.PlotL4.LineWidth = 2;
                layer.PlotL4.Data.Period = 10;
                layer.PlotL4.Data.XOffset = 5;
                layer.PlotL4.IsVisible = false;

                // L0 (Full data)
                layer.PlotL0 = _smaPlot.Add.Signal(layer.DataL0);
                layer.PlotL0.Color = scottPlotColor;
                layer.PlotL0.LineWidth = 2;
                layer.PlotL0.Data.Period = 1;
                layer.PlotL0.Data.XOffset = 0;
                layer.PlotL0.IsVisible = false;

                _smaLayers.Add(layer);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA{config.Period} layer added to list");
            }

            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] Total SMA layers: {_smaLayers.Count}");

            _currentSMALayer = 3; // Start with L3 (most zoomed out)

            // Add LOD Logic for SMA plot
            _smaPlot.RenderManager.RenderStarting += (s, e) =>
            {
                try
                {
                    if (_fullData == null) return;

                    var axis = _smaPlot.Axes.Bottom;
                    double range = axis.Max - axis.Min;
                    System.Diagnostics.Debug.WriteLine($"[SMA Render] X range: {axis.Min:F2} to {axis.Max:F2}, range={range:F2}");

                    // Decide which layer to use (same logic as OHLC)
                    int targetLayer = _currentSMALayer;

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
                    if (targetLayer != _currentSMALayer)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SMA LOD Switch] Range: {range:N0} -> Switching to Layer {targetLayer}");

                        foreach (var layer in _smaLayers)
                        {
                            layer.PlotL0.IsVisible = (targetLayer == 0);
                            layer.PlotL1.IsVisible = (targetLayer == 1);
                            layer.PlotL2.IsVisible = (targetLayer == 2);
                            layer.PlotL3.IsVisible = (targetLayer == 3);
                            layer.PlotL4.IsVisible = (targetLayer == 4);
                        }

                        _currentSMALayer = targetLayer;
                    }

                    // Sync Y axis with main plot (OHLC) since SMAs are based on price
                    var mainYAxis = _mainPlot.Axes.Left;
                    _smaPlot.Axes.SetLimitsY(mainYAxis.Min, mainYAxis.Max);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CRASH] SMA RenderStarting: {ex.Message}");
                }
            };

            //_smaPlot.Title("SMA Indicators (20, 50, 200)");
            //_smaPlot.Axes.Right.Label.Text = "SMA";

            // Add the same DateTime formatter for SMA plot's X axis
            ScottPlot.TickGenerators.NumericAutomatic smaTickGen = new ScottPlot.TickGenerators.NumericAutomatic();
            smaTickGen.LabelFormatter = (x) =>
            {
                int index = (int)x;
                if (index < 0 || index >= _fullData.Length) return "";

                var axis = _smaPlot.Axes.Bottom;
                double visibleRange = axis.Max - axis.Min;

                DateTime dt = _fullData[index].DateTime;

                if (visibleRange > 1440) // More than 1 day
                {
                    return dt.ToString("yyyy.MM.dd");
                }
                else // Intraday - show time too
                {
                    return dt.ToString("yyyy.MM.dd HH:mm:ss");
                }
            };
            _smaPlot.Axes.Bottom.TickGenerator = smaTickGen;

            // Set initial Y-axis limits for SMA plot
            // Calculate global Y limits from SMA data
            double smaYMin = double.MaxValue;
            double smaYMax = double.MinValue;

            foreach (var layer in _smaLayers)
            {
                foreach (var value in layer.DataL3)
                {
                    if (!double.IsNaN(value))
                    {
                        smaYMin = Math.Min(smaYMin, value);
                        smaYMax = Math.Max(smaYMax, value);
                    }
                }
            }

            if (smaYMin < smaYMax)
            {
                double yPadding = (smaYMax - smaYMin) * 0.05;
                _smaPlot.Axes.SetLimitsY(smaYMin - yPadding, smaYMax + yPadding);
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] Y limits set: {smaYMin - yPadding:F2} to {smaYMax + yPadding:F2}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] WARNING: Invalid Y limits: smaYMin={smaYMin}, smaYMax={smaYMax}");
            }

            // Set initial X-axis limits to match main plot
            // This ensures SMA plot is visible on startup
            var mainXAxis = _mainPlot.Axes.Bottom;
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] Main plot X limits: {mainXAxis.Min:F2} to {mainXAxis.Max:F2}");
            _smaPlot.Axes.SetLimitsX(mainXAxis.Min, mainXAxis.Max);
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMA plot X limits set: {_smaPlot.Axes.Bottom.Min:F2} to {_smaPlot.Axes.Bottom.Max:F2}");

            // Add Crosshair to SMA plot
            _smaCrosshair = _smaPlot.Add.Crosshair(0, 0);
            _smaCrosshair.IsVisible = false;

            // Also add SMAs to main OHLC plot as overlay
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] Adding SMAs to main OHLC plot...");
            foreach (var layer in _smaLayers)
            {
                AddSignalToMainPlot(new SignalConfig
                {
                    Name = $"SMA{layer.Period}",
                    Color = layer.Color,
                    LineWidth = 2,
                    DataL0 = layer.DataL0,
                    DataL1 = layer.DataL1,
                    DataL2 = layer.DataL2,
                    DataL3 = layer.DataL3,
                    DataL4 = layer.DataL4
                });
            }
            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] SMAs added to main plot. Total: {_smaLayers.Count}");

            System.Diagnostics.Debug.WriteLine($"[SetupSMAPlot] ========== SMA PLOT SETUP COMPLETE ==========");
        }
        private void SetupMACDPlot()
        {
            System.Diagnostics.Debug.WriteLine($"[SetupMACDPlot] ========== STARTING MACD PLOT SETUP ==========");
            // Initialize MACD layer
            _macdLayer = new MACDLayer();
            // Calculate MACD for each LOD level
            var (macdL0, signalL0, histL0) = IndicatorManager.CalculateMACD(_fullData);
            _macdLayer.MacdLineL0 = macdL0;
            _macdLayer.SignalLineL0 = signalL0;
            _macdLayer.HistogramL0 = histL0;
            var (macdL4, signalL4, histL4) = IndicatorManager.CalculateMACD(_ohlcLayer.DataL4);
            _macdLayer.MacdLineL4 = macdL4;
            _macdLayer.SignalLineL4 = signalL4;
            _macdLayer.HistogramL4 = histL4;
            var (macdL1, signalL1, histL1) = IndicatorManager.CalculateMACD(_ohlcLayer.DataL1);
            _macdLayer.MacdLineL1 = macdL1;
            _macdLayer.SignalLineL1 = signalL1;
            _macdLayer.HistogramL1 = histL1;
            var (macdL2, signalL2, histL2) = IndicatorManager.CalculateMACD(_ohlcLayer.DataL2);
            _macdLayer.MacdLineL2 = macdL2;
            _macdLayer.SignalLineL2 = signalL2;
            _macdLayer.HistogramL2 = histL2;
            var (macdL3, signalL3, histL3) = IndicatorManager.CalculateMACD(_ohlcLayer.DataL3);
            _macdLayer.MacdLineL3 = macdL3;
            _macdLayer.SignalLineL3 = signalL3;
            _macdLayer.HistogramL3 = histL3;
            // Add plots - L3 visible by default
            _macdLayer.MacdPlotL3 = _macdPlot.Add.Signal(_macdLayer.MacdLineL3);
            _macdLayer.MacdPlotL3.Color = new ScottPlot.Color(0, 0, 255);
            _macdLayer.MacdPlotL3.LineWidth = 2;
            _macdLayer.MacdPlotL3.Data.Period = 10000;
            _macdLayer.MacdPlotL3.Data.XOffset = 5000;
            _macdLayer.MacdPlotL3.IsVisible = true;
            _macdLayer.SignalPlotL3 = _macdPlot.Add.Signal(_macdLayer.SignalLineL3);
            _macdLayer.SignalPlotL3.Color = new ScottPlot.Color(255, 0, 0);
            _macdLayer.SignalPlotL3.LineWidth = 2;
            _macdLayer.SignalPlotL3.Data.Period = 10000;
            _macdLayer.SignalPlotL3.Data.XOffset = 5000;
            _macdLayer.SignalPlotL3.IsVisible = true;
            _macdLayer.HistogramPlotL3 = _macdPlot.Add.Signal(_macdLayer.HistogramL3);
            _macdLayer.HistogramPlotL3.Color = new ScottPlot.Color(128, 128, 128);
            _macdLayer.HistogramPlotL3.LineWidth = 1;
            _macdLayer.HistogramPlotL3.Data.Period = 10000;
            _macdLayer.HistogramPlotL3.Data.XOffset = 5000;
            _macdLayer.HistogramPlotL3.IsVisible = true;
            // L2
            _macdLayer.MacdPlotL2 = _macdPlot.Add.Signal(_macdLayer.MacdLineL2);
            _macdLayer.MacdPlotL2.Color = new ScottPlot.Color(0, 0, 255);
            _macdLayer.MacdPlotL2.LineWidth = 2;
            _macdLayer.MacdPlotL2.Data.Period = 1000;
            _macdLayer.MacdPlotL2.Data.XOffset = 500;
            _macdLayer.MacdPlotL2.IsVisible = false;
            _macdLayer.SignalPlotL2 = _macdPlot.Add.Signal(_macdLayer.SignalLineL2);
            _macdLayer.SignalPlotL2.Color = new ScottPlot.Color(255, 0, 0);
            _macdLayer.SignalPlotL2.LineWidth = 2;
            _macdLayer.SignalPlotL2.Data.Period = 1000;
            _macdLayer.SignalPlotL2.Data.XOffset = 500;
            _macdLayer.SignalPlotL2.IsVisible = false;
            _macdLayer.HistogramPlotL2 = _macdPlot.Add.Signal(_macdLayer.HistogramL2);
            _macdLayer.HistogramPlotL2.Color = new ScottPlot.Color(128, 128, 128);
            _macdLayer.HistogramPlotL2.LineWidth = 1;
            _macdLayer.HistogramPlotL2.Data.Period = 1000;
            _macdLayer.HistogramPlotL2.Data.XOffset = 500;
            _macdLayer.HistogramPlotL2.IsVisible = false;
            // L1
            _macdLayer.MacdPlotL1 = _macdPlot.Add.Signal(_macdLayer.MacdLineL1);
            _macdLayer.MacdPlotL1.Color = new ScottPlot.Color(0, 0, 255);
            _macdLayer.MacdPlotL1.LineWidth = 2;
            _macdLayer.MacdPlotL1.Data.Period = 100;
            _macdLayer.MacdPlotL1.Data.XOffset = 50;
            _macdLayer.MacdPlotL1.IsVisible = false;
            _macdLayer.SignalPlotL1 = _macdPlot.Add.Signal(_macdLayer.SignalLineL1);
            _macdLayer.SignalPlotL1.Color = new ScottPlot.Color(255, 0, 0);
            _macdLayer.SignalPlotL1.LineWidth = 2;
            _macdLayer.SignalPlotL1.Data.Period = 100;
            _macdLayer.SignalPlotL1.Data.XOffset = 50;
            _macdLayer.SignalPlotL1.IsVisible = false;
            _macdLayer.HistogramPlotL1 = _macdPlot.Add.Signal(_macdLayer.HistogramL1);
            _macdLayer.HistogramPlotL1.Color = new ScottPlot.Color(128, 128, 128);
            _macdLayer.HistogramPlotL1.LineWidth = 1;
            _macdLayer.HistogramPlotL1.Data.Period = 100;
            _macdLayer.HistogramPlotL1.Data.XOffset = 50;
            _macdLayer.HistogramPlotL1.IsVisible = false;
            // L4
            _macdLayer.MacdPlotL4 = _macdPlot.Add.Signal(_macdLayer.MacdLineL4);
            _macdLayer.MacdPlotL4.Color = new ScottPlot.Color(0, 0, 255);
            _macdLayer.MacdPlotL4.LineWidth = 2;
            _macdLayer.MacdPlotL4.Data.Period = 10;
            _macdLayer.MacdPlotL4.Data.XOffset = 5;
            _macdLayer.MacdPlotL4.IsVisible = false;
            _macdLayer.SignalPlotL4 = _macdPlot.Add.Signal(_macdLayer.SignalLineL4);
            _macdLayer.SignalPlotL4.Color = new ScottPlot.Color(255, 0, 0);
            _macdLayer.SignalPlotL4.LineWidth = 2;
            _macdLayer.SignalPlotL4.Data.Period = 10;
            _macdLayer.SignalPlotL4.Data.XOffset = 5;
            _macdLayer.SignalPlotL4.IsVisible = false;
            _macdLayer.HistogramPlotL4 = _macdPlot.Add.Signal(_macdLayer.HistogramL4);
            _macdLayer.HistogramPlotL4.Color = new ScottPlot.Color(128, 128, 128);
            _macdLayer.HistogramPlotL4.LineWidth = 1;
            _macdLayer.HistogramPlotL4.Data.Period = 10;
            _macdLayer.HistogramPlotL4.Data.XOffset = 5;
            _macdLayer.HistogramPlotL4.IsVisible = false;
            // L0
            _macdLayer.MacdPlotL0 = _macdPlot.Add.Signal(_macdLayer.MacdLineL0);
            _macdLayer.MacdPlotL0.Color = new ScottPlot.Color(0, 0, 255);
            _macdLayer.MacdPlotL0.LineWidth = 2;
            _macdLayer.MacdPlotL0.Data.Period = 1;
            _macdLayer.MacdPlotL0.Data.XOffset = 0;
            _macdLayer.MacdPlotL0.IsVisible = false;
            _macdLayer.SignalPlotL0 = _macdPlot.Add.Signal(_macdLayer.SignalLineL0);
            _macdLayer.SignalPlotL0.Color = new ScottPlot.Color(255, 0, 0);
            _macdLayer.SignalPlotL0.LineWidth = 2;
            _macdLayer.SignalPlotL0.Data.Period = 1;
            _macdLayer.SignalPlotL0.Data.XOffset = 0;
            _macdLayer.SignalPlotL0.IsVisible = false;
            _macdLayer.HistogramPlotL0 = _macdPlot.Add.Signal(_macdLayer.HistogramL0);
            _macdLayer.HistogramPlotL0.Color = new ScottPlot.Color(128, 128, 128);
            _macdLayer.HistogramPlotL0.LineWidth = 1;
            _macdLayer.HistogramPlotL0.Data.Period = 1;
            _macdLayer.HistogramPlotL0.Data.XOffset = 0;
            _macdLayer.HistogramPlotL0.IsVisible = false;
            _currentMACDLayer = 3;
            // LOD Logic
            _macdPlot.RenderManager.RenderStarting += (s, e) =>
            {
                try
                {
                    if (_fullData == null) return;
                    var axis = _macdPlot.Axes.Bottom;
                    double range = axis.Max - axis.Min;
                    int targetLayer = _currentMACDLayer;
                    if (range > 1_000_000) targetLayer = 3;
                    else if (range > 500_000) targetLayer = 2;
                    else if (range > 20_000) targetLayer = 1;
                    else if (range > 2_000) targetLayer = 4;
                    else targetLayer = 0;
                    if (targetLayer != _currentMACDLayer)
                    {
                        _macdLayer.MacdPlotL0.IsVisible = _macdLayer.SignalPlotL0.IsVisible = _macdLayer.HistogramPlotL0.IsVisible = (targetLayer == 0);
                        _macdLayer.MacdPlotL1.IsVisible = _macdLayer.SignalPlotL1.IsVisible = _macdLayer.HistogramPlotL1.IsVisible = (targetLayer == 1);
                        _macdLayer.MacdPlotL2.IsVisible = _macdLayer.SignalPlotL2.IsVisible = _macdLayer.HistogramPlotL2.IsVisible = (targetLayer == 2);
                        _macdLayer.MacdPlotL3.IsVisible = _macdLayer.SignalPlotL3.IsVisible = _macdLayer.HistogramPlotL3.IsVisible = (targetLayer == 3);
                        _macdLayer.MacdPlotL4.IsVisible = _macdLayer.SignalPlotL4.IsVisible = _macdLayer.HistogramPlotL4.IsVisible = (targetLayer == 4);
                        _currentMACDLayer = targetLayer;
                    }

                    // Auto-scale Y axis based on visible X range
                    int visibleStartIdx = Math.Max(0, (int)Math.Floor(axis.Min));
                    int visibleEndIdx = Math.Min(_fullData.Length, (int)Math.Ceiling(axis.Max));

                    if (visibleStartIdx < visibleEndIdx)
                    {
                        double yMin = double.MaxValue;
                        double yMax = double.MinValue;

                        // Get current visible MACD data based on layer
                        double[] currentMacdData = targetLayer switch
                        {
                            0 => _macdLayer.MacdLineL0,
                            1 => _macdLayer.MacdLineL1,
                            2 => _macdLayer.MacdLineL2,
                            3 => _macdLayer.MacdLineL3,
                            4 => _macdLayer.MacdLineL4,
                            _ => _macdLayer.MacdLineL3
                        };

                        double[] currentSignalData = targetLayer switch
                        {
                            0 => _macdLayer.SignalLineL0,
                            1 => _macdLayer.SignalLineL1,
                            2 => _macdLayer.SignalLineL2,
                            3 => _macdLayer.SignalLineL3,
                            4 => _macdLayer.SignalLineL4,
                            _ => _macdLayer.SignalLineL3
                        };

                        double[] currentHistData = targetLayer switch
                        {
                            0 => _macdLayer.HistogramL0,
                            1 => _macdLayer.HistogramL1,
                            2 => _macdLayer.HistogramL2,
                            3 => _macdLayer.HistogramL3,
                            4 => _macdLayer.HistogramL4,
                            _ => _macdLayer.HistogramL3
                        };

                        // Calculate index range for current layer
                        int layerStartIdx = visibleStartIdx;
                        int layerEndIdx = visibleEndIdx;

                        if (targetLayer == 1) { layerStartIdx /= 100; layerEndIdx /= 100; }
                        else if (targetLayer == 2) { layerStartIdx /= 1000; layerEndIdx /= 1000; }
                        else if (targetLayer == 3) { layerStartIdx /= 10000; layerEndIdx /= 10000; }
                        else if (targetLayer == 4) { layerStartIdx /= 10; layerEndIdx /= 10; }

                        layerStartIdx = Math.Max(0, layerStartIdx);
                        layerEndIdx = Math.Min(currentMacdData.Length, layerEndIdx);

                        for (int i = layerStartIdx; i < layerEndIdx; i++)
                        {
                            if (!double.IsNaN(currentMacdData[i]))
                            {
                                yMin = Math.Min(yMin, currentMacdData[i]);
                                yMax = Math.Max(yMax, currentMacdData[i]);
                            }
                            if (!double.IsNaN(currentSignalData[i]))
                            {
                                yMin = Math.Min(yMin, currentSignalData[i]);
                                yMax = Math.Max(yMax, currentSignalData[i]);
                            }
                            if (!double.IsNaN(currentHistData[i]))
                            {
                                yMin = Math.Min(yMin, currentHistData[i]);
                                yMax = Math.Max(yMax, currentHistData[i]);
                            }
                        }

                        if (yMin < yMax)
                        {
                            double yPadding = (yMax - yMin) * 0.05;
                            _macdPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                        }
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CRASH] MACD RenderStarting: {ex.Message}"); }
            };
            // DateTime formatter
            ScottPlot.TickGenerators.NumericAutomatic macdTickGen = new ScottPlot.TickGenerators.NumericAutomatic();
            macdTickGen.LabelFormatter = (x) =>
            {
                int index = (int)x;
                if (index < 0 || index >= _fullData.Length) return "";
                DateTime dt = _fullData[index].DateTime;
                double visibleRange = _macdPlot.Axes.Bottom.Max - _macdPlot.Axes.Bottom.Min;
                return visibleRange > 1440 ? dt.ToString("yyyy.MM.dd") : dt.ToString("yyyy.MM.dd HH:mm:ss");
            };
            _macdPlot.Axes.Bottom.TickGenerator = macdTickGen;
            // Set Y limits
            double macdYMin = double.MaxValue, macdYMax = double.MinValue;
            foreach (var value in _macdLayer.MacdLineL3)
                if (!double.IsNaN(value)) { macdYMin = Math.Min(macdYMin, value); macdYMax = Math.Max(macdYMax, value); }
            if (macdYMin < macdYMax)
            {
                double yPadding = (macdYMax - macdYMin) * 0.05;
                _macdPlot.Axes.SetLimitsY(macdYMin - yPadding, macdYMax + yPadding);
            }
            // Set X limits to match main plot
            _macdPlot.Axes.SetLimitsX(_mainPlot.Axes.Bottom.Min, _mainPlot.Axes.Bottom.Max);

            // Add Crosshair to MACD plot
            _macdCrosshair = _macdPlot.Add.Crosshair(0, 0);
            _macdCrosshair.IsVisible = false;

            System.Diagnostics.Debug.WriteLine($"[SetupMACDPlot] ========== MACD PLOT SETUP COMPLETE ==========");
        }

        private void SetupRSIPlot()
        {
            System.Diagnostics.Debug.WriteLine($"[SetupRSIPlot] ========== STARTING RSI PLOT SETUP ==========");
            // Initialize RSI layer
            _rsiLayer = new RSILayer();
            // Calculate RSI for each LOD level
            _rsiLayer.DataL0 = IndicatorManager.CalculateRSI(_fullData, 14);
            _rsiLayer.DataL4 = IndicatorManager.CalculateRSI(_ohlcLayer.DataL4, 14);
            _rsiLayer.DataL1 = IndicatorManager.CalculateRSI(_ohlcLayer.DataL1, 14);
            _rsiLayer.DataL2 = IndicatorManager.CalculateRSI(_ohlcLayer.DataL2, 14);
            _rsiLayer.DataL3 = IndicatorManager.CalculateRSI(_ohlcLayer.DataL3, 14);
            // Add plots - L3 visible by default
            _rsiLayer.PlotL3 = _rsiPlot.Add.Signal(_rsiLayer.DataL3);
            _rsiLayer.PlotL3.Color = new ScottPlot.Color(128, 0, 128);
            _rsiLayer.PlotL3.LineWidth = 2;
            _rsiLayer.PlotL3.Data.Period = 10000;
            _rsiLayer.PlotL3.Data.XOffset = 5000;
            _rsiLayer.PlotL3.IsVisible = true;
            // L2
            _rsiLayer.PlotL2 = _rsiPlot.Add.Signal(_rsiLayer.DataL2);
            _rsiLayer.PlotL2.Color = new ScottPlot.Color(128, 0, 128);
            _rsiLayer.PlotL2.LineWidth = 2;
            _rsiLayer.PlotL2.Data.Period = 1000;
            _rsiLayer.PlotL2.Data.XOffset = 500;
            _rsiLayer.PlotL2.IsVisible = false;
            // L1
            _rsiLayer.PlotL1 = _rsiPlot.Add.Signal(_rsiLayer.DataL1);
            _rsiLayer.PlotL1.Color = new ScottPlot.Color(128, 0, 128);
            _rsiLayer.PlotL1.LineWidth = 2;
            _rsiLayer.PlotL1.Data.Period = 100;
            _rsiLayer.PlotL1.Data.XOffset = 50;
            _rsiLayer.PlotL1.IsVisible = false;
            // L4
            _rsiLayer.PlotL4 = _rsiPlot.Add.Signal(_rsiLayer.DataL4);
            _rsiLayer.PlotL4.Color = new ScottPlot.Color(128, 0, 128);
            _rsiLayer.PlotL4.LineWidth = 2;
            _rsiLayer.PlotL4.Data.Period = 10;
            _rsiLayer.PlotL4.Data.XOffset = 5;
            _rsiLayer.PlotL4.IsVisible = false;
            // L0
            _rsiLayer.PlotL0 = _rsiPlot.Add.Signal(_rsiLayer.DataL0);
            _rsiLayer.PlotL0.Color = new ScottPlot.Color(128, 0, 128);
            _rsiLayer.PlotL0.LineWidth = 2;
            _rsiLayer.PlotL0.Data.Period = 1;
            _rsiLayer.PlotL0.Data.XOffset = 0;
            _rsiLayer.PlotL0.IsVisible = false;
            _currentRSILayer = 3;
            // LOD Logic
            _rsiPlot.RenderManager.RenderStarting += (s, e) =>
            {
                try
                {
                    if (_fullData == null) return;
                    var axis = _rsiPlot.Axes.Bottom;
                    double range = axis.Max - axis.Min;
                    int targetLayer = _currentRSILayer;
                    if (range > 1_000_000) targetLayer = 3;
                    else if (range > 500_000) targetLayer = 2;
                    else if (range > 20_000) targetLayer = 1;
                    else if (range > 2_000) targetLayer = 4;
                    else targetLayer = 0;
                    if (targetLayer != _currentRSILayer)
                    {
                        _rsiLayer.PlotL0.IsVisible = (targetLayer == 0);
                        _rsiLayer.PlotL1.IsVisible = (targetLayer == 1);
                        _rsiLayer.PlotL2.IsVisible = (targetLayer == 2);
                        _rsiLayer.PlotL3.IsVisible = (targetLayer == 3);
                        _rsiLayer.PlotL4.IsVisible = (targetLayer == 4);
                        _currentRSILayer = targetLayer;
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CRASH] RSI RenderStarting: {ex.Message}"); }
            };
            // DateTime formatter
            ScottPlot.TickGenerators.NumericAutomatic rsiTickGen = new ScottPlot.TickGenerators.NumericAutomatic();
            rsiTickGen.LabelFormatter = (x) =>
            {
                int index = (int)x;
                if (index < 0 || index >= _fullData.Length) return "";
                DateTime dt = _fullData[index].DateTime;
                double visibleRange = _rsiPlot.Axes.Bottom.Max - _rsiPlot.Axes.Bottom.Min;
                return visibleRange > 1440 ? dt.ToString("yyyy.MM.dd") : dt.ToString("yyyy.MM.dd HH:mm:ss");
            };
            _rsiPlot.Axes.Bottom.TickGenerator = rsiTickGen;
            // Set Y limits (RSI ranges from 0 to 100)
            _rsiPlot.Axes.SetLimitsY(0, 100);
            // Add reference lines at 30 and 70
            var line30 = _rsiPlot.Add.HorizontalLine(30);
            line30.Color = new ScottPlot.Color(200, 200, 200);
            line30.LineWidth = 2;
            line30.LinePattern = ScottPlot.LinePattern.Solid;
            var line70 = _rsiPlot.Add.HorizontalLine(70);
            line70.Color = new ScottPlot.Color(200, 200, 200);
            line70.LineWidth = 2;
            line70.LinePattern = ScottPlot.LinePattern.Solid;
            // Set X limits to match main plot
            _rsiPlot.Axes.SetLimitsX(_mainPlot.Axes.Bottom.Min, _mainPlot.Axes.Bottom.Max);

            // Add Crosshair to RSI plot
            _rsiCrosshair = _rsiPlot.Add.Crosshair(0, 0);
            _rsiCrosshair.IsVisible = false;

            System.Diagnostics.Debug.WriteLine($"[SetupRSIPlot] ========== RSI PLOT SETUP COMPLETE ==========");
        }

        private void OnHScroll()
        {
            if (_isUpdatingScrollBars || _fullData == null) return;

            try
            {
                _isUpdatingScrollBars = true;

                int maxScroll = _hScrollBar.Maximum - _hScrollBar.LargeChange + 1;
                double scrollRatio = (double)_hScrollBar.Value / maxScroll;

                var xAxis = _mainPlot.Axes.Bottom;
                double currentSpan = xAxis.Max - xAxis.Min;

                // Total data range
                double totalMin = 0;
                double totalMax = _fullData.Length;
                double totalSpan = totalMax - totalMin;

                // Calculate new center based on scroll ratio
                double newMin = totalMin + (scrollRatio * (totalSpan - currentSpan));
                double newMax = newMin + currentSpan;

                _mainPlot.Axes.SetLimitsX(newMin, newMax);

                // Auto-scale Y axis if enabled
                if (_autoScaleY)
                {
                    int visibleStartIdx = Math.Max(0, (int)Math.Floor(newMin));
                    int visibleEndIdx = Math.Min(_fullData.Length, (int)Math.Ceiling(newMax));

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
                            _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);
                        }
                    }
                }

                _formsPlot.Refresh();
            }
            finally
            {
                _isUpdatingScrollBars = false;
            }
        }

        private void OnVScroll()
        {
            if (_isUpdatingScrollBars || _fullData == null) return;

            try
            {
                _isUpdatingScrollBars = true;

                // Disable auto-scale when user manually scrolls Y-axis
                _autoScaleY = false;

                // Invert Y scrollbar because 0 is at top in WinForms but Y-axis Max is at top
                int maxScroll = _vScrollBar.Maximum - _vScrollBar.LargeChange + 1;
                int invertedValue = maxScroll - _vScrollBar.Value;
                double scrollRatio = (double)invertedValue / maxScroll;

                var yAxis = _mainPlot.Axes.Left;
                double currentSpan = yAxis.Max - yAxis.Min;

                // For Y-axis, we need to determine a "Total Range".
                double globalYMin = _globalYMin;
                double globalYMax = _globalYMax;

                double totalSpan = globalYMax - globalYMin;

                // Apply scroll
                double newMin = globalYMin + (scrollRatio * (totalSpan - currentSpan));
                double newMax = newMin + currentSpan;

                _mainPlot.Axes.SetLimitsY(newMin, newMax);
                _formsPlot.Refresh();
            }
            finally
            {
                _isUpdatingScrollBars = false;
            }
        }

        private void UpdateScrollBarsFromPlot()
        {
            if (_isUpdatingScrollBars || _fullData == null) return;

            try
            {
                _isUpdatingScrollBars = true;

                // Update HScrollBar (X-Axis)
                var xAxis = _mainPlot.Axes.Bottom;
                double xMin = xAxis.Min;
                double xMax = xAxis.Max;
                double xSpan = xMax - xMin;
                double totalXSpan = _fullData.Length;

                if (totalXSpan > 0)
                {
                    // Calculate proportion of visible area
                    double viewRatio = xSpan / totalXSpan;
                    int newLargeChange = (int)(viewRatio * _hScrollBar.Maximum);
                    newLargeChange = Math.Max(1, Math.Min(_hScrollBar.Maximum, newLargeChange));

                    _hScrollBar.LargeChange = newLargeChange;

                    // Calculate position
                    double posRatio = xMin / totalXSpan;
                    int newValue = (int)(posRatio * _hScrollBar.Maximum);
                    newValue = Math.Max(_hScrollBar.Minimum, Math.Min(_hScrollBar.Maximum - newLargeChange + 1, newValue));

                    _hScrollBar.Value = newValue;
                }

                // Update VScrollBar (Y-Axis)
                var yAxis = _mainPlot.Axes.Left;
                double yMin = yAxis.Min;
                double yMax = yAxis.Max;
                double ySpan = yMax - yMin;

                double globalYMin = _globalYMin;
                double globalYMax = _globalYMax;

                double totalYSpan = globalYMax - globalYMin;

                if (totalYSpan > 0)
                {
                    double viewRatio = ySpan / totalYSpan;
                    int newLargeChange = (int)(viewRatio * _vScrollBar.Maximum);
                    newLargeChange = Math.Max(1, Math.Min(_vScrollBar.Maximum, newLargeChange));

                    _vScrollBar.LargeChange = newLargeChange;

                    // Position (Inverted for WinForms VScrollBar)
                    double distFromMin = yMin - globalYMin;
                    double posRatio = distFromMin / totalYSpan;

                    // Invert
                    int maxScroll = _vScrollBar.Maximum - newLargeChange + 1;
                    int newValue = maxScroll - (int)(posRatio * maxScroll);

                    newValue = Math.Max(_vScrollBar.Minimum, Math.Min(maxScroll, newValue));

                    _vScrollBar.Value = newValue;

                    // Enable/disable based on whether we can scroll
                    _vScrollBar.Enabled = (newLargeChange < _vScrollBar.Maximum);
                }
            }
            catch
            {
                // Ignore errors during update
            }
            finally
            {
                _isUpdatingScrollBars = false;
            }
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

            // Calculate Global Y Limits
            _globalYMin = double.MaxValue;
            _globalYMax = double.MinValue;
            foreach (var o in ohlcs)
            {
                if (o.Low < _globalYMin) _globalYMin = o.Low;
                if (o.High > _globalYMax) _globalYMax = o.High;
            }
            // Add some padding to global limits
            double padding = (_globalYMax - _globalYMin) * 0.05;
            _globalYMin -= padding;
            _globalYMax += padding;

            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"[GenerateOHLCs] Completed in {sw.ElapsedMilliseconds}ms");
            return ohlcs;
        }

        /// <summary>
        /// Generate X coordinates for aggregated data to match OHLC X positions
        /// </summary>
        private double[] GenerateXCoordinates(int count, int aggregationFactor)
        {
            double[] xCoords = new double[count];
            for (int i = 0; i < count; i++)
            {
                // Calculate center position of aggregated data
                xCoords[i] = i * aggregationFactor + (aggregationFactor / 2.0);
            }
            return xCoords;
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
                var plotArea = _mainPlot.RenderManager.LastRender.DataRect;
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
                if (_rbLastFit.Checked)
                {
                    // Show last N bars that fit on screen
                    int visibleCount = CalculateFitToScreenCount();
                    int startIdx = Math.Max(0, totalCount - visibleCount);
                    int endIdx = totalCount;

                    // Add 2% padding to X-axis
                    double xRange = endIdx - startIdx;
                    double xPadding = xRange * 0.02;
                    _mainPlot.Axes.SetLimitsX(startIdx - xPadding, endIdx + xPadding);

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
                    _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

                    _formsPlot.Refresh();
                }
                else if (_rbReset.Checked || _rbFullData.Checked)
                {
                    // Show all data, let LOD decide which layer
                    // Add 2% padding to X-axis
                    double xPadding = totalCount * 0.02;
                    _mainPlot.Axes.SetLimitsX(-xPadding, totalCount + xPadding);

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
                    _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

                    _formsPlot.Refresh();
                }
                else if (_rbFitToScreen.Checked)
                {
                    // Get current X-axis limits
                    var xAxis = _mainPlot.Axes.Bottom;
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
                        _mainPlot.Axes.SetLimitsX(startIdx - xPadding, endIdx + xPadding);

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
                        _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

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
                        _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

                        System.Diagnostics.Debug.WriteLine($"[FitToScreen] Custom view - keeping X-axis [{visibleStartIdx} to {visibleEndIdx}], fitting Y: {yMin:F2} to {yMax:F2}");
                    }

                    _formsPlot.Refresh();
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
                        _mainPlot.Axes.SetLimitsX(startIdx - xPadding, totalCount + xPadding);

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
                        _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

                        _formsPlot.Refresh();
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
                        _mainPlot.Axes.SetLimitsX(-xPadding, endIdx + xPadding);

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
                        _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

                        _formsPlot.Refresh();
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
                            _mainPlot.Axes.SetLimitsX(start - xPadding, end + xPadding);

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
                            _mainPlot.Axes.SetLimitsY(yMin - yPadding, yMax + yPadding);

                            _formsPlot.Refresh();
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
                else if (_rbAlignRight.Checked)
                {
                    // Align Right: Shift visible data to align the last bar with the right edge
                    var xAxis = _mainPlot.Axes.Bottom;
                    double currentMin = xAxis.Min;
                    double currentMax = xAxis.Max;
                    double currentRange = currentMax - currentMin;

                    int lastDataIndex = _fullData.Length - 1;

                    // Add 2% padding to the right edge (same as Last N mode)
                    double xPadding = currentRange * 0.02;

                    // Always shift the view to align last bar with right edge (with padding)
                    double newMax = lastDataIndex + xPadding;
                    double newMin = newMax - currentRange;

                    // Make sure we don't go below 0
                    if (newMin < 0)
                    {
                        newMin = 0;
                        newMax = currentRange;
                    }

                    _mainPlot.Axes.SetLimitsX(newMin, newMax);
                    _smaPlot.Axes.SetLimitsX(newMin, newMax);

                    _autoScaleY = true;
                    _formsPlot.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying view mode: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Adds a single signal (line) to the main OHLC plot with LOD support
        /// </summary>
        public void AddSignalToMainPlot(SignalConfig config)
        {
            if (config.DataL0 == null || config.DataL0.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[AddSignalToMainPlot] WARNING: Signal '{config.Name}' has no data");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AddSignalToMainPlot] Adding signal: {config.Name}");

            var signal = new MainPlotSignal
            {
                Name = config.Name
            };

            var scottPlotColor = new ScottPlot.Color(config.Color.R, config.Color.G, config.Color.B, config.Color.A);

            // Add L3 (most zoomed out) - visible by default
            signal.PlotL3 = _mainPlot.Add.Signal(config.DataL3);
            signal.PlotL3.Color = scottPlotColor;
            signal.PlotL3.LineWidth = config.LineWidth;
            signal.PlotL3.Data.Period = 10000;
            signal.PlotL3.Data.XOffset = 5000;
            signal.PlotL3.IsVisible = (_currentLayer == 3);

            // Add L2
            signal.PlotL2 = _mainPlot.Add.Signal(config.DataL2);
            signal.PlotL2.Color = scottPlotColor;
            signal.PlotL2.LineWidth = config.LineWidth;
            signal.PlotL2.Data.Period = 1000;
            signal.PlotL2.Data.XOffset = 500;
            signal.PlotL2.IsVisible = (_currentLayer == 2);

            // Add L1
            signal.PlotL1 = _mainPlot.Add.Signal(config.DataL1);
            signal.PlotL1.Color = scottPlotColor;
            signal.PlotL1.LineWidth = config.LineWidth;
            signal.PlotL1.Data.Period = 100;
            signal.PlotL1.Data.XOffset = 50;
            signal.PlotL1.IsVisible = (_currentLayer == 1);

            // Add L4
            signal.PlotL4 = _mainPlot.Add.Signal(config.DataL4);
            signal.PlotL4.Color = scottPlotColor;
            signal.PlotL4.LineWidth = config.LineWidth;
            signal.PlotL4.Data.Period = 10;
            signal.PlotL4.Data.XOffset = 5;
            signal.PlotL4.IsVisible = (_currentLayer == 4);

            // Add L0 (full data)
            signal.PlotL0 = _mainPlot.Add.Signal(config.DataL0);
            signal.PlotL0.Color = scottPlotColor;
            signal.PlotL0.LineWidth = config.LineWidth;
            signal.PlotL0.Data.Period = 1;
            signal.PlotL0.Data.XOffset = 0;
            signal.PlotL0.IsVisible = (_currentLayer == 0);

            _mainPlotSignals.Add(signal);
            System.Diagnostics.Debug.WriteLine($"[AddSignalToMainPlot] Signal '{config.Name}' added successfully. Total signals: {_mainPlotSignals.Count}");
        }

        /// <summary>
        /// Adds multiple signals (lines) to the main OHLC plot with LOD support
        /// </summary>
        public void AddSignalsToMainPlot(params SignalConfig[] configs)
        {
            if (configs == null || configs.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[AddSignalsToMainPlot] WARNING: No signals to add");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AddSignalsToMainPlot] Adding {configs.Length} signals...");

            foreach (var config in configs)
            {
                AddSignalToMainPlot(config);
            }

            // Refresh plot after adding all signals
            _formsPlot.Refresh();
            System.Diagnostics.Debug.WriteLine($"[AddSignalsToMainPlot] All signals added. Total: {_mainPlotSignals.Count}");
        }

        /// <summary>
        /// Generates buy/sell/flat signals based on SMA crossover strategy with profit/loss targets
        /// </summary>
        /// <param name="fastPeriod">Fast SMA period (e.g., 50)</param>
        /// <param name="slowPeriod">Slow SMA period (e.g., 100)</param>
        /// <param name="indicatorType">Indicator type (currently only "SMA" supported)</param>
        /// <param name="profitTargetPercent">Profit target in % (e.g., 5 for +5%)</param>
        /// <param name="stopLossPercent">Stop loss in % (e.g., -3 for -3%)</param>
        /// <returns>List of trading signals</returns>
        public List<TradingSignal> GenerateBuySellFlatSignals(int fastPeriod, int slowPeriod, string indicatorType, double profitTargetPercent, double stopLossPercent)
        {
            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] ========== STARTING SIGNAL GENERATION ==========");
            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] Parameters: Fast={fastPeriod}, Slow={slowPeriod}, Profit={profitTargetPercent}%, Stop={stopLossPercent}%");

            var signals = new List<TradingSignal>();

            if (_fullData == null || _fullData.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] ERROR: No data available");
                return signals;
            }

            // Calculate SMAs
            double[] fastSMA = IndicatorManager.CalculateSMA(_fullData, fastPeriod);
            double[] slowSMA = IndicatorManager.CalculateSMA(_fullData, slowPeriod);

            // Trading state
            bool inPosition = false;
            double entryPrice = 0;
            int entryIndex = 0;

            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] Starting signal detection...");

            for (int i = slowPeriod; i < _fullData.Length; i++)
            {
                // Skip if SMA values are invalid
                if (double.IsNaN(fastSMA[i]) || double.IsNaN(slowSMA[i]) ||
                    double.IsNaN(fastSMA[i - 1]) || double.IsNaN(slowSMA[i - 1]))
                    continue;

                double currentPrice = _fullData[i].Close;

                if (!inPosition)
                {
                    // BUY SIGNAL: Fast SMA crosses above Slow SMA (Golden Cross)
                    if (fastSMA[i - 1] <= slowSMA[i - 1] && fastSMA[i] > slowSMA[i])
                    {
                        signals.Add(new TradingSignal
                        {
                            Index = i,
                            Type = SignalType.Buy,
                            Price = currentPrice
                        });

                        inPosition = true;
                        entryPrice = currentPrice;
                        entryIndex = i;

                        System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] BUY at index {i}, price {currentPrice:F2}");
                    }
                }
                else
                {
                    // Calculate P&L
                    double pnlPercent = ((currentPrice - entryPrice) / entryPrice) * 100;

                    // SELL SIGNAL: Hit profit target or stop loss
                    bool hitProfitTarget = pnlPercent >= profitTargetPercent;
                    bool hitStopLoss = pnlPercent <= stopLossPercent;

                    // Also check for Death Cross (Fast SMA crosses below Slow SMA)
                    bool deathCross = fastSMA[i - 1] >= slowSMA[i - 1] && fastSMA[i] < slowSMA[i];

                    if (hitProfitTarget || hitStopLoss || deathCross)
                    {
                        string reason = hitProfitTarget ? "Profit Target" : (hitStopLoss ? "Stop Loss" : "Death Cross");

                        signals.Add(new TradingSignal
                        {
                            Index = i,
                            Type = SignalType.Sell,
                            Price = currentPrice
                        });

                        System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] SELL at index {i}, price {currentPrice:F2}, P&L={pnlPercent:F2}%, Reason={reason}");

                        inPosition = false;
                        entryPrice = 0;
                    }
                }
            }

            // Close any open position at the end
            if (inPosition)
            {
                int lastIndex = _fullData.Length - 1;
                signals.Add(new TradingSignal
                {
                    Index = lastIndex,
                    Type = SignalType.Flat,
                    Price = _fullData[lastIndex].Close
                });
                System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] FLAT (end of data) at index {lastIndex}");
            }

            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] ========== SIGNAL GENERATION COMPLETE ==========");
            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] Total signals: {signals.Count}");
            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] Buy signals: {signals.Count(s => s.Type == SignalType.Buy)}");
            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] Sell signals: {signals.Count(s => s.Type == SignalType.Sell)}");
            System.Diagnostics.Debug.WriteLine($"[GenerateBuySellFlatSignals] Flat signals: {signals.Count(s => s.Type == SignalType.Flat)}");

            return signals;
        }

        /// <summary>
        /// Adds buy/sell/flat signals as markers to the main OHLC plot with LOD support
        /// </summary>
        public void AddTradingSignalsToPlot(List<TradingSignal> signals)
        {
            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] ========== ADDING TRADING SIGNALS WITH LOD ==========");
            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] Total signals: {signals.Count}");

            // Separate signals by type
            var buySignals = signals.Where(s => s.Type == SignalType.Buy).ToList();
            var sellSignals = signals.Where(s => s.Type == SignalType.Sell).ToList();
            var flatSignals = signals.Where(s => s.Type == SignalType.Flat).ToList();

            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] Buy: {buySignals.Count}, Sell: {sellSignals.Count}, Flat: {flatSignals.Count}");

            _tradingMarkers = new TradingMarkerLayer();

            // Helper function to create scatter for a specific LOD layer
            ScottPlot.Plottables.Scatter CreateMarkerScatter(List<TradingSignal> sigs, int layer, ScottPlot.Color color, ScottPlot.MarkerShape shape, float size, double yOffset)
            {
                if (sigs.Count == 0)
                {
                    // Return empty scatter
                    var empty = _mainPlot.Add.Scatter(Array.Empty<double>(), Array.Empty<double>());
                    empty.IsVisible = false;
                    return empty;
                }

                int divisor = layer switch
                {
                    0 => 1,
                    1 => 100,
                    2 => 1000,
                    3 => 10000,
                    4 => 10,
                    _ => 1
                };

                int offset = layer switch
                {
                    0 => 0,
                    1 => 50,
                    2 => 500,
                    3 => 5000,
                    4 => 5,
                    _ => 0
                };

                int period = divisor; // Period matches divisor for coordinate system

                // Calculate X coordinates matching OHLC coordinate system
                // Formula: (Index / divisor) * period + offset
                double[] x = sigs.Select(s => (double)((s.Index / divisor) * period + offset)).ToArray();
                double[] y = sigs.Select(s => s.Price * yOffset).ToArray();

                var scatter = _mainPlot.Add.Scatter(x, y);
                scatter.Color = color;
                scatter.MarkerStyle.Shape = shape;
                scatter.MarkerStyle.Size = size;
                scatter.LineWidth = 0;
                scatter.IsVisible = (_currentLayer == layer);

                return scatter;
            }

            // Create Buy markers for all layers
            var buyColor = new ScottPlot.Color(0, 255, 0);
            _tradingMarkers.BuyMarkersL0 = CreateMarkerScatter(buySignals, 0, buyColor, ScottPlot.MarkerShape.FilledTriangleUp, 15, 0.98);
            _tradingMarkers.BuyMarkersL1 = CreateMarkerScatter(buySignals, 1, buyColor, ScottPlot.MarkerShape.FilledTriangleUp, 15, 0.98);
            _tradingMarkers.BuyMarkersL2 = CreateMarkerScatter(buySignals, 2, buyColor, ScottPlot.MarkerShape.FilledTriangleUp, 15, 0.98);
            _tradingMarkers.BuyMarkersL3 = CreateMarkerScatter(buySignals, 3, buyColor, ScottPlot.MarkerShape.FilledTriangleUp, 15, 0.98);
            _tradingMarkers.BuyMarkersL4 = CreateMarkerScatter(buySignals, 4, buyColor, ScottPlot.MarkerShape.FilledTriangleUp, 15, 0.98);
            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] Created BUY markers for all LOD layers");

            // Create Sell markers for all layers
            var sellColor = new ScottPlot.Color(255, 0, 0);
            _tradingMarkers.SellMarkersL0 = CreateMarkerScatter(sellSignals, 0, sellColor, ScottPlot.MarkerShape.FilledTriangleDown, 15, 1.02);
            _tradingMarkers.SellMarkersL1 = CreateMarkerScatter(sellSignals, 1, sellColor, ScottPlot.MarkerShape.FilledTriangleDown, 15, 1.02);
            _tradingMarkers.SellMarkersL2 = CreateMarkerScatter(sellSignals, 2, sellColor, ScottPlot.MarkerShape.FilledTriangleDown, 15, 1.02);
            _tradingMarkers.SellMarkersL3 = CreateMarkerScatter(sellSignals, 3, sellColor, ScottPlot.MarkerShape.FilledTriangleDown, 15, 1.02);
            _tradingMarkers.SellMarkersL4 = CreateMarkerScatter(sellSignals, 4, sellColor, ScottPlot.MarkerShape.FilledTriangleDown, 15, 1.02);
            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] Created SELL markers for all LOD layers");

            // Create Flat markers for all layers
            var flatColor = new ScottPlot.Color(128, 128, 128);
            _tradingMarkers.FlatMarkersL0 = CreateMarkerScatter(flatSignals, 0, flatColor, ScottPlot.MarkerShape.FilledCircle, 10, 1.0);
            _tradingMarkers.FlatMarkersL1 = CreateMarkerScatter(flatSignals, 1, flatColor, ScottPlot.MarkerShape.FilledCircle, 10, 1.0);
            _tradingMarkers.FlatMarkersL2 = CreateMarkerScatter(flatSignals, 2, flatColor, ScottPlot.MarkerShape.FilledCircle, 10, 1.0);
            _tradingMarkers.FlatMarkersL3 = CreateMarkerScatter(flatSignals, 3, flatColor, ScottPlot.MarkerShape.FilledCircle, 10, 1.0);
            _tradingMarkers.FlatMarkersL4 = CreateMarkerScatter(flatSignals, 4, flatColor, ScottPlot.MarkerShape.FilledCircle, 10, 1.0);
            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] Created FLAT markers for all LOD layers");

            _formsPlot.Refresh();
            System.Diagnostics.Debug.WriteLine($"[AddTradingSignalsToPlot] ========== ALL TRADING SIGNALS ADDED WITH LOD ==========");
        }

        ~GuiManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                _formsPlot?.Dispose();
            }
        }
    }
}
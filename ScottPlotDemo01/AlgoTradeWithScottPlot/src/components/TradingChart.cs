using ScottPlot.WinForms;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;

namespace AlgoTradeWithScottPlot.Components
{
    /// <summary>
    /// Plot tipi - Chart'ın hangi veri tipini göstereceğini belirler
    /// </summary>
    [Flags]
    public enum PlotType
    {
        None = 0,
        Candlestick = 1 << 0,   // Candlestick mum grafikleri (renkli kutular)
        OHLC = 1 << 1,          // OHLC bar grafikleri (dikey çizgiler)
        Line = 1 << 2,          // Çizgi grafikleri (göstergeler için)
        Scatter = 1 << 3,       // Nokta grafikleri (sinyal noktaları için)
        Bar = 1 << 4,           // Çubuk grafik
        Histogram = 1 << 5,     // Histogram (frekans dağılımları için)
        Volume = 1 << 6,        // Hacim çubukları
        Signal = 1 << 7,        // Hızlı sinyal çizgileri (büyük veri setleri için)
        Area = 1 << 8,          // Alan grafikleri
        Heatmap = 1 << 9,       // Isı haritası
        Marker = 1 << 10,       // İşaretçiler (buy/sell okları için)
        Text = 1 << 11,         // Metin etiketleri
        Arrow = 1 << 12,        // Oklar (trend göstergeleri için)
        HorizontalLine = 1 << 13, // Yatay çizgiler (destek/direnç için)
        VerticalLine = 1 << 14,   // Dikey çizgiler (önemli zamanlar için)
        Box = 1 << 15,          // Kutu grafikleri (box plot)
        Polygon = 1 << 16,      // Çokgenler (bölge işaretleme için)
        ErrorBar = 1 << 17,     // Hata çubukları (istatistiksel veriler için)
        FillY = 1 << 18,        // Y eksenine göre doldurma (band plotları için)
        FillX = 1 << 19         // X eksenine göre doldurma
    }

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TradingChart))]
    [Description("Advanced trading chart with 4-panel layout and ScottPlot integration")]
    [Category("AlgoTrade Components")]
    public partial class TradingChart : UserControl
    {
        private Panel panelTop;
        private Panel panelBottom;
        private Panel panelLeft;
        private Panel panelRight;

        // Top Panel Controls
        private ComboBox modeComboBox;
        private Button applyButton;
        private Button topResetButton;
        private Button btnMinimize;
        private Button btnMaximize;
        private Button btnClose;

        // Left Panel Controls
        private Button prevButton;
        private Button nextButton;

        // Right Panel Controls
        private Button rightZoomInButton;
        private Button rightZoomOutButton;

        // Bottom Panel Controls
        private TrackBar zoomTrackBar;
        private Label statusLabel;

        // Zoom and Pan control buttons
        private Button yZoomInButton;
        private Button yZoomOutButton;
        private Button xZoomInButton;
        private Button xZoomOutButton;
        private Button yPanUpButton;
        private Button yPanDownButton;
        private Button xPanLeftButton;
        private Button xPanRightButton;
        private Button resetButton;
        private Button copyToAllButton;
        private Button resetXButton;
        private Button resetYButton;

        // Original axis limits for reset functionality
        private ScottPlot.AxisLimits? originalLimits;
        
        // Designer controls
        private Panel panelCenter;
        private StatusStrip statusStrip1;

        public Panel TopPanel => panelTop;
        public Panel BottomPanel => panelBottom;
        public Panel LeftPanel => panelLeft;
        public Panel RightPanel => panelRight;
        public FormsPlot Plot => formsPlot;

        // Crosshair plottable
        public ScottPlot.Plottables.Crosshair? Crosshair;

        // Data storage for the chart
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public double[]? XData { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public double[]? YData { get; set; }

        // Strategy ID - Bu chart hangi stratejiye ait?
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int StrategyId { get; set; } = -1; // -1 = ortak/global chart

        // Plot Type - Bu chart hangi tipleri destekliyor? (flags ile birden fazla olabilir)
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public PlotType EnabledPlotTypes { get; set; } = PlotType.None;

        // Events for minimize, maximize, close
        public event EventHandler? MinimizeRequested;
        public event EventHandler? MaximizeRequested;
        public event EventHandler? CloseRequested;

        // Event for copy to all functionality
        public event EventHandler? CopyToAllRequested;

        public TradingChart()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Enable double buffering to reduce flicker

            // Only setup plot enhancements at runtime, not in designer
            if (!DesignMode)
            {
                SetupPlotEnhancements();
            }
        }

        private void InitializeComponent()
        {
            panelTop = new Panel();
            resetXButton = new Button();
            modeComboBox = new ComboBox();
            applyButton = new Button();
            topResetButton = new Button();
            resetButton = new Button();
            xPanLeftButton = new Button();
            xZoomInButton = new Button();
            xZoomOutButton = new Button();
            xPanRightButton = new Button();
            btnMinimize = new Button();
            btnMaximize = new Button();
            btnClose = new Button();
            panelBottom = new Panel();
            zoomTrackBar = new TrackBar();
            statusLabel = new Label();
            panelLeft = new Panel();
            resetYButton = new Button();
            prevButton = new Button();
            nextButton = new Button();
            yPanUpButton = new Button();
            yZoomInButton = new Button();
            yZoomOutButton = new Button();
            yPanDownButton = new Button();
            copyToAllButton = new Button();
            panelRight = new Panel();
            rightZoomInButton = new Button();
            rightZoomOutButton = new Button();
            panelCenter = new Panel();
            formsPlot = new FormsPlot();
            hScrollBar1 = new HScrollBar();
            vScrollBar = new VScrollBar();
            statusStrip1 = new StatusStrip();
            panelTop.SuspendLayout();
            panelBottom.SuspendLayout();
            ((ISupportInitialize)zoomTrackBar).BeginInit();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            panelCenter.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.LightGray;
            panelTop.Controls.Add(resetXButton);
            panelTop.Controls.Add(modeComboBox);
            panelTop.Controls.Add(applyButton);
            panelTop.Controls.Add(topResetButton);
            panelTop.Controls.Add(resetButton);
            panelTop.Controls.Add(xPanLeftButton);
            panelTop.Controls.Add(xZoomInButton);
            panelTop.Controls.Add(xZoomOutButton);
            panelTop.Controls.Add(xPanRightButton);
            panelTop.Controls.Add(btnMinimize);
            panelTop.Controls.Add(btnMaximize);
            panelTop.Controls.Add(btnClose);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(800, 40);
            panelTop.TabIndex = 3;
            // 
            // resetXButton
            // 
            resetXButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            resetXButton.BackColor = Color.LightSalmon;
            resetXButton.FlatStyle = FlatStyle.Flat;
            resetXButton.Font = new Font("Arial", 7F, FontStyle.Bold);
            resetXButton.Location = new Point(608, 6);
            resetXButton.Name = "resetXButton";
            resetXButton.Size = new Size(24, 25);
            resetXButton.TabIndex = 29;
            resetXButton.Text = "RX";
            resetXButton.UseVisualStyleBackColor = false;
            // 
            // modeComboBox
            // 
            modeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modeComboBox.Location = new Point(54, 9);
            modeComboBox.Name = "modeComboBox";
            modeComboBox.Size = new Size(100, 23);
            modeComboBox.TabIndex = 5;
            // 
            // applyButton
            // 
            applyButton.Location = new Point(160, 9);
            applyButton.Name = "applyButton";
            applyButton.Size = new Size(60, 28);
            applyButton.TabIndex = 6;
            applyButton.Text = "Apply";
            applyButton.UseVisualStyleBackColor = true;
            // 
            // topResetButton
            // 
            topResetButton.Location = new Point(220, 9);
            topResetButton.Name = "topResetButton";
            topResetButton.Size = new Size(60, 28);
            topResetButton.TabIndex = 7;
            topResetButton.Text = "Reset";
            topResetButton.UseVisualStyleBackColor = true;
            // 
            // resetButton
            // 
            resetButton.BackColor = Color.Orange;
            resetButton.FlatStyle = FlatStyle.Flat;
            resetButton.Font = new Font("Arial", 10F, FontStyle.Bold);
            resetButton.Location = new Point(18, 7);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(30, 25);
            resetButton.TabIndex = 24;
            resetButton.Text = "⟲";
            resetButton.UseVisualStyleBackColor = false;
            // 
            // xPanLeftButton
            // 
            xPanLeftButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            xPanLeftButton.BackColor = Color.LightGreen;
            xPanLeftButton.FlatStyle = FlatStyle.Flat;
            xPanLeftButton.Font = new Font("Arial", 10F, FontStyle.Bold);
            xPanLeftButton.Location = new Point(552, 6);
            xPanLeftButton.Name = "xPanLeftButton";
            xPanLeftButton.Size = new Size(25, 25);
            xPanLeftButton.TabIndex = 19;
            xPanLeftButton.Text = "←";
            xPanLeftButton.UseVisualStyleBackColor = false;
            // 
            // xZoomInButton
            // 
            xZoomInButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            xZoomInButton.BackColor = Color.LightGreen;
            xZoomInButton.FlatStyle = FlatStyle.Flat;
            xZoomInButton.Font = new Font("Arial", 7F, FontStyle.Bold);
            xZoomInButton.Location = new Point(580, 6);
            xZoomInButton.Name = "xZoomInButton";
            xZoomInButton.Size = new Size(25, 25);
            xZoomInButton.TabIndex = 20;
            xZoomInButton.Text = "X+";
            xZoomInButton.UseVisualStyleBackColor = false;
            // 
            // xZoomOutButton
            // 
            xZoomOutButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            xZoomOutButton.BackColor = Color.LightGreen;
            xZoomOutButton.FlatStyle = FlatStyle.Flat;
            xZoomOutButton.Font = new Font("Arial", 7F, FontStyle.Bold);
            xZoomOutButton.Location = new Point(638, 6);
            xZoomOutButton.Name = "xZoomOutButton";
            xZoomOutButton.Size = new Size(25, 25);
            xZoomOutButton.TabIndex = 21;
            xZoomOutButton.Text = "X-";
            xZoomOutButton.UseVisualStyleBackColor = false;
            // 
            // xPanRightButton
            // 
            xPanRightButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            xPanRightButton.BackColor = Color.LightGreen;
            xPanRightButton.FlatStyle = FlatStyle.Flat;
            xPanRightButton.Font = new Font("Arial", 10F, FontStyle.Bold);
            xPanRightButton.Location = new Point(669, 6);
            xPanRightButton.Name = "xPanRightButton";
            xPanRightButton.Size = new Size(25, 25);
            xPanRightButton.TabIndex = 22;
            xPanRightButton.Text = "→";
            xPanRightButton.UseVisualStyleBackColor = false;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Arial", 10F);
            btnMinimize.Location = new Point(714, 6);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(25, 25);
            btnMinimize.TabIndex = 26;
            btnMinimize.Text = "-";
            btnMinimize.UseVisualStyleBackColor = true;
            // 
            // btnMaximize
            // 
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.FlatStyle = FlatStyle.Flat;
            btnMaximize.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnMaximize.Location = new Point(742, 6);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.Size = new Size(25, 25);
            btnMaximize.TabIndex = 27;
            btnMaximize.Text = "□";
            btnMaximize.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.IndianRed;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(770, 6);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(25, 25);
            btnClose.TabIndex = 28;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // panelBottom
            // 
            panelBottom.BackColor = Color.LightGray;
            panelBottom.Controls.Add(zoomTrackBar);
            panelBottom.Controls.Add(statusLabel);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 560);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(800, 40);
            panelBottom.TabIndex = 4;
            // 
            // zoomTrackBar
            // 
            zoomTrackBar.Location = new Point(10, 8);
            zoomTrackBar.Maximum = 100;
            zoomTrackBar.Minimum = 1;
            zoomTrackBar.Name = "zoomTrackBar";
            zoomTrackBar.Size = new Size(200, 45);
            zoomTrackBar.TabIndex = 12;
            zoomTrackBar.TickStyle = TickStyle.None;
            zoomTrackBar.Value = 50;
            // 
            // statusLabel
            // 
            statusLabel.ForeColor = Color.DarkGreen;
            statusLabel.Location = new Point(220, 12);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(150, 16);
            statusLabel.TabIndex = 13;
            statusLabel.Text = "Extended Plot Ready";
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.LightBlue;
            panelLeft.Controls.Add(resetYButton);
            panelLeft.Controls.Add(prevButton);
            panelLeft.Controls.Add(nextButton);
            panelLeft.Controls.Add(yPanUpButton);
            panelLeft.Controls.Add(yZoomInButton);
            panelLeft.Controls.Add(yZoomOutButton);
            panelLeft.Controls.Add(yPanDownButton);
            panelLeft.Controls.Add(copyToAllButton);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 40);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(60, 520);
            panelLeft.TabIndex = 1;
            // 
            // resetYButton
            // 
            resetYButton.BackColor = Color.LightSalmon;
            resetYButton.FlatStyle = FlatStyle.Flat;
            resetYButton.Font = new Font("Arial", 7F, FontStyle.Bold);
            resetYButton.Location = new Point(18, 65);
            resetYButton.Name = "resetYButton";
            resetYButton.Size = new Size(25, 25);
            resetYButton.TabIndex = 24;
            resetYButton.Text = "RY";
            resetYButton.UseVisualStyleBackColor = false;
            // 
            // prevButton
            // 
            prevButton.Location = new Point(3, 195);
            prevButton.Name = "prevButton";
            prevButton.Size = new Size(50, 25);
            prevButton.TabIndex = 8;
            prevButton.Text = "Prev";
            prevButton.UseVisualStyleBackColor = true;
            // 
            // nextButton
            // 
            nextButton.Location = new Point(3, 226);
            nextButton.Name = "nextButton";
            nextButton.Size = new Size(50, 25);
            nextButton.TabIndex = 9;
            nextButton.Text = "Next";
            nextButton.UseVisualStyleBackColor = true;
            // 
            // yPanUpButton
            // 
            yPanUpButton.BackColor = Color.LightSkyBlue;
            yPanUpButton.FlatStyle = FlatStyle.Flat;
            yPanUpButton.Font = new Font("Arial", 10F, FontStyle.Bold);
            yPanUpButton.Location = new Point(18, 6);
            yPanUpButton.Name = "yPanUpButton";
            yPanUpButton.Size = new Size(25, 25);
            yPanUpButton.TabIndex = 14;
            yPanUpButton.Text = "↑";
            yPanUpButton.UseVisualStyleBackColor = false;
            // 
            // yZoomInButton
            // 
            yZoomInButton.BackColor = Color.LightBlue;
            yZoomInButton.FlatStyle = FlatStyle.Flat;
            yZoomInButton.Font = new Font("Arial", 7F, FontStyle.Bold);
            yZoomInButton.Location = new Point(18, 34);
            yZoomInButton.Name = "yZoomInButton";
            yZoomInButton.Size = new Size(25, 25);
            yZoomInButton.TabIndex = 15;
            yZoomInButton.Text = "Y+";
            yZoomInButton.UseVisualStyleBackColor = false;
            // 
            // yZoomOutButton
            // 
            yZoomOutButton.BackColor = Color.LightBlue;
            yZoomOutButton.FlatStyle = FlatStyle.Flat;
            yZoomOutButton.Font = new Font("Arial", 7F, FontStyle.Bold);
            yZoomOutButton.Location = new Point(18, 96);
            yZoomOutButton.Name = "yZoomOutButton";
            yZoomOutButton.Size = new Size(25, 25);
            yZoomOutButton.TabIndex = 16;
            yZoomOutButton.Text = "Y-";
            yZoomOutButton.UseVisualStyleBackColor = false;
            // 
            // yPanDownButton
            // 
            yPanDownButton.BackColor = Color.LightSkyBlue;
            yPanDownButton.FlatStyle = FlatStyle.Flat;
            yPanDownButton.Font = new Font("Arial", 10F, FontStyle.Bold);
            yPanDownButton.Location = new Point(18, 124);
            yPanDownButton.Name = "yPanDownButton";
            yPanDownButton.Size = new Size(25, 25);
            yPanDownButton.TabIndex = 17;
            yPanDownButton.Text = "↓";
            yPanDownButton.UseVisualStyleBackColor = false;
            // 
            // copyToAllButton
            // 
            copyToAllButton.BackColor = Color.LightGreen;
            copyToAllButton.FlatStyle = FlatStyle.Flat;
            copyToAllButton.Font = new Font("Arial", 10F, FontStyle.Bold);
            copyToAllButton.Location = new Point(18, 152);
            copyToAllButton.Name = "copyToAllButton";
            copyToAllButton.Size = new Size(25, 25);
            copyToAllButton.TabIndex = 18;
            copyToAllButton.Text = "⇄";
            copyToAllButton.UseVisualStyleBackColor = false;
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.LightBlue;
            panelRight.Controls.Add(rightZoomInButton);
            panelRight.Controls.Add(rightZoomOutButton);
            panelRight.Dock = DockStyle.Right;
            panelRight.Location = new Point(714, 40);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(86, 520);
            panelRight.TabIndex = 2;
            panelRight.Paint += panelRight_Paint;
            // 
            // rightZoomInButton
            // 
            rightZoomInButton.Location = new Point(17, 17);
            rightZoomInButton.Name = "rightZoomInButton";
            rightZoomInButton.Size = new Size(50, 25);
            rightZoomInButton.TabIndex = 10;
            rightZoomInButton.Text = "Zoom+";
            rightZoomInButton.UseVisualStyleBackColor = true;
            // 
            // rightZoomOutButton
            // 
            rightZoomOutButton.Location = new Point(17, 52);
            rightZoomOutButton.Name = "rightZoomOutButton";
            rightZoomOutButton.Size = new Size(50, 25);
            rightZoomOutButton.TabIndex = 11;
            rightZoomOutButton.Text = "Zoom-";
            rightZoomOutButton.UseVisualStyleBackColor = true;
            // 
            // panelCenter
            // 
            panelCenter.BackColor = Color.Transparent;
            panelCenter.Controls.Add(formsPlot);
            panelCenter.Controls.Add(hScrollBar1);
            panelCenter.Controls.Add(vScrollBar);
            panelCenter.Controls.Add(statusStrip1);
            panelCenter.Dock = DockStyle.Fill;
            panelCenter.Location = new Point(60, 40);
            panelCenter.Name = "panelCenter";
            panelCenter.Size = new Size(654, 520);
            panelCenter.TabIndex = 5;
            // 
            // formsPlot
            // 
            formsPlot.DisplayScale = 1F;
            formsPlot.Dock = DockStyle.Fill;
            formsPlot.Location = new Point(0, 0);
            formsPlot.Name = "formsPlot";
            formsPlot.Padding = new Padding(25);
            formsPlot.Size = new Size(644, 481);
            formsPlot.TabIndex = 5;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Dock = DockStyle.Bottom;
            hScrollBar1.Location = new Point(0, 481);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(644, 17);
            hScrollBar1.TabIndex = 4;
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = DockStyle.Right;
            vScrollBar.Location = new Point(644, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new Size(10, 498);
            vScrollBar.TabIndex = 3;
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 498);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(654, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // TradingChart
            // 
            BackColor = Color.White;
            Controls.Add(panelCenter);
            Controls.Add(panelLeft);
            Controls.Add(panelRight);
            Controls.Add(panelTop);
            Controls.Add(panelBottom);
            Name = "TradingChart";
            Size = new Size(800, 600);
            panelTop.ResumeLayout(false);
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ((ISupportInitialize)zoomTrackBar).EndInit();
            panelLeft.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelCenter.ResumeLayout(false);
            panelCenter.PerformLayout();
            ResumeLayout(false);
        }

        private void InitializeAllControls()
        {
            // Initialize Top Panel Controls
            modeComboBox = new ComboBox()
            {
                Location = new Point(5, 8),
                Size = new Size(100, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            applyButton = new Button()
            {
                Location = new Point(115, 6),
                Size = new Size(60, 28),
                Text = "Apply",
                UseVisualStyleBackColor = true
            };

            topResetButton = new Button()
            {
                Location = new Point(185, 6),
                Size = new Size(60, 28),
                Text = "Reset",
                UseVisualStyleBackColor = true
            };

            btnMinimize = new Button()
            {
                Size = new Size(30, 28),
                Text = "_",
                Font = new Font("Arial", 12, FontStyle.Bold),
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(700, 6) // Will be repositioned dynamically
            };

            btnMaximize = new Button()
            {
                Size = new Size(30, 28),
                Text = "□",
                Font = new Font("Arial", 12, FontStyle.Bold),
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(735, 6) // Will be repositioned dynamically
            };

            btnClose = new Button()
            {
                Size = new Size(30, 28),
                Text = "X",
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(770, 6) // Will be repositioned dynamically
            };

            // Initialize Left Panel Controls
            prevButton = new Button()
            {
                Location = new Point(5, 20),
                Size = new Size(50, 25),
                Text = "Prev",
                UseVisualStyleBackColor = true
            };

            nextButton = new Button()
            {
                Location = new Point(5, 55),
                Size = new Size(50, 25),
                Text = "Next",
                UseVisualStyleBackColor = true
            };

            // Initialize Right Panel Controls
            rightZoomInButton = new Button()
            {
                Location = new Point(5, 20),
                Size = new Size(50, 25),
                Text = "Zoom+",
                UseVisualStyleBackColor = true
            };

            rightZoomOutButton = new Button()
            {
                Location = new Point(5, 55),
                Size = new Size(50, 25),
                Text = "Zoom-",
                UseVisualStyleBackColor = true
            };

            // Initialize Bottom Panel Controls
            zoomTrackBar = new TrackBar()
            {
                Location = new Point(10, 8),
                Size = new Size(200, 24),
                Minimum = 1,
                Maximum = 100,
                Value = 50,
                TickStyle = TickStyle.None
            };

            statusLabel = new Label()
            {
                Location = new Point(220, 12),
                Size = new Size(150, 16),
                Text = "Extended Plot Ready",
                ForeColor = Color.DarkGreen
            };

            // Initialize Zoom and Pan buttons
            const int buttonSize = 25;
            const int margin = 3;
            int yStartY = 85; // Below nextButton

            yPanUpButton = new Button
            {
                Text = "↑",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(5, yStartY),
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left
            };

            yZoomInButton = new Button
            {
                Text = "Y+",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(5, yStartY + buttonSize + margin),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left
            };

            yZoomOutButton = new Button
            {
                Text = "Y-",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(5, yStartY + 2 * (buttonSize + margin)),
                BackColor = Color.LightCoral,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left
            };

            yPanDownButton = new Button
            {
                Text = "↓",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(5, yStartY + 3 * (buttonSize + margin)),
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left
            };

            copyToAllButton = new Button
            {
                Text = "⇄",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(5, yStartY + 4 * (buttonSize + margin)),
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Left
            };

            // X-axis controls for top panel
            xPanLeftButton = new Button
            {
                Text = "←",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(300, 6), // Will be repositioned dynamically
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            xZoomInButton = new Button
            {
                Text = "X+",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(328, 6), // Will be repositioned dynamically
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            xZoomOutButton = new Button
            {
                Text = "X-",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(356, 6), // Will be repositioned dynamically
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightYellow,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            xPanRightButton = new Button
            {
                Text = "→",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(384, 6), // Will be repositioned dynamically
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            // Reset buttons for top panel
            int resetStartX = 260;

            resetYButton = new Button
            {
                Text = "RY",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(resetStartX, margin),
                BackColor = Color.LightSalmon,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            resetButton = new Button
            {
                Text = "⟲",
                Size = new Size(buttonSize + 5, buttonSize),
                Location = new Point(resetStartX + buttonSize + margin, margin),
                BackColor = Color.Orange,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };

            resetXButton = new Button
            {
                Text = "RX",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(resetStartX + 2 * buttonSize + margin + 8, margin),
                BackColor = Color.LightSalmon,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
        }

        private void AddControlsToPanels()
        {
            // Add controls to Top Panel
            panelTop.Controls.AddRange(new Control[] {
                modeComboBox, applyButton, topResetButton,
                resetYButton, resetButton, resetXButton,
                xPanLeftButton, xZoomInButton, xZoomOutButton, xPanRightButton,
                btnMinimize, btnMaximize, btnClose
            });

            // Add controls to Left Panel
            panelLeft.Controls.AddRange(new Control[] {
                prevButton, nextButton,
                yPanUpButton, yZoomInButton, yZoomOutButton, yPanDownButton, copyToAllButton
            });

            // Add controls to Right Panel
            panelRight.Controls.AddRange(new Control[] {
                rightZoomInButton, rightZoomOutButton
            });

            // Add controls to Bottom Panel
            panelBottom.Controls.AddRange(new Control[] {
                zoomTrackBar, statusLabel
            });

            // Setup event handlers only at runtime
            if (!DesignMode)
            {
                SetupEventHandlers();
            }
        }

        private void SetupEventHandlers()
        {
            // Setup ComboBox items at runtime
            modeComboBox.Items.AddRange(new object[] { "Candlestick", "Line", "OHLC" });
            modeComboBox.SelectedIndex = 0;

            // Window control events
            btnMinimize.Click += (s, e) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
            btnMaximize.Click += (s, e) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
            btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

            // Zoom and pan events
            yZoomInButton.Click += (s, e) => ZoomY(0.8);
            yZoomOutButton.Click += (s, e) => ZoomY(1.25);
            xZoomInButton.Click += (s, e) => ZoomX(0.8);
            xZoomOutButton.Click += (s, e) => ZoomX(1.25);

            yPanUpButton.Click += (s, e) => PanY(0.2);
            yPanDownButton.Click += (s, e) => PanY(-0.2);
            xPanLeftButton.Click += (s, e) => PanX(-0.2);
            xPanRightButton.Click += (s, e) => PanX(0.2);

            resetButton.Click += (s, e) => ResetView();
            resetXButton.Click += (s, e) => ResetViewX();
            resetYButton.Click += (s, e) => ResetViewY();

            copyToAllButton.Click += (s, e) => CopyToAllRequested?.Invoke(this, EventArgs.Empty);

            // Dynamic positioning
            panelTop.SizeChanged += (s, e) =>
            {
                int rightOffset = panelTop.Width - 10;
                btnClose.Location = new Point(rightOffset - 30, 6);
                btnMaximize.Location = new Point(rightOffset - 65, 6);
                btnMinimize.Location = new Point(rightOffset - 100, 6);

                int xAxisStartX = rightOffset - 222;
                xPanLeftButton.Location = new Point(xAxisStartX + 0 * 28, 6);
                xZoomInButton.Location = new Point(xAxisStartX + 1 * 28, 6);
                xZoomOutButton.Location = new Point(xAxisStartX + 2 * 28, 6);
                xPanRightButton.Location = new Point(xAxisStartX + 3 * 28, 6);
            };
        }

        private void AddBuiltInControls()
        {
            // Top Panel Controls
            var modeComboBox = new ComboBox()
            {
                Location = new Point(5, 8),
                Size = new Size(100, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            modeComboBox.Items.AddRange(new[] { "Candlestick", "Line", "OHLC" });
            modeComboBox.SelectedIndex = 0;

            var applyButton = new Button()
            {
                Location = new Point(115, 6),
                Size = new Size(60, 28),
                Text = "Apply",
                UseVisualStyleBackColor = true
            };

            var resetButton = new Button()
            {
                Location = new Point(185, 6),
                Size = new Size(60, 28),
                Text = "Reset",
                UseVisualStyleBackColor = true
            };

            // Window control buttons (right side of top panel)
            var btnMinimize = new Button()
            {
                Size = new Size(30, 28),
                Text = "_",
                Font = new Font("Arial", 12, FontStyle.Bold),
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnMaximize = new Button()
            {
                Size = new Size(30, 28),
                Text = "□",
                Font = new Font("Arial", 12, FontStyle.Bold),
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            var btnClose = new Button()
            {
                Size = new Size(30, 28),
                Text = "X",
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Add event handlers only at runtime
            if (!DesignMode)
            {
                btnMinimize.Click += (s, e) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
                btnMaximize.Click += (s, e) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
                btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

                // Position buttons on the right side (calculated after parent resize)
                panelTop.SizeChanged += (s, e) =>
                {
                    int rightOffset = panelTop.Width - 10;
                    btnClose.Location = new Point(rightOffset - 30, 6);
                    btnMaximize.Location = new Point(rightOffset - 65, 6);
                    btnMinimize.Location = new Point(rightOffset - 100, 6);
                };
            }

            panelTop.Controls.AddRange(new Control[] { modeComboBox, applyButton, resetButton, btnMinimize, btnMaximize, btnClose });

            // Left Panel Controls
            var prevButton = new Button()
            {
                Location = new Point(5, 20),
                Size = new Size(50, 25),
                Text = "Prev",
                UseVisualStyleBackColor = true
            };

            var nextButton = new Button()
            {
                Location = new Point(5, 55),
                Size = new Size(50, 25),
                Text = "Next",
                UseVisualStyleBackColor = true
            };

            panelLeft.Controls.AddRange(new Control[] { prevButton, nextButton });

            // Right Panel Controls
            var zoomInButton = new Button()
            {
                Location = new Point(5, 20),
                Size = new Size(50, 25),
                Text = "Zoom+",
                UseVisualStyleBackColor = true
            };

            var zoomOutButton = new Button()
            {
                Location = new Point(5, 55),
                Size = new Size(50, 25),
                Text = "Zoom-",
                UseVisualStyleBackColor = true
            };

            panelRight.Controls.AddRange(new Control[] { zoomInButton, zoomOutButton });

            // Bottom Panel Controls
            var zoomTrackBar = new TrackBar()
            {
                Location = new Point(10, 8),
                Size = new Size(200, 24),
                Minimum = 1,
                Maximum = 100,
                Value = 50,
                TickStyle = TickStyle.None
            };

            var statusLabel = new Label()
            {
                Location = new Point(220, 12),
                Size = new Size(150, 16),
                Text = "Extended Plot Ready",
                ForeColor = Color.DarkGreen
            };

            panelBottom.Controls.AddRange(new Control[] { zoomTrackBar, statusLabel });

            // Create zoom and pan controls for design-time visibility
            // CreateZoomControls(); // Disabled - using static controls instead
        }

        private void SetupPlotEnhancements()
        {
            // Add overlay text
            var overlayText = formsPlot.Plot.Add.Text("Extended Plot Ready", 10, 10);
            overlayText.LabelFontColor = ScottPlot.Colors.Green;
            overlayText.LabelFontSize = 14;

            // Enable tooltip on mouse hover
            formsPlot.MouseMove += OnMouseMove;

            // Mouse wheel event'ini bağla
            this.MouseWheel += OnPlotMouseWheel;
            formsPlot.MouseMove += OnPlotMouseMove;
            formsPlot.MouseDown += OnPlotMouseDown;
            formsPlot.MouseUp += OnPlotMouseUp;
            formsPlot.MouseLeave += OnPlotMouseLeave; // MouseLeave event'ini ekle

            // Crosshair'i oluştur ve plot'a ekle
            Crosshair = formsPlot.Plot.Add.Crosshair(0, 0);
            Crosshair.IsVisible = false;
            Crosshair.LineColor = ScottPlot.Colors.Red;
            Crosshair.LineWidth = 1;
            Crosshair.LinePattern = ScottPlot.LinePattern.Solid;

            // Auto-scale functionality
            formsPlot.Plot.Axes.AutoScale();

            // Set initial plot styling
            formsPlot.Plot.FigureBackground.Color = ScottPlot.Colors.White;
            formsPlot.Plot.DataBackground.Color = ScottPlot.Colors.White;
            formsPlot.Plot.Grid.MajorLineColor = ScottPlot.Colors.LightGray;

            // Attach all control events
            AttachControlEvents();

            formsPlot.Refresh();
        }

        private void OnMouseMove(object? sender, MouseEventArgs e)
        {
            // Get coordinates where mouse is positioned
            var mousePixel = new ScottPlot.Pixel(e.X, e.Y);
            var mouseCoordinate = formsPlot.Plot.GetCoordinates(mousePixel);

            // Update tooltip (you can customize this further)
            // this.ToolTipText = $"X: {mouseCoordinate.X:F2}, Y: {mouseCoordinate.Y:F2}";

            // Update status label if it exists
            var statusLabel = panelBottom.Controls.OfType<Label>().FirstOrDefault();
            if (statusLabel != null)
            {
                statusLabel.Text = $"X: {mouseCoordinate.X:F2}, Y: {mouseCoordinate.Y:F2}";
            }
        }

        public void AutoScale()
        {
            formsPlot.Plot.Axes.AutoScale();
            formsPlot.Refresh();
        }

        public void ZoomIn()
        {
            formsPlot.Plot.Axes.Zoom(0.8, 0.8);
            formsPlot.Refresh();
        }

        public void ZoomOut()
        {
            formsPlot.Plot.Axes.Zoom(1.25, 1.25);
            formsPlot.Refresh();
        }

        /// <summary>
        /// Belirtilen plot tipini aktif eder (flag olarak ekler)
        /// Birden fazla tip için birden fazla kez çağrılabilir
        /// </summary>
        /// <param name="plotType">Aktif edilecek plot tipi</param>
        public void SetPlotType(PlotType plotType)
        {
            // Flag'i ekle
            EnabledPlotTypes |= plotType;
        }

        /// <summary>
        /// Belirtilen plot tipini devre dışı bırakır
        /// </summary>
        /// <param name="plotType">Devre dışı bırakılacak plot tipi</param>
        public void DisablePlotType(PlotType plotType)
        {
            // Flag'i kaldır
            EnabledPlotTypes &= ~plotType;
        }

        /// <summary>
        /// Belirtilen plot tipinin aktif olup olmadığını kontrol eder
        /// </summary>
        /// <param name="plotType">Kontrol edilecek plot tipi</param>
        /// <returns>Plot tipi aktif ise true</returns>
        public bool IsPlotTypeEnabled(PlotType plotType)
        {
            return (EnabledPlotTypes & plotType) == plotType;
        }

        /// <summary>
        /// Tüm plot tiplerini temizler
        /// </summary>
        public void ClearPlotTypes()
        {
            EnabledPlotTypes = PlotType.None;
        }

        /// <summary>
        /// Aktif plot tiplerinin listesini string olarak döndürür
        /// </summary>
        /// <returns>Aktif plot tipleri (virgülle ayrılmış)</returns>
        public string GetEnabledPlotTypesString()
        {
            if (EnabledPlotTypes == PlotType.None)
                return "None";

            var types = new List<string>();

            foreach (PlotType type in Enum.GetValues(typeof(PlotType)))
            {
                if (type != PlotType.None && (EnabledPlotTypes & type) == type)
                {
                    types.Add(type.ToString());
                }
            }

            return string.Join(", ", types);
        }


        public void ZoomX(double factor)
        {
            try
            {
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                double centerX = (currentLimits.Left + currentLimits.Right) / 2;
                double spanX = (currentLimits.Right - currentLimits.Left) * factor;

                formsPlot.Plot.Axes.SetLimitsX(centerX - spanX / 2, centerX + spanX / 2);
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during X zoom: {ex.Message}");
            }
        }

        public void ZoomY(double factor)
        {
            try
            {
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                double centerY = (currentLimits.Bottom + currentLimits.Top) / 2;
                double spanY = (currentLimits.Top - currentLimits.Bottom) * factor;

                formsPlot.Plot.Axes.SetLimitsY(centerY - spanY / 2, centerY + spanY / 2);
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during Y zoom: {ex.Message}");
            }
        }

        public void PanX(double factor)
        {
            try
            {
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                double spanX = currentLimits.Right - currentLimits.Left;
                double shiftX = spanX * factor;

                formsPlot.Plot.Axes.SetLimitsX(
                    currentLimits.Left + shiftX,
                    currentLimits.Right + shiftX
                );
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during X pan: {ex.Message}");
            }
        }

        public void PanY(double factor)
        {
            try
            {
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                double spanY = currentLimits.Top - currentLimits.Bottom;
                double shiftY = spanY * factor;

                formsPlot.Plot.Axes.SetLimitsY(
                    currentLimits.Bottom + shiftY,
                    currentLimits.Top + shiftY
                );
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during Y pan: {ex.Message}");
            }
        }

        public void ResetView()
        {
            try
            {
                formsPlot.Plot.Axes.AutoScale();
                formsPlot.Refresh();

                // Update original limits for future resets
                originalLimits = formsPlot.Plot.Axes.GetLimits();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during reset: {ex.Message}");
            }
        }

        public void ResetViewX()
        {
            try
            {
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                formsPlot.Plot.Axes.AutoScaleX();
                formsPlot.Plot.Axes.SetLimitsY(currentLimits.Bottom, currentLimits.Top);
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during X reset: {ex.Message}");
            }
        }

        public void ResetViewY()
        {
            try
            {
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                formsPlot.Plot.Axes.AutoScaleY();
                formsPlot.Plot.Axes.SetLimitsX(currentLimits.Left, currentLimits.Right);
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during Y reset: {ex.Message}");
            }
        }

        /// <summary>
        /// Orijinal axis limitleri saklar (veri eklendikten sonra çağrılmalı)
        /// </summary>
        public void SaveOriginalLimits()
        {
            originalLimits = formsPlot.Plot.Axes.GetLimits();
        }

        /// <summary>
        /// Mouse wheel event'ini yakalar ve zoom işlemlerini yapar
        /// </summary>
        private void OnPlotMouseWheel(object? sender, MouseEventArgs e)
        {
            try
            {
                // Mouse wheel ile zoom yapma
                double zoomFactor = e.Delta > 0 ? 0.9 : 1.1;
                
                // Hem X hem Y ekseninde zoom yap
                var currentLimits = formsPlot.Plot.Axes.GetLimits();
                double centerX = (currentLimits.Left + currentLimits.Right) / 2;
                double centerY = (currentLimits.Bottom + currentLimits.Top) / 2;
                
                double spanX = (currentLimits.Right - currentLimits.Left) * zoomFactor;
                double spanY = (currentLimits.Top - currentLimits.Bottom) * zoomFactor;
                
                formsPlot.Plot.Axes.SetLimits(
                    centerX - spanX / 2, centerX + spanX / 2,
                    centerY - spanY / 2, centerY + spanY / 2
                );
                
                formsPlot.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPlotMouseWheel: {ex.Message}");
            }
        }

        /// <summary>
        /// Mouse drag event'ini yakalar ve pan/zoom işlemlerini yapar
        /// </summary>
        private Point? lastMousePosition = null;
        private bool isLeftDragging = false;

        // Orta tuş için özel flag'ler
        private bool isMiddleDragging = false;
        private bool hasMiddleDragged = false; // Sürüklemenin "tıklama" olmadığını anlamak için
        private ScottPlot.Plottables.Rectangle? zoomRectangle; // Zoom dikdörtgenini çizmek için
        private ScottPlot.Coordinates? middleDragStartCoordinates; // Sürüklemenin başlangıç koordinatları

        private void OnPlotMouseMove(object? sender, MouseEventArgs e)
        {
            try
            {
                // Crosshair güncelle
                var coords = formsPlot.Plot.GetCoordinates(new ScottPlot.Pixel(e.X, e.Y));
                if (Crosshair != null)
                {
                    Crosshair.Position = coords;
                    Crosshair.IsVisible = true;
                }

                // Sol tuş ile sürükleme (Pan)
                if (isLeftDragging && lastMousePosition.HasValue)
                {
                    double deltaX = e.X - lastMousePosition.Value.X;
                    double deltaY = e.Y - lastMousePosition.Value.Y;
                    
                    // Pixel delta'yı koordinat delta'ya çevir
                    var currentLimits = formsPlot.Plot.Axes.GetLimits();
                    var plotArea = formsPlot.Plot.RenderManager.LastRender.DataRect;
                    
                    if (plotArea.Width > 0 && plotArea.Height > 0)
                    {
                        double coordDeltaX = -deltaX * (currentLimits.Right - currentLimits.Left) / plotArea.Width;
                        double coordDeltaY = deltaY * (currentLimits.Top - currentLimits.Bottom) / plotArea.Height;
                        
                        formsPlot.Plot.Axes.SetLimits(
                            currentLimits.Left + coordDeltaX, currentLimits.Right + coordDeltaX,
                            currentLimits.Bottom + coordDeltaY, currentLimits.Top + coordDeltaY
                        );
                        formsPlot.Refresh();
                    }
                }
                // Orta tuş ile sürükleme (Zoom Rectangle)
                else if (isMiddleDragging)
                {
                    // Sürüklemenin başladığını işaretle
                    if (!hasMiddleDragged && lastMousePosition.HasValue && (Math.Abs(e.X - lastMousePosition.Value.X) > 2 || Math.Abs(e.Y - lastMousePosition.Value.Y) > 2))
                    {
                        hasMiddleDragged = true;
                    }

                    // Zoom rectangle çiz/güncelle
                    if (hasMiddleDragged)
                    {
                        if (zoomRectangle is null && middleDragStartCoordinates.HasValue)
                        {
                            zoomRectangle = formsPlot.Plot.Add.Rectangle(0, 0, 0, 0);
                            zoomRectangle.FillStyle.Color = ScottPlot.Colors.LightBlue.WithAlpha(100);
                            zoomRectangle.LineStyle.Color = ScottPlot.Colors.Blue;
                            zoomRectangle.LineStyle.Width = 2;
                        }

                        if (zoomRectangle is not null && middleDragStartCoordinates.HasValue)
                        {
                            var currentCoordinates = formsPlot.Plot.GetCoordinates(new ScottPlot.Pixel(e.Location.X, e.Location.Y));
                            zoomRectangle.X1 = middleDragStartCoordinates.Value.X;
                            zoomRectangle.X2 = currentCoordinates.X;
                            zoomRectangle.Y1 = middleDragStartCoordinates.Value.Y;
                            zoomRectangle.Y2 = currentCoordinates.Y;
                            formsPlot.Refresh();
                        }
                    }
                }

                lastMousePosition = e.Location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPlotMouseMove: {ex.Message}");
            }
        }

        private void OnPlotMouseLeave(object? sender, EventArgs e)
        {
            // Fare plot alanından ayrıldığında crosshair'i gizle
            if (Crosshair != null)
            {
                Crosshair.IsVisible = false;
                formsPlot.Refresh();
            }
        }

        private void OnPlotMouseDown(object? sender, MouseEventArgs e)
        {
            lastMousePosition = e.Location;
            if (e.Button == MouseButtons.Left)
            {
                isLeftDragging = true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                isMiddleDragging = true;
                hasMiddleDragged = false;
                middleDragStartCoordinates = formsPlot.Plot.GetCoordinates(new ScottPlot.Pixel(e.Location.X, e.Location.Y));
            }
        }

        private void OnPlotMouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isLeftDragging)
            {
                isLeftDragging = false;
            }
            else if (e.Button == MouseButtons.Middle && isMiddleDragging)
            {
                if (hasMiddleDragged) // Zoom rectangle yapıldıysa
                {
                    if (zoomRectangle is not null && middleDragStartCoordinates.HasValue)
                    {
                        var endCoordinates = formsPlot.Plot.GetCoordinates(new ScottPlot.Pixel(e.Location.X, e.Location.Y));

                        // Geçerli dikdörtgen koordinatları
                        double x1 = Math.Min(middleDragStartCoordinates.Value.X, endCoordinates.X);
                        double x2 = Math.Max(middleDragStartCoordinates.Value.X, endCoordinates.X);
                        double y1 = Math.Min(middleDragStartCoordinates.Value.Y, endCoordinates.Y);
                        double y2 = Math.Max(middleDragStartCoordinates.Value.Y, endCoordinates.Y);

                        // Minimum area kontrolü
                        if ((x2 - x1) > 0.001 && (y2 - y1) > 0.001)
                        {
                            formsPlot.Plot.Axes.SetLimits(x1, x2, y1, y2);
                            formsPlot.Refresh();
                        }

                        // Zoom rectangle'ı temizle
                        formsPlot.Plot.Remove(zoomRectangle);
                        zoomRectangle = null;
                        formsPlot.Refresh();
                    }
                }
                else // Middle click = Reset view
                {
                    ResetView();
                }

                // Middle button flags sıfırla
                isMiddleDragging = false;
                hasMiddleDragged = false;
                middleDragStartCoordinates = null;
            }

            lastMousePosition = null;
        }



        /// <summary>
        /// Tüm kontrol düğmelerinin event handler'larını bağlar
        /// Plot oluşturulduktan sonra programatik olarak çağrılmalıdır
        /// </summary>
        public void AttachControlEvents()
        {
            // ComboBox items ekleme
            if (modeComboBox.Items.Count == 0)
            {
                modeComboBox.Items.AddRange(new object[] { "Candlestick", "Line", "OHLC" });
                modeComboBox.SelectedIndex = 0;
            }

            // Window control events
            btnMinimize.Click += (s, e) => MinimizeRequested?.Invoke(this, EventArgs.Empty);
            btnMaximize.Click += (s, e) => MaximizeRequested?.Invoke(this, EventArgs.Empty);
            btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);

            // Y-axis zoom and pan events
            yZoomInButton.Click += (s, e) => ZoomY(0.8);
            yZoomOutButton.Click += (s, e) => ZoomY(1.25);
            yPanUpButton.Click += (s, e) => PanY(0.2);
            yPanDownButton.Click += (s, e) => PanY(-0.2);

            // X-axis zoom and pan events
            xZoomInButton.Click += (s, e) => ZoomX(0.8);
            xZoomOutButton.Click += (s, e) => ZoomX(1.25);
            xPanLeftButton.Click += (s, e) => PanX(-0.2);
            xPanRightButton.Click += (s, e) => PanX(0.2);

            // Reset events
            resetButton.Click += (s, e) => ResetView();
            resetXButton.Click += (s, e) => ResetViewX();
            resetYButton.Click += (s, e) => ResetViewY();
            topResetButton.Click += (s, e) => ResetView();

            // Other events
            copyToAllButton.Click += (s, e) => CopyToAllRequested?.Invoke(this, EventArgs.Empty);
            applyButton.Click += (s, e) => ApplyModeSelection();

            // Right panel zoom events
            rightZoomInButton.Click += (s, e) => ZoomIn();
            rightZoomOutButton.Click += (s, e) => ZoomOut();

            // Dynamic positioning for window control buttons - DISABLED to use designer positions
            // panelTop.SizeChanged += (s, e) => PositionWindowControlButtons();
        }

        /// <summary>
        /// Mode seçimini uygular (ComboBox'tan)
        /// </summary>
        private void ApplyModeSelection()
        {
            string selectedMode = modeComboBox.SelectedItem?.ToString() ?? "Candlestick";
            
            // TODO: Implement mode switching logic based on selectedMode
            // Bu kısmı plot tipine göre implement edebilirsin
            
            // Örnek: PlotType flag'ini güncelle
            switch (selectedMode)
            {
                case "Candlestick":
                    SetPlotType(PlotType.Candlestick);
                    break;
                case "Line":
                    SetPlotType(PlotType.Line);
                    break;
                case "OHLC":
                    SetPlotType(PlotType.OHLC);
                    break;
            }
            
            formsPlot.Refresh();
        }

        /// <summary>
        /// Window kontrol düğmelerini dinamik olarak konumlandırır
        /// DEVRE DIŞI - Designer'da ayarlanan konumları kullan
        /// </summary>
        private void PositionWindowControlButtons()
        {
            // Bu metod şimdi devre dışı - tasarımcıda ayarlanan konumlar kullanılıyor
            return;
            
            /*
            int rightOffset = panelTop.Width - 10;
            
            // Window control buttons positioning
            btnClose.Location = new Point(rightOffset - 25, 6);
            btnMaximize.Location = new Point(rightOffset - 53, 6);
            btnMinimize.Location = new Point(rightOffset - 81, 6);
            
            // X-axis buttons positioning (before window controls)
            int xAxisStartX = rightOffset - 200;
            xPanLeftButton.Location = new Point(xAxisStartX + 0 * 28, 6);
            xZoomInButton.Location = new Point(xAxisStartX + 1 * 28, 6);
            xZoomOutButton.Location = new Point(xAxisStartX + 2 * 28, 6);
            xPanRightButton.Location = new Point(xAxisStartX + 3 * 28, 6);
            */
        }

        private void panelRight_Paint(object? sender, PaintEventArgs e)
        {

        }
    }
}
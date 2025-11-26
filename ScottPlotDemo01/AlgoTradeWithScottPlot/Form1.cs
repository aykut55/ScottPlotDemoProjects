using AlgoTradeWithScottPlot.Components;
using OoplesFinance.StockIndicators.Enums;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using Signal = ScottPlot.Plottables.Signal;

namespace AlgoTradeWithScottPlot
{
    public partial class Form1 : Form
    {
        private PlotManager plotManager;
        private ScottPlot.OHLC[]? ohlcData;
        private double[]? volumeData;
        private double[]? sma100;
        private double[]? sma200;
        private double[]? rsi;
        private double[]? macdLine;
        private double[]? signalLine;
        private double[]? histogram;
        private double[]? stochK;
        private double[]? stochD;
        private int[]? signals;
        private double[]? entryPrices;
        private double[]? exitPrices;
        private double[]? unrealizedPnL;
        private double[]? realizedPnL;
        private double[]? cumulativePnL;
        private double[]? balance;

        public Form1()
        {
            InitializeComponent();

            plotManager = new PlotManager(); // Bu satırı ekleyin

            // Initialize any additional UI components if needed
            InitializeUIComponents();
            
            // Initialize plot event handling system
            InitializePlotEvents();
        }

        private void InitializeUIComponents()
        {
            // Initialize and start the status bar timer
            statusTimer = new System.Windows.Forms.Timer();
            statusTimer.Interval = 1000; // Update every second
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();

            // Set initial date and time
            UpdateStatusBarTime();
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            UpdateStatusBarTime();
        }

        private void UpdateStatusBarTime()
        {
            // Update time label with current date and time
            timeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        #region Button Event Handlers - Empty Templates

        // Layout Control Events
        private void btnLinearLayout_Click(object sender, EventArgs e) { }
        private void btnMainChartLayout_Click(object sender, EventArgs e) { }
        private void btnTestGridLayout_Click(object sender, EventArgs e) { }
        private void btnClearCharts_Click(object sender, EventArgs e) { }
        private void btnCloseCharts_Click(object sender, EventArgs e) { }

        // Chart Management Events  
        private void btnCreateGrid_Click(object sender, EventArgs e) { }
        private void btnAddChart_Click(object sender, EventArgs e) { }
        private void btnResetChartsPosition_Click(object sender, EventArgs e) { }
        private void btnHideChart_Click(object sender, EventArgs e) { }
        private void btnShowChart_Click(object sender, EventArgs e) { }
        private void btnCloseChart_Click(object sender, EventArgs e) { }

        // Data Control Events
        private void buttonGenerateData_Click(object sender, EventArgs e) { }
        private void buttonAddData_Click(object sender, EventArgs e) { }
        private void buttonReadFile_Click(object sender, EventArgs e) { }
        private void button2_Click(object sender, EventArgs e) { } // Plot Data
        private void button3_Click(object sender, EventArgs e) { } // Calculate MA
        private void button4_Click(object sender, EventArgs e) { } // Calculate RSI

        private bool showMessage = false; // Set to false to disable message boxes

        // Button1980 Event - Multiplot Demo (3 subplots)
        private void button1980_Click(object sender, EventArgs e)
        {
            try
            {
                // TradingChart component'indeki formsPlot'u al
                var formsPlot = tradingChart.Plot;

                // TradingChart'ın çevre panellerini gizle (multiplot için)
                tradingChart.TopPanel.Visible = true;
                tradingChart.BottomPanel.Visible = false;
                tradingChart.LeftPanel.Visible = true;
                tradingChart.RightPanel.Visible = false;

                // TradingChart'ın tüm kontrollerini gez ve statusStrip'i bul ve gizle
                foreach (Control ctrl in tradingChart.Controls)
                {
                    if (ctrl is Panel panel)
                    {
                        foreach (Control innerCtrl in panel.Controls)
                        {
                            if (innerCtrl is StatusStrip)
                            {
                                innerCtrl.Visible = false;
                            }
                        }
                    }
                }

                // TradingChart'ın arka plan rengini ayarla (beyaz alan kalmaması için)
                //tradingChart.BackColor = System.Drawing.Color.FromArgb(45, 45, 48); // Koyu gri

                // Multiplot'u temizle (eğer önceden oluşturulmuşsa)
                formsPlot.Reset();

                // 9 subplot ekle ve dikey olarak birleştir
                formsPlot.Multiplot.AddPlots(9);
                formsPlot.Multiplot.CollapseVertically();

                // Her subplot için yükseklikleri ayarla
                // Plot 0 (OHLC): 600px, Diğerleri (1-8): 400px each
                int ohlcHeight = 600;
                int otherHeight = 400;
                int totalHeight = ohlcHeight + (8 * otherHeight); // 600 + 3200 = 3800px

                // Layout ile yükseklikleri ayarla (API kontrol edilmeli - şimdilik yorum satırı)
                // var layout = formsPlot.Multiplot.Layout;
                // layout.SetHeights API'si mevcut değil, alternatif:
                // ScottPlot 5'te her subplot'un size'ı otomatik ayarlanıyor
                // Manuel ayar için Layout property'lerini incele

                // Şimdilik eşit yükseklikte plotlar (CollapseVertically ile)
                // totalHeight'ı 9 eşit parçaya böl
                totalHeight = 9 * 400; // 3600px (her plot 400px)

                // FormsPlot'u yeniden yapılandır (scroll için)
                // Dock yerine manuel boyutlandırma kullan (scroll için gerekli)
                formsPlot.Dock = DockStyle.None;
                formsPlot.Height = totalHeight; // Toplam yükseklik = 3600px
                formsPlot.Location = new Point(0, 0);

                // Genişliği parent panel'e göre ayarla (panelCenter'ın gerçek genişliği)
                // Reflection ile panelCenter'ı al
                var panelCenterProp = tradingChart.GetType().GetProperty("panelCenter",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Panel? panelCenterObj = panelCenterProp?.GetValue(tradingChart) as Panel;

                if (panelCenterObj != null)
                {
                    // panelCenter'ın gerçek kullanılabilir alanını kullan
                    int adjustedWidth = panelCenterObj.ClientSize.Width;

                    // LeftPanel visible ise ekstra düzeltme yap
                    if (tradingChart.LeftPanel.Visible)
                    {
                        adjustedWidth -= (tradingChart.LeftPanel.Width - 1); // LeftPanel width + tolerans
                    }

                    // TopPanel visible ise ekstra düzeltme
                    if (tradingChart.TopPanel.Visible)
                    {
                        adjustedWidth -= 5; // Küçük tolerans
                    }

                    formsPlot.Width = adjustedWidth;
                }
                else
                {
                    // Fallback: tradingChart kullan
                    formsPlot.Width = tradingChart.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 20;
                }

                // TradingChart resize event'i ekle - form büyüdüğünde FormsPlot genişliğini güncelle
                tradingChart.Resize += (s, ev) =>
                {
                    // Multiplot aktif ise genişliği güncelle
                    try
                    {
                        if (formsPlot.Dock == DockStyle.None && formsPlot.Height > 1000) // Multiplot modunda
                        {
                            if (panelCenterObj != null)
                            {
                                int resizedWidth = panelCenterObj.ClientSize.Width;

                                // LeftPanel visible ise ekstra düzeltme yap
                                if (tradingChart.LeftPanel.Visible)
                                {
                                    resizedWidth -= tradingChart.LeftPanel.Width + 5;
                                }

                                // TopPanel visible ise ekstra düzeltme
                                if (tradingChart.TopPanel.Visible)
                                {
                                    resizedWidth -= 5;
                                }

                                formsPlot.Width = resizedWidth;
                            }
                            else
                            {
                                formsPlot.Width = tradingChart.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 20;
                            }
                        }
                    }
                    catch { /* Ignore resize errors */ }
                };

                // ===========================================
                // PLOT 0: Candlestick (Price Chart)
                // ===========================================
                var plot0 = formsPlot.Multiplot.GetPlot(0);

                // Örnek OHLC verisi oluştur
                var ohlcData = new List<ScottPlot.OHLC>();
                Random rand = new Random(0);
                double price = 100;
                DateTime startDate = DateTime.Now.AddDays(-100);

                for (int i = 0; i < 100; i++)
                {
                    double open = price;
                    double close = price + rand.NextDouble() * 4 - 2;
                    double high = Math.Max(open, close) + rand.NextDouble() * 2;
                    double low = Math.Min(open, close) - rand.NextDouble() * 2;

                    // OHLC(open, high, low, close, DateTime, TimeSpan)
                    ohlcData.Add(new ScottPlot.OHLC(open, high, low, close, startDate.AddDays(i), TimeSpan.FromDays(1)));
                    price = close;
                }

                // Candlestick plot ekle
                var candlePlot = plot0.Add.Candlestick(ohlcData);

                // Sequential positions kullan (DateTime yerine index)
                candlePlot.Sequential = true;
                // ScottPlot 5'te renk ayarları (API'ye göre değişebilir)
                // candlePlot.ColorUp = ScottPlot.Colors.LimeGreen;
                // candlePlot.ColorDown = ScottPlot.Colors.Red;

                // Plot 0 ayarları
                plot0.Title("Price Chart (Candlestick)");
                plot0.YLabel("Price");
                plot0.Axes.AutoScale();

                // ===========================================
                // PLOT 1: Volume
                // ===========================================
                var plot1 = formsPlot.Multiplot.GetPlot(1);

                // Volume verisi oluştur
                double[] volumeData = new double[100];
                for (int i = 0; i < 100; i++)
                {
                    volumeData[i] = 1000 + rand.NextDouble() * 2000;
                }

                // Bar chart olarak volume ekle
                var volumeBars = plot1.Add.Bars(volumeData);

                // Volume barlarını renklendir (yeşil/kırmızı)
                for (int i = 0; i < volumeBars.Bars.Count; i++)
                {
                    // Close > Open ise yeşil, değilse kırmızı
                    if (i < ohlcData.Count && ohlcData[i].Close >= ohlcData[i].Open)
                    {
                        volumeBars.Bars[i].FillColor = ScottPlot.Colors.LimeGreen.WithAlpha(0.7);
                    }
                    else
                    {
                        volumeBars.Bars[i].FillColor = ScottPlot.Colors.Red.WithAlpha(0.7);
                    }
                }

                // Plot 1 ayarları
                plot1.Title("Volume");
                plot1.YLabel("Volume");
                plot1.Axes.AutoScale();

                // ===========================================
                // PLOT 2: Moving Averages
                // ===========================================
                var plot2 = formsPlot.Multiplot.GetPlot(2);

                // Close fiyatlarından Moving Average hesapla
                double[] closePrices = ohlcData.Select(x => x.Close).ToArray();

                // SMA 20 hesapla
                double[] sma20 = new double[100];
                for (int i = 0; i < 100; i++)
                {
                    if (i < 20)
                    {
                        sma20[i] = closePrices[i]; // İlk 20 için direk fiyat
                    }
                    else
                    {
                        double sum = 0;
                        for (int j = 0; j < 20; j++)
                        {
                            sum += closePrices[i - j];
                        }
                        sma20[i] = sum / 20;
                    }
                }

                // SMA 50 hesapla
                double[] sma50 = new double[100];
                for (int i = 0; i < 100; i++)
                {
                    if (i < 50)
                    {
                        sma50[i] = closePrices[i]; // İlk 50 için direk fiyat
                    }
                    else
                    {
                        double sum = 0;
                        for (int j = 0; j < 50; j++)
                        {
                            sum += closePrices[i - j];
                        }
                        sma50[i] = sum / 50;
                    }
                }

                // MA çizgilerini ekle
                var ma20Line = plot2.Add.Signal(sma20);
                ma20Line.Color = ScottPlot.Colors.Blue;
                ma20Line.LineWidth = 2;
                ma20Line.LegendText = "SMA(20)";

                var ma50Line = plot2.Add.Signal(sma50);
                ma50Line.Color = ScottPlot.Colors.Orange;
                ma50Line.LineWidth = 2;
                ma50Line.LegendText = "SMA(50)";

                // Plot 2 ayarları
                plot2.Title("Moving Averages");
                plot2.YLabel("Price");
                plot2.Axes.AutoScale();
                plot2.ShowLegend();

                // ===========================================
                // PLOT 3: RSI (Relative Strength Index)
                // ===========================================
                var plot3 = formsPlot.Multiplot.GetPlot(3);

                // Basit RSI benzeri veri oluştur (gerçek RSI hesabı yapılmamış, sadece örnek)
                double[] xData = new double[100];
                double[] rsiData = new double[100];

                for (int i = 0; i < 100; i++)
                {
                    xData[i] = i;
                    rsiData[i] = 30 + rand.NextDouble() * 40; // 30-70 arası RSI
                }

                // RSI çizgisi ekle
                var rsiLine = plot3.Add.Signal(rsiData);
                rsiLine.Color = ScottPlot.Colors.Blue;
                rsiLine.LineWidth = 2;
                rsiLine.LegendText = "RSI";

                // RSI referans çizgileri (30 ve 70)
                var hline30 = plot3.Add.HorizontalLine(30);
                hline30.LineColor = ScottPlot.Colors.Red;
                hline30.LineWidth = 1;
                hline30.LinePattern = ScottPlot.LinePattern.Dashed;

                var hline70 = plot3.Add.HorizontalLine(70);
                hline70.LineColor = ScottPlot.Colors.Red;
                hline70.LineWidth = 1;
                hline70.LinePattern = ScottPlot.LinePattern.Dashed;

                // Plot 3 ayarları
                plot3.Title("RSI Indicator");
                plot3.YLabel("RSI");
                plot3.Axes.SetLimitsY(0, 100);
                plot3.Axes.AutoScaleX();
                plot3.ShowLegend();

                // ===========================================
                // PLOT 4: MACD (Moving Average Convergence Divergence)
                // ===========================================
                var plot4 = formsPlot.Multiplot.GetPlot(4);

                // Basit MACD hesaplama (gerçek MACD için EMA kullanılmalı)
                double[] macdLine = new double[100];
                double[] signalLine = new double[100];
                double[] histogram = new double[100];

                // Simplified MACD calculation (12-26-9)
                for (int i = 0; i < 100; i++)
                {
                    if (i < 26)
                    {
                        macdLine[i] = 0;
                        signalLine[i] = 0;
                        histogram[i] = 0;
                    }
                    else
                    {
                        // Basitleştirilmiş MACD (gerçek EMA yerine SMA kullanıyoruz)
                        double ema12 = closePrices.Skip(i - 12).Take(12).Average();
                        double ema26 = closePrices.Skip(i - 26).Take(26).Average();
                        macdLine[i] = ema12 - ema26;

                        if (i >= 35) // 26 + 9
                        {
                            signalLine[i] = macdLine.Skip(i - 9).Take(9).Average();
                            histogram[i] = macdLine[i] - signalLine[i];
                        }
                    }
                }

                // MACD line
                var macdPlot = plot4.Add.Signal(macdLine);
                macdPlot.Color = ScottPlot.Colors.Blue;
                macdPlot.LineWidth = 2;
                macdPlot.LegendText = "MACD";

                // Signal line
                var signalPlot = plot4.Add.Signal(signalLine);
                signalPlot.Color = ScottPlot.Colors.Red;
                signalPlot.LineWidth = 2;
                signalPlot.LegendText = "Signal";

                // Histogram
                var histogramBars = plot4.Add.Bars(histogram);
                for (int i = 0; i < histogramBars.Bars.Count; i++)
                {
                    histogramBars.Bars[i].FillColor = histogram[i] >= 0
                        ? ScottPlot.Colors.Green.WithAlpha(0.5)
                        : ScottPlot.Colors.Red.WithAlpha(0.5);
                }

                // Zero line
                var zeroLine = plot4.Add.HorizontalLine(0);
                zeroLine.LineColor = ScottPlot.Colors.Gray;
                zeroLine.LineWidth = 1;
                zeroLine.LinePattern = ScottPlot.LinePattern.Dashed;

                // Plot 4 ayarları
                plot4.Title("MACD");
                plot4.YLabel("MACD");
                plot4.Axes.AutoScale();
                plot4.ShowLegend();

                // ===========================================
                // PLOT 5: Stochastic Oscillator
                // ===========================================
                var plot5 = formsPlot.Multiplot.GetPlot(5);

                // Stochastic %K ve %D hesaplama (14,3,3)
                double[] stochK = new double[100];
                double[] stochD = new double[100];
                int kPeriod = 14;

                for (int i = 0; i < 100; i++)
                {
                    if (i < kPeriod)
                    {
                        stochK[i] = 50; // Default değer
                        stochD[i] = 50;
                    }
                    else
                    {
                        // Son 14 bar içinde en yüksek ve en düşük
                        double highest = double.MinValue;
                        double lowest = double.MaxValue;

                        for (int j = 0; j < kPeriod; j++)
                        {
                            if (ohlcData[i - j].High > highest) highest = ohlcData[i - j].High;
                            if (ohlcData[i - j].Low < lowest) lowest = ohlcData[i - j].Low;
                        }

                        // %K hesapla
                        if (highest != lowest)
                        {
                            stochK[i] = ((closePrices[i] - lowest) / (highest - lowest)) * 100;
                        }
                        else
                        {
                            stochK[i] = 50;
                        }

                        // %D hesapla (3-period SMA of %K)
                        if (i >= kPeriod + 2)
                        {
                            stochD[i] = (stochK[i] + stochK[i - 1] + stochK[i - 2]) / 3;
                        }
                        else
                        {
                            stochD[i] = stochK[i];
                        }
                    }
                }

                // %K line
                var kLine = plot5.Add.Signal(stochK);
                kLine.Color = ScottPlot.Colors.Blue;
                kLine.LineWidth = 2;
                kLine.LegendText = "%K";

                // %D line
                var dLine = plot5.Add.Signal(stochD);
                dLine.Color = ScottPlot.Colors.Red;
                dLine.LineWidth = 2;
                dLine.LegendText = "%D";

                // Overbought/Oversold lines
                var stochLine80 = plot5.Add.HorizontalLine(80);
                stochLine80.LineColor = ScottPlot.Colors.Red;
                stochLine80.LineWidth = 1;
                stochLine80.LinePattern = ScottPlot.LinePattern.Dashed;

                var stochLine20 = plot5.Add.HorizontalLine(20);
                stochLine20.LineColor = ScottPlot.Colors.Red;
                stochLine20.LineWidth = 1;
                stochLine20.LinePattern = ScottPlot.LinePattern.Dashed;

                // Plot 5 ayarları
                plot5.Title("Stochastic Oscillator");
                plot5.YLabel("Stochastic");
                plot5.Axes.SetLimitsY(0, 100);
                plot5.Axes.AutoScaleX();
                plot5.ShowLegend();

                // ===========================================
                // PLOT 6: Trading Signals (Buy/Sell)
                // ===========================================
                var plot6 = formsPlot.Multiplot.GetPlot(6);

                // Rastgele buy/sell sinyalleri oluştur
                List<double> buySignalX = new List<double>();
                List<double> buySignalY = new List<double>();
                List<double> sellSignalX = new List<double>();
                List<double> sellSignalY = new List<double>();

                for (int i = 0; i < 100; i++)
                {
                    if (rand.Next(0, 10) < 2) // %20 buy sinyali
                    {
                        buySignalX.Add(i);
                        buySignalY.Add(1); // Signal = 1 (Buy)
                    }
                    if (rand.Next(0, 10) < 2) // %20 sell sinyali
                    {
                        sellSignalX.Add(i);
                        sellSignalY.Add(-1); // Signal = -1 (Sell)
                    }
                }

                // Buy signals (yeşil yukarı üçgen)
                if (buySignalX.Count > 0)
                {
                    var buyScatter = plot6.Add.Scatter(buySignalX.ToArray(), buySignalY.ToArray());
                    buyScatter.Color = ScottPlot.Colors.Green;
                    buyScatter.MarkerSize = 10;
                    buyScatter.MarkerShape = ScottPlot.MarkerShape.FilledTriangleUp;
                    buyScatter.LegendText = "Buy";
                }

                // Sell signals (kırmızı aşağı üçgen)
                if (sellSignalX.Count > 0)
                {
                    var sellScatter = plot6.Add.Scatter(sellSignalX.ToArray(), sellSignalY.ToArray());
                    sellScatter.Color = ScottPlot.Colors.Red;
                    sellScatter.MarkerSize = 10;
                    sellScatter.MarkerShape = ScottPlot.MarkerShape.FilledTriangleDown;
                    sellScatter.LegendText = "Sell";
                }

                // Zero line
                var signalZeroLine = plot6.Add.HorizontalLine(0);
                signalZeroLine.LineColor = ScottPlot.Colors.Gray;
                signalZeroLine.LineWidth = 1;

                // Plot 6 ayarları
                plot6.Title("Trading Signals");
                plot6.YLabel("Signal");
                plot6.Axes.SetLimitsY(-1.5, 1.5);
                plot6.Axes.AutoScaleX();
                plot6.ShowLegend();

                // ===========================================
                // PLOT 7: PnL (Profit and Loss)
                // ===========================================
                var plot7 = formsPlot.Multiplot.GetPlot(7);

                // Kümülatif PnL hesapla (basit simülasyon)
                double[] pnlData = new double[100];
                double cumulativePnL = 0;
                bool inPosition = false;
                double entryPrice = 0;

                for (int i = 0; i < 100; i++)
                {
                    // Buy sinyali varsa pozisyon aç
                    if (buySignalX.Contains(i) && !inPosition)
                    {
                        inPosition = true;
                        entryPrice = closePrices[i];
                    }
                    // Sell sinyali varsa pozisyon kapat ve PnL hesapla
                    else if (sellSignalX.Contains(i) && inPosition)
                    {
                        double pnl = closePrices[i] - entryPrice;
                        cumulativePnL += pnl;
                        inPosition = false;
                    }

                    pnlData[i] = cumulativePnL;
                }

                // PnL line
                var pnlLine = plot7.Add.Signal(pnlData);
                pnlLine.Color = ScottPlot.Colors.Blue;
                pnlLine.LineWidth = 2;
                pnlLine.LegendText = "Cumulative PnL";

                // Zero line
                var pnlZeroLine = plot7.Add.HorizontalLine(0);
                pnlZeroLine.LineColor = ScottPlot.Colors.Gray;
                pnlZeroLine.LineWidth = 1;
                pnlZeroLine.LinePattern = ScottPlot.LinePattern.Dashed;

                // Pozitif/negatif alanları renklendir (yorum satırı - API kontrol edilmeli)
                // var pnlFill = plot7.Add.FillY(ScottPlot.Generate.Consecutive(100), pnlData);
                // pnlFill.FillColor = cumulativePnL >= 0
                //     ? ScottPlot.Colors.Green.WithAlpha(0.2)
                //     : ScottPlot.Colors.Red.WithAlpha(0.2);
                // pnlFill.LineWidth = 0;

                // Plot 7 ayarları
                plot7.Title("Profit & Loss (PnL)");
                plot7.YLabel("PnL");
                plot7.Axes.AutoScale();
                plot7.ShowLegend();

                // ===========================================
                // PLOT 8: Balance (Account Balance)
                // ===========================================
                var plot8 = formsPlot.Multiplot.GetPlot(8);

                // Başlangıç bakiyesi
                double initialBalance = 10000;
                double[] balanceData = new double[100];

                // Balance = InitialBalance + CumulativePnL
                for (int i = 0; i < 100; i++)
                {
                    balanceData[i] = initialBalance + pnlData[i];
                }

                // Balance line
                var balanceLine = plot8.Add.Signal(balanceData);
                balanceLine.Color = ScottPlot.Colors.DarkGreen;
                balanceLine.LineWidth = 3;
                balanceLine.LegendText = "Account Balance";

                // Initial balance reference line
                var initialBalanceLine = plot8.Add.HorizontalLine(initialBalance);
                initialBalanceLine.LineColor = ScottPlot.Colors.Gray;
                initialBalanceLine.LineWidth = 1;
                initialBalanceLine.LinePattern = ScottPlot.LinePattern.Dashed;
                // initialBalanceLine.LabelText = $"Initial: ${initialBalance:N0}"; // API kontrol edilmeli

                // Balance area fill (yorum satırı - API kontrol edilmeli)
                // var balanceFill = plot8.Add.FillY(ScottPlot.Generate.Consecutive(100), balanceData);
                // balanceFill.FillColor = ScottPlot.Colors.Green.WithAlpha(0.1);
                // balanceFill.LineWidth = 0;

                // Plot 8 ayarları
                plot8.Title("Account Balance");
                plot8.YLabel("Balance ($)");
                plot8.XLabel("Time (Bar Index)");
                plot8.Axes.AutoScale();
                plot8.ShowLegend();

                // ===========================================
                // X Eksenini Paylaş (Zoom/Pan Sync)
                // ===========================================
                // SharedAxes'den ÖNCE her plot'un axis limitlerini ayarla
                plot0.Axes.AutoScale();
                plot1.Axes.AutoScale();
                plot2.Axes.AutoScale();
                plot3.Axes.AutoScale();
                plot4.Axes.AutoScale();
                plot5.Axes.AutoScale();
                plot6.Axes.AutoScale();
                plot7.Axes.AutoScale();
                plot8.Axes.AutoScale();

                // Şimdi X eksenini paylaş
                formsPlot.Multiplot.SharedAxes.ShareX(new[] { plot0, plot1, plot2, plot3, plot4, plot5, plot6, plot7, plot8 });

                // X ekseni limitleri ayarla (0-100 bar)
                foreach (var plot in new[] { plot0, plot1, plot2, plot3, plot4, plot5, plot6, plot7, plot8 })
                {
                    plot.Axes.SetLimitsX(0, 100);
                    plot.Axes.AutoScaleY(); // Y eksenini otomatik ayarla
                }

                // Grid ayarları - tüm plotlar son plotun (plot8) X eksenini kullanır
                plot0.Grid.XAxis = plot8.Axes.Bottom;
                plot1.Grid.XAxis = plot8.Axes.Bottom;
                plot2.Grid.XAxis = plot8.Axes.Bottom;
                plot3.Grid.XAxis = plot8.Axes.Bottom;
                plot4.Grid.XAxis = plot8.Axes.Bottom;
                plot5.Grid.XAxis = plot8.Axes.Bottom;
                plot6.Grid.XAxis = plot8.Axes.Bottom;
                plot7.Grid.XAxis = plot8.Axes.Bottom;

                // Her plotun grid'i kendi Right axis'ini kullanır
                plot0.Grid.YAxis = plot0.Axes.Right;
                plot1.Grid.YAxis = plot1.Axes.Right;
                plot2.Grid.YAxis = plot2.Axes.Right;
                plot3.Grid.YAxis = plot3.Axes.Right;
                plot4.Grid.YAxis = plot4.Axes.Right;
                plot5.Grid.YAxis = plot5.Axes.Right;
                plot6.Grid.YAxis = plot6.Axes.Right;
                plot7.Grid.YAxis = plot7.Axes.Right;
                plot8.Grid.YAxis = plot8.Axes.Right;

                // ===========================================
                // Mouse Scroll Event Handler
                // ===========================================
                // FormsPlot üzerinde mouse scroll → Zoom in/out
                // ScrollBar üzerinde mouse scroll → Plotlar arasında hareket

                bool isMouseOverPlot = false;

                formsPlot.MouseEnter += (s, e) => isMouseOverPlot = true;
                formsPlot.MouseLeave += (s, e) => isMouseOverPlot = false;

                // Mouse scroll event'i - parent scroll'u engelle
                formsPlot.MouseWheel += (s, e) =>
                {
                    if (isMouseOverPlot)
                    {
                        // Plot üzerinde → Zoom yapılacak (ScottPlot default behavior)
                        // Parent panelin scroll yapmasını engelle
                        ((HandledMouseEventArgs)e).Handled = true;
                    }
                };

                // ===========================================
                // Tüm plotları refresh et
                // ===========================================
                formsPlot.Refresh();

                // Parent panel'e AutoScroll ekle (eğer yoksa)
                if (tradingChart.Parent is Panel parentPanel)
                {
                    parentPanel.AutoScroll = true;
                }

                // TradingChart'ın kendi panelCenter'ına da AutoScroll ekle ve renk ayarla
                var panelCenterProperty = tradingChart.GetType().GetProperty("panelCenter",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (panelCenterProperty != null)
                {
                    var panelCenterInner = panelCenterProperty.GetValue(tradingChart) as Panel;
                    if (panelCenterInner != null)
                    {
                        panelCenterInner.AutoScroll = true;
                        panelCenterInner.BackColor = System.Drawing.Color.FromArgb(45, 45, 48); // Koyu gri - beyaz alan kalmaması için

                        // Panel üzerinde mouse scroll → Dikey scroll (plotlar arasında hareket)
                        panelCenterInner.MouseWheel += (s, e) =>
                        {
                            // Mouse FormsPlot üzerinde değilse, panel scroll yapsın
                            if (!formsPlot.ClientRectangle.Contains(formsPlot.PointToClient(System.Windows.Forms.Cursor.Position)))
                            {
                                // Panel scroll (default behavior)
                                // e.Handled zaten false olduğu için otomatik scroll yapılacak
                            }
                        };
                    }
                }

                if (showMessage)
                {
                    MessageBox.Show(
                        "9 subplot başarıyla oluşturuldu!\n\n" +
                        $"Toplam Yükseklik: {totalHeight}px (3600px)\n\n" +
                        "Plot 0: Price (Candlestick) - 400px\n" +
                        "Plot 1: Volume - 400px\n" +
                        "Plot 2: Moving Averages (SMA 20 & 50) - 400px\n" +
                        "Plot 3: RSI Indicator - 400px\n" +
                        "Plot 4: MACD - 400px\n" +
                        "Plot 5: Stochastic Oscillator - 400px\n" +
                        "Plot 6: Trading Signals (Buy/Sell) - 400px\n" +
                        "Plot 7: Profit & Loss (PnL) - 400px\n" +
                        "Plot 8: Account Balance - 400px\n\n" +
                        "✅ Özellikler:\n" +
                        "• X ekseni senkronize (tüm plotlar birlikte zoom/pan)\n" +
                        "• Plot üzerinde mouse scroll → Zoom in/out\n" +
                        "• ScrollBar üzerinde mouse scroll → Plotlar arası hareket\n" +
                        "• Form resize → Otomatik genişlik ayarı\n" +
                        "• Dikey scroll bar ile gezinme",
                        "Multiplot Demo - Full Trading Dashboard",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Hata oluştu:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Menu Events - File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("New file functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Open file functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Save file functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Save As functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Menu Events - Edit
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Undo functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Redo functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Cut functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Copy functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Paste functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Select All functionality not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Menu Events - View
        private void toolbarsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainToolStrip.Visible = !mainToolStrip.Visible;
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusBar.Visible = !statusBar.Visible;
            statusBarToolStripMenuItem.Checked = statusBar.Visible;
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                fullScreenToolStripMenuItem.Text = "Exit &Full Screen";
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                fullScreenToolStripMenuItem.Text = "&Full Screen";
            }
        }

        // Menu Events - Tools
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Options dialog not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Preferences dialog not implemented yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Menu Events - Help
        private void documentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Documentation not implemented yet.\\nPress F1 for help.", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("AlgoTrade - Trading Dashboard\\nVersion 1.0\\n\\nBuilt with ScottPlot and .NET 9.0\\n\\n© 2024", "About AlgoTrade", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ToolStrip Events
        private void mainToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Handle toolbar clicks through the ItemClicked event
            switch (e.ClickedItem?.Name)
            {
                case "newToolStripButton":
                    newToolStripMenuItem_Click(sender, e);
                    break;
                case "openToolStripButton":
                    openToolStripMenuItem_Click(sender, e);
                    break;
                case "saveToolStripButton":
                    saveToolStripMenuItem_Click(sender, e);
                    break;
                case "cutToolStripButton":
                    cutToolStripMenuItem_Click(sender, e);
                    break;
                case "copyToolStripButton":
                    copyToolStripMenuItem_Click(sender, e);
                    break;
                case "pasteToolStripButton":
                    pasteToolStripMenuItem_Click(sender, e);
                    break;
                case "helpToolStripButton":
                    documentationToolStripMenuItem_Click(sender, e);
                    break;
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            newToolStripMenuItem_Click(sender, e);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(sender, e);
        }

        private void cutToolStripButton_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(sender, e);
        }

        private void copyToolStripButton_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }

        private void helpToolStripButton_Click(object sender, EventArgs e)
        {
            documentationToolStripMenuItem_Click(sender, e);
        }

        #endregion

        #region Plot Event Handling System

        /// <summary>
        /// Tüm plot event'lerini initialize eder
        /// </summary>
        private void InitializePlotEvents()
        {
            InitializeMouseEvents();
            InitializeKeyboardEvents();
            InitializePlotLifecycleEvents();
            InitializeTradingSpecificEvents();
        }

        #region Mouse Events

        /// <summary>
        /// Mouse event'lerini initialize eder
        /// </summary>
        private void InitializeMouseEvents()
        {
            if (tradingChart?.Plot == null) return;

            tradingChart.Plot.MouseClick += OnPlotMouseClick;
            tradingChart.Plot.MouseDoubleClick += OnPlotMouseDoubleClick;
            tradingChart.Plot.MouseDown += OnPlotMouseDown;
            tradingChart.Plot.MouseUp += OnPlotMouseUp;
            tradingChart.Plot.MouseMove += OnPlotMouseMove;
            tradingChart.Plot.MouseEnter += OnPlotMouseEnter;
            tradingChart.Plot.MouseLeave += OnPlotMouseLeave;
            tradingChart.Plot.MouseWheel += OnPlotMouseWheel;
        }

        private void OnPlotMouseClick(object? sender, MouseEventArgs e)
        {
            var plotIndex = GetPlotIndexAtPosition(e.X, e.Y);
            var plotId = plotManager.GetPlotId(plotIndex);
            var coords = GetCoordinatesAtPosition(e.X, e.Y, plotIndex);

            System.Diagnostics.Debug.WriteLine($"Plot Click - Index: {plotIndex}, ID: {plotId}, Button: {e.Button}, Coords: ({coords.X:F2}, {coords.Y:F2})");

            // Plot tipine göre özel işlemler
            switch (plotId)
            {
                case "0": // Candlestick
                    OnCandlestickClick(coords, e.Button);
                    break;
                default:
                    OnIndicatorClick(plotIndex, coords, e.Button);
                    break;
            }
        }

        private void OnPlotMouseDoubleClick(object? sender, MouseEventArgs e)
        {
            var plotIndex = GetPlotIndexAtPosition(e.X, e.Y);
            var plotId = plotManager.GetPlotId(plotIndex);

            System.Diagnostics.Debug.WriteLine($"Plot DoubleClick - Index: {plotIndex}, ID: {plotId}, Button: {e.Button}");

            if (e.Button == MouseButtons.Middle)
            {
                OnMiddleDoubleClick(plotIndex);
            }
            else if (e.Button == MouseButtons.Left)
            {
                OnLeftDoubleClick(plotIndex);
            }
        }

        private bool isMouseDown = false;
        private Point mouseDownPoint;
        private MouseButtons mouseDownButton;
        private DateTime lastRefreshTime = DateTime.MinValue;
        
        // Interaction flags for performance optimization
        private bool isDragging = false;
        private bool isZooming = false;
        private bool isPanning = false;
        private bool isDividerDrag = false;
        private bool isUpdatingAxisSync = false;
        
        // Divider drag state
        private int? dividerBeingDragged = null;
        private ScottPlot.MultiplotLayouts.DraggableRows? customLayout = null;
        
        // Crosshair list for all plots
        private List<ScottPlot.Plottables.Crosshair> crosshairs = new List<ScottPlot.Plottables.Crosshair>();
        private bool useCrossHairAllPlots = true;
        private ScottPlot.Plot[] allPlots = new ScottPlot.Plot[0];

        private void OnPlotMouseDown(object? sender, MouseEventArgs e)
        {
            isMouseDown = true;
            mouseDownPoint = new Point(e.X, e.Y);
            mouseDownButton = e.Button;

            var plotIndex = GetPlotIndexAtPosition(e.X, e.Y);
            System.Diagnostics.Debug.WriteLine($"Plot MouseDown - Index: {plotIndex}, Button: {e.Button}, Position: ({e.X}, {e.Y})");
        }

        private void OnPlotMouseUp(object? sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                var dragDistance = Math.Sqrt(Math.Pow(e.X - mouseDownPoint.X, 2) + Math.Pow(e.Y - mouseDownPoint.Y, 2));
                
                if (dragDistance > 5) // Minimum drag distance
                {
                    OnPlotDragEnd(mouseDownPoint, new Point(e.X, e.Y), mouseDownButton);
                }
            }

            // Reset all flags
            isMouseDown = false;
            isDragging = false;
            isPanning = false;
            isZooming = false;
        }

        private void OnPlotMouseMove(object? sender, MouseEventArgs e)
        {
            var plotIndex = GetPlotIndexAtPosition(e.X, e.Y);
            var coords = GetCoordinatesAtPosition(e.X, e.Y, plotIndex);

            if (isMouseDown)
            {
                // Drag başladığında flag'i set et
                if (!isDragging)
                {
                    var dragDistance = Math.Sqrt(Math.Pow(e.X - mouseDownPoint.X, 2) + Math.Pow(e.Y - mouseDownPoint.Y, 2));
                    if (dragDistance > 3) // Minimum drag threshold
                    {
                        isDragging = true;
                        if (mouseDownButton == MouseButtons.Left)
                        {
                            isPanning = true;
                        }
                    }
                }

                if (isDragging)
                {
                    OnPlotDragging(mouseDownPoint, new Point(e.X, e.Y), mouseDownButton);
                    // Dragging sırasında crosshair'i gizle
                    return; 
                }
            }

            // Crosshair güncelleme - sadece etkileşim yokken ve throttle ile
            if (!isDragging && !isZooming && !isPanning && !isDividerDrag)
            {
                var now = DateTime.Now;
                if ((now - lastRefreshTime).TotalMilliseconds > 16) // 60 FPS limit
                {
                    OnCrosshairMove(plotIndex, coords);
                    lastRefreshTime = now;
                }
            }
        }

        private void OnPlotMouseEnter(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Plot MouseEnter");
        }

        private void OnPlotMouseLeave(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Plot MouseLeave");
            OnCrosshairMove(-1, new ScottPlot.Coordinates(0, 0)); // Hide crosshair
        }

        private void OnPlotMouseWheel(object? sender, MouseEventArgs e)
        {
            var plotIndex = GetPlotIndexAtPosition(e.X, e.Y);
            var zoomDirection = e.Delta > 0 ? "In" : "Out";
            
            System.Diagnostics.Debug.WriteLine($"Plot MouseWheel - Index: {plotIndex}, Direction: {zoomDirection}, Delta: {e.Delta}");
            
            // Zoom flag'ini set et
            isZooming = true;
            
            OnPlotZoom(plotIndex, e.Delta > 0, new Point(e.X, e.Y));
            
            // Zoom işlemi tamamlandıktan sonra flag'i reset et (kısa delay ile)
            Task.Delay(100).ContinueWith(_ => isZooming = false);
        }

        private void OnPlotDragging(Point startPoint, Point currentPoint, MouseButtons button)
        {
            var deltaX = currentPoint.X - startPoint.X;
            var deltaY = currentPoint.Y - startPoint.Y;

            switch (button)
            {
                case MouseButtons.Left:
                    OnLeftDrag(startPoint, currentPoint, deltaX, deltaY);
                    break;
                case MouseButtons.Right:
                    OnRightDrag(startPoint, currentPoint, deltaX, deltaY);
                    break;
                case MouseButtons.Middle:
                    OnMiddleDrag(startPoint, currentPoint, deltaX, deltaY);
                    break;
            }
        }

        private void OnPlotDragEnd(Point startPoint, Point endPoint, MouseButtons button)
        {
            System.Diagnostics.Debug.WriteLine($"Plot DragEnd - Button: {button}, Start: ({startPoint.X}, {startPoint.Y}), End: ({endPoint.X}, {endPoint.Y})");
        }

        private void OnLeftDrag(Point start, Point current, int deltaX, int deltaY)
        {
            // Sol drag: Pan işlemi (varsayılan ScottPlot davranışı)
            System.Diagnostics.Debug.WriteLine($"Left Drag - Delta: ({deltaX}, {deltaY})");
        }

        private void OnRightDrag(Point start, Point current, int deltaX, int deltaY)
        {
            // Sağ drag: Özel işlem (örn: zoom rectangle)
            System.Diagnostics.Debug.WriteLine($"Right Drag - Delta: ({deltaX}, {deltaY})");
        }

        private void OnMiddleDrag(Point start, Point current, int deltaX, int deltaY)
        {
            // Orta drag: Zoom rectangle veya özel işlem
            System.Diagnostics.Debug.WriteLine($"Middle Drag - Delta: ({deltaX}, {deltaY})");
        }

        private void OnMiddleDoubleClick(int plotIndex)
        {
            System.Diagnostics.Debug.WriteLine($"Middle DoubleClick - Plot: {plotIndex} - Auto Scale");
            // Auto scale işlemi
            if (plotIndex >= 0 && plotIndex < plotManager.Count)
            {
                var plot = plotManager.GetPlot(plotIndex);
                plot.Axes.AutoScale();
                plotManager.Refresh();
            }
        }

        private void OnLeftDoubleClick(int plotIndex)
        {
            System.Diagnostics.Debug.WriteLine($"Left DoubleClick - Plot: {plotIndex}");
            // Özel çift tıklama işlemi
        }

        /// <summary>
        /// TradingChart mouse wheel event handler - zoom functionality
        /// </summary>
        private void TradingChartPlotMouseWheel(object? sender, MouseEventArgs e)
        {
            // Mouse wheel ile zoom yap
            double zoomFactor = e.Delta > 0 ? 0.9 : 1.1;  // Delta > 0 = zoom in, Delta < 0 = zoom out

            // Mouse'un üzerinde olduğu plot'u bul
            var mousePixel = new ScottPlot.Pixel(e.X, e.Y);

            // Tüm plotlarda zoom yap (X ekseni shared olduğu için)
            foreach (var plot in tradingChart.Plot.Multiplot.GetPlots())
            {
                var currentLimits = plot.Axes.GetLimits();

                // X ekseninde zoom (tüm plotlarda aynı)
                double centerX = (currentLimits.Left + currentLimits.Right) / 2;
                double spanX = (currentLimits.Right - currentLimits.Left) * zoomFactor;

                // Y ekseninde zoom (her plot kendi Y ekseninde)
                double centerY = (currentLimits.Bottom + currentLimits.Top) / 2;
                double spanY = (currentLimits.Top - currentLimits.Bottom) * zoomFactor;

                plot.Axes.SetLimits(
                    centerX - spanX / 2, centerX + spanX / 2,
                    centerY - spanY / 2, centerY + spanY / 2
                );
            }

            tradingChart.Plot.Refresh();

            // Event'i handle et - scrollbar'a gitmesin
            ((System.Windows.Forms.HandledMouseEventArgs)e).Handled = true;
        }

        /// <summary>
        /// TradingChart resize event handler - multiplot genişlik ayarı
        /// </summary>
        private void TradingChartResize(object? sender, EventArgs e)
        {
            // Multiplot aktif ise genişliği güncelle
            try
            {
                if (tradingChart.Plot.Dock == DockStyle.None && tradingChart.Plot.Height > 1000) // Multiplot modunda
                {
                    // Reflection ile panelCenter'ı al
                    var panelCenterProp = tradingChart.GetType().GetProperty("panelCenter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Panel? panelCenterObj = panelCenterProp?.GetValue(tradingChart) as Panel;

                    if (panelCenterObj != null)
                    {
                        int resizedWidth = panelCenterObj.ClientSize.Width;

                        // LeftPanel visible ise ekstra düzeltme yap
                        if (tradingChart.LeftPanel.Visible)
                        {
                            resizedWidth -= tradingChart.LeftPanel.Width + 5;
                        }

                        // RightPanel visible ise ekstra düzeltme yap
                        if (tradingChart.RightPanel.Visible)
                        {
                            resizedWidth -= tradingChart.RightPanel.Width + 5;
                        }

                        // TopPanel visible ise ekstra düzeltme
                        if (tradingChart.TopPanel.Visible)
                        {
                            resizedWidth -= 5;
                        }

                        // Scrollbar ve plot margin'leri için ekstra boşluk (axes lock size kullanıldığı için daha az gerekli)
                        resizedWidth -= 20;

                        tradingChart.Plot.Width = resizedWidth;
                    }
                    else
                    {
                        tradingChart.Plot.Width = tradingChart.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 20;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// TradingChart plot mouse down event handler - divider drag detection
        /// </summary>
        private void TradingChartPlotMouseDown(object? sender, MouseEventArgs e)
        {
            dividerBeingDragged = customLayout.GetDivider(e.Y);
            tradingChart.Plot.UserInputProcessor.IsEnabled = dividerBeingDragged is null;
            
            // Divider drag flag'ini set et
            if (dividerBeingDragged is not null)
            {
                isDividerDrag = true;
            }
        }

        /// <summary>
        /// TradingChart plot mouse up event handler - divider drag completion
        /// </summary>
        private void TradingChartPlotMouseUp(object? sender, MouseEventArgs e)
        {
            if (dividerBeingDragged is not null)
            {
                dividerBeingDragged = null;
                tradingChart.Plot.UserInputProcessor.IsEnabled = true;
            }
            isDividerDrag = false;
        }

        /// <summary>
        /// TradingChart plot mouse move event handler - divider dragging and cursor
        /// </summary>
        private void TradingChartPlotMouseMove(object? sender, MouseEventArgs e)
        {
            HandleDividerDrag(e);
            HandleCrosshair(e);
        }

        /// <summary>
        /// Handle divider dragging and cursor changes
        /// </summary>
        private void HandleDividerDrag(MouseEventArgs e)
        {
            if (dividerBeingDragged is not null)
            {
                customLayout?.SetDivider(dividerBeingDragged.Value, e.Y);
                tradingChart.Plot.Refresh();
            }

            Cursor = customLayout?.GetDivider(e.Y) is not null ? Cursors.SizeNS : Cursors.Default;
        }

        /// <summary>
        /// Handle crosshair display logic
        /// </summary>
        private void HandleCrosshair(MouseEventArgs e)
        {
            // Eğer divider sürükleniyorsa crosshair gösterme
            if (dividerBeingDragged is not null) return;

            // Mouse koordinatlarını al
            var mousePixel = new ScottPlot.Pixel(e.X, e.Y);

            if (useCrossHairAllPlots)
            {
                // MOD 1: Tüm plotlarda crosshair göster (X senkronize, Y her plot'ta farklı)
                // Önce mouse'un hangi plot üzerinde olduğunu bul
                int mouseOverPlotIndex = -1;
                for (int i = 0; i < allPlots.Length; i++)
                {
                    var plotRect = allPlots[i].RenderManager.LastRender.DataRect;
                    if (e.X >= plotRect.Left && e.X <= plotRect.Right &&
                        e.Y >= plotRect.Top && e.Y <= plotRect.Bottom)
                    {
                        mouseOverPlotIndex = i;
                        break;
                    }
                }

                // Eğer mouse herhangi bir plot üzerindeyse
                if (mouseOverPlotIndex >= 0)
                {
                    // X koordinatını mouse'un olduğu plot'tan al
                    var mainCoords = allPlots[mouseOverPlotIndex].GetCoordinates(mousePixel);
                    double sharedX = mainCoords.X;

                    // Tüm plotlarda aynı X koordinatında crosshair göster
                    for (int i = 0; i < allPlots.Length && i < crosshairs.Count; i++)
                    {
                        var plot = allPlots[i];
                        var crosshair = crosshairs[i];

                        if (i == mouseOverPlotIndex)
                        {
                            // Mouse'un üzerindeki plot - tam koordinatları kullan
                            crosshair.Position = mainCoords;
                        }
                        else
                        {
                            // Diğer plotlar - aynı X, kendi plot'larının ortasında Y
                            var plotLimits = plot.Axes.GetLimits();
                            double centerY = (plotLimits.Top + plotLimits.Bottom) / 2;
                            crosshair.Position = new ScottPlot.Coordinates(sharedX, centerY);
                        }

                        crosshair.IsVisible = true;
                    }
                }
                else
                {
                    // Mouse hiçbir plot üzerinde değil - tümünü gizle
                    foreach (var crosshair in crosshairs)
                    {
                        crosshair.IsVisible = false;
                    }
                }
            }
            else
            {
                // MOD 2: Sadece mouse'un üzerindeki plot'ta crosshair göster
                for (int i = 0; i < allPlots.Length && i < crosshairs.Count; i++)
                {
                    var plot = allPlots[i];
                    var crosshair = crosshairs[i];

                    // Mouse'un bu plot üzerinde olup olmadığını kontrol et
                    var plotRect = plot.RenderManager.LastRender.DataRect;
                    if (e.X >= plotRect.Left && e.X <= plotRect.Right &&
                        e.Y >= plotRect.Top && e.Y <= plotRect.Bottom)
                    {
                        // Mouse bu plot üzerinde - koordinatları al
                        var coords = plot.GetCoordinates(mousePixel);
                        crosshair.Position = coords;
                        crosshair.IsVisible = true;
                    }
                    else
                    {
                        // Mouse bu plot üzerinde değil - gizle
                        crosshair.IsVisible = false;
                    }
                }
            }

            tradingChart.Plot.Refresh();
        }

        private void TradingChartPlotMouseLeave(object? sender, EventArgs e)
        {
            foreach (var crosshair in crosshairs)
            {
                crosshair.IsVisible = false;
            }
            tradingChart.Plot.Refresh();
        }

#endregion

#region Keyboard Events

/// <summary>
/// Keyboard event'lerini initialize eder
/// </summary>
private void InitializeKeyboardEvents()
        {
            this.KeyPreview = true; // Form'un key event'leri yakalaması için
            this.KeyDown += OnFormKeyDown;
            this.KeyUp += OnFormKeyUp;
            this.KeyPress += OnFormKeyPress;
        }

        private void OnFormKeyDown(object? sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Key Down: {e.KeyCode}, Ctrl: {e.Control}, Shift: {e.Shift}, Alt: {e.Alt}");

            // Hotkey kombinasyonları
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.C:
                        OnCopyData();
                        break;
                    case Keys.V:
                        OnPasteData();
                        break;
                    case Keys.S:
                        OnSaveChart();
                        break;
                    case Keys.O:
                        OnOpenChart();
                        break;
                    case Keys.Z:
                        OnUndo();
                        break;
                    case Keys.Y:
                        OnRedo();
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        OnDeleteSelected();
                        break;
                    case Keys.Escape:
                        OnEscape();
                        break;
                    case Keys.Space:
                        OnSpacePressed();
                        break;
                    case Keys.F1:
                        OnShowHelp();
                        break;
                    case Keys.F5:
                        OnRefreshData();
                        break;
                    case Keys.Left:
                        OnNavigateLeft();
                        break;
                    case Keys.Right:
                        OnNavigateRight();
                        break;
                    case Keys.Up:
                        OnNavigateUp();
                        break;
                    case Keys.Down:
                        OnNavigateDown();
                        break;
                }
            }
        }

        private void OnFormKeyUp(object? sender, KeyEventArgs e)
        {
            // Key release işlemleri
        }

        private void OnFormKeyPress(object? sender, KeyPressEventArgs e)
        {
            // Karakter girişi işlemleri
        }

        #endregion

        #region Plot Lifecycle Events

        /// <summary>
        /// Plot lifecycle event'lerini initialize eder
        /// </summary>
        private void InitializePlotLifecycleEvents()
        {
            // PlotManager event'leri dinlenebilir
        }

        private void OnPlotCreated(int plotIndex, string plotId)
        {
            System.Diagnostics.Debug.WriteLine($"Plot Created - Index: {plotIndex}, ID: {plotId}");
        }

        private void OnPlotDestroyed(int plotIndex, string plotId)
        {
            System.Diagnostics.Debug.WriteLine($"Plot Destroyed - Index: {plotIndex}, ID: {plotId}");
        }

        private void OnDataAdded(int plotIndex, PlotType plotType, object data)
        {
            System.Diagnostics.Debug.WriteLine($"Data Added - Plot: {plotIndex}, Type: {plotType}");
        }

        private void OnDataRemoved(int plotIndex, PlotType plotType)
        {
            System.Diagnostics.Debug.WriteLine($"Data Removed - Plot: {plotIndex}, Type: {plotType}");
        }

        private void OnDataUpdated(int plotIndex, PlotType plotType, object newData)
        {
            System.Diagnostics.Debug.WriteLine($"Data Updated - Plot: {plotIndex}, Type: {plotType}");
        }

        private void OnPlotRefresh(int plotIndex)
        {
            System.Diagnostics.Debug.WriteLine($"Plot Refresh - Index: {plotIndex}");
        }

        private void OnPlotResize(int plotIndex, Size newSize)
        {
            System.Diagnostics.Debug.WriteLine($"Plot Resize - Index: {plotIndex}, Size: {newSize}");
        }

        private void OnAxesChanged(int plotIndex, ScottPlot.AxisLimits newLimits)
        {
            System.Diagnostics.Debug.WriteLine($"Axes Changed - Plot: {plotIndex}, Limits: ({newLimits.Left:F2}, {newLimits.Right:F2}, {newLimits.Bottom:F2}, {newLimits.Top:F2})");
        }

        private void OnPlotZoom(int plotIndex, bool zoomIn, Point mousePosition)
        {
            System.Diagnostics.Debug.WriteLine($"Plot Zoom - Index: {plotIndex}, ZoomIn: {zoomIn}, Mouse: ({mousePosition.X}, {mousePosition.Y})");
        }

        #endregion

        #region Trading Specific Events

        /// <summary>
        /// Trading specific event'leri initialize eder
        /// </summary>
        private void InitializeTradingSpecificEvents()
        {
            // Trading chart'a özgü event'ler
        }

        private void OnCandlestickClick(ScottPlot.Coordinates coords, MouseButtons button)
        {
            System.Diagnostics.Debug.WriteLine($"Candlestick Click - Time: {coords.X:F2}, Price: {coords.Y:F2}, Button: {button}");
            
            // OHLC verisinden en yakın mum bulma
            if (ohlcData != null)
            {
                int candleIndex = (int)Math.Round(coords.X);
                if (candleIndex >= 0 && candleIndex < ohlcData.Length)
                {
                    var candle = ohlcData[candleIndex];
                    System.Diagnostics.Debug.WriteLine($"Candle {candleIndex}: O:{candle.Open:F2}, H:{candle.High:F2}, L:{candle.Low:F2}, C:{candle.Close:F2}");
                }
            }
        }

        private void OnIndicatorClick(int plotIndex, ScottPlot.Coordinates coords, MouseButtons button)
        {
            var plotId = plotManager.GetPlotId(plotIndex);
            System.Diagnostics.Debug.WriteLine($"Indicator Click - Plot: {plotId}, Coords: ({coords.X:F2}, {coords.Y:F2}), Button: {button}");
        }

        private void OnCrosshairMove(int plotIndex, ScottPlot.Coordinates coords)
        {
            // Eğer herhangi bir etkileşim aktifse crosshair güncelleme
            if (isDragging || isZooming || isPanning || isDividerDrag)
            {
                return; // Skip crosshair update during interactions
            }
            
            // Crosshair position güncellemesi (mevcut crosshair sistemine entegre)
            // System.Diagnostics.Debug.WriteLine($"Crosshair Move - Plot: {plotIndex}, Coords: ({coords.X:F2}, {coords.Y:F2})");
        }

        private void OnTimeframeChange(string oldTimeframe, string newTimeframe)
        {
            System.Diagnostics.Debug.WriteLine($"Timeframe Change - From: {oldTimeframe}, To: {newTimeframe}");
        }

        private void OnMarketDataUpdate(string symbol, double price, DateTime timestamp)
        {
            System.Diagnostics.Debug.WriteLine($"Market Data Update - Symbol: {symbol}, Price: {price:F2}, Time: {timestamp}");
        }

        private void OnOrderPlaced(string orderType, double price, double quantity)
        {
            System.Diagnostics.Debug.WriteLine($"Order Placed - Type: {orderType}, Price: {price:F2}, Quantity: {quantity}");
        }

        #endregion

        #region Hotkey Actions

        private void OnCopyData()
        {
            System.Diagnostics.Debug.WriteLine("Copy Data - Ctrl+C");
            // Seçili veriyi clipboard'a kopyala
        }

        private void OnPasteData()
        {
            System.Diagnostics.Debug.WriteLine("Paste Data - Ctrl+V");
            // Clipboard'dan veri yapıştır
        }

        private void OnSaveChart()
        {
            System.Diagnostics.Debug.WriteLine("Save Chart - Ctrl+S");
            // Chart'ı dosyaya kaydet
        }

        private void OnOpenChart()
        {
            System.Diagnostics.Debug.WriteLine("Open Chart - Ctrl+O");
            // Chart dosyası aç
        }

        private void OnUndo()
        {
            System.Diagnostics.Debug.WriteLine("Undo - Ctrl+Z");
            // Son işlemi geri al
        }

        private void OnRedo()
        {
            System.Diagnostics.Debug.WriteLine("Redo - Ctrl+Y");
            // İşlemi yeniden yap
        }

        private void OnDeleteSelected()
        {
            System.Diagnostics.Debug.WriteLine("Delete Selected - Delete Key");
            // Seçili öğeleri sil
        }

        private void OnEscape()
        {
            System.Diagnostics.Debug.WriteLine("Escape - ESC");
            // Mevcut işlemi iptal et
        }

        private void OnSpacePressed()
        {
            System.Diagnostics.Debug.WriteLine("Space Pressed - Space Key");
            // Özel işlem (örn: pause/resume)
        }

        private void OnShowHelp()
        {
            System.Diagnostics.Debug.WriteLine("Show Help - F1");
            // Yardım penceresini aç
        }

        private void OnRefreshData()
        {
            System.Diagnostics.Debug.WriteLine("Refresh Data - F5");
            // Veriyi yenile
        }

        private void OnNavigateLeft()
        {
            System.Diagnostics.Debug.WriteLine("Navigate Left - Left Arrow");
            // Sol navigasyon
        }

        private void OnNavigateRight()
        {
            System.Diagnostics.Debug.WriteLine("Navigate Right - Right Arrow");
            // Sağ navigasyon
        }

        private void OnNavigateUp()
        {
            System.Diagnostics.Debug.WriteLine("Navigate Up - Up Arrow");
            // Yukarı navigasyon
        }

        private void OnNavigateDown()
        {
            System.Diagnostics.Debug.WriteLine("Navigate Down - Down Arrow");
            // Aşağı navigasyon
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Pixel pozisyonundan plot index'ini bulur
        /// </summary>
        private int GetPlotIndexAtPosition(int x, int y)
        {
            if (tradingChart?.Plot?.Multiplot == null) return -1;

            var plots = tradingChart.Plot.Multiplot.GetPlots();
            for (int i = 0; i < plots.Length; i++)
            {
                var plotRect = plots[i].RenderManager.LastRender.DataRect;
                if (x >= plotRect.Left && x <= plotRect.Right && y >= plotRect.Top && y <= plotRect.Bottom)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Pixel pozisyonunu plot koordinatlarına çevirir
        /// </summary>
        private ScottPlot.Coordinates GetCoordinatesAtPosition(int x, int y, int plotIndex)
        {
            if (plotIndex < 0 || tradingChart?.Plot?.Multiplot == null) 
                return new ScottPlot.Coordinates(0, 0);

            var plots = tradingChart.Plot.Multiplot.GetPlots();
            if (plotIndex >= plots.Length) 
                return new ScottPlot.Coordinates(0, 0);

            return plots[plotIndex].GetCoordinates(new ScottPlot.Pixel(x, y));
        }

        /// <summary>
        /// Belirtilen koordinattaki OHLC verisini bulur
        /// </summary>
        private ScottPlot.OHLC? GetOHLCAtCoordinate(double x)
        {
            if (ohlcData == null) return null;

            int index = (int)Math.Round(x);
            if (index >= 0 && index < ohlcData.Length)
                return ohlcData[index];

            return null;
        }

        /// <summary>
        /// Plot event'lerini temizler
        /// </summary>
        private void CleanupPlotEvents()
        {
            if (tradingChart?.Plot != null)
            {
                tradingChart.Plot.MouseClick -= OnPlotMouseClick;
                tradingChart.Plot.MouseDoubleClick -= OnPlotMouseDoubleClick;
                tradingChart.Plot.MouseDown -= OnPlotMouseDown;
                tradingChart.Plot.MouseUp -= OnPlotMouseUp;
                tradingChart.Plot.MouseMove -= OnPlotMouseMove;
                tradingChart.Plot.MouseEnter -= OnPlotMouseEnter;
                tradingChart.Plot.MouseLeave -= OnPlotMouseLeave;
                tradingChart.Plot.MouseWheel -= OnPlotMouseWheel;
            }
        }

        #endregion

        #endregion

        private void button1979_Click(object sender, EventArgs e)
        {
            // Yeni bir form oluştur ve göster (Draggable Multiplot Demo)
            var multiplotForm = new MultiplotDraggableForm();
            multiplotForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Generate 2M OHLC points
            int pointCount = 2_000_000 * 0 + 2_000 * 1;

            statusLabel.Text = $"Generating {pointCount:N0} OHLC bars...";
            Application.DoEvents(); // UI güncellenmesi için

            ohlcData = DataGenerator.GenerateOHLCs(pointCount);

            statusLabel.Text = $"Generated {ohlcData.Length:N0} OHLC bars | Calculating indicators...";
            Application.DoEvents();

            // Open, Close, High, Low fiyatlarını al
            var openPrices = ohlcData.Select(x => x.Open).ToArray();
            var closePrices = ohlcData.Select(x => x.Close).ToArray();
            var highPrices = ohlcData.Select(x => x.High).ToArray();
            var lowPrices = ohlcData.Select(x => x.Low).ToArray();

            // Volume üret
            volumeData = DataGenerator.GenerateVolume(pointCount, 1000000, 10000000);

            // SMA hesapla
            sma100 = DataGenerator.CalculateSMA(closePrices, 100);
            sma200 = DataGenerator.CalculateSMA(closePrices, 200);

            // RSI hesapla
            rsi = DataGenerator.CalculateRSI(closePrices, 14);

            // MACD hesapla
            (macdLine, signalLine, histogram) = DataGenerator.CalculateMACD(closePrices, 12, 26, 9);

            // Stochastic hesapla
            (stochK, stochD) = DataGenerator.CalculateStochastic(highPrices, lowPrices, closePrices, 14, 3, 3);

            statusLabel.Text = $"Calculating trading signals...";
            Application.DoEvents();

            // Trading sinyalleri üret (SMA100 ve SMA200 kesişimlerinden, %5 kar al, %3 zarar kes)
            (signals, entryPrices, exitPrices) = DataGenerator.CalculateTradingSignals(sma100, sma200, 5, -3);

            // PnL hesapla (3 ayrı değer: Anlık, Gerçekleşen, Kümülatif)
            (unrealizedPnL, realizedPnL, cumulativePnL) = DataGenerator.CalculatePnL(signals, entryPrices, exitPrices, closePrices);

            // Balance hesapla (Başlangıç sermayesi: 100,000) - Kümülatif PnL kullan
            balance = DataGenerator.CalculateBalance(100000, cumulativePnL);

            // Sonuçları göster
            int buySignals = signals.Count(s => s == 1);
            int sellSignals = signals.Count(s => s == -1);
            int flatSignals = 0;
            double finalBalance = balance[balance.Length - 1];
            double totalPnL = cumulativePnL[cumulativePnL.Length - 1];
            double lastTradeRealizedPnL = realizedPnL[realizedPnL.Length - 1];
            double currentUnrealizedPnL = unrealizedPnL[unrealizedPnL.Length - 1];
            double returnPercent = (totalPnL / 100000) * 100;

            statusLabel.Text = $"Completed! Bars: {ohlcData.Length:N0} | Signals: {buySignals} buy, {sellSignals} sell, {flatSignals} flat | " +
                              $"Initial: $100,000 | Final: ${finalBalance:N2} | " +
                              $"Total PnL: ${totalPnL:N2} ({returnPercent:F2}%) | " +
                              $"Last Trade: ${lastTradeRealizedPnL:N2} | Open Position: ${currentUnrealizedPnL:N2}";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Önce veri üret
            button1_Click(sender, e);

            plotManager.SetTradingChart(tradingChart);

            plotManager.RemoveAllPlots();

            // Y ekseni tercihi: true = sağ eksen kullan, false = sol eksen kullan
            bool useRightYAxis = false;

            // Cross-hair tercihi: true = tüm plotlarda sync, false = sadece mouse'un üzerindeki plot'ta
            useCrossHairAllPlots = true;

            // ===========================================
            // PLOT 0: Candlestick (Price Chart)  
            // ===========================================
            var plot0 = plotManager.GetPlot(0);
            plotManager.SetPlotId(plot0, 0);
            plotManager.AddPlotData(plot0, PlotType.Candlestick, ohlcData, 0);
            plot0.Title("Price Chart (Candlestick)");
            plot0.YLabel("Price");
            plot0.Axes.AutoScale();
/*
            // ===========================================
            // PLOT 1: Volume
            // ===========================================
            var plot1 = plotManager.AddPlot();
            plotManager.SetPlotId(plot1, 1);
            var volumePlottable = plotManager.AddPlotData(plot1, PlotType.Volume, volumeData, 0);

            // Cast edip kullanın
            if (volumePlottable is BarPlot volumeBars)
            {
                for (int i = 0; i < volumeBars.Bars.Count && i < ohlcData.Length; i++)
                {
                    // Close > Open ise yeşil, değilse kırmızı
                    if (ohlcData[i].Close >= ohlcData[i].Open)
                    {
                        volumeBars.Bars[i].FillColor = ScottPlot.Colors.LimeGreen.WithAlpha(0.7);
                    }
                    else
                    {
                        volumeBars.Bars[i].FillColor = ScottPlot.Colors.Red.WithAlpha(0.7);
                    }
                }
            }
            plot1.Title("Volume");
            plot1.YLabel("Volume");
            plot1.Axes.AutoScale();
*/

            // ===========================================
            // PLOT 2: Moving Averages
            // ===========================================
            var plot2 = plotManager.AddPlot();
            plotManager.SetPlotId(plot2, 2);
            var sma100Line = plotManager.AddPlotData(plot2, PlotType.Line, sma100, 0);
            var sma200Line = plotManager.AddPlotData(plot2, PlotType.Line, sma200, 1);

            // Renkleri ve özellikleri ayarla
            if (sma100Line is Signal sma100Signal)
            {
                sma100Signal.Color = ScottPlot.Colors.Blue;
                sma100Signal.LineWidth = 2;
                sma100Signal.LegendText = "SMA(100)";
            }

            if (sma200Line is Signal sma200Signal)
            {
                sma200Signal.Color = ScottPlot.Colors.Orange;
                sma200Signal.LineWidth = 2;
                sma200Signal.LegendText = "SMA(200)";
            }

            // Plot 2 ayarları
            plot2.Title("Moving Averages");
            plot2.YLabel("Price");
            plot2.Axes.AutoScale();
            plot2.ShowLegend();

            // ===========================================
            // PLOT 3: RSI (Relative Strength Index)
            // ===========================================
            var plot3 = plotManager.AddPlot();
            plotManager.SetPlotId(plot3, 3);
            // RSI çizgisini PlotManager ile ekle
            var rsiLine = plotManager.AddPlotData(plot3, PlotType.Line, rsi, 0);

            // RSI referans çizgilerini PlotManager ile ekle
            var hline30 = plotManager.AddPlotData(plot3, PlotType.HorizontalLine, 30.0, 1);
            var hline70 = plotManager.AddPlotData(plot3, PlotType.HorizontalLine, 70.0, 2);

            // RSI çizgisi özelliklerini ayarla
            if (rsiLine is Signal rsiSignal)
            {
                rsiSignal.Color = ScottPlot.Colors.Blue;
                rsiSignal.LineWidth = 2;
                rsiSignal.LegendText = "RSI";
            }

            // Referans çizgileri özelliklerini ayarla
            if (hline30 is HorizontalLine h30)
            {
                h30.LineColor = ScottPlot.Colors.Red;
                h30.LineWidth = 1;
                h30.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            if (hline70 is HorizontalLine h70)
            {
                h70.LineColor = ScottPlot.Colors.Red;
                h70.LineWidth = 1;
                h70.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Plot 3 ayarları
            plot3.Title("RSI Indicator");
            plot3.YLabel("RSI");
            plot3.Axes.SetLimitsY(0, 100);
            plot3.Axes.AutoScaleX();
            plot3.ShowLegend();


            // ===========================================
            // PLOT 4: MACD (Moving Average Convergence Divergence)
            // ===========================================
            var plot4 = plotManager.AddPlot();
            plotManager.SetPlotId(plot4, 4);

            // PlotManager ile MACD verilerini ekle
            var _macdLine = plotManager.AddPlotData(plot4, PlotType.Line, macdLine, 0);
            var _signalLine = plotManager.AddPlotData(plot4, PlotType.Line, signalLine, 1);
            var _histogram = plotManager.AddPlotData(plot4, PlotType.Bar, histogram, 2);
            var zeroLine = plotManager.AddPlotData(plot4, PlotType.HorizontalLine, 0.0, 3);

            // MACD line özelliklerini ayarla
            if (_macdLine is Signal macdSignal)
            {
                macdSignal.Color = ScottPlot.Colors.Blue;
                macdSignal.LineWidth = 2;
                macdSignal.LegendText = "MACD";
            }

            // Signal line özelliklerini ayarla
            if (_signalLine is Signal signalSignal)
            {
                signalSignal.Color = ScottPlot.Colors.Red;
                signalSignal.LineWidth = 2;
                signalSignal.LegendText = "Signal";
            }

            // Histogram özelliklerini ayarla
            if (_histogram is BarPlot histogramBars)
            {
                for (int i = 0; i < histogramBars.Bars.Count; i++)
                {
                    histogramBars.Bars[i].FillColor = histogram[i] >= 0
                        ? ScottPlot.Colors.Green.WithAlpha(0.5)
                        : ScottPlot.Colors.Red.WithAlpha(0.5);
                }
            }

            // Zero line özelliklerini ayarla
            if (zeroLine is HorizontalLine zLine)
            {
                zLine.LineColor = ScottPlot.Colors.Gray;
                zLine.LineWidth = 1;
                zLine.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Plot 4 ayarları
            plot4.Title("MACD");
            plot4.YLabel("MACD");
            plot4.Axes.AutoScale();
            plot4.ShowLegend();


            // ===========================================
            // PLOT 5: Stochastic Oscillator
            // ===========================================
            var plot5 = plotManager.AddPlot();
            plotManager.SetPlotId(plot5, 5);

            // PlotManager ile Stochastic verilerini ekle
            var kLine = plotManager.AddPlotData(plot5, PlotType.Line, stochK, 0);
            var dLine = plotManager.AddPlotData(plot5, PlotType.Line, stochD, 1);
            var stochLine80 = plotManager.AddPlotData(plot5, PlotType.HorizontalLine, 80.0, 2);
            var stochLine20 = plotManager.AddPlotData(plot5, PlotType.HorizontalLine, 20.0, 3);

            // %K line özelliklerini ayarla
            if (kLine is Signal kSignal)
            {
                kSignal.Color = ScottPlot.Colors.Blue;
                kSignal.LineWidth = 2;
                kSignal.LegendText = "%K";
            }

            // %D line özelliklerini ayarla
            if (dLine is Signal dSignal)
            {
                dSignal.Color = ScottPlot.Colors.Red;
                dSignal.LineWidth = 2;
                dSignal.LegendText = "%D";
            }

            // Overbought line (80) özelliklerini ayarla
            if (stochLine80 is HorizontalLine line80)
            {
                line80.LineColor = ScottPlot.Colors.Red;
                line80.LineWidth = 1;
                line80.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Oversold line (20) özelliklerini ayarla
            if (stochLine20 is HorizontalLine line20)
            {
                line20.LineColor = ScottPlot.Colors.Red;
                line20.LineWidth = 1;
                line20.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Plot 5 ayarları
            plot5.Title("Stochastic Oscillator");
            plot5.YLabel("Stochastic");
            plot5.Axes.SetLimitsY(0, 100);
            plot5.Axes.AutoScaleX();
            plot5.ShowLegend();

            // ===========================================
            // PLOT 6: Trading Signals (Buy/Sell)
            // ===========================================
            var plot6 = plotManager.AddPlot();
            plotManager.SetPlotId(plot6, 6);

            // Buy ve sell signal verilerini hazırla
            List<double> buySignalX = new List<double>();
            List<double> buySignalY = new List<double>();
            List<double> sellSignalX = new List<double>();
            List<double> sellSignalY = new List<double>();

            for (int i = 0; i < signals.Length; i++)
            {
                if (signals[i] == 1) // Buy signal
                {
                    buySignalX.Add(i);
                    buySignalY.Add(1); // Signal = 1 (Buy)
                }
                else if (signals[i] == -1) // Sell signal
                {
                    sellSignalX.Add(i);
                    sellSignalY.Add(-1); // Signal = -1 (Sell)
                }
            }

            // Trading signals PlotManager ile ekle
            var buyScatter = buySignalX.Count > 0 ? plotManager.AddPlotData(plot6, PlotType.Scatter, (buySignalX.ToArray(), buySignalY.ToArray()), 0) : null;
            var sellScatter = sellSignalX.Count > 0 ? plotManager.AddPlotData(plot6, PlotType.Scatter, (sellSignalX.ToArray(), sellSignalY.ToArray()), 1) : null;

            // Zero line PlotManager ile ekle
            var signalZeroLine = plotManager.AddPlotData(plot6, PlotType.HorizontalLine, 0.0, 2);

            // Buy signals özelliklerini ayarla
            if (buyScatter is ScottPlot.Plottables.Scatter buyScatterPlot)
            {
                buyScatterPlot.Color = ScottPlot.Colors.Green;
                buyScatterPlot.MarkerSize = 10;
                buyScatterPlot.MarkerShape = ScottPlot.MarkerShape.FilledTriangleUp;
                buyScatterPlot.LegendText = "Buy";
            }

            // Sell signals özelliklerini ayarla
            if (sellScatter is ScottPlot.Plottables.Scatter sellScatterPlot)
            {
                sellScatterPlot.Color = ScottPlot.Colors.Red;
                sellScatterPlot.MarkerSize = 10;
                sellScatterPlot.MarkerShape = ScottPlot.MarkerShape.FilledTriangleDown;
                sellScatterPlot.LegendText = "Sell";
            }

            // Zero line özelliklerini ayarla
            if (signalZeroLine is HorizontalLine signalZLine)
            {
                signalZLine.LineColor = ScottPlot.Colors.Gray;
                signalZLine.LineWidth = 1;
                signalZLine.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Plot 6 ayarları
            plot6.Title("Trading Signals");
            plot6.YLabel("Signal");
            plot6.Axes.SetLimitsY(-1.5, 1.5);
            plot6.Axes.AutoScaleX();
            plot6.ShowLegend();

            // ===========================================
            // PLOT 7: PnL (Profit and Loss)
            // ===========================================
            var plot7 = plotManager.AddPlot();
            plotManager.SetPlotId(plot7, 7);

            // PnL çizgisini PlotManager ile ekle
            var pnlLine = plotManager.AddPlotData(plot7, PlotType.Line, cumulativePnL, 0);  //unrealizedPnL, realizedPnL, cumulativePnL

            // Zero line PlotManager ile ekle
            var pnlZeroLine = plotManager.AddPlotData(plot7, PlotType.HorizontalLine, 0.0, 1);

            // PnL çizgisi özelliklerini ayarla
            if (pnlLine is Signal pnlSignal)
            {
                pnlSignal.Color = ScottPlot.Colors.Blue;
                pnlSignal.LineWidth = 2;
                pnlSignal.LegendText = "Cumulative PnL";
            }

            // Zero line özelliklerini ayarla
            if (pnlZeroLine is HorizontalLine pnlZLine)
            {
                pnlZLine.LineColor = ScottPlot.Colors.Gray;
                pnlZLine.LineWidth = 1;
                pnlZLine.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Plot 7 ayarları
            plot7.Title("Profit & Loss (PnL)");
            plot7.YLabel("PnL");
            plot7.Axes.AutoScale();
            plot7.ShowLegend();



            // ===========================================
            // PLOT 8: Balance (Account Balance)
            // ===========================================
            var plot8 = plotManager.AddPlot();
            plotManager.SetPlotId(plot8, 8);

            // Balance çizgisini PlotManager ile ekle
            var balanceLine = plotManager.AddPlotData(plot8, PlotType.Line, balance, 0);

            // Initial balance reference line PlotManager ile ekle
            double initialBalance = 100000;
            var initialBalanceLine = plotManager.AddPlotData(plot8, PlotType.HorizontalLine, initialBalance, 1);

            // Balance çizgisi özelliklerini ayarla
            if (balanceLine is Signal balanceSignal)
            {
                balanceSignal.Color = ScottPlot.Colors.DarkGreen;
                balanceSignal.LineWidth = 3;
                balanceSignal.LegendText = "Account Balance";
            }

            // Initial balance line özelliklerini ayarla
            if (initialBalanceLine is HorizontalLine initBalLine)
            {
                initBalLine.LineColor = ScottPlot.Colors.Gray;
                initBalLine.LineWidth = 1;
                initBalLine.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // Plot 8 ayarları
            plot8.Title("Account Balance");
            plot8.YLabel("Balance ($)");
            plot8.XLabel("Time (Bar Index)");
            plot8.Axes.AutoScale();
            plot8.ShowLegend();

            // ===========================================
            // Plot yüksekliklerini setle
            // ===========================================
            // ESKI KOD - ARTIK GEREKSIZ (DraggableRows kullanılıyor)

            /*for (int i = 0; i < plotManager.Count; i++)
            {
                if (plotManager.GetPlotId(i) == "0")
                {
                    plotManager.SetPlotHeightOnly(i, 400);
                }
                else // Diğer plotlar (indikatörler)
                {
                    plotManager.SetPlotHeightOnly(i, 200);
                }
            }*/

            // ===================================================================
            // DraggableRows Layout ile plot yüksekliklerini ayarla
            // ===================================================================
            // DraggableRows layout oluştur
            customLayout = new ScottPlot.MultiplotLayouts.DraggableRows();
            tradingChart.Plot.Multiplot.Layout = customLayout;

            // Her plot için yükseklikleri belirle
            var plotHeights = new List<float>();
            for (int i = 0; i < plotManager.Count; i++)
            {
                if (plotManager.GetPlotId(i) == "0")
                {
                    plotHeights.Add(400); // Candlestick chart için daha yüksek
                }
                else
                {
                    plotHeights.Add(300); // Diğer indikatörler için standart yükseklik
                }
            }

            // Yükseklikleri uygula
            customLayout.SetHeights(plotHeights.ToArray());

            // ===================================================================
            // Tüm plotların axes genişliklerini sabitle (alignment için, flag'e göre)
            // ===================================================================
            foreach (var plot in tradingChart.Plot.Multiplot.GetPlots())
            {
                if (useRightYAxis)
                {
                    plot.Axes.Left.LockSize(50);   // Sol ekseni küçük tut
                    plot.Axes.Right.LockSize(80);  // Sağ ekseni geniş tut (tick'ler ve label'lar için)
                }
                else
                {
                    plot.Axes.Left.LockSize(80);   // Sol ekseni geniş tut (tick'ler ve label'lar için)
                    plot.Axes.Right.LockSize(50);  // Sağ ekseni küçük tut
                }
            }

            // Toplam yüksekliği hesapla (scroll için gerekli)
            int totalHeight = (int)plotHeights.Sum();

            // FormsPlot'un toplam yüksekliğini ayarla
            tradingChart.Plot.Dock = DockStyle.None;
            tradingChart.Plot.Height = totalHeight;
            tradingChart.Plot.Location = new Point(0, 0);

            // Genişliği parent panel'e göre ayarla (panelCenter'ın gerçek genişliği)
            // Reflection ile panelCenter'ı al
            var panelCenterProp = tradingChart.GetType().GetProperty("panelCenter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Panel? panelCenterObj = panelCenterProp?.GetValue(tradingChart) as Panel;

            int adjustedWidth = 0;

            if (panelCenterObj != null)
            {
                // panelCenter'ın gerçek kullanılabilir alanını kullan
                adjustedWidth = panelCenterObj.ClientSize.Width;

                // LeftPanel visible ise ekstra düzeltme yap
                if (tradingChart.LeftPanel.Visible)
                {
                    adjustedWidth -= (tradingChart.LeftPanel.Width - 1); // LeftPanel width + tolerans
                }

                // RightPanel visible ise ekstra düzeltme yap
                if (tradingChart.RightPanel.Visible)
                {
                    adjustedWidth -= (tradingChart.RightPanel.Width - 1); // RightPanel width + tolerans
                }

                // TopPanel visible ise ekstra düzeltme
                if (tradingChart.TopPanel.Visible)
                {
                    //adjustedWidth -= 5; // Küçük tolerans
                }

                // Scrollbar ve plot margin'leri için ekstra boşluk (axes lock size kullanıldığı için daha az gerekli)
                adjustedWidth -= 20;

                tradingChart.Plot.Width = adjustedWidth;

                // Debug: Genişlik bilgilerini yazdır
                System.Diagnostics.Debug.WriteLine($"PanelCenter Width: {panelCenterObj.ClientSize.Width}, Adjusted Width: {adjustedWidth}, FormsPlot Width: {tradingChart.Plot.Width}");
            }
            else
            {
                // Fallback: panelCenter bulunamadı, tradingChart.ClientSize kullan
                adjustedWidth = tradingChart.ClientSize.Width;

                // LeftPanel visible ise çıkar
                if (tradingChart.LeftPanel.Visible)
                {
                    adjustedWidth -= tradingChart.LeftPanel.Width;
                }

                // RightPanel visible ise çıkar
                if (tradingChart.RightPanel.Visible)
                {
                    adjustedWidth -= tradingChart.RightPanel.Width;
                }

                // TopPanel visible ise (yükseklik olduğu için genişliği etkilemez ama tutarlılık için)
                if (tradingChart.TopPanel.Visible)
                {
                    // TopPanel genişliği etkilemez
                }

                // BottomPanel visible ise (yükseklik olduğu için genişliği etkilemez)
                if (tradingChart.BottomPanel.Visible)
                {
                    // BottomPanel genişliği etkilemez
                }

                // Scrollbar ve plot margin'leri için ekstra boşluk
                adjustedWidth -= 20;

                tradingChart.Plot.Width = adjustedWidth;

                // Debug: Fallback kullanıldı
                System.Diagnostics.Debug.WriteLine($"PanelCenter null! TradingChart Width: {tradingChart.ClientSize.Width}, Adjusted Width: {adjustedWidth}");
            }

            // ===================================================================
            // Mouse event'leri - Plotlar arasındaki divider'ları sürüklenebilir yap
            // ===================================================================
            tradingChart.Plot.MouseDown += TradingChartPlotMouseDown;

            tradingChart.Plot.MouseUp += TradingChartPlotMouseUp;

            tradingChart.Plot.MouseMove += TradingChartPlotMouseMove;

            // ===================================================================
            // Mouse Wheel - Zoom (scrollbar'ı etkilemesin)
            // ===================================================================
            tradingChart.Plot.MouseWheel += TradingChartPlotMouseWheel;

            // ===================================================================
            // TradingChart resize event'i ekle - form büyüdüğünde FormsPlot genişliğini güncelle
            // ===================================================================
            tradingChart.Resize += TradingChartResize;


            // ===================================================================
            // Grid ve X-axis sharing ayarları
            // ===================================================================
            // Tüm plotları al
            allPlots = tradingChart.Plot.Multiplot.GetPlots();

            if (allPlots.Length > 0)
            {
                // En alttaki plot (plot8 - Balance)
                var bottomPlot = allPlots[allPlots.Length - 1];

                // Tüm plotların grid'lerini en alttaki plot'un X ekseninden tick'leri kullanacak şekilde ayarla
                foreach (var plot in allPlots)
                {
                    if (plot != bottomPlot)
                    {
                        plot.Grid.XAxis = bottomPlot.Axes.Bottom;
                    }
                }

                // Tüm plotların grid'lerini Y ekseninden tick'leri kullanacak şekilde ayarla (flag'e göre)
                foreach (var plot in allPlots)
                {
                    plot.Grid.YAxis = useRightYAxis ? plot.Axes.Right : plot.Axes.Left;
                }

                // Tüm plotların X eksenlerini paylaştır (link et) - zoom/pan senkronize olsun
                tradingChart.Plot.Multiplot.SharedAxes.ShareX(allPlots);
            }
            // ===================================================================

            // ===================================================================
            // Plottable'ları Y eksenine bağla (flag'e göre)
            // ===================================================================
            if (useRightYAxis)
            {
                foreach (var plot in allPlots)
                {
                    // Plot içindeki tüm plottable'ları Right Y eksenine bağla
                    foreach (var plottable in plot.GetPlottables())
                    {
                        plottable.Axes.YAxis = plot.Axes.Right;
                    }
                }
            }
            // (useRightYAxis = false ise, plottable'lar zaten default olarak Left'te)
            // ===================================================================

            // ===================================================================
            // Y ekseni label'larını ve tick'lerini flag'e göre düzenle
            // ===================================================================
            if (useRightYAxis)
            {
                foreach (var plot in allPlots)
                {
                    // Sol eksendeki label'ı sağ eksene taşı
                    string leftLabel = plot.Axes.Left.Label.Text;
                    if (!string.IsNullOrEmpty(leftLabel))
                    {
                        plot.Axes.Right.Label.Text = leftLabel;
                        plot.Axes.Left.Label.Text = "";  // Sol ekseni temizle
                    }

                    // Sol eksendeki tick label'larını gizle
                    plot.Axes.Left.TickLabelStyle.IsVisible = false;
                    plot.Axes.Right.TickLabelStyle.IsVisible = true;
                }
            }
            else
            {
                foreach (var plot in allPlots)
                {
                    // Sağ eksendeki label'ı sol eksene taşı (eğer varsa)
                    string rightLabel = plot.Axes.Right.Label.Text;
                    if (!string.IsNullOrEmpty(rightLabel))
                    {
                        plot.Axes.Left.Label.Text = rightLabel;
                        plot.Axes.Right.Label.Text = "";  // Sağ ekseni temizle
                    }

                    // Sağ eksendeki tick label'larını gizle
                    plot.Axes.Left.TickLabelStyle.IsVisible = true;
                    plot.Axes.Right.TickLabelStyle.IsVisible = false;
                }
            }
            // ===================================================================

            // ===================================================================
            // Cross-hair - Tüm plotlarda mouse koordinatlarını göster
            // ===================================================================

            // Her plot için cross-hair oluştur
            crosshairs.Clear(); // Önceki crosshair'leri temizle

            foreach (var plot in allPlots)
            {
                var crosshair = plot.Add.Crosshair(0, 0);
                crosshair.IsVisible = false;
                crosshair.LineColor = ScottPlot.Colors.Red.WithAlpha(0.7);
                crosshair.LineWidth = 1;
                crosshair.LinePattern = ScottPlot.LinePattern.Solid;
                crosshairs.Add(crosshair);
            }

            // Mouse move event - Crosshair'i güncelle (flag'e göre)
            // Artık TradingChartPlotMouseMove metodu hem divider hem crosshair işlerini yapıyor

            // ===================================================================
            // Mouse leave event - Crosshair'leri gizle
            // ===================================================================
            tradingChart.Plot.MouseLeave += TradingChartPlotMouseLeave;
            // ===================================================================

            // ===================================================================

            // ===========================================
            // Plot yüksekliklerini ayarla
            // ===========================================
            // ESKI KOD - ARTIK GEREKSIZ (DraggableRows kullanılıyor)
            /*
            for (int i = 0; i < plotManager.Count; i++)
            {
                if (plotManager.GetPlotId(i) == "0")
                {
                    plotManager.SetPlotHeightOnly(i, 200);
                }
                else // Diğer plotlar (indikatörler)
                {
                    plotManager.SetPlotHeightOnly(i, 100);
                }
            }

            // ===========================================
            //
            // ===========================================
            int totalHeight = 0;
            for (int i = 0; i < plotManager.Count; i++)
            {
                totalHeight += plotManager.GetPlotHeight(i);
            }
            */




            // ===========================================
            // Finalize Layout and Refresh
            // ===========================================
            plotManager.FinalizeLayout();

            plotManager.Refresh();


        }
    }
}

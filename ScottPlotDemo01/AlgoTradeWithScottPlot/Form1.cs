using ScottPlot;

namespace AlgoTradeWithScottPlot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            // Initialize any additional UI components if needed
            InitializeUIComponents();
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
    }
}

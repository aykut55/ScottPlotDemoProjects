# Proje Yeniden Yazım Planı: AlgoTradeWithScottPlot

Bu belge, mevcut AlgoTradeWithScottPlot projesinin daha modern, esnek, test edilebilir ve sürdürülebilir bir mimariye dönüştürülmesi için detaylı bir plan sunmaktadır. Mevcut "code-behind" ve monolitik yapı, sorumlulukların ayrılması (Separation of Concerns) prensibine uygun olarak yeniden düzenlenecektir.

---

### **A. Genel Mimari ve Yeniden Yazım Stratejisi**

Mevcut monolitik yapıyı, sorumlulukları net bir şekilde ayrılmış katmanlara böleceğiz. Bu, "Separation of Concerns" (Sorumlulukların Ayrılması) ilkesine dayanır.

1.  **Veri Katmanı (Data Layer):** Sadece veri üretmek, indikatörleri hesaplamak ve bu verileri sağlamakla sorumlu olacak. UI'dan tamamen habersiz olacak.
2.  **Grafik Yönetim Katmanı (Chart Management Layer):** Veri katmanından gelen ham verileri dinleyecek, bu verileri grafiklerin anlayacağı formatlara dönüştürecek ve hangi grafiğin ne göstereceğini yönetecek.
3.  **UI Katmanı (User Interface Layer):** Sadece veriyi görselleştirmek ve kullanıcı etkileşimlerini ilgili servislere iletmekle sorumlu olacak. Kendi içinde iş mantığı barındırmayacak.

Bu katmanlar arasındaki iletişim, C# `event`'leri aracılığıyla sağlanacak. Bu sayede, örneğin veri üretildiğinde, veri katmanı sadece "Yeni veri var!" diye bir olay yayınlayacak. Kimin bu olayı dinlediğini bilmek zorunda kalmayacak.

---

### **B. Olay (Event) Mimarisi: Senkronizasyonun Kalbi**

Bileşenlerin birbirleriyle konuşması için iki ana olay tanımlayacağız.

1.  **`OnNewDataPointGenerated` (Yeni Veri Noktası Üretildi Olayı)**
    *   **Yayınlayan:** `TradingDataService` (Veri Katmanı).
    *   **Tetiklenme:** Her yeni `OHLC` barı (mum) üretildiğinde (örneğin bir zamanlayıcı ile).
    *   **İçerik (Payload):** Yeni üretilen `OHLCBar` nesnesini taşır.
    *   **Amacı:** Sisteme yeni bir veri parçasının geldiğini duyurmak. Grafik yöneticisi bu olayı dinleyerek kendi verilerini günceller.
    *   **Detaylı Anlatım:** `TradingDataService` içindeki bir `System.Threading.Timer`, periyodik olarak `GenerateNextBar()` metodunu çağırır. Bu metot yeni bir `OHLCBar` nesnesi oluşturur ve `OnNewDataPointGenerated?.Invoke(this, new OHLCEventArgs(newBar));` kodunu çalıştırarak olayı tetikler.

2.  **`OnChartNeedsUpdate` (Grafik Güncellenmeli Olayı)**
    *   **Yayınlayan:** `ChartManagerService` (Grafik Yönetim Katmanı).
    *   **Tetiklenme:** `OnNewDataPointGenerated` olayını dinledikten ve gelen ham veriyi işleyip (örn. yeni indikatör değeri hesaplayıp) çizime hazır hale getirdikten sonra.
    *   **İçerik (Payload):** Hangi grafiğin (`PlotId`) ve hangi veri setleriyle (`double[]` dizileri) güncelleneceğini içeren bir veri transfer nesnesi (`PlotUpdateData`) taşır.
    *   **Amacı:** UI katmanına, "Belirtilen grafiği şu yeni verilerle yeniden çiz" komutunu vermek.
    *   **Detaylı Anlatım:** `ChartManagerService`, `OnNewDataPointGenerated` olayını yakaladığında, gelen `OHLCBar`'ı kendi iç listesine ekler. Ardından, bu yeni veriyle ilgili tüm indikatörleri (RSI, MACD vb.) yeniden hesaplar. Hesaplama bitince, bir `PlotUpdateData` nesnesi oluşturur, içine grafik ID'sini ve ScottPlot'un `Add` metotlarının istediği `double[]` gibi dizileri koyar ve `OnChartNeedsUpdate?.Invoke(this, updateData);` koduyla olayı tetikler.

---

### **C. Detaylı Bileşen Planı**

#### **1. Veri Katmanı (Data Layer) - `src/services/TradingDataService.cs`**

Bu sınıf, statik `DataGenerator`'ın yerini alacak ve bir servis olarak çalışacak.

*   **Sorumluluklar:**
    *   Tarihsel ve/veya anlık (simüle edilmiş) OHLC verileri üretmek.
    *   Ham fiyat verilerinden teknik indikatörleri hesaplamak.
    *   Yeni veri üretildiğinde `OnNewDataPointGenerated` olayını yayınlamak.

*   **Yeni Veri Yapıları (`src/models/` altında):**
    *   `OHLCBar.cs`: ScottPlot'un `OHLC`'si yerine kendi modelimiz. Daha esnek olur.
        ```csharp
        public struct OHLCBar
        {
            public DateTime Timestamp { get; set; }
            public double Open { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Close { get; set; }
            public double Volume { get; set; }
        }
        ```
    *   `OHLCEventArgs.cs`: Olayla birlikte veri taşımak için.
        ```csharp
        public class OHLCEventArgs : EventArgs
        {
            public OHLCBar NewBar { get; }
            public OHLCEventArgs(OHLCBar newBar) { NewBar = newBar; }
        }
        ```

*   **Sınıf Yapısı (`TradingDataService.cs`):**
    ```csharp
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace AlgoTradeWithScottPlot.Services
    {
        public class TradingDataService
        {
            private List<OHLCBar> _data = new List<OHLCBar>();
            private System.Threading.Timer _timer;
            private Random _random = new Random();
            private double _lastPrice = 100.0;

            // Olay tanımı
            public event EventHandler<OHLCEventArgs> OnNewDataPointGenerated;

            public void StartRealTimeGeneration(int intervalMilliseconds = 1000)
            {
                _timer = new System.Threading.Timer(GenerateNextBar, null, 0, intervalMilliseconds);
            }

            public void StopGeneration()
            {
                _timer?.Change(Timeout.Infinite, 0);
            }

            // "Add" metodu yerine geçen veri üretim metodu
            private void GenerateNextBar(object state)
            {
                // Yeni bir OHLCBar oluşturma mantığı
                double open = _lastPrice;
                double close = open + (_random.NextDouble() - 0.5) * 2;
                double high = Math.Max(open, close) + _random.NextDouble();
                double low = Math.Min(open, close) - _random.NextDouble();
                var newBar = new OHLCBar
                {
                    Timestamp = DateTime.Now,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = 1000 + _random.NextDouble() * 500
                };
                _lastPrice = close;
                _data.Add(newBar);

                // Olayı tetikle!
                OnNewDataPointGenerated?.Invoke(this, new OHLCEventArgs(newBar));
            }

            // Statik indikatör hesaplama metotları (DataGenerator'dan taşınacak)
            // Örnek:
            public static double[] CalculateSMA(List<OHLCBar> data, int period)
            {
                double[] prices = data.Select(b => b.Close).ToArray();
                double[] sma = new double[prices.Length];

                for (int i = 0; i < prices.Length; i++)
                {
                    if (i < period - 1)
                    {
                        sma[i] = 0; // NaN yerine 0
                        continue;
                    }

                    double sum = 0;
                    for (int j = 0; j < period; j++)
                    {
                        sum += prices[i - j];
                    }
                    sma[i] = sum / period;
                }
                return sma;
            }

            public static double[] CalculateRSI(List<OHLCBar> data, int period = 14)
            {
                double[] prices = data.Select(b => b.Close).ToArray();
                double[] rsi = new double[prices.Length];

                if (prices.Length < period + 1) return rsi;

                double[] gains = new double[prices.Length];
                double[] losses = new double[prices.Length];

                for (int i = 1; i < prices.Length; i++)
                {
                    double change = prices[i] - prices[i - 1];
                    gains[i] = change > 0 ? change : 0;
                    losses[i] = change < 0 ? -change : 0;
                }

                double avgGain = gains.Skip(1).Take(period).Average();
                double avgLoss = losses.Skip(1).Take(period).Average();

                if (avgLoss == 0) rsi[period] = 100;
                else rsi[period] = 100 - (100 / (1 + avgGain / avgLoss));

                for (int i = period + 1; i < prices.Length; i++)
                {
                    avgGain = (avgGain * (period - 1) + gains[i]) / period;
                    avgLoss = (avgLoss * (period - 1) + losses[i]) / period;

                    if (avgLoss == 0) rsi[i] = 100;
                    else rsi[i] = 100 - (100 / (1 + avgGain / avgLoss));
                }
                return rsi;
            }
            // Diğer indikatör metotları da buraya taşınacak
        }
    }
    ```

#### **2. Grafik Yönetim Katmanı (Chart Management Layer) - `src/services/ChartManagerService.cs`**

`PlotManager`'ın çok daha gelişmiş hali. UI ile Veri Katmanı arasında köprü görevi görür.

*   **Sorumluluklar:**
    *   `TradingDataService`'in `OnNewDataPointGenerated` olayını dinlemek.
    *   Gelen ham veriyi kendi içinde yönetmek (örn. `List<OHLCBar>`).
    *   Veri her güncellendiğinde indikatörleri yeniden hesaplamak.
    *   Hesaplanan verileri, ScottPlot'un çizim yapabileceği basit `double[]` gibi formatlara dönüştürmek.
    *   `OnChartNeedsUpdate` olayını yayınlayarak UI'ı bilgilendirmek.

*   **Yeni Veri Yapıları (`src/models/` altında):**
    *   `PlotUpdateData.cs`: UI'a gönderilecek temiz veri paketi.
        ```csharp
        using ScottPlot;
        using System.Collections.Generic;

        namespace AlgoTradeWithScottPlot.Models
        {
            public class PlotUpdateData
            {
                public string PlotId { get; set; } // Hangi plot güncellenecek? "Price", "RSI" vs.
                public OHLC[] CandlestickData { get; set; } // ScottPlot'un istediği format
                public double[] PrimaryLineData { get; set; } // Ana çizgi (örn. RSI çizgisi)
                public double[] SecondaryLineData { get; set; } // İkincil çizgi (örn. MACD sinyal çizgisi)
                public double[] BarData { get; set; } // Bar grafik verisi (örn. MACD histogram)
                // İhtiyaca göre diğer veri tipleri eklenebilir (örn. ScatterData, HorizontalLineValue vb.)
                public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
            }
        }
        ```

*   **Sınıf Yapısı (`ChartManagerService.cs`):**
    ```csharp
    using AlgoTradeWithScottPlot.Models;
    using AlgoTradeWithScottPlot.Services;
    using ScottPlot;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    namespace AlgoTradeWithScottPlot.Services
    {
        public class ChartManagerService
        {
            private readonly TradingDataService _dataService;
            private List<OHLCBar> _allData = new List<OHLCBar>();
            private readonly object _dataLock = new object();

            // Olay tanımı
            public event Action<PlotUpdateData> OnChartNeedsUpdate;

            public ChartManagerService(TradingDataService dataService)
            {
                _dataService = dataService;
                // Veri servisinin olayına abone ol!
                _dataService.OnNewDataPointGenerated += OnNewDataReceived;
            }

            // "Add" metodu: Yeni bir grafik tipi yönetime eklenir.
            // Bu metot, hangi grafiklerin aktif olduğunu ve hangi indikatörleri hesaplaması gerektiğini belirlemek için kullanılabilir.
            public void RegisterChart(string plotId)
            {
                // Örneğin, hangi plotId'lerin aktif olduğunu bir listede tutabiliriz.
                // Bu sayede sadece aktif plot'lar için hesaplama yaparız.
            }

            private async void OnNewDataReceived(object sender, OHLCEventArgs e)
            {
                lock (_dataLock)
                {
                    _allData.Add(e.NewBar);
                }

                // İşlemleri asenkron yap ki UI donmasın
                await Task.Run(() =>
                {
                    List<OHLCBar> currentData;
                    lock (_dataLock)
                    {
                        currentData = new List<OHLCBar>(_allData); // Verinin anlık kopyasını al
                    }

                    // 1. Ana Fiyat Grafiğini Hazırla
                    var priceUpdate = new PlotUpdateData
                    {
                        PlotId = "Price",
                        CandlestickData = currentData.Select(b => new ScottPlot.OHLC(b.Open, b.High, b.Low, b.Close, b.Timestamp, TimeSpan.FromMinutes(1))).ToArray()
                    };
                    OnChartNeedsUpdate?.Invoke(priceUpdate);

                    // 2. RSI Grafiğini Hazırla
                    if (currentData.Count > 14)
                    {
                        var rsiData = TradingDataService.CalculateRSI(currentData, 14); // Hesaplama
                        var rsiUpdate = new PlotUpdateData
                        {
                            PlotId = "RSI",
                            PrimaryLineData = rsiData
                        };
                        OnChartNeedsUpdate?.Invoke(rsiUpdate);
                    }
                    
                    // 3. MACD Grafiğini Hazırla
                    if (currentData.Count > 26 + 9) // MACD için gerekli minimum bar sayısı
                    {
                        var (macdLine, signalLine, histogram) = TradingDataService.CalculateMACD(currentData);
                        var macdUpdate = new PlotUpdateData
                        {
                            PlotId = "MACD",
                            PrimaryLineData = macdLine,
                            SecondaryLineData = signalLine,
                            BarData = histogram
                        };
                        OnChartNeedsUpdate?.Invoke(macdUpdate);
                    }
                    // Diğer tüm indikatörler için benzer bloklar...
                });
            }
        }
    }
    ```

#### **3. UI Katmanı (User Interface Layer) - `TradingChart.cs` ve `Form1.cs`**

*   **`TradingChart.cs` (Yeniden Yazım):**
    *   **Sorumluluk:** Sadece kendisine verilen veriyi çizmek. İçinde hesaplama veya veri yönetimi olmayacak. Kullanıcı etkileşimlerini (zoom, pan, reset) kendi içinde yönetebilir ancak bu etkileşimlerin sonucunu dışarıya bildirmelidir (örn. `OnZoomed` olayı).
    *   **API Değişikliği:** Public `Plot` propert'si kaldırılacak. Yerine public metotlar gelecek.
    *   **Yeni Metotlar:**
        ```csharp
        using ScottPlot;
        using ScottPlot.Plottables;
        using ScottPlot.WinForms;
        using System;
        using System.Drawing;
        using System.Windows.Forms;

        namespace AlgoTradeWithScottPlot.Components
        {
            public partial class TradingChart : UserControl
            {
                private FormsPlot formsPlot; // Private olacak
                private Crosshair _crosshair;

                public TradingChart()
                {
                    InitializeComponent(); // formsPlot'u burada initialize et
                    SetupPlot();
                }

                private void SetupPlot()
                {
                    formsPlot = new FormsPlot();
                    formsPlot.Dock = DockStyle.Fill;
                    this.Controls.Add(formsPlot);
                    formsPlot.BringToFront(); // formsPlot'un diğer kontrollerin üzerinde olduğundan emin ol

                    formsPlot.Plot.FigureBackground.Color = ScottPlot.Colors.White;
                    formsPlot.Plot.DataBackground.Color = ScottPlot.Colors.White;
                    formsPlot.Plot.Grid.MajorLineColor = ScottPlot.Colors.LightGray;

                    _crosshair = formsPlot.Plot.Add.Crosshair(0, 0);
                    _crosshair.IsVisible = false;
                    _crosshair.LineColor = ScottPlot.Colors.Red;

                    formsPlot.MouseMove += FormsPlot_MouseMove;
                    formsPlot.MouseLeave += FormsPlot_MouseLeave;
                    formsPlot.MouseWheel += FormsPlot_MouseWheel;
                    formsPlot.MouseDown += FormsPlot_MouseDown;
                    formsPlot.MouseUp += FormsPlot_MouseUp;
                }

                // Kullanıcı etkileşimleri için olaylar (Form1'e bildirmek için)
                public event EventHandler OnZoomed;
                public event EventHandler OnPanned;
                public event EventHandler OnResetView;

                // Veri güncelleme metotları
                public void UpdateCandlestickData(OHLC[] data)
                {
                    formsPlot.Plot.Clear();
                    formsPlot.Plot.Add.Candlestick(data);
                    formsPlot.Plot.Axes.AutoScale();
                    formsPlot.Refresh();
                }

                public void UpdateLineData(double[] lineData, double[] secondaryLineData = null)
                {
                    formsPlot.Plot.Clear();
                    if (lineData != null)
                    {
                        var signal = formsPlot.Plot.Add.Signal(lineData);
                        signal.Color = ScottPlot.Colors.Blue; // Varsayılan renk
                    }
                    if (secondaryLineData != null)
                    {
                        var signal = formsPlot.Plot.Add.Signal(secondaryLineData);
                        signal.Color = ScottPlot.Colors.Red; // Varsayılan renk
                    }
                    formsPlot.Plot.Axes.AutoScale();
                    formsPlot.Refresh();
                }

                public void UpdateBarData(double[] barData)
                {
                    formsPlot.Plot.Clear();
                    if (barData != null)
                    {
                        var bars = formsPlot.Plot.Add.Bars(barData);
                        // Histogram renkleri için örnek:
                        for (int i = 0; i < bars.Bars.Count; i++)
                        {
                            bars.Bars[i].FillColor = barData[i] >= 0 ? ScottPlot.Colors.Green.WithAlpha(0.5) : ScottPlot.Colors.Red.WithAlpha(0.5);
                        }
                    }
                    formsPlot.Plot.Axes.AutoScale();
                    formsPlot.Refresh();
                }

                // Ortak bir güncelleme metodu
                public void ApplyPlotUpdate(PlotUpdateData updateData)
                {
                    formsPlot.Plot.Clear(); // Mevcut tüm çizimleri temizle

                    if (updateData.CandlestickData != null && updateData.CandlestickData.Length > 0)
                    {
                        formsPlot.Plot.Add.Candlestick(updateData.CandlestickData);
                    }
                    if (updateData.PrimaryLineData != null && updateData.PrimaryLineData.Length > 0)
                    {
                        var line = formsPlot.Plot.Add.Signal(updateData.PrimaryLineData);
                        line.Color = ScottPlot.Colors.Blue;
                        line.LegendText = updateData.PlotId + " Line";
                    }
                    if (updateData.SecondaryLineData != null && updateData.SecondaryLineData.Length > 0)
                    {
                        var line = formsPlot.Plot.Add.Signal(updateData.SecondaryLineData);
                        line.Color = ScottPlot.Colors.Red;
                        line.LegendText = updateData.PlotId + " Signal";
                    }
                    if (updateData.BarData != null && updateData.BarData.Length > 0)
                    {
                        var bars = formsPlot.Plot.Add.Bars(updateData.BarData);
                        for (int i = 0; i < bars.Bars.Count; i++)
                        {
                            bars.Bars[i].FillColor = updateData.BarData[i] >= 0 ? ScottPlot.Colors.Green.WithAlpha(0.5) : ScottPlot.Colors.Red.WithAlpha(0.5);
                        }
                        bars.LegendText = updateData.PlotId + " Histogram";
                    }
                    
                    // Ek veriler için (örn. yatay çizgiler)
                    if (updateData.AdditionalData.ContainsKey("HorizontalLineValue") && updateData.AdditionalData["HorizontalLineValue"] is double hLineVal)
                    {
                        formsPlot.Plot.Add.HorizontalLine(hLineVal);
                    }

                    formsPlot.Plot.Axes.AutoScale();
                    formsPlot.Refresh();
                }

                // Kullanıcı etkileşimleri (Crosshair, Zoom, Pan)
                private void FormsPlot_MouseMove(object sender, MouseEventArgs e)
                {
                    var mouseCoords = formsPlot.Plot.GetCoordinates(e.Location);
                    _crosshair.Position = mouseCoords;
                    _crosshair.IsVisible = true;
                    formsPlot.Refresh();
                }

                private void FormsPlot_MouseLeave(object sender, EventArgs e)
                {
                    _crosshair.IsVisible = false;
                    formsPlot.Refresh();
                }

                private Point _mouseDownLocation;
                private bool _isPanning = false;
                private bool _isZoomingRect = false;
                private ScottPlot.Plottables.Rectangle _zoomRectangle;
                private ScottPlot.Coordinates _zoomRectStartCoords;

                private void FormsPlot_MouseDown(object sender, MouseEventArgs e)
                {
                    _mouseDownLocation = e.Location;
                    if (e.Button == MouseButtons.Left)
                    {
                        _isPanning = true;
                    }
                    else if (e.Button == MouseButtons.Middle)
                    {
                        _isZoomingRect = true;
                        _zoomRectStartCoords = formsPlot.Plot.GetCoordinates(e.Location);
                        _zoomRectangle = formsPlot.Plot.Add.Rectangle(0, 0, 0, 0);
                        _zoomRectangle.FillStyle.Color = ScottPlot.Colors.LightBlue.WithAlpha(100);
                        _zoomRectangle.LineStyle.Color = ScottPlot.Colors.Blue;
                        _zoomRectangle.LineStyle.Width = 2;
                    }
                }

                private void FormsPlot_MouseUp(object sender, MouseEventArgs e)
                {
                    if (_isPanning)
                    {
                        _isPanning = false;
                        OnPanned?.Invoke(this, EventArgs.Empty);
                    }
                    else if (_isZoomingRect)
                    {
                        _isZoomingRect = false;
                        formsPlot.Plot.Remove(_zoomRectangle); // Dikdörtgeni kaldır

                        var endCoords = formsPlot.Plot.GetCoordinates(e.Location);
                        double x1 = Math.Min(_zoomRectStartCoords.X, endCoords.X);
                        double x2 = Math.Max(_zoomRectStartCoords.X, endCoords.X);
                        double y1 = Math.Min(_zoomRectStartCoords.Y, endCoords.Y);
                        double y2 = Math.Max(_zoomRectStartCoords.Y, endCoords.Y);

                        if (Math.Abs(x2 - x1) > 0.001 && Math.Abs(y2 - y1) > 0.001) // Geçerli bir dikdörtgen çizildiyse
                        {
                            formsPlot.Plot.Axes.SetLimits(x1, x2, y1, y2);
                            OnZoomed?.Invoke(this, EventArgs.Empty);
                        }
                        else // Sadece tıklandıysa, resetle
                        {
                            formsPlot.Plot.Axes.AutoScale();
                            OnResetView?.Invoke(this, EventArgs.Empty);
                        }
                        formsPlot.Refresh();
                    }
                }

                private void FormsPlot_MouseWheel(object sender, MouseEventArgs e)
                {
                    formsPlot.Plot.Axes.Zoom(e.Delta > 0 ? 0.8 : 1.25, e.Location);
                    formsPlot.Refresh();
                    OnZoomed?.Invoke(this, EventArgs.Empty);
                }

                // Diğer UI kontrolleri (butonlar, paneller) ve event handler'ları
                // Bu kısım mevcut TradingChart.cs'deki gibi kalabilir, ancak iç mantıkları
                // artık doğrudan ScottPlot'u manipüle etmek yerine, dışarıya olay fırlatmalı
                // veya Form1'deki servis metotlarını çağırmalıdır.
            }
        }
        ```

*   **`Form1.cs` (Yeniden Yazım):**
    *   **Sorumluluk:** Servisleri başlatmak, UI kontrollerini (butonlar, paneller) yönetmek ve servislerden gelen olayları dinleyip ilgili `TradingChart` bileşenine veriyi paslamak.
    *   **Yapısı:**
        ```csharp
        using AlgoTradeWithScottPlot.Components;
        using AlgoTradeWithScottPlot.Models;
        using AlgoTradeWithScottPlot.Services;
        using System;
        using System.Collections.Generic;
        using System.Windows.Forms;

        namespace AlgoTradeWithScottPlot
        {
            public partial class Form1 : Form
            {
                private TradingDataService _dataService;
                private ChartManagerService _chartManager;
                private LogManager _logManager; // LogManager'ı da servis olarak kullan

                // Form'da birden çok TradingChart bileşeni olabilir
                private TradingChart _priceChart;
                private TradingChart _rsiChart;
                private TradingChart _macdChart;
                // ... diğer chartlar

                private Dictionary<string, TradingChart> _charts = new Dictionary<string, TradingChart>();

                public Form1()
                {
                    InitializeComponent();
                    _logManager = LogManager.Instance; // Singleton LogManager
                    _logManager.AttachRichTextBox(richTextBox1); // Logları RichTextBox'a bağla

                    InitializeServices();
                    InitializeUIComponents();
                }

                private void InitializeServices()
                {
                    _dataService = new TradingDataService();
                    _chartManager = new ChartManagerService(_dataService);

                    // ChartManager'dan gelen güncelleme olayını dinle
                    _chartManager.OnChartNeedsUpdate += OnChartUpdateReceived;
                    _logManager.LogInfo("Servisler başlatıldı ve olaylara abone olundu.");
                }

                private void InitializeUIComponents()
                {
                    // TradingChart bileşenlerini dinamik olarak oluştur ve panele ekle
                    // Örnek:
                    _priceChart = new TradingChart { Dock = DockStyle.Top, Height = 300 };
                    _rsiChart = new TradingChart { Dock = DockStyle.Top, Height = 150 };
                    _macdChart = new TradingChart { Dock = DockStyle.Top, Height = 150 };

                    panelCharts.Controls.Add(_macdChart); // En alttan üste doğru ekle
                    panelCharts.Controls.Add(_rsiChart);
                    panelCharts.Controls.Add(_priceChart);

                    _charts.Add("Price", _priceChart);
                    _charts.Add("RSI", _rsiChart);
                    _charts.Add("MACD", _macdChart);
                    // Diğer chartları da ekle

                    // Her bir TradingChart'ın kendi etkileşim olaylarına abone ol
                    _priceChart.OnResetView += (s, e) => _logManager.LogInfo("Price Chart resetlendi.");
                    // Diğer chartlar için de benzer abonelikler
                }

                private void OnChartUpdateReceived(PlotUpdateData updateData)
                {
                    // UI thread'inde olduğundan emin ol
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => OnChartUpdateReceived(updateData)));
                        return;
                    }

                    // Gelen veriye göre doğru TradingChart bileşenini güncelle
                    if (_charts.TryGetValue(updateData.PlotId, out var chartComponent))
                    {
                        chartComponent.ApplyPlotUpdate(updateData);
                        _logManager.LogDebug($"{updateData.PlotId} grafiği güncellendi.");
                    }
                    else
                    {
                        _logManager.LogWarning($"'{updateData.PlotId}' ID'li grafik bileşeni bulunamadı.");
                    }
                }

                private void buttonGenerateData_Click(object sender, EventArgs e)
                {
                    _logManager.LogInfo("Veri üretimi başlatılıyor...");
                    _dataService.StartRealTimeGeneration(500); // Her 500ms'de bir yeni bar üret
                }

                private void buttonStopData_Click(object sender, EventArgs e)
                {
                    _logManager.LogInfo("Veri üretimi durduruluyor.");
                    _dataService.StopGeneration();
                }

                // Diğer buton event handler'ları da benzer şekilde servis metotlarını çağıracak.
                // Örneğin, "Add Chart" butonu ChartManagerService.RegisterChart() metodunu çağırabilir.
            }
        }
        ```

---

Bu yeniden yazım planı, projenizin temelini sağlamlaştıracak ve gelecekteki geliştirmeler için çok daha uygun bir zemin hazırlayacaktır. Her adımda test edilebilirliği ve modülerliği artırarak, daha yönetilebilir bir kod tabanı elde edeceksiniz.

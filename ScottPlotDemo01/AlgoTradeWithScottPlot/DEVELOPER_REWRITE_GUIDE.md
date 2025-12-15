# Geliştirici Rehberi: AlgoTradeWithScottPlot Projesini Yeniden Yapılandırma

**Tarih:** 27.11.2025
**Hazırlayan:** Gemini AI

## 1. Giriş ve Amaç

Bu doküman, `AlgoTradeWithScottPlot` projesinin mevcut mimarisini analiz ederek, daha modern, sürdürülebilir ve test edilebilir bir yapıya kavuşturmak için önerilen yeniden yapılandırma (refactoring) adımlarını detaylandırmaktadır.

Mevcut proje, tipik bir "Code-Behind" yaklaşımıyla geliştirilmiş bir Windows Forms uygulamasıdır. Bu yaklaşımda, `Form1.cs` gibi UI sınıfları, iş mantığı, veri erişimi ve UI etkileşimleri gibi birçok farklı sorumluluğu kendi üzerinde toplamaktadır. Bu durum, projenin büyümesini, bakımını ve test edilmesini zorlaştırır.

**Bu yeniden yapılandırmanın temel amacı, projeyi aşağıdaki ilkelere uygun hale getirmektir:**

*   **Sorumlulukların Ayrılması (Separation of Concerns):** Her sınıfın veya katmanın net ve tek bir sorumluluğu olmalıdır.
*   **Gevşek Bağlılık (Loose Coupling):** Bileşenler birbirine sıkı sıkıya bağlı olmamalı, aralarındaki iletişim arayüzler veya olaylar (events) üzerinden sağlanmalıdır.
*   **Test Edilebilirlik (Testability):** İş mantığı, UI'dan bağımsız olarak birim testleri (unit tests) ile test edilebilmelidir.
*   **Duyarlılık (Responsiveness):** Uzun süren işlemler (veri üretimi, hesaplama vb.) UI thread'ini kilitlememelidir.

Bu hedeflere ulaşmak için **Olay Tabanlı (Event-Driven) ve Servis Odaklı (Service-Oriented)** bir mimari benimsenecektir.

---

## 2. Mevcut Mimarinin Analizi ve Tespit Edilen Sorunlar

Mevcut kod tabanında aşağıdaki temel sorunlar tespit edilmiştir:

1.  **"God Class" Anti-Pattern'i:** `Form1.cs` sınıfı, veri üretimi, indikatör hesaplamaları, grafik çizimi, UI etkileşimleri ve durum yönetimi gibi neredeyse tüm görevleri tek başına üstlenmektedir. Bu, sınıfı aşırı karmaşık ve anlaşılması zor hale getirir.
2.  **Sıkı Bağlılık (Tight Coupling):** `Form1`, `DataGenerator` ve `PlotManager` gibi sınıfları doğrudan bilir ve kullanır. UI katmanı, altyapı ve iş mantığı katmanlarıyla iç içe geçmiştir.
3.  **Test Edilemezlik:** Bir indikatör hesaplamasının doğruluğunu veya bir alım-satım sinyalinin mantığını test etmek için tüm formu ayağa kaldırmak gerekir. İş mantığı, UI'dan ayrılamadığı için birim testleri yazılamaz.
4.  **UI Donmaları:** `button1_Click` gibi olayların içinde yapılan uzun süreli senkron işlemler (`DataGenerator.GenerateOHLCs`, indikatör hesaplamaları vb.) ve bu sırada kullanılan `Application.DoEvents()` çağrıları, uygulamanın donmasına ve kötü bir kullanıcı deneyimine yol açar.
5.  **Kırılganlık ve Bakım Zorluğu:** Grafik üzerinde küçük bir değişiklik yapmak (örneğin bir çizginin rengini değiştirmek) veya yeni bir indikatör eklemek, `Form1.cs` içindeki yüzlerce satırlık kod blokları arasında gezinmeyi gerektirir. Bu süreç, hata yapmaya çok açıktır.

---

## 3. Önerilen Yeni Mimari ve Veri Akışı

Önerilen mimari, projeyi üç ana katmana ayırır:

1.  **Veri Katmanı (`TradingDataService`):** Uygulamanın "motoru". Ham veriyi üretir, saklar ve ham veriden indikatörleri hesaplar. Diğer katmanlardan tamamen habersizdir.
2.  **Uygulama/Servis Katmanı (`ChartManagerService`):** Uygulamanın "beyni". Veri katmanından gelen olayları dinler, bu verileri UI'ın çizebileceği modellere dönüştürür ve UI katmanını bilgilendirir.
3.  **UI Katmanı (`Form1`, `TradingChart`):** Uygulamanın "yüzü". Sadece kendine verilen hazır veriyi ekranda gösterir ve kullanıcıdan gelen komutları (örn. "Veri Üretimini Başlat" butonu) ilgili servislere iletir.

### Olay Tabanlı Veri Akışı

Bu katmanlar arasındaki iletişim şu şekilde gerçekleşecektir:

1.  **`TradingDataService`**, bir zamanlayıcı (timer) aracılığıyla yeni bir `OHLCBar` (mum çubuğu) verisi üretir.
2.  Veri üretildikten hemen sonra, `OnNewDataPointGenerated` adında bir olay (event) tetikler. Bu olayın içinde yeni üretilen `OHLCBar` verisi bulunur.
3.  **`ChartManagerService`**, başlangıçta bu olaya abone olmuştur. Olay tetiklendiğinde, yeni `OHLCBar` verisini alır ve kendi iç veri listesine ekler.
4.  `ChartManagerService`, bu yeni veriyle birlikte tüm indikatörleri (RSI, MACD vb.) **asenkron** bir şekilde yeniden hesaplar.
5.  Hesaplama bittiğinde, her bir grafik (Fiyat, RSI, MACD) için çizime hazır veri paketleri (`PlotUpdateData` nesneleri) oluşturur.
6.  Her bir hazır paket için `OnChartNeedsUpdate` olayını tetikler. Bu olayın içinde, hangi grafiğin güncelleneceği (`PlotId`) ve çizilecek veriler bulunur.
7.  **`Form1`**, başlangıçta `OnChartNeedsUpdate` olayına abone olmuştur. Olay tetiklendiğinde, gelen `PlotId`'ye bakarak ilgili `TradingChart` bileşenini bulur.
8.  `Form1`, bulduğu `TradingChart` bileşeninin `ApplyPlotUpdate` metodunu çağırarak, çizime hazır veri paketini ona teslim eder.
9.  **`TradingChart`** bileşeni, aldığı bu hazır veriyi ekrana çizer.

Bu yapı sayesinde, hiçbir katman bir diğerinin iç işleyişini bilmek zorunda kalmaz ve tüm sistem olaylar üzerinden haberleşir.

---

## 4. Adım Adım Uygulama Rehberi

Bu bölümde, yukarıda açıklanan mimariyi hayata geçirmek için gereken kod değişiklikleri ve yeni dosyalar adım adım sunulmaktadır.

### Adım 1: Proje Yapısını ve Modelleri Oluşturma

Öncelikle, `src` klasörü altında `models` ve `services` adında iki yeni klasör oluşturulur. Ardından, bu yeni mimaride veri taşımak için kullanılacak olan model sınıfları `src/models` klasörüne eklenir.

**`src/models/OHLCBar.cs`**
*Amaç: ScottPlot kütüphanesine bağımlı olmayan, kendi esnek OHLC veri modelimizi oluşturmak.*
```csharp
using System;

namespace AlgoTradeWithScottPlot.Models
{
    public struct OHLCBar
    {
        public DateTime Timestamp { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }
}
```

**`src/models/OHLCEventArgs.cs`**
*Amaç: `OnNewDataPointGenerated` olayı tetiklendiğinde, yeni bar verisini standart bir şekilde taşımak.*
```csharp
using System;
using AlgoTradeWithScottPlot.Models;

namespace AlgoTradeWithScottPlot.Models
{
    public class OHLCEventArgs : EventArgs
    {
        public OHLCBar NewBar { get; }
        public OHLCEventArgs(OHLCBar newBar) { NewBar = newBar; }
    }
}
```

**`src/models/PlotUpdateData.cs`**
*Amaç: `ChartManagerService`'ten UI katmanına, bir grafiğin nasıl çizileceğine dair tüm bilgileri içeren temiz bir veri paketi göndermek.*
```csharp
using ScottPlot;
using System.Collections.Generic;

namespace AlgoTradeWithScottPlot.Models
{
    public class PlotUpdateData
    {
        public string PlotId { get; set; }
        public OHLC[] CandlestickData { get; set; }
        public double[] PrimaryLineData { get; set; }
        public double[] SecondaryLineData { get; set; }
        public double[] BarData { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
}
```

### Adım 2: Veri Servisini Geliştirme (`TradingDataService`)

Bu servis, eski statik `DataGenerator` sınıfının yerini alır. `src/services` klasörü altına oluşturulur.

**`src/services/TradingDataService.cs`**
*Amaç: Veri üretimi ve indikatör hesaplama sorumluluğunu tek bir merkezde toplamak ve yeni veri üretildiğinde olay yayınlamak.*
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlgoTradeWithScottPlot.Models;

namespace AlgoTradeWithScottPlot.Services
{
    public class TradingDataService
    {
        private readonly List<OHLCBar> _data = new List<OHLCBar>();
        private readonly object _dataLock = new object();
        private Timer _timer;
        private readonly Random _random = new Random();
        private double _lastPrice = 100.0;

        public event EventHandler<OHLCEventArgs> OnNewDataPointGenerated;

        public void StartRealTimeGeneration(int intervalMilliseconds = 1000)
        {
            _timer = new Timer(GenerateNextBar, null, 0, intervalMilliseconds);
        }

        public void StopGeneration()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        private void GenerateNextBar(object state)
        {
            var newBar = new OHLCBar
            {
                Timestamp = DateTime.Now,
                Open = _lastPrice,
                Close = _lastPrice + (_random.NextDouble() - 0.5) * 2,
                High = Math.Max(_lastPrice, _lastPrice + (_random.NextDouble() - 0.5) * 2) + _random.NextDouble(),
                Low = Math.Min(_lastPrice, _lastPrice + (_random.NextDouble() - 0.5) * 2) - _random.NextDouble(),
                Volume = 1000 + _random.NextDouble() * 500
            };
            _lastPrice = newBar.Close;

            lock (_dataLock) { _data.Add(newBar); }

            OnNewDataPointGenerated?.Invoke(this, new OHLCEventArgs(newBar));
        }

        // --- Indicator Calculation Methods ---
        // (Buraya REWRITE_PLAN.md dosyasında belirtilen tüm statik hesaplama metotları gelecek)
        // Örnek: public static double[] CalculateSMA(List<OHLCBar> data, int period) { ... }
        // ...
    }
}
```

### Adım 3: Grafik Yönetim Servisini Geliştirme (`ChartManagerService`)

Bu yeni servis, veri ve UI katmanları arasındaki koordinasyonu sağlar. `src/services` klasörü altına oluşturulur.

**`src/services/ChartManagerService.cs`**
*Amaç: Ham veriyi dinlemek, onu çizime hazır hale getirmek ve UI'a "çiz" komutunu göndermek.*
```csharp
using AlgoTradeWithScottPlot.Models;
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
        private readonly List<OHLCBar> _allData = new List<OHLCBar>();
        private readonly object _dataLock = new object();
        private readonly HashSet<string> _registeredCharts = new HashSet<string>();

        public event Action<PlotUpdateData> OnChartNeedsUpdate;

        public ChartManagerService(TradingDataService dataService)
        {
            _dataService = dataService;
            _dataService.OnNewDataPointGenerated += OnNewDataReceived;
        }

        public void RegisterChart(string plotId)
        {
            _registeredCharts.Add(plotId);
        }

        private async void OnNewDataReceived(object sender, OHLCEventArgs e)
        {
            lock (_dataLock) { _allData.Add(e.NewBar); }

            await Task.Run(() =>
            {
                List<OHLCBar> currentData;
                lock (_dataLock) { currentData = new List<OHLCBar>(_allData); }

                if (_registeredCharts.Contains("Price")) ProcessPriceChart(currentData);
                if (_registeredCharts.Contains("RSI")) ProcessRsiChart(currentData);
                if (_registeredCharts.Contains("MACD")) ProcessMacdChart(currentData);
            });
        }

        // Her grafik tipi için ayrı bir işlem metodu
        private void ProcessPriceChart(List<OHLCBar> data) { /* ... */ }
        private void ProcessRsiChart(List<OHLCBar> data) { /* ... */ }
        private void ProcessMacdChart(List<OHLCBar> data) { /* ... */ }
        // (Bu metotların iç mantığı REWRITE_PLAN.md dosyasında detaylandırılmıştır)
    }
}
```

### Adım 4: `TradingChart` Bileşenini Yeniden Yapılandırma

Mevcut `TradingChart` bileşeni, dışarıya `Plot` nesnesini açarak ve çok fazla sorumluluk alarak mimariyi bozmaktadır. Bu bileşen, sadece veri alıp çizen "aptal" bir bileşene dönüştürülecektir.

**`src/components/TradingChart.cs`**
*Amaç: Bileşeni yeniden kullanılabilir ve sadece görselleştirmeden sorumlu hale getirmek.*
```csharp
using AlgoTradeWithScottPlot.Models;
using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlgoTradeWithScottPlot.Components
{
    public partial class TradingChart : UserControl
    {
        private readonly FormsPlot _formsPlot;
        private readonly Crosshair _crosshair;
        
        // (Diğer private UI ve etkileşim değişkenleri)

        public TradingChart()
        {
            InitializeComponent();
            // _formsPlot'u manuel oluştur ve ayarla
            _formsPlot = new FormsPlot { Dock = DockStyle.Fill };
            this.Controls.Add(_formsPlot);
            // (Diğer temel stil ve olay abonelikleri)
        }

        // DIŞARIYA AÇILAN ANA METOT
        public void ApplyPlotUpdate(PlotUpdateData updateData)
        {
            if (updateData == null) return;
            _formsPlot.Plot.Clear();

            // Gelen veriye göre çizim yap
            if (updateData.CandlestickData != null) { /* ... */ }
            if (updateData.PrimaryLineData != null) { /* ... */ }
            // (Bu metodun iç mantığı REWRITE_PLAN.md dosyasında detaylandırılmıştır)

            _formsPlot.Plot.Axes.AutoScale();
            _formsPlot.Refresh();
        }

        // (Mouse etkileşim metotları: FormsPlot_MouseMove, FormsPlot_MouseDown vb. burada kalacak)
    }
}
```
**Önemli Not:** `TradingChart.Designer.cs` dosyasındaki `formsPlot` ile ilgili satırların manuel olarak kaldırılması gerekebilir, çünkü artık `formsPlot` kod tarafında (programatik olarak) oluşturulmaktadır.

### Adım 5: `Form1`'i Sadeleştirme

`Form1` artık bir orkestratör görevi görecek. Tüm iş mantığı ve veri yönetimi kodları temizlenecek.

**`Form1.cs`**
*Amaç: `Form1`'i sadece servisleri başlatmak, olayları dinlemek ve UI bileşenlerini koordine etmekle sorumlu, basit bir sınıf haline getirmek.*
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
        private LogManager _logManager;
        private Dictionary<string, TradingChart> _charts = new Dictionary<string, TradingChart>();

        public Form1()
        {
            InitializeComponent();
            InitializeLogging();
            InitializeServices();
            InitializeUIComponentsAndCharts();
        }

        private void InitializeLogging() { /* ... */ }

        private void InitializeServices()
        {
            _dataService = new TradingDataService();
            _chartManager = new ChartManagerService(_dataService);
            _chartManager.OnChartNeedsUpdate += OnChartUpdateReceived;
        }

        private void InitializeUIComponentsAndCharts()
        {
            // Chart bileşenlerini oluştur ve panele ekle
            var priceChart = new TradingChart { Dock = DockStyle.Top, Height = 300 };
            var rsiChart = new TradingChart { Dock = DockStyle.Top, Height = 150 };
            // ...

            // Oluşturulan chart'ları dictionary'e ekle
            _charts.Add("Price", priceChart);
            _charts.Add("RSI", rsiChart);
            // ...

            // ChartManager'a hangi grafikleri yöneteceğini bildir
            _chartManager.RegisterChart("Price");
            _chartManager.RegisterChart("RSI");
            // ...
        }

        private void OnChartUpdateReceived(PlotUpdateData data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnChartUpdateReceived(data)));
                return;
            }

            if (_charts.TryGetValue(data.PlotId, out var chart))
            {
                chart.ApplyPlotUpdate(data);
            }
        }

        private void buttonGenerateData_Click(object sender, EventArgs e)
        {
            _dataService.StartRealTimeGeneration(500);
        }
    }
}
```

### Adım 6: Proje Dosyasını Güncelleme ve Temizlik

1.  **Proje Dosyasını Düzenle:** `AlgoTradeWithScottPlot.csproj` dosyasını bir metin editörü ile açın.
2.  **Eski Dosyayı Kaldır:** `<Compile Include="src\DataGenerator.cs" />` satırını bulun ve silin.
3.  **Yeni Dosyaları Ekle:** Aşağıdaki satırları uygun bir `ItemGroup` içine ekleyin:
    ```xml
    <Compile Include="src\models\OHLCBar.cs" />
    <Compile Include="src\models\OHLCEventArgs.cs" />
    <Compile Include="src\models\PlotUpdateData.cs" />
    <Compile Include="src\services\ChartManagerService.cs" />
    <Compile Include="src/services/TradingDataService.cs" />
    ```
4.  **Dosyayı Sil:** Proje klasöründen `src/DataGenerator.cs` dosyasını fiziksel olarak silin.

---

## 5. Sonuç

Bu yeniden yapılandırma planı tamamlandığında, elinizde şu özelliklere sahip bir proje olacaktır:
*   **Modüler:** Her parçanın görevi nettir ve bağımsız olarak geliştirilebilir.
*   **Test Edilebilir:** `TradingDataService` ve `ChartManagerService` sınıfları, UI olmadan test edilebilir.
*   **Esnek:** Yeni bir indikatör veya grafik eklemek, sadece `ChartManagerService`'te küçük bir ekleme ve `Form1`'de yeni bir `TradingChart` bileşeni oluşturmak kadar kolay olacaktır.
*   **Performanslı:** Asenkron yapı sayesinde, veri işlenirken kullanıcı arayüzü akıcı kalacaktır.

Bu rehber, projeyi daha profesyonel bir standarda taşımak için sağlam bir yol haritası sunmaktadır. Başarılar!

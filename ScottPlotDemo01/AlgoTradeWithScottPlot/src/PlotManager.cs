using ScottPlot;
using ScottPlot.WinForms;
using ScottPlot.Plottables;
using AlgoTradeWithScottPlot.Components;
using System.Drawing;

namespace AlgoTradeWithScottPlot.Components
{
    /// <summary>
    /// TradingChart'ın plotlarını yöneten sınıf
    /// Plot ekleme, çıkarma, bulma ve yönetme işlemlerini gerçekleştirir
    /// </summary>
    public class PlotManager
    {
        private TradingChart? _tradingChart;
        private FormsPlot? _formsPlot;
        private readonly Dictionary<string, PlotInfo> _plots;
        private readonly Dictionary<int, string> _plotIds; // Plot index -> Plot ID mapping
        private readonly Dictionary<ScottPlot.Plot, int> _plotToIndex; // Plot object -> Index mapping
        private readonly object _lockObject = new object();
        private int _plotCounter = 0;
        private bool _isInitialized = false;
        private LogManager? _logManager;
        
        // Default boyutlar
        public static readonly int DefaultWidth = 1200;
        public static readonly int DefaultHeight = 600;
        public static readonly int DefaultCandlestickHeight = 800;
        public static readonly int DefaultIndicatorHeight = 400;

        public IReadOnlyDictionary<string, PlotInfo> Plots => _plots;

        public int Count => _plots.Count;

        public PlotManager()
        {
            _plots = new Dictionary<string, PlotInfo>();
            _plotIds = new Dictionary<int, string>();
            _plotToIndex = new Dictionary<ScottPlot.Plot, int>();
            _logManager = LogManager.Instance; // Default olarak singleton instance kullan
        }

        /// <summary>
        /// LogManager'ı set eder
        /// </summary>
        /// <param name="logManager">Kullanılacak LogManager instance'ı</param>
        public void SetLogManager(LogManager? logManager)
        {
            _logManager = logManager;
        }

        /// <summary>
        /// Mevcut LogManager'ı döndürür
        /// </summary>
        /// <returns>LogManager instance'ı veya null</returns>
        public LogManager? GetLogManager()
        {
            return _logManager;
        }

        /// <summary>
        /// Log mesajı yazmak için yardımcı metod
        /// </summary>
        /// <param name="level">Log seviyesi</param>
        /// <param name="message">Log mesajı</param>
        private void Log(LogLevel level, string message)
        {
            _logManager?.Log(level, $"[PlotManager] {message}");
        }

        public void SetTradingChart(TradingChart tradingChart)
        {
            _tradingChart = tradingChart ?? throw new ArgumentNullException(nameof(tradingChart));
            _formsPlot = _tradingChart.Plot ?? throw new ArgumentNullException(nameof(_tradingChart.Plot));
            _plotCounter = 0;
            _isInitialized = false;
            Log(LogLevel.Info, "TradingChart bağlandı");
        }

        /// <summary>
        /// Tüm plotları temizler ve baştan başlar
        /// </summary>
        public void ResetAll()
        {
            if (_formsPlot == null) return;
            
            lock (_lockObject)
            {
                _formsPlot.Reset();
                _plotCounter = 0;
                _isInitialized = false;
                _plots.Clear();
            }
        }

        /// <summary>
        /// Tüm plotları kaldırır ve temizler
        /// </summary>
        public void RemoveAllPlots()
        {
            if (_formsPlot == null) return;
            
            lock (_lockObject)
            {
                try
                {
                    Log(LogLevel.Info, "Tüm plotlar temizleniyor...");
                    
                    // Multiplot'u tamamen temizle
                    _formsPlot.Reset();
                    
                    // Counter ve flag'leri sıfırla
                    _plotCounter = 0;
                    _isInitialized = false;
                    
                    // Plot bilgilerini temizle
                    _plots.Clear();
                    _plotIds.Clear();
                    _plotToIndex.Clear();
                    
                    // Chart'ı yenile
                    _formsPlot.Refresh();
                    
                    Log(LogLevel.Info, "Tüm plotlar temizlendi");
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Error, $"Plot temizleme hatası: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Error removing all plots: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirtilen sayıda plot oluşturur (MultiplotDraggableForm gibi)
        /// </summary>
        /// <param name="plotCount">Oluşturulacak plot sayısı</param>
        public void CreatePlots(int plotCount)
        {
            if (_tradingChart == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
                if (!_isInitialized)
                {
                    _formsPlot.Reset();
                    
                    // Tek seferde birden fazla plot ekle
                    _formsPlot.Multiplot.AddPlots(plotCount);
                    
                    // Layout düzenle
                    _formsPlot.Multiplot.CollapseVertically();
                    
                    _isInitialized = true;
                    _plotCounter = plotCount;
                }
            }
        }

        /// <summary>
        /// Yeni plot ekler (dinamik kullanım için)
        /// </summary>
        /// <returns>Eklenen plot</returns>
        public ScottPlot.Plot AddPlot()
        {
            if (_tradingChart == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
/*
                // İlk plot ekleniyorsa multiplot'u başlat
                if (!_isInitialized)
                {
                    _formsPlot.Reset();
                    _isInitialized = true;
                }
*/
                // Yeni plot ekle
                var plot = _formsPlot.Multiplot.AddPlot();
                
                // Plot nesnesini index ile eşleştir
                _plotToIndex[plot] = _plotCounter;
                
                Log(LogLevel.Debug, $"Yeni plot eklendi - Index: {_plotCounter}");
                
                // Plot counter'ı artır
                _plotCounter++;

                // NOT: Layout güncellenmesi FinalizeLayout() ile yapılacak
                // Her AddPlot()'ta CollapseVertically() yapmıyoruz
                
                return plot;
            }
        }

        /// <summary>
        /// Belirtilen index'teki plot'u döndürür (oluşturulmamışsa önce oluşturur)
        /// </summary>
        /// <param name="index">Plot index'i</param>
        /// <param name="totalPlots">Toplam plot sayısı (ilk çağrıda gerekli)</param>
        /// <returns></returns>
        public ScottPlot.Plot GetOrCreatePlot(int index, int totalPlots = 2)
        {
            if (_tradingChart == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
                // İlk kez çağrılıyorsa plotları oluştur
                if (!_isInitialized)
                {
                    CreatePlots(totalPlots);
                }

                // Plot'u döndür
                return _formsPlot.Multiplot.GetPlot(index);
            }
        }

        /// <summary>
        /// Tüm plotları düzenler ve yeniler
        /// </summary>
        public void FinalizeLayout()
        {
            if (_formsPlot == null) return;
            
            lock (_lockObject)
            {
                Log(LogLevel.Debug, "Layout düzenleniyor...");
                // Dikey olarak daralt
                _formsPlot.Multiplot.CollapseVertically();
                
                // Chart'ı yenile
                _formsPlot.Refresh();
                Log(LogLevel.Debug, "Layout düzenlendi");
            }
        }

        /// <summary>
        /// Belirtilen index'teki plot'u döndürür
        /// </summary>
        /// <param name="index">Plot index'i (0-based)</param>
        /// <returns>Plot nesnesi</returns>
        public ScottPlot.Plot GetPlot(int index)
        {
            if (_formsPlot == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            // Multiplot modunda mı kontrol et
            if (_isInitialized && _plotCounter > 0)
            {
                // Multiplot modunda - tüm plotlar multiplot'tan
                try
                {
                    var plot = _formsPlot.Multiplot.GetPlot(index);
                    
                    // Plot nesnesini index ile eşleştir (eğer yoksa)
                    if (!_plotToIndex.ContainsKey(plot))
                    {
                        _plotToIndex[plot] = index;
                    }
                    
                    return plot;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Plot index {index} bulunamadı: {ex.Message}");
                }
            }
            else
            {
                // Tek plot modunda - sadece ana plot
                if (index == 0)
                {
                    _formsPlot.Plot.Clear();
                    var plot = _formsPlot.Plot;
                    
                    // Plot nesnesini index ile eşleştir (eğer yoksa)
                    if (!_plotToIndex.ContainsKey(plot))
                    {
                        _plotToIndex[plot] = index;
                    }
                    
                    return plot;
                }
                else
                {
                    throw new ArgumentException($"Tek plot modunda index {index} geçersiz. Önce AddPlot() çağırın.");
                }
            }
        }

        /// <summary>
        /// Plot'a veri ekler (index zorunlu)
        /// </summary>
        /// <param name="plot">Hedef plot</param>
        /// <param name="plotType">Veri tipi</param>
        /// <param name="data">Veri</param>
        /// <param name="index">Veri indeksi (zorunlu - her data için unique olmalı)</param>
        /// <returns>Oluşturulan plottable</returns>
        public IPlottable AddPlotData(ScottPlot.Plot plot, PlotType plotType, object data, int index)
        {
            if (plot == null)
                throw new ArgumentNullException(nameof(plot));

            lock (_lockObject)
            {
                Log(LogLevel.Debug, $"Plot data ekleniyor - Tip: {plotType}, Index: {index}");
                IPlottable plottable = null;

                switch (plotType)
                {
                    case PlotType.Candlestick:
                        if (data is ScottPlot.OHLC[] ohlcArray)
                        {
                            var candlePlot = plot.Add.Candlestick(ohlcArray);
                            candlePlot.Sequential = true;
                            plottable = candlePlot;
                        }
                        else if (data is List<ScottPlot.OHLC> ohlcList)
                        {
                            var candlePlot = plot.Add.Candlestick(ohlcList);
                            candlePlot.Sequential = true;
                            plottable = candlePlot;
                        }
                        break;

                    case PlotType.Line:
                        if (data is double[] lineData)
                        {
                            var line = plot.Add.Signal(lineData);
                            line.LineWidth = 2;
                            plottable = line;
                        }
                        break;

                    case PlotType.Bar:
                    case PlotType.Volume:
                        if (data is double[] barData)
                        {
                            var bars = plot.Add.Bars(barData);
                            plottable = bars;
                        }
                        break;

                    case PlotType.Scatter:
                        if (data is ValueTuple<double[], double[]> scatterTuple)
                        {
                            var scatter = plot.Add.Scatter(scatterTuple.Item1, scatterTuple.Item2);
                            plottable = scatter;
                        }
                        break;

                    case PlotType.HorizontalLine:
                        if (data is double yValue)
                        {
                            var hLine = plot.Add.HorizontalLine(yValue);
                            plottable = hLine;
                        }
                        break;

                    case PlotType.VerticalLine:
                        if (data is double xValue)
                        {
                            var vLine = plot.Add.VerticalLine(xValue);
                            plottable = vLine;
                        }
                        break;

                    default:
                        throw new NotImplementedException($"PlotType {plotType} henüz desteklenmiyor.");
                }

                if (plottable == null)
                    throw new ArgumentException($"Veri tipi {data?.GetType().Name} PlotType {plotType} ile uyumlu değil.");

                // PlotInfo oluştur ve sakla (index zorunlu)
                string plotId = $"{plotType}_{index}";
                var plotInfo = new PlotInfo(plotId, plotType.ToString(), plotType)
                {
                    Plottable = plottable
                };

                _plots[plotId] = plotInfo;

                return plottable;
            }
        }

        /// <summary>
        /// Plot'a veri ekler (plot index ile)
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="plotType">Veri tipi</param>
        /// <param name="data">Veri</param>
        /// <param name="dataIndex">Veri indeksi (zorunlu)</param>
        /// <returns>Oluşturulan plottable</returns>
        public IPlottable AddPlotData(int plotIndex, PlotType plotType, object data, int dataIndex)
        {
            var plot = GetPlot(plotIndex);
            return AddPlotData(plot, plotType, data, dataIndex);
        }

        /// <summary>
        /// Belirli plot'taki tüm data'ları iterate etmek için
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <returns>Plot'taki tüm PlotInfo'lar</returns>
        public List<PlotInfo> GetPlotDataList(int plotIndex)
        {
            lock (_lockObject)
            {
                return _plots.Values.Where(p => p.Plottable != null).ToList();
            }
        }

        /// <summary>
        /// Plot'taki belirli tipte data'ları döndür
        /// </summary>
        /// <param name="plotType">Aranacak PlotType</param>
        /// <returns>Bu tipte tüm PlotInfo'lar</returns>
        public List<PlotInfo> GetDataByType(PlotType plotType)
        {
            lock (_lockObject)
            {
                return _plots.Values.Where(p => p.PlotType == plotType).ToList();
            }
        }

        /// <summary>
        /// Belirli plotType ve index'teki plottable'ı döndür
        /// </summary>
        /// <param name="plotType">PlotType</param>
        /// <param name="index">Data index</param>
        /// <returns>Plottable</returns>
        public IPlottable? GetPlottable(PlotType plotType, int index)
        {
            lock (_lockObject)
            {
                string plotId = $"{plotType}_{index}";
                return _plots.TryGetValue(plotId, out var plotInfo) ? plotInfo.Plottable : null;
            }
        }

        /// <summary>
        /// Belirli plotType ve index'teki plottable'ı type-safe döndür
        /// </summary>
        /// <typeparam name="T">Plottable tipi</typeparam>
        /// <param name="plotType">PlotType</param>
        /// <param name="index">Data index</param>
        /// <returns>Typed plottable</returns>
        public T? GetPlottable<T>(PlotType plotType, int index) where T : class, IPlottable
        {
            var plottable = GetPlottable(plotType, index);
            return plottable as T;
        }

        public void Refresh()
        {
            if (_tradingChart == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            _tradingChart.Plot.Refresh();
        }

        /// <summary>
        /// Plot'u default boyutlarla initialize eder
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="useCandlestickHeight">Plot 0 (Candlestick) için true, diğerleri için false</param>
        public void InitializePlotSize(int plotIndex, bool useCandlestickHeight = false)
        {
            int height = useCandlestickHeight ? DefaultCandlestickHeight : DefaultIndicatorHeight;
            SetPlotSize(plotIndex, DefaultWidth, height);
        }

        /// <summary>
        /// Plot'un yüksekliğini ayarlar (genişlik default kalır)
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="height">Yükseklik</param>
        public void SetPlotHeightOnly(int plotIndex, int height)
        {
            SetPlotSize(plotIndex, DefaultWidth, height);
        }

        /// <summary>
        /// Belirtilen plot'un boyutunu ayarlar
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="width">Genişlik</param>
        /// <param name="height">Yükseklik</param>
        public void SetPlotSize(int plotIndex, int width, int height)
        {
            if (_formsPlot == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
                try
                {
                    // FormsPlot'un ana boyutunu ayarla
                    if (plotIndex == 0 || !_isInitialized)
                    {
                        _formsPlot.Size = new Size(width, height);
                        _formsPlot.Width = width;
                        _formsPlot.Height = height;
                    }
                    else
                    {
                        // Multiplot modunda bireysel plot boyutları ScottPlot tarafından otomatik yönetilir
                        // Bu durumda ana FormsPlot boyutunu ayarlayıp layout'u yenileriz
                        _formsPlot.Size = new Size(width, height);
                        FinalizeLayout();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting plot size: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirtilen plot'un genişliğini ayarlar
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="width">Genişlik</param>
        public void SetPlotWidth(int plotIndex, int width)
        {
            if (_formsPlot == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
                try
                {
                    var currentHeight = GetPlotHeight(plotIndex);
                    SetPlotSize(plotIndex, width, currentHeight);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting plot width: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirtilen plot'un yüksekliğini ayarlar
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="height">Yükseklik</param>
        public void SetPlotHeight(int plotIndex, int height)
        {
            if (_formsPlot == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
                try
                {
                    var currentWidth = GetPlotWidth(plotIndex);
                    SetPlotSize(plotIndex, currentWidth, height);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting plot height: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirtilen plot'un boyutunu döndürür
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <returns>Plot boyutu (Width, Height)</returns>
        public (int Width, int Height) GetPlotSize(int plotIndex)
        {
            if (_formsPlot == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            try
            {
                // FormsPlot'un boyutunu döndür (tüm plotlar için geçerli)
                return (_formsPlot.Width, _formsPlot.Height);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting plot size: {ex.Message}");
                return (800, 600); // Default boyut
            }
        }

        /// <summary>
        /// Belirtilen plot'un genişliğini döndürür
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <returns>Plot genişliği</returns>
        public int GetPlotWidth(int plotIndex)
        {
            return GetPlotSize(plotIndex).Width;
        }

        /// <summary>
        /// Belirtilen plot'un yüksekliğini döndürür
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <returns>Plot yüksekliği</returns>
        public int GetPlotHeight(int plotIndex)
        {
            return GetPlotSize(plotIndex).Height;
        }

        /// <summary>
        /// Tüm plotları belirtilen boyuta ayarlar
        /// </summary>
        /// <param name="width">Genişlik</param>
        /// <param name="height">Yükseklik</param>
        public void SetAllPlotsSize(int width, int height)
        {
            if (_formsPlot == null)
                throw new InvalidOperationException("TradingChart ayarlanmamış. Önce SetTradingChart() çağırın.");

            lock (_lockObject)
            {
                try
                {
                    _formsPlot.Size = new Size(width, height);
                    _formsPlot.Width = width;
                    _formsPlot.Height = height;
                    FinalizeLayout();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting all plots size: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Belirtilen plot index'ine ID atar
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="plotId">Plot ID'si (örn: "Candlestick", "Volume", "RSI")</param>
        public void SetPlotId(int plotIndex, string plotId)
        {
            lock (_lockObject)
            {
                _plotIds[plotIndex] = plotId ?? "Unknown";
            }
        }

        /// <summary>
        /// Belirtilen plot index'ine ID atar (integer ID)
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <param name="plotId">Plot ID'si (integer)</param>
        public void SetPlotId(int plotIndex, int plotId)
        {
            SetPlotId(plotIndex, plotId.ToString());
        }

        /// <summary>
        /// Plot nesnesine ID atar (string ID)
        /// </summary>
        /// <param name="plot">Plot nesnesi</param>
        /// <param name="plotId">Plot ID'si (string)</param>
        public void SetPlotId(ScottPlot.Plot plot, string plotId)
        {
            lock (_lockObject)
            {
                if (_plotToIndex.TryGetValue(plot, out int plotIndex))
                {
                    SetPlotId(plotIndex, plotId);
                }
                else
                {
                    throw new ArgumentException("Plot nesnesi PlotManager'da bulunamadı.");
                }
            }
        }

        /// <summary>
        /// Plot nesnesine ID atar (integer ID)
        /// </summary>
        /// <param name="plot">Plot nesnesi</param>
        /// <param name="plotId">Plot ID'si (integer)</param>
        public void SetPlotId(ScottPlot.Plot plot, int plotId)
        {
            SetPlotId(plot, plotId.ToString());
        }

        /// <summary>
        /// Belirtilen plot index'inin ID'sini döndürür
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <returns>Plot ID'si, bulunamazsa null</returns>
        public string? GetPlotId(int plotIndex)
        {
            lock (_lockObject)
            {
                return _plotIds.TryGetValue(plotIndex, out string? plotId) ? plotId : null;
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip plot'un index'ini döndürür
        /// </summary>
        /// <param name="plotId">Plot ID'si</param>
        /// <returns>Plot index'i, bulunamazsa -1</returns>
        public int GetPlotIndexById(string plotId)
        {
            lock (_lockObject)
            {
                foreach (var kvp in _plotIds)
                {
                    if (kvp.Value == plotId)
                        return kvp.Key;
                }
                return -1;
            }
        }

        /// <summary>
        /// Belirtilen plot ID'sinin "Candlestick" olup olmadığını kontrol eder
        /// </summary>
        /// <param name="plotIndex">Plot indeksi</param>
        /// <returns>Candlestick plot'u ise true</returns>
        public bool IsCandlestickPlot(int plotIndex)
        {
            string? plotId = GetPlotId(plotIndex);
            return plotId?.Equals("Candlestick", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
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
        private readonly object _lockObject = new object();
        private int _plotCounter = 0;
        private bool _isInitialized = false;

        public IReadOnlyDictionary<string, PlotInfo> Plots => _plots;

        public int Count => _plots.Count;

        public PlotManager()
        {
            _plots = new Dictionary<string, PlotInfo>();
        }

        public void SetTradingChart(TradingChart tradingChart)
        {
            _tradingChart = tradingChart ?? throw new ArgumentNullException(nameof(tradingChart));
            _formsPlot = _tradingChart.Plot ?? throw new ArgumentNullException(nameof(_tradingChart.Plot));
            _plotCounter = 0;
            _isInitialized = false;
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
                    // Multiplot'u tamamen temizle
                    _formsPlot.Reset();
                    
                    // Counter ve flag'leri sıfırla
                    _plotCounter = 0;
                    _isInitialized = false;
                    
                    // Plot bilgilerini temizle
                    _plots.Clear();
                    
                    // Chart'ı yenile
                    _formsPlot.Refresh();
                }
                catch (Exception ex)
                {
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
                // Dikey olarak daralt
                _formsPlot.Multiplot.CollapseVertically();
                
                // Chart'ı yenile
                _formsPlot.Refresh();
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
                    return _formsPlot.Multiplot.GetPlot(index);
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
                    return _formsPlot.Plot;
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
    }
}
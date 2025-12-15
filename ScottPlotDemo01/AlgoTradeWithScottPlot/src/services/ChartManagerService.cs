using AlgoTradeWithScottPlot.Models;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlgoTradeWithScottPlot.Services
{
    /// <summary>
    /// Veri katmanı ile UI katmanı arasında köprü görevi görür.
    /// Ham veriyi dinler, işler, çizime hazır hale getirir ve UI'ı güncellenmesi için bilgilendirir.
    /// </summary>
    public class ChartManagerService
    {
        private readonly TradingDataService _dataService;
        private readonly List<OHLCBar> _allData = new List<OHLCBar>();
        private readonly object _dataLock = new object();
        private readonly HashSet<string> _registeredCharts = new HashSet<string>();

        /// <summary>
        /// Bir grafik güncellenmesi gerektiğinde tetiklenir. UI katmanı bu olayı dinler.
        /// </summary>
        public event Action<PlotUpdateData> OnChartNeedsUpdate;

        public ChartManagerService(TradingDataService dataService)
        {
            _dataService = dataService;
            // Veri servisinden gelen yeni veri olayına abone ol.
            _dataService.OnNewDataPointGenerated += OnNewDataReceived;
        }

        /// <summary>
        /// Yönetilecek bir grafik tipini kaydeder. Sadece kayıtlı grafikler için hesaplama yapılır.
        /// </summary>
        public void RegisterChart(string plotId)
        {
            if (!_registeredCharts.Contains(plotId))
            {
                _registeredCharts.Add(plotId);
            }
        }

        /// <summary>
        /// TradingDataService'den yeni bir veri noktası geldiğinde tetiklenir.
        /// </summary>
        private async void OnNewDataReceived(object sender, OHLCEventArgs e)
        {
            lock (_dataLock)
            {
                _allData.Add(e.NewBar);
            }

            // Hesaplamaları ve olay tetiklemelerini UI thread'ini bloklamamak için
            // bir arka plan görevinde yap.
            await Task.Run(() =>
            {
                List<OHLCBar> currentData;
                lock (_dataLock)
                {
                    // Thread-safe bir şekilde verinin anlık kopyasını al.
                    currentData = new List<OHLCBar>(_allData);
                }

                // Kayıtlı her grafik için ilgili güncellemeyi oluştur ve gönder.
                if (_registeredCharts.Contains("Price"))
                {
                    ProcessPriceChart(currentData);
                }
                if (_registeredCharts.Contains("RSI"))
                {
                    ProcessRsiChart(currentData);
                }
                if (_registeredCharts.Contains("MACD"))
                {
                    ProcessMacdChart(currentData);
                }
            });
        }

        private void ProcessPriceChart(List<OHLCBar> currentData)
        {
            var priceUpdate = new PlotUpdateData
            {
                PlotId = "Price",
                CandlestickData = currentData.Select(b => new OHLC(b.Open, b.High, b.Low, b.Close, b.Timestamp, TimeSpan.FromMinutes(1))).ToArray()
            };
            OnChartNeedsUpdate?.Invoke(priceUpdate);
        }

        private void ProcessRsiChart(List<OHLCBar> currentData)
        {
            if (currentData.Count > 14)
            {
                var rsiData = TradingDataService.CalculateRSI(currentData, 14);
                var rsiUpdate = new PlotUpdateData
                {
                    PlotId = "RSI",
                    PrimaryLineData = rsiData,
                    AdditionalData = {
                        { "HorizontalLine-70", 70.0 },
                        { "HorizontalLine-30", 30.0 }
                    }
                };
                OnChartNeedsUpdate?.Invoke(rsiUpdate);
            }
        }

        private void ProcessMacdChart(List<OHLCBar> currentData)
        {
            if (currentData.Count > 26 + 9) // MACD için gerekli minimum bar sayısı
            {
                var (macdLine, signalLine, histogram) = TradingDataService.CalculateMACD(currentData);
                var macdUpdate = new PlotUpdateData
                {
                    PlotId = "MACD",
                    PrimaryLineData = macdLine,
                    SecondaryLineData = signalLine,
                    BarData = histogram,
                    AdditionalData = {
                        { "HorizontalLine-0", 0.0 }
                    }
                };
                OnChartNeedsUpdate?.Invoke(macdUpdate);
            }
        }
    }
}

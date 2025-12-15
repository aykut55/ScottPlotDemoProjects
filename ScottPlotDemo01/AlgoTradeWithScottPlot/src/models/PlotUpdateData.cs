using ScottPlot;
using System.Collections.Generic;

namespace AlgoTradeWithScottPlot.Models
{
    /// <summary>
    /// Bir grafiğin güncellenmesi için gereken tüm verileri taşıyan veri transfer nesnesi (DTO).
    /// Bu nesne, ChartManagerService tarafından oluşturulur ve UI katmanına gönderilir.
    /// </summary>
    public class PlotUpdateData
    {
        /// <summary>
        /// Hangi grafiğin güncelleneceğini belirten benzersiz kimlik (örn: "Price", "RSI").
        /// </summary>
        public string PlotId { get; set; }

        /// <summary>
        /// Candlestick grafikleri için ScottPlot'un beklediği formatta OHLC verisi.
        /// </summary>
        public OHLC[] CandlestickData { get; set; }

        /// <summary>
        /// Ana çizgi grafiği verisi (örn: RSI çizgisi, MACD çizgisi).
        /// </summary>
        public double[] PrimaryLineData { get; set; }

        /// <summary>
        /// İkincil çizgi grafiği verisi (örn: MACD sinyal çizgisi).
        /// </summary>
        public double[] SecondaryLineData { get; set; }

        /// <summary>
        /// Bar grafiği verisi (örn: MACD histogramı, Hacim).
        /// </summary>
        public double[] BarData { get; set; }

        /// <summary>
        /// Diğer özel veri türleri için esnek bir sözlük yapısı.
        /// Örneğin, yatay çizgiler, işaretçiler veya metin etiketleri bu yolla taşınabilir.
        /// Örnek: AdditionalData.Add("HorizontalLineValue", 80.0);
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
}

using System;

namespace AlgoTradeWithScottPlot.Models
{
    /// <summary>
    /// Tek bir OHLC (Open, High, Low, Close) barını temsil eden yapı.
    /// ScottPlot'un kendi OHLC tipi yerine daha esnek bir model kullanmak için oluşturulmuştur.
    /// </summary>
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

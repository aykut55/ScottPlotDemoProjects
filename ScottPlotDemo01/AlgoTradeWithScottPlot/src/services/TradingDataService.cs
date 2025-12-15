using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlgoTradeWithScottPlot.Models;

namespace AlgoTradeWithScottPlot.Services
{
    /// <summary>
    /// Statik DataGenerator sınıfının yerini alan, olay tabanlı ve servis odaklı veri üretici.
    /// Hem anlık (simüle) veri üretir hem de teknik indikatör hesaplamaları için metotlar barındırır.
    /// </summary>
    public class TradingDataService
    {
        private readonly List<OHLCBar> _data = new List<OHLCBar>();
        private readonly object _dataLock = new object();
        private System.Threading.Timer _timer;
        private readonly Random _random = new Random();
        private double _lastPrice = 100.0;

        /// <summary>
        /// Yeni bir OHLC barı üretildiğinde tetiklenir.
        /// </summary>
        public event EventHandler<OHLCEventArgs> OnNewDataPointGenerated;

        /// <summary>
        /// Belirtilen aralıklarla anlık veri üretimini başlatır.
        /// </summary>
        public void StartRealTimeGeneration(int intervalMilliseconds = 1000)
        {
            _timer = new System.Threading.Timer(GenerateNextBar, null, 0, intervalMilliseconds);
        }

        /// <summary>
        /// Veri üretimini durdurur.
        /// </summary>
        public void StopGeneration()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        /// <summary>
        /// Simüle edilmiş yeni bir OHLC barı üretir ve olayı tetikler.
        /// </summary>
        private void GenerateNextBar(object state)
        {
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

            lock (_dataLock)
            {
                _data.Add(newBar);
            }

            // Olayı tetikle
            OnNewDataPointGenerated?.Invoke(this, new OHLCEventArgs(newBar));
        }

        #region Indicator Calculation Methods

        // Bu metotlar, eski DataGenerator sınıfından alınmış ve yeni OHLCBar modeline uyarlanmıştır.
        // Hesaplamaların doğruluğu ve verimliliği için daha gelişmiş kütüphaneler (örn: Skender.Stock.Indicators)
        // kullanılması ileride düşünülebilir.

        public static double[] CalculateSMA(List<OHLCBar> data, int period)
        {
            double[] prices = data.Select(b => b.Close).ToArray();
            double[] sma = new double[prices.Length];
            if (period > prices.Length) return sma;

            for (int i = period - 1; i < prices.Length; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += prices[i - j];
                }
                sma[i] = sum / period;
            }
            return sma;
        }

        public static double[] CalculateEMA(List<OHLCBar> data, int period)
        {
            double[] prices = data.Select(b => b.Close).ToArray();
            double[] ema = new double[prices.Length];
            if (period > prices.Length) return ema;

            double multiplier = 2.0 / (period + 1);
            double initialSma = prices.Take(period).Average();
            ema[period - 1] = initialSma;

            for (int i = period; i < prices.Length; i++)
            {
                ema[i] = (prices[i] - ema[i - 1]) * multiplier + ema[i - 1];
            }
            return ema;
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

        public static (double[] macdLine, double[] signalLine, double[] histogram) CalculateMACD(List<OHLCBar> data, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            double[] prices = data.Select(b => b.Close).ToArray();
            double[] macdLine = new double[prices.Length];
            double[] signalLine = new double[prices.Length];
            double[] histogram = new double[prices.Length];

            if (data.Count < slowPeriod) return (macdLine, signalLine, histogram);

            double[] fastEma = CalculateEMA(data, fastPeriod);
            double[] slowEma = CalculateEMA(data, slowPeriod);

            for (int i = slowPeriod - 1; i < prices.Length; i++)
            {
                macdLine[i] = fastEma[i] - slowEma[i];
            }

            // MACD'nin EMA'sını (Sinyal Çizgisi) hesapla
            double signalMultiplier = 2.0 / (signalPeriod + 1);
            int signalStartIndex = slowPeriod - 1 + signalPeriod - 1;
            if (signalStartIndex >= macdLine.Length) return (macdLine, signalLine, histogram);

            double initialSignalSma = macdLine.Skip(slowPeriod - 1).Take(signalPeriod).Average();
            signalLine[signalStartIndex] = initialSignalSma;

            for (int i = signalStartIndex + 1; i < macdLine.Length; i++)
            {
                signalLine[i] = (macdLine[i] - signalLine[i - 1]) * signalMultiplier + signalLine[i - 1];
            }

            // Histogramı hesapla
            for (int i = signalStartIndex; i < prices.Length; i++)
            {
                histogram[i] = macdLine[i] - signalLine[i];
            }

            return (macdLine, signalLine, histogram);
        }

        #endregion
    }
}

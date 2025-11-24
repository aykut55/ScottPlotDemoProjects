using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgoTradeWithScottPlot
{
    /// <summary>
    /// Trading verileri için veri üretici sınıf
    /// OHLCV bar verileri, teknik indikatör verileri ve test verileri üretir
    /// </summary>
    public static class DataGenerator
    {
        private static readonly Random _random = new Random();

        // Global Y limits for generated data
        private static double _globalYMin = double.MaxValue;
        private static double _globalYMax = double.MinValue;

        /// <summary>
        /// Global Y minimum değerini döndürür (son GenerateOHLCs çağrısından)
        /// </summary>
        public static double GlobalYMin => _globalYMin;

        /// <summary>
        /// Global Y maximum değerini döndürür (son GenerateOHLCs çağrısından)
        /// </summary>
        public static double GlobalYMax => _globalYMax;

        /// <summary>
        /// Hızlı OHLC array verisi üretir (performans için optimize edilmiş)
        /// </summary>
        /// <param name="count">Üretilecek bar sayısı</param>
        /// <returns>OHLC array</returns>
        public static OHLC[] GenerateOHLCs(int count)
        {
            System.Diagnostics.Debug.WriteLine($"[GenerateOHLCs] Starting generation of {count:N0} points...");
            var sw = System.Diagnostics.Stopwatch.StartNew();

            OHLC[] ohlcs = new OHLC[count];
            DateTime startDate = new DateTime(2020, 1, 1);
            double open = 100;
            Random rand = new Random(0);

            for (int i = 0; i < count; i++)
            {
                double change = (rand.NextDouble() - 0.5) * 2;
                double close = open + change;
                double high = Math.Max(open, close) + rand.NextDouble();
                double low = Math.Min(open, close) - rand.NextDouble();

                high = Math.Max(high, Math.Max(open, close));
                low = Math.Min(low, Math.Min(open, close));

                TimeSpan timeSpan = TimeSpan.FromMinutes(1);
                DateTime time = startDate + (timeSpan * i);

                // Use real time for proper date display on X-axis
                ohlcs[i] = new OHLC(open, high, low, close, time, timeSpan);
                open = close;
            }

            // Calculate Global Y Limits
            _globalYMin = double.MaxValue;
            _globalYMax = double.MinValue;
            foreach (var o in ohlcs)
            {
                if (o.Low < _globalYMin) _globalYMin = o.Low;
                if (o.High > _globalYMax) _globalYMax = o.High;
            }
            // Add some padding to global limits
            double padding = (_globalYMax - _globalYMin) * 0.05;
            _globalYMin -= padding;
            _globalYMax += padding;

            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"[GenerateOHLCs] Completed in {sw.ElapsedMilliseconds}ms");
            return ohlcs;
        }

        /// <summary>
        /// Random OHLC bar verisi üretir
        /// </summary>
        /// <param name="count">Üretilecek bar sayısı</param>
        /// <param name="startPrice">Başlangıç fiyatı</param>
        /// <param name="volatility">Volatilite (fiyat değişim yüzdesi)</param>
        /// <returns>OHLC bar listesi</returns>
        public static List<ScottPlot.OHLC> GenerateOHLC(int count, double startPrice = 100, double volatility = 0.02)
        {
            var ohlcList = new List<ScottPlot.OHLC>();
            double price = startPrice;
            DateTime startDate = DateTime.Now.AddDays(-count);

            for (int i = 0; i < count; i++)
            {
                double open = price;
                double change = ((_random.NextDouble() - 0.5) * 2) * startPrice * volatility;
                double close = open + change;

                double high = Math.Max(open, close) + (_random.NextDouble() * startPrice * volatility * 0.5);
                double low = Math.Min(open, close) - (_random.NextDouble() * startPrice * volatility * 0.5);

                var ohlc = new ScottPlot.OHLC(open, high, low, close, startDate.AddDays(i), TimeSpan.FromDays(1));
                ohlcList.Add(ohlc);

                price = close;
            }

            return ohlcList;
        }

        /// <summary>
        /// Random volume verisi üretir
        /// </summary>
        /// <param name="count">Veri sayısı</param>
        /// <param name="minVolume">Minimum hacim</param>
        /// <param name="maxVolume">Maksimum hacim</param>
        /// <returns>Volume dizisi</returns>
        public static double[] GenerateVolume(int count, double minVolume = 1000000, double maxVolume = 10000000)
        {
            double[] volumes = new double[count];
            for (int i = 0; i < count; i++)
            {
                volumes[i] = minVolume + (_random.NextDouble() * (maxVolume - minVolume));
            }
            return volumes;
        }

        /// <summary>
        /// Simple Moving Average (SMA) hesaplar
        /// </summary>
        /// <param name="prices">Fiyat dizisi</param>
        /// <param name="period">Periyot</param>
        /// <returns>SMA dizisi (başlangıçta 0)</returns>
        public static double[] CalculateSMA(double[] prices, int period)
        {
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

        /// <summary>
        /// Exponential Moving Average (EMA) hesaplar
        /// </summary>
        /// <param name="prices">Fiyat dizisi</param>
        /// <param name="period">Periyot</param>
        /// <returns>EMA dizisi (başlangıçta 0)</returns>
        public static double[] CalculateEMA(double[] prices, int period)
        {
            double[] ema = new double[prices.Length];
            double multiplier = 2.0 / (period + 1);

            // İlk EMA değeri SMA ile başla
            double sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += prices[i];
            }
            ema[period - 1] = sum / period;

            // Önceki değerler 0
            for (int i = 0; i < period - 1; i++)
            {
                ema[i] = 0; // NaN yerine 0
            }

            // EMA hesapla
            for (int i = period; i < prices.Length; i++)
            {
                ema[i] = (prices[i] - ema[i - 1]) * multiplier + ema[i - 1];
            }

            return ema;
        }

        /// <summary>
        /// RSI (Relative Strength Index) hesaplar
        /// </summary>
        /// <param name="prices">Fiyat dizisi</param>
        /// <param name="period">Periyot (genellikle 14)</param>
        /// <returns>RSI dizisi (0-100 arası, başlangıçta 0)</returns>
        public static double[] CalculateRSI(double[] prices, int period = 14)
        {
            double[] rsi = new double[prices.Length];

            if (prices.Length < period + 1)
            {
                // Tüm değerler 0
                return rsi; // Zaten 0 ile initialize edildi
            }

            double[] gains = new double[prices.Length];
            double[] losses = new double[prices.Length];

            // İlk değişimleri hesapla
            for (int i = 1; i < prices.Length; i++)
            {
                double change = prices[i] - prices[i - 1];
                gains[i] = change > 0 ? change : 0;
                losses[i] = change < 0 ? -change : 0;
            }

            // İlk ortalama kazanç ve kayıp
            double avgGain = gains.Skip(1).Take(period).Average();
            double avgLoss = losses.Skip(1).Take(period).Average();

            // İlk RSI - önceki değerler 0 kalacak (default)
            if (avgLoss == 0)
            {
                rsi[period] = 100;
            }
            else
            {
                double rs = avgGain / avgLoss;
                rsi[period] = 100 - (100 / (1 + rs));
            }

            // Sonraki RSI değerleri
            for (int i = period + 1; i < prices.Length; i++)
            {
                avgGain = (avgGain * (period - 1) + gains[i]) / period;
                avgLoss = (avgLoss * (period - 1) + losses[i]) / period;

                if (avgLoss == 0)
                {
                    rsi[i] = 100;
                }
                else
                {
                    double rs = avgGain / avgLoss;
                    rsi[i] = 100 - (100 / (1 + rs));
                }
            }

            return rsi;
        }

        /// <summary>
        /// MACD (Moving Average Convergence Divergence) hesaplar
        /// </summary>
        /// <param name="prices">Fiyat dizisi</param>
        /// <param name="fastPeriod">Hızlı EMA periyodu (genellikle 12)</param>
        /// <param name="slowPeriod">Yavaş EMA periyodu (genellikle 26)</param>
        /// <param name="signalPeriod">Sinyal çizgisi periyodu (genellikle 9)</param>
        /// <returns>(MACD çizgisi, Sinyal çizgisi, Histogram) tuple (başlangıçta 0)</returns>
        public static (double[] macdLine, double[] signalLine, double[] histogram) CalculateMACD(
            double[] prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            double[] fastEMA = CalculateEMA(prices, fastPeriod);
            double[] slowEMA = CalculateEMA(prices, slowPeriod);

            double[] macdLine = new double[prices.Length];
            for (int i = 0; i < prices.Length; i++)
            {
                // EMA artık 0 döndürüyor, direkt hesapla
                macdLine[i] = fastEMA[i] - slowEMA[i];
            }

            double[] signalLine = CalculateEMA(macdLine, signalPeriod);

            double[] histogram = new double[prices.Length];
            for (int i = 0; i < prices.Length; i++)
            {
                histogram[i] = macdLine[i] - signalLine[i];
            }

            return (macdLine, signalLine, histogram);
        }

        /// <summary>
        /// Bollinger Bands hesaplar
        /// </summary>
        /// <param name="prices">Fiyat dizisi</param>
        /// <param name="period">Periyot (genellikle 20)</param>
        /// <param name="stdDevMultiplier">Standart sapma çarpanı (genellikle 2)</param>
        /// <returns>(Üst band, Orta band (SMA), Alt band) tuple (başlangıçta 0)</returns>
        public static (double[] upper, double[] middle, double[] lower) CalculateBollingerBands(
            double[] prices, int period = 20, double stdDevMultiplier = 2.0)
        {
            double[] middle = CalculateSMA(prices, period);
            double[] upper = new double[prices.Length];
            double[] lower = new double[prices.Length];

            for (int i = 0; i < prices.Length; i++)
            {
                if (i < period - 1)
                {
                    upper[i] = 0; // NaN yerine 0
                    lower[i] = 0; // NaN yerine 0
                    continue;
                }

                // Standart sapma hesapla
                double sum = 0;
                for (int j = 0; j < period; j++)
                {
                    double diff = prices[i - j] - middle[i];
                    sum += diff * diff;
                }
                double stdDev = Math.Sqrt(sum / period);

                upper[i] = middle[i] + (stdDev * stdDevMultiplier);
                lower[i] = middle[i] - (stdDev * stdDevMultiplier);
            }

            return (upper, middle, lower);
        }

        /// <summary>
        /// Stochastic Oscillator hesaplar
        /// </summary>
        /// <param name="highs">Yüksek fiyat dizisi</param>
        /// <param name="lows">Düşük fiyat dizisi</param>
        /// <param name="closes">Kapanış fiyat dizisi</param>
        /// <param name="period">Periyot (genellikle 14)</param>
        /// <param name="smoothK">%K smooth periyodu (genellikle 3)</param>
        /// <param name="smoothD">%D periyodu (genellikle 3)</param>
        /// <returns>(%K çizgisi, %D çizgisi) tuple (başlangıçta 0)</returns>
        public static (double[] k, double[] d) CalculateStochastic(
            double[] highs, double[] lows, double[] closes,
            int period = 14, int smoothK = 3, int smoothD = 3)
        {
            double[] fastK = new double[closes.Length];

            for (int i = 0; i < closes.Length; i++)
            {
                if (i < period - 1)
                {
                    fastK[i] = 0; // NaN yerine 0
                    continue;
                }

                double highestHigh = highs.Skip(i - period + 1).Take(period).Max();
                double lowestLow = lows.Skip(i - period + 1).Take(period).Min();

                if (highestHigh == lowestLow)
                {
                    fastK[i] = 50;
                }
                else
                {
                    fastK[i] = 100 * (closes[i] - lowestLow) / (highestHigh - lowestLow);
                }
            }

            // %K smooth
            double[] k = CalculateSMA(fastK, smoothK);

            // %D = %K'nın SMA'sı
            double[] d = CalculateSMA(k, smoothD);

            return (k, d);
        }

        /// <summary>
        /// Random walk ile test verisi üretir
        /// </summary>
        /// <param name="count">Veri sayısı</param>
        /// <param name="start">Başlangıç değeri</param>
        /// <param name="step">Adım büyüklüğü</param>
        /// <returns>Random walk dizisi</returns>
        public static double[] GenerateRandomWalk(int count, double start = 50, double step = 5)
        {
            double[] data = new double[count];
            data[0] = start;

            for (int i = 1; i < count; i++)
            {
                data[i] = data[i - 1] + ((_random.NextDouble() - 0.5) * 2 * step);
            }

            return data;
        }

        /// <summary>
        /// Belirli aralıkta random sayı dizisi üretir
        /// </summary>
        /// <param name="count">Veri sayısı</param>
        /// <param name="min">Minimum değer</param>
        /// <param name="max">Maksimum değer</param>
        /// <returns>Random sayı dizisi</returns>
        public static double[] GenerateRandomValues(int count, double min, double max)
        {
            double[] data = new double[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = min + (_random.NextDouble() * (max - min));
            }
            return data;
        }

        /// <summary>
        /// SMA kesişimlerinden alım/satım sinyalleri üretir
        /// </summary>
        /// <param name="smaFast">Hızlı SMA dizisi</param>
        /// <param name="smaSlow">Yavaş SMA dizisi</param>
        /// <param name="takeProfitPercent">Kar alma yüzdesi (örn: 5 = %5, null = kullanma)</param>
        /// <param name="stopLossPercent">Zarar kesme yüzdesi (örn: -3 = %3, null = kullanma)</param>
        /// <returns>(Sinyal dizisi: 1=Long, -1=Short, 0=Flat, Giriş fiyatları, Çıkış fiyatları)</returns>
        public static (int[] signals, double[] entryPrices, double[] exitPrices) CalculateTradingSignals(
            double[] smaFast, double[] smaSlow, double? takeProfitPercent = null, double? stopLossPercent = null)
        {
            int count = Math.Min(smaFast.Length, smaSlow.Length);
            int[] signals = new int[count];
            double[] entryPrices = new double[count];
            double[] exitPrices = new double[count];

            // Position states: isLong, isShort, isFlat
            bool isLong = false;
            bool isShort = false;
            bool isFlat = true; // Başlangıçta flat
            double entryPrice = 0;

            // TP/SL tanımlı mı?
            bool useTpSl = takeProfitPercent.HasValue && stopLossPercent.HasValue;

            for (int i = 1; i < count; i++)
            {
                // 0 değer kontrolü (SMA henüz hesaplanmamış)
                if (smaFast[i] == 0 || smaSlow[i] == 0 || smaFast[i - 1] == 0 || smaSlow[i - 1] == 0)
                {
                    signals[i] = isLong ? 1 : (isShort ? -1 : 0);
                    continue;
                }

                double currentPrice = smaFast[i];

                // TP/SL kontrolü (eğer tanımlıysa)
                if (useTpSl && (isLong || isShort))
                {
                    double pnlPercent = ((currentPrice - entryPrice) / entryPrice) * 100;

                    if (isLong)
                    {
                        // Long pozisyon - kar al veya zarar kes → FLAT OL
                        if (pnlPercent >= takeProfitPercent.Value || pnlPercent <= stopLossPercent.Value)
                        {
                            signals[i] = 0; // Flat ol
                            exitPrices[i] = currentPrice;
                            isLong = false;
                            isFlat = true;
                            continue;
                        }
                    }
                    else if (isShort)
                    {
                        // Short pozisyon - kar al veya zarar kes → FLAT OL (ters hesaplama)
                        if (pnlPercent <= -takeProfitPercent.Value || pnlPercent >= -stopLossPercent.Value)
                        {
                            signals[i] = 0; // Flat ol
                            exitPrices[i] = currentPrice;
                            isShort = false;
                            isFlat = true;
                            continue;
                        }
                    }
                }

                // Golden Cross (Hızlı SMA, yavaş SMA'yı yukarı kesiyor)
                if (smaFast[i - 1] <= smaSlow[i - 1] && smaFast[i] > smaSlow[i])
                {
                    if (!isLong) // Zaten long değilse
                    {
                        if (isShort)
                        {
                            // Short'tan Long'a geçiş
                            exitPrices[i] = currentPrice;
                        }

                        signals[i] = 1; // Long aç
                        entryPrice = currentPrice;
                        entryPrices[i] = entryPrice;
                        isLong = true;
                        isShort = false;
                        isFlat = false;
                    }
                    else
                    {
                        signals[i] = 1; // Long devam
                    }
                }
                // Death Cross (Hızlı SMA, yavaş SMA'yı aşağı kesiyor)
                else if (smaFast[i - 1] >= smaSlow[i - 1] && smaFast[i] < smaSlow[i])
                {
                    if (!isShort) // Zaten short değilse
                    {
                        if (isLong)
                        {
                            // Long'tan Short'a geçiş
                            exitPrices[i] = currentPrice;
                        }

                        signals[i] = -1; // Short aç
                        entryPrice = currentPrice;
                        entryPrices[i] = entryPrice;
                        isShort = true;
                        isLong = false;
                        isFlat = false;
                    }
                    else
                    {
                        signals[i] = -1; // Short devam
                    }
                }
                else
                {
                    // Kesişim yok - mevcut durumu koru
                    signals[i] = isLong ? 1 : (isShort ? -1 : 0);
                }
            }

            return (signals, entryPrices, exitPrices);
        }

        /// <summary>
        /// Trading sinyallerinden kar/zarar hesaplar (3 ayrı değer)
        /// </summary>
        /// <param name="signals">Sinyal dizisi (1=Buy, -1=Sell, 0=Hold)</param>
        /// <param name="entryPrices">Giriş fiyatları</param>
        /// <param name="exitPrices">Çıkış fiyatları</param>
        /// <param name="prices">Gerçek fiyat dizisi</param>
        /// <returns>(Anlık PnL, Gerçekleşen PnL, Kümülatif Total PnL)</returns>
        public static (double[] unrealized, double[] realized, double[] cumulative) CalculatePnL(
            int[] signals, double[] entryPrices, double[] exitPrices, double[] prices)
        {
            double[] unrealizedPnL = new double[signals.Length];
            double[] realizedPnL = new double[signals.Length];
            double[] cumulativePnL = new double[signals.Length];

            double totalRealizedPnL = 0; // Tüm kapatılmış pozisyonların toplam kar/zararı
            double lastTradePnL = 0; // Son kapatılan pozisyonun kar/zararı
            double? openPosition = null;
            bool isLong = false;

            for (int i = 0; i < signals.Length; i++)
            {
                if (signals[i] == 1) // Buy signal
                {
                    if (openPosition.HasValue && !isLong)
                    {
                        // Close short position
                        double exitPrice = exitPrices[i] > 0 ? exitPrices[i] : prices[i];
                        double positionPnL = openPosition.Value - exitPrice;
                        totalRealizedPnL += positionPnL;
                        lastTradePnL = positionPnL; // Son işlem kar/zararı
                        openPosition = null;
                    }
                    else if (!openPosition.HasValue)
                    {
                        // Open long position
                        openPosition = entryPrices[i] > 0 ? entryPrices[i] : prices[i];
                        isLong = true;
                    }
                }
                else if (signals[i] == -1) // Sell signal
                {
                    if (openPosition.HasValue && isLong)
                    {
                        // Close long position
                        double exitPrice = exitPrices[i] > 0 ? exitPrices[i] : prices[i];
                        double positionPnL = exitPrice - openPosition.Value;
                        totalRealizedPnL += positionPnL;
                        lastTradePnL = positionPnL; // Son işlem kar/zararı
                        openPosition = null;
                    }
                    else if (!openPosition.HasValue)
                    {
                        // Open short position
                        openPosition = entryPrices[i] > 0 ? entryPrices[i] : prices[i];
                        isLong = false;
                    }
                }

                // 1. Anlık (unrealized) PnL - Açık pozisyonun şu anki kar/zararı
                double currentUnrealizedPnL = 0;
                if (openPosition.HasValue)
                {
                    if (isLong)
                    {
                        // Long pozisyon: mevcut fiyat - giriş fiyatı
                        currentUnrealizedPnL = prices[i] - openPosition.Value;
                    }
                    else
                    {
                        // Short pozisyon: giriş fiyatı - mevcut fiyat
                        currentUnrealizedPnL = openPosition.Value - prices[i];
                    }
                }

                // 2. Gerçekleşen (realized) PnL - Son kapatılan işlemin kar/zararı
                // (Pozisyon açıkken önceki değeri taşır)

                // 3. Kümülatif (cumulative) PnL - Tüm realized + şu anki unrealized
                unrealizedPnL[i] = currentUnrealizedPnL;
                realizedPnL[i] = lastTradePnL;
                cumulativePnL[i] = totalRealizedPnL + currentUnrealizedPnL;
            }

            return (unrealizedPnL, realizedPnL, cumulativePnL);
        }

        /// <summary>
        /// Başlangıç sermayesi ve PnL'den hesap bakiyesini hesaplar
        /// </summary>
        /// <param name="initialBalance">Başlangıç sermayesi</param>
        /// <param name="pnl">Kar/zarar dizisi</param>
        /// <returns>Bakiye dizisi</returns>
        public static double[] CalculateBalance(double initialBalance, double[] pnl)
        {
            double[] balance = new double[pnl.Length];
            for (int i = 0; i < pnl.Length; i++)
            {
                balance[i] = initialBalance + pnl[i];
            }
            return balance;
        }
    }
}


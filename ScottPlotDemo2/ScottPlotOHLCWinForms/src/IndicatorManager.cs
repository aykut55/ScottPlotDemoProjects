using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;

namespace ScottPlotOHLCWinForms
{
    public static class IndicatorManager
    {
        public static double[] CalculateSMA(OHLC[] data, int period)
        {
            double[] sma = new double[data.Length];
            double sum = 0;

            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i].Close;
                if (i >= period)
                {
                    sum -= data[i - period].Close;
                    sma[i] = sum / period;
                }
                else if (i >= period - 1)
                {
                    sma[i] = sum / period;
                }
                else
                {
                    sma[i] = double.NaN; // Not enough data
                }
            }
            return sma;
        }

        public static (double[] macdLine, double[] signalLine, double[] histogram) CalculateMACD(OHLC[] data, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            double[] fastEma = CalculateEMA(data, fastPeriod);
            double[] slowEma = CalculateEMA(data, slowPeriod);
            double[] macdLine = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                if (!double.IsNaN(fastEma[i]) && !double.IsNaN(slowEma[i]))
                {
                    macdLine[i] = fastEma[i] - slowEma[i];
                }
                else
                {
                    macdLine[i] = double.NaN;
                }
            }

            double[] signalLine = CalculateEMA(macdLine, signalPeriod);
            double[] histogram = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                if (!double.IsNaN(macdLine[i]) && !double.IsNaN(signalLine[i]))
                {
                    histogram[i] = macdLine[i] - signalLine[i];
                }
                else
                {
                    histogram[i] = double.NaN;
                }
            }

            return (macdLine, signalLine, histogram);
        }

        public static double[] CalculateRSI(OHLC[] data, int period = 14)
        {
            double[] rsi = new double[data.Length];
            double avgGain = 0;
            double avgLoss = 0;

            for (int i = 1; i < data.Length; i++)
            {
                double change = data[i].Close - data[i - 1].Close;
                double gain = change > 0 ? change : 0;
                double loss = change < 0 ? -change : 0;

                if (i <= period)
                {
                    avgGain += gain;
                    avgLoss += loss;
                    if (i == period)
                    {
                        avgGain /= period;
                        avgLoss /= period;
                        rsi[i] = 100 - (100 / (1 + (avgGain / (avgLoss == 0 ? 1 : avgLoss))));
                    }
                    else
                    {
                        rsi[i] = double.NaN;
                    }
                }
                else
                {
                    avgGain = (avgGain * (period - 1) + gain) / period;
                    avgLoss = (avgLoss * (period - 1) + loss) / period;
                    
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
            }
            
            // Fill first period with NaN
            for(int i=0; i<period && i<rsi.Length; i++) rsi[i] = double.NaN;

            return rsi;
        }

        private static double[] CalculateEMA(OHLC[] data, int period)
        {
            double[] prices = data.Select(x => x.Close).ToArray();
            return CalculateEMA(prices, period);
        }

        private static double[] CalculateEMA(double[] data, int period)
        {
            double[] ema = new double[data.Length];
            double multiplier = 2.0 / (period + 1);

            // Initial SMA
            double sum = 0;
            for (int i = 0; i < period && i < data.Length; i++)
            {
                sum += data[i];
                if (!double.IsNaN(data[i]))
                    ema[i] = double.NaN; 
            }
            
            if (data.Length >= period)
            {
                double sma = sum / period;
                ema[period - 1] = sma;

                for (int i = period; i < data.Length; i++)
                {
                    if (double.IsNaN(data[i]))
                    {
                        ema[i] = double.NaN;
                        continue;
                    }
                    
                    // Handle previous NaN
                    if (double.IsNaN(ema[i-1]))
                    {
                         // Restart? For simplicity, just continue NaN or try to restart if we have enough data.
                         // Here we assume continuous data after start.
                         ema[i] = data[i]; 
                    }
                    else
                    {
                        ema[i] = (data[i] - ema[i - 1]) * multiplier + ema[i - 1];
                    }
                }
            }

            return ema;
        }
    }
}

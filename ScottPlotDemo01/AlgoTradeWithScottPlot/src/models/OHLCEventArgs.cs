using System;
using AlgoTradeWithScottPlot.Models;

namespace AlgoTradeWithScottPlot.Models
{
    /// <summary>
    /// Yeni bir OHLC barı üretildiğinde tetiklenen olaylar için veri taşır.
    /// </summary>
    public class OHLCEventArgs : EventArgs
    {
        public OHLCBar NewBar { get; }

        public OHLCEventArgs(OHLCBar newBar)
        {
            NewBar = newBar;
        }
    }
}

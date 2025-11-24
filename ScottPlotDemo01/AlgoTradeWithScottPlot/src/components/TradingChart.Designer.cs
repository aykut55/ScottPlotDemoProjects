namespace AlgoTradeWithScottPlot.Components
{
    partial class TradingChart
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private ScottPlot.WinForms.FormsPlot formsPlot;
        private HScrollBar hScrollBar1;
        private VScrollBar vScrollBar;
    }
}
using ScottPlot;
using ScottPlot.WinForms;
using System.Drawing;
using System.Windows.Forms;

namespace ScottPlotOHLCWinForms;

public class MultiplotDraggable : Form
{
    private readonly FormsPlot formsPlot1;
    private readonly Button btnAddRow;
    private readonly Button btnDeleteRow;

    public MultiplotDraggable()
    {
        // Initialize UI components programmatically
        this.Text = "Multiplot with Draggable Subplots";
        this.Size = new Size(800, 600);

        var panelTop = new Panel { Dock = DockStyle.Top, Height = 40 };
        btnAddRow = new Button { Text = "Add Row", Location = new Point(10, 8), AutoSize = true };
        btnDeleteRow = new Button { Text = "Delete Row", Location = new Point(100, 8), AutoSize = true };
        panelTop.Controls.Add(btnAddRow);
        panelTop.Controls.Add(btnDeleteRow);
        this.Controls.Add(panelTop);

        formsPlot1 = new FormsPlot { Dock = DockStyle.Fill };
        this.Controls.Add(formsPlot1);
        this.Controls.SetChildIndex(panelTop, 0); // Ensure panel is on top

        // Setup Multiplot Logic
        SetupMultiplot();
    }

    private void SetupMultiplot()
    {
        // setup a multiplot with 3 subplots
        formsPlot1.Multiplot.AddPlots(3);

        // set padding so there is no space between the middle and adjacent plots
        formsPlot1.Multiplot.CollapseVertically();

        // add sample price data to the first plot
        Plot pricePlot = formsPlot1.Multiplot.GetPlot(0);
        pricePlot.Axes.Right.Label.Text = "Price";
        List<OHLC> ohlcs = Generate.RandomOHLCs(50);
        var candlestick = pricePlot.Add.Candlestick(ohlcs);
        candlestick.Axes.YAxis = pricePlot.Axes.Right;
        candlestick.Sequential = true;

        // add sample RSI data to the second plot
        Plot rsiPlot = formsPlot1.Multiplot.GetPlot(1);
        rsiPlot.Axes.Right.Label.Text = "RSI";
        double[] rsiValues = Generate.RandomWalk(ohlcs.Count);
        var rsiSig = rsiPlot.Add.Signal(rsiValues);
        rsiSig.Axes.YAxis = rsiPlot.Axes.Right;
        rsiSig.LineWidth = 2;

        // add sample volume data to the third plot
        Plot volumePlot = formsPlot1.Multiplot.GetPlot(2);
        volumePlot.Axes.Right.Label.Text = "Volume";
        double[] volumes = Generate.RandomSample(50, 10, 90);
        var bars = volumePlot.Add.Bars(volumes);
        bars.Axes.YAxis = volumePlot.Axes.Right;
        volumePlot.Axes.Margins(bottom: 0);

        // use the same size for all right axes to ensure alignment regardless of tick label length
        foreach (Plot plot in formsPlot1.Multiplot.GetPlots())
        {
            plot.Axes.Left.LockSize(10);
            plot.Axes.Right.LockSize(80);
        }

        // update grids to use ticks from the bottom plot
        pricePlot.Grid.XAxis = volumePlot.Axes.Bottom;
        rsiPlot.Grid.XAxis = volumePlot.Axes.Bottom;

        // update grids to use ticks from the right axis
        pricePlot.Grid.YAxis = pricePlot.Axes.Right;
        rsiPlot.Grid.YAxis = rsiPlot.Axes.Right;
        volumePlot.Grid.YAxis = volumePlot.Axes.Right;

        // link horizontal axes across all plots
        formsPlot1.Multiplot.SharedAxes.ShareX([pricePlot, rsiPlot, volumePlot]);

        // use custom logic to tell the multiplot how large to make each plot
        ScottPlot.MultiplotLayouts.DraggableRows customLayout = new();
        formsPlot1.Multiplot.Layout = customLayout;

        // set the initial heights for each plot
        customLayout.SetHeights([600, 100, 100]);

        // wire mouse move events to allow dragging dividers between plots
        int? dividerBeingDragged = null;

        formsPlot1.MouseDown += (s, e) =>
        {
            dividerBeingDragged = customLayout.GetDivider(e.Y);
            formsPlot1.UserInputProcessor.IsEnabled = dividerBeingDragged is null;
        };

        formsPlot1.MouseUp += (s, e) =>
        {
            if (dividerBeingDragged is not null)
            {
                dividerBeingDragged = null;
                formsPlot1.UserInputProcessor.IsEnabled = true;
            }
        };

        formsPlot1.MouseMove += (s, e) =>
        {
            if (dividerBeingDragged is not null)
            {
                customLayout.SetDivider(dividerBeingDragged.Value, e.Y);
                formsPlot1.Refresh();
            }

            Cursor = customLayout.GetDivider(e.Y) is not null ? Cursors.SizeNS : Cursors.Default;
        };

        btnAddRow.Click += (s, e) =>
        {
            Plot plot = formsPlot1.Multiplot.AddPlot();
            plot.Axes.Left.LockSize(10);
            plot.Axes.Right.LockSize(80);
            formsPlot1.Multiplot.CollapseVertically();
            formsPlot1.Refresh();
        };

        btnDeleteRow.Click += (s, e) =>
        {
            if (formsPlot1.Multiplot.Subplots.Count < 2)
                return;

            Plot plotToRemove = formsPlot1.Multiplot.Subplots.GetPlots().Last();
            formsPlot1.Multiplot.RemovePlot(plotToRemove);

            // revert the collapse of the lower edge of the new bottom plot and use the original tick generator
            Plot newBottomPlot = formsPlot1.Multiplot.Subplots.GetPlots().Last();
            newBottomPlot.Axes.Bottom.ResetSize();
            newBottomPlot.Axes.Bottom.TickGenerator = plotToRemove.Axes.Bottom.TickGenerator;

            formsPlot1.Refresh();
        };
    }
}

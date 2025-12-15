using ScottPlotOHLCWinForms;

namespace ScottPlotOHLCWinForms;

public partial class Form1 : Form
{
    private GuiManager _guiManager;

    public Form1()
    {
        InitializeComponent();
        
        _guiManager = new GuiManager(this);
        _guiManager.Initialize();
    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }
}

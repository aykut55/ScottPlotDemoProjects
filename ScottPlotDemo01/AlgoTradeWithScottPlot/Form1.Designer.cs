namespace AlgoTradeWithScottPlot
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        // Status Bar Timer  
        private System.Windows.Forms.Timer statusTimer;
        
        // Main Layout Components
        private MenuStrip mainMenu;
        private ToolStrip mainToolStrip;
        private StatusStrip statusBar;
        private Panel topPanel;
        private Panel bottomPanel;
        private Panel leftPanel;
        private Panel rightPanel;
        private Panel centerPanel;
        
        // MenuStrip Items
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem toolbarsToolStripMenuItem;
        private ToolStripMenuItem statusBarToolStripMenuItem;
        private ToolStripMenuItem fullScreenToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem preferencesToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem documentationToolStripMenuItem;
        
        // ToolStrip Items
        private ToolStripButton newToolStripButton;
        private ToolStripButton openToolStripButton;
        private ToolStripButton saveToolStripButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton cutToolStripButton;
        private ToolStripButton copyToolStripButton;
        private ToolStripButton pasteToolStripButton;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton helpToolStripButton;
        
        // StatusStrip Items
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private ToolStripStatusLabel spacerLabel;
        private ToolStripStatusLabel timeLabel;
        
        // Left Panel Controls
        private Label leftTitleLabel;
        private Button btnLinearLayout;
        private Button btnMainChartLayout;
        private Button btnClearCharts;
        private Button btnCloseCharts;
        private Button btnTestGridLayout;
        
        // Chart Management Controls
        private ComboBox cmbChartSelector;
        private Button btnHideChart;
        private Button btnShowChart;
        private Button btnCloseChart;
        private ComboBox cmbHiddenChartSelector;
        
        // Grid Creation Controls
        private Label lblChartId;
        private TextBox textBox1;
        private Label lblStrategyId;
        private TextBox textBox2;
        private Button btnAddChart;
        private Button btnCreateGrid;
        
        // Data Controls
        private Button buttonGenerateData;
        private Button buttonAddData;
        private Button buttonReadFile;
        private Button button3; // Calculate MA
        private Button button4; // Calculate RSI
        private Button button2; // Plot Data
        
        // Additional Controls
        private Button btnResetChartsPosition;
        
        // TradingChartGrid ToolStrips - 4 comprehensive toolstrips
        private ToolStrip gridLayoutToolStrip;
        private ToolStrip chartControlsToolStrip;
        private ToolStrip dataDisplayToolStrip;
        private ToolStrip dataFilterToolStrip;
        
        // ToolStrip Items for Grid Layout (First Row)
        private ToolStripButton linearLayoutBtn;
        private ToolStripButton customLayoutBtn;
        private ToolStripButton hybridLayoutBtn;
        private ToolStripSeparator separator1;
        private ToolStripButton addRowBtn;
        private ToolStripButton removeRowBtn;
        private ToolStripButton addColBtn;
        private ToolStripButton removeColBtn;
        private ToolStripSeparator separator2;
        private ToolStripButton mainChartBtn;
        private ToolStripButton technicalStackBtn;
        private ToolStripButton multiTimeframeBtn;
        
        // ToolStrip Items for Chart Controls (Second Row)
        private ToolStripButton panLeftBtn;
        private ToolStripButton zoomInXBtn;
        private ToolStripButton resetXBtn;
        private ToolStripButton zoomOutXBtn;
        private ToolStripButton panRightBtn;
        private ToolStripButton panUpBtn;
        private ToolStripButton zoomInYBtn;
        private ToolStripButton resetYBtn;
        private ToolStripButton zoomOutYBtn;
        private ToolStripButton panDownBtn;
        private ToolStripButton resetAllBtn;
        private ToolStripSeparator separator4;
        private ToolStripComboBox zoomModeCombo;
        private ToolStripComboBox panModeCombo;
        private ToolStripComboBox wheelModeCombo;
        private ToolStripComboBox resetModeCombo;
        private ToolStripComboBox dragModeCombo;
        private ToolStripComboBox crosshairModeCombo;
        private ToolStripButton setAllNoneBtn;
        private ToolStripButton setAllBothBtn;
        
        // ToolStrip Items for Data Display Mode (Third Row)
        private ToolStripLabel lblDataMode;
        private ToolStripComboBox cmbDisplayMode;
        private ToolStripLabel lblIndex1;
        private ToolStripTextBox txtIndex1;
        private ToolStripLabel lblIndex2;
        private ToolStripTextBox txtIndex2;
        private ToolStripLabel lblDateTime1;
        private ToolStripControlHost dtpHost1;
        private ToolStripLabel lblDateTime2;
        private ToolStripControlHost dtpHost2;
        private ToolStripButton btnApplyDisplay;
        private ToolStripLabel lblDisplayStatus;
        
        // ToolStrip Items for DataReader Filter (Fourth Row)
        private ToolStripLabel lblFilterMode;
        private ToolStripComboBox cmbFilterMode;
        private ToolStripLabel lblFilterIndex1;
        private ToolStripTextBox txtFilterIndex1;
        private ToolStripLabel lblFilterIndex2;
        private ToolStripTextBox txtFilterIndex2;
        private ToolStripLabel lblFilterDateTime1;
        private ToolStripControlHost dtpFilterHost1;
        private ToolStripLabel lblFilterDateTime2;
        private ToolStripControlHost dtpFilterHost2;
        private ToolStripButton btnApplyFilter;
        private ToolStripLabel lblFilterStatus;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            
            // Dispose of the status timer
            if (disposing && statusTimer != null)
            {
                statusTimer.Stop();
                statusTimer.Dispose();
            }
            
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainMenu = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            undoToolStripMenuItem = new ToolStripMenuItem();
            redoToolStripMenuItem = new ToolStripMenuItem();
            cutToolStripMenuItem = new ToolStripMenuItem();
            copyToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            selectAllToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            toolbarsToolStripMenuItem = new ToolStripMenuItem();
            statusBarToolStripMenuItem = new ToolStripMenuItem();
            fullScreenToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            optionsToolStripMenuItem = new ToolStripMenuItem();
            preferencesToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            documentationToolStripMenuItem = new ToolStripMenuItem();
            mainToolStrip = new ToolStrip();
            newToolStripButton = new ToolStripButton();
            openToolStripButton = new ToolStripButton();
            saveToolStripButton = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            cutToolStripButton = new ToolStripButton();
            copyToolStripButton = new ToolStripButton();
            pasteToolStripButton = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            helpToolStripButton = new ToolStripButton();
            statusBar = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            progressBar = new ToolStripProgressBar();
            spacerLabel = new ToolStripStatusLabel();
            timeLabel = new ToolStripStatusLabel();
            topPanel = new Panel();
            bottomPanel = new Panel();
            leftPanel = new Panel();
            
            // Initialize TradingChartGrid ToolStrips
            gridLayoutToolStrip = new ToolStrip();
            chartControlsToolStrip = new ToolStrip();
            dataDisplayToolStrip = new ToolStrip();
            dataFilterToolStrip = new ToolStrip();
            
            // Grid Layout ToolStrip Items
            linearLayoutBtn = new ToolStripButton();
            customLayoutBtn = new ToolStripButton();
            hybridLayoutBtn = new ToolStripButton();
            separator1 = new ToolStripSeparator();
            addRowBtn = new ToolStripButton();
            removeRowBtn = new ToolStripButton();
            addColBtn = new ToolStripButton();
            removeColBtn = new ToolStripButton();
            separator2 = new ToolStripSeparator();
            mainChartBtn = new ToolStripButton();
            technicalStackBtn = new ToolStripButton();
            multiTimeframeBtn = new ToolStripButton();
            
            // Chart Controls ToolStrip Items
            panLeftBtn = new ToolStripButton();
            zoomInXBtn = new ToolStripButton();
            resetXBtn = new ToolStripButton();
            zoomOutXBtn = new ToolStripButton();
            panRightBtn = new ToolStripButton();
            panUpBtn = new ToolStripButton();
            zoomInYBtn = new ToolStripButton();
            resetYBtn = new ToolStripButton();
            zoomOutYBtn = new ToolStripButton();
            panDownBtn = new ToolStripButton();
            resetAllBtn = new ToolStripButton();
            separator4 = new ToolStripSeparator();
            zoomModeCombo = new ToolStripComboBox();
            panModeCombo = new ToolStripComboBox();
            wheelModeCombo = new ToolStripComboBox();
            resetModeCombo = new ToolStripComboBox();
            dragModeCombo = new ToolStripComboBox();
            crosshairModeCombo = new ToolStripComboBox();
            setAllNoneBtn = new ToolStripButton();
            setAllBothBtn = new ToolStripButton();
            
            // Data Display Mode ToolStrip Items
            lblDataMode = new ToolStripLabel();
            cmbDisplayMode = new ToolStripComboBox();
            lblIndex1 = new ToolStripLabel();
            txtIndex1 = new ToolStripTextBox();
            lblIndex2 = new ToolStripLabel();
            txtIndex2 = new ToolStripTextBox();
            lblDateTime1 = new ToolStripLabel();
            dtpHost1 = new ToolStripControlHost(new DateTimePicker());
            lblDateTime2 = new ToolStripLabel();
            dtpHost2 = new ToolStripControlHost(new DateTimePicker());
            btnApplyDisplay = new ToolStripButton();
            lblDisplayStatus = new ToolStripLabel();
            
            // DataReader Filter ToolStrip Items
            lblFilterMode = new ToolStripLabel();
            cmbFilterMode = new ToolStripComboBox();
            lblFilterIndex1 = new ToolStripLabel();
            txtFilterIndex1 = new ToolStripTextBox();
            lblFilterIndex2 = new ToolStripLabel();
            txtFilterIndex2 = new ToolStripTextBox();
            lblFilterDateTime1 = new ToolStripLabel();
            dtpFilterHost1 = new ToolStripControlHost(new DateTimePicker());
            lblFilterDateTime2 = new ToolStripLabel();
            dtpFilterHost2 = new ToolStripControlHost(new DateTimePicker());
            btnApplyFilter = new ToolStripButton();
            lblFilterStatus = new ToolStripLabel();
            
            button4 = new Button();
            button2 = new Button();
            button3 = new Button();
            buttonReadFile = new Button();
            buttonAddData = new Button();
            buttonGenerateData = new Button();
            btnResetChartsPosition = new Button();
            cmbHiddenChartSelector = new ComboBox();
            btnCloseChart = new Button();
            btnShowChart = new Button();
            btnHideChart = new Button();
            cmbChartSelector = new ComboBox();
            btnCloseCharts = new Button();
            btnClearCharts = new Button();
            btnMainChartLayout = new Button();
            btnLinearLayout = new Button();
            btnTestGridLayout = new Button();
            lblChartId = new Label();
            textBox1 = new TextBox();
            lblStrategyId = new Label();
            textBox2 = new TextBox();
            btnAddChart = new Button();
            btnCreateGrid = new Button();
            leftTitleLabel = new Label();
            rightPanel = new Panel();
            centerPanel = new Panel();
            mainMenu.SuspendLayout();
            mainToolStrip.SuspendLayout();
            statusBar.SuspendLayout();
            leftPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenu
            // 
            mainMenu.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            mainMenu.Location = new Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Size = new Size(973, 24);
            mainMenu.TabIndex = 0;
            mainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, new ToolStripSeparator(), saveToolStripMenuItem, saveAsToolStripMenuItem, new ToolStripSeparator(), exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newToolStripMenuItem.Size = new Size(146, 22);
            newToolStripMenuItem.Text = "&New";
            newToolStripMenuItem.Click += newToolStripMenuItem_Click;
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openToolStripMenuItem.Size = new Size(146, 22);
            openToolStripMenuItem.Text = "&Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new Size(146, 22);
            saveToolStripMenuItem.Text = "&Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(146, 22);
            saveAsToolStripMenuItem.Text = "Save &As...";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(146, 22);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, new ToolStripSeparator(), cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, new ToolStripSeparator(), selectAllToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
            undoToolStripMenuItem.Size = new Size(164, 22);
            undoToolStripMenuItem.Text = "&Undo";
            undoToolStripMenuItem.Click += undoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
            redoToolStripMenuItem.Size = new Size(164, 22);
            redoToolStripMenuItem.Text = "&Redo";
            redoToolStripMenuItem.Click += redoToolStripMenuItem_Click;
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutToolStripMenuItem.Size = new Size(164, 22);
            cutToolStripMenuItem.Text = "Cu&t";
            cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(164, 22);
            copyToolStripMenuItem.Text = "&Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(164, 22);
            pasteToolStripMenuItem.Text = "&Paste";
            pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
            // 
            // selectAllToolStripMenuItem
            // 
            selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            selectAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            selectAllToolStripMenuItem.Size = new Size(164, 22);
            selectAllToolStripMenuItem.Text = "Select &All";
            selectAllToolStripMenuItem.Click += selectAllToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolbarsToolStripMenuItem, statusBarToolStripMenuItem, new ToolStripSeparator(), fullScreenToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "&View";
            // 
            // toolbarsToolStripMenuItem
            // 
            toolbarsToolStripMenuItem.Name = "toolbarsToolStripMenuItem";
            toolbarsToolStripMenuItem.Size = new Size(135, 22);
            toolbarsToolStripMenuItem.Text = "&Toolbars";
            toolbarsToolStripMenuItem.Click += toolbarsToolStripMenuItem_Click;
            // 
            // statusBarToolStripMenuItem
            // 
            statusBarToolStripMenuItem.Checked = true;
            statusBarToolStripMenuItem.CheckState = CheckState.Checked;
            statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
            statusBarToolStripMenuItem.Size = new Size(135, 22);
            statusBarToolStripMenuItem.Text = "&Status Bar";
            statusBarToolStripMenuItem.Click += statusBarToolStripMenuItem_Click;
            // 
            // fullScreenToolStripMenuItem
            // 
            fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
            fullScreenToolStripMenuItem.ShortcutKeys = Keys.F11;
            fullScreenToolStripMenuItem.Size = new Size(135, 22);
            fullScreenToolStripMenuItem.Text = "&Full Screen";
            fullScreenToolStripMenuItem.Click += fullScreenToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { optionsToolStripMenuItem, preferencesToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(47, 20);
            toolsToolStripMenuItem.Text = "&Tools";
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.Size = new Size(135, 22);
            optionsToolStripMenuItem.Text = "&Options";
            optionsToolStripMenuItem.Click += optionsToolStripMenuItem_Click;
            // 
            // preferencesToolStripMenuItem
            // 
            preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            preferencesToolStripMenuItem.Size = new Size(135, 22);
            preferencesToolStripMenuItem.Text = "&Preferences";
            preferencesToolStripMenuItem.Click += preferencesToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { documentationToolStripMenuItem, new ToolStripSeparator(), aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // documentationToolStripMenuItem
            // 
            documentationToolStripMenuItem.Name = "documentationToolStripMenuItem";
            documentationToolStripMenuItem.ShortcutKeys = Keys.F1;
            documentationToolStripMenuItem.Size = new Size(168, 22);
            documentationToolStripMenuItem.Text = "&Documentation";
            documentationToolStripMenuItem.Click += documentationToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(168, 22);
            aboutToolStripMenuItem.Text = "&About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // mainToolStrip
            // 
            mainToolStrip.Items.AddRange(new ToolStripItem[] { newToolStripButton, openToolStripButton, saveToolStripButton, toolStripSeparator1, cutToolStripButton, copyToolStripButton, pasteToolStripButton, toolStripSeparator2, helpToolStripButton });
            mainToolStrip.Location = new Point(0, 24);
            mainToolStrip.Name = "mainToolStrip";
            mainToolStrip.Size = new Size(973, 25);
            mainToolStrip.TabIndex = 1;
            // 
            // newToolStripButton
            // 
            newToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            newToolStripButton.Name = "newToolStripButton";
            newToolStripButton.Size = new Size(50, 22);
            newToolStripButton.Text = "📄 New";
            newToolStripButton.ToolTipText = "Create a new file (Ctrl+N)";
            // 
            // openToolStripButton
            // 
            openToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            openToolStripButton.Name = "openToolStripButton";
            openToolStripButton.Size = new Size(55, 22);
            openToolStripButton.Text = "📁 Open";
            openToolStripButton.ToolTipText = "Open an existing file (Ctrl+O)";
            // 
            // saveToolStripButton
            // 
            saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            saveToolStripButton.Name = "saveToolStripButton";
            saveToolStripButton.Size = new Size(52, 22);
            saveToolStripButton.Text = "💾 Save";
            saveToolStripButton.ToolTipText = "Save the current file (Ctrl+S)";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // cutToolStripButton
            // 
            cutToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            cutToolStripButton.Name = "cutToolStripButton";
            cutToolStripButton.Size = new Size(45, 22);
            cutToolStripButton.Text = "✂️ Cut";
            cutToolStripButton.ToolTipText = "Cut selected content (Ctrl+X)";
            // 
            // copyToolStripButton
            // 
            copyToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            copyToolStripButton.Name = "copyToolStripButton";
            copyToolStripButton.Size = new Size(55, 22);
            copyToolStripButton.Text = "📋 Copy";
            copyToolStripButton.ToolTipText = "Copy selected content (Ctrl+C)";
            // 
            // pasteToolStripButton
            // 
            pasteToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            pasteToolStripButton.Name = "pasteToolStripButton";
            pasteToolStripButton.Size = new Size(58, 22);
            pasteToolStripButton.Text = "📌 Paste";
            pasteToolStripButton.ToolTipText = "Paste clipboard content (Ctrl+V)";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // helpToolStripButton
            // 
            helpToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            helpToolStripButton.Name = "helpToolStripButton";
            helpToolStripButton.Size = new Size(52, 22);
            helpToolStripButton.Text = "❓ Help";
            helpToolStripButton.ToolTipText = "Show help documentation (F1)";
            // 
            // statusBar
            // 
            statusBar.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar, spacerLabel, timeLabel });
            statusBar.Location = new Point(0, 687);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(973, 25);
            statusBar.TabIndex = 2;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(39, 20);
            statusLabel.Text = "Ready";
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(100, 19);
            // 
            // spacerLabel
            // 
            spacerLabel.Name = "spacerLabel";
            spacerLabel.Size = new Size(10, 20);
            spacerLabel.Spring = true;
            spacerLabel.Text = "";
            // 
            // timeLabel
            // 
            timeLabel.Name = "timeLabel";
            timeLabel.Size = new Size(120, 20);
            timeLabel.Text = "00:00:00";
            timeLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // topPanel
            // 
            topPanel.BackColor = Color.LightBlue;
            topPanel.BorderStyle = BorderStyle.FixedSingle;
            topPanel.Controls.Add(dataFilterToolStrip);
            topPanel.Controls.Add(dataDisplayToolStrip);
            topPanel.Controls.Add(chartControlsToolStrip);
            topPanel.Controls.Add(gridLayoutToolStrip);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 49);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(973, 120);
            topPanel.TabIndex = 3;
            // 
            // bottomPanel
            // 
            bottomPanel.BackColor = Color.LightGreen;
            bottomPanel.BorderStyle = BorderStyle.FixedSingle;
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 637);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(973, 50);
            bottomPanel.TabIndex = 2;
            // 
            // leftPanel
            // 
            leftPanel.BackColor = Color.LightYellow;
            leftPanel.BorderStyle = BorderStyle.FixedSingle;
            leftPanel.Controls.Add(button4);
            leftPanel.Controls.Add(button2);
            leftPanel.Controls.Add(button3);
            leftPanel.Controls.Add(buttonReadFile);
            leftPanel.Controls.Add(buttonAddData);
            leftPanel.Controls.Add(buttonGenerateData);
            leftPanel.Controls.Add(btnResetChartsPosition);
            leftPanel.Controls.Add(cmbHiddenChartSelector);
            leftPanel.Controls.Add(btnCloseChart);
            leftPanel.Controls.Add(btnShowChart);
            leftPanel.Controls.Add(btnHideChart);
            leftPanel.Controls.Add(cmbChartSelector);
            leftPanel.Controls.Add(btnCloseCharts);
            leftPanel.Controls.Add(btnClearCharts);
            leftPanel.Controls.Add(btnMainChartLayout);
            leftPanel.Controls.Add(btnLinearLayout);
            leftPanel.Controls.Add(btnTestGridLayout);
            leftPanel.Controls.Add(lblChartId);
            leftPanel.Controls.Add(textBox1);
            leftPanel.Controls.Add(lblStrategyId);
            leftPanel.Controls.Add(textBox2);
            leftPanel.Controls.Add(btnAddChart);
            leftPanel.Controls.Add(btnCreateGrid);
            leftPanel.Controls.Add(leftTitleLabel);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Location = new Point(0, 109);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(238, 528);
            leftPanel.TabIndex = 1;
            // 
            // button4
            // 
            button4.Location = new Point(135, 473);
            button4.Name = "button4";
            button4.Size = new Size(96, 23);
            button4.TabIndex = 22;
            button4.Text = "Calculate RSI";
            button4.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(34, 502);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 23;
            button2.Text = "Plot Data";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(34, 473);
            button3.Name = "button3";
            button3.Size = new Size(96, 23);
            button3.TabIndex = 21;
            button3.Text = "Calculate MA";
            button3.UseVisualStyleBackColor = true;
            // 
            // buttonReadFile
            // 
            buttonReadFile.Location = new Point(34, 444);
            buttonReadFile.Name = "buttonReadFile";
            buttonReadFile.Size = new Size(75, 23);
            buttonReadFile.TabIndex = 20;
            buttonReadFile.Text = "Read File";
            buttonReadFile.UseVisualStyleBackColor = true;
            // 
            // buttonAddData
            // 
            buttonAddData.Location = new Point(127, 404);
            buttonAddData.Name = "buttonAddData";
            buttonAddData.Size = new Size(97, 23);
            buttonAddData.TabIndex = 19;
            buttonAddData.Text = "Add Data";
            buttonAddData.UseVisualStyleBackColor = true;
            // 
            // buttonGenerateData
            // 
            buttonGenerateData.Location = new Point(14, 375);
            buttonGenerateData.Name = "buttonGenerateData";
            buttonGenerateData.Size = new Size(209, 23);
            buttonGenerateData.TabIndex = 18;
            buttonGenerateData.Text = "Generate Data";
            buttonGenerateData.UseVisualStyleBackColor = true;
            // 
            // btnResetChartsPosition
            // 
            btnResetChartsPosition.BackColor = Color.Gold;
            btnResetChartsPosition.Location = new Point(14, 222);
            btnResetChartsPosition.Name = "btnResetChartsPosition";
            btnResetChartsPosition.Size = new Size(102, 30);
            btnResetChartsPosition.TabIndex = 12;
            btnResetChartsPosition.Text = "Reset Charts";
            btnResetChartsPosition.UseVisualStyleBackColor = false;
            // 
            // cmbHiddenChartSelector
            // 
            cmbHiddenChartSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHiddenChartSelector.Font = new Font("Arial", 8F);
            cmbHiddenChartSelector.FormattingEnabled = true;
            cmbHiddenChartSelector.Location = new Point(122, 258);
            cmbHiddenChartSelector.Name = "cmbHiddenChartSelector";
            cmbHiddenChartSelector.Size = new Size(102, 22);
            cmbHiddenChartSelector.TabIndex = 16;
            // 
            // btnCloseChart
            // 
            btnCloseChart.Font = new Font("Arial", 8F);
            btnCloseChart.Location = new Point(14, 322);
            btnCloseChart.Name = "btnCloseChart";
            btnCloseChart.Size = new Size(209, 30);
            btnCloseChart.TabIndex = 17;
            btnCloseChart.Text = "Close Chart";
            btnCloseChart.UseVisualStyleBackColor = true;
            // 
            // btnShowChart
            // 
            btnShowChart.Font = new Font("Arial", 8F);
            btnShowChart.Location = new Point(122, 286);
            btnShowChart.Name = "btnShowChart";
            btnShowChart.Size = new Size(102, 30);
            btnShowChart.TabIndex = 15;
            btnShowChart.Text = "Show Chart";
            btnShowChart.UseVisualStyleBackColor = true;
            // 
            // btnHideChart
            // 
            btnHideChart.Font = new Font("Arial", 8F);
            btnHideChart.Location = new Point(14, 286);
            btnHideChart.Name = "btnHideChart";
            btnHideChart.Size = new Size(102, 30);
            btnHideChart.TabIndex = 14;
            btnHideChart.Text = "Hide Chart";
            btnHideChart.UseVisualStyleBackColor = true;
            // 
            // cmbChartSelector
            // 
            cmbChartSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbChartSelector.Font = new Font("Arial", 8F);
            cmbChartSelector.FormattingEnabled = true;
            cmbChartSelector.Location = new Point(14, 258);
            cmbChartSelector.Name = "cmbChartSelector";
            cmbChartSelector.Size = new Size(102, 22);
            cmbChartSelector.TabIndex = 13;
            // 
            // btnCloseCharts
            // 
            btnCloseCharts.BackColor = Color.IndianRed;
            btnCloseCharts.Location = new Point(14, 107);
            btnCloseCharts.Name = "btnCloseCharts";
            btnCloseCharts.Size = new Size(102, 30);
            btnCloseCharts.TabIndex = 5;
            btnCloseCharts.Text = "Close Charts";
            btnCloseCharts.UseVisualStyleBackColor = false;
            // 
            // btnClearCharts
            // 
            btnClearCharts.BackColor = Color.LightSalmon;
            btnClearCharts.Location = new Point(122, 107);
            btnClearCharts.Name = "btnClearCharts";
            btnClearCharts.Size = new Size(102, 30);
            btnClearCharts.TabIndex = 4;
            btnClearCharts.Text = "Clear Charts";
            btnClearCharts.UseVisualStyleBackColor = false;
            // 
            // btnMainChartLayout
            // 
            btnMainChartLayout.Location = new Point(122, 35);
            btnMainChartLayout.Name = "btnMainChartLayout";
            btnMainChartLayout.Size = new Size(102, 30);
            btnMainChartLayout.TabIndex = 2;
            btnMainChartLayout.Text = "Main Layout";
            btnMainChartLayout.UseVisualStyleBackColor = true;
            // 
            // btnLinearLayout
            // 
            btnLinearLayout.Location = new Point(14, 35);
            btnLinearLayout.Name = "btnLinearLayout";
            btnLinearLayout.Size = new Size(102, 30);
            btnLinearLayout.TabIndex = 1;
            btnLinearLayout.Text = "Linear Layout";
            btnLinearLayout.UseVisualStyleBackColor = true;
            // 
            // btnTestGridLayout
            // 
            btnTestGridLayout.Location = new Point(14, 71);
            btnTestGridLayout.Name = "btnTestGridLayout";
            btnTestGridLayout.Size = new Size(102, 30);
            btnTestGridLayout.TabIndex = 3;
            btnTestGridLayout.Text = "Test Grid";
            btnTestGridLayout.UseVisualStyleBackColor = true;
            // 
            // lblChartId
            // 
            lblChartId.Location = new Point(14, 175);
            lblChartId.Name = "lblChartId";
            lblChartId.Size = new Size(50, 15);
            lblChartId.TabIndex = 10;
            lblChartId.Text = "ChartId:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(14, 193);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(81, 23);
            textBox1.TabIndex = 11;
            textBox1.Text = "1";
            // 
            // lblStrategyId
            // 
            lblStrategyId.Location = new Point(122, 175);
            lblStrategyId.Name = "lblStrategyId";
            lblStrategyId.Size = new Size(65, 15);
            lblStrategyId.TabIndex = 8;
            lblStrategyId.Text = "StrategyId:";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(122, 193);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(81, 23);
            textBox2.TabIndex = 9;
            textBox2.Text = "1";
            // 
            // btnAddChart
            // 
            btnAddChart.Location = new Point(14, 143);
            btnAddChart.Name = "btnAddChart";
            btnAddChart.Size = new Size(102, 22);
            btnAddChart.TabIndex = 7;
            btnAddChart.Text = "Add Chart";
            btnAddChart.UseVisualStyleBackColor = true;
            // 
            // btnCreateGrid
            // 
            btnCreateGrid.Location = new Point(122, 143);
            btnCreateGrid.Name = "btnCreateGrid";
            btnCreateGrid.Size = new Size(102, 22);
            btnCreateGrid.TabIndex = 6;
            btnCreateGrid.Text = "Create Grid";
            btnCreateGrid.UseVisualStyleBackColor = true;
            // 
            // leftTitleLabel
            // 
            leftTitleLabel.BackColor = Color.DarkBlue;
            leftTitleLabel.Dock = DockStyle.Top;
            leftTitleLabel.Font = new Font("Arial", 10F, FontStyle.Bold);
            leftTitleLabel.ForeColor = Color.White;
            leftTitleLabel.Location = new Point(0, 0);
            leftTitleLabel.Name = "leftTitleLabel";
            leftTitleLabel.Size = new Size(236, 20);
            leftTitleLabel.TabIndex = 0;
            leftTitleLabel.Text = "Trading Controls";
            // 
            // rightPanel
            // 
            rightPanel.BackColor = Color.LightCoral;
            rightPanel.BorderStyle = BorderStyle.FixedSingle;
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Location = new Point(853, 109);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(120, 528);
            rightPanel.TabIndex = 0;
            // 
            // centerPanel
            // 
            centerPanel.BackColor = Color.White;
            centerPanel.BorderStyle = BorderStyle.FixedSingle;
            centerPanel.Dock = DockStyle.Fill;
            centerPanel.Location = new Point(238, 109);
            centerPanel.Name = "centerPanel";
            centerPanel.Size = new Size(615, 528);
            centerPanel.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(973, 712);
            Controls.Add(centerPanel);
            Controls.Add(rightPanel);
            Controls.Add(leftPanel);
            Controls.Add(bottomPanel);
            Controls.Add(topPanel);
            Controls.Add(mainToolStrip);
            Controls.Add(mainMenu);
            Controls.Add(statusBar);
            MainMenuStrip = mainMenu;
            Name = "Form1";
            Text = "AlgoTrade - Trading Dashboard";
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            mainToolStrip.ResumeLayout(false);
            mainToolStrip.PerformLayout();
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            // 
            // gridLayoutToolStrip
            // 
            gridLayoutToolStrip.Dock = DockStyle.Top;
            gridLayoutToolStrip.BackColor = Color.LightGray;
            gridLayoutToolStrip.Items.AddRange(new ToolStripItem[] {
                linearLayoutBtn, customLayoutBtn, hybridLayoutBtn, separator1,
                addRowBtn, removeRowBtn, addColBtn, removeColBtn, separator2,
                mainChartBtn, technicalStackBtn, multiTimeframeBtn
            });
            gridLayoutToolStrip.Location = new Point(0, 0);
            gridLayoutToolStrip.Name = "gridLayoutToolStrip";
            gridLayoutToolStrip.Size = new Size(971, 25);
            gridLayoutToolStrip.TabIndex = 0;
            
            // Grid Layout Buttons
            linearLayoutBtn.Text = "Linear";
            linearLayoutBtn.ToolTipText = "Linear layout - Alt alta düzen";
            customLayoutBtn.Text = "Custom";
            customLayoutBtn.ToolTipText = "Custom layout - Manuel grid";
            hybridLayoutBtn.Text = "Hybrid";
            hybridLayoutBtn.ToolTipText = "Hybrid layout - Preset layoutlar";
            addRowBtn.Text = "+ Row";
            addRowBtn.ToolTipText = "Satır ekle";
            removeRowBtn.Text = "- Row";
            removeRowBtn.ToolTipText = "Satır sil";
            addColBtn.Text = "+ Col";
            addColBtn.ToolTipText = "Sütun ekle";
            removeColBtn.Text = "- Col";
            removeColBtn.ToolTipText = "Sütun sil";
            mainChartBtn.Text = "Main Chart";
            mainChartBtn.ToolTipText = "Ana chart + yan paneller";
            technicalStackBtn.Text = "Technical Stack";
            technicalStackBtn.ToolTipText = "Teknik analiz yığını";
            multiTimeframeBtn.Text = "Multi TF";
            multiTimeframeBtn.ToolTipText = "Çoklu zaman dilimi";
            
            // 
            // chartControlsToolStrip
            // 
            chartControlsToolStrip.Dock = DockStyle.Top;
            chartControlsToolStrip.BackColor = Color.LightGray;
            chartControlsToolStrip.Items.AddRange(new ToolStripItem[] {
                panLeftBtn, zoomInXBtn, resetXBtn, zoomOutXBtn, panRightBtn,
                panUpBtn, zoomInYBtn, resetYBtn, zoomOutYBtn, panDownBtn, resetAllBtn,
                separator4,
                new ToolStripLabel("Zoom:"), zoomModeCombo,
                new ToolStripLabel("Pan:"), panModeCombo,
                new ToolStripLabel("Wheel:"), wheelModeCombo,
                new ToolStripLabel("Reset:"), resetModeCombo,
                new ToolStripLabel("Drag:"), dragModeCombo,
                new ToolStripLabel("Cross:"), crosshairModeCombo,
                new ToolStripSeparator(),
                setAllNoneBtn, setAllBothBtn
            });
            chartControlsToolStrip.Location = new Point(0, 25);
            chartControlsToolStrip.Name = "chartControlsToolStrip";
            chartControlsToolStrip.Size = new Size(971, 25);
            chartControlsToolStrip.TabIndex = 1;
            
            // Chart Control Buttons
            panLeftBtn.Text = "←";
            panLeftBtn.ToolTipText = "Tüm chart'larda sola kaydır";
            zoomInXBtn.Text = "X+";
            zoomInXBtn.ToolTipText = "Tüm chart'larda X zoom in";
            resetXBtn.Text = "RX";
            resetXBtn.ToolTipText = "Tüm chart'larda X reset";
            zoomOutXBtn.Text = "X-";
            zoomOutXBtn.ToolTipText = "Tüm chart'larda X zoom out";
            panRightBtn.Text = "→";
            panRightBtn.ToolTipText = "Tüm chart'larda sağa kaydır";
            panUpBtn.Text = "↑";
            panUpBtn.ToolTipText = "Tüm chart'larda yukarı kaydır";
            zoomInYBtn.Text = "Y+";
            zoomInYBtn.ToolTipText = "Tüm chart'larda Y zoom in";
            resetYBtn.Text = "RY";
            resetYBtn.ToolTipText = "Tüm chart'larda Y reset";
            zoomOutYBtn.Text = "Y-";
            zoomOutYBtn.ToolTipText = "Tüm chart'larda Y zoom out";
            panDownBtn.Text = "↓";
            panDownBtn.ToolTipText = "Tüm chart'larda aşağı kaydır";
            resetAllBtn.Text = "⟲";
            resetAllBtn.ToolTipText = "Tüm chart'larda tam reset";
            setAllNoneBtn.Text = "ALL: N";
            setAllNoneBtn.ToolTipText = "Tüm modları 'None' yap";
            setAllBothBtn.Text = "ALL: B";
            setAllBothBtn.ToolTipText = "Tüm modları 'Both' yap";
            
            // Chart Control ComboBoxes
            zoomModeCombo.Name = "ZoomMode";
            zoomModeCombo.ToolTipText = "Zoom modu";
            zoomModeCombo.Items.AddRange(new string[] { "None", "OnlyX", "OnlyY", "Both" });
            zoomModeCombo.SelectedItem = "Both";
            panModeCombo.Name = "PanMode";
            panModeCombo.ToolTipText = "Pan modu";
            panModeCombo.Items.AddRange(new string[] { "None", "OnlyX", "OnlyY", "Both" });
            panModeCombo.SelectedItem = "Both";
            wheelModeCombo.Name = "WheelMode";
            wheelModeCombo.ToolTipText = "Mouse wheel modu";
            wheelModeCombo.Items.AddRange(new string[] { "None", "OnlyX", "OnlyY", "Both" });
            wheelModeCombo.SelectedItem = "Both";
            resetModeCombo.Name = "ResetMode";
            resetModeCombo.ToolTipText = "Reset modu";
            resetModeCombo.Items.AddRange(new string[] { "None", "OnlyX", "OnlyY", "Both" });
            resetModeCombo.SelectedItem = "Both";
            dragModeCombo.Name = "DragMode";
            dragModeCombo.ToolTipText = "Drag modu";
            dragModeCombo.Items.AddRange(new string[] { "None", "OnlyX", "OnlyY", "Both" });
            dragModeCombo.SelectedItem = "Both";
            crosshairModeCombo.Name = "CrosshairMode";
            crosshairModeCombo.ToolTipText = "Crosshair modu";
            crosshairModeCombo.Items.AddRange(new string[] { "None", "Vertical", "Horizontal", "Both" });
            crosshairModeCombo.SelectedItem = "Both";
            
            // 
            // dataDisplayToolStrip
            // 
            dataDisplayToolStrip.Dock = DockStyle.Top;
            dataDisplayToolStrip.BackColor = Color.LightSteelBlue;
            dataDisplayToolStrip.Items.AddRange(new ToolStripItem[] {
                lblDataMode, new ToolStripSeparator(), cmbDisplayMode, new ToolStripSeparator(),
                lblIndex1, txtIndex1, lblIndex2, txtIndex2, new ToolStripSeparator(),
                lblDateTime1, dtpHost1, lblDateTime2, dtpHost2, new ToolStripSeparator(),
                btnApplyDisplay, lblDisplayStatus
            });
            dataDisplayToolStrip.Location = new Point(0, 50);
            dataDisplayToolStrip.Name = "dataDisplayToolStrip";
            dataDisplayToolStrip.Size = new Size(971, 25);
            dataDisplayToolStrip.TabIndex = 2;
            
            // Data Display Mode Controls
            lblDataMode.Text = "Data Display:";
            lblDataMode.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            cmbDisplayMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDisplayMode.Size = new Size(120, 25);
            cmbDisplayMode.ToolTipText = "Select data display mode";
            cmbDisplayMode.Items.AddRange(new object[] {
                "All", "FitToScreen", "LastN", "FirstN", "IndexRange",
                "AfterDateTime", "BeforeDateTime", "DateTimeRange"
            });
            cmbDisplayMode.SelectedIndex = 0;
            lblIndex1.Text = "N1:";
            txtIndex1.Text = "300";
            txtIndex1.Size = new Size(70, 25);
            txtIndex1.ToolTipText = "First index or count value";
            lblIndex2.Text = "N2:";
            txtIndex2.Text = "2000";
            txtIndex2.Size = new Size(70, 25);
            txtIndex2.ToolTipText = "Second index value for range";
            lblDateTime1.Text = "Date1:";
            ((DateTimePicker)dtpHost1.Control).Format = DateTimePickerFormat.Short;
            ((DateTimePicker)dtpHost1.Control).Value = new DateTime(2025, 11, 12);
            ((DateTimePicker)dtpHost1.Control).Size = new Size(100, 25);
            dtpHost1.ToolTipText = "Start date for date filtering";
            lblDateTime2.Text = "Date2:";
            ((DateTimePicker)dtpHost2.Control).Format = DateTimePickerFormat.Short;
            ((DateTimePicker)dtpHost2.Control).Value = new DateTime(2025, 11, 12, 23, 59, 59);
            ((DateTimePicker)dtpHost2.Control).Size = new Size(100, 25);
            dtpHost2.ToolTipText = "End date for date filtering";
            btnApplyDisplay.Text = "Apply Display";
            btnApplyDisplay.ToolTipText = "Apply the selected display mode";
            btnApplyDisplay.BackColor = Color.LightGreen;
            lblDisplayStatus.Text = "Mode: FitScreen";
            lblDisplayStatus.Name = "lblDisplayStatus";
            
            // 
            // dataFilterToolStrip
            // 
            dataFilterToolStrip.Dock = DockStyle.Top;
            dataFilterToolStrip.BackColor = Color.LightGoldenrodYellow;
            dataFilterToolStrip.Items.AddRange(new ToolStripItem[] {
                lblFilterMode, new ToolStripSeparator(), cmbFilterMode, new ToolStripSeparator(),
                lblFilterIndex1, txtFilterIndex1, lblFilterIndex2, txtFilterIndex2, new ToolStripSeparator(),
                lblFilterDateTime1, dtpFilterHost1, lblFilterDateTime2, dtpFilterHost2, new ToolStripSeparator(),
                btnApplyFilter, lblFilterStatus
            });
            dataFilterToolStrip.Location = new Point(0, 75);
            dataFilterToolStrip.Name = "dataFilterToolStrip";
            dataFilterToolStrip.Size = new Size(971, 25);
            dataFilterToolStrip.TabIndex = 3;
            
            // DataReader Filter Controls
            lblFilterMode.Text = "DataReader Filter:";
            lblFilterMode.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            cmbFilterMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterMode.Size = new Size(120, 25);
            cmbFilterMode.ToolTipText = "Select data filter mode";
            cmbFilterMode.Items.AddRange(new object[] {
                "All", "LastN", "FirstN", "IndexRange",
                "AfterDateTime", "BeforeDateTime", "DateTimeRange"
            });
            cmbFilterMode.SelectedIndex = 0;
            lblFilterIndex1.Text = "N1:";
            txtFilterIndex1.Text = "300";
            txtFilterIndex1.Size = new Size(70, 25);
            txtFilterIndex1.ToolTipText = "First index or count value";
            lblFilterIndex2.Text = "N2:";
            txtFilterIndex2.Text = "2000";
            txtFilterIndex2.Size = new Size(70, 25);
            txtFilterIndex2.ToolTipText = "Second index value for range";
            lblFilterDateTime1.Text = "Date1:";
            ((DateTimePicker)dtpFilterHost1.Control).Format = DateTimePickerFormat.Short;
            ((DateTimePicker)dtpFilterHost1.Control).Value = new DateTime(2025, 11, 12);
            ((DateTimePicker)dtpFilterHost1.Control).Size = new Size(100, 25);
            dtpFilterHost1.ToolTipText = "Start date for date filtering";
            lblFilterDateTime2.Text = "Date2:";
            ((DateTimePicker)dtpFilterHost2.Control).Format = DateTimePickerFormat.Short;
            ((DateTimePicker)dtpFilterHost2.Control).Value = new DateTime(2025, 11, 12, 23, 59, 59);
            ((DateTimePicker)dtpFilterHost2.Control).Size = new Size(100, 25);
            dtpFilterHost2.ToolTipText = "End date for date filtering";
            btnApplyFilter.Text = "Apply Filter";
            btnApplyFilter.ToolTipText = "Apply the selected filter to data reading";
            btnApplyFilter.BackColor = Color.LightGreen;
            lblFilterStatus.Text = "Filter: All";
            lblFilterStatus.Name = "lblFilterStatus";
            
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}

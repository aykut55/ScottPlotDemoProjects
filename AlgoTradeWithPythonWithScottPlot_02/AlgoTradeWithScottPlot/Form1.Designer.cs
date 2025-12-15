namespace AlgoTradeWithScottPlot
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            timer1 = new System.Windows.Forms.Timer(components);
            panelTop = new Panel();
            panelBottom = new Panel();
            panelLeft = new Panel();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            panelRight = new Panel();
            panelCenter = new Panel();
            splitter3 = new Splitter();
            tableLayoutPanel3 = new TableLayoutPanel();
            formsPlot3 = new ScottPlot.WinForms.FormsPlot();
            splitter1 = new Splitter();
            tableLayoutPanel2 = new TableLayoutPanel();
            formsPlot2 = new ScottPlot.WinForms.FormsPlot();
            splitter2 = new Splitter();
            tableLayoutPanel1 = new TableLayoutPanel();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            panelLeft.SuspendLayout();
            panelCenter.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1212, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(92, 22);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 943);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1212, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(118, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // panelTop
            // 
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 24);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1212, 42);
            panelTop.TabIndex = 2;
            // 
            // panelBottom
            // 
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 909);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(1212, 34);
            panelBottom.TabIndex = 6;
            // 
            // panelLeft
            // 
            panelLeft.Controls.Add(button4);
            panelLeft.Controls.Add(button3);
            panelLeft.Controls.Add(button2);
            panelLeft.Controls.Add(button1);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 66);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(111, 843);
            panelLeft.TabIndex = 7;
            panelLeft.Paint += panelLeft_Paint;
            // 
            // button4
            // 
            button4.Location = new Point(12, 77);
            button4.Name = "button4";
            button4.Size = new Size(96, 23);
            button4.TabIndex = 3;
            button4.Text = "Calculate RSI";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button3
            // 
            button3.Location = new Point(12, 48);
            button3.Name = "button3";
            button3.Size = new Size(96, 23);
            button3.TabIndex = 2;
            button3.Text = "Calculate MA";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(12, 106);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 1;
            button2.Text = "Plot Data";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(12, 19);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "ReadFile";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // panelRight
            // 
            panelRight.Dock = DockStyle.Right;
            panelRight.Location = new Point(1161, 66);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(51, 843);
            panelRight.TabIndex = 8;
            // 
            // panelCenter
            // 
            panelCenter.Controls.Add(splitter3);
            panelCenter.Controls.Add(tableLayoutPanel3);
            panelCenter.Controls.Add(splitter1);
            panelCenter.Controls.Add(tableLayoutPanel2);
            panelCenter.Controls.Add(splitter2);
            panelCenter.Controls.Add(tableLayoutPanel1);
            panelCenter.Dock = DockStyle.Fill;
            panelCenter.Location = new Point(111, 66);
            panelCenter.Name = "panelCenter";
            panelCenter.Size = new Size(1050, 843);
            panelCenter.TabIndex = 13;
            // 
            // splitter3
            // 
            splitter3.Dock = DockStyle.Top;
            splitter3.Location = new Point(0, 679);
            splitter3.Name = "splitter3";
            splitter3.Size = new Size(1050, 3);
            splitter3.TabIndex = 17;
            splitter3.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 98F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1F));
            tableLayoutPanel3.Controls.Add(formsPlot3, 1, 1);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 679);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 2F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 96F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 2F));
            tableLayoutPanel3.Size = new Size(1050, 161);
            tableLayoutPanel3.TabIndex = 16;
            // 
            // formsPlot3
            // 
            formsPlot3.DisplayScale = 1F;
            formsPlot3.Dock = DockStyle.Fill;
            formsPlot3.Location = new Point(13, 6);
            formsPlot3.Name = "formsPlot3";
            formsPlot3.Size = new Size(1023, 148);
            formsPlot3.TabIndex = 0;
            // 
            // splitter1
            // 
            splitter1.Dock = DockStyle.Top;
            splitter1.Location = new Point(0, 676);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(1050, 3);
            splitter1.TabIndex = 15;
            splitter1.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 98F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1F));
            tableLayoutPanel2.Controls.Add(formsPlot2, 1, 1);
            tableLayoutPanel2.Dock = DockStyle.Top;
            tableLayoutPanel2.Location = new Point(0, 396);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 2F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 96F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 2F));
            tableLayoutPanel2.Size = new Size(1050, 280);
            tableLayoutPanel2.TabIndex = 14;
            // 
            // formsPlot2
            // 
            formsPlot2.DisplayScale = 1F;
            formsPlot2.Dock = DockStyle.Fill;
            formsPlot2.Location = new Point(13, 8);
            formsPlot2.Name = "formsPlot2";
            formsPlot2.Size = new Size(1023, 262);
            formsPlot2.TabIndex = 0;
            // 
            // splitter2
            // 
            splitter2.Dock = DockStyle.Bottom;
            splitter2.Location = new Point(0, 840);
            splitter2.Name = "splitter2";
            splitter2.Size = new Size(1050, 3);
            splitter2.TabIndex = 13;
            splitter2.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 98F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1F));
            tableLayoutPanel1.Controls.Add(formsPlot1, 1, 1);
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 2F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 96F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 2F));
            tableLayoutPanel1.Size = new Size(1050, 396);
            tableLayoutPanel1.TabIndex = 10;
            // 
            // formsPlot1
            // 
            formsPlot1.AutoScroll = true;
            formsPlot1.DisplayScale = 1F;
            formsPlot1.Dock = DockStyle.Fill;
            formsPlot1.Location = new Point(13, 10);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(1023, 374);
            formsPlot1.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1212, 965);
            Controls.Add(panelCenter);
            Controls.Add(panelRight);
            Controls.Add(panelLeft);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            panelLeft.ResumeLayout(false);
            panelCenter.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private StatusStrip statusStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private Panel panelTop;
        private Panel panelBottom;
        private Panel panelLeft;
        private Panel panelRight;
        private Button button1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button button2;
        private Button button3;
        private Panel panelCenter;
        private TableLayoutPanel tableLayoutPanel2;
        private ScottPlot.WinForms.FormsPlot formsPlot2;
        private Splitter splitter2;
        private TableLayoutPanel tableLayoutPanel1;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private Splitter splitter1;
        private Splitter splitter3;
        private TableLayoutPanel tableLayoutPanel3;
        private ScottPlot.WinForms.FormsPlot formsPlot3;
        private Button button4;
    }
}

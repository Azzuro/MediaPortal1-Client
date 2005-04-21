using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// The setup form of this plugin
  /// </summary>
  public class SetupForm : Form
  {

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private Container components = null;

    private Label label1;
    private GroupBox groupBox1;
    private Label label2;
    private Label label3;
    private Label label4;
    private TextBox txtCols;
    private TextBox txtRows;
    private TextBox txtTim;
    private ComboBox cmbPort;
    private CheckBox cbLight;
    private Label label7;
    private ComboBox cmbType;
    private Label label8;
    private TextBox txtTimG;
    private TextBox txtRowsG;
    private TextBox txtColsG;
    private Label label9;
    private Label label10;
    private GroupBox gbTextMode;
    private GroupBox gbGraphMode;
    private CheckBox cbPropertyBrowser;
    private Button btnOK;
    private System.Windows.Forms.Button btnAdvanced;
    private IDisplay lcd = null;

    public SetupForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      cmbPort.SelectedIndex = 0;
      cmbType.DataSource = Settings.Instance.Drivers;
      cmbType.DisplayMember = "Description";
      cmbType.DataBindings.Add("SelectedItem", Settings.Instance, "LCDType");
      cmbType.SelectedIndex = 0;
      cmbPort.DataBindings.Add("SelectedItem", Settings.Instance, "Port");
      cbPropertyBrowser.DataBindings.Add("Checked", Settings.Instance, "ShowPropertyBrowser");
      cbLight.DataBindings.Add("Checked", Settings.Instance, "BackLight");
      txtCols.DataBindings.Add("Text", Settings.Instance, "TextWidth");
      txtRows.DataBindings.Add("Text", Settings.Instance, "TextHeight");
      txtColsG.DataBindings.Add("Text", Settings.Instance, "GraphicWidth");
      txtRowsG.DataBindings.Add("Text", Settings.Instance, "GraphicHeight");
      txtTim.DataBindings.Add("Text", Settings.Instance, "TextComDelay");
      txtTimG.DataBindings.Add("Text", Settings.Instance, "GraphicComDelay");
      lcd = Settings.Instance.LCDType;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnAdvanced = new System.Windows.Forms.Button();
      this.cmbPort = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.gbGraphMode = new System.Windows.Forms.GroupBox();
      this.label8 = new System.Windows.Forms.Label();
      this.txtTimG = new System.Windows.Forms.TextBox();
      this.txtRowsG = new System.Windows.Forms.TextBox();
      this.txtColsG = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.label10 = new System.Windows.Forms.Label();
      this.gbTextMode = new System.Windows.Forms.GroupBox();
      this.label2 = new System.Windows.Forms.Label();
      this.txtTim = new System.Windows.Forms.TextBox();
      this.txtRows = new System.Windows.Forms.TextBox();
      this.txtCols = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.cmbType = new System.Windows.Forms.ComboBox();
      this.cbLight = new System.Windows.Forms.CheckBox();
      this.cbPropertyBrowser = new System.Windows.Forms.CheckBox();
      this.btnOK = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.gbGraphMode.SuspendLayout();
      this.gbTextMode.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnAdvanced
      // 
      this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAdvanced.Location = new System.Drawing.Point(232, 176);
      this.btnAdvanced.Name = "btnAdvanced";
      this.btnAdvanced.Size = new System.Drawing.Size(88, 23);
      this.btnAdvanced.TabIndex = 70;
      this.btnAdvanced.Text = "&Advanced";
      this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
      // 
      // cmbPort
      // 
      this.cmbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbPort.Items.AddRange(new object[] {
                                                 "378",
                                                 "278",
                                                 "3BC",
                                                 "178",
                                                 "USB",
                                                 "COM1",
                                                 "COM2",
                                                 "COM3",
                                                 "COM4",
                                                 "NONE"});
      this.cmbPort.Location = new System.Drawing.Point(40, 48);
      this.cmbPort.Name = "cmbPort";
      this.cmbPort.Size = new System.Drawing.Size(64, 21);
      this.cmbPort.TabIndex = 20;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 48);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(32, 23);
      this.label1.TabIndex = 2;
      this.label1.Text = "Port";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.gbGraphMode);
      this.groupBox1.Controls.Add(this.gbTextMode);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.cmbType);
      this.groupBox1.Controls.Add(this.cbLight);
      this.groupBox1.Controls.Add(this.btnAdvanced);
      this.groupBox1.Controls.Add(this.cmbPort);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Location = new System.Drawing.Point(8, 8);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(334, 208);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Configuration";
      // 
      // gbGraphMode
      // 
      this.gbGraphMode.Controls.Add(this.label8);
      this.gbGraphMode.Controls.Add(this.txtTimG);
      this.gbGraphMode.Controls.Add(this.txtRowsG);
      this.gbGraphMode.Controls.Add(this.txtColsG);
      this.gbGraphMode.Controls.Add(this.label9);
      this.gbGraphMode.Controls.Add(this.label10);
      this.gbGraphMode.Location = new System.Drawing.Point(168, 72);
      this.gbGraphMode.Name = "gbGraphMode";
      this.gbGraphMode.Size = new System.Drawing.Size(152, 96);
      this.gbGraphMode.TabIndex = 72;
      this.gbGraphMode.TabStop = false;
      this.gbGraphMode.Text = "GraphMode";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(8, 16);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 23);
      this.label8.TabIndex = 3;
      this.label8.Text = "Columns";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtTimG
      // 
      this.txtTimG.Location = new System.Drawing.Point(88, 64);
      this.txtTimG.Name = "txtTimG";
      this.txtTimG.Size = new System.Drawing.Size(48, 20);
      this.txtTimG.TabIndex = 50;
      this.txtTimG.Text = "1";
      // 
      // txtRowsG
      // 
      this.txtRowsG.Location = new System.Drawing.Point(88, 40);
      this.txtRowsG.Name = "txtRowsG";
      this.txtRowsG.Size = new System.Drawing.Size(48, 20);
      this.txtRowsG.TabIndex = 40;
      this.txtRowsG.Text = "240";
      // 
      // txtColsG
      // 
      this.txtColsG.Location = new System.Drawing.Point(88, 16);
      this.txtColsG.Name = "txtColsG";
      this.txtColsG.Size = new System.Drawing.Size(48, 20);
      this.txtColsG.TabIndex = 30;
      this.txtColsG.Text = "320";
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(8, 64);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(80, 23);
      this.label9.TabIndex = 5;
      this.label9.Text = "Comm. Delay";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(8, 40);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(72, 23);
      this.label10.TabIndex = 4;
      this.label10.Text = "Rows";
      this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // gbTextMode
      // 
      this.gbTextMode.Controls.Add(this.label2);
      this.gbTextMode.Controls.Add(this.txtTim);
      this.gbTextMode.Controls.Add(this.txtRows);
      this.gbTextMode.Controls.Add(this.txtCols);
      this.gbTextMode.Controls.Add(this.label4);
      this.gbTextMode.Controls.Add(this.label3);
      this.gbTextMode.Location = new System.Drawing.Point(8, 72);
      this.gbTextMode.Name = "gbTextMode";
      this.gbTextMode.Size = new System.Drawing.Size(152, 96);
      this.gbTextMode.TabIndex = 71;
      this.gbTextMode.TabStop = false;
      this.gbTextMode.Text = "TextMode";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 16);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(64, 23);
      this.label2.TabIndex = 3;
      this.label2.Text = "Columns";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // txtTim
      // 
      this.txtTim.Location = new System.Drawing.Point(88, 64);
      this.txtTim.Name = "txtTim";
      this.txtTim.Size = new System.Drawing.Size(48, 20);
      this.txtTim.TabIndex = 50;
      this.txtTim.Text = "1";
      // 
      // txtRows
      // 
      this.txtRows.Location = new System.Drawing.Point(88, 40);
      this.txtRows.Name = "txtRows";
      this.txtRows.Size = new System.Drawing.Size(48, 20);
      this.txtRows.TabIndex = 40;
      this.txtRows.Text = "2";
      // 
      // txtCols
      // 
      this.txtCols.Location = new System.Drawing.Point(88, 16);
      this.txtCols.Name = "txtCols";
      this.txtCols.Size = new System.Drawing.Size(48, 20);
      this.txtCols.TabIndex = 30;
      this.txtCols.Text = "16";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 64);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 23);
      this.label4.TabIndex = 5;
      this.label4.Text = "Comm. Delay";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 40);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(72, 23);
      this.label3.TabIndex = 4;
      this.label3.Text = "Rows";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(8, 16);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(32, 23);
      this.label7.TabIndex = 11;
      this.label7.Text = "Type";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cmbType
      // 
      this.cmbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbType.Location = new System.Drawing.Point(40, 16);
      this.cmbType.Name = "cmbType";
      this.cmbType.Size = new System.Drawing.Size(286, 21);
      this.cmbType.TabIndex = 10;
      this.cmbType.SelectionChangeCommitted += new System.EventHandler(this.cmbType_SelectionChangeCommitted);
      // 
      // cbLight
      // 
      this.cbLight.Location = new System.Drawing.Point(8, 168);
      this.cbLight.Name = "cbLight";
      this.cbLight.Size = new System.Drawing.Size(80, 24);
      this.cbLight.TabIndex = 60;
      this.cbLight.Text = "BackLight";
      // 
      // cbPropertyBrowser
      // 
      this.cbPropertyBrowser.Location = new System.Drawing.Point(8, 224);
      this.cbPropertyBrowser.Name = "cbPropertyBrowser";
      this.cbPropertyBrowser.Size = new System.Drawing.Size(168, 24);
      this.cbPropertyBrowser.TabIndex = 4;
      this.cbPropertyBrowser.Text = "Show property browser";
      // 
      // btnOK
      // 
      this.btnOK.Location = new System.Drawing.Point(264, 224);
      this.btnOK.Name = "btnOK";
      this.btnOK.TabIndex = 5;
      this.btnOK.Text = "&OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // SetupForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(350, 254);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.cbPropertyBrowser);
      this.Controls.Add(this.groupBox1);
      this.Name = "SetupForm";
      this.Text = "ExternalDisplay Configuration";
      this.groupBox1.ResumeLayout(false);
      this.gbGraphMode.ResumeLayout(false);
      this.gbTextMode.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
/*
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
      Application.Run(new SetupForm());
    }
*/
    private void btnAdvanced_Click(object sender, EventArgs e)
    {
      this.Cursor = Cursors.WaitCursor;
      try
      {
        lcd.Configure();
      }
      finally
      {
        this.Cursor = Cursors.Default;
      }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      Settings.Save();
      this.Close();
    }

    private void cmbType_SelectionChangeCommitted(object sender, EventArgs e)
    {
      lcd = cmbType.SelectedItem as IDisplay;
      gbGraphMode.Visible = lcd.SupportsGraphics;
      gbTextMode.Visible = lcd.SupportsText;
      Settings.Instance.LCDType = lcd;
    }

  }
}
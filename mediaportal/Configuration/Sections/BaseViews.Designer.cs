﻿namespace MediaPortal.Configuration.Sections
{
  partial class BaseViews
  {
    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      this.groupBox = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lblViews = new MediaPortal.UserInterface.Controls.MPLabel();
      this.cbViews = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.lblViewName = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbViewName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.lblActionCodes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.btnDelete = new MediaPortal.UserInterface.Controls.MPButton();
      this.dgSelection = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgOperator = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgRestriction = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.dgViewAs = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgSortBy = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.dgAsc = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.dgSkip = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.groupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox.Controls.Add(this.lblViews);
      this.groupBox.Controls.Add(this.cbViews);
      this.groupBox.Controls.Add(this.lblViewName);
      this.groupBox.Controls.Add(this.tbViewName);
      this.groupBox.Controls.Add(this.dataGrid);
      this.groupBox.Controls.Add(this.lblActionCodes);
      this.groupBox.Controls.Add(this.btnSave);
      this.groupBox.Controls.Add(this.btnDelete);
      this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(472, 408);
      this.groupBox.TabIndex = 0;
      this.groupBox.TabStop = false;
      // 
      // lblViews
      // 
      this.lblViews.AutoSize = true;
      this.lblViews.Location = new System.Drawing.Point(13, 27);
      this.lblViews.Name = "lblViews";
      this.lblViews.Size = new System.Drawing.Size(33, 13);
      this.lblViews.TabIndex = 0;
      this.lblViews.Text = "View:";
      // 
      // cbViews
      // 
      this.cbViews.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbViews.BorderColor = System.Drawing.Color.Empty;
      this.cbViews.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbViews.Location = new System.Drawing.Point(145, 24);
      this.cbViews.Name = "cbViews";
      this.cbViews.Size = new System.Drawing.Size(311, 21);
      this.cbViews.TabIndex = 1;
      this.cbViews.SelectedIndexChanged += new System.EventHandler(this.cbViews_SelectedIndexChanged);
      // 
      // lblViewName
      // 
      this.lblViewName.AutoSize = true;
      this.lblViewName.Location = new System.Drawing.Point(13, 54);
      this.lblViewName.Name = "lblViewName";
      this.lblViewName.Size = new System.Drawing.Size(126, 13);
      this.lblViewName.TabIndex = 2;
      this.lblViewName.Text = "Name or Localized Code:";
      // 
      // tbViewName
      // 
      this.tbViewName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tbViewName.BorderColor = System.Drawing.Color.Empty;
      this.tbViewName.Location = new System.Drawing.Point(145, 51);
      this.tbViewName.Name = "tbViewName";
      this.tbViewName.Size = new System.Drawing.Size(311, 20);
      this.tbViewName.TabIndex = 3;
      // 
      // dataGrid
      // 
      this.dataGrid.AllowDrop = true;
      this.dataGrid.AllowUserToAddRows = false;
      this.dataGrid.AllowUserToDeleteRows = false;
      this.dataGrid.AllowUserToResizeColumns = false;
      this.dataGrid.AllowUserToResizeRows = false;
      this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightSteelBlue;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgSelection,
            this.dgOperator,
            this.dgRestriction,
            this.dgLimit,
            this.dgViewAs,
            this.dgSortBy,
            this.dgAsc,
            this.dgSkip});
      this.dataGrid.Location = new System.Drawing.Point(16, 78);
      this.dataGrid.MultiSelect = false;
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(440, 258);
      this.dataGrid.TabIndex = 4;
      this.dataGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
      this.dataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
      this.dataGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.OnDragOver);
      this.dataGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.OnCellPainting);
      this.dataGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGrid_CurrentCellDirtyStateChanged);
      this.dataGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dataGrid_DataError);
      this.dataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGrid_KeyDown);
      this.dataGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
      // 
      // lblActionCodes
      // 
      this.lblActionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblActionCodes.Location = new System.Drawing.Point(16, 339);
      this.lblActionCodes.Name = "lblActionCodes";
      this.lblActionCodes.Size = new System.Drawing.Size(440, 29);
      this.lblActionCodes.TabIndex = 7;
      this.lblActionCodes.Text = "Use the \"Ins\" and \"Del\" key to insert and delete lines. Drag the rows to change o" +
          "rder";
      this.lblActionCodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnSave
      // 
      this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSave.Location = new System.Drawing.Point(304, 376);
      this.btnSave.Name = "btnSave";
      this.btnSave.Size = new System.Drawing.Size(72, 22);
      this.btnSave.TabIndex = 5;
      this.btnSave.Text = "Save";
      this.btnSave.UseVisualStyleBackColor = true;
      this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
      // 
      // btnDelete
      // 
      this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDelete.Location = new System.Drawing.Point(384, 376);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new System.Drawing.Size(72, 22);
      this.btnDelete.TabIndex = 6;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
      // 
      // dgSelection
      // 
      this.dgSelection.HeaderText = "Selection";
      this.dgSelection.Name = "dgSelection";
      this.dgSelection.ToolTipText = "Select the field that should be retrieved from the database";
      this.dgSelection.Width = 80;
      // 
      // dgOperator
      // 
      this.dgOperator.HeaderText = "Operator";
      this.dgOperator.Name = "dgOperator";
      this.dgOperator.ToolTipText = "Choose an Operator for the Restriction";
      this.dgOperator.Width = 55;
      // 
      // dgRestriction
      // 
      this.dgRestriction.HeaderText = "Restriction";
      this.dgRestriction.Name = "dgRestriction";
      this.dgRestriction.ToolTipText = "Restrict the selected rows by the value specified";
      this.dgRestriction.Width = 75;
      // 
      // dgLimit
      // 
      this.dgLimit.HeaderText = "Limit";
      this.dgLimit.Name = "dgLimit";
      this.dgLimit.ToolTipText = "Enter a Limit for Rows to be returned";
      this.dgLimit.Width = 48;
      // 
      // dgViewAs
      // 
      this.dgViewAs.HeaderText = "ViewAs";
      this.dgViewAs.Name = "dgViewAs";
      this.dgViewAs.ToolTipText = "Select how the returned data should be shown";
      this.dgViewAs.Width = 60;
      // 
      // dgSortBy
      // 
      this.dgSortBy.HeaderText = "SortBy";
      this.dgSortBy.Name = "dgSortBy";
      this.dgSortBy.ToolTipText = "Choose the sort field";
      this.dgSortBy.Width = 60;
      // 
      // dgAsc
      // 
      this.dgAsc.FalseValue = "false";
      this.dgAsc.HeaderText = "Asc";
      this.dgAsc.Name = "dgAsc";
      this.dgAsc.ToolTipText = "Chose sort direction";
      this.dgAsc.TrueValue = "true";
      this.dgAsc.Width = 28;
      // 
      // dgSkip
      // 
      this.dgSkip.FalseValue = "false";
      this.dgSkip.HeaderText = "Skip";
      this.dgSkip.Name = "dgSkip";
      this.dgSkip.ToolTipText = "Don\'t display this level, if only 1 row is returned";
      this.dgSkip.TrueValue = "true";
      this.dgSkip.Width = 31;
      // 
      // BaseViews
      // 
      this.Controls.Add(this.groupBox);
      this.Name = "BaseViews";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox;
    private MediaPortal.UserInterface.Controls.MPComboBox cbViews;
    private MediaPortal.UserInterface.Controls.MPLabel lblViewName;
    private MediaPortal.UserInterface.Controls.MPTextBox tbViewName;
    private MediaPortal.UserInterface.Controls.MPLabel lblActionCodes;
    private MediaPortal.UserInterface.Controls.MPButton btnSave;
    private MediaPortal.UserInterface.Controls.MPButton btnDelete;
    private MediaPortal.UserInterface.Controls.MPLabel lblViews;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSelection;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgOperator;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgRestriction;
    private System.Windows.Forms.DataGridViewTextBoxColumn dgLimit;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgViewAs;
    private System.Windows.Forms.DataGridViewComboBoxColumn dgSortBy;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgAsc;
    private System.Windows.Forms.DataGridViewCheckBoxColumn dgSkip;
  }
}

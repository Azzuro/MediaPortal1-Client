#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using MediaPortal.GUI.View;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public partial class BaseViews : SectionSettings
  {
    #region Variables

    private DataGridView dataGrid;
    private DataTable datasetFilters;
    private ViewDefinition currentView;
    public ArrayList views;
    private bool updating = false;
    public bool settingsChanged = false;

    private List<string> _selections = new List<string>();
    private List<string> _sqloperators = new List<string>();
    private List<string> _viewsAs = new List<string>();
    private List<string> _sortBy = new List<string>();

    // Drag & Drop
    private int _dragDropCurrentIndex = -1;
    private Rectangle _dragDropRectangle;
    private int _dragDropSourceIndex;
    private int _dragDropTargetIndex;

    #endregion

    #region Properties

    public string[] Selections
    {
      get { return _selections.ToArray(); }
      set
      {
        _selections.Clear();
        _selections.AddRange(value);
      }
    }

    public string[] Sqloperators
    {
      get { return _sqloperators.ToArray(); }
      set
      {
        _sqloperators.Clear();
        _sqloperators.AddRange(value);
      }
    }

    public string[] ViewsAs
    {
      get { return _viewsAs.ToArray(); }
      set
      {
        _viewsAs.Clear();
        _viewsAs.AddRange(value);
      }
    }

    public string[] SortBy
    {
      get { return _sortBy.ToArray(); }
      set
      {
        _sortBy.Clear();
        _sortBy.AddRange(value);
      }
    }

    #endregion

    #region ctor

    public BaseViews()
      : base("<Unknown>")
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public BaseViews(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    #endregion

    #region Initialisation

    /// <summary>
    /// Set up the Datagrid column and the DataTable to which the grid is bound
    /// </summary>
    private void SetupGrid()
    {
      // Declare and initialize local variables used
      DataColumn dtCol = null; //Data Column variable
      string[] arrColumnNames = null; //string array variable

      // Fill the Combo Values
      foreach (string strText in Selections)
      {
        dgSelection.Items.Add(strText);
      }

      foreach (string strText in Sqloperators)
      {
        dgOperator.Items.Add(strText);
      }

      foreach (string strText in ViewsAs)
      {
        dgViewAs.Items.Add(strText);
      }

      foreach (string strText in SortBy)
      {
        dgSortBy.Items.Add(strText);
      }

      //Create the String array object, initialize the array with the column
      //names to be displayed
      arrColumnNames = new string[6];
      arrColumnNames[0] = "Selection";
      arrColumnNames[1] = "Operator";
      arrColumnNames[2] = "Restriction";
      arrColumnNames[3] = "Limit";
      arrColumnNames[4] = "ViewAs";
      arrColumnNames[5] = "SortBy";

      //Create the Data Table object which will then be used to hold
      //columns and rows
      datasetFilters = new DataTable("Selection");

      //Add the string array of columns to the DataColumn object       
      for (int i = 0; i < arrColumnNames.Length; i++)
      {
        string str = arrColumnNames[i];
        dtCol = new DataColumn(str);
        dtCol.DataType = Type.GetType("System.String");
        dtCol.DefaultValue = "";
        datasetFilters.Columns.Add(dtCol);
      }

      // Add a Column with checkbox at last in the Grid     
      DataColumn dtcCheck = new DataColumn("SortAsc"); //create the data column object
      dtcCheck.DataType = Type.GetType("System.Boolean"); //Set its data Type
      dtcCheck.DefaultValue = false; //Set the default value
      dtcCheck.AllowDBNull = false;
      datasetFilters.Columns.Add(dtcCheck); //Add the above column to the Data Table

      // Set the Data Properties for the field to map to the data table
      dgSelection.DataPropertyName = "Selection";
      dgOperator.DataPropertyName = "Operator";
      dgRestriction.DataPropertyName = "Restriction";
      dgLimit.DataPropertyName = "Limit";
      dgViewAs.DataPropertyName = "ViewAs";
      dgSortBy.DataPropertyName = "SortBy";
      dgAsc.DataPropertyName = "SortAsc"; // Set the data property for the grif

      //Set the Data Grid Source as the Data Table created above
      dataGrid.AutoGenerateColumns = false;
      dataGrid.DataSource = datasetFilters;
    }

    public void LoadViews()
    {
      updating = true;
      cbViews.Items.Clear();
      foreach (ViewDefinition view in views)
      {
        if (view.Name != string.Empty)
        {
          cbViews.Items.Add(view);
        }
      }
      ViewDefinition newDef = new ViewDefinition();
      newDef.Name = "new...";
      cbViews.Items.Add(newDef);
      if (cbViews.Items.Count > 0)
      {
        cbViews.SelectedIndex = 0;
      }

      UpdateView();
      updating = false;
    }

    #endregion

    #region Private Methods

    private void UpdateView()
    {
      updating = true;
      datasetFilters.Clear();
      currentView = (ViewDefinition)cbViews.SelectedItem;
      if (currentView == null)
      {
        return;
      }
      tbViewName.Text = currentView.Name;


      //fill in all rows...
      for (int i = 0; i < currentView.Filters.Count; ++i)
      {
        FilterDefinition def = (FilterDefinition)currentView.Filters[i];
        string limit = def.Limit.ToString();
        if (def.Limit < 0)
        {
          limit = "";
        }
        datasetFilters.Rows.Add(
          new object[]
            {
              def.Where,
              def.SqlOperator,
              def.Restriction,
              limit,
              def.DefaultView,
              def.DefaultSort,
              def.SortAscending,
            }
          );
      }

      updating = false;
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// A new View has selected. 
    /// Store the changes of the current one and fill the grid with the data of the selected one
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cbViews_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (updating)
      {
        return;
      }
      StoreGridInView();
      UpdateView();
    }

    /// <summary>
    /// The Save button has been pressed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave_Click(object sender, EventArgs e)
    {
      StoreGridInView();
    }

    /// <summary>
    /// Store the Grid Values in the View
    /// </summary>
    private void StoreGridInView()
    {
      if (updating)
      {
        return;
      }
      if (dataGrid.DataSource == null)
      {
        return;
      }
      if (currentView == null)
      {
        return;
      }
      settingsChanged = true;
      ViewDefinition view = currentView;
      DataTable dt = dataGrid.DataSource as DataTable;
      if (view.Name == "new...")
      {
        if (dt.Rows.Count == 0)
        {
          return;
        }
        view = new ViewDefinition();
        view.Name = tbViewName.Text;
        views.Add(view);
        currentView = view;
        cbViews.Items.Insert(cbViews.Items.Count - 1, view);
        updating = true;
        cbViews.SelectedItem = view;
        updating = false;
      }
      else
      {
        updating = true;
        view.Name = tbViewName.Text;
        int index = cbViews.Items.IndexOf(view);
        if (index >= 0)
        {
          cbViews.Items[index] = view;
        }
        cbViews.Update();
        updating = false;
      }
      view.Name = tbViewName.Text;
      view.Filters.Clear();

      foreach (DataRow row in dt.Rows)
      {
        FilterDefinition def = new FilterDefinition();
        def.Where = row[0] as string;
        if (def.Where == string.Empty)
        {
          continue;
        }
        def.SqlOperator = row[1].ToString();
        def.Restriction = row[2].ToString();
        try
        {
          def.Limit = Int32.Parse(row[3].ToString());
        }
        catch (Exception)
        {
          def.Limit = -1;
        }
        def.DefaultView = row[4].ToString();
        def.DefaultSort = row[5].ToString();
        def.SortAscending = (bool)row[6];
        view.Filters.Add(def);
      }
    }

    /// <summary>
    /// Delete the selected View
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnDelete_Click(object sender, EventArgs e)
    {
      ViewDefinition viewSelected = cbViews.SelectedItem as ViewDefinition;
      if (viewSelected == null)
      {
        return;
      }
      for (int i = 0; i < views.Count; ++i)
      {
        ViewDefinition view = views[i] as ViewDefinition;
        if (view == viewSelected)
        {
          views.RemoveAt(i);
          break;
        }
      }
      LoadViews();
    }

    /// <summary>
    /// Only allow valid values to be entered.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
      if (e.Exception == null) return;

      // If the user-specified value is invalid, cancel the change 
      if ((e.Context & DataGridViewDataErrorContexts.Commit) != 0 &&
          (typeof (FormatException).IsAssignableFrom(e.Exception.GetType()) ||
           typeof (ArgumentException).IsAssignableFrom(e.Exception.GetType())))
      {
        e.Cancel = true;
      }
      else
      {
        // Rethrow any exceptions that aren't related to the user input.
        e.ThrowException = true;
      }
    }

    /// <summary>
    /// Handles editing of data columns
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
    {
      // For combo box and check box cells, commit any value change as soon
      // as it is made rather than waiting for the focus to leave the cell.
      if (!dataGrid.CurrentCell.OwningColumn.GetType().Equals(typeof (DataGridViewTextBoxColumn)))
      {
        dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
    }

    /// <summary>
    /// Handle the Keypress for the action column
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void dataGrid_KeyDown(object sender, KeyEventArgs e)
    {
      if (dataGrid.CurrentRow == null)
      {
        return;
      }
      int rowSelected = dataGrid.CurrentRow.Index;
      if (rowSelected == -1 || rowSelected == datasetFilters.Rows.Count)
      {
        return;
      }

      switch (e.KeyCode)
      {
        case System.Windows.Forms.Keys.Insert:
          DataRow row = datasetFilters.NewRow();
          row[0] = row[1] = row[2] = row[3] = row[4] = row[5] = "";
          row[6] = false;
          datasetFilters.Rows.InsertAt(row, rowSelected + 1);
          e.Handled = true;
          break;
        case System.Windows.Forms.Keys.Delete:
          datasetFilters.Rows.RemoveAt(rowSelected);
          e.Handled = true;
          break;
      }
    }

    #region Drag & Drop

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
      //stores values for drag/drop operations if necessary
      if (dataGrid.AllowDrop)
      {
        int selectedRow = dataGrid.HitTest(e.X, e.Y).RowIndex;
        if (selectedRow > -1)
        {
          Size DragSize = SystemInformation.DragSize;
          _dragDropRectangle = new Rectangle(new Point(e.X - (DragSize.Width / 2), e.Y - (DragSize.Height / 2)), DragSize);
          _dragDropSourceIndex = selectedRow;
        }
      }
      else
      {
        _dragDropRectangle = Rectangle.Empty;
      }

      // Show Context Menu on Right Mouse Click
      if (e.Button == MouseButtons.Right)
        dataGrid.ContextMenu.Show(dataGrid, new Point(e.X, e.Y));

      base.OnMouseDown(e);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (dataGrid.AllowDrop)
      {
        if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
        {
          if (_dragDropRectangle != Rectangle.Empty && !_dragDropRectangle.Contains(e.X, e.Y))
          {
            DragDropEffects DropEffect = dataGrid.DoDragDrop(dataGrid.Rows[_dragDropSourceIndex],
                                                                     DragDropEffects.Move);
          }
        }
      }
      base.OnMouseMove(e);
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
      //runs while the drag/drop is in progress
      if (dataGrid.AllowDrop)
      {
        e.Effect = DragDropEffects.Move;
        int CurRow =
          dataGrid.HitTest(dataGrid.PointToClient(new Point(e.X, e.Y)).X,
                                   dataGrid.PointToClient(new Point(e.X, e.Y)).Y).RowIndex;
        if (_dragDropCurrentIndex != CurRow)
        {
          _dragDropCurrentIndex = CurRow;
          dataGrid.Invalidate(); //repaint
        }
      }
      base.OnDragOver(e);
    }

    private void OnDragDrop(object sender, DragEventArgs drgevent)
    {
      //runs after a drag/drop operation for column/row has completed
      if (dataGrid.AllowDrop)
      {
        if (drgevent.Effect == DragDropEffects.Move)
        {
          Point ClientPoint = dataGrid.PointToClient(new Point(drgevent.X, drgevent.Y));

          _dragDropTargetIndex = dataGrid.HitTest(ClientPoint.X, ClientPoint.Y).RowIndex;
          if (_dragDropTargetIndex > -1 && _dragDropCurrentIndex < dataGrid.RowCount - 1)
          {
            _dragDropCurrentIndex = -1;
            // Copy the existing row elements, before removing it from table
            DataRow row = datasetFilters.NewRow();
            for (int i = 0; i < 7; i++)
            {
              row[i] = datasetFilters.Rows[_dragDropSourceIndex][i];
            }
            datasetFilters.Rows.RemoveAt(_dragDropSourceIndex);

            if (_dragDropTargetIndex > _dragDropSourceIndex)
              _dragDropTargetIndex--;

            datasetFilters.Rows.InsertAt(row, _dragDropTargetIndex);

            dataGrid.ClearSelection();
            dataGrid.Rows[_dragDropTargetIndex].Selected = true;
          }
        }
      }
      base.OnDragDrop(drgevent);
    }

    private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      if (_dragDropCurrentIndex > -1)
      {
        if (e.RowIndex == _dragDropCurrentIndex && _dragDropCurrentIndex < dataGrid.RowCount - 1)
        {
          //if this cell is in the same row as the mouse cursor
          Pen p = new Pen(Color.Red, 1);
          e.Graphics.DrawLine(p, e.CellBounds.Left, e.CellBounds.Top - 1, e.CellBounds.Right, e.CellBounds.Top - 1);
        }
      }
    }

    #endregion

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Load the Views
    /// </summary>
    /// <param name="mediaType"></param>
    /// <param name="selections"></param>
    /// <param name="sqloperators"></param>
    /// <param name="viewsAs"></param>
    /// <param name="sortBy"></param>
    protected void LoadSettings(
      string mediaType,
      string[] selections,
      string[] sqloperators,
      string[] viewsAs,
      string[] sortBy
      )
    {
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      string defaultViews = Config.GetFile(Config.Dir.Base, "default" + mediaType + "Views.xml");
      Selections = selections;
      Sqloperators = sqloperators;
      ViewsAs = viewsAs;
      SortBy = sortBy;

      if (!File.Exists(customViews))
      {
        File.Copy(defaultViews, customViews);
      }

      views = new ArrayList();

      try
      {
        using (FileStream fileStream = new FileInfo(customViews).OpenRead())
        {
          SoapFormatter formatter = new SoapFormatter();
          views = (ArrayList)formatter.Deserialize(fileStream);
          fileStream.Close();
        }
      }
      catch (Exception) {}

      SetupGrid();
      LoadViews();
    }

    /// <summary>
    /// Save the Views
    /// </summary>
    /// <param name="mediaType"></param>
    protected void SaveSettings(string mediaType)
    {
      string customViews = Config.GetFile(Config.Dir.Config, mediaType + "Views.xml");
      if (settingsChanged)
      {
        try
        {
          using (FileStream fileStream = new FileInfo(customViews).OpenWrite())
          {
            SoapFormatter formatter = new SoapFormatter();
            formatter.Serialize(fileStream, views);
            fileStream.Close();
          }
        }
        catch (Exception) {}
      }
    }

    #endregion
  }
}
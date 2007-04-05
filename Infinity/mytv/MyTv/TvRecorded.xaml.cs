using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using TvDatabase;
using Gentle.Common;
using Gentle.Framework;
using Dialogs;
using TvControl;
using ProjectInfinity;
using ProjectInfinity.Logging;
using ProjectInfinity.Localisation;

namespace MyTv
{
  /// <summary>
  /// Interaction logic for TvRecorded.xaml
  /// </summary>

  public partial class TvRecorded : System.Windows.Controls.Page
  {
    #region enums and variables
    TvRecordedViewModel _model;
    #endregion

    #region ctor
    /// <summary>
    /// Initializes a new instance of the <see cref="TvRecorded"/> class.
    /// </summary>
    public TvRecorded()
    {
      InitializeComponent();
    }
    #endregion

    #region event handlers
    /// <summary>
    /// Called when screen is loaded
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      //create new view model
      _model = new TvRecordedViewModel(this);
      //and set the datacontext to our model
      gridMain.DataContext = _model;

      // Sets keyboard focus on the first Button in the sample.
      Keyboard.Focus(buttonView);

      //add some event handlers to keep mouse/keyboard focused together...
      Keyboard.AddPreviewKeyDownHandler(this, new KeyEventHandler(onKeyDown));
      Mouse.AddMouseMoveHandler(this, new MouseEventHandler(handleMouse));
      gridList.AddHandler(ListBoxItem.MouseDownEvent, new RoutedEventHandler(Button_Click), true);
      gridList.KeyDown += new KeyEventHandler(gridList_KeyDown);

      //Thread thumbNailThread = new Thread(new ThreadStart(CreateThumbnailsThread));
      //thumbNailThread.Start();
    }

    /// <summary>
    /// Event handler for mouse events
    /// When mouse enters an control, this method will give the control keyboardfocus
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
    void handleMouse(object sender, MouseEventArgs e)
    {
      FrameworkElement element = Mouse.DirectlyOver as FrameworkElement;
      while (element != null)
      {
        if (element as Button != null)
        {
          Keyboard.Focus((Button)element);
          return;
        }
        if (element as ListBoxItem != null)
        {
          gridList.SelectedItem = element.DataContext;
          Keyboard.Focus((ListBoxItem)element);
          return;
        }
        element = element.TemplatedParent as FrameworkElement;
      }
    }
    /// <summary>
    /// Event handler for OnKeyDown
    /// Handles some basic navigation
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    protected void onKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Left)
      {
        Keyboard.Focus(buttonView);
        e.Handled = true;
        return;
      }
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        //return to previous screen
        this.NavigationService.GoBack();
        return;
      }
      if (e.Key == System.Windows.Input.Key.X)
      {
        if (TvPlayerCollection.Instance.Count > 0)
        {
          this.NavigationService.Navigate(new Uri("/MyTv;component/TvFullScreen.xaml", UriKind.Relative));
          return;
        }
      }
    }
    /// <summary>
    /// Handles the KeyDown event of the gridList control.
    /// When keydown=enter, OnRecordingClicked() gets called
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    void gridList_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Enter)
      {
        OnRecordingClicked();
        e.Handled = true;
        return;
      }
    }
    void Button_Click(object sender, RoutedEventArgs e)
    {
      if (e.Source != gridList) return;
      OnRecordingClicked();
    }
    #endregion

    #region button handlers
    /// <summary>
    /// Called when user has clicked on a recording
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void OnRecordingClicked()
    {
      RecordingModel item = gridList.SelectedItem as RecordingModel;
      Recording recording = item.Recording;
      if (recording == null) return;
      MpMenu dlgMenu = new MpMenu();
      Window w = Window.GetWindow(this);
      dlgMenu.WindowStartupLocation = WindowStartupLocation.CenterOwner;
      dlgMenu.Owner = w;
      dlgMenu.Items.Clear();
      dlgMenu.Header = ServiceScope.Get<ILocalisation>().ToString("mytv", 68);//"Menu";
      dlgMenu.SubTitle = "";
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 92)/*Play recording*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 93)/*Delete recording*/));
      dlgMenu.Items.Add(new DialogMenuItem(ServiceScope.Get<ILocalisation>().ToString("mytv", 94)/*Settings*/));
      dlgMenu.ShowDialog();
      if (dlgMenu.SelectedIndex < 0) return;//nothing selected
      switch (dlgMenu.SelectedIndex)
      {
        case 0:
          {
            ICommand command = _model.Play;
            command.Execute(recording.FileName);
          }
          break;

        case 1:
          {
            ICommand command = _model.Delete;
            command.Execute(recording);
          }
          break;

        case 2:
          {
            TvRecordedInfo infopage = new TvRecordedInfo(recording);
            this.NavigationService.Navigate(infopage);
          }
          break;
      }
    }

    #endregion

    #region thumnail thread
    /// <summary>
    /// background thread which creates thumbnails for all recordings.
    /// </summary>
    void CreateThumbnailsThread()
    {
      System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Lowest;
      IList recordings = Recording.ListAll();
      foreach (Recording rec in recordings)
      {
        //ThumbnailGenerator generator = new ThumbnailGenerator();
        //if (generator.GenerateThumbnail(rec.FileName))
        //{
          //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new UpdateListDelegate(LoadRecordings));
        //}
      }
    }
    #endregion
  }
}
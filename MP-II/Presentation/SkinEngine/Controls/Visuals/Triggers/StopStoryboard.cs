#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using MediaPortal.Presentation.Properties;
using Presentation.SkinEngine.Controls.Animations;
using MediaPortal.Utilities.DeepCopy;
using Presentation.SkinEngine.Xaml.Interfaces;

namespace Presentation.SkinEngine.Controls.Visuals.Triggers
{
  public class StopStoryboard : TriggerAction
  {
    #region Private fields

    Property _beginStoryBoardProperty;

    #endregion

    #region Ctor

    public StopStoryboard()
    {
      Init();
    }

    void Init()
    {
      _beginStoryBoardProperty = new Property(typeof(string), null);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      StopStoryboard s = source as StopStoryboard;
      BeginStoryboardName = copyManager.GetCopy(s.BeginStoryboardName);
    }

    #endregion

    public Property BeginStoryboardNameProperty
    {
      get { return _beginStoryBoardProperty; }
      set { _beginStoryBoardProperty = value; }
    }

    public string BeginStoryboardName
    {
      get { return _beginStoryBoardProperty.GetValue() as string; }
      set { _beginStoryBoardProperty.SetValue(value); }
    }

    public override void Execute(UIElement element, TriggerBase trigger)
    {
      INameScope ns = FindNameScope();
      BeginStoryboard beginAction = null;
      if (ns != null)
        beginAction = ns.FindName(BeginStoryboardName) as BeginStoryboard;
      if (beginAction != null)
        element.StopStoryboard(beginAction.Storyboard as Storyboard);
    }
  }
}

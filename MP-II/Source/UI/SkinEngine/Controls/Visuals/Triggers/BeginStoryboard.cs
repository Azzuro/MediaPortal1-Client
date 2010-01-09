#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
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

using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.Controls.Animations;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals.Triggers
{
  public class BeginStoryboard : TriggerAction
  {
    #region Private fields

    protected AbstractProperty _storyBoardProperty;
    protected AbstractProperty _nameProperty;
    protected AbstractProperty _handoffBehaviorProperty;

    #endregion

    #region Ctor

    public BeginStoryboard()
    {
      Init();
    }

    void Init()
    {
      _storyBoardProperty = new SProperty(typeof(Storyboard), null);
      _nameProperty = new SProperty(typeof(string), "");
      _handoffBehaviorProperty = new SProperty(typeof(HandoffBehavior), Animations.HandoffBehavior.SnapshotAndReplace);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      BeginStoryboard s = (BeginStoryboard) source;
      Storyboard = copyManager.GetCopy(s.Storyboard);
      Name = copyManager.GetCopy(s.Name);
      HandoffBehavior = copyManager.GetCopy(s.HandoffBehavior);
    }

    #endregion

    #region Public properties

    public AbstractProperty StoryboardProperty
    {
      get { return _storyBoardProperty; }
    }

    public Storyboard Storyboard
    {
      get { return (Storyboard) _storyBoardProperty.GetValue(); }
      set { _storyBoardProperty.SetValue(value); }
    }


    public AbstractProperty NameProperty
    {
      get { return _nameProperty; }
    }

    public string Name
    {
      get { return _nameProperty.GetValue() as string; }
      set { _nameProperty.SetValue(value); }
    }

    public AbstractProperty HandoffBehaviorProperty
    {
      get { return _handoffBehaviorProperty; }
    }

    public HandoffBehavior HandoffBehavior
    {
      get { return (HandoffBehavior) _handoffBehaviorProperty.GetValue(); }
      set { _handoffBehaviorProperty.SetValue(value); }
    }

    #endregion

    public override void Execute(UIElement element)
    {
      if (Storyboard != null)
        element.StartStoryboard(Storyboard, HandoffBehavior);
    }
  }
}

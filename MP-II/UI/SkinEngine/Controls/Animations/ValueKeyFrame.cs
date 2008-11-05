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

using System;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.SkinEngine.Controls.Animations
{
  public abstract class ValueKeyFrame<T>: DependencyObject, IKeyFrame
  {
    #region Private fields

    Property _keyTimeProperty;
    Property _keyValueProperty;

    #endregion

    #region Ctor

    public ValueKeyFrame()
    {
      Init();
    }

    void Init()
    {
      _keyTimeProperty = new Property(typeof(TimeSpan), new TimeSpan(0, 0, 0));
      _keyValueProperty = new Property(typeof(T), null); // Will be initialized in subclasses
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      ValueKeyFrame<T> kf = (ValueKeyFrame<T>) source;
      KeyTime = copyManager.GetCopy(kf.KeyTime);
      Value = copyManager.GetCopy(kf.Value);
    }

    #endregion

    #region properties

    public Property KeyTimeProperty
    {
      get { return _keyTimeProperty; }
    }

    public Property ValueProperty
    {
      get { return _keyValueProperty; }
    }

    public T Value
    {
      get { return (T) _keyValueProperty.GetValue(); }
      set { _keyValueProperty.SetValue(value); }
    }

    #endregion

    #region IKeyFrame implementation

    public TimeSpan KeyTime
    {
      get { return (TimeSpan)_keyTimeProperty.GetValue(); }
      set { _keyTimeProperty.SetValue(value); }
    }

    object IKeyFrame.Value
    {
      get { return this.Value; }
      set { this.Value = (T) value; }
    }

    #endregion
  }
}

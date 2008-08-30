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
using System.Collections.Generic;
using MediaPortal.SkinEngine.Controls.Visuals;
using MediaPortal.SkinEngine.Xaml;
using MediaPortal.SkinEngine.Xaml.Interfaces;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.SkinEngine.Controls.Animations
{
  /// <summary>
  /// Timeline context class for <see cref="PropertyAnimationTimeline"/>s.
  /// </summary>
  internal class PropertyAnimationTimelineContext : TimelineContext
  {
    protected IDataDescriptor _dataDescriptor;
    protected object _startValue = null;
    protected object _originalValue = null;

    public PropertyAnimationTimelineContext(UIElement element)
      : base(element)
    { }

    public IDataDescriptor DataDescriptor
    {
      get { return _dataDescriptor; }
      set { _dataDescriptor = value; }
    }

    public object StartValue
    {
      get { return _startValue; }
      set { _startValue = value; }
    }

    public object OriginalValue
    {
      get { return _originalValue; }
      set { _originalValue = value; }
    }
  }

  /// <summary>
  /// Base class for all property animations.
  /// </summary>
  public class PropertyAnimationTimeline: Timeline
  {
    #region Protected fields

    protected PathExpression _propertyExpression = null;

    #endregion

    #region Ctor

    /// <summary>
    /// Creates a new <see cref="PropertyAnimationTimeline"/> for use in XAML.
    /// Method <see cref="Initialize(IParserContext)"/> will have to be called to complete the
    /// initialization.
    /// </summary>
    public PropertyAnimationTimeline()
    {
      Duration = new TimeSpan(0, 0, 1);
    }

    /// <summary>
    /// Creates a new <see cref="PropertyAnimationTimeline"/> for use in Code.
    /// </summary>
    public PropertyAnimationTimeline(PathExpression animatePropertyExpression): this()
    {
      _propertyExpression = animatePropertyExpression;
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      PropertyAnimationTimeline t = source as PropertyAnimationTimeline;
      _propertyExpression = copyManager.GetCopy(t._propertyExpression);
    }

    #endregion

    #region Animation methods

    public override void Start(TimelineContext context, uint timePassed)
    {
      PropertyAnimationTimelineContext patc = context as PropertyAnimationTimelineContext;
      if (patc.DataDescriptor == null)
        return;
      else
        patc.State = State.Idle;
      base.Start(context, timePassed);
    }

    protected IDataDescriptor GetDataDescriptor(UIElement element)
    {
      string targetName = Storyboard.GetTargetName(this);
      object targetObject = element.FindElement(new NameFinder(targetName));
      if (targetObject == null)
        return null;
      IDataDescriptor result = new ValueDataDescriptor(targetObject);
      if (_propertyExpression == null || !_propertyExpression.Evaluate(result, out result))
        return null;
      return result;
    }

    public override TimelineContext CreateTimelineContext(UIElement element)
    {
      PropertyAnimationTimelineContext result = new PropertyAnimationTimelineContext(element);
      result.DataDescriptor = GetDataDescriptor(element);
      return result;
    }

    public override void AddAllAnimatedProperties(TimelineContext context,
        IDictionary<IDataDescriptor, object> result)
    {
      PropertyAnimationTimelineContext patc = context as PropertyAnimationTimelineContext;
      if (patc.DataDescriptor == null)
        return;
      result.Add(patc.DataDescriptor, patc.OriginalValue);
    }

    public override void Setup(TimelineContext context,
        IDictionary<IDataDescriptor, object> propertyConfigurations)
    {
      base.Setup(context, propertyConfigurations);
      PropertyAnimationTimelineContext patc = context as PropertyAnimationTimelineContext;
      if (patc.DataDescriptor == null)
        return;
      if (propertyConfigurations.ContainsKey(patc.DataDescriptor))
        patc.OriginalValue = propertyConfigurations[patc.DataDescriptor];
      else
        patc.OriginalValue = patc.DataDescriptor.Value;
      patc.StartValue = patc.DataDescriptor.Value;
    }

    public override void Reset(TimelineContext context)
    {
      PropertyAnimationTimelineContext patc = context as PropertyAnimationTimelineContext;
      if (patc.DataDescriptor == null)
        return;
      patc.DataDescriptor.Value = patc.OriginalValue;
    }

    #endregion

    #region Base overrides

    public override void Initialize(IParserContext context)
    {
      base.Initialize(context);
      if (String.IsNullOrEmpty(Storyboard.GetTargetName(this)) || String.IsNullOrEmpty(Storyboard.GetTargetProperty(this)))
      {
        _propertyExpression = null;
        return;
      }
      string targetProperty = Storyboard.GetTargetProperty(this);
      _propertyExpression = PathExpression.Compile(context, targetProperty);
    }

    #endregion
  }
}

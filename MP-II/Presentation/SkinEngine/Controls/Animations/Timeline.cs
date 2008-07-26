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
using MediaPortal.Presentation.Properties;
using Presentation.SkinEngine.Controls;
using Presentation.SkinEngine.Controls.Visuals;
using MediaPortal.Utilities.DeepCopy;
using Presentation.SkinEngine.Xaml;

namespace Presentation.SkinEngine.Controls.Animations
{
  public enum RepeatBehavior { None, Forever };
  public enum FillBehavior { HoldEnd, Stop };
  // TODO Albert78: new HandoffBehavior TemporaryReplace
  public enum HandoffBehavior { Compose, SnapshotAndReplace };

  public abstract class Timeline: DependencyObject
  {
    #region Protected fields

    protected Property _beginTimeProperty;
    protected Property _accellerationProperty;
    protected Property _autoReverseProperty;
    protected Property _decelerationRatioProperty;
    protected Property _durationProperty;
    protected Property _repeatBehaviourProperty;
    protected Property _fillBehaviourProperty;

    #endregion

    #region Ctor

    public Timeline()
    {
      Init();
    }

    void Init()
    {
      _beginTimeProperty = new Property(typeof(TimeSpan), new TimeSpan(0, 0, 0));
      _accellerationProperty = new Property(typeof(double), 1.0);
      _autoReverseProperty = new Property(typeof(bool), false);
      _decelerationRatioProperty = new Property(typeof(double), 1.0);
      _durationProperty = new Property(typeof(TimeSpan?), null);
      _repeatBehaviourProperty = new Property(typeof(RepeatBehavior), RepeatBehavior.None);
      _fillBehaviourProperty = new Property(typeof(FillBehavior), FillBehavior.HoldEnd);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      base.DeepCopy(source, copyManager);
      Timeline t = source as Timeline;
      BeginTime = copyManager.GetCopy(t.BeginTime);
      Accelleration = copyManager.GetCopy(t.Accelleration);
      AutoReverse = copyManager.GetCopy(t.AutoReverse);
      DecelerationRatio = copyManager.GetCopy(t.DecelerationRatio);
      _durationProperty.SetValue(copyManager.GetCopy(t._durationProperty.GetValue())); // Copying of a Nullable<TimeSpan>
      FillBehavior = copyManager.GetCopy(t.FillBehavior);
      RepeatBehavior = copyManager.GetCopy(t.RepeatBehavior);
    }

    #endregion

    #region Public properties

    public Property BeginTimeProperty
    {
      get { return _beginTimeProperty; }
    }

    public TimeSpan BeginTime
    {
      get { return (TimeSpan) _beginTimeProperty.GetValue(); }
      set { _beginTimeProperty.SetValue(value); }
    }

    public Property AccellerationProperty
    {
      get { return _accellerationProperty; }
    }

    public double Accelleration
    {
      get { return (double) _accellerationProperty.GetValue(); }
      set { _accellerationProperty.SetValue(value); }
    }

    public Property AutoReverseProperty
    {
      get { return _autoReverseProperty; }
    }

    public bool AutoReverse
    {
      get { return (bool) _autoReverseProperty.GetValue(); }
      set { _autoReverseProperty.SetValue(value); }
    }

    public Property DecelerationRatioProperty
    {
      get { return _decelerationRatioProperty; }
      set { _decelerationRatioProperty = value; }
    }

    public double DecelerationRatio
    {
      get { return (double) _decelerationRatioProperty.GetValue(); }
      set { _decelerationRatioProperty.SetValue(value); }
    }

    public Property DurationProperty
    {
      get { return _durationProperty; }
    }

    public TimeSpan Duration
    {
      get { return ((TimeSpan?) _durationProperty.GetValue()).GetValueOrDefault(); }
      set { _durationProperty.SetValue(value); }
    }

    public bool DurationSet
    {
      get { return ((TimeSpan?) _durationProperty.GetValue()).HasValue; }
    }

    public Property RepeatBehaviorProperty
    {
      get { return _repeatBehaviourProperty; }
    }

    public RepeatBehavior RepeatBehavior
    {
      get { return (RepeatBehavior) _repeatBehaviourProperty.GetValue(); }
      set { _repeatBehaviourProperty.SetValue(value); }
    }

    public Property FillBehaviourProperty
    {
      get { return _fillBehaviourProperty; }
    }

    public FillBehavior FillBehavior
    {
      get { return (FillBehavior) _fillBehaviourProperty.GetValue(); }
      set { _fillBehaviourProperty.SetValue(value); }
    }

    #endregion

    #region Animation control methods

    /// <summary>
    /// Starts this animation and initializes the time counter for this animation.
    /// </summary>
    /// <remarks>
    /// For a normal operation, method <see cref="Animate(TimelineContext,uint)"/>
    /// should be called frequently, until method <see cref="HasEnded(TimelineContext)"/>
    /// returns true. After that, method <see cref="Stop(TimelineContext)"/> has
    /// to be called to correctly restore animation property values and to set the
    /// animation's final state.
    /// </remarks>
    public virtual void Start(TimelineContext context, uint timePassed)
    {
      Stop(context);
      Started(context, timePassed);
    }

    public virtual void Stop(TimelineContext context)
    {
      Stopped(context, FillBehavior == Animations.FillBehavior.Stop);
    }

    /// <summary>
    /// Entry method to execute the animation. This method will evaluate all
    /// animation control properties defined in this class, calculate a value for the internal
    /// time counter and delegate to method <see cref="DoAnimation(TimelineContext,uint)"/>.
    /// </summary>
    public void Animate(TimelineContext context, uint timePassed)
    {
      uint passed = (timePassed - context.TimeStarted);

      switch (context.State)
      {
        case State.WaitBegin:
          if (passed >= BeginTime.TotalMilliseconds)
          {
            passed = 0;
            context.TimeStarted = timePassed;
            context.State = State.Running;
            goto case State.Running;
          }
          break;

        case State.Running:
          if (!DurationSet || passed < Duration.TotalMilliseconds)
            DoAnimation(context, passed);
          else
          {
            if (AutoReverse)
            {
              context.State = State.Reverse;
              context.TimeStarted = timePassed;
              passed = 0;
              goto case State.Reverse;
            }
            else if (RepeatBehavior == RepeatBehavior.Forever)
            {
              context.TimeStarted = timePassed;
              DoAnimation(context, timePassed - context.TimeStarted);
            }
            else
            {
              DoAnimation(context, (uint) Duration.TotalMilliseconds);
              Stop(context);
            }
          }
          break;

        case State.Reverse:
          if (!DurationSet)
            return;
          if (passed < Duration.TotalMilliseconds)
            DoAnimation(context, (uint) (Duration.TotalMilliseconds - passed));
          else
          {
            if (RepeatBehavior == RepeatBehavior.Forever)
            {
              context.State = State.Running;
              context.TimeStarted = timePassed;
              DoAnimation(context, timePassed - context.TimeStarted);
            }
            else
            {
              DoAnimation(context, 0);
              Stop(context);
            }
          }
          break;
      }
    }

    #endregion

    #region Animation state methods

    /// <summary>
    /// Creates a timeline context object needed for this class of timeline.
    /// </summary>
    /// <returns>Instance of a subclass of <see cref="TimelineContext"/>, which will
    /// fit the need of this class.</returns>
    /// <remarks>
    /// The needs of <see cref="Timeline"/> subclasses for a context object are different.
    /// Timeline groups will, for example, employ a context class which can hold different
    /// contexts for each child. Animations will need to put current values in their context.
    /// So subclasses of <see cref="Timeline"/> will employ their own variation of a
    /// <see cref="TimelineContext"/>.
    /// The returned context object will be used throughout the animation for this
    /// class in every call to any animation method.
    /// </remarks>
    public abstract TimelineContext CreateTimelineContext(UIElement element);

    /// <summary>
    /// Adds the descriptors for all properties which are animated by this animation and
    /// by sub animations to the specified <paramref name="result"/> parameter.
    /// </summary>
    /// <param name="context">The animation context.</param>
    /// <param name="result">Dictionary to add all animated properties from this animation.</param>
    public abstract void AddAllAnimatedProperties(TimelineContext context,
        IDictionary<IDataDescriptor, object> result);

    /// <summary>
    /// Sets up the specified <paramref name="context"/> object with all necessary
    /// values for this timeline.
    /// </summary>
    /// <param name="context">The animation context.</param>
    /// <param name="propertyConfigurations">Data descriptors which were animated by a
    /// predecessor animation, mapped to their original values.
    /// The original value for all data descriptors contained in this map should be
    /// initialized with the mapped value instead of the current value.</param>
    public virtual void Setup(TimelineContext context,
        IDictionary<IDataDescriptor, object> propertyConfigurations)
    {
      context.State = State.Setup;
    }

    /// <summary>
    /// Will restore the original values in all properties which have been animated
    /// by this timeline.
    /// </summary>
    /// <param name="context">Current animation context.</param>
    public virtual void Reset(TimelineContext context)
    { }

    /// <summary>
    /// Will do the real work of animating the underlaying property.
    /// </summary>
    /// <remarks>
    /// The animation progress should be calculated to the specified relative
    /// time. It is not necessary to evaluate time overflows or to revert
    /// the animation depending on properties; these calculations have been
    /// done before this method will be called and will be reflected in the
    /// <paramref name="reltime"/> parameter. 
    /// </remarks>
    /// <param name="reltime">This parameter holds the relative animation time in
    /// milliseconds from the <see cref="BeginTime"/> on, up to a maximum value
    /// of Duration.Milliseconds.</param>
    protected virtual void DoAnimation(TimelineContext context, uint reltime)
    { }

    /// <summary>
    /// Will be called if this timeline was started.
    /// </summary>
    /// <param name="context">Current animation context.</param>
    /// <param name="timePassed">The time counter for this animation. All further
    /// time values will be relative to this specified time.</param>
    public virtual void Started(TimelineContext context, uint timePassed)
    {
      context.TimeStarted = timePassed;
      context.State = State.WaitBegin;
    }

    /// <summary>
    /// Will be called if this timeline was stopped.
    /// </summary>
    /// <param name="context">Current animation context.</param>
    /// <param name="forceReset">If this value is <c>true</c>, this animation will
    /// reset the animation property to its original value.</param>
    public virtual void Stopped(TimelineContext context, bool forceReset)
    {
      if (forceReset)
        Reset(context);
      context.State = State.Idle;
    }

    /// <summary>
    /// Gets a value indicating whether this timeline has finished its execution or
    /// was stopped.
    /// This method will will be overridden by composed timelines where the result
    /// will depend on their composition parts.
    /// </summary>
    /// <param name="context">Current animation context.</param>
    /// <returns>
    /// <c>true</c> if this animation has ended or was stopped; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool IsStopped(TimelineContext context)
    {
      return context.State == State.Idle;
    }

    #endregion
  }
}

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
using System.Globalization;
using System.Timers;
using MediaPortal.Core;
using MediaPortal.Core.Messaging;
using MediaPortal.Core.Settings;
using MediaPortal.Presentation.DataObjects;
using MediaPortal.Presentation.Localization;
using UiComponents.SkinBase.Settings;

namespace UiComponents.SkinBase
{
  /// <summary>
  /// Model which provides data for the skin clock.
  /// </summary>
  public class TimeModel : IDisposable
  {
    #region Protected fields

    protected Timer _timer = null;

    protected string _dateFormat = "D";
    protected string _timeFormat = "t";

    protected Property _currentTimeProperty = new Property(typeof(string), string.Empty);
    protected Property _currentDateProperty = new Property(typeof(string), string.Empty);

    protected Property _hourAngleProperty = new Property(typeof(double), 0.0);
    protected Property _minuteAngleProperty = new Property(typeof(double), 0.0);

    #endregion

    public TimeModel()
    {
      ReadSettings();
      Update();
      SubscribeToMessages();
    }

    public void Dispose()
    {
      StopListening();
      UnsubscribeFromMessages();
    }

    protected void SubscribeToMessages()
    {
      IMessageBroker broker = ServiceScope.Get<IMessageBroker>();
      broker.GetOrCreate(SkinMessaging.Queue).MessageReceived += OnSkinMessageReceived;

      ISystemStateService systemStateService = ServiceScope.Get<ISystemStateService>();
      if (systemStateService.CurrentState == SystemState.Started)
        StartListening();

      broker.GetOrCreate(SystemMessaging.QUEUE).MessageReceived += OnSystemMessageReceived;
    }

    protected void UnsubscribeFromMessages()
    {
      IMessageBroker broker = ServiceScope.Get<IMessageBroker>();
      broker.GetOrCreate(SkinMessaging.Queue).MessageReceived -= OnSkinMessageReceived;
      broker.GetOrCreate(SystemMessaging.QUEUE).MessageReceived -= OnSystemMessageReceived;
    }

    protected void StartListening()
    {
      if (_timer != null)
        return;
      // Setup timer to update the properties
      _timer = new Timer(500);
      _timer.Elapsed += OnTimerElapsed;
      _timer.Enabled = true;
    }

    protected void StopListening()
    {
      if (_timer == null)
        return;
      _timer.Enabled = false;
      _timer.Elapsed -= OnTimerElapsed;
      _timer = null;
    }

    protected void ReadSettings()
    {
      ISettingsManager settingsManager = ServiceScope.Get<ISettingsManager>();
      SkinBaseSettings settings = settingsManager.Load<SkinBaseSettings>();
      _dateFormat = settings.DateFormat;
      _timeFormat = settings.TimeFormat;
    }

    protected void OnSkinMessageReceived(QueueMessage message)
    {
      if (((SkinMessaging.NotificationType) message.MessageData[SkinMessaging.Notification]) ==
          SkinMessaging.NotificationType.DateTimeFormatChanged)
        // The DateFormat and TimeFormat configuration classes will send this message when they
        // changed the formats, so we have to update our format here
        ReadSettings();
    }

    protected void OnSystemMessageReceived(QueueMessage message)
    {
      SystemMessaging.MessageType messageType =
          (SystemMessaging.MessageType) message.MessageData[SystemMessaging.MESSAGE_TYPE];
      if (messageType == SystemMessaging.MessageType.SystemStateChanged)
      {
        SystemState state = (SystemState) message.MessageData[SystemMessaging.PARAM];
        switch (state)
        {
          case SystemState.Started:
            StartListening();
            break;
          case SystemState.ShuttingDown:
            Dispose();
            break;
        }
      }
    }

    protected void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
      Update();
    }

    protected void Update()
    {
      ILocalization localization = ServiceScope.Get<ILocalization>();
      CultureInfo culture = localization.CurrentCulture;

      DateTime now = DateTime.Now;

      double Angle = now.Hour * 30;
      HourAngle = Angle + 12 * now.Minute / 60.0;

      MinuteAngle = now.Minute * 6;

      CurrentTime = now.ToString(_timeFormat, culture);
      CurrentDate = now.ToString(_dateFormat, culture);
    }

    public Property CurrentDateProperty
    {
      get { return _currentDateProperty; }
    }

    public string CurrentDate
    {
      get { return _currentDateProperty.GetValue() as string; }
      set { _currentDateProperty.SetValue(value); }
    }

    public Property CurrentTimeProperty
    {
      get { return _currentTimeProperty; }
    }

    public string CurrentTime
    {
      get { return _currentTimeProperty.GetValue() as string; }
      set { _currentTimeProperty.SetValue(value); }
    }

    public Property HourAngleProperty
    {
      get { return _hourAngleProperty; }
    }

    public double HourAngle
    {
      get { return (double)_hourAngleProperty.GetValue(); }
      set { _hourAngleProperty.SetValue(value); }
    }

    public Property MinuteAngleProperty
    {
      get { return _minuteAngleProperty; }
    }

    public double MinuteAngle
    {
      get { return (double)_minuteAngleProperty.GetValue(); }
      set { _minuteAngleProperty.SetValue(value); }
    }
  }
}

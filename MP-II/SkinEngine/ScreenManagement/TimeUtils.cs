
using System;
using System.Timers;
using MediaPortal.Core;
using MediaPortal.Utilities;
using MediaPortal.Presentation.Localisation;
using MediaPortal.Presentation.DataObjects;

namespace MediaPortal.SkinEngine
{
  public class TimeUtils
  {
    /// <summary>
    /// Singleton instance variable.
    /// </summary>
    private static TimeUtils _instance;

    Timer _timer;

    // Todo: read this from settings
    string _dateFormat = "<Day> <Month> <DD>";
    string _timeFormat = "<h>:<M>";

    Property _currentTimeProperty = new Property(typeof(string), string.Empty);
    Property _currentDateProperty = new Property(typeof(string), string.Empty);

    Property _hourAngleProperty = new Property(typeof(double), 0.0);
    Property _minuteAngleProperty = new Property(typeof(double), 0.0);

    private TimeUtils()
    {
      _update();

      //setup a timer 
      _timer = new Timer(500);
      _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
      _timer.Enabled = true;
    }

    /// <summary>
    /// Returns the Singleton TimeUtils instance.
    /// </summary>
    /// <value>The Singleton TimeUtils instance.</value>
    public static TimeUtils Instance
    {
      get
      {
        if (_instance == null)
          _instance = new TimeUtils();
        return _instance;
      }
    }

    public Property CurrentDateProperty
    {
      get
      {
        return _currentDateProperty;
      }
      set
      {
        _currentDateProperty = value;
      }
    }

    public string CurrentDate
    {
      get
      {
        return _currentDateProperty.GetValue() as string;
      }
      set
      {
        _currentDateProperty.SetValue(value);
      }
    }


    public Property CurrentTimeProperty
    {
      get
      {
        return _currentTimeProperty;
      }
      set
      {
        _currentTimeProperty = value;
      }
    }

    public string CurrentTime
    {
      get
      {
        return _currentTimeProperty.GetValue() as string;
      }
      set
      {
        _currentTimeProperty.SetValue(value);
      }
    }


    public Property HourAngleProperty
    {
      get
      {
        return _hourAngleProperty;
      }
      set
      {
        _hourAngleProperty = value;
      }
    }

    public double HourAngle
    {
      get
      {
        return (double)_hourAngleProperty.GetValue();
      }
      set
      {
        _hourAngleProperty.SetValue(value);
      }
    }
    public Property MinuteAngleProperty
    {
      get
      {
        return _minuteAngleProperty;
      }
      set
      {
        _minuteAngleProperty = value;
      }
    }

    public double MinuteAngle
    {
      get
      {
        return (double)_minuteAngleProperty.GetValue();
      }
      set
      {
        _minuteAngleProperty.SetValue(value);
      }
    }

    /// <summary>
    /// Formats the date based on the user preferences.
    /// </summary>
    /// <returns>A string containing the localized version of the date.</returns>
    public string FormatDate(DateTime cur)
    {
      string dateString = _dateFormat;

      string day;

      switch (cur.DayOfWeek)
      {
        case DayOfWeek.Monday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "1");
          break;
        case DayOfWeek.Tuesday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "2");
          break;
        case DayOfWeek.Wednesday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "3");
          break;
        case DayOfWeek.Thursday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "4");
          break;
        case DayOfWeek.Friday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "5");
          break;
        case DayOfWeek.Saturday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "6");
          break;
        case DayOfWeek.Sunday:
          day = ServiceScope.Get<ILocalisation>().ToString("days", "0");;
          break;
        default:
          throw new ArgumentException("Invalid DayOfWeek");
      }
      string month;
      switch (cur.Month)
      {
        case 1:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "1");
          break;
        case 2:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "2");
          break;
        case 3:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "3");
          break;
        case 4:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "4");
          break;
        case 5:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "5");
          break;
        case 6:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "6");
          break;
        case 7:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "7");
          break;
        case 8:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "8");
          break;
        case 9:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "9");
          break;
        case 10:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "10");
          break;
        case 11:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "11");
          break;
        case 12:
          month = ServiceScope.Get<ILocalisation>().ToString("months", "12");
          break;
        default:
          throw new ArgumentException("Invalid Month");
      }
      StringUtils.ReplaceTag(ref dateString, "<Day>", day);
      StringUtils.ReplaceTag(ref dateString, "<DD>", cur.Day.ToString());
      StringUtils.ReplaceTag(ref dateString, "<Month>", month);
      StringUtils.ReplaceTag(ref dateString, "<MM>", cur.Month.ToString());
      StringUtils.ReplaceTag(ref dateString, "<Year>", cur.Year.ToString());
      StringUtils.ReplaceTag(ref dateString, "<YY>", (cur.Year - 2000).ToString("00"));
      return dateString;
    }

    /// Todo: Add time formats
    /// <summary>
    /// Formats the time based on the user preferences.
    /// </summary>
    /// <returns>A string containing the localized version of the time.</returns>
    public string FormatTime(DateTime cur)
    {
      string timeString = _timeFormat;
      
      // Hour 0-24
      StringUtils.ReplaceTag(ref timeString, "<h>", cur.Hour.ToString());
      // Hour 00-24
      string hour = string.Format("{0:00}", cur.Hour);
      StringUtils.ReplaceTag(ref timeString, "<H>", hour);

      // Minutes 0-59
      StringUtils.ReplaceTag(ref timeString, "<m>", cur.Minute.ToString());
      // Minutes 00-59
      string minute = string.Format("{0:00}", cur.Minute);
      StringUtils.ReplaceTag(ref timeString, "<M>", minute);

      // Seconds 0-59
      StringUtils.ReplaceTag(ref timeString, "<s>", cur.Second.ToString());
      // Seconds 00-59
      string second = string.Format("{0:00}", cur.Second);
      StringUtils.ReplaceTag(ref timeString, "<S>", second);

      return timeString;
    }


    protected void _update()
    {
      
      double Angle = DateTime.Now.Hour * 30;
      HourAngle = (double)(Angle + 12 * DateTime.Now.Minute / 60);

      MinuteAngle = (double)(DateTime.Now.Minute * 6);

      CurrentTime = FormatTime(DateTime.Now);
      CurrentDate = FormatDate(DateTime.Now);
    }

    protected void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _update();
    }
  }
}

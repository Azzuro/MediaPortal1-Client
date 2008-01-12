//========================================================================
// This file was generated using the MyGeneration tool in combination
// with the Gentle.NET Business Entity template, $Rev: 965 $
//========================================================================
using System;
using System.Collections;
using Gentle.Common;
using Gentle.Framework;

namespace TvDatabase
{
  public enum KeepMethodType
  {
    UntilSpaceNeeded,
    UntilWatched,
    TillDate,
    Always
  }
  public enum ScheduleRecordingType
  {
    Once,
    Daily,
    Weekly,
    EveryTimeOnThisChannel,
    EveryTimeOnEveryChannel,
    Weekends,
    WorkingDays
  }
  /// <summary>
  /// Instances of this class represent the properties and methods of a row in the table <b>Schedule</b>.
  /// </summary>
  [TableName("Schedule")]
  public class Schedule : Persistent
  {
    public enum QualityType
    {
      NotSet,
      Portable,
      Low,
      Medium,
      High
    }
    public static DateTime MinSchedule = new DateTime(2000, 1, 1);
    static public readonly int HighestPriority = Int32.MaxValue;
    static public readonly int LowestPriority = 0;
    bool _isSeries = false;
    #region Members
    private bool isChanged;
    [TableColumn("id_Schedule", NotNull = true), PrimaryKey(AutoGenerated = true)]
    private int idSchedule;
    [TableColumn("idChannel", NotNull = true), ForeignKey("Channel", "idChannel")]
    private int idChannel;
    [TableColumn("scheduleType", NotNull = true)]
    private int scheduleType;
    [TableColumn("programName", NotNull = true)]
    private string programName;
    [TableColumn("startTime", NotNull = true)]
    private DateTime startTime;
    [TableColumn("endTime", NotNull = true)]
    private DateTime endTime;
    [TableColumn("maxAirings", NotNull = true)]
    private int maxAirings;
    [TableColumn("priority", NotNull = true)]
    private int priority;
    [TableColumn("directory", NotNull = true)]
    private string directory;
    [TableColumn("quality", NotNull = true)]
    private int quality;
    [TableColumn("keepMethod", NotNull = true)]
    private int keepMethod;
    [TableColumn("keepDate", NotNull = true)]
    private DateTime keepDate;
    [TableColumn("preRecordInterval", NotNull = true)]
    private int preRecordInterval;
    [TableColumn("postRecordInterval", NotNull = true)]
    private int postRecordInterval;
    [TableColumn("canceled", NotNull = true)]
    private DateTime canceled;
    [TableColumn("recommendedCard", NotNull = true)]
    private int recommendedCard;
    #endregion

    #region Constructors

    public Schedule(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      isChanged = true;
      this.idChannel = idChannel;
      ProgramName = programName;
      Canceled = MinSchedule;
      Directory = "";
      EndTime = endTime;
      KeepDate = MinSchedule;
      KeepMethod = (int)KeepMethodType.UntilSpaceNeeded;
    	MaxAirings = Int32.MaxValue; //BAV: changed due to mantis bug 1162 - old value 5;
      PostRecordInterval = 0;
      PreRecordInterval = 0;
      Priority = 0;
      Quality = 0;
      ScheduleType = (int)ScheduleRecordingType.Once;
      Series = false;
      StartTime = startTime;
      this.recommendedCard = -1;
    }

    /// <summary> 
    /// Create a new object by specifying all fields (except the auto-generated primary key field). 
    /// </summary> 
    public Schedule(int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
    {
      isChanged = true;
      this.idChannel = idChannel;
      this.scheduleType = scheduleType;
      this.programName = programName;
      this.startTime = startTime;
      this.endTime = endTime;
      this.maxAirings = maxAirings;
      this.priority = priority;
      this.directory = directory;
      this.quality = quality;
      this.keepMethod = keepMethod;
      this.keepDate = keepDate;
      this.preRecordInterval = preRecordInterval;
      this.postRecordInterval = postRecordInterval;
      this.canceled = canceled;
      this.recommendedCard = -1;
    }

    /// <summary> 
    /// Create a new schedule from an existing one (except the auto-generated primary key field). 
    /// </summary> 
    public Schedule(Schedule schedule)
    {
      isChanged = true;
      this.idChannel = schedule.idChannel;
      this.scheduleType = schedule.scheduleType;
      this.programName = schedule.programName;
      this.startTime = schedule.startTime;
      this.endTime = schedule.endTime;
      this.maxAirings = schedule.maxAirings;
      this.priority = schedule.priority;
      this.directory = schedule.directory;
      this.quality = schedule.quality;
      this.keepMethod = schedule.keepMethod;
      this.keepDate = schedule.keepDate;
      this.preRecordInterval = schedule.preRecordInterval;
      this.postRecordInterval = schedule.postRecordInterval;
      this.canceled = schedule.canceled;
      this.recommendedCard = -1;
    }

    /// <summary> 
    /// Create an object from an existing row of data. This will be used by Gentle to 
    /// construct objects from retrieved rows. 
    /// </summary> 
    public Schedule(int idSchedule, int idChannel, int scheduleType, string programName, DateTime startTime, DateTime endTime, int maxAirings, int priority, string directory, int quality, int keepMethod, DateTime keepDate, int preRecordInterval, int postRecordInterval, DateTime canceled)
    {
      this.idSchedule = idSchedule;
      this.idChannel = idChannel;
      this.scheduleType = scheduleType;
      this.programName = programName;
      this.startTime = startTime;
      this.endTime = endTime;
      this.maxAirings = maxAirings;
      this.priority = priority;
      this.directory = directory;
      this.quality = quality;
      this.keepMethod = keepMethod;
      this.keepDate = keepDate;
      this.preRecordInterval = preRecordInterval;
      this.postRecordInterval = postRecordInterval;
      this.canceled = canceled;
      this.recommendedCard = -1;
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Indicates whether the entity is changed and requires saving or not.
    /// </summary>
    public bool IsChanged
    {
      get { return isChanged; }
    }

    /// <summary>
    /// Property relating to database column id_Schedule
    /// </summary>
    public int IdSchedule
    {
      get { return idSchedule; }
    }

    /// <summary>
    /// Property to get/set the card id recommended by ConflictsManager plugin
    /// </summary>
    public int RecommendedCard
    {
      get { return recommendedCard; }
      set { isChanged |= recommendedCard != value; recommendedCard = value; }
    }

    /// <summary>
    /// Property relating to database column idChannel
    /// </summary>
    public int IdChannel
    {
      get { return idChannel; }
      set { isChanged |= idChannel != value; idChannel = value; }
    }

    /// <summary>
    /// Property relating to database column scheduleType
    /// </summary>
    public int ScheduleType
    {
      get { return scheduleType; }
      set { isChanged |= scheduleType != value; scheduleType = value; }
    }

    /// <summary>
    /// Property relating to database column programName
    /// </summary>
    public string ProgramName
    {
      get { return programName; }
      set { isChanged |= programName != value; programName = value; }
    }

    /// <summary>
    /// Property relating to database column startTime
    /// </summary>
    public DateTime StartTime
    {
      get { return startTime; }
      set { isChanged |= startTime != value; startTime = value; }
    }

    /// <summary>
    /// Property relating to database column endTime
    /// </summary>
    public DateTime EndTime
    {
      get { return endTime; }
      set { isChanged |= endTime != value; endTime = value; }
    }

    /// <summary>
    /// Property relating to database column maxAirings
    /// </summary>
    public int MaxAirings
    {
      get { return maxAirings; }
      set { isChanged |= maxAirings != value; maxAirings = value; }
    }

    /// <summary>
    /// Property relating to database column priority
    /// </summary>
    public int Priority
    {
      get { return priority; }
      set { isChanged |= priority != value; priority = value; }
    }

    /// <summary>
    /// Property relating to database column directory
    /// </summary>
    public string Directory
    {
      get { return directory; }
      set { isChanged |= directory != value; directory = value; }
    }

    /// <summary>
    /// Property relating to database column quality
    /// </summary>
    public int Quality
    {
      get { return quality; }
      set { isChanged |= quality != value; quality = value; }
    }

    /// <summary>
    /// Property relating to database column keepMethod
    /// </summary>
    public int KeepMethod
    {
      get { return keepMethod; }
      set { isChanged |= keepMethod != value; keepMethod = value; }
    }

    /// <summary>
    /// Property relating to database column keepDate
    /// </summary>
    public DateTime KeepDate
    {
      get { return keepDate; }
      set { isChanged |= keepDate != value; keepDate = value; }
    }

    /// <summary>
    /// Property relating to database column preRecordInterval
    /// </summary>
    public int PreRecordInterval
    {
      get { return preRecordInterval; }
      set { isChanged |= preRecordInterval != value; preRecordInterval = value; }
    }

    /// <summary>
    /// Property relating to database column postRecordInterval
    /// </summary>
    public int PostRecordInterval
    {
      get { return postRecordInterval; }
      set { isChanged |= postRecordInterval != value; postRecordInterval = value; }
    }

    /// <summary>
    /// Property relating to database column canceled
    /// </summary>
    public DateTime Canceled
    {
      get { return canceled; }
      set { isChanged |= canceled != value; canceled = value; }
    }
    #endregion

    #region Storage and Retrieval

    /// <summary>
    /// Static method to retrieve all instances that are stored in the database in one call
    /// </summary>
    public static IList ListAll()
    {
      return Broker.RetrieveList(typeof(Schedule));
    }

    /// <summary>
    /// Retrieves an entity given it's id.
    /// </summary>
    public static Schedule Retrieve(int id)
    {
      // Return null if id is smaller than seed and/or increment for autokey
      if (id < 1)
      {
        return null;
      }
      Key key = new Key(typeof(Schedule), true, "id_Schedule", id);
      return Broker.TryRetrieveInstance(typeof(Schedule), key) as Schedule;
    }

    /// <summary>
    /// Retrieves an entity given it's id, using Gentle.Framework.Key class.
    /// This allows retrieval based on multi-column keys.
    /// </summary>
    public static Schedule Retrieve(Key key)
    {
      return Broker.TryRetrieveInstance(typeof(Schedule), key) as Schedule;
    }

    /// <summary>
    /// Persists the entity if it was never persisted or was changed.
    /// </summary>
    public override void Persist()
    {
      if (IsChanged || !IsPersisted)
      {
        base.Persist();
        isChanged = false;
      }
    }

    /// <summary>
    /// Retreives the first found instance of a 'Once' typed schedule given its Channel,Title,Start and End Times 
    /// </summary>
    /// <param name="IdChannel">Channel id to look for</param>
    /// <param name="title">Title we wanna look for</param>
    /// <param name="startTime">StartTime</param>
    /// <param name="endTime">EndTime</param>
    /// <returns>schedule instance or null</returns>
    public static Schedule RetrieveOnce(int idChannel, string programName, DateTime startTime, DateTime endTime)
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Schedule));

      // 
      sb.AddConstraint(Operator.Equals, "scheduleType", 0);
      sb.AddConstraint(Operator.Equals, "idChannel", idChannel);
      sb.AddConstraint(Operator.Equals, "programName", programName);
      sb.AddConstraint(Operator.Equals, "startTime", startTime);
      sb.AddConstraint(Operator.Equals, "endTime", endTime);
      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      IList getList = ObjectFactory.GetCollection(typeof(Schedule), stmt.Execute());
      if (getList.Count != 0) return (Schedule)getList[0];
      else return null;

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(ChannelMap), this );
    }

    #endregion

    #region Relations

    /// <summary>
    /// Get a list of CanceledSchedule referring to the current entity.
    /// </summary>
    public IList ReferringCanceledSchedule()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(CanceledSchedule));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idSchedule", idSchedule);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection(typeof(CanceledSchedule), stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(CanceledSchedule), this );
    }
    /// <summary>
    /// Get a list of Conflicts referring to the current entity.
    /// </summary>
    public IList ReferringConflicts()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Conflict));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idSchedule", idSchedule);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection(typeof(Conflict), stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(CanceledSchedule), this );
    }

    /// <summary>
    /// Get a list of Conflicts referring to the current entity.
    /// </summary>
    public IList ConflictingSchedules()
    {
      //select * from 'foreigntable'
      SqlBuilder sb = new SqlBuilder(StatementType.Select, typeof(Conflict));

      // where foreigntable.foreignkey = ourprimarykey
      sb.AddConstraint(Operator.Equals, "idConflictingSchedule", idSchedule);

      // passing true indicates that we'd like a list of elements, i.e. that no primary key
      // constraints from the type being retrieved should be added to the statement
      SqlStatement stmt = sb.GetStatement(true);

      // execute the statement/query and create a collection of User instances from the result set
      return ObjectFactory.GetCollection(typeof(Conflict), stmt.Execute());

      // TODO In the end, a GentleList should be returned instead of an arraylist
      //return new GentleList( typeof(CanceledSchedule), this );
    }


    /// <summary>
    ///
    /// </summary>
    public Channel ReferencedChannel()
    {
      return Channel.Retrieve(IdChannel);
    }
    #endregion

    public bool IsSerieIsCanceled(DateTime startTime)
    {
      foreach (CanceledSchedule schedule in ReferringCanceledSchedule())
      {
        if (schedule.CancelDateTime == startTime) return true;
      }
      return false;
    }
    public void UnCancelSerie(DateTime startTime)
    {
      foreach (CanceledSchedule schedule in ReferringCanceledSchedule())
      {
        if (schedule.CancelDateTime == startTime)
        {
          schedule.Remove();
          return;
        }
      }
      return;
    }

    /// <summary>
    /// Checks if the recording should record the specified tvprogram
    /// </summary>
    /// <param name="program">TVProgram to check</param>
    /// <returns>true if the specified tvprogram should be recorded</returns>
    /// <returns>filterCanceledRecordings (true/false)
    /// if true then  we'll return false if recording has been canceled for this program</returns>
    /// if false then we'll return true if recording has been not for this program</returns>
    /// <seealso cref="MediaPortal.TV.Database.TVProgram"/>
    public bool IsRecordingProgram(Program program, bool filterCanceledRecordings)
    {
      ScheduleRecordingType scheduleType = (ScheduleRecordingType)this.ScheduleType;
      switch (scheduleType)
      {
        case ScheduleRecordingType.Once:
          {
            if (program.StartTime == StartTime && program.EndTime == EndTime && program.IdChannel == IdChannel)
            {
              if (filterCanceledRecordings)
              {
                if (this.ReferringCanceledSchedule().Count > 0) return false;
              }
              return true;
            }
          }
          break;
        case ScheduleRecordingType.EveryTimeOnEveryChannel:
          if (program.Title == ProgramName)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
        case ScheduleRecordingType.EveryTimeOnThisChannel:
          if (program.Title == ProgramName && program.IdChannel == IdChannel)
          {
            if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
            return true;
          }
          break;
        case ScheduleRecordingType.Daily:
          if (program.IdChannel == IdChannel)
          {
            int iHourProg = program.StartTime.Hour;
            int iMinProg = program.StartTime.Minute;
            if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
            {
              iHourProg = program.EndTime.Hour;
              iMinProg = program.EndTime.Minute;
              if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
              {
                if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                return true;
              }
            }
          }
          break;
        case ScheduleRecordingType.WorkingDays:
          if (program.StartTime.DayOfWeek >= DayOfWeek.Monday && program.StartTime.DayOfWeek <= DayOfWeek.Friday)
          {
            if (program.IdChannel == IdChannel)
            {
              int iHourProg = program.StartTime.Hour;
              int iMinProg = program.StartTime.Minute;
              if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
              {
                iHourProg = program.EndTime.Hour;
                iMinProg = program.EndTime.Minute;
                if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                  return true;
                }
              }
            }
          }
          break;

        case ScheduleRecordingType.Weekends:
          if (program.StartTime.DayOfWeek == DayOfWeek.Saturday || program.StartTime.DayOfWeek == DayOfWeek.Sunday)
          {
            if (program.IdChannel == IdChannel)
            {
              int iHourProg = program.StartTime.Hour;
              int iMinProg = program.StartTime.Minute;
              if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
              {
                iHourProg = program.EndTime.Hour;
                iMinProg = program.EndTime.Minute;
                if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                  return true;
                }
              }
            }
          }
          break;

        case ScheduleRecordingType.Weekly:
          if (program.IdChannel == IdChannel)
          {
            int iHourProg = program.StartTime.Hour;
            int iMinProg = program.StartTime.Minute;
            if (iHourProg == StartTime.Hour && iMinProg == StartTime.Minute)
            {
              iHourProg = program.EndTime.Hour;
              iMinProg = program.EndTime.Minute;
              if (iHourProg == EndTime.Hour && iMinProg == EndTime.Minute)
              {
                if ((StartTime.DayOfWeek == program.StartTime.DayOfWeek) && (program.StartTime.Date >= StartTime.Date))
                {
                  if (filterCanceledRecordings && IsSerieIsCanceled(program.StartTime)) return false;
                  return true;
                }
              }
            }
          }
          break;
      }
      return false;
    }//IsRecordingProgram(TVProgram program, bool filterCanceledRecordings)


    public bool DoesUseEpisodeManagement
    {
      get
      {
        if (MaxAirings == Int32.MaxValue) return false;
        if (MaxAirings < 1) return false;
        return true;
      }
    }
    /// <summary>
    /// Checks whether this recording is finished and can be deleted
    /// 
    /// </summary>
    /// <returns>true:Recording is finished can be deleted
    ///          false:Recording is not done yet, or needs to be done multiple times
    /// </returns>
    public bool IsDone()
    {
      if (ScheduleType != (int)ScheduleRecordingType.Once) return false;
      if (DateTime.Now > EndTime) return true;
      return false;
    }

    public void Delete()
    {
      IList list = ReferringConflicts();
      foreach (Conflict conflict in list)
        conflict.Remove();

      list = ConflictingSchedules();
      foreach (Conflict conflict in list)
        conflict.Remove();

      list = ReferringCanceledSchedule();
      foreach (CanceledSchedule schedule in list)
        schedule.Remove();

      // does the schedule still exist ?
      // if yes then remove it, if no leave it.
      Schedule schedExists = Retrieve(this.idSchedule);
      if (schedExists != null)
      {
        Remove();
      }
    }

    public bool Series
    {
      get
      {
        return _isSeries;
      }
      set
      {
        _isSeries = value;
      }
    }
    public Schedule Clone()
    {
      Schedule schedule = new Schedule(IdChannel, scheduleType, ProgramName, StartTime, EndTime, MaxAirings, Priority, Directory, Quality, KeepMethod, KeepDate, PreRecordInterval, PostRecordInterval, Canceled);

      schedule._isSeries = _isSeries;
      schedule.idSchedule = idSchedule;
      schedule.isChanged = false;
      return schedule;
    }

    public bool IsOverlapping(Schedule schedule)
    {
      DateTime Start1, Start2, End1, End2;

      Start1 = this.StartTime.AddMinutes(-this.preRecordInterval);
      Start2 = schedule.StartTime.AddMinutes(-schedule.preRecordInterval);
      End1 = this.EndTime.AddMinutes(this.postRecordInterval);
      End2 = schedule.EndTime.AddMinutes(schedule.postRecordInterval);

      // sch_1        s------------------------e
      // sch_2    ---------s-----------------------------
      // sch_2    s--------------------------------e
      // sch_2  ------------------e
      if ((Start2 >= Start1 && Start2 < End1) ||
          (Start2 <= Start1 && End2 >= End1) ||
          (End2 > Start1 && End2 <= End1)) return true;
      return false;
    }

    /// <summary>
    /// checks if 2 schedules have a common Transponder
    /// depending on tuningdetails of their respective channels
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns>True if a common transponder exists</returns>
    public bool isSameTransponder(Schedule schedule)
    {
      IList tuningList1 = this.ReferencedChannel().ReferringTuningDetail();
      IList tuningList2 = schedule.ReferencedChannel().ReferringTuningDetail();
      foreach (TuningDetail tun1 in tuningList1)
      {
        foreach (TuningDetail tun2 in tuningList2)
        {
          if (tun1.Frequency == tun2.Frequency) return true;
        }
      }
      return false;
      
    }


    public override string ToString()
    {
      return String.Format("{0} on {1} {2} - {3}", ProgramName, IdChannel, StartTime, EndTime);
    }
  }
}

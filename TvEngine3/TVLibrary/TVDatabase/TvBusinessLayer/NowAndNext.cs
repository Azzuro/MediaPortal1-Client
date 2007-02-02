using System;
using System.Collections.Generic;
using System.Text;

namespace TvDatabase
{
  public class NowAndNext
  {
    int     _idChannel;
    DateTime _nowStart;
    DateTime _nowEnd;
    DateTime _nextStart;
    DateTime _nextEnd;
    string _titleNow;
    string _titleNext;
    int _idProgramNow;
    int _idProgramNext;

    public NowAndNext(int idChannel, DateTime nowStart, DateTime nowEnd, DateTime nextStart, DateTime nextEnd, string titleNow, string titleNext, int idProgramNow, int idProgramNext)
    {
       _idChannel=idChannel;
      _nowStart=nowStart;
      _nowEnd=nowEnd;
      _nextStart=nextStart;
      _nextEnd=nextEnd;
      _titleNow=titleNow;
      _titleNext=titleNext;
      _idProgramNow=idProgramNow;
      _idProgramNext=idProgramNext;
    }
    public int IdChannel
    {
      get
      {
        return _idChannel ;
      }
      set
      {
        _idChannel = value;
      }
    }
    public DateTime NowStartTime
    {
      get
      {
        return _nowStart;
      }
      set
      {
        _nowStart = value;
      }
    }
    public DateTime NextStartTime
    {
      get
      {
        return _nextStart;
      }
      set
      {
        _nextStart = value;
      }
    }
    public DateTime NowEndTime
    {
      get
      {
        return _nowEnd;
      }
      set
      {
        _nowEnd = value;
      }
    }
    public DateTime NextEndTime
    {
      get
      {
        return _nextEnd;
      }
      set
      {
        _nextEnd = value;
      }
    }
    public string TitleNow
    {
      get
      {
        return _titleNow;
      }
      set
      {
        _titleNow = value;
      }
    }
    public string TitleNext
    {
      get
      {
        return _titleNext;
      }
      set
      {
        _titleNext = value;
      }
    }
    public int IdProgramNow
    {
      get
      {
        return _idProgramNow;
      }
      set
      {
        _idProgramNow = value;
      }
    }
    public int IdProgramNext
    {
      get
      {
        return _idProgramNext;
      }
      set
      {
        _idProgramNext = value;
      }
    }
  }
}

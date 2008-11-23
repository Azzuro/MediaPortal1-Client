﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Message
  {
    [XmlElement("IsNull", typeof(IsNullCondition)), XmlElement("And", typeof(AndCondition)), XmlElement("NotNull", typeof(NotNullCondition)), DefaultValue((string)null), XmlElement("Or", typeof(OrCondition))]
    public MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting.Condition Condition;
    [XmlElement("Image", typeof(Image))]
    public List<Image> Images = new List<Image>();
    [XmlElement("Line", typeof(Line))]
    public List<Line> Lines = new List<Line>();
    [XmlAttribute, DefaultValue(9)]
    public MediaPortal.ProcessPlugins.MiniDisplayPlugin.Status Status = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Status.Any;
    [XmlElement("Window", typeof(int))]
    public List<int> Windows = new List<int>();

    public bool Process(DisplayHandler _keeper)
    {
      if ((this.Condition != null) && !this.Condition.Evaluate())
      {
        return false;
      }
      _keeper.Lines = this.Lines;
      _keeper.Images = this.Images;
      return true;
    }
  }
}


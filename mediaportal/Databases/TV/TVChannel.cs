using System;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Collections;
using SQLite.NET;
using DShowNET;
namespace MediaPortal.TV.Database
{
  /// <summary>
  /// Class which holds all information about a tv channel
  /// </summary>
  public class TVChannel
  {
    string m_strName;
    int    m_iNumber;
    int    m_iID;
    long   m_lFrequency;
    string m_strXMLId;
    bool   m_bExternal=false;
    string m_strExternalTunerChannel="";
    bool   m_bVisibleInGuide=true;
    
    AnalogVideoStandard _TVStandard;
    /// <summary> 
    /// Property to indicate if this is an internal or external (USB-UIRT) channel
    /// </summary>
    public bool External
    {
      get { return m_bExternal;}
      set {m_bExternal=value;}
    }

    public AnalogVideoStandard TVStandard
    {
      get { return _TVStandard;}
      set { _TVStandard =value;}
    }

    /// <summary>
    /// Property that indicates if this channel should be visible in the EPG or not.
    /// </summary>
    public bool VisibleInGuide
    {
      get { return m_bVisibleInGuide; }
      set { m_bVisibleInGuide = value; }
    }

    /// <summary> 
    /// Property to get/set the external tuner channel
    /// </summary>
    public string ExternalTunerChannel
    {
      get { return m_strExternalTunerChannel;}
      set {
          m_strExternalTunerChannel=value;
          if (m_strExternalTunerChannel.Equals("unknown") ) m_strExternalTunerChannel="";
      }
    }

    /// <summary> 
    /// Property to get/set the ID the tv channel has in the XMLTV file
    /// </summary>
    public string XMLId
    {
      get { return m_strXMLId;}
      set {m_strXMLId=value;}
    }

    /// <summary>
    /// Property to get/set the ID the tvchannel has in the tv database
    /// </summary>
    public int ID
    {
      get { return m_iID;}
      set {m_iID=value;}
    }
 
    /// <summary>
    /// Property to get/set the name of the tvchannel
    /// </summary>
    public string Name
    {
      get { return m_strName;}
      set {m_strName=value;}
    }
 
    /// <summary>
    /// Property to get/set the the tvchannel number
    /// </summary>
    public int Number
    {
      get { return m_iNumber;}
      set {m_iNumber=value;}
    }

    /// <summary>
    /// Property to get/set the the frequency of the tvchannel (0=use default)
    /// </summary>
    public long Frequency
    {
      get { return m_lFrequency;}
      set {m_lFrequency=value;}
    }
  }
}
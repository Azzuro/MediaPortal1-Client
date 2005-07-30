/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Drawing;
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.Subtitle
{
	/// <summary>
	/// 
	/// </summary>
  public class SubTitles
  {
    public class Line
    {
        string m_strLine="";
        int    m_iStartTime=0;
        int    m_iEndTime=0;
     
        public Line()
        {
        }
        public Line(string strLine, int iStart, int iEnd)
        {
          m_strLine=strLine;
          m_iStartTime=iStart;
          m_iEndTime=iEnd;
        }
        public int StartTime
        {
          get { return m_iStartTime;}
          set { m_iStartTime=value;}
        }
        public int EndTime
        {
          get { return m_iEndTime;}
          set { m_iEndTime=value;}
        }
        public string Text
        {
          get { return m_strLine;}
          set { m_strLine=value;}
        }
    };

    ArrayList m_subs =new ArrayList();
    string    m_strFont="Arial";
    int       m_iFontSize=14;
    bool      m_bBold=true;
    long      m_iColor=0xffffffff;
    int       m_iShadow=5;
    GUIFont   m_font=null;

		public SubTitles()
		{
			m_subs.Clear();
    }
    public SubTitles(SubTitles subs)
    {
      m_subs=(ArrayList)subs.m_subs.Clone();
    }

    public void Clear()
    {
      m_subs.Clear();
    }
    public int Count
    {
      get { return m_subs.Count;}
    }

    public void Add(Line line)
    {
      m_subs.Add(line);
    }

    public void Render(double dTime)
    {
      int lTime=(int)(1000.0d * dTime);
      foreach (Line line in m_subs)
      {
        if (lTime >= line.StartTime && lTime <= line.EndTime)
        {
          if (m_font!=null)
          {
            try
            {
              float fw=0,fh=0;
              m_font.GetTextExtent(line.Text, ref fw, ref fh);
              int iposx=(GUIGraphicsContext.OverScanWidth-(int)fw)/2;
              int iposy=(GUIGraphicsContext.Subtitles-(int)fh);
              m_font.DrawShadowText( (float)iposx,(float)iposy,m_iColor,line.Text,GUIControl.Alignment.ALIGN_LEFT,m_iShadow,m_iShadow,0xff000000);
            } 
            catch (Exception)
            {
            }
          }
        }
        if (line.StartTime > lTime) return;
      }
    }
    #region Serialisation
    public void LoadSettings()
    {
      using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        string strTmp="";
        m_strFont=xmlreader.GetValueAsString("subtitles","fontface","Arial");
        m_iFontSize=xmlreader.GetValueAsInt("subtitles","fontsize",18);
        m_bBold=xmlreader.GetValueAsBool("subtitles","bold",true);
        strTmp=xmlreader.GetValueAsString("subtitles","color","ffffff");
        m_iColor=Convert.ToInt64(strTmp,16);
        
        m_iShadow=xmlreader.GetValueAsInt("subtitles","shadow",5);

        FontStyle style=FontStyle.Regular;
        if (m_bBold)
          style=FontStyle.Bold;
        m_font=new GUIFont("subFont",m_strFont,m_iFontSize,style);
        m_font.Load();
        m_font.InitializeDeviceObjects();
        m_font.RestoreDeviceObjects();
      }
    }

    #endregion


	}
}

using System;
using System.IO;
using System.Text;

namespace MediaPortal.Subtitle
{
  /// <summary>
  /// 
  /// </summary>
  public class SAMISubReader:ISubtitleReader
  {
    SubTitles m_subs=new SubTitles();
    public SAMISubReader()
    {
      // 
      // TODO: Add constructor logic here
      //
    }
    public override bool SupportsFile(string strFileName)
    {
      string strExt=System.IO.Path.GetExtension(strFileName).ToLower();
      if (strExt==".smi") return true;
      if (strExt==".sami") return true;
      return false;
    }

    public override  bool ReadSubtitles(string strFileName)
    {
      try
      {
        m_subs.Clear();
        using (StreamReader oRead = new StreamReader(strFileName,System.Text.Encoding.GetEncoding(1252)))
        {
          while (oRead.Peek() >= 0) 
          {
            string s = oRead.ReadLine();
            string sText = "";
            string StartTime = "";
            string EndTime = "";
            if(s != null && s.Trim() != "")
            {
              if(s.Trim().ToLower().StartsWith("<sync start="))
              {
                StartTime = s.Trim().Substring(12,s.Trim().IndexOf(">") - 12);
                if(StartTime!="0")
                {
									
                  sText = ReplaceHTML(s.Trim().Substring(s.Trim().IndexOf(">") + 1));
								
                  s = oRead.ReadLine();
									
                  EndTime = s.Trim().Substring(12,s.Trim().IndexOf(">") - 12);



                  SubTitles.Line newLine =new SubTitles.Line();
                  newLine.StartTime=GetSamiTime(StartTime.Replace(":","").Replace(",",""));
                  newLine.Text=sText;
                  newLine.EndTime=GetSamiTime(EndTime.Replace(":","").Replace(",",""));
                  m_subs.Add(newLine);
                }
              }
            }
          }
          if (m_subs.Count>0) return true;
          return false;
        }
      }
      catch(Exception)
      {
      }

      return false;
    }

    //*********************************************************************************************
    private Int32 GetSamiTime(string HHMMSSmmm)
    {
      //Create same Time like SAMI File
      //H = H * 60 * 60
      //M = M * 60
      //S = S
      //Total = (H + M + S * 1000) + Milliseconds

      Int32 H = 0;
      Int32 M = 0;
      Int32 S = 0;
      Int32 Mil = 0;

      if(HHMMSSmmm.Length == 7)
      {
        M = Convert.ToInt32(HHMMSSmmm.Substring(0,2));
        S = Convert.ToInt32(HHMMSSmmm.Substring(2,2));
        Mil = Convert.ToInt32(HHMMSSmmm.Substring(4));
      }
      else if(HHMMSSmmm.Length == 9)
      {
        H = Convert.ToInt32(HHMMSSmmm.Substring(0,2));
        M = Convert.ToInt32(HHMMSSmmm.Substring(2,2));
        S = Convert.ToInt32(HHMMSSmmm.Substring(4,2));
        Mil = Convert.ToInt32(HHMMSSmmm.Substring(6));
      }

      Int32 SamiTime = (((H * 3600) + (M * 60) +  S) * 1000) + Mil;
      return SamiTime;
    }
    private string ReplaceHTML(string vData)
    {
      //Replace all HTML tags

      string tmpText = vData.Trim();
			
      tmpText = tmpText.Replace("<br>","\n\r");
      tmpText = tmpText.Replace("<BR>","\n\r");
      tmpText = tmpText.Replace("<Br>","\n\r");
      tmpText = tmpText.Replace("<bR>","\n\r");
      tmpText = tmpText.Replace("<p>","");
      tmpText = tmpText.Replace("<P>","");
      tmpText = tmpText.Replace("</p>","");
      tmpText = tmpText.Replace("</P>","");

      if(tmpText.ToLower().StartsWith("<p class="))
        tmpText = tmpText.Substring(tmpText.IndexOf(">"));
			
      return tmpText;
    }

    public override  SubTitles Subs
    {
      get { return m_subs;}
    }
  }
}

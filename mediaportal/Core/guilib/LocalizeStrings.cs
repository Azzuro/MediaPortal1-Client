
using System;
using System.Collections;
using System.Xml;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// This class will hold all text used in the application
	/// The text is loaded for the current language from
	/// the file language/[language]/strings.xml
	/// </summary>
	public class GUILocalizeStrings
	{

		static System.Collections.Hashtable m_mapStrings = new Hashtable();

    // singleton. Dont allow any instance of this class
    private GUILocalizeStrings()
    {
    }


		/// <summary>
		/// Clean up.
		/// just delete all text
		/// </summary>
    static public void Dispose()
    {
      m_mapStrings.Clear();
    }

		/// <summary>
		/// Load the text from the strings.xml file
		/// </summary>
		/// <param name="strFileName">filename to string.xml for current language</param>
		/// <param name="map">on return this map will contain all texts loaded</param>
		/// <param name="bDetermineNumberOfChars">
		/// when true this function will determine the total number of characters needed for this language.
		/// This is later on used by the font classes to cache those characters
		/// when false this function wont determine the total number of characters needed for this language.
		/// </param>
		/// <returns></returns>
    static bool LoadMap(string strFileName, ref System.Collections.Hashtable map, bool bDetermineNumberOfChars)
    {
			if (strFileName==null) return false;
			if (strFileName==String.Empty) return false;
			if (map==null) return false;
      map.Clear();
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(strFileName);
        if (doc.DocumentElement==null) return false;
        string strRoot=doc.DocumentElement.Name;
        if (strRoot!="strings") return false;
        if (bDetermineNumberOfChars==true)
        {
          int iChars=255;
          XmlNode nodeChars = doc.DocumentElement.SelectSingleNode("/strings/characters");
          if (nodeChars!=null)
          {
            if (nodeChars.InnerText!=null && nodeChars.InnerText.Length>0)
            {
              try
              {
                iChars=Convert.ToInt32(nodeChars.InnerText);
                if (iChars < 255) iChars=255;
              }
              catch(Exception)
              {
                iChars=255;
              }
              GUIGraphicsContext.CharsInCharacterSet=iChars;
            }
          }
        }
        XmlNodeList list=doc.DocumentElement.SelectNodes("/strings/string");
        foreach (XmlNode node in list)
        {
          string strLine=node.SelectSingleNode("value").InnerText;
          int    iCode       =(int)System.Int32.Parse(node.SelectSingleNode("id").InnerText);
          map[iCode]=strLine;
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Write("exception loading language {0} err:{1} stack:{2}", strFileName, ex.Message,ex.StackTrace);
        return false;
      }
    }

		/// <summary>
		/// Public method to load the text from a strings/xml file into memory
		/// </summary>
		/// <param name="strFileName">Contains the filename+path for the string.xml file</param>
		/// <returns>
		/// true when text is loaded
		/// false when it was unable to load the text
		/// </returns>
		static public bool	Load(string strFileName)
		{
			if (strFileName==null) return false;
			if (strFileName==String.Empty) return false;
			System.Collections.Hashtable mapEnglish = new Hashtable();
			// load the text for the current language
			LoadMap(strFileName,ref m_mapStrings,true);
			//load the text for the english language
			LoadMap(@"language\English\strings.xml",ref mapEnglish,false);

			// check if current language contains an entry for each textline found
			// in the english version
			foreach (DictionaryEntry d in mapEnglish)
			{
				if (!m_mapStrings.ContainsKey((int)d.Key))
				{
					//if current language does not contain a translation for this text
					//then use the english variant
					m_mapStrings[d.Key] = (string)d.Value;
					Log.Write("language file:{0} is missing entry for id:{1} text:{2}", strFileName,(int)d.Key,(string)d.Value);
				}
			}
			mapEnglish=null;
			return true;
		} 

		/// <summary>
		/// Get the translation for a given id
		/// </summary>
		/// <param name="dwCode">id of text</param>
		/// <returns>
		/// string containing the translated text
		/// </returns>
		static public string  Get(int dwCode)
		{
			if (m_mapStrings.ContainsKey(dwCode))
			{
				return (string)m_mapStrings[dwCode];
			}
			return "";
		}
		
		static public void LocalizeLabel(ref string strLabel)
		{
			if (strLabel==null) strLabel=String.Empty;
			if (strLabel == "-")	strLabel = "";
			if (strLabel == "")		return;
			// This can't be a valid string code if the first character isn't a number.
			// This check will save us from catching unnecessary exceptions.
			if (!char.IsNumber(strLabel, 0))
				return;
			try
			{
				int dwLabelID = System.Int32.Parse(strLabel);
				strLabel = GUILocalizeStrings.Get(dwLabelID);
			}
			catch (FormatException)
			{
			}
		}
	}
}

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
  /// <summary>
  /// 
  /// </summary>
  public class MusicInfoScraper
  {
    ArrayList m_albums = new ArrayList();
    public MusicInfoScraper()
    {
    }
    public int Count
    {
      get { return m_albums.Count;}
    }
    public MusicAlbumInfo this[int index]
    {
      get { return (MusicAlbumInfo)m_albums[index];}
    }
    public bool FindAlbuminfo(string strAlbum)
    {
      m_albums.Clear();

//     strAlbum="1999";//escapolygy";

      // make request
      // type is 
      // http://www.allmusic.com/cg/amg.dll?P=amg&SQL=escapolygy&OPT1=2

      string strPostData=String.Format("P=amg&SQL={0}&OPT1=2", strAlbum );
		
      string strHTML=PostHTTP("http://www.allmusic.com/cg/amg.dll", strPostData);
      if (strHTML.Length==0) return false;

      // check if this is an album
      MusicAlbumInfo newAlbum = new MusicAlbumInfo();
      newAlbum.AlbumURL="http://www.allmusic.com/cg/amg.dll?"+strPostData;
      if ( newAlbum.Parse(strHTML) )
      {
        m_albums.Add(newAlbum);
        return true;
      }

      string strHTMLLow=strHTML;
      strHTMLLow=strHTMLLow.ToLower();
      int iStartOfTable=strHTMLLow.IndexOf("id=\"expansiontable1\"");
      if (iStartOfTable< 0) return false;
      iStartOfTable=strHTMLLow.LastIndexOf("<table",iStartOfTable);
      if (iStartOfTable < 0) return false;
      
      HTMLUtil  util=new HTMLUtil();
      HTMLTable table=new HTMLTable();
      string strTable=strHTML.Substring(iStartOfTable);
      table.Parse(strTable);

      for (int i=1; i < table.Rows; ++i)
      {
        HTMLTable.HTMLRow row=table.GetRow(i);
        string strAlbumName="";
        string strAlbumURL="";
        for (int iCol=0; iCol < row.Columns; ++iCol)
        {
          string strColum=row.GetColumValue(iCol);
          if (iCol==1 && (strColum.Length!=0)) strAlbumName="("+strColum+")";
          if (iCol==2)
          {
            string strArtist=strColum;
            util.RemoveTags(ref strArtist);
            if (!strColum.Equals("&nbsp;"))
              strAlbumName=String.Format("- {0} {1}",strArtist,strAlbumName);
          }
          if (iCol==4)
          {
            string strAlbumTemp=strColum;
            util.RemoveTags(ref strAlbumTemp);
            strAlbumName=String.Format("{0} {1}",strAlbumTemp,strAlbumName);
          }
          if (iCol==4 && strColum.IndexOf("<a href=\"") >= 0)
          {
            int pos1=strColum.IndexOf("<a href=\"") ;
            pos1+=+"<a href=\"".Length;
            int iPos2=strColum.IndexOf("\">",pos1);
            if (iPos2 >= 0)
            {
                // full album url:
                // http://www.allmusic.com/cg/amg.dll?p=amg&token=&sql=10:66jieal64xs7
                string strurl=strColum.Substring(pos1, iPos2-pos1);                
                string strAlbumNameStripped;
                strAlbumURL=String.Format("http://www.allmusic.com{0}", strurl);
                MusicAlbumInfo newAlbumInfo = new MusicAlbumInfo();
                util.ConvertHTMLToAnsi(strAlbumName, out strAlbumNameStripped);
                newAlbumInfo.Title2=strAlbumNameStripped;
                newAlbumInfo.AlbumURL=strAlbumURL;
                m_albums.Add(newAlbumInfo);
            
            }
          }
        }
      }
	
      return true;
    }
    
    
    string PostHTTP(string strURL, string strData)
    {
      try
      {
        string strBody;
        WebRequest req = WebRequest.Create(strURL);
        req.Method="POST";
        req.ContentType = "application/x-www-form-urlencoded";

        byte [] bytes = null;
        // Get the data that is being posted (or sent) to the server
        bytes = System.Text.Encoding.ASCII.GetBytes (strData);
        req.ContentLength = bytes.Length;
        // 1. Get an output stream from the request object
        Stream outputStream = req.GetRequestStream ();

        // 2. Post the data out to the stream
        outputStream.Write (bytes, 0, bytes.Length);

        // 3. Close the output stream and send the data out to the web server
        outputStream.Close ();


        WebResponse result = req.GetResponse();
        Stream ReceiveStream = result.GetResponseStream();
        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
        StreamReader sr = new StreamReader( ReceiveStream, encode );
        strBody=sr.ReadToEnd();
        return strBody;
      }
      catch(Exception)
      {
      }
      return "";
    }
  }
}

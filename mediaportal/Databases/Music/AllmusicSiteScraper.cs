using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.Music.Database;

using MediaPortal.Util;

namespace MediaPortal.Music.Database
{
	/// <summary>
	/// Summary description for ArtistInfoScraper.
	/// </summary>
	public class AllmusicSiteScraper
	{
		public enum SearchBy : int {Artists = 1, Albums};

		internal const string MAINURL = "http://www.allmusic.com";
		internal const string URLPROGRAM = "/cg/amg.dll";
    internal const string JAVASCRIPTZ = "p=amg&token=&sql=";
		protected ArrayList m_codes = new ArrayList();	// if multiple..
		protected ArrayList m_values = new ArrayList();	// if multiple..
		protected bool m_multiple = false;
		protected string m_htmlCode = null;
		protected string m_queryString = "";

		public AllmusicSiteScraper()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public bool IsMultiple()
		{
			return m_multiple;
		}

		public string[] GetItemsFound()
		{
			return (string[]) m_values.ToArray(typeof(string));
		}

        public string GetHtmlContent()
        {
            return m_htmlCode;
        }

		public bool FindInfoByIndex(int index)
		{
      if(index < 0 || index > m_codes.Count -1)
        return false;

			string strGetData=m_queryString + m_codes[index];
    
			string strHTML=GetHTTP(MAINURL+URLPROGRAM+"?"+strGetData);
			if (strHTML.Length==0) return false;

			m_htmlCode = strHTML;	// save the html content...
			return true;
		}

		public bool FindInfo(SearchBy searchBy, string searchStr)
		{
      HTMLUtil  util=new HTMLUtil();
			string strPostData=String.Format("P=amg&opt1={0}&sql={1}&Image1.x=18&Image1.y=14", (int)searchBy, searchStr);
    
			string strHTML=PostHTTP(MAINURL+URLPROGRAM, strPostData);
			if (strHTML.Length==0) return false;

			m_htmlCode = strHTML;	// save the html content...

			Regex multiples = new Regex(
				@"\sSearch\sResults\sfor:",
				RegexOptions.IgnoreCase
				| RegexOptions.Multiline
				| RegexOptions.IgnorePatternWhitespace
				| RegexOptions.Compiled
				);

			if(multiples.IsMatch(strHTML))
			{
        string pattern = "bogus";
        if(searchBy.ToString().Equals("Artists"))
          pattern =  @"<a\shref.*?sql=(?<code>(11:|41).*?)"">(?<name>.*?)</a>.*?<TD" + 
                     @"\sclass.*?>(?<name2>.*?)</TD>.*?""cell"">(?<name3>.*?)</td>";
        else if(searchBy.ToString().Equals("Albums")) // below patter needs to be checked
          pattern = @"""cell"">(?<name2>.*?)</TD>.*?style.*?word;"">(?<name3>.*?)<" + 
                    @"/TD>.*?onclick=""z\('(?<code>.*?)'\)"">(?<name>.*?)</a>";



				Match m;
				Regex itemsFoundFromSite = new Regex(
					pattern,
					RegexOptions.IgnoreCase
					| RegexOptions.Multiline
					| RegexOptions.IgnorePatternWhitespace
					| RegexOptions.Compiled
					);


				for (m = itemsFoundFromSite.Match(strHTML); m.Success; m = m.NextMatch()) 
				{
					string code = m.Groups["code"].ToString();
					string name = m.Groups["name"].ToString();
					string detail = m.Groups["name2"].ToString();
          string detail2 = m.Groups["name3"].ToString();

          util.RemoveTags(ref name);
          util.ConvertHTMLToAnsi(name, out name);

          util.RemoveTags(ref detail);
          util.ConvertHTMLToAnsi(detail, out detail);

          util.RemoveTags(ref detail);
          util.ConvertHTMLToAnsi(detail, out detail);

          util.RemoveTags(ref detail2);
          util.ConvertHTMLToAnsi(detail2, out detail2);

          detail += " - " + detail2;
					System.Console.Out.WriteLine("code = {0}, name = {1}, detail = {2}", code, name, detail);
					if(detail.Length > 0)
					{
						m_codes.Add(code); 
						m_values.Add(name + " - " + detail); 
					}
					else
					{
						m_codes.Add(code); 
						m_values.Add(name); 
					}
				}
				m_queryString = JAVASCRIPTZ;
				System.Console.Out.WriteLine("url = {0}", m_queryString);
				m_multiple = true;
			}
			else // found the right one
			{
			}
			return true;

		}

		internal static string PostHTTP(string strURL, string strData)
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
				//Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

        // 1252 is encoding for Windows format
        Encoding encode = System.Text.Encoding.GetEncoding(1252);
				StreamReader sr = new StreamReader( ReceiveStream, encode );
				strBody=sr.ReadToEnd();
				return strBody;
			}
			catch(Exception)
			{
			}
			return "";
		}

    internal static string GetHTTP(string strURL)
    {
      string retval = null;

      // Initialize the WebRequest.
      WebRequest myRequest = WebRequest.Create(strURL);

      // Return the response. 
      WebResponse myResponse = myRequest.GetResponse();

      Stream ReceiveStream = myResponse.GetResponseStream();

      // 1252 is encoding for Windows format
      Encoding encode = System.Text.Encoding.GetEncoding(1252);
      StreamReader sr = new StreamReader( ReceiveStream, encode );
      retval=sr.ReadToEnd();

      // Close the response to free resources.
      myResponse.Close();

      return retval;
    }

		[STAThread]
		static void Main(string[] args)
		{
      MusicArtistInfo artist = new MusicArtistInfo();
      MusicAlbumInfo album = new MusicAlbumInfo();
			AllmusicSiteScraper prog = new AllmusicSiteScraper();
			
      prog.FindInfo(SearchBy.Artists, "Gloria Estefan");
      //prog.FindInfo(SearchBy.Albums, "Unwrapped");
      //prog.FindInfoByIndex(1);
			artist.Parse(prog.GetHtmlContent());
      //album.Parse(prog.GetHtmlContent());
		
    }

	}

}

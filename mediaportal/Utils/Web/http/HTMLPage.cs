#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;
using System.Web;
using mshtml;
using SHDocVw;
using MediaPortal.Services;

namespace MediaPortal.Utils.Web
{
  public class HTMLPage
  {
    string _strPageHead = string.Empty;
    string _strPageSource = string.Empty;
    string _defaultEncode = "iso-8859-1";
    string _pageEncodingMessage = string.Empty;
    string _encoding = string.Empty;
    string _error;
    IHtmlCache _cache;

    public HTMLPage()
    {
      _cache = GlobalServiceProvider.Get<IHtmlCache>();
    }

    public HTMLPage(HTTPRequest page)
    {
      _encoding = page.Encoding;
      LoadPage(page);
    }

    public HTMLPage(HTTPRequest page, string encoding)
    {
      _encoding = encoding;
      LoadPage(page);
    }

    public string Encoding
    {
      get { return _encoding; }
      set { _encoding = value; }
    }

    public string PageEncodingMessage
    {
      get { return _pageEncodingMessage; }
    }

    public string Error
    {
      get { return _error; }
    }

    public bool LoadPage(HTTPRequest page)
    {
      if (_cache != null && _cache.Initialised)
      {
        if (_cache.LoadPage(page.Uri))
        {
          _strPageSource = _cache.GetPage();
          return true;
        }
      }

      bool success;

      if (page.External)
      {
        success = GetExternal(page);
      }
      else
      {
        success = GetInternal(page);
      }

      if (success)
      {
        if (_cache != null && _cache.Initialised)
          _cache.SavePage(page.Uri, _strPageSource);

        return true;
      }
      return false;
    }

    public string GetPage()
    {
      return _strPageSource;
    }

    //public string GetBody()
    //{
    //  //return _strPageSource.Substring(_startIndex, _endIndex - _startIndex);
    //  //try
    //  //{
    //  //    XmlDocument xmlDoc = new XmlDocument();
    //  //    xmlDoc.LoadXml(_strPageSource);
    //  //    XmlNode bodyNode = xmlDoc.DocumentElement.SelectSingleNode("//body");
    //  //    return bodyNode.InnerText;
    //  //}
    //  //catch (System.Xml.XmlException ex)
    //  //{
    //  //    _Error = "XML Error finding Body"; 
    //  //}
    //  int startIndex = _strPageSource.ToLower().IndexOf("<body", 0);
    //  if (startIndex == -1)
    //  {
    //    // report Error
    //    _error = "No body start found";
    //    return null;
    //  }

    //  int endIndex = _strPageSource.ToLower().IndexOf("</body", startIndex);

    //  if (endIndex == -1)
    //  {
    //    //report Error
    //    _error = "No body end found";
    //    endIndex = _strPageSource.Length;
    //  }

    //  return _strPageSource.Substring(startIndex, endIndex - startIndex);

    //}

    private bool GetExternal(HTTPRequest page)
    {
      // Use External Browser (IE) to get HTML page
      // IE downloads all linked graphics ads, etc
      // IE will run Javascript source if required to renderthe page
      SHDocVw.InternetExplorer IE = new SHDocVw.InternetExplorer();
      IWebBrowser2 webBrowser = (IWebBrowser2)IE;

      object empty = System.Reflection.Missing.Value;
      webBrowser.Navigate(page.Url, ref empty, ref empty, ref empty, ref empty);
      while (webBrowser.Busy == true) System.Threading.Thread.Sleep(500);
      HTMLDocumentClass doc = (HTMLDocumentClass)webBrowser.Document;

      _strPageSource = doc.body.innerHTML;

      return true;
    }

    private bool GetInternal(HTTPRequest page)
    {
      // Use internal code to get HTML page
      HTTPTransaction Page = new HTTPTransaction();
      Encoding encode;
      string strEncode = _defaultEncode;

      if (Page.HTTPGet(page))
      {
        byte[] pageData = Page.GetData();
        int i;

        if (_encoding != "")
        {
          strEncode = _encoding;
          _pageEncodingMessage = "Forced: " + _encoding;
        }
        else
        {
          encode = System.Text.Encoding.GetEncoding(_defaultEncode);
          _strPageSource = encode.GetString(pageData);
          int headEnd;
          if ((headEnd = _strPageSource.ToLower().IndexOf("</head")) != -1)
          {
            if ((i = _strPageSource.ToLower().IndexOf("charset", 0, headEnd)) != -1)
            {
              strEncode = "";
              i += 8;
              for (; i < _strPageSource.Length && _strPageSource[i] != '\"'; i++)
                strEncode += _strPageSource[i];
              _encoding = strEncode;
            }

            if (strEncode == "")
            {
              strEncode = _defaultEncode;
              _pageEncodingMessage = "Default: " + _defaultEncode;
            }
            else
            {
              _pageEncodingMessage = strEncode;
            }
          }
        }

        // Encoding: depends on selected page
        if (_strPageSource == "" || strEncode.ToLower() != _defaultEncode)
        {
          try
          {
            encode = System.Text.Encoding.GetEncoding(strEncode);
            _strPageSource = encode.GetString(pageData);
          }
          catch (System.ArgumentException)
          {
          }
        }
        return true;
      }
      _error = Page.GetError();
      return false;
    }
  }
}

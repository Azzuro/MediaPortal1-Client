#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *  Copyright (C) 2005-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
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

using System.Xml;

namespace UPnP.Infrastructure.Utils
{
  public static class SoapHelper
  {
    public static void WriteSoapEnvelopeStart(XmlWriter writer, bool addXSINamespace)
    {
      writer.WriteStartDocument();
      writer.WriteStartElement("s", "Envelope", UPnPConsts.NS_SOAP_ENVELOPE);
      if (addXSINamespace)
        writer.WriteAttributeString("xmlns", "xsi", null, UPnPConsts.NS_XSI);
      writer.WriteAttributeString("s", "encodingStyle", null, UPnPConsts.NS_SOAP_ENCODING);
      writer.WriteStartElement("Body", UPnPConsts.NS_SOAP_ENVELOPE);
    }

    public static void WriteSoapEnvelopeEndAndClose(XmlWriter writer)
    {
      writer.WriteEndElement(); // s:Body
      writer.WriteEndElement(); // s:Envelope
      writer.Close();
    }

    public static void WriteNull(XmlWriter writer)
    {
      writer.WriteStartAttribute("null", UPnPConsts.NS_XSI);
      writer.WriteValue(true);
    }

    public static bool ReadNull(XmlReader reader)
    {
      return reader.MoveToAttribute("null", UPnPConsts.NS_XSI) && reader.ReadContentAsBoolean();
    }
  }
}
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

using System;
using System.Xml;
using System.Xml.XPath;
using MediaPortal.Utilities.Exceptions;
using UPnP.Infrastructure.Common;

namespace UPnP.Infrastructure.Utils
{
  public class ParserHelper
  {
    /// <summary>
    /// Given the <paramref name="userAgentStr"/> from an HTTP USER-AGENT header, this method extracts the UPnP version
    /// from the string.
    /// </summary>
    /// <param name="userAgentStr">USER-AGENT header entry of the form "OS/version UPnP/1.1 product/version".</param>
    /// <param name="minorVersion">Returns the minor version number in the specified <paramref name="userAgentStr"/>.</param>
    /// <returns><c>true</c>, if the user agent string could successfully be parsed and denotes a UPnP major version of 1.</returns>
    /// <exception cref="MediaPortal.Utilities.Exceptions.InvalidDataException">If the specified header value is malformed.</exception>
    public static bool ParseUserAgentUPnP1MinorVersion(string userAgentStr, out int minorVersion)
    {
      string[] tokens = userAgentStr.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length != 3)
        throw new InvalidDataException("Invalid USER-AGENT header entry");
      string upnpToken = tokens[1];
      UPnPVersion ver;
      if (!UPnPVersion.TryParse(upnpToken, out ver))
        throw new UnsupportedRequestException(string.Format("Unsupported USER-AGENT header entry '{0}'", userAgentStr));
      minorVersion = 0;
      if (ver.VerMax != 1)
        return false;
      minorVersion = ver.VerMin;
      return true;
    }

    public static bool TryParseTypeVersion_URN(string typeVersionURN, out string type, out int version)
    {
      type = null;
      version = 0;
      int index = typeVersionURN.LastIndexOf(':');
      if (!typeVersionURN.StartsWith("urn:") || index == -1)
        return false;
      type = typeVersionURN.Substring("urn:".Length, index - "urn:".Length); // Type without "urn:" prefix and without version suffix
      string versionStr = typeVersionURN.Substring(index + 1); // Version suffix
      return int.TryParse(versionStr, out version); // We don't permit version numbers which aren't integers.
    }

    public static bool TryParseDataTypeReference(string typeStr, XPathNavigator dataTypeElementNav,
        out string schemaURI, out string dataTypeName)
    {
      schemaURI = null;
      dataTypeName = null;
      int index = typeStr.LastIndexOf(':');
      if (index == -1)
        return false;
      string prefix = typeStr.Substring(0, index);
      dataTypeName = typeStr.Substring(index + 1);
      schemaURI = prefix.StartsWith("urn:") ? prefix : dataTypeElementNav.GetNamespace(prefix);
      return true;
    }

    /// <summary>
    /// Extracts the UUID from a string containing a UDN (of the form: "uuid:[uuid]").
    /// </summary>
    /// <param name="udn">UDN to break up.</param>
    /// <returns>UUID part of the given <paramref name="udn"/></returns>
    /// <exception cref="ArgumentException">If the given <paramref name="udn"/> doesn't start with "uuid:".</exception>
    public static string ExtractUUIDFromUDN(string udn)
    {
      if (!udn.StartsWith("uuid:"))
        throw new ArgumentException(string.Format("Invalid UDN '{0}'", udn));
      return udn.Substring("uuid:".Length);
    }

    /// <summary>
    /// Returns the text string result of the specified <paramref name="xPathExpr"/> referencing an
    /// XML text node.
    /// </summary>
    /// <param name="elementNav">XPath navigator pointing to an XML element to apply the XPath expression to.</param>
    /// <param name="xPathExpr">XPath expression which references an XML text node (i.e. must end with "text()").</param>
    /// <param name="nsmgr">Namespace resolver for the used namespace prefixes in the <paramref name="xPathExpr"/>.
    /// If set to <c>null</c>, no namespace resolver will be used.</param>
    /// <returns>Contents of the referenced XML text node.</returns>
    /// <exception cref="ArgumentException">If the given <paramref name="xPathExpr"/> doesn't reference an
    /// XML text node.</exception>
    public static string SelectText(XPathNavigator elementNav, string xPathExpr, IXmlNamespaceResolver nsmgr)
    {
      XPathNodeIterator it = elementNav.Select(xPathExpr, nsmgr);
      if (it.MoveNext())
        return it.Current.Value;
      throw new ArgumentException(string.Format("Error evaluating XPath expression '{0}'", xPathExpr));
    }

    /// <summary>
    /// Given an XPath navigator <paramref name="soapEnvelopeNav"/> pointing to an XML element with a SOAP envelope,
    /// this method unwraps the envelope and returns the <paramref name="body"/> element.
    /// </summary>
    /// <param name="soapEnvelopeNav">XPath navigator pointing to a SOAP envelope XML element.</param>
    /// <param name="body">Unwrapped SOAP body.</param>
    /// <returns><c>true</c> if the unwrapping was successful, else <c>false</c>.</returns>
    public static bool UnwrapSoapEnvelopeElement(XPathNavigator soapEnvelopeNav, out XPathNavigator body)
    {
      body = null;
      if (soapEnvelopeNav == null || soapEnvelopeNav.LocalName != "Envelope" || soapEnvelopeNav.NamespaceURI != Consts.NS_SOAP_ENVELOPE)
        return false;
      XPathNodeIterator it = soapEnvelopeNav.SelectChildren("Body", Consts.NS_SOAP_ENVELOPE);
      if (it.MoveNext())
        body = it.Current;
      return true;
    }
  }
}

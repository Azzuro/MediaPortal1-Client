using System.IO;
using InvalidDataException=MediaPortal.Utilities.Exceptions.InvalidDataException;

namespace UPnP.Infrastructure.Dv.HTTP
{
  /// <summary>
  /// Encapsulates an HTTP request.
  /// </summary>
  /// <remarks>
  /// This class contains methods to create an HTTP request which can produce a byte array to be sent to the network
  /// and the other way, to parse an <see cref="SimpleHTTPRequest"/> instance from a given byte stream.
  /// </remarks>
  public class SimpleHTTPRequest : SimpleHTTPMessage
  {
    protected string _method;
    protected string _param;

    internal SimpleHTTPRequest() { }

    public SimpleHTTPRequest(string method, string param)
    {
      _method = method;
      _param = param;
    }

    public string Method
    {
      get { return _method; }
      set { _method = value; }
    }

    public string Param
    {
      get { return _param; }
      set { _param = value; }
    }

    protected override string EncodeStartingLine()
    {
      return string.Format("{0} {1} {2}", _method, _param, _httpVersion);
    }

    /// <summary>
    /// Parses the HTTP request out of the given <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">HTTP data stream to parse.</param>
    /// <param name="result">Returns the parsed HTTP request instance.</param>
    /// <exception cref="MediaPortal.Utilities.Exceptions.InvalidDataException">If the given <paramref name="data"/> is malformed.</exception>
    public static void Parse(Stream stream, out SimpleHTTPRequest result)
    {
      result = new SimpleHTTPRequest();
      string firstLine;
      result.ParseHeaderAndBody(stream, out firstLine);
      string[] elements = firstLine.Split(' ');
      if (elements.Length != 3)
        throw new InvalidDataException("Invalid HTTP request header starting line '{0}'", firstLine);
      string httpVersion = elements[2];
      if (httpVersion != "HTTP/1.0" && httpVersion != "HTTP/1.1")
        throw new InvalidDataException("Invalid HTTP request header starting line '{0}'", firstLine);
      result._method = elements[0];
      result._param = elements[1];
      result._httpVersion = httpVersion;
    }
  }
}

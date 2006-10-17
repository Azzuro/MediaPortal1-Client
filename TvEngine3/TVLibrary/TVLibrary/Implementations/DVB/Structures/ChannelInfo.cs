using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  /// <summary>
  /// class holding all information about a channel including pids
  /// </summary>
  public class ChannelInfo
  {
    /// <summary>
    /// program number (service id)
    /// </summary>
    public int program_number;
    /// <summary>
    /// reserved
    /// </summary>
    public int reserved;
    /// <summary>
    /// pid of the PMT
    /// </summary>
    public int network_pmt_PID;
    /// <summary>
    /// transport stream id
    /// </summary>
    public int transportStreamID;
    /// <summary>
    /// name of the provider
    /// </summary>
    public string service_provider_name;
    /// <summary>
    /// name of the service
    /// </summary>
    public string service_name;
    /// <summary>
    /// service type
    /// </summary>
    public int serviceType;
    /// <summary>
    /// eit schedule flag
    /// </summary>
    public bool eitSchedule;
    /// <summary>
    /// eit prefollow flag
    /// </summary>
    public bool eitPreFollow;
    /// <summary>
    /// indicates if channel is scrambled
    /// </summary>
    public bool scrambled;
    /// <summary>
    /// carrier frequency
    /// </summary>
    public int freq;// 12188
    /// <summary>
    /// symbol rate
    /// </summary>
    public int symb;// 27500
    /// <summary>
    /// fec
    /// </summary>
    public int fec;// 6
    /// <summary>
    /// diseqc type
    /// </summary>
    public int diseqc;// 1
    /// <summary>
    /// LNB low oscilator frequency
    /// </summary>
    public int lnb01;// 10600
    /// <summary>
    /// LNB frequency
    /// </summary>
    public int lnbkhz;// 1 = 22
    /// <summary>
    /// Polarisation
    /// </summary>
    public int pol; // 0 - h
    /// <summary>
    /// pid of the PCR
    /// </summary>
    public int pcr_pid;
    /// <summary>
    /// ArrayList of PidInfo containing all pids
    /// </summary>
    public ArrayList pids;
    /// <summary>
    /// Service Id
    /// </summary>
    public int serviceID;
    /// <summary>
    /// Network Id
    /// </summary>
    public int networkID;
    /// <summary>
    /// pidcache?
    /// </summary>
    public string pidCache;
    /// <summary>
    /// Atsc minor channel number
    /// </summary>
    public int minorChannel;
    /// <summary>
    /// atsc major channel number
    /// </summary>
    public int majorChannel;
    /// <summary>
    /// Modulation
    /// </summary>
    public int modulation;
    /// <summary>
    /// CaPmt
    /// </summary>
    public CaPMT caPMT;
    /// <summary>
    /// Logical channel number
    /// </summary>
    public int LCN;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:ChannelInfo"/> class.
    /// </summary>
    public ChannelInfo()
    {
      pids = new ArrayList();
    }
    /// <summary>
    /// Adds a pid to the pidtable
    /// </summary>
    /// <param name="info">The info.</param>
    public void AddPid(PidInfo info)
    {
      pids.Add(info);
    }
    /// <summary>
    /// Decodes the pmt specified in data.
    /// </summary>
    /// <param name="data">The data.</param>
    public void Decode(IntPtr data)
    {
      byte[] da = new byte[600];
      Marshal.Copy(data, da, 0, 580);
      program_number = -1;
      network_pmt_PID = -1;
      transportStreamID = -1;
      service_provider_name = String.Empty;
      service_name = String.Empty;
      serviceType = -1;
      eitSchedule = false;
      eitPreFollow = false;
      scrambled = false;
      freq = -1;
      symb = -1;
      fec = -1;
      diseqc = -1;
      lnb01 = -1;
      lnbkhz = -1;
      pol = -1;
      pcr_pid = -1;
      pids = new ArrayList();
      serviceID = -1;
      networkID = -1;
      pidCache = String.Empty;
      minorChannel = -1;
      majorChannel = -1;
      modulation = -1;
      majorChannel = -1;
      minorChannel = -1;


      transportStreamID = Marshal.ReadInt32(data, 0);
      program_number = Marshal.ReadInt32(data, 4);
      network_pmt_PID = Marshal.ReadInt32(data, 8);
      pcr_pid = Marshal.ReadInt32(data, 12);
      serviceID = program_number;
      pids = new ArrayList();
      PidInfo pmt = new PidInfo();
      // video
      pmt.pid = Marshal.ReadInt16(data, 16);
      pmt.isVideo = true;
      pmt.stream_type = 1;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // audio 1
      pmt.pid = Marshal.ReadInt16(data, 18);
      pmt.isAudio = true;
      pmt.stream_type = 3;
      pmt.language = "" + (char)Marshal.ReadByte(data, 20) + (char)Marshal.ReadByte(data, 21) + (char)Marshal.ReadByte(data, 22);
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // audio 2
      pmt.pid = Marshal.ReadInt16(data, 24);
      pmt.isAudio = true;
      pmt.stream_type = 3;
      pmt.language = "" + (char)Marshal.ReadByte(data, 26) + (char)Marshal.ReadByte(data, 27) + (char)Marshal.ReadByte(data, 28);
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // audio 3
      pmt.pid = Marshal.ReadInt16(data, 30);
      pmt.isAudio = true;
      pmt.stream_type = 3;
      pmt.language = "" + (char)Marshal.ReadByte(data, 32) + (char)Marshal.ReadByte(data, 33) + (char)Marshal.ReadByte(data, 34);
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // ac3
      pmt.pid = Marshal.ReadInt16(data, 36);
      pmt.isAC3Audio = true;
      pmt.stream_type = 0x81;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // teletext
      pmt.pid = Marshal.ReadInt16(data, 38);
      pmt.isTeletext = true;
      pmt.stream_type = 0;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      // sub
      pmt.pid = Marshal.ReadInt16(data, 40);
      pmt.isDVBSubtitle = true;
      pmt.stream_type = 0;
      pmt.language = "";
      RemoveInvalidChars(ref pmt.language);
      pids.Add(pmt);
      pmt = new PidInfo();

      byte[] d = new byte[255];
      //Marshal.Copy((IntPtr)(((int)data)+42),d,0,255);
      service_name = Marshal.PtrToStringAnsi((IntPtr)(((int)data) + 42));
      //Marshal.Copy((IntPtr)(((int)data)+297),d,0,255);
      service_provider_name = Marshal.PtrToStringAnsi((IntPtr)(((int)data) + 297));
      eitPreFollow = (Marshal.ReadInt16(data, 552)) == 1 ? true : false;
      eitSchedule = (Marshal.ReadInt16(data, 554)) == 1 ? true : false;
      scrambled = (Marshal.ReadInt16(data, 556)) == 1 ? true : false;
      serviceType = Marshal.ReadInt16(data, 558);
      networkID = Marshal.ReadInt32(data, 560);

      majorChannel = Marshal.ReadInt16(data, 568);
      minorChannel = Marshal.ReadInt16(data, 570);
      modulation = Marshal.ReadInt16(data, 572);
      freq = Marshal.ReadInt32(data, 576);
      LCN = Marshal.ReadInt32(data, 580);
      RemoveInvalidChars(ref service_name);
      RemoveInvalidChars(ref service_provider_name);

    }
    void RemoveInvalidChars(ref string strTxt)
    {
      if (strTxt == null)
      {
        strTxt = String.Empty;
        return;
      }
      if (strTxt.Length == 0)
      {
        strTxt = String.Empty;
        return;
      }
      string strReturn = String.Empty;
      for (int i = 0; i < (int)strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'')
        {
          strReturn += "'";
        }
        if ((byte)k == 0)// remove 0-bytes from the string
          k = (char)32;

        strReturn += k;
      }
      strReturn = strReturn.Trim();
      strTxt = strReturn;
    }
    /// <summary>
    /// Decodes the PMT supplied in buf and fills the pid table with all pids found
    /// </summary>
    /// <param name="buf">The buf.</param>
    public void DecodePmt(byte[] buf)
    {
      if (buf.Length < 13)
      {
        //Log.Log.WriteFile("decodePMTTable() len < 13 len={0}", buf.Length);
        return;
      }
      int table_id = buf[0];
      int section_syntax_indicator = (buf[1] >> 7) & 1;
      int section_length = ((buf[1] & 0xF) << 8) + buf[2];
      int program_number = (buf[3] << 8) + buf[4];
      int version_number = ((buf[5] >> 1) & 0x1F);
      int current_next_indicator = buf[5] & 1;
      int section_number = buf[6];
      int last_section_number = buf[7];
      int pcr_pid = ((buf[8] & 0x1F) << 8) + buf[9];
      int program_info_length = ((buf[10] & 0xF) << 8) + buf[11];


      caPMT = new CaPMT();
      caPMT.ProgramNumber = program_number;
      caPMT.CurrentNextIndicator = current_next_indicator;
      caPMT.VersionNumber = version_number;
      caPMT.CAPmt_Listmanagement = ListManagementType.Only;

      //if (pat.program_number != program_number)
      //{

      //Log.Write("decodePMTTable() pat program#!=program numer {0}!={1}", pat.program_number, program_number);
      //return 0;
      //}
      //pat.pid_list = new ArrayList();

      //string pidText = "";

      int pointer = 12;
      int x;
      int len1 = section_length - pointer;
      int len2 = program_info_length;
      Log.Log.Write("Decode pmt");
      while (len2 > 0)
      {
        if (pointer + 2 > buf.Length) break;
        int indicator = buf[pointer];
        x = 0;
        x = buf[pointer + 1] + 2;
        byte[] data = new byte[x];

        if (pointer + x > buf.Length) break;
        System.Array.Copy(buf, pointer, data, 0, x);
       
        if (indicator == 0x9) //MPEG CA Descriptor
        {
          //Log.Log.Write("  descriptor1:{0:X} len:{1} {2:X} {3:X}", indicator,data.Length,buf[pointer],buf[pointer+1]);
          caPMT.Descriptors.Add(data);
          caPMT.ProgramInfoLength += data.Length;
          //string tmpString = DVB_CADescriptor(data);
          //if (pidText.IndexOf(tmpString, 0) == -1)
          // pidText += tmpString + ";";
        }
        len2 -= x;
        pointer += x;
        len1 -= x;
      }
      if (caPMT.ProgramInfoLength > 0)
      {
        caPMT.CommandId = CommandIdType.Descrambling;
        caPMT.ProgramInfoLength += 1;
      }
      //byte[] b = new byte[6];
      PidInfo pmt;
      while (len1 > 4)
      {
        if (pointer + 5 > section_length) break;
        pmt = new PidInfo();
        //System.Array.Copy(buf, pointer, b, 0, 5);
        try
        {
          pmt.stream_type = buf[pointer];
          pmt.reserved_1 = (buf[pointer + 1] >> 5) & 7;
          pmt.pid = ((buf[pointer + 1] & 0x1F) << 8) + buf[pointer + 2];
          pmt.reserved_2 = (buf[pointer + 3] >> 4) & 0xF;
          pmt.ES_info_length = ((buf[pointer + 3] & 0xF) << 8) + buf[pointer + 4];
        }
        catch
        {
        }
        
        switch (pmt.stream_type)
        {
          case 0x1b://H.264
            pmt.isVideo = true;
            break;
          case 0x10://MPEG4
            pmt.isVideo = true;
            break;
          case 0x1://MPEG-2 VIDEO
            pmt.isVideo = true;
            break;
          case 0x2://MPEG-2 VIDEO
            pmt.isVideo = true;
            break;
          case 0x3://MPEG-2 AUDIO
            pmt.isAudio = true;
            break;
          case 0x4://MPEG-2 AUDIO
            pmt.isAudio = true;
            break;
        }
        pointer += 5;
        len1 -= 5;
        len2 = pmt.ES_info_length;

        CaPmtEs pmtEs = new CaPmtEs();
        pmtEs.StreamType = pmt.stream_type;
        pmtEs.ElementaryStreamPID = pmt.pid;
        pmtEs.CommandId = CommandIdType.Descrambling;

        if (len1 > 0)
        {
          while (len2 > 0)
          {
            x = 0;
            if (pointer + 1 < buf.Length)
            {
              int indicator = buf[pointer];
              x = buf[pointer + 1] + 2;
              //Log.Log.Write("  descriptor2:{0:X}", indicator);
              if (x + pointer < buf.Length) // parse descriptor data
              {
                byte[] data = new byte[x];
                System.Array.Copy(buf, pointer, data, 0, x);
                switch (indicator)
                {
                  case 0x02: // video
                  case 0x03: // audio
                    //Log.Write("dvbsections: indicator {1} {0} found",(indicator==0x02?"for video":"for audio"),indicator);
                    break;
                  case 0x09:
                    //Log.Log.Write("  descriptor2:{0:X} len:{1:X} {2:X} {3:X}",
                    //    indicator,data.Length, buf[pointer], buf[pointer + 1]);
                    
                    pmtEs.Descriptors.Add(data);
                    pmtEs.ElementaryStreamInfoLength+= data.Length;
                    //caData.Add(data);
                    //string tmpString = DVB_CADescriptor(data);
                    //if (pidText.IndexOf(tmpString, 0) == -1)
                    //  pidText += tmpString + ";";
                    break;
                  case 0x0A:
                    pmt.language = DVB_GetMPEGISO639Lang(data);
                    break;
                  case 0x6A:
                    pmt.isAudio = false;
                    pmt.isVideo = false;
                    pmt.isTeletext = false;
                    pmt.isDVBSubtitle = true;
                    pmt.isAC3Audio = true;
                    pmt.stream_type = 0x81;
                    break;
                  case 0x56:
                    pmt.isAC3Audio = false;
                    pmt.isAudio = false;
                    pmt.isVideo = false;
                    pmt.isTeletext = true;
                    pmt.teletextLANG = DVB_GetTeletextDescriptor(data);
                    break;
                  //case 0xc2:
                  case 0x59:
                    if (pmt.stream_type == 0x05 || pmt.stream_type == 0x06)
                    {
                      pmt.isAC3Audio = false;
                      pmt.isAudio = false;
                      pmt.isVideo = false;
                      pmt.isTeletext = false;
                      pmt.isDVBSubtitle = true;
                      pmt.stream_type = 0x6;
                      pmt.language = DVB_SubtitleDescriptior(data);
                    }
                    break;
                  default:
                    pmt.language = "";
                    break;
                }
              }
            }
            else
            {
              break;
            }
            len2 -= x;
            len1 -= x;
            pointer += x;
          }
        }
        if (pmt.isVideo || pmt.isAC3Audio || pmt.isAudio)
        {
          if (pmtEs.ElementaryStreamInfoLength > 0)
          {
            pmtEs.CommandId = CommandIdType.Descrambling;
            pmtEs.ElementaryStreamInfoLength += 1;
          }
          caPMT.CaPmtEsList.Add(pmtEs);
        }
        pids.Add(pmt);
      }
      //pat.pidCache = pidText;
      //caPMT.Dump();
    }
    private string DVB_GetMPEGISO639Lang(byte[] b)
    {

      int descriptor_tag;
      int descriptor_length;
      string ISO_639_language_code = "";
      int audio_type;
      int len;

      descriptor_tag = b[0];
      descriptor_length = b[1];
      if (descriptor_length < b.Length)
        if (descriptor_tag == 0xa)
        {
          len = descriptor_length;
          byte[] bytes = new byte[len + 1];

          int pointer = 2;

          while (len > 0)
          {
            System.Array.Copy(b, pointer, bytes, 0, len);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            if (bytes.Length >= 4)
              audio_type = bytes[3];
            pointer += 4;
            len -= 4;
          }
        }

      return ISO_639_language_code;
    }

    string DVB_SubtitleDescriptior(byte[] buf)
    {
      int descriptor_tag;
      int descriptor_length;
      string ISO_639_language_code = "";
      int subtitling_type;
      int composition_page_id;
      int ancillary_page_id;
      int len;

      descriptor_tag = buf[0];
      descriptor_length = buf[1];
      if (descriptor_length < buf.Length)
        if (descriptor_tag == 0x59)
        {
          len = descriptor_length;
          byte[] bytes = new byte[len + 1];

          int pointer = 2;

          while (len > 0)
          {
            System.Array.Copy(buf, pointer, bytes, 0, len);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            if (bytes.Length >= 4)
              subtitling_type = bytes[3];
            if (bytes.Length >= 6)
              composition_page_id = (bytes[4] << 8) + bytes[5];
            if (bytes.Length >= 8)
              ancillary_page_id = (bytes[6] << 8) + bytes[7];

            pointer += 8;
            len -= 8;
          }
        }

      return ISO_639_language_code;
    }
    private string DVB_GetTeletextDescriptor(byte[] b)
    {
      int descriptor_tag;
      int descriptor_length;
      string ISO_639_language_code = "";
      int teletext_type;
      int teletext_magazine_number;
      int teletext_page_number;
      int len;
      if (b.Length < 2) return String.Empty;
      descriptor_tag = b[0];
      descriptor_length = b[1];

      len = descriptor_length;
      byte[] bytes = new byte[len + 1];
      if (len < b.Length + 2)
        if (descriptor_tag == 0x56)
        {
          int pointer = 2;

          while (len > 0 && (pointer + 3 <= b.Length))
          {
            System.Array.Copy(b, pointer, bytes, 0, 3);
            ISO_639_language_code += System.Text.Encoding.ASCII.GetString(bytes, 0, 3);
            teletext_type = (bytes[3] >> 3) & 0x1F;
            teletext_magazine_number = bytes[3] & 7;
            teletext_page_number = bytes[4];
            pointer += 5;
            len -= 5;
          }
        }
      if (ISO_639_language_code.Length >= 3)
        return ISO_639_language_code.Substring(0, 3);
      return "";
    }

  }
}

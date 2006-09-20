/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Roger.ID3
{
	/// <summary>
	/// A class for reading the tags from an ID3 header.
	/// </summary>
	public class TagReader
	{
		Stream stream;
		byte majorVersion;
		byte minorVersion;
		int lastTagPos;

		string frameId;
		object frameValue;

		// Default to v2.3.0 format.
		byte DefaultMajorVersion = 3;
		byte DefaultMinorVersion = 0;

		/// <summary>
		/// Assumes that the stream is positioned at the 'I' of 'ID3'.
		/// </summary>
		public TagReader(Stream stream)
		{
			this.stream = stream;

			string magic = ReadMagic();
			if (magic == "ID3")
			{
				majorVersion = (byte)stream.ReadByte();
				minorVersion = (byte)stream.ReadByte();

				// Get the flags
				int tagFlags = stream.ReadByte();

				if (tagFlags != 0)
					throw new TagsException("We only know how to deal with tagFlags == 0");

				// Figure out the length.
				int tagLength = ReadInt28();

				// That tagLength doesn't include the 10 bytes of _this_ header, but does include padding.
				int tagEndPos = tagLength + 10;

				// The last possible place for a tag to start is 10 bytes before that.
				this.lastTagPos = tagEndPos - 10;
			}
			else
			{
				// It's not an ID3 tag.
				majorVersion = DefaultMajorVersion;
				minorVersion = DefaultMinorVersion;

				this.stream = null;
			}

			this.frameId = null;
			this.frameValue = null;
		}

		public bool Read()
		{
			// Reset these.
			frameId = null;
			frameValue = null;

			if (stream == null)
				return false;	// Wasn't an ID3 frame

			if (stream.Position > lastTagPos)
				return false;	// Ran off the end.

			frameId = ReadFrameId();
			int frameLength = ReadFrameLength();

			if (frameLength == 0)
				return false;	// Bail early.

			short frameFlags = ReadFrameFlags();
			frameValue = ReadFrameValue(frameLength);

			return true;
		}

		public string GetKey()
		{
			if (this.frameId == null)
				throw new TagsException("You gotta call Read first.");

			return this.frameId;
		}

		public object GetValue()
		{
			if (this.frameId == null)
				throw new TagsException("You gotta call Read first.");

			return this.frameValue;
		}

		public byte MajorVersion
		{
			get
			{
				return majorVersion;
			}
		}

		public byte MinorVersion
		{
			get
			{
				return minorVersion;
			}
		}

		string ReadMagic()
		{
			byte[] magicBuffer = new byte[3];
			stream.Read(magicBuffer, 0, 3);

			string magic = Encoding.ASCII.GetString(magicBuffer, 0, 3);
			return magic;
		}

		string ReadFrameId()
		{
			byte[] frameBytes = new byte[4];
			stream.Read(frameBytes, 0, 4);

			// Frame Ids are restricted to the characters A-Z and 0-9, encoded in ASCII.
			string frameId = Encoding.ASCII.GetString(frameBytes, 0, 4);
			return frameId;
		}

		int ReadFrameLength()
		{
			int frameLength;
			if (majorVersion == 4)
				frameLength = ReadInt28();
			else if (majorVersion == 3)
				frameLength = ReadInt32();
			else
				throw new TagsException("Don't know how to deal with this version.");

			return frameLength;
		}

		short ReadFrameFlags()
		{
			int flagsH = stream.ReadByte();
			int flagsL = stream.ReadByte();

			short frameFlags = (short)((flagsH << 8) | flagsL);
			return frameFlags;
		}

		object ReadFrameValue(int frameLength)
		{
			// Read the rest of the frame:
			byte[] frameBuffer = new byte[frameLength];
			stream.Read(frameBuffer, 0, frameLength);

			if (frameId[0] == 'T')
			{
				// It's a text frame. It starts with the text encoding value.
				Encoding encoding = GetFrameEncoding(frameBuffer[0]);
				string textFrameValue = encoding.GetString(frameBuffer, 1, frameLength-1);

				return textFrameValue;
			}
			else	// Otherwise it's opaque binary:
				return frameBuffer;
		}

		int ReadInt28()
		{
			byte[] buffer = new byte[4];
			stream.Read(buffer, 0, 4);

			if ((buffer[0] & 0x80) != 0 || (buffer[1] & 0x80) != 0 || (buffer[2] & 0x80) != 0 || (buffer[3] & 0x80) != 0)
				throw new TagsException("Found invalid syncsafe integer");

			int result = (buffer[0] << 21) | (buffer[1] << 14) | (buffer[2] << 7) | buffer[3];
			return result;
		}

		int ReadInt32()
		{
			byte[] buffer = new byte[4];
			stream.Read(buffer, 0, 4);

			int result = (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
			return result;
		}

		Encoding GetFrameEncoding(byte frameEncoding)
		{
			switch (frameEncoding)
			{
				default:
				case 0: // ISO 8859-1
					return Encoding.GetEncoding(1252);
				case 1: // UTF16 with BOM
					return Encoding.GetEncoding("utf-16");
				case 2:	// UTF16, BE
					return Encoding.GetEncoding("UTF-16BE");
				case 3:	// UTF8
					return Encoding.UTF8;
			}
		}	
	}
}

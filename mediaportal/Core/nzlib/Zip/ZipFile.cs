// ZipFile.cs
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.

using System;
using System.Collections;
using System.IO;
using System.Text;

using NZlib.Streams;
using NZlib.Compression;

namespace NZlib.Zip {
	
	/// <summary>
	/// This class represents a Zip archive.  You can ask for the contained
	/// entries, or get an input stream for a file entry.  The entry is
	/// automatically decompressed.
	/// 
	/// This class is thread safe:  You can open input streams for arbitrary
	/// entries in different threads.
	/// 
	/// author of the original java version : Jochen Hoenicke
	/// </summary>
	/// <example>
	/// using System;
	/// using System.Text;
	/// using System.Collections;
	/// using System.IO;
	/// 
	/// using NZlib.Zip;
	/// 
	/// class MainClass
	/// {
	/// 	static public void Main(string[] args)
	/// 	{
	/// 		ZipFile zFile = new ZipFile(args[0]);
	/// 		Console.WriteLine("Listing of : " + zFile.Name);
	/// 		Console.WriteLine("");
	/// 		Console.WriteLine("Raw Size    Size      Date     Time     Name");
	/// 		Console.WriteLine("--------  --------  --------  ------  ---------");
	/// 		foreach (ZipEntry e in zFile) {
	/// 			DateTime d = e.DateTime;
	/// 			Console.WriteLine("{0, -10}{1, -10}{2}  {3}   {4}", e.Size, e.CompressedSize,
	/// 			                                                    d.ToString("dd-MM-yy"), d.ToString("t"),
	/// 			                                                    e.Name);
	/// 		}
	/// 	}
	/// }
	/// </example>
	public class ZipFile : IEnumerable
	{
		string     name;
		Stream     baseStream;
		ZipEntry[] entries;
		
		/// <summary>
		/// Opens a Zip file with the given name for reading.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// IOException if a i/o error occured.
		/// </exception>
		/// <exception name="NZlib.ZipException>
		/// if the file doesn't contain a valid zip archive.
		/// </exception>
		public ZipFile(string name) : this(File.OpenRead(name))
		{
		}
		
		/// <summary>
		/// Opens a Zip file reading the given FileStream
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// IOException if a i/o error occured.
		/// </exception>
		/// <exception name="NZlib.ZipException>
		/// if the file doesn't contain a valid zip archive.
		/// </exception>
		public ZipFile(FileStream file)
		{
			this.baseStream  = file;
			this.name = file.Name;
			ReadEntries();
		}
		
		/// <summary>
		/// Read an unsigned short in little endian byte order.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="System.IO.EndOfStreamException">
		/// if the file ends prematurely
		/// </exception>
		int ReadLeShort()
		{
			return baseStream.ReadByte() | baseStream.ReadByte() << 8;
		}
		
		/// <summary>
		/// Read an int in little endian byte order.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="System.IO.EndOfStreamException">
		/// if the file ends prematurely
		/// </exception>
		int ReadLeInt()
		{
			return ReadLeShort() | ReadLeShort() << 16;
		}
		
		/// <summary>
		/// Read the central directory of a zip file and fill the entries
		/// array.  This is called exactly once by the constructors.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="NZlib.ZipException>
		/// if the central directory is malformed
		/// </exception>
		void ReadEntries()
		{
			/* Search for the End Of Central Directory.  When a zip comment is
			* present the directory may start earlier.
			* FIXME: This searches the whole file in a very slow manner if the
			* file isn't a zip file.
			*/
			long pos = baseStream.Length - ZipConstants.ENDHDR;
			do {
				if (pos < 0) {
					throw new ZipException("central directory not found, probably not a zip file");
				}
				baseStream.Seek(pos--, SeekOrigin.Begin);
			} while (ReadLeInt() != ZipConstants.ENDSIG);
			
			long oldPos = baseStream.Position;
			baseStream.Position += ZipConstants.ENDTOT - ZipConstants.ENDNRD;
			
			if (baseStream.Position - oldPos != ZipConstants.ENDTOT - ZipConstants.ENDNRD) {
				throw new EndOfStreamException();
			}
			int count = ReadLeShort();
			
			oldPos = baseStream.Position;
			baseStream.Position += ZipConstants.ENDOFF - ZipConstants.ENDSIZ;
			
			if (baseStream.Position - oldPos != ZipConstants.ENDOFF - ZipConstants.ENDSIZ) {
				throw new EndOfStreamException();
			}
			
			int centralOffset = ReadLeInt();
			
			entries = new ZipEntry[count];
			baseStream.Seek(centralOffset, SeekOrigin.Begin);
			for (int i = 0; i < count; i++) {
				if (ReadLeInt() != ZipConstants.CENSIG) {
					throw new ZipException("Wrong Central Directory signature");
				}
				
				oldPos = baseStream.Position;
				baseStream.Position += ZipConstants.CENHOW - ZipConstants.CENVEM;
				
				if (baseStream.Position - oldPos != ZipConstants.CENHOW - ZipConstants.CENVEM) {
					throw new EndOfStreamException();
				}
				int method = ReadLeShort();
				int dostime = ReadLeInt();
				int crc = ReadLeInt();
				int csize = ReadLeInt();
				int size = ReadLeInt();
				int nameLen = ReadLeShort();
				int extraLen = ReadLeShort();
				int commentLen = ReadLeShort();
				
				oldPos = baseStream.Position;
				baseStream.Position += ZipConstants.CENOFF - ZipConstants.CENDSK;
				if (baseStream.Position - oldPos != ZipConstants.CENOFF - ZipConstants.CENDSK) {
					throw new EndOfStreamException();
				}
				int offset = ReadLeInt();
				
				byte[] buffer = new byte[Math.Max(nameLen, commentLen)];
				
				baseStream.Read(buffer, 0, nameLen);
				string name = new UTF8Encoding().GetString(buffer, 0, nameLen);
				
				ZipEntry entry = new ZipEntry(name);
				entry.CompressionMethod = (CompressionMethod)method;
				entry.Crc = crc & 0xffffffffL;
				entry.Size = size & 0xffffffffL;
				entry.CompressedSize = csize & 0xffffffffL;
				entry.DosTime = dostime;
				if (extraLen > 0) {
					byte[] extra = new byte[extraLen];
					baseStream.Read(extra, 0, extraLen);
					entry.ExtraData = extra;
				}
				if (commentLen > 0) {
					baseStream.Read(buffer, 0, commentLen);
					entry.Comment = new UTF8Encoding().GetString(buffer, 0, commentLen);
				}
				entry.zipFileIndex = i;
				entry.offset = offset;
				entries[i] = entry;
			}
		}
		
		/// <summary>
		/// Closes the ZipFile.  This also closes all input streams given by
		/// this class.  After this is called, no further method should be
		/// called.
		/// </summary>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		public void Close()
		{
			entries = null;
			lock(baseStream) {
				baseStream.Close();
			}
		}
		
		/// <summary>
		/// Returns an IEnumerator of all Zip entries in this Zip file.
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			if (entries == null) {
				throw new InvalidOperationException("ZipFile has closed");
			}
			
			return new ZipEntryEnumeration(entries);
		}
		
		int GetEntryIndex(string name)
		{
			for (int i = 0; i < entries.Length; i++) {
				if (name.Equals(entries[i].Name)) {
					return i;
				}
			}
			return -1;
		}
		
		/// <summary>
		/// Searches for a zip entry in this archive with the given name.
		/// </summary>
		/// <param name="name">
		/// the name. May contain directory components separated by slashes ('/').
		/// </param>
		/// <returns>
		/// the zip entry, or null if no entry with that name exists.
		/// </returns>
		public ZipEntry GetEntry(string name)
		{
			if (entries == null) {
				throw new InvalidOperationException("ZipFile has closed");
			}
			int index = GetEntryIndex(name);
			return index >= 0 ? (ZipEntry) entries[index].Clone() : null;
		}
		
		/// <summary>
		/// Checks, if the local header of the entry at index i matches the
		/// central directory, and returns the offset to the data.
		/// </summary>
		/// <returns>
		/// the start offset of the (compressed) data.
		/// </returns>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="NZlib.ZipException>
		/// if the local header doesn't match the central directory header
		/// </exception>
		long CheckLocalHeader(ZipEntry entry)
		{
			lock(baseStream) {
				baseStream.Seek(entry.offset, SeekOrigin.Begin);
				if (ReadLeInt() != ZipConstants.LOCSIG) {
					throw new ZipException("Wrong Local header signature");
				}
				
				/* skip version and flags */
				long oldPos = baseStream.Position;
				baseStream.Position += ZipConstants.LOCHOW - ZipConstants.LOCVER;
				if (baseStream.Position - oldPos != ZipConstants.LOCHOW - ZipConstants.LOCVER) {
					throw new EndOfStreamException();
				}
				
				if (entry.CompressionMethod != (CompressionMethod)ReadLeShort()) {
					throw new ZipException("Compression method mismatch");
				}
				
				/* Skip time, crc, size and csize */
				oldPos = baseStream.Position;
				baseStream.Position += ZipConstants.LOCNAM - ZipConstants.LOCTIM;
				
				if (baseStream.Position - oldPos != ZipConstants.LOCNAM - ZipConstants.LOCTIM) {
					throw new EndOfStreamException();
				}
				
				if (entry.Name.Length != ReadLeShort()) {
					throw new ZipException("file name length mismatch");
				}
				
				int extraLen = entry.Name.Length + ReadLeShort();
				return entry.offset + ZipConstants.LOCHDR + extraLen;
			}
		}
		
		/// <summary>
		/// Creates an input stream reading the given zip entry as
		/// uncompressed data.  Normally zip entry should be an entry
		/// returned by GetEntry().
		/// </summary>
		/// <returns>
		/// the input stream.
		/// </returns>
		/// <exception name="System.IO.IOException">
		/// if a i/o error occured.
		/// </exception>
		/// <exception name="NZlib.ZipException>
		/// if the Zip archive is malformed.
		/// </exception>
		public Stream GetInputStream(ZipEntry entry)
		{
			if (entries == null) {
				throw new InvalidOperationException("ZipFile has closed");
			}
			
			int index = entry.zipFileIndex;
			if (index < 0 || index >= entries.Length || entries[index].Name != entry.Name) {
				index = GetEntryIndex(entry.Name);
				if (index < 0) {
					throw new IndexOutOfRangeException();
				}
			}
			
			long start = CheckLocalHeader(entries[index]);
			CompressionMethod method = entries[index].CompressionMethod;
			Stream istr = new PartialInputStream(baseStream, start, entries[index].CompressedSize);
			switch (method) {
				case CompressionMethod.Stored:
					return istr;
				case CompressionMethod.Deflated:
					return new InflaterInputStream(istr, new Inflater(true));
				default:
					throw new ZipException("Unknown compression method " + method);
			}
		}
		
		/// <summary>
		/// Returns the name of this zip file.
		/// </summary>
		public string Name {
			get {
				return name;
			}
		}
		
		/// <summary>
		/// Returns the number of entries in this zip file.
		/// </summary>
		public int Size {
			get {
				try {
					return entries.Length;
				} catch (Exception) {
					throw new InvalidOperationException("ZipFile has closed");
				}
			}
		}
		
		class ZipEntryEnumeration : IEnumerator
		{
			ZipEntry[] array;
			int ptr = 0;
			
			public ZipEntryEnumeration(ZipEntry[] arr)
			{
				array = arr;
			}
			
			public object Current {
				get {
					return array[ptr];
				}
			}
			
			public void Reset()
			{
				ptr = 0;
			}
			
			public bool MoveNext() 
			{
				return (++ptr < array.Length);
			}
		}
		
		class PartialInputStream : InflaterInputStream
		{
			Stream baseStream;
			long filepos, end;
			
			public PartialInputStream(Stream baseStream, long start, long len) : base(baseStream)
			{
				this.baseStream = baseStream;
				filepos = start;
				end = start + len;
			}
			
			public override int Available {
				get {
					long amount = end - filepos;
					if (amount > Int32.MaxValue) {
						return Int32.MaxValue;
					}
					
					return (int) amount;
				}
			}
			
			public override int ReadByte()
			{
				if (filepos == end) {
					return -1;
				}
				lock(baseStream) {
					baseStream.Seek(filepos++, SeekOrigin.Begin);
					return baseStream.ReadByte();
				}
			}
			
			public override int Read(byte[] b, int off, int len)
			{
				if (len > end - filepos) {
					len = (int) (end - filepos);
					if (len == 0) {
						return -1;
					}
				}
				lock(baseStream) {
					baseStream.Seek(filepos, SeekOrigin.Begin);
					int count = baseStream.Read(b, off, len);
					if (count > 0) {
						filepos += len;
					}
					return count;
				}
			}
			
			public long SkipBytes(long amount)
			{
				if (amount < 0) {
					throw new ArgumentOutOfRangeException();
				}
				if (amount > end - filepos) {
					amount = end - filepos;
				}
				filepos += amount;
				return amount;
			}
		}
	}
}

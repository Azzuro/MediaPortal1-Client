/***************************************************************************
    copyright            : (C) 2005 by Brian Nickel
                         : (C) 2006 Novell, Inc.
    email                : brian.nickel@gmail.com
                         : Aaron Bockover <abockover@novell.com>
 ***************************************************************************/

/***************************************************************************
 *   This library is free software; you can redistribute it and/or modify  *
 *   it  under the terms of the GNU Lesser General Public License version  *
 *   2.1 as published by the Free Software Foundation.                     *
 *                                                                         *
 *   This library is distributed in the hope that it will be useful, but   *
 *   WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU     *
 *   Lesser General Public License for more details.                       *
 *                                                                         *
 *   You should have received a copy of the GNU Lesser General Public      *
 *   License along with this library; if not, write to the Free Software   *
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  *
 *   USA                                                                   *
 ***************************************************************************/

using System.Collections.Generic;
using System;

namespace TagLib
{
   public enum TagTypes
   {
      NoTags  = 0x0000,
      Xiph    = 0x0001,
      Id3v1   = 0x0002,
      Id3v2   = 0x0004,
      Ape     = 0x0008,
      Apple   = 0x0010,
      Asf     = 0x0020,
      AllTags = 0xFFFF
   }
   
   public abstract class File
   {
      /*private static Dictionary<string, System.Type> file_types = new Dictionary<string, System.Type> ();*/
      
      public delegate IFileAbstraction FileAbstractionCreator (string path);
      public delegate File             FileTypeResolver       (string path, ReadStyle style);
         
      public enum AccessMode
      {
         Read,
         Write,
         Closed
      }
      
      protected System.IO.Stream file_stream;
      private IFileAbstraction file_abstraction;
      private string mime_type;
      private static uint buffer_size = 1024;
      
      private static List<FileTypeResolver> file_type_resolvers = new List<FileTypeResolver> ();
      private static FileAbstractionCreator file_abstraction_creator = new FileAbstractionCreator (LocalFileAbstraction.CreateFile);
      
      //////////////////////////////////////////////////////////////////////////
      // public members
      //////////////////////////////////////////////////////////////////////////
      
      public File (string file)
      {
         file_stream = null;
         file_abstraction = file_abstraction_creator (file);
      }
      
      // Added by Albert/MediaPortal
      public File (IFileAbstraction fileAbstraction)
      {
         file_stream = null;
         file_abstraction = fileAbstraction;
      }
      
      public string Name {get {return file_abstraction.Name;}}
      public string MimeType { 
         get { return mime_type; }
         internal set { mime_type = value; }
      }
      
      public abstract Tag Tag {get;}
      
      public abstract Properties Properties {get;}
      
      public abstract void Save ();
      
      public ByteVector ReadBlock (int length)
      {
         if (length == 0)
            return new ByteVector ();
         
         Mode = AccessMode.Read;
         
         if (length > buffer_size && (long) length > (Length - Tell))
            length = (int) (Length - Tell);
         
         byte [] buffer = new byte [length];
         int count = file_stream.Read (buffer, 0, length);
         return new ByteVector (buffer, count);
      }

      public void WriteBlock (ByteVector data)
      {
         Mode = AccessMode.Write;
         file_stream.Write (data.Data, 0, data.Count);
      }

      public long Find (ByteVector pattern, long from_offset, ByteVector before)
      {
         Mode = AccessMode.Read;
         
         if (pattern.Count > buffer_size)
            return -1;
         
         // The position in the file that the current buffer starts at.

         long buffer_offset = from_offset;
         ByteVector buffer;

         // These variables are used to keep track of a partial match that happens at
         // the end of a buffer.

         int previous_partial_match = -1;
         int before_previous_partial_match = -1;
         
         // Save the location of the current read pointer.  We will restore the
         // position using seek() before all returns.

         long original_position = file_stream.Position;

         // Start the search at the offset.

         file_stream.Position = from_offset;
         
         // This loop is the crux of the find method.  There are three cases that we
         // want to account for:
         //
         // (1) The previously searched buffer contained a partial match of the search
         // pattern and we want to see if the next one starts with the remainder of
         // that pattern.
         //
         // (2) The search pattern is wholly contained within the current buffer.
         //
         // (3) The current buffer ends with a partial match of the pattern.  We will
         // note this for use in the next itteration, where we will check for the rest
         // of the pattern.
         //
         // All three of these are done in two steps.  First we check for the pattern
         // and do things appropriately if a match (or partial match) is found.  We
         // then check for "before".  The order is important because it gives priority
         // to "real" matches.
         
         for (buffer = ReadBlock((int)buffer_size); buffer.Count > 0; buffer = ReadBlock((int)buffer_size))
         {
            
            // (1) previous partial match
            
            if (previous_partial_match >= 0 && (int) buffer_size > previous_partial_match)
            {
               int pattern_offset = (int) buffer_size - previous_partial_match;
               
               if(buffer.ContainsAt (pattern, 0, pattern_offset))
               {
                  file_stream.Position = original_position;
                  return buffer_offset - buffer_size + previous_partial_match;
               }
            }

            if (before != null && before_previous_partial_match >= 0 && (int) buffer_size > before_previous_partial_match)
            {
               int before_offset = (int) buffer_size - before_previous_partial_match;
               if (buffer.ContainsAt (before, 0, before_offset))
               {
                  file_stream.Position = original_position;
                  return -1;
               }
            }

            // (2) pattern contained in current buffer

            long location = buffer.Find (pattern);
            if (location >= 0)
            {
               file_stream.Position = original_position;
               return buffer_offset + location;
            }

            if (before != null && buffer.Find (before) >= 0)
            {
               file_stream.Position = original_position;
               return -1;
            }

            // (3) partial match

            previous_partial_match = buffer.EndsWithPartialMatch (pattern);

            if (before != null)
            before_previous_partial_match = buffer.EndsWithPartialMatch (before);

            buffer_offset += buffer_size;
         }

         // Since we hit the end of the file, reset the status before continuing.

         file_stream.Position = original_position;
         return -1;
      }
      
      public long Find (ByteVector pattern, long from_offset)
      {
         return Find (pattern, from_offset, null);
      }
      
      public long Find (ByteVector pattern)
      {
         return Find (pattern, 0);
      }
      
      long RFind (ByteVector pattern, long from_offset, ByteVector before)
      {
         Mode = AccessMode.Read;
         
         if (pattern.Count > buffer_size)
            return -1;

         // The position in the file that the current buffer starts at.

         ByteVector buffer;

         // These variables are used to keep track of a partial match that happens at
         // the end of a buffer.

         /*
         int previous_partial_match = -1;
         int before_previous_partial_match = -1;
         */

         // Save the location of the current read pointer.  We will restore the
         // position using seek() before all returns.

         long original_position = file_stream.Position;

         // Start the search at the offset.

         long buffer_offset;
         if (from_offset == 0)
            Seek (-1 * (int) buffer_size, System.IO.SeekOrigin.End);
         else
            Seek (from_offset + -1 * (int) buffer_size, System.IO.SeekOrigin.Begin);
         
         buffer_offset = file_stream.Position;
         
         // See the notes in find() for an explanation of this algorithm.

         for (buffer = ReadBlock((int)buffer_size); buffer.Count > 0; buffer = ReadBlock ((int)buffer_size))
         {
            // TODO: (1) previous partial match

            // (2) pattern contained in current buffer

            long location = buffer.RFind (pattern);
            if (location >= 0)
            {
               file_stream.Position = original_position;
               return buffer_offset + location;
            }

            if(before != null && buffer.Find (before) >= 0)
            {
               file_stream.Position = original_position;
               return -1;
            }

            // TODO: (3) partial match

            buffer_offset -= buffer_size;
            file_stream.Position = buffer_offset;
         }

         // Since we hit the end of the file, reset the status before continuing.

         file_stream.Position = original_position;
         return -1;
      }
      
      public long RFind (ByteVector pattern, long from_offset)
      {
         return RFind (pattern, from_offset, null);
      }
      
      public long RFind (ByteVector pattern)
      {
         return RFind (pattern, 0);
      }

      public void Insert (ByteVector data, long start, long replace)
      {
         Mode = AccessMode.Write;
         
         if (data.Count == replace)
         {
            file_stream.Position = start;
            WriteBlock (data);
            return;
         }
         else if(data.Count < replace)
         {
            file_stream.Position = start;
            WriteBlock (data);
            RemoveBlock (start + data.Count, replace - data.Count);
            return;
         }

         // Woohoo!  Faster (about 20%) than id3lib at last.  I had to get hardcore
         // and avoid TagLib's high level API for rendering just copying parts of
         // the file that don't contain tag data.
         //
         // Now I'll explain the steps in this ugliness:

         // First, make sure that we're working with a buffer that is longer than
         // the *differnce* in the tag sizes.  We want to avoid overwriting parts
         // that aren't yet in memory, so this is necessary.
         
         int buffer_length = (int) BufferSize;
         while (data.Count - replace > buffer_length)
            buffer_length += (int) BufferSize;

         // Set where to start the reading and writing.

         long read_position = start + replace;
         long write_position = start;
         
         byte [] buffer;
         byte [] about_to_overwrite;

         // This is basically a special case of the loop below.  Here we're just
         // doing the same steps as below, but since we aren't using the same buffer
         // size -- instead we're using the tag size -- this has to be handled as a
         // special case.  We're also using File::writeBlock() just for the tag.
         // That's a bit slower than using char *'s so, we're only doing it here.

         file_stream.Position = read_position;
         about_to_overwrite = ReadBlock (buffer_length).Data;
         read_position += buffer_length;

         file_stream.Position = write_position;
         WriteBlock (data);
         write_position += data.Count;

         buffer = new byte [about_to_overwrite.Length];
         System.Array.Copy (about_to_overwrite, 0, buffer, 0, about_to_overwrite.Length);

         // Ok, here's the main loop.  We want to loop until the read fails, which
         // means that we hit the end of the file.

         while (buffer_length != 0)
         {
            // Seek to the current read position and read the data that we're about
            // to overwrite.  Appropriately increment the readPosition.

            file_stream.Position = read_position;
            
            int bytes_read = file_stream.Read (about_to_overwrite, 0, buffer_length < about_to_overwrite.Length ? buffer_length : about_to_overwrite.Length);
            read_position += buffer_length;

            // Seek to the write position and write our buffer.  Increment the
            // writePosition.
            
            file_stream.Position = write_position;
            file_stream.Write (buffer, 0, buffer_length < buffer.Length ? buffer_length : buffer.Length);
            write_position += buffer_length;
            
            // Make the current buffer the data that we read in the beginning.
            System.Array.Copy (about_to_overwrite, 0, buffer, 0, bytes_read);
            
            // Again, we need this for the last write.  We don't want to write garbage
            // at the end of our file, so we need to set the buffer size to the amount
            // that we actually read.

            buffer_length = bytes_read;
         }
      }
      
      public void Insert (ByteVector data, long start)
      {
         Insert (data, start, 0);
      }
      
      public void Insert (ByteVector data)
      {
         Insert (data, 0);
      }
      
      public void RemoveBlock (long start, long length)
      {
         if (length == 0)
            return;
         
         Mode = AccessMode.Write;
         
         int buffer_length = (int) BufferSize;
         
         long read_position = start + length;
         long write_position = start;
         
         ByteVector buffer = (byte) 1;

         while(buffer.Count != 0)
         {
            file_stream.Position = read_position;
            buffer = ReadBlock (buffer_length);
            read_position += buffer.Count;

            file_stream.Position = write_position;
            WriteBlock (buffer);
            write_position += buffer.Count;
         }
         
         Truncate (write_position);
      }
      
      [Obsolete("This method is obsolete; it has no real use.")]
      public void RemoveBlock (long start) {}
      
      [Obsolete("This method is obsolete; it has no real use.")]
      public void RemoveBlock () {}
      
      [Obsolete("This property is obsolete; Invalid files now throw exceptions.")]
      public bool IsValid {get {return true;}}
      
      public void Seek (long offset, System.IO.SeekOrigin p)
      {
         if (Mode != AccessMode.Closed)
            file_stream.Seek (offset, p);
      }
      
      public void Seek (long offset)
      {
         Seek (offset, System.IO.SeekOrigin.Begin);
      }
      
      public long Tell
      {
         get {return (Mode == AccessMode.Closed) ? 0 : file_stream.Position;}
      }
      
      public long Length
      {
         get {return (Mode == AccessMode.Closed) ? 0 : file_stream.Length;}
      }
      
      public AccessMode Mode
      {
         get
         {
            return (file_stream == null) ? AccessMode.Closed : (file_stream.CanWrite) ? AccessMode.Write : AccessMode.Read;
         }
         set
         {
            if (Mode == value || (Mode == AccessMode.Write && value == AccessMode.Read))
               return;
            
            if (file_stream != null)
               file_stream.Close ();
            file_stream = null;
            
            if (value == AccessMode.Read)
               file_stream = file_abstraction.ReadStream;
            else if (value == AccessMode.Write)
               file_stream = file_abstraction.WriteStream;
            
            Mode = value;
         }
      }
      
      public abstract void RemoveTags (TagTypes types);
      public abstract Tag GetTag (TagTypes type, bool create);
      
      public Tag GetTag (TagTypes type)
      {
         return GetTag (type, false);
      }
            
      public static File Create(string path)
      {
         return Create(path, null, ReadStyle.Average);
      }
                  
      public static File Create(string path, ReadStyle style) 
      {
         return Create(path, null, style);
      }
      
      public static File Create(string path, string mimetype, ReadStyle style)
      {
         foreach (FileTypeResolver resolver in file_type_resolvers)
         {
            File file = resolver(path, style);
            if(file != null)
               return file;
         }
         
         if(mimetype == null)
         {
            /* ext = System.IO.Path.GetExtension(path).Substring(1) */
            string ext = String.Empty;
        
            try
            {
               int index = path.LastIndexOf(".") + 1;
               if(index >= 1 && index < path.Length)
                  ext = path.Substring(index, path.Length - index);
            } catch {
               /* Proper exception will be thrown later */
            }
            
            mimetype = "taglib/" + ext.ToLower();
         }
 
         if(!FileTypes.AvailableTypes.ContainsKey(mimetype)) {
            throw new UnsupportedFormatException(String.Format("{0} ({1})", path, mimetype));
         }
         
         Type file_type = FileTypes.AvailableTypes[mimetype];
                 
         try {
            File file = (File)Activator.CreateInstance(file_type, new object [] { path, style });
            file.MimeType = mimetype;
            return file;
         } catch(System.Reflection.TargetInvocationException e) {
            throw e.InnerException;
         }
      }

      // Added by Albert/MediaPortal
      public static File Create(IFileAbstraction fileAbstraction, string fileName, string mimetype, ReadStyle style)
      {
         if(mimetype == null)
         {
            /* ext = System.IO.Path.GetExtension(path).Substring(1) */
            string ext = String.Empty;
        
            try
            {
               int index = fileName.LastIndexOf(".") + 1;
               if(index >= 1 && index < fileName.Length)
                  ext = fileName.Substring(index, fileName.Length - index);
            } catch {
               /* Proper exception will be thrown later */
            }
            
            mimetype = "taglib/" + ext.ToLower();
         }
 
         if(!FileTypes.AvailableTypes.ContainsKey(mimetype)) {
            throw new UnsupportedFormatException(String.Format("{0} ({1})", fileName, mimetype));
         }
         
         Type file_type = FileTypes.AvailableTypes[mimetype];
                 
         try {
            File file = (File)Activator.CreateInstance(file_type, new object [] { fileAbstraction, style });
            file.MimeType = mimetype;
            return file;
         } catch(System.Reflection.TargetInvocationException e) {
            throw e.InnerException;
         }
      }
      
      public static void AddFileTypeResolver (FileTypeResolver resolver)
      {
         if (resolver != null)
            file_type_resolvers.Insert (0, resolver);
      }
      
      public static void SetFileAbstractionCreator (FileAbstractionCreator creator)
      {
         if (creator != null)
            file_abstraction_creator = creator;
      }
            
      internal static FileAbstractionCreator GetFileAbstractionCreator()
      {
         return file_abstraction_creator; 
      }
      
      //////////////////////////////////////////////////////////////////////////
      // protected members
      //////////////////////////////////////////////////////////////////////////
      [Obsolete("This property is obsolete; invalid files now throw exceptions.")]
      protected void SetValid (bool valid)
      {
         if (valid == false)
            throw new CorruptFileException ();
      }
      
      protected void Truncate (long length)
      {
         Mode = AccessMode.Write;
         file_stream.SetLength (length);
      }
      
      protected static uint BufferSize {get {return buffer_size;}}
      
      
      //////////////////////////////////////////////////////////////////////////
      // LocalFileAbstraction class
      //////////////////////////////////////////////////////////////////////////
      private class LocalFileAbstraction : IFileAbstraction
      {
         private string name;
         
         public LocalFileAbstraction (string file)
         {
            name = file;
         }
         
         public string Name {get {return name;}}
         
         public System.IO.Stream ReadStream
         {
            get {return System.IO.File.Open (Name, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);}
         }
         
         public System.IO.Stream WriteStream
         {
            get {return System.IO.File.Open (Name, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);}
         }
         
         public static IFileAbstraction CreateFile (string path)
         {
            return new LocalFileAbstraction (path);
         }
      }
      
      //////////////////////////////////////////////////////////////////////////
      // IFileAbstraction interface
      //////////////////////////////////////////////////////////////////////////
      public interface IFileAbstraction
      {
         string Name {get;}
         
         System.IO.Stream ReadStream  {get;}
         System.IO.Stream WriteStream {get;}
      }
   }
}

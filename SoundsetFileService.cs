using System;
using System.IO;
using System.Text;


namespace nwn2_Chatter
{
	static class SoundsetFileService
	{
		#region Fields (static)
		internal const string Ver10 = "SSF V1.0";
		internal const string Ver11 = "SSF V1.1";
		#endregion Fields (static)


		#region Methods (static)
		/// <summary>
		/// Reads a SoundSetFile given a <c>string</c> and a few pointers in
		/// which to store the relevant data.
		/// </summary>
		/// <param name="pfe"></param>
		/// <param name="resrefs"></param>
		/// <param name="strrefs"></param>
		/// <param name="ver"></param>
		/// <returns>count of entries if successful and count is greater than 0
		/// else <c>-1</c></returns>
		internal static int ReadSoundsetFile(string pfe, ref string[] resrefs, ref uint[] strrefs, ref string ver)
		{
			if (File.Exists(pfe))
			{
				byte[] bytes = null;

/*				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var br = new BinaryReader(fs))
				{
					bytes = br.ReadBytes((int)br.BaseStream.Length);
				}
//				var buffer = new byte[bytes.Length];
//				using (var ms = new MemoryStream(bytes, 0, bytes.Length))
//				{
//					ms.Write(bytes, 0, bytes.Length);
//					bytes = ms.ToArray();
//				} */

				try
				{
					// NOTE: File.ReadAllBytes() does *not* close the handle to the
					// directory (in .net 3.5 at least) until Chatter is closed.
					// Or it could have something to do with 'Environment.CurrentDirectory'
					// ... etc.

					bytes = File.ReadAllBytes(pfe);
				}
//				catch (ArgumentException ae)            {} // taken care of by File.Exists()
//				catch (ArgumentNullException ane)       {} // taken care of by File.Exists()
//				catch (PathTooLongException ptle)       {} // taken care of by File.Exists()
//				catch (DirectoryNotFoundException dnfe) {} // taken care of by File.Exists()
//				catch (FileNotFoundException fnfe)      {} // taken care of by File.Exists()
//				catch (NotSupportedException nse)       {} // taken care of by File.Exists() probably
				catch (IOException ioe)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												ioe.Message,
												"IOException" + Environment.NewLine + ioe.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return -1;
				}
				catch (UnauthorizedAccessException uae)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												uae.Message,
												"UnauthorizedAccessException" + Environment.NewLine + uae.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return -1;
				}
//				catch (SecurityException se) // 'SecurityException' could not be found.
//				{}
//				finally {}
/*
ArgumentException: path is a zero-length string, contains only white space, or
contains one or more invalid characters as defined by InvalidPathChars.

ArgumentNullException: path is null.

PathTooLongException: The specified path, file name, or both exceed the
system-defined maximum length. For example, on Windows-based platforms, paths
must be less than 248 characters, and file names must be less than 260
characters.

DirectoryNotFoundException: The specified path is invalid (for example, it is on
an unmapped drive).

IOException: An I/O error occurred while opening the file.

UnauthorizedAccessException: This operation is not supported on the current
platform. -or- path specified a directory. -or- The caller does not have the
required permission.

FileNotFoundException: The file specified in path was not found.

NotSupportedException: path is in an invalid format.

SecurityException: The caller does not have the required permission.
*/
				if (bytes != null)
					return ReadSoundsetFile(bytes, ref resrefs, ref strrefs, ref ver);
			}
			return -1;
		}

		/// <summary>
		/// Reads a SoundSetFile given an array of <c>bytes</c> and a few
		/// pointers in which to store the relevant data.
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="resrefs"></param>
		/// <param name="strrefs"></param>
		/// <param name="ver"></param>
		/// <returns></returns>
		internal static int ReadSoundsetFile(byte[] bytes, ref string[] resrefs, ref uint[] strrefs, ref string ver)
		{
			uint pos = 0; int b;

			// header ->
			var buffer = new byte[8]; // filetype + ssf version -> "SSF V1.0" (nwn) or "SSF V1.1" (nwn2)
			for (b = 0; b != 8; ++b)
				buffer[b] = bytes[pos++];

			ver = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
			//logfile.Log(". ver= " + ver);

			if (ver == Ver10 || ver == Ver11)
			{
				bool le = BitConverter.IsLittleEndian; // hardware architecture

				buffer = new byte[4]; // EntryCount
				for (b = 0; b != 4; ++b)
					buffer[b] = bytes[pos++];

				if (!le) Array.Reverse(buffer);
				uint count = BitConverter.ToUInt32(buffer, 0);
				//logfile.Log(". count= " + count);

				if (count == (uint)49 || count == (uint)51)
				{
					buffer = new byte[4]; // offset of the EntryTable (static @ 40-bytes)
					for (b = 0; b != 4; ++b)
						buffer[b] = bytes[pos++];

					if (!le) Array.Reverse(buffer);
					uint offset = BitConverter.ToUInt32(buffer, 0);
					//logfile.Log(". offset= " + offset);

//					pos += 24; // null block


					// entrytable ->
					uint dataoffset;

					pos = offset; // = 40 duh.

					var dataoffsets = new uint[count];
					for (int i = 0; i != count; ++i)
					{
						buffer = new byte[4];
						for (b = 0; b != 4; ++b)
							buffer[b] = bytes[pos++];

						if (!le) Array.Reverse(buffer);
						dataoffset = BitConverter.ToUInt32(buffer, 0);
						//logfile.Log(". dataoffsets[" + i + "]= " + dataoffset);

						dataoffsets[i] = dataoffset;
					}


					// datatable ->
					int resreflength = (ver == Ver10) ? 16 : 32;

					resrefs = new string[dataoffsets.Length];
					strrefs = new uint[dataoffsets.Length];

					string resref; uint strref;

					for (int i = 0; i != dataoffsets.Length; ++i)
					{
						pos = dataoffsets[i];

						buffer = new byte[resreflength];
						for (b = 0; b != resreflength; ++b)
							buffer[b] = bytes[pos++];

						resref = Encoding.ASCII.GetString(buffer, 0, buffer.Length).TrimEnd('\0');
						//logfile.Log(". resref= " + resref);
						resrefs[i] = resref;

						buffer = new byte[4];
						for (b = 0; b != 4; ++b)
							buffer[b] = bytes[pos++];

						if (!le) Array.Reverse(buffer);
						strref = BitConverter.ToUInt32(buffer, 0);
						//logfile.Log(". strref= " + strref);
						strrefs[i] = strref;
					}

					return (int)count;
				}
			}
			return -1;
		}

		/// <summary>
		/// Writes a SoundSetFile based on a specified
		/// <c><see cref="ChatPageControl"/></c>.
		/// </summary>
		/// <param name="chatter"></param>
		/// <remarks>SSF 1.0 files are 1216 bytes. SSF 1.1 files are either 2000
		/// bytes or 2080 bytes depending on whether or not they have the extra
		/// 2 attack voices #49 and #50 (note that they don't have to have any
		/// voices assigned - they just have to be there). The Chatter shall
		/// output 2000-byte files unless
		/// <c><see cref="Chatter.Extended">Chatter.Extended</see></c> is
		/// <c>true</c> in which case a 2080-byte file gets written.</remarks>
		internal static void WriteSoundsetFile(ChatPageControl chatter)
		{
			string pfe = chatter._pfe; // shall not be null or 0-length

			string dir = Path.GetDirectoryName(pfe);
			if (!Directory.Exists(dir))
			{
				try
				{
					Directory.CreateDirectory(dir);
				}
				catch (PathTooLongException ptle) // I can't make sense of these; handle them all ->
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												ptle.Message,
												"PathTooLongException" + Environment.NewLine + ptle.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
				catch (DirectoryNotFoundException dnfe)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												dnfe.Message,
												"DirectoryNotFoundException" + Environment.NewLine + dnfe.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
				catch (IOException ioe)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												ioe.Message,
												"IOException" + Environment.NewLine + ioe.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
				catch (UnauthorizedAccessException uae)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												uae.Message,
												"UnauthorizedAccessException" + Environment.NewLine + uae.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
				catch (ArgumentNullException ane)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												ane.Message,
												"ArgumentNullException" + Environment.NewLine + ane.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
				catch (ArgumentException ae)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												ae.Message,
												"ArgumentException" + Environment.NewLine + ae.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
				catch (NotSupportedException nse)
				{
					using (var ib = new Infobox(Infobox.Title_excep,
												nse.Message,
												"NotSupportedException" + Environment.NewLine + nse.StackTrace,
												InfoboxType.Error,
												InfoboxButtons.Abort))
					{
						ib.ShowDialog();
					}
					return;
				}
//				finally {}
/*
IOException: The directory specified by path is read-only.

UnauthorizedAccessException: The caller does not have the required permission.

ArgumentException: path is a zero-length string, contains only white space, or
contains one or more invalid characters as defined by InvalidPathChars. -or-
path is prefixed with, or contains only a colon character (:).

ArgumentNullException: path is null.

PathTooLongException: The specified path, file name, or both exceed the
system-defined maximum length. For example, on Windows-based platforms, paths
must be less than 248 characters and file names must be less than 260
characters.

DirectoryNotFoundException: The specified path is invalid (for example, it is on
an unmapped drive).

NotSupportedException: path contains a colon character (:) that is not part of a
drive label ("C:\").
*/
			}

			string[] resrefs = chatter._resrefs;
			uint[]   strrefs = chatter._strrefs;

			try
			{
				using (var fs = new FileStream(pfe, FileMode.Create, FileAccess.Write, FileShare.None))
				if (fs != null)
				using (var bw = new BinaryWriter(fs)) // 'ArgumentException' and 'ArgumentNullException' should be taken care of by this point <-
				{
					string ver; int length; uint count;

					if (Chatter.Output == SsfFormat.ssf10)
					{
						ver = Ver10; // nwn "SSF V1.0"
						length = 16; // nwn 16-byte resrefs
					}
					else // Chatter.Output == SsfFormat.ssf11
					{
						ver = Ver11; // nwn2 "SSF V1.1"
						length = 32; // nwn2 32-byte resrefs
					}

					if (Chatter.Extended) count = (uint)51;
					else                  count = (uint)49;

					bool le = BitConverter.IsLittleEndian; // hardware architecture


					// header ->
					byte[] bytes = Encoding.ASCII.GetBytes(ver);
					bw.Write(bytes);


					bytes = BitConverter.GetBytes(count);
					if (!le) Array.Reverse(bytes);
					bw.Write(bytes);

					bytes = BitConverter.GetBytes((uint)40);
					if (!le) Array.Reverse(bytes);
					bw.Write(bytes);

					for (int i = 0; i != 24; ++i)
						bw.Write((byte)0);


					// entry table ->
					uint offset;
					for (uint i = 0; i != count; ++i)
					{
						offset = (uint)40 + count * (uint)4 + i * ((uint)length + (uint)4);
						bytes = BitConverter.GetBytes(offset);
						if (!le) Array.Reverse(bytes);
						bw.Write(bytes);
					}


					// data table ->
					for (int i = 0; i != count; ++i)
					{
						if (i < resrefs.Length)
						{
							if (resrefs[i].Length > length) // write resref string ->
							{
								byte[] resref = Encoding.ASCII.GetBytes(resrefs[i]);
								bytes = new byte[length];
								Buffer.BlockCopy(resref, 0, bytes, 0, length);
							}
							else
								bytes = Encoding.ASCII.GetBytes(resrefs[i]);

							bw.Write(bytes);

							for (int j = bytes.Length; j != length; ++j)
							{
								bw.Write((byte)0); // pad resref out to 'length'
							}

							bytes = BitConverter.GetBytes(strrefs[i]); // write strref uint ->
							if (!le) Array.Reverse(bytes);
							bw.Write(bytes);
						}
						else
						{
							for (int j = 0; j != length; ++j) // write null resref
								bw.Write((byte)0);

							bw.Write(UInt32.MaxValue); // write -1 strref
						}
					}
				}
			}
//			catch (ArgumentNullException ane)       {} // 'FileStream' exceptions ->
//			catch (FileNotFoundException fnfe)      {}
//			catch (PathTooLongException ptle)       {}
//			catch (DirectoryNotFoundException dnfe) {} // either directory exists, was created, or 'return' above^
			catch (ArgumentOutOfRangeException aoore)
			{
				using (var ib = new Infobox(Infobox.Title_excep,
											aoore.Message,
											"ArgumentOutOfRangeException" + Environment.NewLine + aoore.StackTrace,
											InfoboxType.Error,
											InfoboxButtons.Abort))
				{
					ib.ShowDialog();
				}
			}
			catch (ArgumentException ae)
			{
				using (var ib = new Infobox(Infobox.Title_excep,
											ae.Message,
											"ArgumentException" + Environment.NewLine + ae.StackTrace,
											InfoboxType.Error,
											InfoboxButtons.Abort))
				{
					ib.ShowDialog();
				}
			}
			catch (NotSupportedException nse)
			{
				using (var ib = new Infobox(Infobox.Title_excep,
											nse.Message,
											"NotSupportedException" + Environment.NewLine + nse.StackTrace,
											InfoboxType.Error,
											InfoboxButtons.Abort))
				{
					ib.ShowDialog();
				}
			}
			catch (IOException ioe)
			{
				using (var ib = new Infobox(Infobox.Title_excep,
											ioe.Message,
											"IOException" + Environment.NewLine + ioe.StackTrace,
											InfoboxType.Error,
											InfoboxButtons.Abort))
				{
					ib.ShowDialog();
				}
			}
			catch (UnauthorizedAccessException uae)
			{
				using (var ib = new Infobox(Infobox.Title_excep,
											uae.Message,
											"UnauthorizedAccessException" + Environment.NewLine + uae.StackTrace,
											InfoboxType.Error,
											InfoboxButtons.Abort))
				{
					ib.ShowDialog();
				}
			}
//			catch (SecurityException se) // 'SecurityException' could not be found.
//			{}
//			finally {}
/*
ArgumentNullException: path is null.

ArgumentException: path is an empty string (""), contains only white space, or
contains one or more invalid characters. -or- path refers to a non-file device,
such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.

NotSupportedException: path refers to a non-file device, such as "con:",
"com1:", "lpt1:", etc. in a non-NTFS environment.

FileNotFoundException: The file cannot be found, such as when mode is
FileMode.Truncate or FileMode.Open, and the file specified by path does not
exist. The file must already exist in these modes.

IOException: An I/O error occurs, such as specifying FileMode.CreateNew and the
file specified by path already exists. -or- The system is running Windows 98 or
Windows 98 Second Edition and share is set to FileShare.Delete. -or- The stream
has been closed.

SecurityException: The caller does not have the required permission.

DirectoryNotFoundException: The specified path is invalid, such as being on an
unmapped drive.

UnauthorizedAccessException: The access requested is not permitted by the
operating system for the specified path, such as when access is Write or
ReadWrite and the file or directory is set for read-only access.

PathTooLongException: The specified path, file name, or both exceed the
system-defined maximum length. For example, on Windows-based platforms, paths
must be less than 248 characters, and file names must be less than 260
characters.

ArgumentOutOfRangeException: mode contains an invalid value.
*/
		}
		#endregion Methods (static)
	}
}

/*
Bioware_Aurora_SSF_Format.pdf

3. Header Format
---------------------------------------
Value       Size/Type       Description
---------------------------------------
FileType    4 char          4-char file type string. "SSF "
FileVersion 4 char          4-char SSF version. "V1.0"
EntryCount  32-bit unsigned Number of entries in Entry Table
TableOffset 32-bit unsigned Byte offset of Entry Table from start of file
Padding     24 bytes        NULL padding

4. Entry Table
32-bit offsets to the entries.

5. Data Table
-------------------------------------
Value     Size/Type       Description
-------------------------------------
ResRef    16 char         Name of sound file to play [increased to 32-char in Version 1.1 for nwn2 resref strings]
StringRef 32-bit unsigned Index to string in dialog.tlk
*/

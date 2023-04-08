using System;
using System.IO;
using System.Text;


namespace nwn2_Chatter
{
	static class SoundsetFileService
	{
		#region Fields (static)
		internal const string Ver1 = "SSF V1.0";
		internal const string Ver2 = "SSF V1.1";
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
				return ReadSoundsetFile(File.ReadAllBytes(pfe), ref resrefs, ref strrefs, ref ver);

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

			if (ver == Ver1 || ver == Ver2)
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
					int resreflength = (ver == Ver1) ? 16 : 32;

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
			string   pfe     = chatter._pfe;
			string[] resrefs = chatter._resrefs;
			uint[]   strrefs = chatter._strrefs;

			using (var fs = new FileStream(pfe, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				var bw = new BinaryWriter(fs);

				string ver; int length; uint count;

				if (Chatter.Output == SsfFormat.ssf10)
				{
					ver = Ver1;		// nwn "SSF V1.0"
					length = 16;	// nwn 16-byte resrefs
				}
				else // Chatter._outputformat == SsfFormat.ssf11
				{
					ver = Ver2;		// nwn2 "SSF V1.1"
					length = 32;	// nwn2 32-byte resrefs
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

						bw.Write((uint)0xFFFFFFFF); // write -1 strref
					}
				}
			}
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

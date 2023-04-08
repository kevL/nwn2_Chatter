using System;
using System.Collections.Generic;
using System.IO;
//using System.Windows.Forms;
using System.Text;


namespace nwn2_Chatter
{
	static class TalkReader
	{
		#region Fields (static)
		/// <summary>
		/// The dialog-dictionary. The dictionary does not contain unassigned
		/// entries so check if a key is valid before trying to get its value.
		/// </summary>
		/// <remarks>Does not really need to be "Sorted" ...</remarks>
		internal static SortedDictionary<int, string> DictDialo =
					new SortedDictionary<int, string>();

		internal static SortedDictionary<int, string> DictCusto =
					new SortedDictionary<int, string>();

		internal const int bitCusto = 0x01000000; // flag in the strref for Custo talktable
		internal const int strref   = 0x00FFFFFF; // isolates the actual strref-val
		internal const int invalid  = -1;         // no entry, a blank string; note that a valid id without an entry is also a blank string

		const uint TEXT_PRESENT    =  1; // Data flag - a text is present in the Texts area

		const uint HeaderStart     =  0; // start-byte of the byte-array
//		const uint LanguageIdStart =  8; // offset of the field that contains the LanguageId
		const uint TextsCountField = 12; // offset of the field that contains the TextsCount
		const uint TextsStartField = 16; // offset of the field that contains the offset of the Texts area

		const uint DataStart       = 20; // length in bytes of the Header area
		const uint DataLength      = 40; // length in bytes of 1 Data area

		const uint TextStartField  = 28; // offset from the start of a Data ele to its TextStart field
		const uint TextLengthField = 32; // offset from the start of a Data ele to its TextLength field

		const string Ver = "TLK V3.0";

		internal static int loDialo, hiDialo;
		internal static int loCusto, hiCusto;

		internal static string AltLabel;
		#endregion Fields (static)


		#region Methods (static)
		/// <summary>
		/// Adds key-value pairs [(int)strref, (string)text] to
		/// <c><see cref="DictDialo"/></c> or <c><see cref="DictCusto"/></c>.
		/// </summary>
		/// <param name="pfe">fullpath of a talktable</param>
//		/// <param name="it">a menuitem to check/uncheck</param>
		/// <returns><c>true</c> if successful</returns>
		/// <remarks>See description of .tlk Format at the bot of this file.</remarks>
		internal static bool Load(string pfe) //, ToolStripMenuItem it
		{
			SortedDictionary<int, string> dict;

			bool alt = false; //(it == Yata.that.it_PathTalkC);
			if (alt)
			{
				dict = DictCusto;
				AltLabel = null;
			}
			else
				dict = DictDialo;

			dict.Clear();

			if (File.Exists(pfe))
			{
//				using (FileStream fs = File.OpenRead(pfeTlk)){}

				if (alt) AltLabel = Path.GetFileNameWithoutExtension(pfe).ToUpperInvariant();

				byte[] bytes = File.ReadAllBytes(pfe);

				uint pos = HeaderStart;
				uint b;

				var buffer = new byte[8];
				for (b = 0; b != 8; ++b)
					buffer[b] = bytes[pos++];

				string sUtf8 = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
				if (sUtf8 == Ver) // check if v3.0 tlktable
				{
					bool le = BitConverter.IsLittleEndian; // hardware architecture

//					pos = HeaderStart + LanguageIdStart;
//					buffer = new byte[4];
//					for (b = 0; b != 4; ++b)
//						buffer[b] = bytes[pos++];
//					if (!le) Array.Reverse(buffer);
//					uint LanguageId = BitConverter.ToUInt32(buffer, 0);


					pos = HeaderStart + TextsCountField;

					buffer = new byte[4];
					for (b = 0; b != 4; ++b)
						buffer[b] = bytes[pos++];

					if (!le) Array.Reverse(buffer);
					uint TextsCount = BitConverter.ToUInt32(buffer, 0);


					pos = HeaderStart + TextsStartField;

					buffer = new byte[4];
					for (b = 0; b != 4; ++b)
						buffer[b] = bytes[pos++];

					if (!le) Array.Reverse(buffer);
					uint TextsStart = BitConverter.ToUInt32(buffer, 0);
//					uint TextsStart = DataStart + (TextCount * DataLength);


					uint start;

					for (uint i = 0; i != TextsCount; ++i)
					{
						start = DataStart + i * DataLength;
						pos = start;

						buffer = new byte[4];
						for (b = 0; b != 4; ++b)
							buffer[b] = bytes[pos++];

						if (!le) Array.Reverse(buffer);
						uint Flags = BitConverter.ToUInt32(buffer, 0);


						if ((Flags & TEXT_PRESENT) != 0)
						{
							pos = start + TextStartField;

							buffer = new byte[4];
							for (b = 0; b != 4; ++b)
								buffer[b] = bytes[pos++];

							if (!le) Array.Reverse(buffer);
							uint TextStart = BitConverter.ToUInt32(buffer, 0);


							pos = start + TextLengthField;

							buffer = new byte[4];
							for (b = 0; b != 4; ++b)
								buffer[b] = bytes[pos++];

							if (!le) Array.Reverse(buffer);
							uint TextLength = BitConverter.ToUInt32(buffer, 0);


							pos = TextsStart + TextStart;

							buffer = new byte[TextLength];
							for (b = 0; b != TextLength; ++b)
								buffer[b] = bytes[pos++];

							string text = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

							dict.Add((int)i, text);
						}
					}

					if (dict.Count != 0)
					{
						int lo = Int32.MaxValue;
						int hi = Int32.MinValue;

						foreach (int key in dict.Keys)
						{
							if (key > hi) hi = key;
							if (key < lo) lo = key;
						}

						if (alt) { loCusto = lo; hiCusto = hi; }
						else     { loDialo = lo; hiDialo = hi; }

//						it.Checked = true;
						return true;
					}
				}
			}
//			it.Checked = false;
			return false;
		}
		#endregion Methods (static)
	}
}

/*
__HEADER__
FileType            4-char "TLK "												0x00000000
FileVersion         4-char "V3.0"												0x00000020
LanguageID          DWORD  Language ID											0x00000040
StringCount         DWORD  Count of strings										0x00000060
StringEntriesOffset DWORD  Offset from start of file to the StringEntryTable	0x00000080

__STRINGDATAELEMENT__															0x000000A0 [start] bit 160 / byte 20
Flags          DWORD   Flags about this StrRef.									0x00000000
SoundResRef    16-char ResRef of the wave file associated with this string.		0x00000020
                       Unused characters are nulls.
VolumeVariance DWORD   not used													0x000000A0
PitchVariance  DWORD   not used													0x000000C0
OffsetToString DWORD   Offset from StringEntriesOffset to the beginning of the	0x000000E0
                       StrRef's text.
StringSize     DWORD   Number of bytes in the string. Null terminating			0x00000100
                       characters are not stored, so this size does not include
                       a null terminator.
SoundLength    FLOAT   Duration in seconds of the associated wave file			0x00000120
																				0x00000140 [length] 320 bits / 40 bytes

__STRINGENTRYTABLE__
The StringEntryTable begins at the StringEntriesOffset specified in the Header
of the file, and continues to the end of the file. All the localized text is
contained in the StringEntryTable as non-null-terminated strings. As soon as one
string ends, the next one begins. kL_note: Consider it UTF8.
*/

// dialog.tlk -> 16777215 MaxVal 0x00FFFFFF
/*
The StrRef is a 32-bit unsigned integer that serves as an index into the table
of strings stored in the talk table. To specify an invalid StrRef, the talk
table system uses a StrRef in which all the bits are 1 (ie. 4294967295, or
0xFFFFFFFF, the maximum possible 32-bit unsigned value, or -1 if it were a
signed 32-bit value). When presented with the invalid StrRef value, the text
returned should be a blank string.

Valid StrRefs can have values of up to 0x00FFFFFF, or 16777215. Any higher
values will have the upper 2 bytes masked off and set to 0, so 0x01000001, or
16777217, for example, will be treated as StrRef 1.

If a module uses an alternate talk table, then bit 0x01000000 of a StrRef
specifies whether the StrRef should be fetched from the normal dialog.tlk or
from the alternate tlk file. If the bit is 0, the StrRef is fetched as normal
from dialog.tlk. If the bit is 1, then the StrRef is fetched from the
alternative talk table.

If the alternate tlk file does not exist, could not be loaded, or does not
contain the requested StrRef, then the StrRef is fetched as normal from the
standard dialog.tlk file. kL_note: But that's just silly.
*/

/*
string convert = "This is the string to be converted.";

// string to byte-array
byte[] buffer = System.Text.Encoding.UTF8.GetBytes(convert);

// byte-array to string
string s = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
*/

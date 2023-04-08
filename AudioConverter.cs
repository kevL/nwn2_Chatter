using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	static class AudioConverter
	{
		#region fields (static)
		const string EXT_BMU = ".bmu";
		const string EXT_MP3 = ".mp3";
		const string EXT_WAV = ".wav";

		internal const string TEMP_MP3 = "nwn2_Chatter" + EXT_MP3;
		internal const string TEMP_WAV = "nwn2_Chatter" + EXT_WAV;

		const string LAME_EXE = "lame.exe";
		#endregion fields (static)


		#region methods (static)
		/// <summary>
		/// Determines the file to use for the SpeechRecognition filestream
		/// converting it from BMU/MP3 to WAV if necessary.
		/// </summary>
		/// <param name="pfe">path_file_extension</param>
		/// <returns>the fullpath to a PCM-wave file else a blank-string</returns>
		/// <remarks>The result shall be PCM 44.1kHz 16-bit Mono.</remarks>
		internal static string deterwave(string pfe)
		{
			string path = Path.GetTempPath();

			if (   pfe.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase) // prep .BMU ->
				|| pfe.EndsWith(EXT_BMU, StringComparison.InvariantCultureIgnoreCase))
			{
				var chars = new char[3];
				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var br = new BinaryReader(fs);
					chars = br.ReadChars(3);
					br.Close();
				}

				if (   chars[0] == 'B' // because .BMUs are .MP3s and NwN2 labels them as .WAVs
					&& chars[1] == 'M'
					&& chars[2] == 'U')
				{
					string pfeT = Path.Combine(path, TEMP_MP3); // so label it as .MP3 and allow the next block to catch it.

					File.Delete(pfeT);
					File.Copy(pfe, pfeT);

					pfe = pfeT;
				}
			}

			if (pfe.EndsWith(EXT_MP3, StringComparison.InvariantCultureIgnoreCase)) // convert to .WAV file ->
			{
				string pfeT = Path.Combine(path, TEMP_WAV);
				File.Delete(pfeT);

//				string execpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//				var info = new ProcessStartInfo(Path.Combine(execpath, LAME_EXE));
				var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, LAME_EXE));
				info.Arguments = "--decode \"" + pfe + "\" \"" + pfeT + "\"";
				info.WindowStyle = ProcessWindowStyle.Hidden;
				info.UseShellExecute = false;
				info.CreateNoWindow  = true;

				using (Process proc = Process.Start(info))
				{
					proc.WaitForExit();
				}

				pfe = pfeT;
			}

// http://www.topherlee.com/software/pcm-tut-wavformat.html
//  1- 4	"RIFF"				Marks the file as a riff file. Characters are each 1 byte long.
//  5- 8	File size (integer)	Size of the overall file - 8 bytes, in bytes (32-bit integer). Typically, you'd fill this in after creation.
//  9-12	"WAVE"				File Type Header. For our purposes, it always equals "WAVE".
// 13-16	"fmt "				Format chunk marker. Includes trailing null
// 17-20	16					Length of format data as listed above
// 21-22	1					Type of format (1 is PCM) - 2 byte integer
// 23-24	2					Number of Channels - 2 byte integer
// 25-28	44100				Sample Rate - 32 byte integer. Common values are 44100 (CD), 48000 (DAT). Sample Rate = Number of Samples per second, or Hertz.
// 29-32	176400				(Sample Rate * BitsPerSample * Channels) / 8.
// 33-34	4					(BitsPerSample * Channels) / 8.1 - 8 bit mono2 - 8 bit stereo/16 bit mono4 - 16 bit stereo
// 35-36	16					Bits per sample
// 37-40	"data"				"data" chunk header. Marks the beginning of the data section.
// 41-44	File size (data)	Size of the data section.

			string audiofile = String.Empty;

			if (pfe.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // check .WAV ->
			{
				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var br = new BinaryReader(fs);

					char[] c = br.ReadChars(16);					// start 0

					if (   c[ 0] == 'R' && c[ 1] == 'I' && c[ 2] == 'F' && c[ 3] == 'F'
						&& c[ 8] == 'W' && c[ 9] == 'A' && c[10] == 'V' && c[11] == 'E'
						&& c[12] == 'f' && c[13] == 'm' && c[14] == 't' && c[15] == ' ')
					{
						br.ReadBytes(4);							// start 16

						short format = br.ReadInt16();				// start 20: is PCM
						if (format == (short)1)
						{
							short channels = br.ReadInt16();		// start 22: is Mono
							if (channels == (short)1)
							{
								// TODO: Sample-rate and bit-depth should probably be relaxed.

								int rate = br.ReadInt32();			// start 24: is 44.1kHz
								if (rate == 44100)
								{
									br.ReadBytes(6);				// start 28
									short bits = br.ReadInt16();	// start 34: is 16-bit
									if (bits == (short)16)
									{
										audiofile = pfe;
									}
								}
							}
						}
					}
					br.Close();
				}
			}

			if (audiofile.Length == 0)
			using (var ib = new Infobox(Infobox.Title_error,
										"Failed to convert to 44.1kHz 16-bit Mono PCM-wave format.",
										null,
										InfoboxType.Error))
			{
				ib.ShowDialog();
			}

			return audiofile;
		}
		#endregion methods (static)
	}
}

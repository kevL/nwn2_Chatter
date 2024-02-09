using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using NAudio.Wave;


namespace nwn2_Chatter
{
	static class AudioConverter
	{
		#region enums
		/// <summary>
		/// Errorcodes for
		/// <c><see cref="DecodeVoiceFile()">DecodeVoiceFile()</see></c>.
		/// </summary>
		enum Fail
		{
			non,					// 0
			Delete_TemporaryMP3,	// 1
			Create_IntermediateMP3,	// 2
			Delete_TemporaryPCM,	// 3
			Create_ConvertedPCM,	// 4
			Config_InvalidPCM,		// 5
			Delete_FinalPCM,		// 6
			Create_FinalPCM,		// 7
			Config_InvalidADPCM,	// 8
			Format_InvalidPCM		// 9
		}
		#endregion enums


		#region fields (static)
		const string EXT_BMU = ".bmu";
		const string EXT_MP3 = ".mp3";
		const string EXT_WAV = ".wav";

		internal const string TEMP_MP3 = "nwn2_Chatter" + EXT_MP3;
		internal const string TEMP_WAV = "nwn2_Chatter" + EXT_WAV;

		const string LAME_EXE = "lame.exe";

		// http://icculus.org/SDL_sound/downloads/external_documentation/wavecomp.htm
		// NAudio appears to respect the Microsoft ADPCM format but the NwN2
		// ADPCM files are Intel ... yet it works.
//		const ushort WAVE_FORMAT_PCM       = 0x0001;
//		const ushort WAVE_FORMAT_ADPCM     = 0x0002; // Microsoft corp.
//		const ushort WAVE_FORMAT_DVI_ADPCM = 0x0011; // Intel corp. = WAVE_FORMAT_IMA_ADPCM
		#endregion fields (static)


		#region methods (static)
		/// <summary>
		/// Encodes a file to BMU/MP3 - converts it from PCM-Wave or ADPCM or
		/// even back from a BMU/MP3. Note that input formats/configs are very
		/// limited.
		/// </summary>
		/// <param name="pfe">path_file_extension</param>
		/// <remarks>The result shall be BMU/MP3 44.1kHz 16-bit Mono.</remarks>
		internal static void EncodeVoiceFile(string pfe)
		{
			string pfe0 = pfe;

			string path = Path.GetTempPath();

			if (pfe.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // check .WAV ->
			{
				string pfeT = null;

				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read)) // TODO: Exception handling <-
				using (var br = new BinaryReader(fs))
				{
					char[] c = br.ReadChars(16); // start 0

					if (   c[ 0] == 'R' && c[ 1] == 'I' && c[ 2] == 'F' && c[ 3] == 'F'
						&& c[ 8] == 'W' && c[ 9] == 'A' && c[10] == 'V' && c[11] == 'E'
						&& c[12] == 'f' && c[13] == 'm' && c[14] == 't' && c[15] == ' ')
					{
										 br.ReadBytes(4);	// start 16
						short format   = br.ReadInt16();	// start 20: is PCM

						short channels = br.ReadInt16();	// start 22: is Mono
						int   rate     = br.ReadInt32();	// start 24: is 44.1kHz
										 br.ReadBytes(6);	// start 28
						short bits     = br.ReadInt16();	// start 34: is 16-bit

						if (format == (short)WaveFormatEncoding.Pcm)
						{
							// TODO: Sample-rate and bit-depth should probably be relaxed.
							if (channels == (short)1
								&& rate  == 44100
								&& bits  == (short)16)
							{
								pfeT = pfe;
							}
							else
							{
								error("PCM config shall be 44.1 kHz 16 bits mono [1]");
								return;
							}
						}
						else if (format == (short)WaveFormatEncoding.DviAdpcm) // ADPCM -> windows won't play this natively.
						{
							// TODO: Sample-rate should probably be relaxed.
							if (channels == (short)1
								&& rate == 44100
								&& bits == (short)4)
							{
								pfeT = Path.Combine(path, TEMP_WAV);
								File.Delete(pfeT);

								if (File.Exists(pfeT))
								{
									error("failed to delete the intermediate PCM file [2]");
									return;
								}

								using (var reader = new WaveFileReader(pfe))
								using (var input  =     WaveFormatConversionStream.CreatePcmStream(reader))
								using (var output = new WaveFormatConversionStream(new WaveFormat(44100, input.WaveFormat.Channels), input))
								{
									WaveFileWriter.CreateWaveFile(pfeT, output); // bingo.
								}

								if (!File.Exists(pfeT))
								{
									error("failed to create an intermediate PCM file [3]");
									return;
								}
							}
							else
							{
								error("ADPCM config shall be 44.1 kHz 4 bits mono [4]");
								return;
							}
						}
						else
						{
							error("unsupported Wave format [5]");
							return;
						}
					}
				}

				if (File.Exists(pfeT))
				{
					WritePcmToBmu(pfeT, pfe0);
					return;
				}
			}

			if (   pfe.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase) // prep .BMU ->
				|| pfe.EndsWith(EXT_BMU, StringComparison.InvariantCultureIgnoreCase))
			{
				var chars = new char[3];
				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read)) // TODO: Exception handling <-
				using (var br = new BinaryReader(fs))
				{
					chars = br.ReadChars(3);
				}

				if (   chars[0] == 'B' // because .BMUs are .MP3s and NwN2 labels them as .WAVs
					&& chars[1] == 'M'
					&& chars[2] == 'U')
				{
					string pfeT = Path.Combine(path, TEMP_MP3); // so label it as .MP3 and allow the next block to catch it.
					File.Delete(pfeT);

					if (File.Exists(pfeT))
					{
						error("failed to delete the intermediate MP3 file [6]");
						return;
					}

					File.Copy(pfe, pfeT);

					if (!File.Exists(pfeT))
					{
						error("failed to copy the intermediate MP3 file [7]");
						return;
					}

					pfe = pfeT;
				}
			}

			if (pfe.EndsWith(EXT_MP3, StringComparison.InvariantCultureIgnoreCase)) // convert to .WAV file ->
			{
				string pfeT = Path.Combine(path, TEMP_WAV);
				File.Delete(pfeT);

				if (File.Exists(pfeT))
				{
					error("failed to delete the intermediate PCM file [8]");
					return;
				}

				// Convert MP3 file to WAV using Lame executable
//				var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, LAME_EXE));
//				info.Arguments = "--decode \"" + pfe + "\" \"" + pfeT + "\"";
//				info.WindowStyle = ProcessWindowStyle.Hidden;
//				info.UseShellExecute = false;
//				info.CreateNoWindow  = true;
//				using (Process proc = Process.Start(info))
//					proc.WaitForExit();

				// Convert MP3 file to WAV using NAudio classes only
				// note: My reading indicates this relies on an OS-installed MP3-decoder.
				using (var reader = new Mp3FileReader(pfe))
				using (var input  =     WaveFormatConversionStream.CreatePcmStream(reader))
				using (var output = new WaveFormatConversionStream(new WaveFormat(44100, input.WaveFormat.Channels), input))
				{
					WaveFileWriter.CreateWaveFile(pfeT, output); // bingo.
				}

				if (File.Exists(pfeT))
				{
					WritePcmToBmu(pfeT, pfe0);
				}
				else
					error("failed to create an intermediate PCM file [9]");
			}
		}

		/// <summary>
		/// Takes a PCM sourcefile and writes it to a destination as a BMU with
		/// a WAV extension (per NwN/2 protocol).
		/// </summary>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <remarks>Uses the Lame executable.</remarks>
		static void WritePcmToBmu(string src, string dst)
		{
			string dir  = Path.GetDirectoryName(dst);
			string file = Path.GetFileNameWithoutExtension(dst);

			dst = file;
			if (File.Exists(Path.Combine(dir, dst + EXT_WAV)))
			{
				int i = 0;

				dst = file + "0";
				while (File.Exists(Path.Combine(dir, dst + EXT_WAV)))
					dst = file + (++i);
			}
			dst = Path.Combine(dir, dst + EXT_WAV);

			var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, LAME_EXE));
			info.Arguments = "--cbr -b 96 \"" + src + "\" \"" + dst + "\"";
			info.WindowStyle = ProcessWindowStyle.Hidden;
			info.UseShellExecute = false;
			info.CreateNoWindow  = true;

			using (Process proc = Process.Start(info))
				proc.WaitForExit();


			if (File.Exists(dst)) // add BMU header ->
			{
				byte[] bytes = File.ReadAllBytes(dst);
				if (bytes.Length > 8) // safety.
				{
					var bytesout = new byte[bytes.Length + 8];

					bytesout[0] = (int)'B'; bytesout[1] = (int)'M'; bytesout[2] = (int)'U'; bytesout[3] = (int)' ';
					bytesout[4] = (int)'V'; bytesout[5] = (int)'1'; bytesout[6] = (int)'.'; bytesout[7] = (int)'0';

					for (int i = 8; i != bytesout.Length; ++i)
						bytesout[i] = bytes[i - 8];

					File.WriteAllBytes(dst, bytesout);
				}

				if (File.Exists(dst))
				{
					using (var ib = new Infobox(Infobox.Title_succf,
												"Audiofile written as BMU/MP3 with WAV extension.",
												dst,
												InfoboxType.Success))
					{
						ib.ShowDialog();
					}
					return;
				}
			}

			error("not sure why but you would not see this if it worked");
		}

		/// <summary>
		/// Pops an <c><see cref="Infobox"/></c> if encoding fails.
		/// </summary>
		/// <param name="copyable">error details</param>
		static void error(string copyable)
		{
			using (var ib = new Infobox(Infobox.Title_error,
										"Audiofile encode failed.",
										copyable,
										InfoboxType.Error))
			{
				ib.ShowDialog();
			}
		}


		/// <summary>
		/// Decodes the file to play - converts it from BMU/MP3 or ADPCM to a
		/// PCM-Wave file if necessary.
		/// </summary>
		/// <param name="pfe">path_file_extension</param>
		/// <returns>the fullpath to a PCM-Wave file else <c>null</c></returns>
		/// <remarks>The result shall be PCM-Wave 44.1kHz 16-bit Mono.</remarks>
		internal static string DecodeVoiceFile(string pfe)
		{
			//logfile.Log("AudioConverter.DecodeVoiceFile() pfe= " + pfe);

			string audiofile = null;

			string info_pfe = pfe;

			bool info_BMU = false;
			bool info_MP3 = false;

			Fail fail = Fail.non;

			string path = Path.GetTempPath();

			if (   pfe.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase) // prep .BMU ->
				|| pfe.EndsWith(EXT_BMU, StringComparison.InvariantCultureIgnoreCase))
			{
				var chars = new char[3];
				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read)) // TODO: Exception handling <-
				using (var br = new BinaryReader(fs))
				{
					chars = br.ReadChars(3);
				}

				if (   chars[0] == 'B' // because .BMUs are .MP3s and NwN2 labels them as .WAVs
					&& chars[1] == 'M'
					&& chars[2] == 'U')
				{
					info_BMU = true;

					string pfeT = Path.Combine(path, TEMP_MP3); // so label it as .MP3 and allow the next block to catch it.
					File.Delete(pfeT);

					if (File.Exists(pfeT))
					{
						fail = Fail.Delete_TemporaryMP3;
					}
					else
					{
						File.Copy(pfe, pfeT);

						if (File.Exists(pfeT))
						{
							pfe = pfeT;
						}
						else
							fail = Fail.Create_IntermediateMP3;
					}
				}
			}

			if (fail == Fail.non
				&& pfe.EndsWith(EXT_MP3, StringComparison.InvariantCultureIgnoreCase)) // convert to .WAV file ->
			{
				info_MP3 = !info_BMU;

				string pfeT = Path.Combine(path, TEMP_WAV);
				File.Delete(pfeT);

				if (File.Exists(pfeT))
				{
					fail = Fail.Delete_TemporaryPCM;
				}
				else
				{
					// Convert MP3 file to WAV using Lame executable
//					var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, LAME_EXE));
//					info.Arguments = "--decode \"" + pfe + "\" \"" + pfeT + "\"";
//					info.WindowStyle = ProcessWindowStyle.Hidden;
//					info.UseShellExecute = false;
//					info.CreateNoWindow  = true;
//					using (Process proc = Process.Start(info))
//						proc.WaitForExit();

					// Convert MP3 file to WAV using NAudio classes only
					// note: My reading indicates this relies on an OS-installed MP3-decoder.
					using (var reader = new Mp3FileReader(pfe))
					using (var input  =     WaveFormatConversionStream.CreatePcmStream(reader))
					using (var output = new WaveFormatConversionStream(new WaveFormat(44100, input.WaveFormat.Channels), input))
					{
						WaveFileWriter.CreateWaveFile(pfeT, output); // bingo.
					}

					if (File.Exists(pfeT))
					{
						pfe = pfeT;
					}
					else
						fail = Fail.Create_ConvertedPCM;
				}
			}


			bool info_PCM   = false;
			bool info_ADPCM = false;

			short channels = -1;
			int   rate     = -1;
			short bits     = -1;


			if (fail == Fail.non
				&& pfe.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // check .WAV ->
			{
				//logfile.Log(". pfe= " + pfe);

				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read)) // TODO: Exception handling <-
				using (var br = new BinaryReader(fs))
				{
					char[] c = br.ReadChars(16); // start 0

					if (   c[ 0] == 'R' && c[ 1] == 'I' && c[ 2] == 'F' && c[ 3] == 'F'
						&& c[ 8] == 'W' && c[ 9] == 'A' && c[10] == 'V' && c[11] == 'E'
						&& c[12] == 'f' && c[13] == 'm' && c[14] == 't' && c[15] == ' ')
					{
						short format;

								   br.ReadBytes(4);	// start 16
						format   = br.ReadInt16();	// start 20: is PCM

						channels = br.ReadInt16();	// start 22: is Mono
						rate     = br.ReadInt32();	// start 24: is 44.1kHz
								   br.ReadBytes(6);	// start 28
						bits     = br.ReadInt16();	// start 34: is 16-bit

						//logfile.Log(". . format= " + format);
						//logfile.Log(". . channels= " + channels);
						//logfile.Log(". . rate= " + rate);
						//logfile.Log(". . bits= " + bits);

						if (format == (short)WaveFormatEncoding.Pcm)
						{
							info_PCM = !info_BMU && !info_PCM;

							// TODO: Sample-rate and bit-depth should probably be relaxed.
							if (channels == (short)1
								&& rate  == 44100
								&& bits  == (short)16)
							{
								audiofile = pfe;
							}
							else
								fail = Fail.Config_InvalidPCM;
						}
						else if (format == (short)WaveFormatEncoding.DviAdpcm) // ADPCM -> windows won't play this natively.
						{
							info_ADPCM = true;

							// TODO: Sample-rate should probably be relaxed.
							if (channels == (short)1
								&& rate == 44100
								&& bits == (short)4)
							{
								string pfeT = Path.Combine(path, TEMP_WAV);
								File.Delete(pfeT);

								if (File.Exists(pfeT))
								{
									fail = Fail.Delete_FinalPCM;
								}
								else
								{
									using (var reader = new WaveFileReader(pfe))
									using (var input  =     WaveFormatConversionStream.CreatePcmStream(reader))
									using (var output = new WaveFormatConversionStream(new WaveFormat(44100, input.WaveFormat.Channels), input))
									{
										WaveFileWriter.CreateWaveFile(pfeT, output); // bingo.
									}

									if (File.Exists(pfeT))
									{
										audiofile = pfeT;
									}
									else
										fail = Fail.Create_FinalPCM;
								}
							}
							else
								fail = Fail.Config_InvalidADPCM;
						}
						else
							fail = Fail.Format_InvalidPCM;
					}
				}
			}

			//logfile.Log("audiofile= " + audiofile);
			if (audiofile == null)
			{
				string copyable = "errorcode " + (int)fail + Environment.NewLine + Environment.NewLine
								+ "input" + Environment.NewLine
								+ info_pfe + Environment.NewLine
								+ (info_BMU ? "BMU" : (info_MP3 ? "MP3" : (info_PCM ? "PCM" : (info_ADPCM ? "ADPCM" : "unknown")))) + Environment.NewLine
								+ "channels " + (channels != -1 ? channels.ToString() : "unknown") + Environment.NewLine
								+ "rate " + (rate != -1 ? rate.ToString() : "unknown") + Environment.NewLine
								+ "bits " + (bits != -1 ? bits.ToString() : "unknown");

				using (var ib = new Infobox(Infobox.Title_error,
											"Failed to convert to 44.1kHz 16-bit Mono PCM-wave format.",
											copyable,
											InfoboxType.Error))
				{
					ib.ShowDialog();
				}
			}

			return audiofile;
		}
		#endregion methods (static)
	}
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

/* play audiofile using NAudio
using NAudio;
using NAudio.Wave;

IWavePlayer waveOutDevice = new WaveOut();
AudioFileReader audioFileReader = new AudioFileReader("file.mp3");
waveOutDevice.Init(audioFileReader);
waveOutDevice.Play();

waveOutDevice.Stop();
audioFileReader.Dispose();
waveOutDevice.Dispose();
*/

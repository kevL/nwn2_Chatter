using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using NAudio.Utils;
using NAudio.Wave;

namespace NAudio.FileFormats.Wav
{
	class WaveFileChunkReader
	{
		WaveFormat waveFormat;
		long dataChunkPosition;
		long dataChunkLength;
		List<RiffChunk> riffChunks;
		readonly bool strictMode;
		bool isRf64;
		readonly bool storeAllChunks;
		long riffSize;

		public WaveFileChunkReader()
		{
			storeAllChunks = true;
			strictMode = false;
		}

		public void ReadWaveHeader(Stream stream)
		{
			dataChunkPosition = -1;
			waveFormat = null;
			riffChunks = new List<RiffChunk>();
			dataChunkLength = 0;

			var br = new BinaryReader(stream);
			ReadRiffHeader(br);
			riffSize = br.ReadUInt32(); // read the file size (minus 8 bytes)

			if (br.ReadInt32() != ChunkIdentifier.ChunkIdentifierToInt32("WAVE"))
			{
				throw new FormatException("Not a WAVE file - no WAVE header");
			}

			if (isRf64)
			{
				ReadDs64Chunk(br);
			}

			int dataChunkId = ChunkIdentifier.ChunkIdentifierToInt32("data");
			int formatChunkId = ChunkIdentifier.ChunkIdentifierToInt32("fmt ");
			
			// sometimes a file has more data than is specified after the RIFF header
			long stopPosition = Math.Min(riffSize + 8, stream.Length);

			// this -8 is so we can be sure that there are at least 8 bytes for a chunk id and length
			while (stream.Position <= stopPosition - 8)
			{
				int chunkIdentifier = br.ReadInt32();
				uint chunkLength = br.ReadUInt32();
				if (chunkIdentifier == dataChunkId)
				{
					dataChunkPosition = stream.Position;
					if (!isRf64) // we already know the dataChunkLength if this is an RF64 file
					{
						dataChunkLength = chunkLength;
					}
					stream.Position += chunkLength;
				}
				else if (chunkIdentifier == formatChunkId)
				{
					if (chunkLength > Int32.MaxValue)
						 throw new InvalidDataException(string.Format("Format chunk length must be between 0 and {0}.", Int32.MaxValue));

					waveFormat = WaveFormat.FromFormatChunk(br, (int)chunkLength);
				}
				else
				{
					// check for invalid chunk length
					if (chunkLength > stream.Length - stream.Position)
					{
						if (strictMode)
						{
							Debug.Assert(false, String.Format("Invalid chunk length {0}, pos: {1}. length: {2}",
								chunkLength, stream.Position, stream.Length));
						}
						// an exception will be thrown further down if we haven't got a format and data chunk yet,
						// otherwise we will tolerate this file despite it having corrupt data at the end
						break;
					}
					if (storeAllChunks)
					{
						if (chunkLength > Int32.MaxValue)
							throw new InvalidDataException(string.Format("RiffChunk chunk length must be between 0 and {0}.", Int32.MaxValue));

						riffChunks.Add(GetRiffChunk(stream, chunkIdentifier, (int)chunkLength));
					}
					stream.Position += chunkLength;
				}

				// All Chunks have to be word aligned.
				// https://www.tactilemedia.com/info/MCI_Control_Info.html
				// "If the chunk size is an odd number of bytes, a pad byte with value zero is
				//  written after ckData. Word aligning improves access speed (for chunks resident in memory)
				//  and maintains compatibility with EA IFF. The ckSize value does not include the pad byte."
				if (chunkLength % 2 != 0 && br.PeekChar() == 0)
				{
					++stream.Position;
				}
			}

			if (waveFormat == null)
			{
				throw new FormatException("Invalid WAV file - No fmt chunk found");
			}

			if (dataChunkPosition == -1)
			{
				throw new FormatException("Invalid WAV file - No data chunk found");
			}
		}

		/// <summary>
		/// http://tech.ebu.ch/docs/tech/tech3306-2009.pdf
		/// </summary>
		void ReadDs64Chunk(BinaryReader reader)
		{
			int ds64ChunkId = ChunkIdentifier.ChunkIdentifierToInt32("ds64");
			int chunkId = reader.ReadInt32();
			if (chunkId != ds64ChunkId)
			{
				throw new FormatException("Invalid RF64 WAV file - No ds64 chunk found");
			}
			int chunkSize = reader.ReadInt32();
			riffSize = reader.ReadInt64();
			dataChunkLength = reader.ReadInt64();
			long sampleCount = reader.ReadInt64(); // replaces the value in the fact chunk
			reader.ReadBytes(chunkSize - 24); // get to the end of this chunk (should parse extra stuff later)
		}

		static RiffChunk GetRiffChunk(Stream stream, Int32 chunkIdentifier, Int32 chunkLength)
		{
			return new RiffChunk(chunkIdentifier, chunkLength, stream.Position);
		}

		void ReadRiffHeader(BinaryReader br)
		{
			int header = br.ReadInt32();
			if (header == ChunkIdentifier.ChunkIdentifierToInt32("RF64"))
			{
				isRf64 = true;
			}
			else if (header != ChunkIdentifier.ChunkIdentifierToInt32("RIFF"))
			{
				throw new FormatException("Not a WAVE file - no RIFF header");
			}
		}

		/// <summary>
		/// WaveFormat
		/// </summary>
		public WaveFormat WaveFormat
		{ get { return waveFormat; } }

		/// <summary>
		/// Data Chunk Position
		/// </summary>
		public long DataChunkPosition
		{ get { return dataChunkPosition; } }

		/// <summary>
		/// Data Chunk Length
		/// </summary>
		public long DataChunkLength
		{ get { return dataChunkLength; } }

		/// <summary>
		/// Riff Chunks
		/// </summary>
		public List<RiffChunk> RiffChunks
		{ get { return riffChunks; } }
	}
}

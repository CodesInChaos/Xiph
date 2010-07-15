using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using Xiph.InteropHelpers;
using Xiph.Ogg;
using System.IO;

namespace Xiph.Vorbis
{
	public class VorbisDspState : DisposableStructWrapper<vorbis_dsp_state>
	{
		public VorbisInfo VorbisInfo { get; private set; }

		public VorbisDspState(VorbisInfo vorbisInfo)
		{
			VorbisInfo = vorbisInfo;
			Errors.CheckVorbisError(NativeMethods.vorbis_analysis_init(ref InternalStruct, ref vorbisInfo.InternalStruct));
		}

		public void WriteHeaders(OggStreamState oggStreamState, VorbisComment comment)
		{
			ogg_packet header = new ogg_packet();
			ogg_packet header_comm = new ogg_packet();
			ogg_packet header_code = new ogg_packet();
			OggPage page = new OggPage();

			Errors.CheckVorbisError(NativeMethods.vorbis_analysis_headerout(ref InternalStruct, ref comment.InternalStruct, ref header, ref header_comm, ref header_code));
			Errors.CheckOggError(NativeMethods.ogg_stream_packetin(ref oggStreamState.InternalStruct, ref header)); // automatically placed in its own page
			Errors.CheckOggError(NativeMethods.ogg_stream_packetin(ref oggStreamState.InternalStruct, ref header_comm));
			Errors.CheckOggError(NativeMethods.ogg_stream_packetin(ref oggStreamState.InternalStruct, ref header_code));
		}

		public void Write(float[,] data, int offset, int count)
		{
			if (data.GetLength(0) != VorbisInfo.Channels)
				throw new ArgumentException("Dimension of data does not match the number of channels");

			unsafe
			{
				float** buffer = (float**)NativeMethods.vorbis_analysis_buffer(ref InternalStruct, count);
				for (int channel = 0; channel < data.GetLength(0); channel++)
				{
					for (int i = 0; i < count; i++)
					{
						buffer[channel][i] = data[channel, i + offset];
					}
				}
			}
			NativeMethods.vorbis_analysis_wrote(ref InternalStruct, count);
		}

		public void Write(float[,] data)
		{
			Write(data, 0, data.GetLength(1));
		}

		public bool BlockOut(VorbisBlock block)
		{
			return NativeMethods.vorbis_analysis_blockout(ref InternalStruct, ref block.InternalStruct) == 1;
		}

		public int BitrateFlushPacket(OggPacket packet)
		{
			return NativeMethods.vorbis_bitrate_flushpacket(ref InternalStruct, ref packet.InternalStruct);
		}

		public void Encode(OggStreamState oggStreamState, VorbisBlock block, Stream stream)
		{
			OggPacket packet = new OggPacket();
			OggPage page = new OggPage();
			/* vorbis does some data preanalysis, then divvies up blocks for
			more involved (potentially parallel) processing.  Get a single
			block for encoding now */
			while (BlockOut(block))
			{
				/* analysis, assume we want to use bitrate management */
				block.Analyze();
				block.BitrateAddBlock();

				while (BitrateFlushPacket(packet) != 0)
				{

					/* weld the packet into the bitstream */
					NativeMethods.ogg_stream_packetin(ref oggStreamState.InternalStruct, ref packet.InternalStruct);

					/* write out pages (if any) */
					bool eos = false;
					while (!eos)
					{
						int result = NativeMethods.ogg_stream_pageout(ref oggStreamState.InternalStruct, ref og);
						if (result == 0) break;
						page.Write(stream);

						/* this could be set above, but for illustrative purposes, I do
						   it here (to show that vorbis does know where the stream ends) */

						if (xm.ogg_page_eos(ref page.InternalStruct) != 0)
							eos = true;
					}
				}
			}
		}


		private void Clear()
		{
			NativeMethods.vorbis_dsp_clear(ref InternalStruct);
		}

		protected override void DisposeOverride()
		{
			if (VorbisInfo.IsDisposed)
				throw new InvalidOperationException("Wrong destruction order");
			Clear();
		}
	}
}

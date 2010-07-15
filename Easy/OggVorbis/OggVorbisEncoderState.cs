using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using System.IO;
using xm = Xiph.LowLevel.NativeMethods;
using System.Runtime.InteropServices;
using Xiph.Easy.Ogg;

namespace Xiph.Easy.OggVorbis
{
	[StructLayout(LayoutKind.Sequential)]
	public struct LowLevelState
	{
		public ogg_stream_state os; // take physical pages, weld into a logical stream of packets
		public ogg_page og; // one Ogg bitstream page.  Vorbis packets are inside
		public ogg_packet op; // one raw packet of data for decode

		public vorbis_info vi; // struct that stores all the static vorbis bitstream settings
		public vorbis_comment vc; // struct that stores all the user comments

		public vorbis_dsp_state vd; // central working state for the packet->PCM decoder
		public vorbis_block vb; // local working space for packet->PCM decode

		public ogg_packet header;
		public ogg_packet header_comm;
		public ogg_packet header_code;
	}

	public class OggVorbisEncoderState : AudioEncoderState
	{
		private unsafe LowLevelState* lls;

		internal OggVorbisEncoderState(OggVorbisEncoderSetup setup, Stream stream)
			: base(setup, stream)
		{
			//Marshal.AllocHGlobal

			//GCHandle handle = GCHandle.Alloc(lls, GCHandleType.Pinned);

			if (setup.BitRate == null)
				throw new ArgumentNullException("Bitrate");
			LowLevelInit(setup);
		}

		private byte[] ReadBuffer(IntPtr buffer, int size)
		{
			byte[] result = new byte[size];
			Marshal.Copy(buffer, result, 0, size);
			return result;
		}

		private void WritePage(ref ogg_page page, Stream stream)
		{
			byte[] headerData = ReadBuffer(page.header, page.header_len);
			byte[] bodyData = ReadBuffer(page.body, page.body_len);
			stream.Write(headerData, 0, headerData.Length);
			stream.Write(bodyData, 0, bodyData.Length);
		}

		private void LowLevelInit(OggVorbisEncoderSetup setup)
		{
			unsafe
			{
				lls = (LowLevelState*)Marshal.AllocHGlobal(sizeof(LowLevelState));
				if (lls == null)
					throw new InvalidOperationException("Could not allocate unmanaged memory");
				*lls = new LowLevelState();
				xm.vorbis_info_init(ref lls->vi);
				// choose an encoding mode.
				setup.BitRate(setup, ref lls->vi);

				/* add a comments */
				xm.vorbis_comment_init(ref lls->vc);
				foreach (OggCommentEntry entry in setup.Comments)
				{
					if (entry.Tag != null)
						xm.vorbis_comment_add_tag(ref lls->vc, entry.Tag, entry.Text);
					else
						xm.vorbis_comment_add(ref lls->vc, entry.Text);
				}

				/* set up the analysis state and auxiliary encoding storage */
				xm.vorbis_analysis_init(ref lls->vd, ref lls->vi);
				xm.vorbis_block_init(ref lls->vd, ref lls->vb);

				/* set up our packet->stream encoder */
				/* pick a random serial number; that way we can more likely build
				   chained streams just by concatenation */
				Random random = new Random();
				xm.ogg_stream_init(ref lls->os, random.Next());

				/* Vorbis streams begin with three headers; the initial header (with
				   most of the codec setup parameters) which is mandated by the Ogg
				   bitstream spec.  The second header holds any comment fields.  The
				   third header holds the bitstream codebook.  We merely need to
				   make the headers, then pass them to libvorbis one at a time;
				   libvorbis handles the additional Ogg bitstream constraints */



				xm.vorbis_analysis_headerout(ref lls->vd, ref lls->vc, ref lls->header, ref lls->header_comm, ref lls->header_code);
				xm.ogg_stream_packetin(ref lls->os, ref lls->header); /* automatically placed in its own
                                         page */
				xm.ogg_stream_packetin(ref lls->os, ref lls->header_comm);
				xm.ogg_stream_packetin(ref lls->os, ref lls->header_code);

				/* This ensures the actual
				 * audio data will start on a new page, as per spec
				 */
				while (true)
				{
					int result = xm.ogg_stream_flush(ref lls->os, ref lls->og);
					if (result == 0) break;
					WritePage(ref lls->og, Stream);
				}
			}
		}

		protected override void WriteOverride(float[,] data, int offset, int size)
		{
			unsafe
			{
				// data to encode
				// expose the buffer to submit data

				float** buffer = (float**)xm.vorbis_analysis_buffer(ref lls->vd, size);

				// uninterleave samples
				for (int channel = 0; channel < Channels; channel++)
					for (int i = 0; i < size; i++)
					{
						buffer[channel][i] = data[channel, i];
					}


				// tell the library how much we actually submitted 
				xm.vorbis_analysis_wrote(ref lls->vd, size);

				/* vorbis does some data preanalysis, then divvies up blocks for
				   more involved (potentially parallel) processing.  Get a single
				   block for encoding now */
				while (xm.vorbis_analysis_blockout(ref lls->vd, ref lls->vb) == 1)
				{

					/* analysis, assume we want to use bitrate management */
					xm.vorbis_analysis(ref lls->vb, ref *(ogg_packet*)null);
					xm.vorbis_bitrate_addblock(ref lls->vb);

					while (xm.vorbis_bitrate_flushpacket(ref lls->vd, ref lls->op) != 0)
					{

						/* weld the packet into the bitstream */
						xm.ogg_stream_packetin(ref lls->os, ref lls->op);

						/* write out pages (if any) */
						bool eos = false;
						while (!eos)
						{
							int result = xm.ogg_stream_pageout(ref lls->os, ref lls->og);
							if (result == 0) break;
							WritePage(ref lls->og, Stream);

							/* this could be set above, but for illustrative purposes, I do
							   it here (to show that vorbis does know where the stream ends) */

							if (xm.ogg_page_eos(ref lls->og) != 0)
								eos = true;
						}
					}
				}
			}
		}

		protected override void FinishOverride()
		{
			unsafe
			{
				/* end of file.  this can be done implicitly in the mainline,
				but it's easier to see here in non-clever fashion.
				Tell the library we're at end of stream so that it can handle
				the last frame and mark end of stream in the output properly */
				xm.vorbis_analysis_wrote(ref lls->vd, 0);

				/* clean up and exit.  vorbis_info_clear() must be called last */

				xm.ogg_stream_clear(ref lls->os);
				xm.vorbis_block_clear(ref lls->vb);
				xm.vorbis_dsp_clear(ref lls->vd);
				xm.vorbis_comment_clear(ref lls->vc);
				xm.vorbis_info_clear(ref lls->vi);
				Marshal.FreeHGlobal((IntPtr)lls);

				/* ogg_page and ogg_packet structs always point to storage in
				   libvorbis.  They're never freed or manipulated directly */
			}
		}
	}
}

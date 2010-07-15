using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using Xiph.InteropHelpers;

namespace Xiph.Vorbis
{
	public class VorbisBlock : DisposableStructWrapper<vorbis_block>
	{
		public VorbisDspState VorbisDspState { get; private set; }

		public VorbisBlock(VorbisDspState vorbisDspState)
		{
			VorbisDspState = vorbisDspState;
			Errors.CheckVorbisError(NativeMethods.vorbis_block_init(ref vorbisDspState.InternalStruct, ref InternalStruct));
		}

		private void Clear()
		{
			NativeMethods.vorbis_block_clear(ref InternalStruct);
		}

		public void Analyze()
		{
			unsafe
			{
				Errors.CheckVorbisError(NativeMethods.vorbis_analysis(ref InternalStruct, ref *(ogg_packet*)null));
			}
		}

		public void BitrateAddBlock()
		{
			Errors.CheckVorbisError(NativeMethods.vorbis_bitrate_addblock(ref InternalStruct));
		}

		protected override void DisposeOverride()
		{
			if (VorbisDspState.IsDisposed)
				throw new InvalidOperationException("Wrong destruction order");
			Clear();
		}
	}
}

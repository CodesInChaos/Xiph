using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using Xiph.InteropHelpers;

namespace Xiph.Easy.OggVorbis
{
	public delegate void VorbisBitrateSetting(OggVorbisEncoderSetup setup, ref vorbis_info vorbisInfo);

	public static class VorbisBitrates
	{
		public static VorbisBitrateSetting VariableBitrate(float baseQuality)
		{
			return delegate(OggVorbisEncoderSetup s, ref vorbis_info vi)
				{
					Errors.CheckVorbisError(NativeMethods.vorbis_encode_init_vbr
						(ref vi, s.Channels, s.SampleRate, baseQuality)
						);
				};
		}

		public static VorbisBitrateSetting AverageBitrate(int maxBitrate, int nominalBitrate, int minBitrate)
		{
			return delegate(OggVorbisEncoderSetup s, ref vorbis_info vi)
			{
				Errors.CheckVorbisError(NativeMethods.vorbis_encode_init
					(ref vi, s.Channels, s.SampleRate, maxBitrate, nominalBitrate, minBitrate)
					);
			};
		}

		public static VorbisBitrateSetting AverageBitrate(int nominalBitrate)
		{
			return AverageBitrate(-1, nominalBitrate, -1);
		}
	}
}

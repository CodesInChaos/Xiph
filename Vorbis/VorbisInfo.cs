using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using Xiph.InteropHelpers;

namespace Xiph.Vorbis
{
	public class VorbisInfo : DisposableStructWrapper<vorbis_info>
	{
		public int Channels { get { return InternalStruct.channels; } }

		private void Init()
		{
			NativeMethods.vorbis_info_init(ref InternalStruct);
		}

		public void EncodeInit(int channels, int sampleRate, int max_bitrate, int nominal_bitrate, int min_bitrate)
		{
			Errors.CheckVorbisError(NativeMethods.vorbis_encode_init(ref InternalStruct, channels, sampleRate, max_bitrate, nominal_bitrate, min_bitrate));
		}

		public void EncodeInit(int channels, int sampleRate, int nominal_bitrate)
		{
			EncodeInit(channels, sampleRate, -1, nominal_bitrate, -1);
		}

		public void EncodeInitVbr(int channels, int sampleRate, float baseQuality)
		{
			Errors.CheckVorbisError(NativeMethods.vorbis_encode_init_vbr(ref InternalStruct, channels, sampleRate, baseQuality));
		}

		public void EncodeControl(int number, IntPtr arg)
		{
			Errors.CheckVorbisError(NativeMethods.vorbis_encode_ctl(ref InternalStruct, number, arg));
		}

		public VorbisDspState AnalysisInit()
		{
			return new VorbisDspState(this);
		}

		public VorbisInfo()
		{
			Init();
		}

		private void Clear()
		{
			NativeMethods.vorbis_info_clear(ref InternalStruct);
		}

		protected override void DisposeOverride()
		{
			Clear();
		}
	}
}

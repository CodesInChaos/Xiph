using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xiph.Easy.Ogg;

namespace Xiph.Easy.OggVorbis
{
	public class OggVorbisEncoderSetup : OggEncoderSetup
	{
		public VorbisBitrateSetting BitRate { get; set; }

		protected override AudioEncoderState StartEncodeOverride(Stream outputStream)
		{
			return new OggVorbisEncoderState(this, outputStream);
		}

		public new OggVorbisEncoderState StartEncode(Stream outputStream)
		{
			return (OggVorbisEncoderState)base.StartEncode(outputStream);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Xiph.Easy
{
	public abstract class AudioEncoderSetup
	{
		public int Channels { get; set; }
		public int SampleRate { get; set; }

		protected abstract AudioEncoderState StartEncodeOverride(Stream outputStream);

		public AudioEncoderState StartEncode(Stream outputStream)
		{
			if (outputStream == null)
				throw new ArgumentNullException("outputStream");
			return StartEncodeOverride(outputStream);
		}
	}
}

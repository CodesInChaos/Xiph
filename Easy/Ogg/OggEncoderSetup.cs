using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xiph.Easy.Ogg
{
	public abstract class OggEncoderSetup : AudioEncoderSetup
	{
		public OggComments Comments { get; private set; }
		public OggEncoderSetup()
		{
			Comments = new OggComments();
		}
	}
}

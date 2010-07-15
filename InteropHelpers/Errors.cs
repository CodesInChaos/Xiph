using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xiph.InteropHelpers
{
	internal class Errors
	{
		public static void CheckVorbisError(int error)
		{
			if (error != 0)
				throw new InvalidOperationException("Vorbis Error: " + error.ToString());
		}

		public static void CheckOggError(int error)
		{
			if (error != 0)
				throw new InvalidOperationException("Ogg Error: " + error.ToString());
		}
	}
}

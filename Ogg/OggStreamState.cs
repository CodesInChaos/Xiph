using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using Xiph.InteropHelpers;
using System.IO;

namespace Xiph.Ogg
{
	public class OggStreamState : DisposableStructWrapper<ogg_stream_state>
	{
		public OggStreamState(int serialNo)
		{
			Init(serialNo);
		}

		public OggStreamState()
		{
			// includes GetHashCode() to create different IDs when multiple OggStreamStates
			// are created shortly after each other
			Random random = new Random(Environment.TickCount ^ this.GetHashCode());
			Init(random.Next());
		}

		private void Init(int serialNo)
		{
			Errors.CheckOggError(NativeMethods.ogg_stream_init(ref InternalStruct, serialNo));
		}

		private void Clear()
		{
			NativeMethods.ogg_stream_clear(ref InternalStruct);
		}

		protected override void DisposeOverride()
		{
			Clear();
		}

		public bool Flush(Stream stream)
		{
			OggPage page = new OggPage();
			if (NativeMethods.ogg_stream_flush(ref InternalStruct, ref page.InternalStruct) == 0)
			{
				return false;
			}
			else
			{
				page.Write(stream);
				return true;
			}
		}

		public void FlushAll(Stream stream)
		{
			bool wroteSomething;
			do
			{
				wroteSomething = Flush(stream);
			} while (wroteSomething);
		}
	}
}

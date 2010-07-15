using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xiph.LowLevel;
using Xiph.InteropHelpers;

namespace Xiph.Vorbis
{
	public class VorbisComment:DisposableStructWrapper<vorbis_comment>
	{
		private void Init()
		{
			NativeMethods.vorbis_comment_init(ref InternalStruct);
		}

		public void Add(string comment)
		{
			NativeMethods.vorbis_comment_add(ref InternalStruct, comment);
		}

		public void AddTag(string tag, string contents)
		{
			NativeMethods.vorbis_comment_add_tag(ref InternalStruct, tag, contents);
		}

		private void Clear()
		{
			NativeMethods.vorbis_comment_clear(ref InternalStruct);
		}

		public VorbisComment()
		{
			Init();
		}

		protected override void DisposeOverride()
		{
			Clear();
		}
	}
}

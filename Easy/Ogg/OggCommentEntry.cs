using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xiph.Easy.Ogg
{
	public struct OggCommentEntry
	{
		public string Tag { get; private set; }
		public string Text { get; private set; }

		public OggCommentEntry(string tag, string text)
			: this()
		{
			Tag = tag;
			Text = text;
		}

		public OggCommentEntry(string text)
			: this()
		{
			Tag = null;
			Text = text;
		}
	}
}

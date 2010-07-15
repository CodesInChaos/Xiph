using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xiph.Easy.Ogg
{
	public class OggComments : List<OggCommentEntry>
	{
		public void AddComment(string comment)
		{
			Add(new OggCommentEntry(comment));
		}

		public void AddTag(string tag, string content)
		{
			Add(new OggCommentEntry(tag, content));
		}
	}
}

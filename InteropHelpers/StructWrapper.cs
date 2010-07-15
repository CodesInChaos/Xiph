using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xiph.InteropHelpers
{
	public abstract class StructWrapper<T>
		where T : struct
	{
		public T InternalStruct;
	}
}

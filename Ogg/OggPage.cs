using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xiph.LowLevel;
using System.Runtime.InteropServices;
using Xiph.InteropHelpers;

namespace Xiph.Ogg
{
	public class OggPage : StructWrapper<ogg_page>
	{
		private byte[] ReadBuffer(IntPtr buffer, int size)
		{
			byte[] result = new byte[size];
			Marshal.Copy(buffer, result, 0, size);
			return result;
		}

		public byte[] ReadHeader()
		{
			return ReadBuffer(InternalStruct.header, InternalStruct.header_len);
		}

		public byte[] ReadBody()
		{
			return ReadBuffer(InternalStruct.body, InternalStruct.body_len);
		}

		public void Write(Stream stream)
		{
			byte[] headerData = ReadHeader();
			byte[] bodyData = ReadBody();
			stream.Write(headerData, 0, headerData.Length);
			stream.Write(bodyData, 0, bodyData.Length);
		}
	}
}

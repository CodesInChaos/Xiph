using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Xiph.Easy
{
	public abstract class AudioEncoderState : IDisposable
	{
		public int SampleRate { get; private set; }
		public int Channels { get; private set; }
		public Stream Stream { get; private set; }
		private bool disposed = false;

		protected AudioEncoderState(AudioEncoderSetup setup, Stream stream)
		{
			if (setup == null)
				throw new ArgumentNullException("setup");
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (setup.Channels < 1)
				throw new ArgumentException("Invalid number of channels");
			if (setup.SampleRate <= 0)
				throw new ArgumentException("Invalid sample-rate");

			SampleRate = setup.SampleRate;
			Channels = setup.Channels;
			Stream = stream;
		}

		public void Write(float[,] data)
		{
			Write(data, 0, data.GetLength(1));
		}

		public void Write(float[,] data, int offset, int size)
		{
			if (disposed)
				throw new InvalidOperationException("Cannot Write when finished/disposed");
			if (data.GetLength(0) != Channels)
				throw new ArgumentException("Dimensions of data do not match number of channels");
			if (offset < 0)
				throw new ArgumentException("offset<0");
			if (size < 0)
				throw new ArgumentException("size<0");
			if (offset + size > data.GetLength(1))
				throw new ArgumentException("offset+size too big");
			WriteOverride(data, offset, size);
		}

		public void Finish()
		{
			if (!disposed)
			{
				disposed = true;
				FinishOverride();
			}
		}

		void IDisposable.Dispose()
		{
			Finish();
		}

		protected abstract void WriteOverride(float[,] data, int offset, int size);
		protected abstract void FinishOverride();
	}
}

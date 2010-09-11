using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using native = Xiph.LowLevel.SpeexPreProcessorNative;
using Xiph.LowLevel;
namespace Xiph.Easy.Speex
{
	public class SpeexPreProcessor : IDisposable
	{
		internal IntPtr State { get; private set; }
		public int SamplesPerFrame { get; private set; }
		public int SamplesPerSecond { get; private set; }
		public bool? VoiceActive { get; private set; }

		private void SetControl(SpeexPreProcessorRequests request, int value)
		{
			if ((int)request % 2 == 1)
				throw new ArgumentException("request is not a setter");
			unsafe
			{
				if (native.speex_preprocess_ctl(State, (int)request, &value) != 0)
					throw new InvalidOperationException("speex_preprocess_ctl returned error");
			}
		}

		private int GetControl(SpeexPreProcessorRequests request)
		{
			if ((int)request % 2 == 0)
				throw new ArgumentException("request is not a getter");
			unsafe
			{
				int value = 0;
				if (native.speex_preprocess_ctl(State, (int)request, &value) != 0)
					throw new InvalidOperationException("speex_preprocess_ctl returned error");
				return value;
			}
		}

		public SpeexPreProcessor(int samplesPerSecond, int samplesPerFrame)
		{
			if (samplesPerFrame <= 0)
				throw new ArgumentException("samplesPerFrame must be >0");
			if (samplesPerSecond <= 0)
				throw new ArgumentException("samplesPerSecond must be >0");
			SamplesPerFrame = samplesPerFrame;
			SamplesPerSecond = samplesPerSecond;
			State = native.speex_preprocess_state_init(samplesPerFrame, samplesPerSecond);
		}


		void IDisposable.Dispose()
		{
			Close();
		}

		public void Close()
		{
			if (State != IntPtr.Zero)
			{
				native.speex_preprocess_state_destroy(State);
				State = IntPtr.Zero;
			}
		}

		public void Run(Int16[] data, int offset)
		{
			Update(data, offset, false);
		}

		public void UpdateEstimate(Int16[] data, int offset)
		{
			Update(data, offset, true);
		}

		public void Update(Int16[] data, int offset, bool estimate)
		{
			if (offset < 0)
				throw new ArgumentException("offset<0");
			if (offset + SamplesPerFrame > data.Length)
				throw new ArgumentException("data too small");
			VoiceActive = null;
			int result = 0;
			unsafe
			{
				fixed (Int16* dataPtr = &(data[offset]))
				{
					if (!estimate)
						result = native.speex_preprocess_run(State, dataPtr);
					else
						native.speex_preprocess_estimate_update(State, dataPtr);
				}
			}
			if (VoiceActivityDetection && !estimate)
				VoiceActive = result != 0;
		}

		public bool DeNoise
		{
			get
			{
				return GetControl(SpeexPreProcessorRequests.GET_DENOISE) != 0;
			}
			set
			{
				SetControl(SpeexPreProcessorRequests.SET_DENOISE, Convert.ToInt32(value));
			}
		}

		public bool VoiceActivityDetection
		{
			get
			{
				return GetControl(SpeexPreProcessorRequests.GET_VAD) != 0;
			}
			set
			{
				SetControl(SpeexPreProcessorRequests.SET_VAD, Convert.ToInt32(value));
			}
		}

		public bool AutomaticGainControl
		{
			get
			{
				return GetControl(SpeexPreProcessorRequests.GET_AGC) != 0;
			}
			set
			{
				SetControl(SpeexPreProcessorRequests.SET_AGC, Convert.ToInt32(value));
			}
		}
	}
}

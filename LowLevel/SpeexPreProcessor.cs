using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Xiph.LowLevel
{
	public enum SpeexPreProcessorRequests
	{
		SET_DENOISE = 0,
		GET_DENOISE = 1,
		SET_AGC = 2,
		GET_AGC = 3,
		SET_VAD = 4,
		GET_VAD = 5,
		SET_AGC_LEVEL = 6,
		GET_AGC_LEVEL = 7,
		SET_DEREVERB = 8,
		GET_DEREVERB = 9,
		SET_DEREVERB_LEVEL = 10,
		GET_DEREVERB_LEVEL = 11,
		SET_DEREVERB_DECAY = 12,
		GET_DEREVERB_DECAY = 13,
		SET_PROB_START = 14,
		GET_PROB_START = 15,
		SET_PROB_CONTINUE = 16,
		GET_PROB_CONTINUE = 17,
		SET_NOISE_SUPPRESS = 18,
		GET_NOISE_SUPPRESS = 19,
		SET_ECHO_SUPPRESS = 20,
		GET_ECHO_SUPPRESS = 21,
		SET_ECHO_SUPPRESS_ACTIVE = 22,
		GET_ECHO_SUPPRESS_ACTIVE = 23,
		SET_ECHO_STATE = 24,
		GET_ECHO_STATE = 25,
		SET_AGC_INCREMENT = 26,
		GET_AGC_INCREMENT = 27,
		SET_AGC_DECREMENT = 28,
		GET_AGC_DECREMENT = 29,
		SET_AGC_MAX_GAIN = 30,
		GET_AGC_MAX_GAIN = 31
	}

	public static unsafe class SpeexPreProcessorNative
	{
		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr speex_preprocess_state_init(int frame_size, int sampling_rate);
		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl)]
		public static extern void speex_preprocess_state_destroy(IntPtr st);
		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl)]
		public static extern int speex_preprocess_run(IntPtr st, Int16* x);
		/// <summary>
		/// Preprocess a frame (deprecated, use speex_preprocess_run() instead)
		/// </summary>
		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl)]
		public static extern int speex_preprocess(IntPtr st, Int16* x, Int16* echo);
		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl)]
		public static extern void speex_preprocess_estimate_update(IntPtr st, Int16* x);
		[DllImport("libspeexdsp", CallingConvention = CallingConvention.Cdecl)]
		public static extern int speex_preprocess_ctl(IntPtr st, int request, void* ptr);
	}
}

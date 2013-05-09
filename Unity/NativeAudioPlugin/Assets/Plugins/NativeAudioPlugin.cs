using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace NativeAudio {

	/// <summary>
	/// Native audio delegate.
	/// </summary>
	public delegate void NativeAudioDelegate(uint channels, ref Int16[] buffer);

	/// <summary>
	/// Native audio plugin.
	/// </summary>
	public class NativeAudioPlugin {

#region privates
		// delegate from Native Plugin
		delegate void NativeAudioCallback(uint channels, uint bufferBytes, IntPtr buffer);
		static GCHandle fpHandle; // function pointer
		static Int16[] audio_buffer;
#endregion

#region static public
		/// <summary>
		/// Occurs when on native audio in.
		/// </summary>
		static public event NativeAudioDelegate OnNativeAudioIn;

		/// <summary>
		/// Start Mic Input Streaming
		/// </summary>
		/// <param name="deviceId">Device identifier.</param>
		public static void Start(int deviceId) {

			// set default device if minus
			if(deviceId < 0) {
				deviceId = (int) getDefaultAudioDevice();
			}

			// initialize audio_buffer
			int buffer_length = (int) getBufferFrames() * (int) getAudioInputChannels((uint)deviceId);
			audio_buffer = new Int16[buffer_length];

			// delegate
			NativeAudioCallback onAudioCallback = (uint channels, uint bufferBytes, IntPtr buffer) => {
				Marshal.Copy(buffer, audio_buffer, 0, buffer_length);

				if(OnNativeAudioIn != null) {
					OnNativeAudioIn(channels, ref audio_buffer);
				}
			};

			fpHandle = GCHandle.Alloc(onAudioCallback);
			IntPtr func = Marshal.GetFunctionPointerForDelegate(onAudioCallback);

			// default audio
			startNativeAudio(deviceId, func);
			Debug.Log("started");
		}

		/// <summary>
		/// Start with systm default device
		/// </summary>
		public static void Start() {
			Start(-1); // start with default device
		}

		/// <summary>
		/// Stop audio streaming
		/// </summary>
		public static void Stop() {
			stopNativeAudio();
			Debug.Log("stoped");
			fpHandle.Free();
		}

		/// <summary>
		/// Lists all device.
		/// </summary>
		public static void ListAllDevice() {
			uint count = getAudioDeviceCount();
			Debug.Log("AUDIO DEVICE LIST");
			for(uint i=0; i<count; i++) {
				Debug.Log(string.Format("[{0}] : {1}", i, getAudioDeviceName(i)));
			}
		}
#endregion

#region DllImport
		[DllImport ("UnityNativeAudio")] private static extern int startNativeAudio(int deviceId, IntPtr callback);
		[DllImport ("UnityNativeAudio")] private static extern void stopNativeAudio();
		[DllImport ("UnityNativeAudio")] private static extern string getAudioDeviceName(uint deviceId);
		[DllImport ("UnityNativeAudio")] private static extern uint getAudioDeviceCount();
		[DllImport ("UnityNativeAudio")] private static extern uint getDefaultAudioDevice();
		[DllImport ("UnityNativeAudio")] private static extern uint getAudioInputChannels(uint deviceId);
		[DllImport ("UnityNativeAudio")] private static extern uint getSampleRate();
		[DllImport ("UnityNativeAudio")] private static extern uint getBufferFrames();
#endregion
	}
}
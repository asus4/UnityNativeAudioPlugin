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
	public class NativeAudioPlugin : MonoBehaviour {

#region singleton
		static NativeAudioPlugin instance;

		public static NativeAudioPlugin Instance {
			get {
				if(instance == null) {
					GameObject go = new GameObject(typeof(NativeAudioPlugin).ToString());
					instance = go.AddComponent<NativeAudioPlugin>();
					DontDestroyOnLoad(go);
				}
				return instance;
			}
		}
#endregion

#region privates
		// delegate from Native Plugin
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)] // important!!
		delegate void NativeAudioCallback(uint channels, uint bufferBytes, IntPtr buffer);
		GCHandle fpHandle; // function pointer
		Int16[] audio_buffer;
		int buffer_length;
		static bool isStart = false;

		// delegate
		void OnNativeAudioCallback(uint channels, uint bufferBytes, IntPtr buffer) {
			Debug.Log("a");
//			Marshal.Copy(buffer, audio_buffer, 0, buffer_length);
			if(OnNativeAudioIn != null) {
				//OnNativeAudioIn(channels, ref audio_buffer);
			}
		}
#endregion

#region lyfecycle
		void OnDisable() {
			this.Stop();
		}

		void Update() {
			if(audio_buffer == null) {
				return;
			}

//			Debug.Log("update():"+getBufferFrames());
//			return;
			while(getAudioBuffer(ref audio_buffer[0]) == 0) {
			//Debug.Log("getaudiobuffer:" + getAudioDeviceCount());
				if(OnNativeAudioIn != null) {
					OnNativeAudioIn(2, ref audio_buffer);
				}
			}
		}
#endregion

#region public
		/// <summary>
		/// Occurs when on native audio in.
		/// </summary>
		public event NativeAudioDelegate OnNativeAudioIn;

		/// <summary>
		/// Start Mic Input Streaming
		/// </summary>
		/// <param name="deviceId">Device identifier.</param>
		public void StartAudio(int deviceId) {
			if(isStart) {
				Debug.LogError("Native Audio Plugin : already running.");
				return;
			}

			isStart = true;
			Application.runInBackground = true;

			// set default device if minus
			if(deviceId < 0) {
				deviceId = (int) getDefaultAudioDevice();
			}

			// initialize audio_buffer
			buffer_length = (int) getBufferFrames() * (int) getAudioInputChannels((uint)deviceId);
			audio_buffer = new Int16[buffer_length];

			// delegate
			NativeAudioCallback callback = OnNativeAudioCallback;
			fpHandle = GCHandle.Alloc(callback, GCHandleType.Pinned);
			IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(callback);

			// start audio
			int success = startNativeAudio(deviceId, 2, funcPtr);
			if(success == 0) {
				Debug.Log("NativeAudio started");
			}
			else {
				Debug.LogError("failed start");
			}
		}

		/// <summary>
		/// Start with systm default device
		/// </summary>
		public void StartAudio() {
			StartAudio(-1); // start with default device
		}

		/// <summary>
		/// Stop audio streaming
		/// </summary>
		public void Stop() {
			Debug.Log("stop start");
			stopNativeAudio();
			Debug.Log("stoped");
			fpHandle.Free();
			isStart = false;
		}

		/// <summary>
		/// Lists all device.
		/// </summary>
		public void ListAllDevice() {
			uint count = getAudioDeviceCount();
			Debug.Log("AUDIO DEVICE LIST");
			for(uint i=0; i<count; i++) {
				Debug.Log(string.Format("[{0}] : {1}", i, getAudioDeviceName(i)));
			}
		}
#endregion

#region DllImport
		[DllImport ("UnityNativeAudio")] private static extern int startNativeAudio(int deviceId, uint channels, IntPtr callback);
		[DllImport ("UnityNativeAudio")] private static extern void stopNativeAudio();
		[DllImport ("UnityNativeAudio")] private static extern int getAudioBuffer(ref short buffer);
		[DllImport ("UnityNativeAudio")] private static extern string getAudioDeviceName(uint deviceId);
		[DllImport ("UnityNativeAudio")] private static extern uint getAudioDeviceCount();
		[DllImport ("UnityNativeAudio")] private static extern uint getDefaultAudioDevice();
		[DllImport ("UnityNativeAudio")] private static extern uint getAudioInputChannels(uint deviceId);
		[DllImport ("UnityNativeAudio")] private static extern uint getSampleRate();
		[DllImport ("UnityNativeAudio")] private static extern uint getBufferFrames();
#endregion
	}
}
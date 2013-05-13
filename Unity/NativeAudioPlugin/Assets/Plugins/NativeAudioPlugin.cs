using UnityEngine;
using System;
using System.Threading;
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

#region singleton
		static NativeAudioPlugin instance = new NativeAudioPlugin();
		public static NativeAudioPlugin Instance {
			get {return instance;}
		}

		NativeAudioPlugin() {
			// private
		}

		~NativeAudioPlugin() {
			this.Stop();
		}

#endregion

#region privates
		Int16[] audio_buffer;
		int buffer_length;
		bool isRunning = false;
		volatile uint channels = 0;
		Thread thread;
#endregion

#region lyfecycle

		// thread
		void ThreadLoop() {
			while(isRunning) {
				if (audio_buffer == null) {
						break;
				}
				lock (audio_buffer) {
					while (getAudioBuffer(ref audio_buffer[0]) == 0) {
						if (OnNativeAudioIn != null) {
							OnNativeAudioIn (channels, ref audio_buffer);
						}
					}
				}
				Thread.Sleep(10);
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
		public void Start(int deviceId, uint channels) {
			if(isRunning) {
				Debug.LogError("Native Audio Plugin : already running.");
				return;
			}

			isRunning = true;
			Application.runInBackground = true;

			// set default device if minus
			if(deviceId < 0) {
				deviceId = (int) getDefaultAudioDevice();
			}
			if (channels > getAudioInputChannels((uint)deviceId)) {
				channels = getAudioInputChannels((uint)deviceId);
			}
			this.channels = channels;

			// initialize audio_buffer
			buffer_length = (int) getBufferFrames() * (int) channels;
			audio_buffer = new Int16[buffer_length];

			// start audio
			int success = startNativeAudio(deviceId, channels);
			if(success == 0) {
				Debug.Log("NativeAudio started");
			}
			else {
				Debug.LogError("failed start");
			}

			thread = new Thread(new ThreadStart(ThreadLoop));
			thread.IsBackground = true;
			thread.Start();

			Debug.Log ("Thread started");
		}

		/// <summary>
		/// Start with systm default device
		/// </summary>
		public void Start() {
			Start(-1, 2); // start with default device
		}
		
		/// <summary>
		/// Start with device name and channels.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="channels">Channels.</param>
		public void Start(string name, uint channels) {
			uint count = DeviceCount;
			int deviceID = -1;

			for(int i=0; i<count; i++) {
				if (name == GetDeviceName ((uint)i)) {
					deviceID = i;
					break;
				}
			}
			Start(deviceID, channels);
		}

		/// <summary>
		/// Stop audio streaming
		/// </summary>
		public void Stop() {
			isRunning = false;
			if (thread != null) {
				thread.Abort();
				thread.Join();
				thread = null;
				Debug.Log("thread stoped:" + isRunning);
			}

			stopNativeAudio();
			Debug.Log("stoped");

			audio_buffer = null;
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

		public uint DeviceCount {
			get {
				return getAudioDeviceCount();
			}
		}

		public string GetDeviceName(uint deviceId) {
			return getAudioDeviceName (deviceId);
		}

#endregion

#region DllImport
		[DllImport ("UnityNativeAudio")] private static extern int startNativeAudio(int deviceId, uint channels);
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
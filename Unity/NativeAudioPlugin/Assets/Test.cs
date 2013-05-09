using UnityEngine;
using System;
using System.Collections;
using NativeAudio;

public class Test : MonoBehaviour {

	void Start() {
		NativeAudioPlugin.ListAllDevice();
	}

	void OnEnable() {
		NativeAudioPlugin.Start(0);
		NativeAudioPlugin.OnNativeAudioIn += HandleOnNativeAudioIn;
	}

	void OnDisable() {
		NativeAudioPlugin.OnNativeAudioIn -= HandleOnNativeAudioIn;
		NativeAudioPlugin.Stop();
	}

	void HandleOnNativeAudioIn (uint channels, ref Int16[] buffer)
	{
		int len = buffer.Length;
		long[] channel_volume = new long[channels];

		for(int i=0; i<len; i++) {
			channel_volume[i%channels] += Math.Abs(buffer[i]);
		}

		string log="";
		for(int i=0; i<channels; i++) {
			channel_volume[i] /= (len/channels);
			log += string.Format("[Channel {0} Volume {1}]",i, channel_volume[i]);
		}

		channel_volume = null;

		Debug.Log(log);
	}
}
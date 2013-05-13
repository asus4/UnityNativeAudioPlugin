using UnityEngine;
using System;
using System.Collections;
using NativeAudio;

public class Test : MonoBehaviour {

	void Awake() {
		NativeAudioPlugin.Instance.ListAllDevice();
	}

	void OnEnable() {
//		NativeAudioPlugin.Instance.Start(1,2); // start with device ID
		NativeAudioPlugin.Instance.Start("Apple Inc.: Built-in Microphone", 2); // start with name
	}

	void OnDisable() {
		NativeAudioPlugin.Instance.Stop();
	}

}
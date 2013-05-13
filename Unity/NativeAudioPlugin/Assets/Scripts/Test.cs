using UnityEngine;
using System;
using System.Collections;
using NativeAudio;

public class Test : MonoBehaviour {

	void Awake() {
		NativeAudioPlugin.Instance.ListAllDevice();
	}

	void OnEnable() {
		NativeAudioPlugin.Instance.Start(1,2);
	}

	void OnDisable() {
		NativeAudioPlugin.Instance.Stop();
	}

}
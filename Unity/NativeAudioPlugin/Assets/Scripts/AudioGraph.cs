using UnityEngine;
using System.Collections;
using NativeAudio;

[RequireComponent(typeof(GLGrapher))]
public class AudioGraph : MonoBehaviour {

	public int channel = 0;
	GLGrapher grapher;

	volatile int total = 0;
	volatile int count = 0;

	void Start() {
		grapher = GetComponent<GLGrapher>();
	}

	void OnEnable() {
		NativeAudioPlugin.Instance.OnNativeAudioIn += HandleOnNativeAudioIn;
	}

	void OnDisable() {
		NativeAudioPlugin.Instance.OnNativeAudioIn -= HandleOnNativeAudioIn;
	}

	void Update() {
		if (total != 0) {
			grapher.AddValue (total / 32767.0f / count);
		} else {
			grapher.AddValue (0);
		}
		total = 0;
		count = 0;
	}
	
	void HandleOnNativeAudioIn (uint channels, ref short[] buffer)
	{
		uint i;
		int len=buffer.Length;

		short a;
		for(i = (uint)channel; i<len; i+=channels) {
			a = buffer [i];
			total += a;
			count++;
		}
	}

}

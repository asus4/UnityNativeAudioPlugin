using UnityEngine;
using System.Collections;
using NativeAudio;

[RequireComponent(typeof(GLGrapher))]
public class AudioGraph : MonoBehaviour {

	public int channel = 0;
	GLGrapher grapher;

	void Start() {
		grapher = GetComponent<GLGrapher>();

	}

	void OnEnable() {
		NativeAudioPlugin.Instance.OnNativeAudioIn += HandleOnNativeAudioIn;
	}

	void OnDisable() {
		NativeAudioPlugin.Instance.OnNativeAudioIn -= HandleOnNativeAudioIn;
	}

	
	void HandleOnNativeAudioIn (uint channels, ref short[] buffer)
	{
		uint i;
		int len=buffer.Length;

		for(i=0; i<len; i+=channels) {
			grapher.AddValue(buffer[i] / 32767.0f);
			//grapher.AddValue(r.Next(-32000, 32000));
			//grapher.AddValue((float)r.NextDouble());
		}

		//Debug.Log ("add");

	}

}

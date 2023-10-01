using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public AudioMixer mixer;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	public void SetSoundFx(float sliderValue)
	{
		mixer.SetFloat("SoundFxVolume", Mathf.Log10(sliderValue) * 20);
	}

	public void SetMusic(float sliderValue)
	{
		mixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
	}
}

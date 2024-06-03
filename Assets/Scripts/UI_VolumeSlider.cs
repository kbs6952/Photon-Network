using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_VolumeSlider : MonoBehaviour
{
    public Slider Slider;
    public string parameter;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float multiplier;

    private const string MasterVolumeKey = "MasterVolume";
    private float defaultValue = 0.5f;
    void Start()
    {
        LoadVolume();
    }

    public void SliderValue(float value)
    {
        audioMixer.SetFloat(parameter, Mathf.Log10(value) * multiplier);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public void LoadVolume()
    {
        if(PlayerPrefs.HasKey(MasterVolumeKey))
        {
            float tempVolume = PlayerPrefs.GetFloat(MasterVolumeKey);
            SliderValue(tempVolume);
            Slider.value = tempVolume;
        }
        else        // 데이터가 없을 때 기본 설정 불러오기
        {
            SliderValue(defaultValue);
        }
    }
}

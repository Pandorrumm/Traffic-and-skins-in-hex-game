using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New AudioData", menuName = "AudioData")]
public class AudioData : ScriptableObject
{
    [System.Serializable]
    public class SoundsData
    {
        public string key;
        public AudioMixerGroupType audioMixerGroupType;        
        public AudioClip audioClip;
        public AudioMixerGroup audioMixerGroup;
       // public bool isPlayMoreThanOnce;
       // public int numberOfTimes;

        public enum AudioMixerGroupType
        {
            BACKGROUND,
            SFX,
        }
    }

    public List<SoundsData> soundsData = new List<SoundsData>();

    public SoundsData SearchClipData(string _key)
    {
        foreach (SoundsData soundsData in soundsData)
        {
            if (soundsData.key == _key)
            {
                return soundsData;
            }
        }

        return null;
    }
}

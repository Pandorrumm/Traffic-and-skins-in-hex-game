using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioData audioData = null;
    private AudioSource audioSource;
  //  public UnityEvent onStart;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

       // onStart?.Invoke();
    }

    public void PlayAudio(string _key)
    {
        var audioClip = audioData.SearchClipData(_key);
        audioSource.clip = audioClip.audioClip;
        audioSource.outputAudioMixerGroup = audioClip.audioMixerGroup;

        if (audioSource.clip != null)
        {
            if (audioClip.audioMixerGroupType == AudioData.SoundsData.AudioMixerGroupType.SFX)
            {
                audioSource.loop = false;
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(audioSource.clip);
            }

            if (audioClip.audioMixerGroupType == AudioData.SoundsData.AudioMixerGroupType.BACKGROUND)
            {
                audioSource.loop = true;
                audioSource.pitch = 1f;

                audioSource.DOFade(1, 0.4f);
                audioSource.Play();
            }
        }
    }

    public void VolumeOff(float _volume)
    {
        audioSource.DOFade(_volume, 0.5f);
    }
}

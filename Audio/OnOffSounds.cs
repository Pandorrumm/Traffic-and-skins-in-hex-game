using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using DG.Tweening;
using TMPro;

public class OnOffSounds : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer = null;
    [SerializeField] private string[] exposetName = null;
    [Space]
    [SerializeField] private Image onButton = null;
    [SerializeField] private TextMeshProUGUI onText = null;
    [Space]
    [SerializeField] private Image offButton = null;    
    [SerializeField] private TextMeshProUGUI offText = null;

    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();

        if (!PlayerPrefs.HasKey("Volume" + gameObject.name))
        {
            ChangeAudioMixer(0);
            PlayerPrefs.SetInt("Volume" + gameObject.name, 1);
            toggle.isOn = true;
            ChangeButtonUIData(1, true, 0, false);
        }
        else
        {
            if (PlayerPrefs.GetInt("Volume" + gameObject.name) == 0)
            {
                toggle.isOn = false;
                ChangeButtonUIData(0, false, 1, true);
            }
            else
            {
                toggle.isOn = true;
                ChangeButtonUIData(1, true, 0, false);
            }
        }
    }

    public void VolumeChange(bool isOn)
    {       
        if (!isOn)
        {           
            ChangeAudioMixer(-80);
            PlayerPrefs.SetInt("Volume" + gameObject.name, 0);
            ChangeButtonUIData(0, false, 1, true);
        }
        else
        {           
            ChangeAudioMixer(0);
            PlayerPrefs.SetInt("Volume" + gameObject.name, 1);
            ChangeButtonUIData(1, true, 0, false);
            SoundsAndMusicController.Instance.PlayButtonPressed();
        }
    }

    private void ChangeAudioMixer(int _value)
    {
        for (int i = 0; i < exposetName.Length; i++)
        {
            audioMixer.DOSetFloat(exposetName[i], _value, 0f);
        }
    }

    private void ChangeButtonUIData(int _alphaOnButton, bool _onText, int _alphaOffButton, bool _offText)
    {
        offButton.color = new Color(offButton.color.r, offButton.color.g, offButton.color.b, _alphaOffButton);
        onButton.color = new Color(onButton.color.r, onButton.color.g, onButton.color.b, _alphaOnButton);
        offText.gameObject.SetActive(_offText);
        onText.gameObject.SetActive(_onText);
    }
}

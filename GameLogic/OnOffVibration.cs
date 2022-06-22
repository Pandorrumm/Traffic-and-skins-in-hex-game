using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Vays.Vibrate
{
    public class OnOffVibration : MonoBehaviour
    {
        private Toggle toggle;

        public static Action PlayVibrationEvent;

        [Space]
        [SerializeField] private Image onButton = null;
        [SerializeField] private TextMeshProUGUI onText = null;
        [Space]
        [SerializeField] private Image offButton = null;
        [SerializeField] private TextMeshProUGUI offText = null;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();

            if (!PlayerPrefs.HasKey("Vibrate"))
            {
                PlayerPrefs.SetInt("Vibrate", 1);
                VibrateManager.Instance.IsVibrate = PlayerPrefs.GetInt("Vibrate", 1) == 1;
                toggle.isOn = true;
                ChangeButtonUIData(1, true, 0, false);
            }
            else
            {
                if (PlayerPrefs.GetInt("Vibrate") == 0)
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

        public void VibrationChange(bool isOn)
        {
            if (!isOn)
            {
                PlayerPrefs.SetInt("Vibrate", 0);
                VibrateManager.Instance.IsVibrate = false;
                ChangeButtonUIData(0, false, 1, true);

               // SoundsAndMusicController.Instance.PlayButtonPressed();
            }
            else
            {
                PlayerPrefs.SetInt("Vibrate", 1);
                VibrateManager.Instance.IsVibrate = true;
                ChangeButtonUIData(1, true, 0, false);
                PlayVibrationEvent?.Invoke();
                SoundsAndMusicController.Instance.PlayButtonPressed();
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
}

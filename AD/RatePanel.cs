using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GameLogic;
using GameUI;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class RatePanel : MonoBehaviour
{
    [SerializeField] private GameObject ratePanel = null;
    [Space]
    [SerializeField] private Button closeRateButton = null;
    [SerializeField] private Button rateGameButton = null;
    [Space]
    [SerializeField] private string iosURL = "";
    [SerializeField] private string iosID = "";

    private void Start()
    {
        closeRateButton.onClick.AddListener(() => CloseRatePanel());
        rateGameButton.onClick.AddListener(() => RateGame());
    }

    private void OnEnable()
    {
        UIButtonPopup.OpenRatePanelIOSEvent += OpenRatePanel;
    }

    private void OnDisable()
    {
        UIButtonPopup.OpenRatePanelIOSEvent -= OpenRatePanel;
    }

    private void OpenRatePanel()
    {
        ratePanel.SetActive(true);
    }

    private void RateGame()
    {
#if UNITY_IOS
        if (!Device.RequestStoreReview())
        {
            string url = $"itms-apps://itunes.apple.com/app/id{iosID}?mt=8&action=write-review";
            Application.OpenURL(url);
        }
#endif
        CloseRatePanel();
    }

    private void CloseRatePanel()
    {
        ratePanel.SetActive(false);
        ScreensHelper<UIScreenVictoryPopup>.Show(false);
        PlayerPrefs.SetInt("OpenRateIOS", 1);
    }
}


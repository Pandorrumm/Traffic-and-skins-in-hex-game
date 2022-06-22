using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Lean.Common;
using GameUI;
using System;

public class SkinSelector : MonoBehaviour
{
    [SerializeField] private SkinsData skinsData = null;
    [Space]
    [SerializeField] private Image previewSkinImage = null;
    [Space]
    [SerializeField] private Button nextButton = null;
    [SerializeField] private Button backButton = null;
    [SerializeField] private Button skinSelectButton = null;
    [SerializeField] private Button openPurchaseScreen = null;
    [Space]
    [SerializeField] private GameObject AdIconIOS = null;
    [Space]
    [SerializeField] private Image openClosedSkinImage = null;
    [SerializeField] private Sprite openSkin = null;
    [SerializeField] private Sprite closeSkin = null;
    [Space]
    [SerializeField] private Lean.Localization.LeanLocalizedTextMeshProUGUI titleLocalization = null;
    [Space]
    [SerializeField] private Lean.Localization.LeanLocalizedTextMeshProUGUI unlockSkinLocalization = null;
    [Space]
    [SerializeField] private GameObject[] pointsOn = null;

    private int selectedOption = 0;

    public static Action<int> InstallerSkinEvent;

    private void OnEnable()
    {
        if (!PlayerPrefs.HasKey("SelectedOptionSkin"))
        {
            selectedOption = 0;
        }
        else
        {
            Load();
        }

        UpdateSkin(selectedOption);

#if UNITY_IOS
        AdIconIOS.SetActive(true);
        openClosedSkinImage.gameObject.SetActive(false);
#endif
    }

    private void Awake()
    {
        unlockSkinLocalization.gameObject.SetActive(false);
    }

    private void Start()
    {    
        pointsOn[selectedOption].gameObject.SetActive(true);

        nextButton.onClick.AddListener(() => NextOption());
        backButton.onClick.AddListener(() => BackOption());
        skinSelectButton.onClick.AddListener(() => InstallSkin());
    }

    private void NextOption()
    {
        selectedOption ++;

        if (selectedOption >= skinsData.SkinsPhotoPanelCount)
        {
            selectedOption = 0;
        }

        UpdatePointsIndicator(selectedOption);
        UpdateSkin(selectedOption);

        Save();
    }

    private void BackOption()
    {
        selectedOption--;

        if (selectedOption < 0)
        {
            selectedOption = skinsData.SkinsPhotoPanelCount - 1;
        }

        UpdatePointsIndicator(selectedOption);
        UpdateSkin(selectedOption);

        Save();
    }

    private void UpdateSkin(int _selectedOption)
    {
        SkinsPhotoPanel skinsPhotoPanel = skinsData.GetSkinsPhotoPanel(_selectedOption);

        if (!skinsPhotoPanel.isPurchasingSkin)
        {            
            openPurchaseScreen.gameObject.SetActive(true);
            skinSelectButton.gameObject.SetActive(false);

#if UNITY_ANDROID
            openClosedSkinImage.sprite = closeSkin;
            unlockSkinLocalization.gameObject.SetActive(true);
            unlockSkinLocalization.TranslationName = skinsPhotoPanel.localizationUnlockSkinID;
#endif
        }
        else if (skinsPhotoPanel.isPurchasingSkin)
        {
            openPurchaseScreen.gameObject.SetActive(false);

#if UNITY_ANDROID
            openClosedSkinImage.sprite = openSkin;
            unlockSkinLocalization.gameObject.SetActive(false);
#endif

            if (PlayerPrefs.GetString("CurrentWeather") == skinsPhotoPanel.key)
            {
                skinSelectButton.gameObject.SetActive(false);
            }
            else
            {
                skinSelectButton.gameObject.SetActive(true);
            }
        }

        previewSkinImage.sprite = skinsPhotoPanel.spriteSkin;
        titleLocalization.TranslationName = skinsPhotoPanel.localizationID;       
    }

    private void UpdatePointsIndicator(int _index)
    {
        for (int i = 0; i < pointsOn.Length; i++)
        {
            pointsOn[i].gameObject.SetActive(false);
        }

        pointsOn[_index].gameObject.SetActive(true);
    }

    private void InstallSkin()
    {
#if UNITY_IOS
        AdsManager.ShowRewarded(() => AssignSkin());

        void AssignSkin()
        {
            InstallerSkinEvent(selectedOption);
            skinSelectButton.gameObject.SetActive(false);
        }  
#else
        InstallerSkinEvent(selectedOption);
        skinSelectButton.gameObject.SetActive(false);
#endif
    }

    private void Load()
    {
        selectedOption = PlayerPrefs.GetInt("SelectedOptionSkin");
    }

    private void Save()
    {
        PlayerPrefs.SetInt("SelectedOptionSkin", selectedOption);
    }   
}

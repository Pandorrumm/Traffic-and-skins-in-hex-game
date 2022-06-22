using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstallerOfSelectedSkin : Singleton<InstallerOfSelectedSkin> /*MonoBehaviour*/
{
    [SerializeField] private SkinsData skinsData = null;
    [Space]
    [SerializeField] private Material baseMaterial = null;
    [SerializeField] private Material spikeletMaterial = null;
    [SerializeField] private Material treesDynamicMaterial = null;
    [Space]
    [SerializeField] private string baseTextureID = null;
    [SerializeField] private string spikeleTextureID = null;
    [SerializeField] private string treesDynamicTextureID = null;

    private void OnEnable()
    {
        SkinSelector.InstallerSkinEvent += SetSelectedSkin;
    }

    private void OnDisable()
    {
        SkinSelector.InstallerSkinEvent -= SetSelectedSkin;
    }

    private void Awake()
    {
        if (HasInstance)
        {
            Destroy(gameObject);
        }
        else
        {
            InitInstance();
            DontDestroyOnLoad(gameObject);
        }

#if UNITY_IOS
        for (int i = 1; i < skinsData.skinsPhotoPanels.Count; i++)
        {
            if (!PlayerPrefs.HasKey("OpenApplySkin " + skinsData.skinsPhotoPanels[i].key))
            {
                skinsData.skinsPhotoPanels[i].isPurchasingSkin = true;
                PlayerPrefs.SetInt("OpenApplySkin " + skinsData.skinsPhotoPanels[i].key, 1);
            }
        }
#endif

        for (int i = 1; i < skinsData.skinsPhotoPanels.Count; i++)
        {
            if (PlayerPrefs.GetInt("OpenApplySkin " + skinsData.skinsPhotoPanels[i].key) == 1)
            {
                skinsData.skinsPhotoPanels[i].isPurchasingSkin = true;
            }
        }

        if (!PlayerPrefs.HasKey("FirstGame"))
        {
            for (int i = 1; i < skinsData.skinsPhotoPanels.Count; i++)
            {
                if (!PlayerPrefs.HasKey("OpenApplySkin " + skinsData.skinsPhotoPanels[i].key))
                {
                    skinsData.skinsPhotoPanels[i].isPurchasingSkin = false;
                }
            }

            SetSelectedSkin(0);
            PlayerPrefs.SetInt("FirstGame", 1);
        }
        else
        {
            int index = skinsData.skinsPhotoPanels.FindIndex(x => x.key == PlayerPrefs.GetString("CurrentWeather"));
            SetSelectedSkin(index);
        }
    }

    public void SetSelectedSkin(int _selectedOption)
    {
        SkinsPhotoPanel skinsPhotoPanel = skinsData.GetSkinsPhotoPanel(_selectedOption);

        baseMaterial.SetTexture(baseTextureID, skinsPhotoPanel.baseTexture);
        spikeletMaterial.SetTexture(spikeleTextureID, skinsPhotoPanel.spikeleTexture);
        treesDynamicMaterial.SetTexture(treesDynamicTextureID, skinsPhotoPanel.treesDynamicTexture);

        PlayerPrefs.SetString("CurrentWeather", skinsPhotoPanel.key.ToString());
    }
}

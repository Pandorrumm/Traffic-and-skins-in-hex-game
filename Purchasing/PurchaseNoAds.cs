using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PurchaseNoAds : MonoBehaviour
{
    public List<GameObject> buttonsObj;

    public bool hideInIOS = true;

    private void Start()
    {
        if (PlayerPrefs.GetInt("NoAds", 0) == 1)
        {
            BuyNoAds();
        }

        if (hideInIOS)
        {
#if UNITY_IOS
            for (int i = 0; i < buttonsObj.Count; i++)
                buttonsObj[i].SetActive(false);
#endif
        }
    }

    public void BuyNoAds()
    {
        PlayerPrefs.SetInt("NoAds", 1);
        AdsManager.DisableAds();

        AnalyticsEventer.AddLog("noads");

        for (int i = 0; i < buttonsObj.Count; i++)
            buttonsObj[i].SetActive(false);
    }
}

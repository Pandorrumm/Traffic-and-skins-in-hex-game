using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GameUI;
using UnityEngine.UI;

public class PurchasingManager : MonoBehaviour
{
    [Space]
    [SerializeField] private List<string> keyBonuses = new List<string>();
    [Space]
    [SerializeField] private int numberBonusesAtFirstGame = 0;

    public void AddBonuses(int _number)
    {
        AnalyticsEventer.AddLog("skills");

        foreach (string key in keyBonuses)
        {
            if (!PlayerPrefs.HasKey("Bonus " + key))
            {
                PlayerPrefs.SetInt("Bonus " + key, numberBonusesAtFirstGame);
            }

            int bonusAmount = PlayerPrefs.GetInt("Bonus " + key) + _number;
            PlayerPrefs.SetInt("Bonus " + key, bonusAmount);
        }
    }

    public void AddBonusesNonConsumable(int _number)
    {
        AnalyticsEventer.AddLog("noads+skills");

        if (!PlayerPrefs.HasKey("NoAdsAndAddBonuses"))
        {
            AddBonuses(_number);

            PlayerPrefs.SetInt("NoAdsAndAddBonuses", 1);
        }       
    }
}

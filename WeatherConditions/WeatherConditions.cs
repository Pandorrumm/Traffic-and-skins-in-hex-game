using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherConditions : MonoBehaviour
{
    [SerializeField] private SkinsData skinsData = null;
    [Space]
    [SerializeField] private GameObject rainEffect = null;
    [SerializeField] private GameObject snowEffect = null;

    private string currentWeather;

    private void Awake()
    {
        for (int i = 0; i < skinsData.skinsPhotoPanels.Count; i++)
        {
            if (PlayerPrefs.HasKey("CurrentWeather"))
            {
                currentWeather = PlayerPrefs.GetString("CurrentWeather");

                switch (currentWeather)
                {
                    case "Winter":
                        rainEffect.SetActive(false);
                        snowEffect.SetActive(true);
                        break;
                    case "Autumn":
                        rainEffect.SetActive(true);
                        snowEffect.SetActive(false);
                        break;
                    case "Summer":
                        rainEffect.SetActive(false);
                        snowEffect.SetActive(false);
                        break;
                }
            }
            else
            {
                rainEffect.SetActive(false);
                snowEffect.SetActive(false);
            }
        }          
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowBanner : MonoBehaviour
{
    [SerializeField] private bool isShowBunner = true;    

    private void Start()
    {
        //bool isAvailable = AdsManager.IsRewardedAvailable();

        //if (isAvailable)
        //{
            if (isShowBunner)
            {
                AdsManager.ShowBanner();
            }
            else
            {
                AdsManager.HideBanner();
            }
     //   }            
    }
}

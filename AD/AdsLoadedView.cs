using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameUI;

public class AdsLoadedView : MonoBehaviour
{
    [SerializeField] private float delayCheck = 3f;
    [Space]
    public GameObject availableIcon = null;
    public GameObject notAvailableIcon = null;

    private Coroutine coroutine;

    private void OnEnable()
    {
        UIBonusesPanel.ViewingADSForBonusEvent += StartCheckAvailable;
        BuildingCellDetection.SetAvailableIconStateEvent += SetAvailableIconState;
        CloseGame.SetAvailableIconStateEvent += SetAvailableIconState;
    }

    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }    
        
        UIBonusesPanel.ViewingADSForBonusEvent -= StartCheckAvailable;
        BuildingCellDetection.SetAvailableIconStateEvent -= SetAvailableIconState;
        CloseGame.SetAvailableIconStateEvent -= SetAvailableIconState;
    }

    private void StartCheckAvailable(Image _bonusButtonImage)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).gameObject.GetComponent<Image>() == _bonusButtonImage)
            {
                UpdateIcon();
                coroutine = StartCoroutine(CheckAvailable());
            }
        }
    }

    private IEnumerator CheckAvailable()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(delayCheck);

            UpdateIcon();
        }
    }

    public void StopUpdateIcon()
    {
      //  StopCoroutine(coroutine);
        StopAllCoroutines();
    }

    private void UpdateIcon()
    {
        bool isAvailable = AdsManager.IsRewardedAvailable();
        availableIcon.SetActive(isAvailable);
        notAvailableIcon.SetActive(!isAvailable);
    }

    private void SetAvailableIconState(bool _state)
    {
        if (availableIcon.activeInHierarchy)
        {
            if (!_state)
            {
                availableIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                availableIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            }
        }       
    }
}

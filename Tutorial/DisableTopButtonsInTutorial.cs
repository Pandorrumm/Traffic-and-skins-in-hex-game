using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameUI;

public class DisableTopButtonsInTutorial : MonoBehaviour
{
    [Space]
    [SerializeField] private Image[] topButtonsImages = null;

    private void Start()
    {
        if (Tutorial.Instance.isTutorial)
        {
            ÑhangeTransparencyButton(topButtonsImages, 0.5f);
        }
    }

    private void ÑhangeTransparencyButton(Image[] _images, float _alpha)
    {
        for (int i = 0; i < _images.Length; i++)
        {
            _images[i].color = new Color(_images[i].color.r, _images[i].color.g, _images[i].color.b, _alpha);
        }
    }
}

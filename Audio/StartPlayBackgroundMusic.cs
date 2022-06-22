using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class StartPlayBackgroundMusic : MonoBehaviour
{
    public static Action PlayBackgroundMusicEvent;

    private void Start()
    {
        DOTween.Sequence()
             .AppendInterval(0.2f)
             .AppendCallback(() => PlayBackgroundMusicEvent?.Invoke());

    }
}

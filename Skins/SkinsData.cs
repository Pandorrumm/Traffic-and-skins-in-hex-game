using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkinsData : ScriptableObject
{
    public List<SkinsPhotoPanel> skinsPhotoPanels;

    public int SkinsPhotoPanelCount
    {
        get
        {
            return skinsPhotoPanels.Count;
        }
    }

    public SkinsPhotoPanel GetSkinsPhotoPanel(int index)
    {
        return skinsPhotoPanels[index];
    }
}

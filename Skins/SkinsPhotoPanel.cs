using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SkinsPhotoPanel 
{
    public string key;
    public string localizationID;
    public string localizationUnlockSkinID;
    public Sprite spriteSkin;

    public Texture baseTexture;  
    public Texture spikeleTexture;   
    public Texture treesDynamicTexture;

    public bool isPurchasingSkin = false;
}

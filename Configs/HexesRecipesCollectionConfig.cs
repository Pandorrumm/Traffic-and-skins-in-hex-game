using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace GameData
{
    [CreateAssetMenu(fileName = "HexesRecipesCollection", menuName = "Hex/HexesRecipesCollectionConfig")]
    public class HexesRecipesCollectionConfig : SerializedScriptableObject
    {
        public List<HexesRecipeDataConfig> recipes = new List<HexesRecipeDataConfig>();
    }
}

using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace GameData
{
    [CreateAssetMenu(fileName = "HexesRecipe", menuName = "Hex/HexesRecipeConfig")]
    public class HexesRecipeDataConfig : SerializedScriptableObject
    {
        public HexesRecipeData recipe;
    }
}

using UnityEngine;
using Sirenix.OdinInspector;

namespace GameData
{
    [CreateAssetMenu(fileName = "HexSide", menuName = "Hex/HexSideData")]
    public class HexSideData : SerializedScriptableObject
    {
        public SideType type;
        public string localization;
        public int scores = 1;
        public Color textColor;
        public Material material;

        public enum SideType
        {
            Forest,
            River,
            Field,
            Road,
            Bonus,
            City,
            Sawmill,
            WaterMill,
            Barn,
            Lake,
            Rails,
            Nothing
        }
    }
}
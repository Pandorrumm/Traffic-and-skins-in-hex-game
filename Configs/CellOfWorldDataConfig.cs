using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using static GameEntity.HexOfWorld;

namespace GameData
{
    [CreateAssetMenu(fileName = "HexSettings", menuName = "Hex/WorldHexSettings")]
    public class CellOfWorldDataConfig : SerializedScriptableObject
    {
        public CellOfWorldData data;
    }
}

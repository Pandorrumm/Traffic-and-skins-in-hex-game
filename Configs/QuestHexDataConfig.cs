using UnityEngine;
using Sirenix.OdinInspector;

namespace GameData
{
    [CreateAssetMenu(fileName = "QuestHexConfig", menuName = "Hex/QuestHexConfig")]
    public class QuestHexDataConfig : SerializedScriptableObject
    {
        public HexQuestData questData;
    }
}

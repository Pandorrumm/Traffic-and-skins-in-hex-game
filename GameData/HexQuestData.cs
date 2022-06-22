using UnityEngine;
using static GameData.HexSideData;
using Sirenix.OdinInspector;

namespace GameData
{
    [System.Serializable]
    public class HexQuestData
    {
        public string questID;
        public int requireScore;
        public SideType sideType;
        public int rewardHexesAmount;
        //public Color questColor;
        public Sprite questIcon;

        [ReadOnly]
        public int currentScore;

        public HexQuestData Copy() {
            HexQuestData result = new HexQuestData();

            result.questID = questID;
            result.requireScore = requireScore;
            result.sideType = sideType;
            result.rewardHexesAmount = rewardHexesAmount;
            //result.questColor = questColor;
            result.questIcon = questIcon;

            return result;
        }
    }
}

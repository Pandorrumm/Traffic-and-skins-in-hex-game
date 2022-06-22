using System.Collections.Generic;
using static GameData.HexSideData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameData
{
    [System.Serializable]
    public class GlobalMissionData
    {
        public string missionID;
        public GlobalMissionType missionType;
        [ShowIf("missionType", GlobalMissionType.BuildChain)]
        public List<SideType> sideType;
        [ShowIf("missionType", GlobalMissionType.BuildChain)]
        public List<int> chainLengths;
        [Space]
        [ShowIf("missionType", GlobalMissionType.BuildChain)]
        public bool isMultipleChains = false;
        [ShowIf("missionType", GlobalMissionType.BuildChain)]
        public bool isMultipleChainsWithoutRoads = false;
        [Space]
        [ShowIf("missionType", GlobalMissionType.CompliteQuests)]
        public int questsAmount;
        [ShowIf("missionType", GlobalMissionType.ScorePoints)]
        public int scoresAmount;

        public string questDescription;
        [Space]
        public Sprite missionIcon;
        public Sprite missionIconCompleted;
        [Space]
        public List<CellOfWorldDataConfig> startDeck = new List<CellOfWorldDataConfig>();

        public GlobalMissionData Copy()
        {
            GlobalMissionData result = new GlobalMissionData();

            result.missionID = missionID;
            result.missionType = missionType;
            result.sideType = sideType;
            result.chainLengths = chainLengths;
            result.isMultipleChains = isMultipleChains;
            result.isMultipleChainsWithoutRoads = isMultipleChainsWithoutRoads;
            result.questDescription = questDescription;
            result.missionIcon = missionIcon;
            result.startDeck = startDeck;
            result.questsAmount = questsAmount;
            result.scoresAmount = scoresAmount;
            result.missionIconCompleted = missionIconCompleted;

            return result;
        }
    }
}

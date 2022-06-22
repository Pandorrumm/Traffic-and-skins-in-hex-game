using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static GameData.HexSideData;

namespace GameData{
    [CreateAssetMenu(fileName = "GlobalMissionConfig", menuName = "GlobalMission")]
    public class GlobalMissionConfig : SerializedScriptableObject
    {
        public GlobalMissionData data;
    }

    public enum GlobalMissionType{
        BuildChain,
        CompliteQuests,
        ScorePoints
    }
}
using GameData;
using System.Collections.Generic;
using UnityEngine;

namespace Singleton
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Singletons/GameSettings")]
    public class GameSettings : SingletonScriptableObject<GameSettings>
    {
        public GameStyle currentGameMode = GameStyle.FreeMode;
        public HexSkin hexesSkin = HexSkin.Standart;

        public string globalMissionName = "";
        public List<GlobalMissionData> globalMissionData = new List<GlobalMissionData>();

        public int currentMissionIndex = -1;
        public List<List<GlobalMissionConfig>> allGlobalMapMissions = new List<List<GlobalMissionConfig>>();
        [Space]
        public List<CellOfWorldDataConfig> freeModeHexes = new List<CellOfWorldDataConfig>();
        [Space]
        public int[] levelsWithTutorial;

        public void Init()
        {
            globalMissionData.Clear();

            //TODO: Load game hexes skin
        }

        public void SelectNextMission()
        {
            if (currentMissionIndex <= allGlobalMapMissions.Count)
            {
                currentMissionIndex++;
            }

            globalMissionData.Clear();

            foreach (GlobalMissionConfig config in allGlobalMapMissions[currentMissionIndex]) {
                globalMissionData.Add(config.data.Copy());
            }
        }
    }

    public enum GameStyle
    {
        GlobalMapMission,
        FreeMode
    }

    public enum HexSkin
    {
        Standart,
        City,
        Cyberpank
    }  
}

using System;
using System.Collections.Generic;
using UnityEngine;
using static GameEntity.HexOfWorld;
using Sirenix.OdinInspector;
using static GameData.HexSideData;

//#if UNITY_EDITOR
using UnityEditor;
//#endif

namespace GameData
{
    [ShowOdinSerializedPropertiesInInspector]
    public class CellOfWorldData
    {
        public string nameId;
        public string localizedName;

        public GameObject prefub;
        [HideInInspector]
        public GameObject gameObject;

        public Dictionary<Side, SideDataAndConditions> sidesData = new Dictionary<Side, SideDataAndConditions>();

        [ReadOnly]
        public int currentAngle = 0;
        public int weight = 20;

        public bool isHexWithWeatherParticleEffects;

        internal CellOfWorldData Copy()
        {
            CellOfWorldData newData = new CellOfWorldData();            

            newData.nameId = this.nameId;
            newData.localizedName = this.localizedName;
            newData.prefub = this.prefub;           
            newData.sidesData = new Dictionary<Side, SideDataAndConditions>(this.sidesData);
            newData.currentAngle = this.currentAngle;
            newData.isHexWithWeatherParticleEffects = this.isHexWithWeatherParticleEffects;

            return newData;
        }

        public bool isSideContains(SideType sideType)
        {
            foreach (KeyValuePair<Side, SideDataAndConditions> keyValue in sidesData) {
                if(keyValue.Value != null && keyValue.Value.sideData.type == sideType) {
                    return true;
                }
            }

            return false;
        }
    }

    [System.Serializable]
    public class SideDataAndConditions
    {
        public HexSideData sideData;
        public List<HexSideData> connectors = new List<HexSideData>();
    }
}

using GameData;
using GameEntity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public class OverlayMultiChoice : MonoBehaviour
    {
        public static Action<HexOfWorld> OnHexWasOverlayed;
        private void Awake()
        {
            BuildingCellDetection.OnGhostHexPlaced += OnHexPlaced;
        }

        private void OnDestroy()
        {
            BuildingCellDetection.OnGhostHexPlaced -= OnHexPlaced;
        }

        private void OnHexPlaced(HexOfWorld hex)
        {
            foreach(KeyValuePair<HexOfWorld.Side, SideDataAndConditions> keyValue in hex.data.sidesData) {
                if(keyValue.Value.sideData.type == HexSideData.SideType.Bonus) {

                }
            }
        }
    }
}

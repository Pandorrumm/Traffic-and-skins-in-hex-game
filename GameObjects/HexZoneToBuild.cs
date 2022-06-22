using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputSystem;
using Helpers;
using static GameEntity.HexOfWorld;

namespace GameEntity {
    public class HexZoneToBuild : MonoBehaviour, IInteractble
    {
        private GameObject _go;
        public GameObject go {
            get { return gameObject; }
            set { _go = value; }
        }

        public HexOfWorld parentCell;
        public Side side;
    }
}
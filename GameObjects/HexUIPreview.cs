using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputSystem;
using GameData;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;
using Helpers;

namespace GameEntity
{
    public class HexUIPreview : MonoBehaviour, IInteractble
    {
        [SerializeField]
        private GameObject _go;
        public GameObject go {
            get {
                return _go;
            }
            set {
                _go = value;
            }
        }

        [SerializeField]
        private List<MaterialColor> materials = new List<MaterialColor>();
        [ReadOnly, SerializeField]
        private HexModel hexModel;
        public Transform modelContainer;

        [ReadOnly]
        public CellOfWorldData data;

        public int index;

        public static Action<int> OnSelected;

        private void Awake()
        {
            //Init();

            BuildingCellDetection.OnHexUIPreviewSelected += HexUIPreviewSelected;
        }

        private void HexUIPreviewSelected(HexUIPreview hexPreview)
        {
            if (hexPreview == this) {
                OnSelected(index);
            }
        }

        private void OnDestroy()
        {
            BuildingCellDetection.OnHexUIPreviewSelected -= HexUIPreviewSelected;
        }

        private void Init()
        {
            modelContainer.transform.Clear();

            hexModel = Instantiate(data.prefub, modelContainer).GetComponent<HexModel>();

            if (data != null && hexModel != null) {
                hexModel.UpdateSides(data);
            }

            UpdateMaterials();
        }

        private void UpdateMaterials()
        {
            MaterialColor matColor;

            for (int i = 0; i < hexModel.meshRenderer.materials.Length; i++) {
                matColor = new MaterialColor();
                matColor.mat = hexModel.meshRenderer.materials[i];
                matColor.startColor = hexModel.meshRenderer.materials[i].color;

                materials.Add(matColor);
            }
        }

        public void ChangeModel(CellOfWorldData data, Quaternion rotation)
        {
            UpdateData(data);

            if (hexModel != null) {
                hexModel.UpdateSides(data);
            } else {
                GameObject model = Instantiate(data.prefub, modelContainer);
                hexModel = model.GetComponent<HexModel>();
                hexModel.UpdateSides(data);
            }

            materials.Clear();

            UpdateMaterials();

            hexModel.transform.rotation = rotation;
        }

        public void UpdateData(CellOfWorldData data)
        {
            this.data = data.Copy();
        }

        public void MoveToPosition(Vector3 position, float time = 1f)
        {
            transform.DOMove(position, time);
        }

        public void Select()
        {
            if(OnSelected != null) {
                OnSelected(index);
            }
        }
    }
}
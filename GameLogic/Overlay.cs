using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameData;
using System.Linq;
using GameEntity;
using System;
using System.Collections;
using Sirenix.OdinInspector;
using GameUI;

namespace GameLogic
{
    public class Overlay : SerializedMonoBehaviour
    {
        public Toggle isLayout;
        public HexesRecipesCollectionConfig allRecipes;

        public static Action<HexOfWorld> OnHexWasOverlayed;

        // Dictionary<hexA.nameId, Dictionary<hexB.nameId, result>>
        [ReadOnly]
        public Dictionary<string, Dictionary<string, CellOfWorldData>> cashedRecipes = 
            new Dictionary<string, Dictionary<string, CellOfWorldData>>();

        private void Awake()
        {
            InitCashedRecipes();

            BuildingCellDetection.OnHexDropedOverOtherHex += OnHexesColides;
        }

        private void InitCashedRecipes()
        {
            cashedRecipes = new Dictionary<string, Dictionary<string, CellOfWorldData>>();

            foreach (HexesRecipeDataConfig dataConfig in allRecipes.recipes) {
                if (cashedRecipes.ContainsKey(dataConfig.recipe.hexA.data.nameId)) {
                    cashedRecipes[dataConfig.recipe.hexA.data.nameId][dataConfig.recipe.hexB.data.nameId] =
                        dataConfig.recipe.hexResult.data;
                } else {
                    Dictionary<string, CellOfWorldData> hexBAndResult = new Dictionary<string, CellOfWorldData>();
                    hexBAndResult.Add(dataConfig.recipe.hexB.data.nameId, dataConfig.recipe.hexResult.data);

                    cashedRecipes.Add(dataConfig.recipe.hexA.data.nameId, hexBAndResult);
                }
            }
        }

        private void OnDestroy()
        {
            BuildingCellDetection.OnHexDropedOverOtherHex -= OnHexesColides;
        }

        private void OnHexesColides(CellOfWorldData hexOnField, CellOfWorldData hexDragAndDrop)
        {
            if (isLayout.isOn) {
                CellOfWorldData recipeResult = getResult(hexOnField, hexDragAndDrop);
                if (recipeResult != null &&
                    hexOnField.gameObject != null &&
                    hexOnField.gameObject.GetComponent<HexOfWorld>() != null) {
                    HexOfWorld hexOnBoard = hexOnField.gameObject.GetComponent<HexOfWorld>();

                    hexOnBoard.ChangeModel(recipeResult, hexOnField.gameObject.transform.rotation);

                    UIDragAndDropZone drugAndDropZone = FindObjectOfType<UIDragAndDropZone>();

                    if (hexOnBoard.isContainsSide(HexSideData.SideType.River) ||
                        hexOnBoard.isContainsSide(HexSideData.SideType.Road) ||
                        hexOnBoard.isContainsSide(HexSideData.SideType.Rails)) {
                        hexOnBoard.SetDataAngle(hexOnBoard.currentDataAngle, hexOnBoard.data);
                    } else if (drugAndDropZone != null &&
                               drugAndDropZone.currentCellPreview.isContainsSide(HexSideData.SideType.River) ||
                               drugAndDropZone.currentCellPreview.isContainsSide(HexSideData.SideType.Road) ||
                               drugAndDropZone.currentCellPreview.isContainsSide(HexSideData.SideType.Rails)) {
                        Debug.Log("currentAngle: " + drugAndDropZone.currentCellPreview.data.currentAngle);

                        hexOnBoard.SetDataAngle(drugAndDropZone.currentCellPreview.data.currentAngle, hexOnBoard.data);
                    }

                    StartCoroutine(cr_HexWasOverlayed(hexOnBoard));
                }
            }
        }

        private IEnumerator cr_HexWasOverlayed(HexOfWorld hexOnField)
        {
            yield return new WaitForFixedUpdate();

            if (OnHexWasOverlayed != null) {
                OnHexWasOverlayed(hexOnField);
            }
        }

        public CellOfWorldData getResult(CellOfWorldData a, CellOfWorldData b)
        {
            List<HexesRecipeDataConfig> results = allRecipes.recipes.Where(recipe => recipe.recipe.hexA.data == a && 
                                                                   recipe.recipe.hexB.data == b).ToList();

            if (cashedRecipes.ContainsKey(a.nameId)) {
                if (cashedRecipes[a.nameId].ContainsKey(b.nameId)) {
                    return cashedRecipes[a.nameId][b.nameId];
                }
            }

            if (cashedRecipes.ContainsKey(b.nameId)) {
                if (cashedRecipes[b.nameId].ContainsKey(a.nameId)) {
                    return cashedRecipes[b.nameId][a.nameId];
                }
            }

            return null;
        }
    }
}
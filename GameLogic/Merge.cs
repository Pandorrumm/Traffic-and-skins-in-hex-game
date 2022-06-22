using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLogic;
using GameEntity;
using GameData;
using static GameData.HexSideData;
using System;
using UnityEngine.UI;
using DG.Tweening;

namespace GameLogic
{
    public class Merge : MonoBehaviour
    {
        private ChainDetection chainDetection;

        public HexOfWorld hexPrefub;
        public CellOfWorldDataConfig bonusConfig;

        public static Action<HexOfWorld> OnHexDestroyed;
        public static Action<HexOfWorld> OnHexCreated;

        public Toggle toggleMerge;

        private void Awake()
        {
            chainDetection = GetComponent<ChainDetection>();

            BuildingCellDetection.OnNewHexAddedToBoard += OnChainsChanged;
        }

        private void OnDestroy()
        {
            BuildingCellDetection.OnNewHexAddedToBoard -= OnChainsChanged;
        }

        private void OnChainsChanged(HexOfWorld hex)
        {
            if (toggleMerge.isOn) {
                StartCoroutine(cr_merge());
            }
        }

        private IEnumerator cr_merge()
        {
            yield return new WaitForFixedUpdate();

            foreach (KeyValuePair<SideType, List<Chain>> keyValue in chainDetection.allChains) {
                if (keyValue.Value != null) {
                    foreach (Chain chain in keyValue.Value.ToArray()) {
                        if (chain.chainType != SideType.Bonus && chain.chain.Count >= 3) {
                            // Записать текущую спаун позицию
                            HexOfWorld lastChainElement = chain.chain[0];

                            Vector3 spawnPosition = lastChainElement.transform.position;
                            Quaternion spawnRotation = lastChainElement.transform.rotation;

                            // Сместить все хексы в позицию последнего хекса в цепи
                            foreach (HexOfWorld hex in chain.chain.ToArray()) {
                                if (hex != chain.chain[0]) {
                                    hex.transform.DOMove(chain.chain[0].transform.position, 0.2f);
                                }
                            }

                            yield return new WaitForSeconds(0.2f);

                            // Уничтожиить все хексы цепи
                            foreach (HexOfWorld hex in chain.chain.ToArray()) {
                                if (OnHexDestroyed != null) {
                                    OnHexDestroyed(hex);
                                }

                                Dictionary<HexOfWorld.Side, HexOfWorld> neightbours =
                                    new Dictionary<HexOfWorld.Side, HexOfWorld>(hex.neightbours);

                                Destroy(hex.gameObject);

                                yield return new WaitForFixedUpdate();

                                foreach (KeyValuePair<HexOfWorld.Side, HexOfWorld> keyValueNeightbours in neightbours) {
                                    if (keyValueNeightbours.Value != null) {
                                        keyValueNeightbours.Value.UpdateNeightbours();
                                    }
                                }
                            }

                            // Создать хекс из префаба
                            HexOfWorld bonusHex = Instantiate(hexPrefub.gameObject, spawnPosition, spawnRotation).GetComponent<HexOfWorld>();
                            bonusHex.ChangeModel(bonusConfig.data, BuildingCellDetection.lastCreatedCellRotation);
                            bonusHex.gameObject.name = "BonusHex " + UnityEngine.Random.Range(0, 1000000);

                            yield return new WaitForFixedUpdate();

                            bonusHex.SetDataAngle(bonusHex.currentDataAngle, bonusConfig.data);

                            if (OnHexCreated != null) {
                                OnHexCreated(bonusHex);
                            }
                        }
                    }
                }
            }
        }
    }
}
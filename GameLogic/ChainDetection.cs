using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using GameEntity;
using static GameData.HexSideData;
using GameData;
using GameUI;
using System;
using Singleton;
using DG.Tweening;

namespace GameLogic
{
    public class ChainDetection : SerializedMonoBehaviour
    {
        public Dictionary<SideType, List<Chain>> allChains = new Dictionary<SideType, List<Chain>>();

        public static Action OnChainsChanged;
        public static Action<List<Chain>, HexOfWorld> OnChainGrow;

        private List<Chain> sideTwoChains = new List<Chain>();

        [Space]
        [Header("Transport")]
        [SerializeField] private GameObject boatPrefab;       
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private GameObject trainPrefab;
        [Space]
        public List<GameObject> boats = new List<GameObject>();
        public List<GameObject> cars = new List<GameObject>();
        public List<GameObject> trains = new List<GameObject>();

        public static Action<HexOfWorld> RepeatInstallationHexEvent;
        public static Action<HexOfWorld> RepeatInstallationLastHexEvent;
        public static Action<HexOfWorld> HexThatCreatesTransportEvent;
        public static Action<GameObject, HexOfWorld, SideType> MoveObjectEvent;
        public static Action<HexOfWorld> RemovingHexPositionFromWayPointEvent;

       [Space]
        private List<Chain> lastChangedChains = new List<Chain>();
        public List<HexOfWorld> chainsCopy = new List<HexOfWorld>();

        private void Awake()
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                allChains.Add(sideType, new List<Chain>());
            }

            BuildingCellDetection.OnGhostHexPlaced += OnGhostHexPlacedOnBoard;
            UIDragAndDropZone.OnFinishRotate += OnFinishRotationOfHex;
            BuildingCellDetection.OnCurrentHexFinishRotation += OnFinishRotationOfHex;
            UIBonusHex.OnSelectHex += OnGhostHexPlacedOnBoard;
            BuildingCellDetection.OnCurrentHexDestroyed += RemoveHexFromChains;
            BuildingCellDetection.OnNewHexAddedToBoard += NewHexAddedToBoard;

            Merge.OnHexCreated += OnGhostHexPlacedOnBoard;
            Merge.OnHexDestroyed += RemoveHexFromChains;

            Overlay.OnHexWasOverlayed += OnGhostHexPlacedOnBoard;

            UIBonusesPanel.OnHexesWasMoved += OnHexedWasMoved;
            UIBonusesPanel.RemoveHexEvent += RemoveHexFromChains;
            UIBonusesPanel.FindRemovingHexPositionFromWayPointEvent += FindRemovingHexPositionFromWayPoint;

            BuildingCellDetection.CheckingForPresenceOfChainEvent += CheckingForPresenceOfChain;
            BuildingCellDetection.СheckLevel1CompletedEvent += CheckingForPresenceOfChain;

            SaveLoadFreeGame.CheckingСhainNewHexEvent += UpdateChains;

            BuildingCellDetection.DefiningChainForTransportEvent += DefiningChainForTransport;
        }

        private void NewHexAddedToBoard(HexOfWorld hex)
        {
            if (lastChangedChains != null)
            {
                if (OnChainGrow != null)
                {
                    OnChainGrow(lastChangedChains, hex);
                }          
            }
        }

        private void OnHexedWasMoved(HexOfWorld hex1, HexOfWorld hex2)
        {
            RemoveHexFromChains(hex1);
            UpdateChains(hex1);

            RemoveHexFromChains(hex2);
            UpdateChains(hex2);
        }

        private void OnDestroy()
        {
            BuildingCellDetection.OnGhostHexPlaced -= OnGhostHexPlacedOnBoard;
            UIDragAndDropZone.OnFinishRotate -= OnFinishRotationOfHex;
            BuildingCellDetection.OnCurrentHexFinishRotation -= OnFinishRotationOfHex;
            BuildingCellDetection.OnCurrentHexDestroyed -= RemoveHexFromChains;
            UIBonusHex.OnSelectHex -= OnFinishRotationOfHex;
            BuildingCellDetection.OnNewHexAddedToBoard -= NewHexAddedToBoard;

            Merge.OnHexCreated -= OnGhostHexPlacedOnBoard;
            Merge.OnHexDestroyed -= RemoveHexFromChains;

            Overlay.OnHexWasOverlayed -= OnGhostHexPlacedOnBoard;
            UIBonusesPanel.OnHexesWasMoved -= OnHexedWasMoved;

            UIBonusesPanel.RemoveHexEvent -= RemoveHexFromChains;
            UIBonusesPanel.FindRemovingHexPositionFromWayPointEvent -= FindRemovingHexPositionFromWayPoint;

            BuildingCellDetection.CheckingForPresenceOfChainEvent -= CheckingForPresenceOfChain;
            BuildingCellDetection.СheckLevel1CompletedEvent -= CheckingForPresenceOfChain;

            SaveLoadFreeGame.CheckingСhainNewHexEvent -= UpdateChains;

            BuildingCellDetection.DefiningChainForTransportEvent -= DefiningChainForTransport;
        }

        private void OnFinishRotationOfHex(HexOfWorld rotatedHex)
        {
            RemoveHexFromChains(rotatedHex);
            UpdateChains(rotatedHex);
        }

        private void OnGhostHexPlacedOnBoard(HexOfWorld placedHex)
        {
            UpdateChains(placedHex);
        }

        private void UpdateChains(HexOfWorld changedHex)
        {
            if (changedHex != null)
            {
                lastChangedChains.Clear();

                foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
                {
                    List<Chain> chains;
                    allChains.TryGetValue(sideType, out chains);

                    if (chains != null)
                    {
                        UpdateChainData(ref chains, ref changedHex, sideType);

                        foreach (Chain chain in chains)
                        {
                            if (chain.chainType == SideType.River || chain.chainType == SideType.Road|| chain.chainType == SideType.Rails)
                            {
                                if (chain.chain.Count > 1)
                                {
                                    chain.isChainWithMovingTransport = true;
                                }
                                else
                                {
                                    chain.isChainWithMovingTransport = false;
                                }
                            }
                        }                       
                    }
                }

                if (OnChainsChanged != null)
                {
                    OnChainsChanged();
                }
            }
        }

        public void CheckingForPresenceOfChain(HexOfWorld _hex, SideType _sideType, int _numberRemainingHexes)
        {
            foreach (KeyValuePair<SideType, List<Chain>> sideType in allChains)
            {
                if (sideType.Key == _sideType)
                {
                    if (sideType.Value.Count == 0)
                    {
                        RepeatInstallationHexEvent?.Invoke(_hex);
                    }
                    else
                    {
                        foreach (Chain currentChain in sideType.Value)
                        {
                            if (currentChain.chain.Count == 2)
                            {
                                if (GameSettings.Instance.currentMissionIndex == 0)
                                {
                                    Tutorial.Instance.NextScreen();
                                }

                                if (GameSettings.Instance.currentMissionIndex == 1)
                                {
                                    if (_numberRemainingHexes > 0)
                                    {
                                        if (Tutorial.Instance.currentScreen == Tutorial.Instance.screenIndexBeforeMistakeInLevel2_1)
                                        {
                                            Tutorial.Instance.currentScreen++;
                                        }

                                        Tutorial.Instance.NextScreen();
                                    }

                                    if (_numberRemainingHexes == 0)
                                    {
                                        RepeatInstallationHexEvent?.Invoke(_hex);
                                    }
                                }

                                if (GameSettings.Instance.currentMissionIndex == Tutorial.Instance.indexFirstLevelWithRiver)
                                {
                                    if (_numberRemainingHexes > 1)
                                    {
                                        Tutorial.Instance.NextScreen();
                                    }
                                    else if (_numberRemainingHexes == 1)
                                    {
                                        if (Tutorial.Instance.currentScreen == Tutorial.Instance.screenIndexBeforeMistakeInLevel9_1)
                                        {
                                            Tutorial.Instance.currentScreen++;
                                        }

                                        Tutorial.Instance.NextScreen();
                                    }
                                    else if (_numberRemainingHexes == 0)
                                    {
                                        RepeatInstallationHexEvent?.Invoke(_hex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    
        private void RemoveHexFromChains(HexOfWorld destroyedHex)
        {
            if (destroyedHex != null)
            {
                foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
                {
                    List<Chain> chains;
                    allChains.TryGetValue(sideType, out chains);

                    if (chains != null)
                    {
                        RemoveHexFromChainCollection(ref chains, ref destroyedHex);
                    }
                }
            }
        }

        private void RemoveHexFromChainCollection(ref List<Chain> chainCollection, ref HexOfWorld destroyedHex)
        {
            foreach (Chain chain in chainCollection.ToArray())
            {
                if (chain.isChainContainsHex(destroyedHex))
                {
                    if (Tutorial.Instance.isTutorial)
                    {
                        chain.chain.Remove(destroyedHex);

                        if (chain.chain.Count <= 1)
                        {
                            chainCollection.Remove(chain);
                        }
                    }
                    else
                    {
                        chain.chain.Remove(destroyedHex);

                        chainsCopy.AddRange(chain.chain.ToArray());
                        chain.chain.Clear();

                        DOTween.Sequence()
                            .AppendInterval(0.1f)
                            .AppendCallback(() => CheckingForChainRemainingHexes());

                        void CheckingForChainRemainingHexes()
                        {
                            for (int i = 0; i < chainsCopy.Count; i++)
                            {
                                UpdateChains(chainsCopy[i]);
                                chainsCopy[i].UpdateQuestText();
                                chainsCopy.Remove(chainsCopy[i]);
                            }                         
                        }
                    }
                }
            }
        }

        private void UpdateChainData(ref List<Chain> chains, ref HexOfWorld placedHex, SideType sideType)
        {
            Chain chainResult = new Chain();

            List<HexOfWorld> alreadyIndexed = new List<HexOfWorld>();

            Chain finalChain = FindChain(placedHex, sideType, ref chainResult, ref alreadyIndexed);

            int chaninIndex = getChainIndexThatContainsChain(ref chains, ref finalChain);          

            if (finalChain.chain.Count > 1)
            {           
                if (chaninIndex >= 0)
                {
                    if (chains[chaninIndex].isChainWithMovingTransport)
                    {
                        finalChain.isChainWithMovingTransport = true;
                    }

                    if (chains[chaninIndex].isTransportIsCreatedForChain)
                    {
                        finalChain.isTransportIsCreatedForChain = true;
                        //if (finalChain.chain.Count < chains[chaninIndex].chain.Count)
                        //{
                        //    finalChain.isTransportIsCreatedForChain = false;
                        //}
                        //else
                        //{
                        //    finalChain.isTransportIsCreatedForChain = true;
                        //}                      
                    }

                    finalChain.chainType = sideType;
                    chains[chaninIndex] = finalChain;

                    lastChangedChains.Add(finalChain);                   
                }
                else
                {
                    finalChain.chainType = sideType;
                    chains.Add(finalChain);

                    lastChangedChains.Add(finalChain);
                }             
            }          
        }

        private void DefiningChainForTransport(HexOfWorld _hex)
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                List<Chain> chains;
                allChains.TryGetValue(sideType, out chains);

                if (chains != null)
                {
                    for (int i = 0; i < chains.Count; i++)
                    {
                        if (chains[i].isChainContainsHex(_hex))
                            {                     

                            switch (chains[i].chainType)
                            {
                                case SideType.River:

                                    if (chains[i].isChainWithMovingTransport/* == false*/)
                                    {
                                        if (/*!chains[i].isTransportIsCreatedForChain && */chains[i].chain.Count == 2)
                                        {
                                            //Debug.Log("Создать лодку!");

                                            GameObject boat = Instantiate(boatPrefab, _hex.GetComponentInChildren<WayPoint>().transform.position, Quaternion.identity);
                                            
                                            boats.Add(boat);

                                            boat.name = boats.Count + " Boat";

                                            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
                                            {
                                                PlayerPrefs.SetInt("NumberBoats", boats.Count);
                                            }
                                           

                                            if (chains[i].chain[0] == _hex)
                                            {
                                                MoveObjectEvent?.Invoke(boat, chains[i].chain[1], SideType.River);
                                            }
                                            else
                                            {
                                                MoveObjectEvent?.Invoke(boat, chains[i].chain[0], SideType.River);
                                            }

                                            MoveObjectEvent?.Invoke(boat, _hex, SideType.River);

                                            // chains[i].isChainWithMovingTransport = true;

                                          //  chains[i].isTransportIsCreatedForChain = true;
                                        }
                                        else
                                        {
                                            foreach (GameObject needBoat in boats)
                                            {
                                                if (chains[i].chain[1] != _hex)
                                                {
                                                    CheckHexInChain(1);
                                                }
                                                else
                                                {
                                                    CheckHexInChain(0);
                                                }

                                                void CheckHexInChain(int _index)
                                                {
                                                    if (needBoat.GetComponent<Traffic>().isThisWayPointIsOnTheWay(chains[i].chain[_index]))
                                                    {
                                                        MoveObjectEvent?.Invoke(needBoat, _hex, SideType.River);
                                                    }
                                                }
                                            }
                                        }
                                    }                                 
                                    break;

                                case SideType.Road:

                                    if (chains[i].isChainWithMovingTransport/* == false*/)
                                    {
                                        if (/*!chains[i].isTransportIsCreatedForChain && */chains[i].chain.Count == 2)
                                        {
                                           // Debug.Log("Создать машину!");

                                            GameObject car = Instantiate(carPrefab, _hex.GetComponentInChildren<WayPoint>().transform.position, Quaternion.identity);

                                            cars.Add(car);

                                            car.name = cars.Count + " Car";

                                            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
                                            {
                                                PlayerPrefs.SetInt("NumberCars", cars.Count);
                                            }

                                            if (chains[i].chain[0] == _hex)
                                            {
                                                MoveObjectEvent?.Invoke(car, chains[i].chain[1], SideType.Road);
                                            }
                                            else
                                            {
                                                MoveObjectEvent?.Invoke(car, chains[i].chain[0], SideType.Road);
                                            }

                                            MoveObjectEvent?.Invoke(car, _hex, SideType.Road);

                                          //  chains[i].isTransportIsCreatedForChain = true;
                                            // chains[i].isChainWithMovingTransport = true;

                                        }
                                        else
                                        {
                                            foreach (GameObject needCar in cars)
                                            {
                                                if (chains[i].chain[1] != _hex)
                                                {
                                                    CheckHexInChain(1);
                                                }
                                                else
                                                {
                                                    CheckHexInChain(0);
                                                }

                                                void CheckHexInChain(int _index)
                                                {
                                                    if (needCar.GetComponent<Traffic>().isThisWayPointIsOnTheWay(chains[i].chain[_index]))
                                                    {
                                                        MoveObjectEvent?.Invoke(needCar, _hex, SideType.Road);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                   
                                    break;

                                case SideType.Rails:
                                    if (chains[i].isChainWithMovingTransport/* == false*/)
                                    {
                                        if (chains[i].chain.Count == 2)
                                        {
                                           // Debug.Log("Создать поезд!");

                                            GameObject train = Instantiate(trainPrefab, _hex.GetComponentInChildren<WayPoint>().transform.position, Quaternion.identity);

                                            trains.Add(train);

                                            train.name = trains.Count + " Train";

                                            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
                                            {
                                                PlayerPrefs.SetInt("NumberTrains", trains.Count);
                                            }

                                            if (chains[i].chain[0] == _hex)
                                            {
                                                MoveObjectEvent?.Invoke(train, chains[i].chain[1], SideType.Rails);
                                            }
                                            else
                                            {
                                                MoveObjectEvent?.Invoke(train, chains[i].chain[0], SideType.Rails);
                                            }

                                            MoveObjectEvent?.Invoke(train, _hex, SideType.Rails);

                                            // chains[i].isChainWithMovingTransport = true;
                                        }
                                        else
                                        {
                                            foreach (GameObject needTrain in trains)
                                            {
                                                if (chains[i].chain[1] != _hex)
                                                {
                                                    CheckHexInChain(1);
                                                }
                                                else
                                                {
                                                    CheckHexInChain(0);
                                                }

                                                void CheckHexInChain(int _index)
                                                {
                                                    if (needTrain.GetComponent<Traffic>().isThisWayPointIsOnTheWay(chains[i].chain[_index]))
                                                    {
                                                        MoveObjectEvent?.Invoke(needTrain, _hex, SideType.Rails);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                 
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void CreatingTransportOnCutOffChains(HexOfWorld _hex)
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                List<Chain> chains;
                allChains.TryGetValue(sideType, out chains);

                if (chains != null)
                {
                    for (int i = 0; i < chains.Count; i++)
                    {
                        if (chains[i].chainType == SideType.River || chains[i].chainType == SideType.Road || chains[i].chainType == SideType.Rails)
                        {
                            // for (int y = 0; y < chains[i].chain.Count; y++)
                            // {
                            if (!chains[i].isTransportIsCreatedForChain)
                            {
                                HexOfWorld firsHex = chains[i].chain[0];
                                HexOfWorld lastHex = chains[i].chain[1];
                            }
                            // }
                        }
                    }
                }
            }
        }

        public int GetChainCountByHex(HexOfWorld _hex)
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                List<Chain> chains;
                allChains.TryGetValue(sideType, out chains);

                if (chains != null)
                {
                    for (int i = 0; i < chains.Count; i++)
                    {
                        if (chains[i].chainType == SideType.River || chains[i].chainType == SideType.Road || chains[i].chainType == SideType.Rails)
                        {
                            for (int y = 0; y < chains[i].chain.Count; y++)
                            {
                                if (chains[i].chain[y] == _hex)
                                {
                                    return chains[i].chain.Count;
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }


        public HexOfWorld HexSearchByWayPoint(Vector3 _wayPoint)
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                List<Chain> chains;
                allChains.TryGetValue(sideType, out chains);

                if (chains != null)
                {
                    for (int i = 0; i < chains.Count; i++)
                    {
                        if (chains[i].chainType == SideType.River || chains[i].chainType == SideType.Road || chains[i].chainType == SideType.Rails)
                        {
                            for (int y = 0; y < chains[i].chain.Count; y++)
                            {
                                if (chains[i].chain[y].GetComponentInChildren<WayPoint>().transform.position == _wayPoint)
                                {
                                    return chains[i].chain[y];
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        //private void FindRemovingHexPositionFromWayPoint(HexOfWorld _deletedHex)
        //{
        //    SearchHexesInChainWithTransport(_deletedHex, SearchWayPoint);

        //    void SearchWayPoint(List<Chain> chains, int i, int j)
        //    {
        //        if (chains[i].chain[j].GetComponentInChildren<WayPoint>().transform.position == _deletedHex.GetComponentInChildren<WayPoint>().transform.position)
        //        {
        //            RemovingHexPositionFromWayPointEvent?.Invoke(chains[i].chain[j]);
        //        }
        //    }
        //}

        private void FindRemovingHexPositionFromWayPoint(HexOfWorld _deletedHex)
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                List<Chain> chains;
                allChains.TryGetValue(sideType, out chains);

                if (chains != null)
                {
                    for (int i = 0; i < chains.Count; i++)
                    {
                        if (chains[i].chainType == SideType.River || chains[i].chainType == SideType.Road || chains[i].chainType == SideType.Rails)
                        {
                            for (int y = 0; y < chains[i].chain.Count; y++)
                            {
                                if (chains[i].chain[y] == _deletedHex)
                                {
                                    if (chains[i].chain[y].GetComponentInChildren<WayPoint>().transform.position == _deletedHex.GetComponentInChildren<WayPoint>().transform.position)
                                    {
                                        RemovingHexPositionFromWayPointEvent?.Invoke(chains[i].chain[y]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //private void SearchHexesInChainWithTransport(HexOfWorld _hex, Action<List<Chain>, int, int> _action)
        //{
        //    foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
        //    {
        //        List<Chain> chains;
        //        allChains.TryGetValue(sideType, out chains);

        //        if (chains != null)
        //        {
        //            for (int i = 0; i < chains.Count; i++)
        //            {
        //                if (chains[i].chainType == SideType.River || chains[i].chainType == SideType.Road || chains[i].chainType == SideType.Rails)
        //                {
        //                    for (int j = 0; j < chains[i].chain.Count; j++)
        //                    {
        //                        _action?.Invoke(chains, i, j);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public void AddNumberTransport(GameObject _transport, string _namekey)
        {
            switch (_namekey)
            {
                case " Car":
                    cars.Add(_transport);
                    break;
                case " Boat":
                    boats.Add(_transport);
                    break;
                case " Train":
                    trains.Add(_transport);
                    break;
            }           
        }

        public int GetChainScoreForHex(HexOfWorld hex, SideType sideType)
        {
            int result = 0;

            Chain longestChainWithHex = FindLongestChainWithHex(hex, sideType);

            if (longestChainWithHex != null)
            {
                result = longestChainWithHex.GetChainScore();
            }

            return result;
        }

        public Chain FindLongestChainBySideType(SideType sideType)
        {
            List<Chain> sideChains;

            if (allChains.ContainsKey(sideType))
            {
                sideChains = allChains[sideType];
            }
            else
            {
                sideChains = null;
            }

            if (sideChains == null || sideChains.Count == 0) return null;

            int index = 0;
            int largestCount = 0;

            for (int i = 0; i < sideChains.Count; i++)
            {
                if (sideChains[i].chain.Count > largestCount)
                {
                    largestCount = sideChains[i].chain.Count;
                    index = i;
                }
            }

            return sideChains[index];
        }

        public Chain FindTwoLongestChainBySideType(SideType sideType, int _indexSideType)
        {
            if (allChains.ContainsKey(sideType))
            {
                sideTwoChains = allChains[sideType];
            }
            else
            {
                sideTwoChains = null;
            }

            if (sideTwoChains == null || sideTwoChains.Count == 0) return null;

            if (sideTwoChains.Count > 1)
            {
                sideTwoChains.Sort((x, y) => y.chain.Count.CompareTo(x.chain.Count));

                if (_indexSideType == 0)
                {
                    return sideTwoChains[0];
                }
                else if (_indexSideType == 1)
                {
                    return sideTwoChains[1];
                }
            }
            else if (sideTwoChains.Count == 1)
            {
                if (_indexSideType == 0)
                {
                    return sideTwoChains[0];
                }
            }

            return null;
        }

        public Chain FindLongestChainWithHex(HexOfWorld hex, SideType sideType)
        {
            List<Chain> result = new List<Chain>();

            List<Chain> sideChains;

            if (allChains.ContainsKey(sideType))
            {
                sideChains = allChains[sideType];
            }
            else
            {
                sideChains = null;
            }

            if (sideChains != null)
            {
                foreach (Chain chain in sideChains)
                {
                    if (chain.isChainContainsHex(hex))
                    {
                        result.Add(chain);
                    }
                }
            }

            if (result == null || result.Count == 0) return null;

            int index = 0;
            int largestCount = 0;

            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].chain.Count > largestCount)
                {
                    largestCount = result[i].chain.Count;
                    index = i;
                }
            }

            return result[index];
        }

        public int getChainIndexThatContainsChain(ref List<Chain> collectionOfChains, ref Chain chainResult)
        {
            for (int i = 0; i < collectionOfChains.Count; i++)
            {
                if (collectionOfChains[i].isChainContainsChainElements(chainResult)) return i;
            }

            return -1;
        }

        public Chain FindChain(HexOfWorld startHex, SideType type, ref Chain result, ref List<HexOfWorld> alreadyIndexed)
        {
            // Выйти из поиска если стартового хексагона не существует
            if (startHex == null) return result;
            // Выйти из поиска если стартовый хексагон уже находился в обработке до этого
            if (alreadyIndexed.Contains(startHex)) return result;
            // Выйти из поиска если стартовый хексагон не содержит в себе нужную сторону
            if (startHex.isContainsSide(type) == false) return result;

            // Добавляем хексагон в коллекцию уже пройденных хексагонов
            alreadyIndexed.Add(startHex);
            // Добавляем текущий хексагон в результат, так как он содержит нужную сторону
            result.chain.Add(startHex);

            foreach (KeyValuePair<HexOfWorld.Side, SideDataAndConditions> startCellSideDataKeyValue in startHex.data.sidesData)
            {
                HexOfWorld.Side currentSide = startCellSideDataKeyValue.Key;

                startHex.UpdateNeightbours();

                HexOfWorld neightbourHex = startHex.neightbours[currentSide];

                // Если тип текущей стороны хекса является искомым типом
                if (neightbourHex != null && startHex.data.sidesData[currentSide].sideData.type == type)
                {
                    // Если ближайший сосед существует и не был проиндексирован
                    if (startCellSideDataKeyValue.Value != null && alreadyIndexed.Contains(neightbourHex) == false)
                    {
                        //Если между стартовым гексом и его соседом одинаковые типы краёв, запускаем алгоритм поиска и для соседа тоже
                        if (isNearHexHaveConnection(startHex, currentSide))
                        {
                            result = FindChain(neightbourHex, type, ref result, ref alreadyIndexed);
                        }
                        else
                        {
                            // NOTHING

                            //if (alreadyIndexed.Contains(neightbourHex) == false) {
                            //    alreadyIndexed.Add(neightbourHex);
                            //}
                        }

                        //Если у соседа нет сторон нужного типа добавить его в пройденные
                        if (neightbourHex.isContainsSide(type) == false && alreadyIndexed.Contains(neightbourHex) == false)
                        {
                            alreadyIndexed.Add(neightbourHex);
                        }
                    }                    
                }
            }

            // Возвращаем полученный результат
            return result;
        }

        public bool isNearHexHaveConnection(HexOfWorld startCell, HexOfWorld.Side startCellSide)
        {
            HexOfWorld neightbour = startCell.neightbours[startCellSide];
            if (neightbour == null) return false;

            HexOfWorld.Side neightbourAdjastedSide = Helpers.SideHelper.GetAdjacedSide(startCellSide);

            return neightbour.data.sidesData[neightbourAdjastedSide].sideData.type ==
                   startCell.data.sidesData[startCellSide].sideData.type;
        }

        public void OnDrawGizmosSelected()
        {
            foreach (SideType sideType in (SideType[])Enum.GetValues(typeof(SideType)))
            {
                List<Chain> chains;
                allChains.TryGetValue(sideType, out chains);

                if (chains != null)
                {
                    foreach (Chain chain in chains)
                    {
                        Color color = Color.white;
                        Vector3 offset = Vector3.zero;

                        switch (sideType)
                        {
                            case SideType.Field:
                                color = Color.yellow;
                                offset = Vector3.left * 0.1f;
                                break;
                            case SideType.Forest:
                                color = Color.green;
                                offset = Vector3.left * 0.15f;
                                break;
                            case SideType.River:
                                color = Color.blue;
                                offset = Vector3.left * 0.20f;
                                break;
                            case SideType.Road:
                                color = Color.gray;
                                offset = Vector3.left * 0.25f;
                                break;
                            case SideType.City:
                                color = Color.white;
                                offset = Vector3.left * 0.30f;
                                break;
                            case SideType.Rails:
                                color = Color.cyan;
                                offset = Vector3.left * 0.35f;
                                break;
                        }

                        chain.DrawGizmos(color, offset);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class Chain
    {
        public List<HexOfWorld> chain = new List<HexOfWorld>();
        public SideType chainType;
        public bool isChainWithMovingTransport = false;
        public bool isTransportIsCreatedForChain = false;

        public bool isChainContainsHex(HexOfWorld hex)
        {
            return chain.Contains(hex);
        }

        public bool isChainContainsChainElements(Chain chain)
        {
            int counter = 0;

            foreach (HexOfWorld hex in chain.chain)
            {
                if (this.chain.Contains(hex)) counter++;
            }

            return counter == this.chain.Count;
        }

        public int GetChainScore()
        {
            int result = 0;

            foreach (HexOfWorld hex in chain)
            {
                result += hex.GetScoreBySideType(chainType);
            }

            return result;
        }

        public void DrawGizmos(Color color, Vector3 offset)
        {
            Gizmos.color = color;

            foreach (HexOfWorld hex in chain)
            {
                foreach (KeyValuePair<HexOfWorld.Side, HexOfWorld> keyValue in hex.neightbours)
                {
                    if (keyValue.Value != null && isChainContainsHex(keyValue.Value))
                    {
                        Gizmos.DrawLine(hex.transform.position + (Vector3.up / 2) + offset,
                                        keyValue.Value.transform.position + (Vector3.up / 2) + offset);
                    }
                }
            }
        }
    }
}
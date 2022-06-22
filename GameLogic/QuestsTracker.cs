using UnityEngine;
using GameEntity;
using System.Collections.Generic;
using Singleton;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using GameData;
using GameUI;
using static GameData.HexSideData;
using static GameEntity.HexOfWorld;
using Helpers;
using DG.Tweening;

namespace GameLogic
{
    public class QuestsTracker : MonoBehaviour
    {
        [ReadOnly]
        public List<HexOfWorld> questHexes = new List<HexOfWorld>();

        public List<HexTaskGameUI> hexTasks;
        public GameObject containerHexTask;
        public GameObject hexTaskGameUI;

        private ChainDetection chainDetection;

        public static Action<HexOfWorld> OnQuestWasComplited;
        public static Action<HexOfWorld> OnQuestWasFailed;

        /// <summary>
        /// OnGlobalQuestConditionProgress(int indexOfCondition, int currentChainElementsAmount, int maxChainLenght)
        /// </summary>
        public static Action<int, int, int, GlobalMissionData> OnGlobalQuestConditionChainProgress;
        public static Action<int, GlobalMissionData> OnGlobalQuestConditionQuestCompliteProgress;
        public static Action<int, GlobalMissionData> OnGlobalQuestConditionQuestScoreProgress;

        private int questsComplite = 0;
        private int currentScoreAmount = 0;
        private int numberСompletedMissions = 0;

        public static Action<bool> IsMissionCompletedEvent;        
        public static Action OpenPopupGlobalMissionsWhenWinningEvent;
        public static Action OpenPopupGlobalMissionsEvent;

        public static Action<HexOfWorld> DeleteQuestSaveKeyEvent;

        public void Awake()
        {
            chainDetection = FindObjectOfType<ChainDetection>();

            ChainDetection.OnChainsChanged += ChainWasChanged;
            BuildingCellDetection.OnNewHexAddedToBoard += OnNewHexPlaced;
            BuildingCellDetection.OnCurrentHexDestroyed += OnHexDestroyed;
            BuildingCellDetection.OnGhostHexPlaced += OnGhostHexPlaced;

            UIBonusesPanel.RemoveHexEvent += OnHexDestroyed;
            UIBonusesPanel.CheckingTaskEvent += TrackGlobalMissionState;
            UIBonusesPanel.CheckingCompletionOfTaskEvent += OnNewHexPlaced;

            Merge.OnHexCreated += OnNewHexPlaced;
            Merge.OnHexDestroyed += OnHexDestroyed;

            UIChainsScoresTracker.GetCurrentScoreAmountEvent += GetCurrentScoreAmount;

            SaveLoadFreeGame.AddQuestHexInQuestTrackerEvent += AddQuestWhenLoadingFreeGame;
            SaveLoadFreeGame.UpdateQuestScoreEvent += ChainWasChanged;

            HexOfWorld.UpdateHexTaskUIEvent += UpdateHexTaskUI;
        }

        private void Start()
        {
            TrackGlobalMissionState();
        }

        private void AddQuestWhenLoadingFreeGame(HexOfWorld _hex)
        {
            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
            {
                questHexes.Add(_hex);
                OnNewHexPlaced(_hex);
            }
        }

        private void OnGhostHexPlaced(HexOfWorld hex)
        {
            if (hex.isQuestHex)
            {
                questHexes.Add(hex);
            }
        }

        private void UpdateHexTaskUI()
        {
            for (int i = 0; i < hexTasks.Count; i++)
            {
                hexTasks[i].score.text = questHexes[i].questText.text;
                hexTasks[i].questIcon.sprite = questHexes[i].questIcon.sprite;
                hexTasks[i].score.color = questHexes[i].questText.color;
            }
        }

        public void OnDestroy()
        {
            ChainDetection.OnChainsChanged -= ChainWasChanged;
            BuildingCellDetection.OnNewHexAddedToBoard -= OnNewHexPlaced;
            BuildingCellDetection.OnCurrentHexDestroyed -= OnHexDestroyed;
            BuildingCellDetection.OnGhostHexPlaced -= OnGhostHexPlaced;

            UIBonusesPanel.RemoveHexEvent -= OnHexDestroyed;
            UIBonusesPanel.CheckingTaskEvent -= TrackGlobalMissionState;
            UIBonusesPanel.CheckingCompletionOfTaskEvent -= OnNewHexPlaced;

            Merge.OnHexCreated -= OnNewHexPlaced;
            Merge.OnHexDestroyed -= OnHexDestroyed;

            UIChainsScoresTracker.GetCurrentScoreAmountEvent -= GetCurrentScoreAmount;

            SaveLoadFreeGame.AddQuestHexInQuestTrackerEvent -= AddQuestWhenLoadingFreeGame;
            SaveLoadFreeGame.UpdateQuestScoreEvent -= ChainWasChanged;

            HexOfWorld.UpdateHexTaskUIEvent += UpdateHexTaskUI;
        }

        private void OnHexDestroyed(HexOfWorld hex)
        {
            if (questHexes.Contains(hex))
            {
                questHexes.Remove(hex);

                for (int i = 0; i < hexTasks.Count; i++)
                {
                    if (hex.questData.questID == hexTasks[i].Id)
                    {
                        Destroy(hexTasks[i].gameObject);
                        hexTasks.Remove(hexTasks[i]);
                    }
                }
            }

            ChainWasChanged();
        }

        private void OnNewHexPlaced(HexOfWorld hex)
        {
            OnNewHexPlaced();

            if (hex.isQuestHex)
            {
                HexTaskGameUI hexTask = Instantiate(hexTaskGameUI, containerHexTask.transform.position, Quaternion.identity).GetComponent<HexTaskGameUI>();
                hexTask.transform.SetParent(containerHexTask.transform);
                hexTask.GetComponent<RectTransform>().localScale = new Vector3(1f,1f,1f);
                hexTask.Id = hex.questData.questID;

                hexTasks.Add(hexTask);
            }

            UpdateHexTaskUI();
        }

        private void OnNewHexPlaced()
        {
            // Global mission tracker
            TrackGlobalMissionState();

            foreach (HexOfWorld hex in questHexes.ToArray())
            {
                if (hex.isQuestScoreLowerThenNeed())
                {
                    //Nothing
                }
                else if (hex.isQuestScoreHigherThenNeed())
                {
                    QuestFailed(hex);
                }
                else if (hex.isQuestScoreCorrect())
                {
                    QuestComplite(hex);
                }
            }
        }

        private void ChainWasChanged()
        {
            StartCoroutine(cr_delayBeforeUpdate());
        }

        private IEnumerator cr_delayBeforeUpdate()
        {
            yield return new WaitForFixedUpdate();
            // Update values for quest hexes
            foreach (HexOfWorld hex in questHexes)
            {
                if (hex != null && chainDetection != null && hex.isQuestHex)
                {
                    hex.UpdateQuestScore(chainDetection.GetChainScoreForHex(hex, hex.questData.sideType));
                }
            }
        }

        private void TrackGlobalMissionState()
        {
            if (GameSettings.Instance.currentGameMode == GameStyle.GlobalMapMission)
            {
                if (chainDetection != null)
                {
                    int correctConditionsForGlobalMission = 0;

                    // Проверка всех условий для каждой глобальной миссии из списка
                    foreach (GlobalMissionData data in GameSettings.Instance.globalMissionData)
                    {
                        switch (data.missionType)
                        {
                            case GlobalMissionType.BuildChain:
                                buildChainCheck(data, ref correctConditionsForGlobalMission);
                                break;
                            case GlobalMissionType.CompliteQuests:
                                compliteHexQuest(data, ref correctConditionsForGlobalMission);
                                break;
                            case GlobalMissionType.ScorePoints:
                                compliteSetOfScores(data, ref correctConditionsForGlobalMission);
                                break;
                        }
                    }

                    if (correctConditionsForGlobalMission > 0 && correctConditionsForGlobalMission < GameSettings.Instance.globalMissionData.Count)
                    {
                        if (numberСompletedMissions < correctConditionsForGlobalMission)
                        {
                            if (!Tutorial.Instance.isTutorial)
                            {
                                OpenPopupGlobalMissionsEvent?.Invoke();
                                numberСompletedMissions = correctConditionsForGlobalMission;
                            }
                        }
                    }
                    else if (correctConditionsForGlobalMission == GameSettings.Instance.globalMissionData.Count)
                    {
                        SaveLevelIndex();

                        IsMissionCompletedEvent?.Invoke(true);

                        if (Tutorial.Instance.isTutorial)
                        {
                            TutorialScreenManagement();
                        }
                        else
                        {
                            OpenPopupGlobalMissionsWhenWinningEvent?.Invoke();
                        }
                    }
                }
            }
        }

        private void TutorialScreenManagement()
        {
            if (GameSettings.Instance.currentMissionIndex == 0)
            {
                if (Tutorial.Instance.currentScreen == Tutorial.Instance.screenIndexBeforeMistakeInLevel1)
                {
                    Tutorial.Instance.NextScreen();
                }
            }
            else if (GameSettings.Instance.currentMissionIndex == 1)
            {
                if (Tutorial.Instance.currentScreen != Tutorial.Instance.screenIndexBeforeMistakeInLevel2_2)
                {
                    Tutorial.Instance.currentScreen++;
                }

                Tutorial.Instance.NextScreen();
            }
            else if (GameSettings.Instance.currentMissionIndex == Tutorial.Instance.indexFirstLevelWithRiver)
            {
                if (Tutorial.Instance.currentScreen == Tutorial.Instance.screenIndexBeforeMistakeInLevel9_2)
                {
                    Tutorial.Instance.currentScreen++;
                }

                Tutorial.Instance.NextScreen();
            }
        }

        private void SaveLevelIndex()
        {
            int currentLevel = GameSettings.Instance.currentMissionIndex + 1;

            if (currentLevel <= GameSettings.Instance.allGlobalMapMissions.Count - 1)
            {
                if (currentLevel >= PlayerPrefs.GetInt("LevelsUnlocked"))
                {
                    PlayerPrefs.SetInt("LevelsUnlocked", currentLevel + 1);
                }
            }
        }

        private void GetCurrentScoreAmount(int _scores)
        {
            currentScoreAmount = _scores;
        }

        private void compliteHexQuest(GlobalMissionData data, ref int correctConditionsForGlobalMission)
        {
            if (data.missionType == GlobalMissionType.CompliteQuests)
            {
                if (questsComplite >= data.questsAmount)
                {
                    correctConditionsForGlobalMission++;
                }

                if (OnGlobalQuestConditionQuestCompliteProgress != null)
                {
                    OnGlobalQuestConditionQuestCompliteProgress(questsComplite, data);
                }
            }
        }

        private void buildChainCheck(GlobalMissionData data, ref int correctConditionsForGlobalMission)
        {
            if (data.isMultipleChains == false && data.isMultipleChainsWithoutRoads == false)
            {
                BuildOnlyOneChain(data, ref correctConditionsForGlobalMission);
            }
            else if (data.isMultipleChains)
            {
                BuildMultipleConnectedChains(data, ref correctConditionsForGlobalMission);
            }
            else if (data.isMultipleChainsWithoutRoads)
            {
                BuildTwoChain(data, ref correctConditionsForGlobalMission);
            }
        }

        private void compliteSetOfScores(GlobalMissionData data, ref int correctConditionsForGlobalMission)
        {
            if (data.missionType == GlobalMissionType.ScorePoints)
            {
                if (currentScoreAmount >= data.scoresAmount)
                {
                    correctConditionsForGlobalMission++;
                }
                //  else
                // {
                OnGlobalQuestConditionQuestScoreProgress?.Invoke(currentScoreAmount, data);
                // }
            }
        }

        private void BuildOnlyOneChain(GlobalMissionData data, ref int correctConditionsForGlobalMission)
        {
            int chainLenght;

            if (data.sideType == null || data.sideType.Count == 0)
            {
                Debug.LogError("Incorrect elements count for mission: " + data.questDescription);
            }

            Chain longestChainByType = chainDetection.FindLongestChainBySideType(data.sideType[0]);

            if (longestChainByType != null)
            {
                chainLenght = longestChainByType.chain.Count;

                if (OnGlobalQuestConditionChainProgress != null)
                {
                    OnGlobalQuestConditionChainProgress(0, chainLenght, data.chainLengths[0], data);
                }

                if (chainLenght >= data.chainLengths[0])
                {
                    correctConditionsForGlobalMission++;
                }
            }
        }

        private void BuildTwoChain(GlobalMissionData data, ref int correctConditionsForGlobalMission)
        {
            int chainLenght1 = 0;
            int chainLenght2 = 0;

            Chain longestChainByType1 = chainDetection.FindTwoLongestChainBySideType(data.sideType[0], 0);

            if (longestChainByType1 != null)
            {
                chainLenght1 = longestChainByType1.chain.Count;
            }

            Chain longestChainByType2 = chainDetection.FindTwoLongestChainBySideType(data.sideType[0], 1);

            if (longestChainByType2 != null)
            {
                chainLenght2 = longestChainByType2.chain.Count;
            }

            OnGlobalQuestConditionChainProgress?.Invoke(0, chainLenght1, data.chainLengths[0], data);
            OnGlobalQuestConditionChainProgress?.Invoke(1, chainLenght2, data.chainLengths[1], data);

            if (chainLenght1 >= data.chainLengths[0] && chainLenght2 >= data.chainLengths[1])
            {
                correctConditionsForGlobalMission++;
            }
        }

        /// <summary>
        /// Несколько цепей вместе и нужного размера, цепь 0 и 2 соеденены цепью 1
        /// </summary>
        /// <param name="data">Данные миссии</param>
        /// <param name="chainsCorrectLength">Правильная длинна цепи, отслеживание</param>
        private void BuildMultipleConnectedChains(GlobalMissionData data, ref int chainsCorrectLength)
        {
            HashSet<SideType> allSideTypes = new HashSet<SideType>();

            SideType City1SideType = data.sideType[0];
            SideType RoadSideType = data.sideType[1];
            SideType City2SideType = data.sideType[2];

            List<Chain> allRoads = chainDetection.allChains[RoadSideType];

            if (allRoads != null && allRoads.Count > 0)
            {

                Chain city1 = null;
                Chain city2 = null;
                Chain road = null;

                foreach (SideType sideType in data.sideType)
                {
                    allSideTypes.Add(sideType);
                }

                if (isDataHaveEnoughtElements(data, 3))
                {
                    // Перебираем все возможные цепи дорог
                    foreach (Chain chainRoad in allRoads)
                    {
                        // Если дорога подходит под условие
                        if (isRaodHasEnoughtHexesInChain(chainRoad, data))
                        {
                            road = chainRoad;
                            UpdateChainsLenght(city1, city2, road, data);
                            // Перебираем каждый хексагон дороги
                            foreach (HexOfWorld hexOfRoad in chainRoad.chain)
                            {
                                // Перебираем каждого соседа текущего хексагона
                                foreach (KeyValuePair<Side, HexOfWorld> keyValue in hexOfRoad.neightbours)
                                {
                                    HexOfWorld neightBourHex = keyValue.Value;

                                    // Если сосед существует
                                    if (neightBourHex != null)
                                    {
                                        Side neightbourSide = SideHelper.GetAdjacedSide(keyValue.Key);

                                        SideType neightbourAdjacedSideType = neightBourHex.data.sidesData[neightbourSide].sideData.type;

                                        if (city1 == null && neightbourAdjacedSideType == City1SideType)
                                        {
                                            FindChainNearTheRoad(0, ref neightBourHex, data, ref city1, ref city2);

                                            UpdateChainsLenght(city1, city2, road, data);

                                            if (isBothCityChainIsNotNull(ref city1, ref city2))
                                            {
                                                break;
                                            }
                                        }
                                        else if (city2 == null && neightbourAdjacedSideType == City2SideType)
                                        {
                                            FindChainNearTheRoad(2, ref neightBourHex, data, ref city2, ref city1);

                                            UpdateChainsLenght(city1, city2, road, data);

                                            if (isBothCityChainIsNotNull(ref city1, ref city2))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (isBothCityChainIsNotNull(ref city1, ref city2)) break;
                            }
                        }
                        if (isBothCityChainIsNotNull(ref city1, ref city2)) break;
                    }
                }

                if (isAllSizesCorrect(data, city1, city2, road))
                {
                    chainsCorrectLength++;
                }
            }
        }

        private void UpdateChainsLenght(Chain city1, Chain city2, Chain road, GlobalMissionData data)
        {
            if (OnGlobalQuestConditionChainProgress != null)
            {
                if (city1 != null)
                {
                    OnGlobalQuestConditionChainProgress(0, city1.chain.Count, data.chainLengths[0], data);
                }
                if (road != null)
                {
                    OnGlobalQuestConditionChainProgress(1, road.chain.Count, data.chainLengths[1], data);
                }
                if (city2 != null)
                {
                    OnGlobalQuestConditionChainProgress(2, city2.chain.Count, data.chainLengths[2], data);
                }
            }
        }

        private void UpdateTwoChainsLenght(Chain city1, Chain city2, GlobalMissionData data)
        {
            if (OnGlobalQuestConditionChainProgress != null)
            {
                if (city1 != null)
                {
                    OnGlobalQuestConditionChainProgress(0, city1.chain.Count, data.chainLengths[0], data);
                }

                if (city2 != null)
                {
                    OnGlobalQuestConditionChainProgress(1, city2.chain.Count, data.chainLengths[1], data);
                }
            }
        }

        private bool isAllSizesCorrect(GlobalMissionData data, Chain city1, Chain city2, Chain road)
        {
            return city1 != null && city2 != null && road != null &&
               city1.chain.Count >= data.chainLengths[0] &&
               road.chain.Count >= data.chainLengths[1] &&
               city2.chain.Count >= data.chainLengths[2];
        }

        private bool isBothCityChainIsNotNull(ref Chain city1, ref Chain city2)
        {
            return city1 != null && city2 != null;
        }

        private bool isDataHaveEnoughtElements(GlobalMissionData data, int minimalElementsAmount)
        {
            if (data.chainLengths.Count < minimalElementsAmount || data.sideType.Count < minimalElementsAmount)
            {
                Debug.LogError("Incorrect elements count for mission: " + data.questDescription);
            }

            return data.chainLengths.Count >= minimalElementsAmount && data.sideType.Count >= minimalElementsAmount;
        }

        private bool isRaodHasEnoughtHexesInChain(Chain roadChain, GlobalMissionData data)
        {
            return roadChain.chain.Count >= data.chainLengths[1];
        }

        private void FindChainNearTheRoad(int index, ref HexOfWorld neightBourHex, GlobalMissionData data, ref Chain cityToSet, ref Chain cityToCompare)
        {
            // Find chain with neightbour hex
            Chain cityChain = chainDetection.FindLongestChainWithHex(neightBourHex, data.sideType[index]);

            if (cityChain == null || data == null) return;

            if ((cityToCompare == null && cityChain.chain.Count >= data.chainLengths[index]) ||
                (cityToCompare != null && cityToCompare.Equals(cityChain) == false && cityChain.chain.Count >= data.chainLengths[index]))
            {
                // Add Chain to possible nearest chains
                cityToSet = cityChain;
            }
        }

        private void QuestFailed(HexOfWorld hex)
        {
            if (OnQuestWasFailed != null)
            {
                OnQuestWasFailed(hex);
            }

            RemoveQuestHex(hex);
        }

        private void QuestComplite(HexOfWorld hex)
        {
            SingletonHexCollection.Instance.AddHexes(hex.questData.rewardHexesAmount);

            questsComplite++;

            TrackGlobalMissionState();

            if (OnQuestWasComplited != null)
            {
                OnQuestWasComplited(hex);
            }

            RemoveQuestHex(hex);
        }

        private void RemoveQuestHex(HexOfWorld hex)
        {
            Debug.Log(2);
            hex.questText.transform.parent.gameObject.SetActive(false);
            hex.questText.text = "";
            hex.isQuestHex = false;
            questHexes.Remove(hex);

            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
            {
                DeleteQuestSaveKeyEvent?.Invoke(hex);
            }

            for (int i = 0; i < hexTasks.Count; i++)
            {
                if (hex.questData.questID == hexTasks[i].Id)
                {
                    Destroy(hexTasks[i].gameObject);
                    hexTasks.Remove(hexTasks[i]);
                }
            }
        }
    }
}

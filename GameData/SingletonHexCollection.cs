using Singleton;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using GameData;
using TMPro;
using GameUI;
using GameEntity;
using static GameData.HexSideData;
using GameLogic;
using static GameEntity.HexOfWorld;
using System;


namespace Singleton
{
    [CreateAssetMenu(fileName = "HexCollection", menuName = "Singletons/HexCollection")]
    public class SingletonHexCollection : SingletonScriptableObject<SingletonHexCollection>
    {
        public int startHexAmount;
        [ReadOnly]
        public int hexsLeft;
        [ReadOnly]
        public int currentIndex;

        [ReadOnly]
        public List<CellOfWorldDataConfig> hexesAvailable = new List<CellOfWorldDataConfig>();
        [Space]
        public Dictionary<SideType, List<QuestHexDataConfig>> quests = new Dictionary<SideType, List<QuestHexDataConfig>>();
        [Space]
        public Dictionary<SideType, List<QuestHexDataConfig>> startQuests = new Dictionary<SideType, List<QuestHexDataConfig>>();

        [ReadOnly]
        public List<CellOfWorldData> hexesInDeck = new List<CellOfWorldData>();
        [ReadOnly]
        public List<QuestHexDataConfig> questsConfigsInDeck = new List<QuestHexDataConfig>();
        [ReadOnly]
        public List<bool> questsInDeck = new List<bool>();
        [ReadOnly]
        public HashSet<SideType> questTypes = new HashSet<SideType>();
        [ReadOnly]
        public UIHexPreview currentHex;
        [ReadOnly]
        public UIHexPreview nextHex;

        public UIHexesCounter textHexsLeft;

        public int startQuestsAmount = 3;
        private int numberSpacesBetweenQuests;
        private int firstSpaces;
        private const int INDEX_FIRST_HEX_WITH_QUEST = 3;

        public CellOfWorldDataConfig bonusHexPrefub;

        [ReadOnly]
        public bool isTutorial;

        private int indexFirstLevelWithRiver = 8;

        public static Action IncreasingCurrentHexIndex;
        public static Action UpdateIsTutorialEvent;
        public static Action CheckingStatusUIHexPreviewCurrentEvent;

        private void OnEnable()
        {
            Tutorial.AddLastHexInTutorialLevel1Event += AddLastHexInTutorialLevel1;
            Tutorial.AddHexInTutorialLevel2Event += AddHexInTutorialLevel2;
            Tutorial.AddLastHexInTutorialLevel2Event += AddLastHexInTutorialLevel2;
            Tutorial.AddHexInTutorialLevel9Event += AddHexInTutorialLevel9;
            Tutorial.AddLastHexInTutorialLevel9Event += AddLastHexInTutorialLevel9;
        }

        private void OnDisable()
        {
            Tutorial.AddLastHexInTutorialLevel1Event -= AddLastHexInTutorialLevel1;
            Tutorial.AddHexInTutorialLevel2Event -= AddHexInTutorialLevel2;
            Tutorial.AddLastHexInTutorialLevel2Event -= AddLastHexInTutorialLevel2;
            Tutorial.AddHexInTutorialLevel9Event -= AddHexInTutorialLevel9;
            Tutorial.AddLastHexInTutorialLevel9Event -= AddLastHexInTutorialLevel9;           
        }

        public void Init()
        {
            isTutorial = PlayerPrefs.GetInt("IsTutorial", 1) > 0;

            if (GameSettings.Instance.currentMissionIndex == indexFirstLevelWithRiver && PlayerPrefs.GetInt("IsTutorial") == 0)
            {
                PlayerPrefs.SetInt("IsTutorial", 1);
                isTutorial = true;

                UpdateIsTutorialEvent?.Invoke();              
            }

            InitStartHexesInDeck();

            numberSpacesBetweenQuests = startHexAmount / startQuestsAmount;
            firstSpaces = numberSpacesBetweenQuests;

            questTypes.Clear();
            hexesInDeck.Clear();
            questsConfigsInDeck.Clear();
            questsInDeck.Clear();

            hexsLeft = startHexAmount;
            currentIndex = 0;

            UIHexPreview[] UIhexprevies = MonoBehaviour.FindObjectsOfType<UIHexPreview>();
            textHexsLeft = MonoBehaviour.FindObjectOfType<UIHexesCounter>();

            foreach (UIHexPreview preview in UIhexprevies) {
                if (preview.currentType == UIHexPreview.HexPreviewType.Current) currentHex = preview;
                if (preview.currentType == UIHexPreview.HexPreviewType.Next) nextHex = preview;
            }

            Unsubscribe();
            Subscribe();

            GenerateRandomHexesDeck();

            UpdateHexUIAndData();
        }

        private void InitStartHexesInDeck()
        {
            hexesAvailable.Clear();

            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode) {
                hexesAvailable = new List<CellOfWorldDataConfig>(GameSettings.Instance.freeModeHexes);
            } else if (GameSettings.Instance.currentGameMode == GameStyle.GlobalMapMission) {
                List<CellOfWorldDataConfig> newHexDeck = new List<CellOfWorldDataConfig>();

                foreach (GlobalMissionData missionData in GameSettings.Instance.globalMissionData) {
                    if (missionData.startDeck != null && missionData.startDeck.Count > 0) {
                        newHexDeck.AddRange(missionData.startDeck);
                    }
                }

                if (newHexDeck.Count > 0) {
                    hexesAvailable = new List<CellOfWorldDataConfig>(newHexDeck);
                } else {
                    hexesAvailable = new List<CellOfWorldDataConfig>(GameSettings.Instance.freeModeHexes);
                }
            }
        }

        public void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            BuildingCellDetection.OnNewHexAddedToBoard += ChangeCurrentHexIndex;
            BuildingCellDetection.OnNewHexAddedToBoard += UpdateHexUIAndData;

            Overlay.OnHexWasOverlayed += ChangeCurrentHexIndex;
            Overlay.OnHexWasOverlayed += UpdateHexUIAndData;

            QuestsTracker.OnQuestWasComplited += QuestHexIsOut;
            QuestsTracker.OnQuestWasFailed += QuestHexIsOut;            
        }

        private void Unsubscribe()
        {
            if (BuildingCellDetection.OnNewHexAddedToBoard != null) {
                BuildingCellDetection.OnNewHexAddedToBoard -= ChangeCurrentHexIndex;
            }
            if (BuildingCellDetection.OnNewHexAddedToBoard != null) {
                BuildingCellDetection.OnNewHexAddedToBoard -= UpdateHexUIAndData;
            }

            if (Overlay.OnHexWasOverlayed != null) {
                Overlay.OnHexWasOverlayed -= ChangeCurrentHexIndex;
            }
            if (Overlay.OnHexWasOverlayed != null) {
                Overlay.OnHexWasOverlayed -= UpdateHexUIAndData;
            }

            if (QuestsTracker.OnQuestWasComplited != null) {
                QuestsTracker.OnQuestWasComplited += QuestHexIsOut;
            }
            if (QuestsTracker.OnQuestWasFailed != null) {
                QuestsTracker.OnQuestWasFailed += QuestHexIsOut;
            }
        }

        private void QuestHexIsOut(HexOfWorld hex)
        {
            if (questTypes.Contains(hex.questData.sideType)) {
                questTypes.Remove(hex.questData.sideType);
            }
        }

        public void UpdateHexUIAndData(HexOfWorld hex)
        {
            UpdateHexUIAndData();
        }

        public void UpdateHexUIAndData()
        {
            if (currentHex != null) {
                if (GetCurrentHexData() != null) {
                    currentHex.hex.gameObject.SetActive(true);

                    if (BuildingCellDetection.lastCreatedCellRotation != null) {
                        currentHex.hex.ChangeModel(GetCurrentHexData(), BuildingCellDetection.lastCreatedCellRotation);
                    } else {
                        currentHex.hex.ChangeModel(GetCurrentHexData(), Quaternion.identity);
                    }

                    if (isCurrentHexQuest()) {
                        currentHex.hex.MakeQuestHex(GetCurrentHexQuestConfig());
                    } else {
                        currentHex.hex.MakeNormalHex();
                    }
                } else {
                    currentHex.hex.gameObject.SetActive(false);
                }
            }

            if (nextHex != null) {
                if (GetNextHexData() != null) {
                    nextHex.hex.gameObject.SetActive(true);

                    nextHex.hex.ChangeModel(GetNextHexData(), BuildingCellDetection.lastCreatedCellRotation);

                    if (isNextHexQuest()) {
                        nextHex.hex.MakeQuestHex(GetNextHexQuest());
                    } else {
                        nextHex.hex.MakeNormalHex();
                    }
                } else {
                    nextHex.hex.gameObject.SetActive(false);
                }
            }

            if (textHexsLeft != null)
            {
                textHexsLeft.hexesLeftCounter.text = LeftHexsInDeck().ToString();           
            }
        }

        public QuestHexDataConfig FindRandomQuest(CellOfWorldData hexData, Dictionary<SideType, List<QuestHexDataConfig>> quests)
        {
            // Задаём стороны хексагона
            List<int> sidesNumber = new List<int>();
            
            for(int i = 0; i < 6; i++) {
                sidesNumber.Add(i);
            }

            // Перемешиваем стороны случайным образом
            for (int i = sidesNumber.Count- 1; i >= 1; i--) {
                System.Random rnd = new System.Random();
                
                int j = rnd.Next(i + 1);
                // обменять значения data[j] и data[i]
                var temp = sidesNumber[j];
                sidesNumber[j] = sidesNumber[i];
                sidesNumber[i] = temp;
            }

            HexSideData sideData = null;

            // Выбрать случайную сторону хекса
            for (int i = 0; i < 6; i++)
            {
                if (hexData.sidesData[(Side)sidesNumber[i]].sideData.type != SideType.Nothing)
                {
                    sideData = hexData.sidesData[(Side)sidesNumber[i]].sideData;
                    break;
                }               
            }

            if (sideData == null) return null;

            // Получить по типу стороны случайное задание
            if (quests.ContainsKey(sideData.type)) {
                return quests[sideData.type][UnityEngine.Random.Range(0, quests[sideData.type].Count)];
            } else {              
                return null;
            }
        }

        public void GenerateRandomHexesDeck()
        {
            hexesInDeck = new List<CellOfWorldData>();
            questsInDeck = new List<bool>();
            questsConfigsInDeck = new List<QuestHexDataConfig>();

            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
            {
                if (PlayerPrefs.HasKey("RemainsNumberHexes"))
                {
                    startHexAmount = PlayerPrefs.GetInt("RemainsNumberHexes");
                    hexsLeft = PlayerPrefs.GetInt("RemainsNumberHexes");
                }
            }

            AddHexes(startHexAmount);
        }

        public void ChangeCurrentHexIndex()
        {
            currentIndex++;

            if (currentIndex == 1)
            {
                if (isTutorial && GameSettings.Instance.currentMissionIndex == 1)
                {
                    Tutorial.Instance.NextScreen();
                }
            }

            //if (currentIndex % 5 == 0 && !isTutorial)
            //{
            //    AdsManager.ShowInterstitial();
            //}
        }

        public void ChangeCurrentHexIndex(HexOfWorld hex) {
            ChangeCurrentHexIndex();

            //SAVE
            if (GameSettings.Instance.currentGameMode == GameStyle.FreeMode)
            {
                hex.gameObject.AddComponent<SaveDataHex>();
                IncreasingCurrentHexIndex?.Invoke();
            }
        }

        public CellOfWorldData GetCurrentHexData()
        {
            if (currentIndex < hexesInDeck.Count) {
                
                return hexesInDeck[currentIndex];
            }

            return null;
        }

        public QuestHexDataConfig GetCurrentHexQuestConfig()
        {
            if (currentIndex < hexesInDeck.Count) {
                return questsConfigsInDeck[currentIndex];
            }

            return null;
        }

        public bool isCurrentHexQuest()
        {
            if (currentIndex < hexesInDeck.Count)
            {
                if (isTutorial && GameSettings.Instance.currentMissionIndex == 1 && LeftHexsInDeck() == startHexAmount)
                {
                    return questsInDeck[currentIndex];
                }
                else if (!isTutorial)
                {
                    return questsInDeck[currentIndex];
                }
            }

            return false;
        }

        public CellOfWorldData GetCurentHexDatafromPreview()
        {
            if (currentIndex < hexesInDeck.Count) {
                return currentHex.hex.data;
            }

            return null;
        }

        public CellOfWorldData GetNextHexData()
        {
            if (currentIndex + 1 < hexesInDeck.Count) {
                return hexesInDeck[currentIndex + 1];
            }

            return null;
        }

        public QuestHexDataConfig GetNextHexQuest()
        {
            if (currentIndex + 1 < hexesInDeck.Count) {
                return questsConfigsInDeck[currentIndex + 1];
            }

            return null;
        }

        public bool isNextHexQuest()
        {
            if (!isTutorial)
            {
                if (currentIndex + 1 < hexesInDeck.Count)
                {
                    return questsInDeck[currentIndex + 1];
                }
            }
            
            return false;
        }

        public int LeftHexsInDeck()
        {
            return hexesInDeck.Count - currentIndex;
        }        

        public void AddHexes(int amount)
        {           
            for (int i = 0; i < amount; i++)
            {
                CellOfWorldDataConfig randomHex = GetRandomDataConfig();
                QuestHexDataConfig quest;

                //не рандомное назначение гексов и квестов на уровни для туториала

                if (isTutorial && hexesAvailable.Count == amount)
                {
                    if (GameSettings.Instance.currentMissionIndex == 1 && questsInDeck.Count < 1)
                    {
                        quest = Tutorial.Instance.questTutorial;
                        questsInDeck.Insert(0, true);
                        questsConfigsInDeck.Insert(0, quest);
                        UpdateHexUIAndData();
                    }

                    for (int y = 0; y < hexesAvailable.Count; y++)
                    {
                        hexesInDeck.Add(hexesAvailable[i].data.Copy());
                        break;
                    }
                }
                else
                {
                    // Select random hex

                    if (randomHex != null)
                    {
                        hexesInDeck.Add(randomHex.data.Copy());
                    }
                    else
                    {
                        if (i > 0) i--;
                    }
                }

                AddQuestsInLevel(randomHex);             
            }

            UpdateHexUIAndData();

            CheckingStatusUIHexPreviewCurrentEvent?.Invoke();
        }
     
        private void AddQuestsInLevel(CellOfWorldDataConfig _randomHex)
        {
            QuestHexDataConfig randomQuest;

            randomQuest = FindRandomQuest(_randomHex.data.Copy(), quests);           

            if (!isTutorial)
            {
                numberSpacesBetweenQuests--;

                if (numberSpacesBetweenQuests == firstSpaces - INDEX_FIRST_HEX_WITH_QUEST)
                {
                    if (randomQuest != null)
                    {
                        questTypes.Add(randomQuest.questData.sideType);

                        questsInDeck.Add(true);
                        questsConfigsInDeck.Add(randomQuest);
                    }
                    else
                    {
                        questsInDeck.Add(false);
                        questsConfigsInDeck.Add(null);
                    }                  
                }
                else
                {
                    questsInDeck.Add(false);
                    questsConfigsInDeck.Add(null);
                }

                if (numberSpacesBetweenQuests == 0)
                {
                    numberSpacesBetweenQuests = startHexAmount / startQuestsAmount;
                }               
            }
        }

        private void AddLastHexInTutorialLevel1()
        {
            hexesInDeck.Add(hexesAvailable[hexesAvailable.Count - 1].data.Copy());
            UpdateHexUIAndData();
        }

        private void AddHexInTutorialLevel2()
        {
            hexesInDeck.Insert(1, hexesAvailable[hexesAvailable.Count - 2].data.Copy());
            UpdateHexUIAndData();
        }

        private void AddLastHexInTutorialLevel2(CellOfWorldDataConfig _lastHex)
        {
            hexesInDeck.Add(_lastHex.data.Copy());
            UpdateHexUIAndData();
        }

        private void AddHexInTutorialLevel9()
        {
            hexesInDeck.Insert(1, hexesAvailable[hexesAvailable.Count - 2].data.Copy());
            UpdateHexUIAndData();
        }

        private void AddLastHexInTutorialLevel9()
        {
            hexesInDeck.Insert(2, hexesAvailable[hexesAvailable.Count - 1].data.Copy());
            UpdateHexUIAndData();
        }

        public int GetSumWeight()
        {
            int result = 0;

            for(int i = 0; i < hexesAvailable.Count; i++) {
                if (hexesAvailable[i] != null && hexesAvailable[i].data != null) {
                    result += hexesAvailable[i].data.weight;
                }
            }

            return result;
        }

        public Dictionary<int, Vector2Int> GetHexsConfigDiapasones()
        {
            int currentMin = 0;

            Dictionary<int, Vector2Int> result = new Dictionary<int, Vector2Int>();

            for (int i = 0; i < hexesAvailable.Count; i++) {
                result.Add(i, new Vector2Int(currentMin, currentMin + hexesAvailable[i].data.weight));
                currentMin += hexesAvailable[i].data.weight;
            }

            return result;
        }

        public CellOfWorldDataConfig GetRandomDataConfig()
        {
            int sumWeight = GetSumWeight();
            int randomNumber = UnityEngine.Random.Range(0, sumWeight);

            Dictionary<int, Vector2Int> hexesDiapasone = GetHexsConfigDiapasones();

            for (int i = 0; i < hexesAvailable.Count; i++) {
                if (hexesDiapasone.ContainsKey(i)) {
                    if(randomNumber >= hexesDiapasone[i].x && 
                       randomNumber < hexesDiapasone[i].y) {
                        return hexesAvailable[i];
                    }
                }
            }

            return null;
        }

        public void AddBonusHexOnTop()
        {
            hexesInDeck.Insert(currentIndex, bonusHexPrefub.data.Copy());
            questsInDeck.Insert(currentIndex, false);
            questsConfigsInDeck.Insert(currentIndex, null);

            UpdateHexUIAndData();
        }

        public void RemoveBonusHexOnTop()
        {
            hexesInDeck.RemoveAt(currentIndex);
            questsInDeck.RemoveAt(currentIndex);
            questsConfigsInDeck.RemoveAt(currentIndex);

            UpdateHexUIAndData();
        }

        public void ReplaceTopHex()
        {
            if (isTutorial)
            {
                currentHex.hex.ChangeModel(Tutorial.Instance.hexToReplaceBonusButton.data.Copy(), currentHex.hex.transform.rotation);
                hexesInDeck[currentIndex] = Tutorial.Instance.hexToReplaceBonusButton.data.Copy();
                currentHex.hex.MakeNormalHex();
            }
            else
            {
                int randomIndex = UnityEngine.Random.Range(0, hexesAvailable.Count);

                if (hexesAvailable[randomIndex] != null)
                {
                    if (currentIndex < hexesInDeck.Count)
                    {

                        currentHex.hex.ChangeModel(hexesAvailable[randomIndex].data.Copy(),
                                               currentHex.hex.transform.rotation);

                        hexesInDeck[currentIndex] = hexesAvailable[randomIndex].data.Copy();

                        ////////////
                        ///
                        if (questsConfigsInDeck[currentIndex] != null)
                        {
                            questsInDeck[currentIndex] = false;

                            // int questIndex = questsConfigsInDeck.FindIndex(w => w = questsConfigsInDeck[currentIndex]);
                            int questIndex = currentIndex;

                            if (questIndex == 0)
                            {
                                questsConfigsInDeck[currentIndex] = questsConfigsInDeck[currentIndex + 1];
                            }
                            else
                            {
                                questsConfigsInDeck[currentIndex] = questsConfigsInDeck[currentIndex - 1];
                            }
                        }

                        currentHex.hex.MakeNormalHex();

                        ///////////
                        ///
                        // Из ОРИГИНАЛА

                        //int randomResult = Random.Range(0, 101);

                        //if (randomResult < questChance) //всегда выполняется, т.к questChance = 100 в инспекторе
                        //{
                        //    questsInDeck[currentIndex] = true;
                        //    questsConfigsInDeck[currentIndex] = FindRandomQuest(hexesAvailable[randomIndex].data.Copy(), quests);
                        //    Debug.Log(questsConfigsInDeck[currentIndex]);
                        //}
                        //else
                        //{
                        //    questsInDeck[currentIndex] = false;
                        //    questsConfigsInDeck[currentIndex] = null;
                        //}

                        //if (questsInDeck[currentIndex])
                        //{
                        //    currentHex.hex.MakeQuestHex(questsConfigsInDeck[currentIndex]);
                        //}
                        //else
                        //{
                        //    currentHex.hex.MakeNormalHex();
                        //}
                    }
                }
            }
        }
    }
}
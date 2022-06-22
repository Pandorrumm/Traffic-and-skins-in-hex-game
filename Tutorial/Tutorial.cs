using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameEntity;
using System;
using GameData;
using Singleton;
using GameLogic;
using DG.Tweening;

namespace GameUI
{
    public class Tutorial : Singleton<Tutorial>
    {
        public bool isTutorial;

        public int currentScreen = 0;
        [Space]
        public int screenIndexBeforeMistakeInLevel1;
        public int screenIndexBeforeMistakeInLevel2_1;
        public int screenIndexBeforeMistakeInLevel2_2;
        public int screenIndexBeforeMistakeInLevel9_1;
        public int screenIndexBeforeMistakeInLevel9_2;
        [Space]
        public int screenIndexFirstPopupLevel2;
        [Space]
        public int indexFirstLevelWithRiver = 8;
        [Space]
        [SerializeField] private GameObject blockBonusButtonPanel = null;
        [SerializeField] private GameObject blockTopButtons = null;
        [Space]
        public QuestHexDataConfig questTutorial;
        public CellOfWorldDataConfig hexToReplaceBonusButton;
        [Space]
        [SerializeField] private GameObject bonusButtonsImages = null;
        //[Space]
        //[SerializeField] private GameObject touchCameraControl = null;
        [Space]
        public GameObject[] screens;

        private int indexMistake1 = 0;
        private int indexMistake2 = 0;
        private int indexMistake3 = 0;
        private int indexMistake4 = 0;

        public static Action AddLastHexInTutorialLevel1Event;
        public static Action AddHexInTutorialLevel2Event;
        public static Action<CellOfWorldDataConfig> AddLastHexInTutorialLevel2Event;
        public static Action AddHexInTutorialLevel9Event;
        public static Action AddLastHexInTutorialLevel9Event;
        public static Action ReplaceTopHexInTutorialEvent;
        public static Action OpenPopupGlobalMissionsWhenWinningEvent;

        private void OnEnable()
        {
            ChainDetection.RepeatInstallationHexEvent += RepeatInstallationHex;
            SingletonHexCollection.UpdateIsTutorialEvent += UpdateStatusIsTutorial;
        }

        private void OnDisable()
        {
            ChainDetection.RepeatInstallationHexEvent -= RepeatInstallationHex;
            SingletonHexCollection.UpdateIsTutorialEvent -= UpdateStatusIsTutorial;
        }

        private void Awake()
        {
            if (HasInstance)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                InitInstance();
            }

            UpdateStatusIsTutorial();
        }

        private void UpdateStatusIsTutorial()
        {          
            isTutorial = PlayerPrefs.GetInt("IsTutorial", 1) > 0;
            StartTutorial();
        }

        public void StartTutorial()
        {
            if (isTutorial)
            {
                if (GameSettings.Instance.currentMissionIndex == indexFirstLevelWithRiver)
                {
                    currentScreen = PlayerPrefs.GetInt("TutorialCurrentScreen");
                    NextScreen();
                }
                else
                {
                   // PlayerPrefs.SetInt("IsTutorial", 0);
                   // PlayerPrefs.SetInt("TutorialCurrentScreen", Tutorial.Instance.screenIndexBeforeMistakeInLevel2_2 + 1);

                    currentScreen = 0;
                }
               
                screens[currentScreen].SetActive(true);

                blockBonusButtonPanel.SetActive(true);
                blockTopButtons.SetActive(true);

                //touchCameraControl.SetActive(false);
            }
            // AnalyticsEventer.AddLog("Tutorial_Start");
        }

        public void CloseCurrent()
        {
            if (currentScreen < screens.Length)
                screens[currentScreen].SetActive(false);
        }

        public void NextScreen()
        {
            CloseCurrent();

            if (isTutorial)
            {
                currentScreen++;               

                if (currentScreen == screenIndexFirstPopupLevel2)
                {
                    blockBonusButtonPanel.SetActive(false);
                }

                if (currentScreen == screenIndexBeforeMistakeInLevel2_2 - 1)
                {
                    blockBonusButtonPanel.SetActive(true);
                }

                if (GameSettings.Instance.currentMissionIndex == 1)
                {
                    bonusButtonsImages.SetActive(true);
                }

                if (currentScreen >= screens.Length)
                {
                    EndTutorial();
                }
                else
                {
                    screens[currentScreen].SetActive(true);
                }
            }
        }

        private void RepeatInstallationHex(HexOfWorld _lastHex)
        {
            if (GameSettings.Instance.currentMissionIndex == 0)
            {
                indexMistake1++;

                if (indexMistake1 > 1)
                {
                    currentScreen--;
                }

                AddLastHexInTutorialLevel1Event?.Invoke();
            }

            if (GameSettings.Instance.currentMissionIndex == 1)
            {
                if (SingletonHexCollection.Instance.LeftHexsInDeck() > 0)
                {
                    indexMistake2++;

                    if (indexMistake2 > 1)
                    {
                        currentScreen--;
                    }

                    AddHexInTutorialLevel2Event?.Invoke();
                }
                else
                {
                    indexMistake3++;

                    if (indexMistake3 > 1)
                    {
                        currentScreen--;
                    }

                    AddLastHexInTutorialLevel2Event?.Invoke(hexToReplaceBonusButton);
                }
            }

            if (GameSettings.Instance.currentMissionIndex == 8)
            {
                if (SingletonHexCollection.Instance.LeftHexsInDeck() > 0)
                {
                    indexMistake2++;

                    if (indexMistake2 > 1)
                    {
                        currentScreen--;
                    }

                    AddHexInTutorialLevel9Event?.Invoke();
                }
                else if (SingletonHexCollection.Instance.LeftHexsInDeck() == 0)
                {
                    indexMistake4++;

                    if (indexMistake4 > 1)
                    {
                        currentScreen--;
                    }

                    AddLastHexInTutorialLevel9Event?.Invoke();
                }
            }

            NextScreen();
            Destroy(_lastHex.gameObject);
        }

        public void EndTutorialLevel()
        {          
            OpenPopupGlobalMissionsWhenWinningEvent?.Invoke();
        }

        public void ReplaceTopHexInTutorial()
        {
            ReplaceTopHexInTutorialEvent?.Invoke();
        }

        public void EndTutorial()
        {
            isTutorial = false;
            blockBonusButtonPanel.SetActive(false);
            blockTopButtons.SetActive(false);
            bonusButtonsImages.SetActive(false);
            
            if (GameSettings.Instance.currentMissionIndex == 1)
            {
                PlayerPrefs.SetInt("IsTutorial", 0);
            }
            else
            {
                PlayerPrefs.SetInt("IsTutorial", -1);
            }
            
            PlayerPrefs.SetInt("TutorialCurrentScreen", currentScreen);

            // AnalyticsEventer.AddLog("Tutorial_Finish");
        }
    }
}


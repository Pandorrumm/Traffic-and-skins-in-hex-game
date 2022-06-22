using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class LevelSelector : MonoBehaviour
    {
        [SerializeField] private Button[] levelButtons = null;
        [Space]
        [SerializeField] private Sprite[] closedLevel = null;
        [SerializeField] private Sprite[] openLevel = null;
        [SerializeField] private Sprite currentLevel = null;

        private int levelsUnlocked;

        private void Start()
        {
            levelsUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);
            //24 - levelButtons.Length
            for (int i = 0; i < levelButtons.Length; i++)
            {
                levelButtons[i].interactable = false;
                levelButtons[i].GetComponent<UIButtonMission>().missionImage.sprite = closedLevel[RandomSelectionClosedLevelSprites()];

            }

            for (int i = 0; i < levelsUnlocked; i++)
            {
                levelButtons[i].interactable = true;

                levelButtons[i].GetComponent<UIButtonMission>().missionImage.sprite = openLevel[RandomSelectionOpenLevelSprites()];
                levelButtons[levelsUnlocked - 1].GetComponent<UIButtonMission>().missionImage.sprite = currentLevel;
            }
        }

        private int RandomSelectionClosedLevelSprites()
        {
            int randomIndex = Random.Range(0, closedLevel.Length);
            return randomIndex;
        }

        private int RandomSelectionOpenLevelSprites()
        {
            int randomIndex = Random.Range(0, openLevel.Length);
            return randomIndex;
        }
    }
}


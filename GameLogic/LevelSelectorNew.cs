using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class LevelSelectorNew : MonoBehaviour
    {
        [SerializeField] private BoxCollider[] levelsColliders = null;
        [Space]
        [SerializeField] private Image[] imagesOfLevelIcons = null;
        [Space]
        [SerializeField] private Sprite closedLevel = null;
        [SerializeField] private Sprite openLevel = null;

        private int levelsUnlocked;

        private void Start()
        {
            levelsUnlocked = PlayerPrefs.GetInt("LevelsUnlocked", 1);

            //24 - levelButtons.Length
            for (int i = 0; i < levelsColliders.Length; i++)
            {
               // levelsColliders[i].enabled = false;
                imagesOfLevelIcons[i].sprite = closedLevel;

            }

            for (int i = 0; i < levelsUnlocked; i++)
            {
               // levelsColliders[i].enabled = true;
                imagesOfLevelIcons[i].sprite = openLevel;
            }
        }

        //private int RandomSelectionClosedLevelSprites()
        //{
        //    int randomIndex = Random.Range(0, closedLevel.Length);
        //    return randomIndex;
        //}

        //private int RandomSelectionOpenLevelSprites()
        //{
        //    int randomIndex = Random.Range(0, openLevel.Length);
        //    return randomIndex;
        //}
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameUI
{
    public class UIBackgroundHideTutorialScreen : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Tutorial.Instance.NextScreen();
        }
    }
}

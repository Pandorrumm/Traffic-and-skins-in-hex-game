using UnityEngine;
using Singleton;

namespace Assets.Scripts.GameLogic
{
    public class ReInitializeHexCollection : MonoBehaviour
    {
        private void Awake()
        {
            SingletonHexCollection.Instance.Init();
        }
    }
}

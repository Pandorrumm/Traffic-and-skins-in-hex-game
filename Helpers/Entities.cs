using UnityEngine;

namespace Helpers
{
    public class Entities<T> where T : MonoBehaviour
    {
        public static void DestroyAllObjectsOnScene()
        {
            T[] objects = Object.FindObjectsOfType<T>();

            foreach(T obj in objects) {
                Object.Destroy(obj.gameObject);
            }
        }
    }
}

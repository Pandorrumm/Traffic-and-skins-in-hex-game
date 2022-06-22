using UnityEngine;

namespace Helpers
{
    public static class TransformEx
    {
        /// <summary>
        /// Remove all child GameObjects from transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>This Transform</returns>
        public static Transform Clear(this Transform transform)
        {
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
            return transform;
        }
    }
}

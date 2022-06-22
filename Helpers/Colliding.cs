using UnityEngine;
using InputSystem;
using System;

namespace Helpers
{
    public static class Colliding
    {
        /// <summary>
        /// Получить первый объект с которым возможны взаимодействия при клике или таче
        /// </summary>
        /// <param name="worldCoordinates">Мировые координаты поиска объекта</param>
        /// <returns></returns>
        public static IInteractble GetFirstInteractbleObject(Vector3 worldCoordinates)
        {
            Collider2D[] allGameObjects = Physics2D.OverlapPointAll(worldCoordinates);
            IInteractble interactZoneObject = null;

            foreach (Collider2D col in allGameObjects) {
                interactZoneObject = col.GetComponent<IInteractble>();

                if (interactZoneObject != null) {
                    return interactZoneObject;
                }
            }

            return interactZoneObject;
        }
    }

    public static class Colliding<T> where T : UnityEngine.Object {
        /// <summary>
        /// Получить первый объект с которым возможны взаимодействия при клике или таче нужного типа
        /// </summary>
        /// <param name="cameraMain">Текущая камера</param>
        /// <param name="screenPosition">Координаты клика или тача</param>
        /// <returns></returns>
        public static T GetFirstInteractbleObjectInScreenPosition(Camera cameraMain, Vector2 screenPosition)
        {
            if (screenPosition.Equals(Vector2.zero)) return null;

            Vector3 worldCoordinates = Coordinates.GetWorldCoordinatesFromScreenCoordinates(cameraMain, screenPosition);
            T result = Colliding<T>.GetFirstInteractbleObjectInWorldPosition(worldCoordinates);

            return result;
        }

        /// <summary>
        /// Получить первый объект с которым возможны взаимодействия при клике или таче
        /// </summary>
        /// <param name="worldCoordinates">Мировые координаты поиска объекта</param>
        /// <param name="type">Тип искомого объекта</param>
        /// <returns></returns>
        public static T GetFirstInteractbleObjectInWorldPosition(Vector3 worldCoordinates)
        {
            Collider2D[] allGameObjects = Physics2D.OverlapPointAll(worldCoordinates);
            T interactZoneObject = null;

            foreach (Collider2D col in allGameObjects) {
                interactZoneObject = col.GetComponent<T>();

                if (interactZoneObject != null) {
                    return interactZoneObject;
                }
            }

            return interactZoneObject;
        }
    }
}

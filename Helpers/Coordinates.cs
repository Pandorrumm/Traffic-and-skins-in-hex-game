using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Helpers
{
    public static class Coordinates
    {
        /// <summary>
        /// Получить мировые координаты в точке экрана, например позиция курсора или элемента UI
        /// </summary>
        /// <param name="cameraMain">Текущая камера</param>
        /// <param name="screenPosition">Позиция точки на экране</param>
        /// <returns></returns>
        public static Vector3 GetWorldCoordinatesFromScreenCoordinates(Camera cameraMain, Vector2 screenPosition)
        {
            Vector3 screenCoordinates = new Vector3(screenPosition.x, screenPosition.y, cameraMain.nearClipPlane);
            Vector3 worldCoordinates = cameraMain.ScreenToWorldPoint(screenCoordinates);

            //Debug.Log(cameraMain.nearClipPlane);
            worldCoordinates.z = 0;

            return worldCoordinates;
        }

        /// <summary>
        /// Получить мировые координаты по позиции UI элемента на экране, ВНИМАНИЕ! Не работает с RectTransform которые находятся в
        /// VerticalLayoutGroup или HorisontalLayoutGroup особенность Unity
        /// </summary>
        /// <param name="rect">UI элемент</param>
        /// <returns>Позиция в мире</returns>
        public static Vector3 GetWorldCoordinatesFromUIElement(RectTransform rect)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(rect.transform.position);
            return worldPos;
        }

        public static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            return new Rect((Vector2)transform.position - (size * transform.pivot), size);
        }

        /// <summary>
        /// Получить координаты в радиусе круга от центра
        /// </summary>
        /// <param name="center">Центральная позиция</param>
        /// <param name="distance">Расстояние от центра, радиус</param>
        /// <param name="elementsCount">Количество точек</param>
        /// <param name="yOffset">Высота относительно центра</param>
        /// <returns>Список точек по радиусу от центра</returns>
        public static List<Vector3> GetRoundPositions(Vector3 center, float distance, int elementsCount, float yOffset)
        {
            List<Vector3> result = new List<Vector3>();

            float slice = 2 * Mathf.PI / elementsCount;
            for (int i = 0; i < elementsCount; i++) {
                float angle = slice * i;
                float newX = center.x + distance * Mathf.Cos(angle);
                float newZ = center.z + distance * Mathf.Sin(angle);
                Vector3 p = new Vector3(newX, center.y + yOffset, newZ);
                result.Add(p);
            }

            return result;
        }
    }
}

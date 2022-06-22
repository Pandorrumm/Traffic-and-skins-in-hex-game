using UnityEngine;
using static GameEntity.HexOfWorld;

namespace Helpers
{
    public class SideHelper
    {
        /// <summary>
        /// Получить прилегающую сторону у соседнего хексагона
        /// </summary>
        /// <param name="currentSide">Текущая сторона</param>
        /// <returns></returns>
        public static Side GetAdjacedSide(Side currentSide) {
            switch (currentSide) {
                case Side.TopLeft:
                    return Side.BottomRight;
                    #pragma warning disable CS0162 // Обнаружен недостижимый код
                    break;
                    #pragma warning restore CS0162 // Обнаружен недостижимый код
                case Side.TopRight:
                    return Side.BottomLeft;
                    #pragma warning disable CS0162 // Обнаружен недостижимый код
                    break;
                    #pragma warning restore CS0162 // Обнаружен недостижимый код
                case Side.Left:
                    return Side.Right;
                    #pragma warning disable CS0162 // Обнаружен недостижимый код
                    break;
                    #pragma warning restore CS0162 // Обнаружен недостижимый код
                case Side.Right:
                    return Side.Left;
                    #pragma warning disable CS0162 // Обнаружен недостижимый код
                    break;
                    #pragma warning restore CS0162 // Обнаружен недостижимый код
                case Side.BottomLeft:
                    return Side.TopRight;
                    #pragma warning disable CS0162 // Обнаружен недостижимый код
                    break;
                    #pragma warning restore CS0162 // Обнаружен недостижимый код
                case Side.BottomRight:
                    return Side.TopLeft;
                    #pragma warning disable CS0162 // Обнаружен недостижимый код
                    break;
                    #pragma warning restore CS0162 // Обнаружен недостижимый код
            }

            Debug.LogError("Unexpected error!! SideHelper");

            return Side.BottomLeft;
        }
    }
}

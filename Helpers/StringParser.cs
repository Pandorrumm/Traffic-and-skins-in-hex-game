using System;
using System.Globalization;

namespace Helpers
{
    public static class StringParser
    {
        public static string GetParcedNumber(ulong number)
        {
            double digitsCount = Math.Floor(Math.Log10(number) + 1);

            if (digitsCount <= 3) {
                return number.ToString();
            } else if (digitsCount >= 4 && digitsCount <= 6) {
                return (number / 1000f).ToString("F1", CultureInfo.InvariantCulture) + "k";
            } else if (digitsCount >= 7 && digitsCount <= 9) {
                return (number / 1000000f).ToString("F1", CultureInfo.InvariantCulture) + "m";
            } else if (digitsCount >= 10 && digitsCount <= 12) {
                return (number / 1000000000f).ToString("F1", CultureInfo.InvariantCulture) + "B";
            } else if (digitsCount >= 13 && digitsCount <= 15) {
                return (number / 1000000000000f).ToString("F1", CultureInfo.InvariantCulture) + "Aa";
            } else if (digitsCount >= 16 && digitsCount <= 18) {
                return (number / 1000000000000000f).ToString("F1", CultureInfo.InvariantCulture) + "Bb";
            } else if (digitsCount >= 19) {
                return (number / 1000000000000000000f).ToString("F1", CultureInfo.InvariantCulture) + "Cc";
            }

            return number.ToString();
        }
    }
}

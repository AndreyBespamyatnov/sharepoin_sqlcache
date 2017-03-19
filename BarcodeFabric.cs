namespace Navicon.SP.Components.SqlCache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.SharePoint;

    /// <summary>
    ///     Класс для создания штрих кодов и их валидации, так же с помошью метода Normalize, возможно привести штрихкод в
    ///     полный 14 значный вид
    /// </summary>
    public static class BarcodeFabric
    {
        public enum SourceSystemType
        {
            None = 0,
            Sharepoint = 2,
            Navision = 1
        }

        public static long Parse(string barcodeValue)
        {
            barcodeValue = Regex.Replace(barcodeValue, @"[^\d]", "");

            long barcode;
            if (long.TryParse(barcodeValue, out barcode))
            {
                return barcode;
            }

            throw new Exception("Не удалось преобразовать строку баркода в число.");
        }

        public static bool TryParse(string barcodeValue, out long barcode)
        {
            barcodeValue = Regex.Replace(barcodeValue, @"[^\d]", "");
            return long.TryParse(barcodeValue, out barcode);
        }

        public static string Create(long unicId)
        {
            return Create(SourceSystemType.Sharepoint, unicId);
        }

        public static string Create(SourceSystemType sourceSystem, long unicId)
        {
            // 0 - 000000123 - идентификатор объекта
            // 1 - 00 - код юр лица, всегда 00
            // 2 - 01 - код системы от куда пришёл - SourceSystemType
            // 3 - контрольная цифра
            // Алгоритм вычисления контрольной цифры:
            // Начиная с первой значимой цифры(>0), каждой позиции последующей цифры присваивается вес по порядку начиная с 1.
            // Вычисляется сумма произведений каждой цифры и веса. Последняя цифра в полученной сумме является контрольной цифрой.
            // 1*1 + 5*2 + 7*3 + 0*4 + 0*5 + 0*6 + 1*7=39 – контрольный символ  - 9
            const string template = "{0}{1}{2}";
            const string legalEntity = "00";

            string id = unicId.ToString();
            string systemCode = ((long) sourceSystem).ToString("D2");

            string barcode = String.Format(template, id, legalEntity, systemCode);
            long barcodeNumber = long.Parse(barcode);

            long[] barcodeNumbers = ToDigitArray(barcodeNumber);
            long checkDigit = barcodeNumbers.Select((t, i) => t * (i + 1)).Sum() % 10;
            barcode += checkDigit;
            return barcode;
        }

        public static bool Validate(SPSite spSite, long barcode)
        {
            return Validate(spSite, barcode.ToString());
        }

        public static bool Validate(SPSite spSite, string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    return false;
                }

                int checkDigitPosition = barcode.Length - 1;
                string sourceCheckDigit = barcode.Substring(checkDigitPosition);

                long checkDigitNumber;
                if (!long.TryParse(sourceCheckDigit, out checkDigitNumber))
                {
                    return false;
                }

                long barcodeNumber;
                if (!long.TryParse(barcode.Remove(checkDigitPosition), out barcodeNumber))
                {
                    return false;
                }

                long[] barcodeNumbers = ToDigitArray(barcodeNumber);

                long checkDigit = barcodeNumbers.Select((t, i) => t * (i + 1)).Sum() % 10;

                if (checkDigit != checkDigitNumber)
                {
                    return false;
                }

                // (валидация по сумме И
                // проверить то что не шарик) ИЛИ
                // если из ширика то проверить что id не больше существующего последнего.
                if (GetSourceSystemType(barcode) != SourceSystemType.Sharepoint)
                {
                    return true;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static long[] ToDigitArray(long n)
        {
            if (n == 0)
            {
                return new long[] {0};
            }

            List<long> digits = new List<long>();

            for (; n != 0; n /= 10)
            {
                digits.Add(n % 10);
            }

            long[] arr = digits.ToArray();
            Array.Reverse(arr);
            return arr;
        }

        private static SourceSystemType GetSourceSystemType(string barcode)
        {
            string sourceSystemStringCode = barcode.Substring(barcode.Length - 3, 2);
            int sourceSystemCode;
            if (!int.TryParse(sourceSystemStringCode, out sourceSystemCode))
            {
                return SourceSystemType.None;
            }

            SourceSystemType sourceSystemType = (SourceSystemType) sourceSystemCode;
            return sourceSystemType;
        }
    }
}
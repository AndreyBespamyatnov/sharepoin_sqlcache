namespace Navicon.SP.Components.SqlCache
{
    using System;

    public class Barcodes
    {
        public static string FormatBarcode(string shortBarcode)
        {
            Int64 barcode = 0;
            Int64.TryParse(shortBarcode, out barcode);
            shortBarcode = barcode.ToString("D14");
            return shortBarcode;
        }

        public static string FormatItemID(string shortId)
        {
            Int64 barcode = 0;
            Int64.TryParse(shortId, out barcode);
            shortId = barcode.ToString("D8");
            return shortId;
        }

        public static string TrimStartBarcode(string fullBarcode)
        {
            if (!NavisionBarcode(fullBarcode))
            {
                //Убираем ведущие нули если это бар-код НЕ из Navision
                return fullBarcode.TrimStart('0');
            }
            return fullBarcode;
        }

        public static bool NavisionBarcode(string fullBarcode)
        {
            int length = fullBarcode.Length;
            string systemId = fullBarcode.Substring(length - 3, 2);
            if (systemId == "01")
            {
                //Navision
                return true;
            }
            //"02" - SharePoint
            return false;
        }
    }
}

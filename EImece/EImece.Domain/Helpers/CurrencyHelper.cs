using System;
using System.Globalization;

namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        private const string CulturaInfoName = "tr-TR";

        // Method to convert Price to Google Product Schema format (price with currency)
        public static string GoogleProductSchema(this decimal price)
        {
            // Format the price with 2 decimal places and ensure period as decimal separator
            // This uses invariant culture to guarantee period as decimal separator
            return price.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string CurrencySignForIyizo(this decimal price)
        {
            return CurrencySign(price).Replace(".", "").Replace("₺", "").Replace(",", ".").Trim();
        }

        public static string CurrencySign(this decimal price)
        {
            decimal v = RoundPriceNumber(price);
            var culture = new CultureInfo(CulturaInfoName);

            if (v > 0)
            {
                var result = v.ToString("N2", culture); // N2: Sayıyı her zaman iki ondalıklı olarak gösterir.
                return $"{result} ₺";
            }
            else
            {
                return "0 ₺";
            }
        }

        public static string ToDecimalToStringConvert(decimal price)
        {
            var item = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
            //  var culture = Thread.CurrentThread.CurrentUICulture.ToString();
            var culture = CultureInfo.CreateSpecificCulture(CulturaInfoName).ToString();
            return item.ToString("#,##", new CultureInfo(culture));
        }

        public static decimal RoundPriceNumber(decimal price)
        {
            var price2 = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
            return System.Convert.ToDecimal(price2);
        }

        public static string CurrencySign(this double price)
        {
            var cultureInfo = new CultureInfo(CulturaInfoName);
            decimal v = System.Convert.ToDecimal(price);
            if (v > 0)
                return string.Format("{0} TL", v.ToString("#,##", cultureInfo));
            else
                return "0 TL";
        }

        public static string CurrencySign(this int price)
        {
            var cultureInfo = new CultureInfo(CulturaInfoName);
            decimal v = System.Convert.ToDecimal(price);
            if (v > 0)
                return string.Format("{0} TL", v.ToString("#,##", cultureInfo));
            else
                return "0 TL";
        }
    }
}
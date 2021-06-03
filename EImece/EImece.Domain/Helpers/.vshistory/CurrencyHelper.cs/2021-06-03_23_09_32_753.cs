using System;
using System.Globalization;
using System.Threading;

namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        private const string CulturaInfoName = "tr-TR";

        public static string CurrencySign(this decimal price)
        {
            var countryCode = Thread.CurrentThread.CurrentUICulture.ToString();
            var cultureInfo = new CultureInfo(countryCode);
            decimal v = RoundPriceNumber(price);
            if (v > 0)
            {
                var result = v.ToString("#,##", cultureInfo);
                return string.Format("{0} TL", result);
            }
            else
            {
                return "0 TL";
            }
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
using System;
using System.Globalization;
using System.Threading;

namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        private const string CulturaInfoName = "en-US";

        public static string CurrencySign(this decimal price)
        {
            var cultureInfo = new CultureInfo(CulturaInfoName);
            //return price.ToString("C");
            var price2 = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
            return string.Format("{0} TL", System.Convert.ToDecimal(price2).ToString("#,##", cultureInfo));
        }

        public static string CurrencySign(this double price)
        {
            var cultureInfo = new CultureInfo(CulturaInfoName);
            return string.Format("{0} TL", System.Convert.ToDecimal(price).ToString("#,##", cultureInfo));
            //  return price.ToString("C");
        }

        public static string CurrencySign(this int price)
        {
            var cultureInfo = new CultureInfo(CulturaInfoName);
            return string.Format("{0} TL", System.Convert.ToDecimal(price).ToString("#,##", cultureInfo));
           // return price.ToString("C");
        }
    }
}
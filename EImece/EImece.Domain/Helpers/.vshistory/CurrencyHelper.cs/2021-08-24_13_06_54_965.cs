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
            decimal v = RoundPriceNumber(price);
            if (v > 0)
            {
                var result = v.ToString("#,##", new CultureInfo(Thread.CurrentThread.CurrentUICulture.ToString()));
                return string.Format("{0} TL", result);
            }
            else
            {
                return "0 TL";
            }
        }
        public static string ToDecimalToStringConvert(decimal price)
        {
            var item = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
          //  var culture = Thread.CurrentThread.CurrentUICulture.ToString();
            var culture = CultureInfo.CreateSpecificCulture("tr-TR");
            return item.ToString("#,##", new CultureInfo(culture)).Replace(",", ".");
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
using System;
using System.Globalization;
using System.Threading;

namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        public static string CurrencySign(this decimal price)
        {
            //return price.ToString("C");
            var price2 = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
            return string.Format("{0} TL", System.Convert.ToDecimal(price2).ToString("#,##"));
        }

        public static string CurrencySign(this double price)
        {
         
           var cultureInfo = new CultureInfo("en-US");
            return string.Format("{0} TL", System.Convert.ToDecimal(price).ToString("#,##", cultureInfo));
            //  return price.ToString("C");
        }

        public static string CurrencySign(this int price)
        {
            return string.Format("{0} TL", System.Convert.ToDecimal(price).ToString("#,##"));
           // return price.ToString("C");
        }
    }
}
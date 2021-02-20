using System;

namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        public static string CurrencySign(this decimal price)
        {
            //return price.ToString("C");
            return string.Format("{0} TL", decimal.Round(price, 2, MidpointRounding.AwayFromZero));
        }

        public static string CurrencySign(this double price)
        {
            return string.Format("{0} TL", price);
            //  return price.ToString("C");
        }

        public static string CurrencySign(this int price)
        {
            return string.Format("{0} TL", decimal.Round(price, 2, MidpointRounding.AwayFromZero));
           // return price.ToString("C");
        }
    }
}
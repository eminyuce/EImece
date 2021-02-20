namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        public static string CurrencySign(this decimal price)
        {
            //return price.ToString("C");
            return string.Format("{0} TL", price);
        }

        public static string CurrencySign(this double price)
        {
            return price.ToString("C");
        }

        public static string CurrencySign(this int price)
        {
            return price.ToString("C");
        }
    }
}
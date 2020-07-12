using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers
{
    public static class CurrencyHelper
    {
        public static string CurrencySign(this double price)
        {
            return price.ToString("C2");
        }
    }
}

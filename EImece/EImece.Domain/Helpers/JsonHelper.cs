using EImece.Domain.Models.FrontModels;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers
{
    public class JsonHelper
    {
        public static string PrettyJsonFormatter(string obj)
        {
            try
            {
                return JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ShoppingCartSession>(obj), Formatting.Indented);
            }
            catch(Exception ex)
            {

            }
            return obj;

        }
    }
}

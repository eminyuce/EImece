using EImece.Domain.Models.FrontModels;
using Newtonsoft.Json;
using System;

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
            catch (Exception ex)
            {
                // Intentionally ignored: PrettyJsonFormatter returns the original string when JSON is not a ShoppingCartSession.
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return obj;

        }
    }
}

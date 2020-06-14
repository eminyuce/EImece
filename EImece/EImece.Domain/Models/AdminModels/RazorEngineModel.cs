using EImece.Domain.Helpers;
using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class RazorEngineModel
    {
        private Dictionary<string, string> _params = new Dictionary<string, string>();

        public RazorEngineModel()
        {
        }

        public bool HasKey(string name)
        {
            name = name.ToStr().Trim().ToLower();
            return _params.ContainsKey(name);
        }

        public string this[string name]
        {
            get
            {
                name = name.ToStr().Trim().ToLower();
                if (_params.ContainsKey(name))
                {
                    return _params[name];
                }
                return string.Empty;
            }
            set
            {
                name = name.ToStr().Trim().ToLower();
                _params[name] = value;
            }
        }
    }
}
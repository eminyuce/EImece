using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle
{
    public class Il
    {
        public string id { get; set; }
        public string il_adi { get; set; }
    }

    public class Iller
    {
        public List<Il> il { get; set; }
    }

    public class IlRoot
    {
        public Iller Iller { get; set; }

    }
}

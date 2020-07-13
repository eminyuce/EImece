using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle
{
    public class Ilce
    {
        public int id { get; set; }
        public int il_id { get; set; }
        public string ilce_adi { get; set; }
    }

    public class Ilceler
    {
        public List<Ilce> ilce { get; set; }
    }
    public class IlceRoot
    {
        public Ilceler ilceler { get; set; }

    }

}

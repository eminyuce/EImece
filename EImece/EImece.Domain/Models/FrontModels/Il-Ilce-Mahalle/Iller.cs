using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle
{
    public class Il
    {
        public int id { get; set; }
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

    public class TurkiyeAdres
    {
        public IlRoot IlRoot { get; set; }
        public IlceRoot IlceRoot { get; set; }
    }
}
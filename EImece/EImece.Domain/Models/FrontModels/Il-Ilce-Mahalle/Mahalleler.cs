using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels.Il_Ilce_Mahalle
{
    public class Mahalle
    {
        public string id { get; set; }
        public string semt_id { get; set; }
        public string mahalle_adi { get; set; }
        public string posta_kodu { get; set; }

    }

    public class Mahalleler
    {
        public List<Mahalle> Mahalle { get; set; }

    }

    public class MahalleRoot
    {
        public Mahalleler Mahalleler { get; set; }

    }
    public class TurkiyeAdres
    {
        public MahalleRoot MahalleRoot { get; set; }
        public IlceRoot IlceRoot { get; set; }
        public IlRoot IlRoot { get; set; }

    }

}

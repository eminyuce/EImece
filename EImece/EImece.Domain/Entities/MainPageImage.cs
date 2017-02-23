using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Resources;


namespace EImece.Domain.Entities
{
    public class MainPageImage : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Link))]
        public string Link { get; set; }

     

    }
}

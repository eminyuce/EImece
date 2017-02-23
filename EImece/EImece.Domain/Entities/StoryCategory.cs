using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Resources;

namespace EImece.Domain.Entities
{
    public class StoryCategory : BaseContent
    {
        public  ICollection<Story> Stories { get; set; }

        public String PageTheme { get; set; }
    }
}

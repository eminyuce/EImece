using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class StoryCategory : BaseContent
    {
        public  ICollection<Story> Stories { get; set; }

        public String PageTheme { get; set; }
    }
}

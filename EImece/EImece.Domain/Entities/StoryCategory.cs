using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class StoryCategory : BaseContent
    {
        public virtual List<Story> Stories { get; set; }
    }
}

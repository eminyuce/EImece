using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Entities
{
    public class Story : BaseContent
    {
        public int StoryCategoryId { get; set; }
        public bool MainPage { get; set; }

        public virtual StoryCategory StoryCategory { get; set; }
    }
}

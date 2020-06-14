using System;
using System.Collections.Generic;

namespace EImece.Domain.Entities
{
    public class StoryCategory : BaseContent
    {
        public ICollection<Story> Stories { get; set; }

        public String PageTheme { get; set; }
    }
}
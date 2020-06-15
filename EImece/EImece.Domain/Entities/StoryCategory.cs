using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class StoryCategory : BaseContent
    {
        public ICollection<Story> Stories { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PageTheme))]
        public String PageTheme { get; set; }
    }
}
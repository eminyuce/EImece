using Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    public class Story : BaseContent
    {
        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.StoryCategoryIdErrorMessage))]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.StoryCategoryId))]
        public int StoryCategoryId { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public bool MainPage { get; set; }

        public StoryCategory StoryCategory { get; set; }
        public ICollection<StoryTag> StoryTags { get; set; }
        public ICollection<StoryFile> StoryFiles { get; set; }


    }
}

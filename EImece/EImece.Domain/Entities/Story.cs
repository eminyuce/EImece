using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Story : BaseContent
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.StoryCategoryIdErrorMessage))]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.StoryCategoryId))]
        public int StoryCategoryId { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public bool MainPage { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.AuthorName))]
        public string AuthorName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.IsFeaturedStory))]
        public bool IsFeaturedStory { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.ShortDescription))]
        public string ShortDescription { get; set; }


        public StoryCategory StoryCategory { get; set; }
        public ICollection<StoryTag> StoryTags { get; set; }
        public ICollection<StoryFile> StoryFiles { get; set; }

        // Kept for Razor view compatibility — canonical storefront logic lives in StorefrontStoryDetailDto.DetailPageUrl
        [NotMapped]
        public string DetailPageUrl
        {
            get
            {
                return this.GetDetailPageUrl("Detail", "Stories", StoryCategory != null ? StoryCategory.Name : "no_category", "", "");
            }
        }

        // Kept for Razor view compatibility — canonical storefront logic lives in StorefrontStoryCardDto.DetailPageRelativeUrl
        [NotMapped]
        public string DetailPageRelativeUrl
        {
            get
            {
                if (HttpContext.Current == null)
                    return "";
                var categoryName = StoryCategory != null ? StoryCategory.Name : "no_category";
                var rc = HttpContext.Current.Request.RequestContext;
                return new UrlHelper(rc).Action("Detail", "Stories", new { categoryName = GeneralHelper.GetUrlSeoString(categoryName), id = this.GetSeoUrl(), area = "" });
            }
        }
    }
}
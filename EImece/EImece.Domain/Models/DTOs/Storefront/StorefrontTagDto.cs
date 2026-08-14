using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected tag read model for storefront tag clouds, similar tags, and product tags.
    /// </summary>
    public class StorefrontTagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TagCategoryId { get; set; }
        public string TagCategoryName { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }
        public bool IsActive { get; set; }
        public int ItemCount { get; set; }

        public string ModifiedId
        {
            get { return GeneralHelper.ModifyId(Id); }
        }

        public string SeoUrl
        {
            get { return string.Format("{0}-{1}", GeneralHelper.GetUrlSeoString(Name), ModifiedId); }
        }

        public string GetSeoUrl()
        {
            return SeoUrl;
        }

        public string DetailPageUrl
        {
            get
            {
                var dummy = new Tag { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Tag", "Products");
            }
        }

        public string DetailPageRelativeUrl
        {
            get { return DetailPageUrl; }
        }

        public string DetailPageAbsoluteUrl
        {
            get { return DetailPageUrl; }
        }

        public string StoryTagDetailPageUrl
        {
            get
            {
                var dummy = new Tag { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Tag", "Stories");
            }
        }

        public string DetailPageRelativeUrlForStories
        {
            get { return StoryTagDetailPageUrl; }
        }

        public string GetSeoTitle(int lang = 1)
        {
            return Name;
        }

        public string GetSeoDescription(int lang = 1)
        {
            return Name;
        }

        public string GetSeoKeywords(int lang = 1)
        {
            return Name;
        }
    }
}

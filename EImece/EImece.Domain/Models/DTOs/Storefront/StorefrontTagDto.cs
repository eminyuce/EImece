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

        public string DetailPageUrl
        {
            get
            {
                var dummy = new Tag { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Tag", "Products");
            }
        }

        public string StoryTagDetailPageUrl
        {
            get
            {
                var dummy = new Tag { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Tag", "Stories");
            }
        }
    }
}

using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Projected brand read model for storefront filters and brand listings.
    /// </summary>
    public class StorefrontBrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

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
                var dummy = new Brand { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Brands");
            }
        }
    }
}

using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal link read model for header / breadcrumb (e.g., home-index, products-index).
    /// Projection: SELECT Id, Name, MenuLink FROM Menus WHERE MenuLink=@link (3 cols).
    /// Omits all other Menu columns never used in header Razor.
    /// </summary>
    public class StorefrontMenuLinkDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MenuLink { get; set; }

        public string DetailPageLink
        {
            get
            {
                var dummy = new Entities.Menu { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Pages");
            }
        }

        public string ModifiedId => GeneralHelper.ModifyId(Id);
    }
}

using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal navigation read model for header, footer and mega-menu.
    /// Projection: SELECT Id, Name, ParentId, MenuLink, Link, PageTheme, Position FROM Menus WHERE Lang=@lang AND IsActive (6 cols).
    /// Omits Description, ShortDescription, MainImageId, Lang, IsActive, Target, IsPageActived, CreatedDate, UpdatedDate — never used in navigation Razor.
    /// </summary>
    public class StorefrontMenuNavigationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string MenuLink { get; set; }
        public string Url { get; set; }
        public string PageTheme { get; set; }
        public int Position { get; set; }
        public int TreeLevel { get; set; }

        public List<StorefrontMenuNavigationDto> Children { get; set; } = new List<StorefrontMenuNavigationDto>();
        public List<StorefrontMenuNavigationDto> SideMenus { get; set; } = new List<StorefrontMenuNavigationDto>();

        public string DetailPageLink
        {
            get
            {
                if (!string.IsNullOrEmpty(Url)) return Url;
                var dummy = new Entities.Menu { Id = Id, Name = Name };
                return dummy.GetDetailPageUrl("Detail", "Pages");
            }
        }

        public string ModifiedId => GeneralHelper.ModifyId(Id);
        public string SeoUrl => $"{GeneralHelper.GetUrlSeoString(Name)}-{ModifiedId}";
    }
}

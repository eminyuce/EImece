using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class Menu : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MenuParentId))]
        public int ParentId { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MenuLink))]
        public string MenuLink { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Static))]
        public Boolean Static { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Link))]
        public string Link { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.PageTheme))]
        public string PageTheme { get; set; }
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.LinkIsActive))]
        public Boolean LinkIsActive { get; set; }
        [NotMapped]
        public List<Menu> Childrens { get; set; }

        public ICollection<MenuFile> MenuFiles { get; set; }
    }
}

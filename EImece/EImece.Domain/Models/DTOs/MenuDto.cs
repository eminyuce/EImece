using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class MenuDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from BaseContent
        public string Description { get; set; }
        public bool ImageState { get; set; }
        public string MetaKeywords { get; set; }
        public int? MainImageId { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public string UpdateUserId { get; set; }
        public string AddUserId { get; set; }

        // from Menu
        public int ParentId { get; set; }
        public Boolean MainPage { get; set; }
        public string MenuLink { get; set; }
        public string Link { get; set; }
        public string PageTheme { get; set; }
        public Boolean LinkIsActive { get; set; }
        public List<MenuDto> Childrens { get; set; }
        public string DetailPageLink { get; set; }
    }
}

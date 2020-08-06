using Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Brand : BaseContent
    {
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.MainPage))]
        public Boolean MainPage { get; set; }
    }
}
using Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Brand : BaseContent
    {
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.MainPage))]
        public Boolean MainPage { get; set; }
    }
}
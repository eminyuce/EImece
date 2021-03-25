using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class Setting : BaseEntity
    {
        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Description))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SettingKey))]
        public string SettingKey { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.SettingValue))]
        public string SettingValue { get; set; }

        public bool IsEmpty()
        {
            return this == null || string.IsNullOrEmpty(SettingValue);
        }

        public bool IsNotEmpty()
        {
            return !IsEmpty();
        }
    }
}
﻿using Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace EImece.Domain.Entities
{
    public class Setting : BaseEntity
    {
        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Description))]
        public string Description { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SettingKey))]
        public string SettingKey { get; set; }

        [AllowHtml]
        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.SettingValue))]
        public string SettingValue { get; set; }


        public bool IsEmpty()
        {
            return this == null || string.IsNullOrEmpty(SettingValue);
        }
    }
}
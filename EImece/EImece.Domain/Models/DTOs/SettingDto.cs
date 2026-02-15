using EImece.Domain.Services;
using System;

namespace EImece.Domain.Models.DTOs
{
    public class SettingDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Setting
        public string Description { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }

        public bool IsEmpty()
        {
            return this == null || string.IsNullOrEmpty(SettingValue) || SettingService.NULL_VALUE.Equals(SettingValue, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsNotEmpty()
        {
            return !IsEmpty();
        }
    }
}

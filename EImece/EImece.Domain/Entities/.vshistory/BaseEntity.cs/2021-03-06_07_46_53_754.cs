﻿using GenericRepository;
using Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    [Serializable]
    public abstract class BaseEntity : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.NamePropertyRequiredErrorMessage))]
        [StringLength(500, ErrorMessageResourceType = typeof(AdminResource), ErrorMessageResourceName = nameof(AdminResource.NamePropertyErrorMessage))]
        [Column("Name")]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.PleaseEnterYourName))]
        public virtual string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.IsActive))]
        public bool IsActive { get; set; }

        [Display(ResourceType = typeof(AdminResource), Name = nameof(AdminResource.Position))]
        public int Position { get; set; }

        public int Lang { get; set; }
    }
}
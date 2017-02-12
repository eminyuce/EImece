using GenericRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources;

namespace EImece.Domain.Entities
{
    public abstract class BaseEntity : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = nameof(Resource.NamePropertyRequiredErrorMessage))]
        [StringLength(500,ErrorMessageResourceType =typeof(Resource), ErrorMessageResourceName =nameof(Resource.NamePropertyErrorMessage))]
        [Column("Name")]
        //[Display(Name = "Name")]
        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Name))]
        public string Name { get; set; }
        public string EntityHash { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

         
       
    }
}

using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class SelectRoleEditorViewModel
    {
        public SelectRoleEditorViewModel()
        {
        }

        public SelectRoleEditorViewModel(IdentityRole role)
        {
            this.RoleName = role.Name;
            this.RoleId = role.Id;
        }

        public bool Selected { get; set; }

        [Required]
        public string RoleName { get; set; }

        public string RoleId { get; set; }
    }
}
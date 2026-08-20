using EImece.Domain.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class SelectUserRolesViewModel
    {
        public SelectUserRolesViewModel()
        {
            this.Roles = new List<SelectRoleEditorViewModel>();
        }

        // Enable initialization with an instance of ApplicationUser:
        public SelectUserRolesViewModel(ApplicationUser user) : this()
        {
            if (user != null)
            {
                this.UserName = user.UserName;
                this.FirstName = user.FirstName;
                this.LastName = user.LastName;
                this.Id = user.Id;
            }
        }

        public void PopulateAdminRoles(ApplicationUser user, IEnumerable<IdentityRole> allRoles)
        {
            if (user == null || allRoles == null)
            {
                return;
            }

            this.Roles.Clear();
            foreach (var role in allRoles)
            {
                if (role.Name.Equals(Domain.Constants.AdministratorRole, StringComparison.InvariantCultureIgnoreCase) ||
                    role.Name.Equals(Domain.Constants.EditorRole, StringComparison.InvariantCultureIgnoreCase))
                {
                    var rvm = new SelectRoleEditorViewModel(role);
                    this.Roles.Add(rvm);
                }
            }

            if (user.Roles != null)
            {
                foreach (var userRole in user.Roles)
                {
                    var checkUserRole = this.Roles.Find(r => r.RoleId.Equals(userRole.RoleId));
                    if (checkUserRole != null)
                    {
                        checkUserRole.Selected = true;
                    }
                }
            }
        }

        public void PopulateRoles(ApplicationUser user, IEnumerable<IdentityRole> allRoles)
        {
            if (user == null || allRoles == null)
            {
                return;
            }

            this.Roles.Clear();
            foreach (var role in allRoles)
            {
                var rvm = new SelectRoleEditorViewModel(role);
                this.Roles.Add(rvm);
            }

            if (user.Roles != null)
            {
                foreach (var userRole in user.Roles)
                {
                    var checkUserRole = this.Roles.Find(r => r.RoleId.Equals(userRole.RoleId));
                    if (checkUserRole != null)
                    {
                        checkUserRole.Selected = true;
                    }
                }
            }
        }

        public string Id { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.Email))]
        public string UserName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.FirstName))]
        public string FirstName { get; set; }

        [Display(ResourceType = typeof(Resource), Name = nameof(Resource.LastName))]
        public string LastName { get; set; }

        public List<SelectRoleEditorViewModel> Roles { get; set; }
    }
}
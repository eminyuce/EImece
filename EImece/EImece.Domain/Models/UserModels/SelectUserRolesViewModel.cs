using EImece.Domain.DbContext;
using EImece.Domain.Services;
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
            this.UserName = user.UserName;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Id = user.Id;
        }

        public void SetAdminRoles(ApplicationUser user)
        {
            var Db = new ApplicationDbContext();

            // Add all available roles to the list of EditorViewModels:
            var allRoles = Db.Roles;
            foreach (var role in allRoles)
            {
                if (role.Name.Equals(Domain.Constants.AdministratorRole, StringComparison.InvariantCultureIgnoreCase) || role.Name.Equals(Domain.Constants.EditorRole, StringComparison.InvariantCultureIgnoreCase))
                {
                    // An EditorViewModel will be used by Editor Template:
                    var rvm = new SelectRoleEditorViewModel(role);
                    this.Roles.Add(rvm);
                }
            }

            // Set the Selected property to true for those roles for
            // which the current user is a member:
            foreach (var userRole in user.Roles)
            {
                var checkUserRole =
                    this.Roles.Find(r => r.RoleId.Equals(userRole.RoleId));
                checkUserRole.Selected = true;
            }
        }

        public void SetRoles(ApplicationUser user)
        {
            var Db = new ApplicationDbContext();

            // Add all available roles to the list of EditorViewModels:
            var allRoles = Db.Roles;
            foreach (var role in allRoles)
            {
                // An EditorViewModel will be used by Editor Template:
                var rvm = new SelectRoleEditorViewModel(role);
                this.Roles.Add(rvm);
            }

            // Set the Selected property to true for those roles for
            // which the current user is a member:
            foreach (var userRole in user.Roles)
            {
                var checkUserRole =
                    this.Roles.Find(r => r.RoleId.Equals(userRole.RoleId));
                checkUserRole.Selected = true;
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
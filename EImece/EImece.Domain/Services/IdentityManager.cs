using EImece.Domain.DbContext;
using EImece.Domain.Helpers.EmailHelper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Helpers.EmailHelper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Ninject;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EImece.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class IdentityManager
    {

  
    public bool RoleExists(string name)
    {
        var rm = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(new ApplicationDbContext()));
        return rm.RoleExists(name);
    }

    public bool CreateRole(string name)
    {
        var rm = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(new ApplicationDbContext()));
        var idResult = rm.Create(new IdentityRole(name));
        return idResult.Succeeded;
    }

    public bool CreateUser(ApplicationUser user, string password)
    {
        var um = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(new ApplicationDbContext()));
        var idResult = um.Create(user, password);
        return idResult.Succeeded;
    }

    public bool AddUserToRole(string userId, string roleName)
    {
        var um = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(new ApplicationDbContext()));
        var idResult = um.AddToRole(userId, roleName);
        return idResult.Succeeded;
    }

    public void ClearUserRoles(string userId)
    {
        var dbContext = new ApplicationDbContext();
        var um = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(dbContext));
        var user = um.FindById(userId);
        var currentRoles = new List<IdentityUserRole>();
        currentRoles.AddRange(user.Roles);
        var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));
        foreach (var role in currentRoles)
        {
            var roleObj = rm.FindById(role.RoleId);
            um.RemoveFromRole(userId, roleObj.Name);
        }
    }

    }
}

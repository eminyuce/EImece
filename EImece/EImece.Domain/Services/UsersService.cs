using EImece.Domain.DbContext;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Services
{
    public class UsersService
    {
        [Inject]
        public ApplicationSignInManager SignInManager { get; set; }

        [Inject]
        public ApplicationUserManager UserManager { get; set; }

        [Inject]
        public IdentityManager IdentityManager { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        public ApplicationUser GetUser(string id)
        {
            return ApplicationDbContext.Users.First(u => u.Id == id);
        }

        public List<EditUserViewModel> GetUsers(string search)
        {
            var users = ApplicationDbContext.Users.AsQueryable();

            var users2 = from u in ApplicationDbContext.Users
                         from ur in u.Roles
                         join r in ApplicationDbContext.Roles on ur.RoleId equals r.Id
                         select new
                         {
                             u.Id,
                             Email = u.UserName,
                             FirstName = u.FirstName,
                             LastName = u.LastName,
                             Role = r.Name,
                         };

            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                users = users.Where(r => r.Email.ToLower().Contains(search) || r.FirstName.ToLower().Contains(search) || r.LastName.ToLower().Contains(search));
            }

            //ViewModel will be posted at the end of the answer
            var model = new List<EditUserViewModel>();
            foreach (var user in users.ToList())
            {
                var u = new EditUserViewModel();
                u.FirstName = user.FirstName;
                u.LastName = user.LastName;
                u.Email = user.Email;
                u.Id = user.Id;
                var p = users2.FirstOrDefault(r => r.Id == u.Id);
                u.Role = p == null ? String.Empty : p.Role.ToStr();
                model.Add(u);
            }

            return model;
        }
    }
}
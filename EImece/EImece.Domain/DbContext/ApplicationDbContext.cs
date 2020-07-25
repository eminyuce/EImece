using EImece.Domain.Services;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EImece.Domain.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base(Domain.Constants.DbConnectionKey, throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
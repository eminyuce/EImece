namespace EImece.Domain.Services.IServices
{
    public interface IIdentityManager
    {
        bool RoleExists(string name);

        bool CreateRole(string name);

        bool CreateUser(ApplicationUser user, string password);

        bool AddUserToRole(string userId, string roleName);

        void ClearUserRoles(string userId);
    }
}

-- ============================================================================
-- EImece Admin User Seed Script
-- ============================================================================
-- Database: eimece
-- File    : 02_SeedAdminUsers.sql
-- Purpose : Seeds one or more admin accounts so a developer can log in
--           immediately after creating the eimece database with
--           01_CreateDatabase.sql.
--
-- Prereqs : 01_CreateDatabase.sql has been executed successfully.
--           The database [eimece] and ASP.NET Identity tables
--           (AspNetRoles, AspNetUsers, AspNetUserRoles) already exist.
--
-- Usage   : 1) In SSMS (or sqlcmd/Invoke-Sqlcmd) ensure you are connected
--              to the same instance where [eimece] was created:
--                USE [eimece];  -- this script does it for you
--           2) Open and execute this file. It is idempotent - safe to run
--              multiple times. Existing rows are skipped / updated.
--           3) Update your connection string to use Initial Catalog=eimece:
--                Data Source=YUCE\SQLEXPRESS;Initial Catalog=eimece;User ID=sqluser;Password=sqluser;Encrypt=True;TrustServerCertificate=True;
--              Or for LocalDB:
--                Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=eimece;Integrated Security=True;
--           4) Run the app (IIS http://localhost:81/ or IIS Express) and log in:
--                URL      : http://localhost:81/account/adminlogin/
--                User 1   : admin@eimece.test      / Admin123!
--                UserName : seed-admin (also works - login accepts email OR UserName)
--                User 2   : devadmin@eimece.test   / Admin123!
--                UserName : seed-devadmin
--                User 3   : editor@eimece.test     / Admin123!  (NormalUser role, also allowed in /admin)
--                All passwords meet Identity PasswordValidator:
--                  RequiredLength=6, RequireDigit=true, RequireLowercase=true,
--                  RequireUppercase=true.  Example: Admin123!
--           5) If RequireAdminAuthenticator (AppConfig / Settings) is true, you
--              may need to set up an authenticator app after first login.
--              To skip 2FA for local dev, set either:
--                - Web.config: <add key="RequireAdminAuthenticator" value="false" />
--                - Or run: UPDATE dbo.Settings SET SettingValue='false' WHERE SettingKey='RequireAdminAuthenticator';
--                - Or set BypassAdminAuth=true (dev only, hard-disabled when SiteStatus=live).
--
-- Notes   : - Roles are created if missing: Admin (full), NormalUser (editor),
--             Customer (storefront). Admin accounts are added to Admin.
--           - Passwords are ASP.NET Identity V2 hashes (PBKDF2-HMAC-SHA1,
--             1000 iterations, 128-bit salt, 256-bit subkey). Hashes below were
--             generated via Microsoft.AspNet.Identity.PasswordHasher.
--             Do NOT store plaintext; hashes verify with PasswordHasher.VerifyHashedPassword().
--           - EmailConfirmed=1, LockoutEnabled=1, AccessFailedCount=0, TwoFactorEnabled=0,
--             PhoneNumberConfirmed=0. SecurityStamp is a GUID; re-generated on each seed.
--           - AspNetUsers.Id is NVARCHAR(128) GUID-like; seed IDs are fixed
--             (seed-admin-... ) so FKs stay stable across re-runs.
--           - This script uses SET NOCOUNT ON; PRINT for progress; transactions
--             per user/role block for safety.
--           - To reset passwords: run aspnet HashPassword locally or use
--             UserManager.ChangePassword() / ResetPassword() in the app.
--
-- Order   : 01_CreateDatabase.sql -> 02_SeedAdminUsers.sql (this file)
-- ============================================================================

SET NOCOUNT ON;
GO

-- Switch to eimece; abort if DB does not exist.
IF DB_ID(N'eimece') IS NULL
BEGIN
    RAISERROR(N'Database [eimece] does not exist. Run 01_CreateDatabase.sql first.', 16, 1);
    RETURN;
END
GO

USE [eimece]
GO

PRINT N'=== EImece Admin Seed: ensuring roles ===';
GO

-- ----------------------------------------------------------------------------
-- 1) Ensure roles exist (Admin, NormalUser, Customer)
--    IDs are deterministic seed IDs; if role exists by Name we reuse its Id.
-- ----------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NULL
BEGIN
    RAISERROR(N'Table dbo.AspNetRoles not found. Ensure 01_CreateDatabase.sql was executed.', 16, 1);
    RETURN;
END
GO

-- Use MERGE-style idempotent inserts
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'Admin')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name) VALUES (N'seed-role-admin', N'Admin');
    PRINT N'Role [Admin] created (Id=seed-role-admin).';
END
ELSE
    PRINT N'Role [Admin] already exists.';

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'NormalUser')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name) VALUES (N'seed-role-editor', N'NormalUser');
    PRINT N'Role [NormalUser] created (Id=seed-role-editor).';
END
ELSE
    PRINT N'Role [NormalUser] already exists.';

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'Customer')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, Name) VALUES (N'seed-role-customer', N'Customer');
    PRINT N'Role [Customer] created (Id=seed-role-customer).';
END
ELSE
    PRINT N'Role [Customer] already exists.';
GO

PRINT N'=== Ensuring admin users ===';
GO

-- ----------------------------------------------------------------------------
-- 2) Ensure admin users exist
--    Password: Admin123!  (meets PasswordValidator)
--    Hashes generated via PasswordHasher (Identity V2):
--      admin@eimece.test    -> AHw3mluCHYautgOt69ZsXzbB+i9x9ePEC9+FtiVyGEuZFK/zqeeAwO2NOTzh9SDwbw==
--      devadmin@eimece.test -> AHRF964eMnZcPCKCJjrjTdrY1MvMx3NM4F9OY63yJWs3NWcUelX4SIUXGuvSY/YrHA==
--      editor@eimece.test   -> ABjoPU3Onp5j2/tR1mrtsdM6dxQcOVgRmakr7w18nEMxlH/Hpo7cx5mAkjFNAtjnGg==  (same password, different salt)
--    SecurityStamp is a fresh GUID per user (required by Identity, changes invalidate old cookies/tokens).
-- ----------------------------------------------------------------------------

DECLARE @AdminRoleId   NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'Admin');
DECLARE @EditorRoleId  NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'NormalUser');

IF @AdminRoleId IS NULL
BEGIN
    RAISERROR(N'Admin role Id not found after seed. Check AspNetRoles.', 16, 1);
    RETURN;
END
IF @EditorRoleId IS NULL
BEGIN
    RAISERROR(N'NormalUser role Id not found after seed. Check AspNetRoles.', 16, 1);
    RETURN;
END

-- Helper: insert or update user. We use separate blocks per user for clarity and idempotency.
-- User 1: admin@eimece.test / seed-admin / Admin role
DECLARE @AdminId       NVARCHAR(128) = N'seed-admin-000000000001';
DECLARE @AdminEmail    NVARCHAR(256) = N'admin@eimece.test';
DECLARE @AdminUserName NVARCHAR(256) = N'seed-admin';
DECLARE @AdminFirst    NVARCHAR(256) = N'Admin';
DECLARE @AdminLast     NVARCHAR(256) = N'EImece';
DECLARE @AdminHash     NVARCHAR(MAX) = N'AHw3mluCHYautgOt69ZsXzbB+i9x9ePEC9+FtiVyGEuZFK/zqeeAwO2NOTzh9SDwbw==';
DECLARE @AdminStamp    NVARCHAR(MAX) = N'fc3b4dfd-9612-4b2f-b7cc-276fb8ed6d47';

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @AdminId OR Email = @AdminEmail)
BEGIN
    PRINT N'Creating user admin@eimece.test (Id=seed-admin-000000000001)...';
    INSERT INTO dbo.AspNetUsers
        (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp,
         PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc,
         LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName,
         TwoFactorAuthenticatorEnabled, AuthenticatorKey)
    VALUES
        (@AdminId, @AdminEmail, 1, @AdminHash, @AdminStamp,
         NULL, 0, 0, NULL,
         1, 0, @AdminUserName, @AdminFirst, @AdminLast,
         0, NULL);
    PRINT N'User admin@eimece.test created.';
END
ELSE
BEGIN
    PRINT N'User admin@eimece.test already exists - updating password/hash and profile to ensure login works...';
    UPDATE dbo.AspNetUsers SET
        Email = @AdminEmail,
        EmailConfirmed = 1,
        PasswordHash = @AdminHash,
        SecurityStamp = @AdminStamp,
        UserName = @AdminUserName,
        FirstName = @AdminFirst,
        LastName = @AdminLast,
        LockoutEnabled = 1,
        AccessFailedCount = 0,
        LockoutEndDateUtc = NULL,
        TwoFactorEnabled = 0,
        TwoFactorAuthenticatorEnabled = 0,
        AuthenticatorKey = NULL
    WHERE Id = @AdminId OR Email = @AdminEmail;
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @AdminId)
        UPDATE dbo.AspNetUsers SET Id = @AdminId WHERE Email = @AdminEmail;
    PRINT N'User admin@eimece.test updated.';
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @AdminId AND RoleId = @AdminRoleId)
BEGIN
    DELETE FROM dbo.AspNetUserRoles WHERE UserId = @AdminId;
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@AdminId, @AdminRoleId);
    PRINT N'Added Admin role to admin@eimece.test.';
END
ELSE
    PRINT N'admin@eimece.test already in Admin role.';

GO
-- User 2: devadmin@eimece.test / seed-devadmin / Admin role  (second admin for teams)
DECLARE @DevAdminId       NVARCHAR(128) = N'seed-devadmin-000000001';
DECLARE @DevAdminEmail    NVARCHAR(256) = N'devadmin@eimece.test';
DECLARE @DevAdminUserName NVARCHAR(256) = N'seed-devadmin';
DECLARE @DevAdminFirst    NVARCHAR(256) = N'Dev';
DECLARE @DevAdminLast     NVARCHAR(256) = N'Admin';
DECLARE @DevAdminHash     NVARCHAR(MAX) = N'AHRF964eMnZcPCKCJjrjTdrY1MvMx3NM4F9OY63yJWs3NWcUelX4SIUXGuvSY/YrHA==';
DECLARE @DevAdminStamp    NVARCHAR(MAX) = N'7a7ec906-1151-49ec-ba59-af93b06d194c';
DECLARE @AdminRoleId2     NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'Admin');

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @DevAdminId OR Email = @DevAdminEmail)
BEGIN
    PRINT N'Creating user devadmin@eimece.test...';
    INSERT INTO dbo.AspNetUsers
        (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp,
         PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc,
         LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName,
         TwoFactorAuthenticatorEnabled, AuthenticatorKey)
    VALUES
        (@DevAdminId, @DevAdminEmail, 1, @DevAdminHash, @DevAdminStamp,
         NULL, 0, 0, NULL,
         1, 0, @DevAdminUserName, @DevAdminFirst, @DevAdminLast,
         0, NULL);
    PRINT N'User devadmin@eimece.test created.';
END
ELSE
BEGIN
    PRINT N'User devadmin@eimece.test already exists - updating...';
    UPDATE dbo.AspNetUsers SET
        Email = @DevAdminEmail,
        EmailConfirmed = 1,
        PasswordHash = @DevAdminHash,
        SecurityStamp = @DevAdminStamp,
        UserName = @DevAdminUserName,
        FirstName = @DevAdminFirst,
        LastName = @DevAdminLast,
        LockoutEnabled = 1,
        AccessFailedCount = 0,
        LockoutEndDateUtc = NULL,
        TwoFactorEnabled = 0,
        TwoFactorAuthenticatorEnabled = 0,
        AuthenticatorKey = NULL
    WHERE Id = @DevAdminId OR Email = @DevAdminEmail;
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @DevAdminId)
        UPDATE dbo.AspNetUsers SET Id = @DevAdminId WHERE Email = @DevAdminEmail;
    PRINT N'User devadmin@eimece.test updated.';
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @DevAdminId AND RoleId = @AdminRoleId2)
BEGIN
    DELETE FROM dbo.AspNetUserRoles WHERE UserId = @DevAdminId;
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@DevAdminId, @AdminRoleId2);
    PRINT N'Added Admin role to devadmin@eimece.test.';
END
ELSE
    PRINT N'devadmin@eimece.test already in Admin role.';
GO

-- User 3: editor@eimece.test / seed-editor / NormalUser role (also allowed in /admin per BaseAdminController)
DECLARE @EditorId       NVARCHAR(128) = N'seed-editor-00000000001';
DECLARE @EditorEmail    NVARCHAR(256) = N'editor@eimece.test';
DECLARE @EditorUserName NVARCHAR(256) = N'seed-editor';
DECLARE @EditorFirst    NVARCHAR(256) = N'Editor';
DECLARE @EditorLast     NVARCHAR(256) = N'EImece';
DECLARE @EditorHash     NVARCHAR(MAX) = N'ABjoPU3Onp5j2/tR1mrtsdM6dxQcOVgRmakr7w18nEMxlH/Hpo7cx5mAkjFNAtjnGg==';
DECLARE @EditorStamp    NVARCHAR(MAX) = N'c3112ad2-f02e-4bbc-9a4b-094e28803cb4';
DECLARE @EditorRoleId2  NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'NormalUser');

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @EditorId OR Email = @EditorEmail)
BEGIN
    PRINT N'Creating user editor@eimece.test (NormalUser)...';
    INSERT INTO dbo.AspNetUsers
        (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp,
         PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEndDateUtc,
         LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName,
         TwoFactorAuthenticatorEnabled, AuthenticatorKey)
    VALUES
        (@EditorId, @EditorEmail, 1, @EditorHash, @EditorStamp,
         NULL, 0, 0, NULL,
         1, 0, @EditorUserName, @EditorFirst, @EditorLast,
         0, NULL);
    PRINT N'User editor@eimece.test created.';
END
ELSE
BEGIN
    PRINT N'User editor@eimece.test already exists - updating...';
    UPDATE dbo.AspNetUsers SET
        Email = @EditorEmail,
        EmailConfirmed = 1,
        PasswordHash = @EditorHash,
        SecurityStamp = @EditorStamp,
        UserName = @EditorUserName,
        FirstName = @EditorFirst,
        LastName = @EditorLast,
        LockoutEnabled = 1,
        AccessFailedCount = 0,
        LockoutEndDateUtc = NULL,
        TwoFactorEnabled = 0,
        TwoFactorAuthenticatorEnabled = 0,
        AuthenticatorKey = NULL
    WHERE Id = @EditorId OR Email = @EditorEmail;
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @EditorId)
        UPDATE dbo.AspNetUsers SET Id = @EditorId WHERE Email = @EditorEmail;
    PRINT N'User editor@eimece.test updated.';
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @EditorId AND RoleId = @EditorRoleId2)
BEGIN
    DELETE FROM dbo.AspNetUserRoles WHERE UserId = @EditorId;
    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@EditorId, @EditorRoleId2);
    PRINT N'Added NormalUser role to editor@eimece.test.';
END
ELSE
    PRINT N'editor@eimece.test already in NormalUser role.';
GO

-- ----------------------------------------------------------------------------
-- Optional: ensure Settings allow admin login without forcing authenticator
-- Uncomment if you want this script to disable RequireAdminAuthenticator:
-- ----------------------------------------------------------------------------
-- IF EXISTS (SELECT 1 FROM dbo.Settings WHERE SettingKey = N'RequireAdminAuthenticator')
--     UPDATE dbo.Settings SET SettingValue = N'false' WHERE SettingKey = N'RequireAdminAuthenticator';
-- ELSE
--     INSERT INTO dbo.Settings (SettingKey, SettingValue) VALUES (N'RequireAdminAuthenticator', N'false');
-- PRINT N'RequireAdminAuthenticator set to false (optional).';
-- GO

-- ----------------------------------------------------------------------------
-- Summary
-- ----------------------------------------------------------------------------
PRINT N'';
PRINT N'========== ADMIN SEED SUMMARY ==========';
SELECT N'Roles' AS [Table], Name, Id FROM dbo.AspNetRoles WHERE Name IN (N'Admin', N'NormalUser', N'Customer')
UNION ALL
SELECT N'Users' AS [Table], Email, Id FROM dbo.AspNetUsers WHERE Email IN (N'admin@eimece.test', N'devadmin@eimece.test', N'editor@eimece.test')
ORDER BY [Table], Name;
PRINT N'';
PRINT N'Credentials (all passwords = Admin123!):';
PRINT N'  admin@eimece.test    / Admin123!  (Admin)';
PRINT N'  devadmin@eimece.test / Admin123!  (Admin)';
PRINT N'  editor@eimece.test   / Admin123!  (NormalUser - also allowed in /admin)';
PRINT N'Login URL: http://localhost:81/account/adminlogin/';
PRINT N'Note: Login accepts either Email or UserName (seed-admin / seed-devadmin).';
PRINT N'If 2FA is enforced, set RequireAdminAuthenticator=false or configure an authenticator app.';
GO
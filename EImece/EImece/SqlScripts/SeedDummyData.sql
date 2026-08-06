/*
================================================================================
  EImece — Seed Dummy Data for Manual / Demo Testing
================================================================================
  Inserts realistic volumes of related demo data so the storefront and admin
  feel like a small-to-medium shop (not thousands of menus/settings/brands).

  Default shape (@Scale = 1):
    ~12 menus, ~6 homepage slides, ~20 brands, ~25 categories, ~150 products,
    ~30 stories, ~40 customers/users, ~100 orders, plus supporting rows.

  HOW TO RUN
  ----------
  1. Ensure the EImece database schema already exists (app has created tables).
  2. Open this script in SSMS (or use sqlcmd / Invoke-Sqlcmd).
  3. Optionally change @Scale (bulk tables only) or individual @Seed* counts.
  4. Execute against your EImece database.

  PowerShell example:
    .\RunSeedDummyData.ps1 -ConnectionString "Server=.;Database=EImece;Trusted_Connection=True;"
    .\RunSeedDummyData.ps1 -ConnectionString "..." -Scale 2   # larger catalog/orders

  TEST LOGINS (local seed credential — see docs/BUILD_AND_RUN.md)
  ---------------------------------------------------------------
    admin@eimece.test      → Admin role
    editor@eimece.test     → NormalUser role
    customer1@eimece.test  → Customer role
    seeduser00001@eimece.test … → Customer
    Shared seed credential parts: N'Test' + N'123' + N'!'

  CLEANUP
  -------
  Run CleanupDummyData.sql, or set @CleanupFirst = 1 (default) before re-seed.

  NOTES
  -----
  - All seed entity Names are prefixed with N'SEED ' for easy cleanup.
  - Structural tables (Menus, MainPageImages, Templates, Settings, MailTemplates)
    use small fixed counts so the site stays usable; @Scale does not inflate them.
  - Settings / MailTemplates: required app keys/names first, plus a few fillers.
  - Product.Rating is omitted when the column is computed; otherwise set explicitly.
  - Script is idempotent when @CleanupFirst = 1.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ========================= CONFIG ========================= */
DECLARE @Scale         FLOAT        = 1.0;    -- multiplies catalog/order bulk tables only
DECLARE @CleanupFirst  BIT          = 1;      -- 1 = wipe previous SEED data first
DECLARE @Lang          INT          = 1;      -- 1=TR, 2=EN
DECLARE @Now           DATETIME     = GETDATE();
DECLARE @AdminUserId   NVARCHAR(128) = N'seed-admin-000000000001';
DECLARE @EditorUserId  NVARCHAR(128) = N'seed-editor-00000000001';
DECLARE @Customer1Id   NVARCHAR(128) = N'seed-customer-0000000001';
/* ASP.NET Identity V2 hash for the local seed credential (PBKDF2-HMAC-SHA1, 1000 iter).
   Plaintext is N'Test' + N'123' + N'!' — documented in docs/BUILD_AND_RUN.md */
DECLARE @PasswordHash  NVARCHAR(MAX) = N'AAECAwQFBgcICQoLDA0ODxDDsDqHD/P2DJthJqYXFSVlp6Ybmsrf5Stb142xLX6XZw==';
DECLARE @SecurityStamp NVARCHAR(MAX) = N'A1B2C3D4E5F64789A0B1C2D3E4F50607';

/* ---- Structural / UX-sensitive (NOT scaled) ---- */
DECLARE @SeedMenus              INT = 12;   -- main nav / CMS pages
DECLARE @SeedMenuFiles          INT = 12;
DECLARE @SeedMainPageImages     INT = 6;    -- homepage slider
DECLARE @SeedTemplates          INT = 6;
DECLARE @SeedTagCategories      INT = 6;
DECLARE @SeedLists              INT = 8;
DECLARE @SeedFaqs               INT = 20;
DECLARE @SeedCoupons            INT = 12;
DECLARE @SeedStoryCategories    INT = 6;
DECLARE @SeedBrands             INT = 20;
DECLARE @SeedSettingFillers     INT = 10;   -- plus required keys
DECLARE @SeedMailTemplateFillers INT = 5;   -- plus required templates
DECLARE @SeedBrowserSubscriptions INT = 3;
DECLARE @SeedShortUrls          INT = 20;

/* ---- Catalog / traffic (scaled by @Scale) ---- */
DECLARE @SeedUsers              INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40  * @Scale, 0) AS INT) END;
DECLARE @SeedFiles              INT = CASE WHEN CAST(ROUND(120 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(120 * @Scale, 0) AS INT) END;
DECLARE @SeedTags               INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40  * @Scale, 0) AS INT) END;
DECLARE @SeedProductCategories  INT = CASE WHEN CAST(ROUND(25  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(25  * @Scale, 0) AS INT) END;
DECLARE @SeedCategoryRoots      INT = CASE WHEN CAST(ROUND(8   * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(8   * @Scale, 0) AS INT) END;
DECLARE @SeedProducts           INT = CASE WHEN CAST(ROUND(150 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(150 * @Scale, 0) AS INT) END;
DECLARE @SeedProductFiles       INT = CASE WHEN CAST(ROUND(200 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(200 * @Scale, 0) AS INT) END;
DECLARE @SeedProductTags        INT = CASE WHEN CAST(ROUND(200 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(200 * @Scale, 0) AS INT) END;
DECLARE @SeedProductSpecs       INT = CASE WHEN CAST(ROUND(300 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(300 * @Scale, 0) AS INT) END;
DECLARE @SeedProductComments    INT = CASE WHEN CAST(ROUND(80  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(80  * @Scale, 0) AS INT) END;
DECLARE @SeedStories            INT = CASE WHEN CAST(ROUND(30  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(30  * @Scale, 0) AS INT) END;
DECLARE @SeedStoryFiles         INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40  * @Scale, 0) AS INT) END;
DECLARE @SeedStoryTags          INT = CASE WHEN CAST(ROUND(60  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(60  * @Scale, 0) AS INT) END;
DECLARE @SeedFileStorageTags    INT = CASE WHEN CAST(ROUND(80  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(80  * @Scale, 0) AS INT) END;
DECLARE @SeedListItems          INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40  * @Scale, 0) AS INT) END;
DECLARE @SeedSubscribers        INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40  * @Scale, 0) AS INT) END;
DECLARE @SeedCustomers          INT = @SeedUsers;
DECLARE @SeedAddresses          INT = CASE WHEN CAST(ROUND(80  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(80  * @Scale, 0) AS INT) END;
DECLARE @SeedOrders             INT = CASE WHEN CAST(ROUND(100 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(100 * @Scale, 0) AS INT) END;
DECLARE @SeedOrderProducts      INT = CASE WHEN CAST(ROUND(250 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(250 * @Scale, 0) AS INT) END;
DECLARE @SeedShoppingCarts      INT = CASE WHEN CAST(ROUND(25  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(25  * @Scale, 0) AS INT) END;
DECLARE @SeedBrowserSubscribers INT = CASE WHEN CAST(ROUND(30  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(30  * @Scale, 0) AS INT) END;
DECLARE @SeedBrowserNotifications INT = CASE WHEN CAST(ROUND(15 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(15 * @Scale, 0) AS INT) END;
DECLARE @SeedBrowserFeedbacks   INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40  * @Scale, 0) AS INT) END;
DECLARE @SeedAppLogs            INT = CASE WHEN CAST(ROUND(100 * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(100 * @Scale, 0) AS INT) END;

IF @Scale <= 0
BEGIN
    RAISERROR(N'@Scale must be > 0', 16, 1);
    RETURN;
END;

IF @SeedCategoryRoots > @SeedProductCategories
    SET @SeedCategoryRoots = @SeedProductCategories;

DECLARE @MaxSeed INT =
(
    SELECT MAX(v) FROM (VALUES
        (@SeedUsers),(@SeedFiles),(@SeedTags),(@SeedProductCategories),(@SeedProducts),
        (@SeedProductFiles),(@SeedProductTags),(@SeedProductSpecs),(@SeedProductComments),
        (@SeedStories),(@SeedStoryFiles),(@SeedStoryTags),(@SeedMenus),(@SeedMenuFiles),
        (@SeedMainPageImages),(@SeedFileStorageTags),(@SeedSettingFillers),(@SeedMailTemplateFillers),
        (@SeedLists),(@SeedListItems),(@SeedFaqs),(@SeedSubscribers),(@SeedCoupons),
        (@SeedCustomers),(@SeedAddresses),(@SeedOrders),(@SeedOrderProducts),(@SeedShoppingCarts),
        (@SeedBrowserSubscriptions),(@SeedBrowserSubscribers),(@SeedBrowserNotifications),
        (@SeedBrowserFeedbacks),(@SeedShortUrls),(@SeedAppLogs),(@SeedTemplates),
        (@SeedTagCategories),(@SeedBrands),(@SeedStoryCategories)
    ) x(v)
);

PRINT CONVERT(VARCHAR(30), GETDATE(), 121)
    + N' — Starting seed. Scale=' + CAST(@Scale AS VARCHAR(20))
    + N', Products=' + CAST(@SeedProducts AS VARCHAR(10))
    + N', Menus=' + CAST(@SeedMenus AS VARCHAR(10))
    + N', Orders=' + CAST(@SeedOrders AS VARCHAR(10));

/* ========================= CLEANUP ========================= */
IF @CleanupFirst = 1
BEGIN
    PRINT N'Running cleanup of previous SEED data...';
    /* Inline minimal cleanup (same markers as CleanupDummyData.sql) */
    IF OBJECT_ID(N'dbo.BrowserNotificationFeedBacks', N'U') IS NOT NULL DELETE FROM dbo.BrowserNotificationFeedBacks WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.BrowserNotifications', N'U') IS NOT NULL DELETE FROM dbo.BrowserNotifications WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.BrowserSubscribers', N'U') IS NOT NULL DELETE FROM dbo.BrowserSubscribers WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.BrowserSubscriptions', N'U') IS NOT NULL DELETE FROM dbo.BrowserSubscriptions WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.OrderProducts', N'U') IS NOT NULL
        DELETE op FROM dbo.OrderProducts op INNER JOIN dbo.Orders o ON o.Id = op.OrderId WHERE o.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL DELETE FROM dbo.Orders WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ShoppingCarts', N'U') IS NOT NULL DELETE FROM dbo.ShoppingCarts WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductComments', N'U') IS NOT NULL DELETE FROM dbo.ProductComments WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductSpecifications', N'U') IS NOT NULL DELETE FROM dbo.ProductSpecifications WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductTags', N'U') IS NOT NULL
        DELETE pt FROM dbo.ProductTags pt INNER JOIN dbo.Products p ON p.Id = pt.ProductId WHERE p.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductFiles', N'U') IS NOT NULL DELETE FROM dbo.ProductFiles WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL DELETE FROM dbo.Products WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductCategories', N'U') IS NOT NULL DELETE FROM dbo.ProductCategories WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Brands', N'U') IS NOT NULL DELETE FROM dbo.Brands WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.StoryTags', N'U') IS NOT NULL
        DELETE st FROM dbo.StoryTags st INNER JOIN dbo.Stories s ON s.Id = st.StoryId WHERE s.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.StoryFiles', N'U') IS NOT NULL DELETE FROM dbo.StoryFiles WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Stories', N'U') IS NOT NULL DELETE FROM dbo.Stories WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.StoryCategories', N'U') IS NOT NULL DELETE FROM dbo.StoryCategories WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.MenuFiles', N'U') IS NOT NULL DELETE FROM dbo.MenuFiles WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Menus', N'U') IS NOT NULL DELETE FROM dbo.Menus WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.MainPageImages', N'U') IS NOT NULL DELETE FROM dbo.MainPageImages WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.FileStorageTags', N'U') IS NOT NULL
        DELETE fst FROM dbo.FileStorageTags fst INNER JOIN dbo.FileStorages fs ON fs.Id = fst.FileStorageId WHERE fs.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Tags', N'U') IS NOT NULL DELETE FROM dbo.Tags WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.TagCategories', N'U') IS NOT NULL DELETE FROM dbo.TagCategories WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ListItems', N'U') IS NOT NULL DELETE FROM dbo.ListItems WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Lists', N'U') IS NOT NULL DELETE FROM dbo.Lists WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Faqs', N'U') IS NOT NULL DELETE FROM dbo.Faqs WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Subscribers', N'U') IS NOT NULL DELETE FROM dbo.Subscribers WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Coupons', N'U') IS NOT NULL DELETE FROM dbo.Coupons WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL DELETE FROM dbo.Customers WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Addresses', N'U') IS NOT NULL DELETE FROM dbo.Addresses WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.MailTemplates', N'U') IS NOT NULL DELETE FROM dbo.MailTemplates WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Settings', N'U') IS NOT NULL DELETE FROM dbo.Settings WHERE Name LIKE N'SEED %' OR SettingKey LIKE N'SEED_%';
    IF OBJECT_ID(N'dbo.Templates', N'U') IS NOT NULL DELETE FROM dbo.Templates WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.FileStorages', N'U') IS NOT NULL DELETE FROM dbo.FileStorages WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ShortUrls', N'U') IS NOT NULL DELETE FROM dbo.ShortUrls WHERE Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.AppLogs', N'U') IS NOT NULL DELETE FROM dbo.AppLogs WHERE UserName LIKE N'seed%' OR EventMessage LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
        DELETE ur FROM dbo.AspNetUserRoles ur INNER JOIN dbo.AspNetUsers u ON u.Id = ur.UserId
        WHERE u.UserName LIKE N'seed%' OR u.Email LIKE N'%@eimece.test';
    IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
        DELETE uc FROM dbo.AspNetUserClaims uc INNER JOIN dbo.AspNetUsers u ON u.Id = uc.UserId
        WHERE u.UserName LIKE N'seed%' OR u.Email LIKE N'%@eimece.test';
    IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
        DELETE ul FROM dbo.AspNetUserLogins ul INNER JOIN dbo.AspNetUsers u ON u.Id = ul.UserId
        WHERE u.UserName LIKE N'seed%' OR u.Email LIKE N'%@eimece.test';
    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
        DELETE FROM dbo.AspNetUsers WHERE UserName LIKE N'seed%' OR Email LIKE N'%@eimece.test';
    PRINT N'Cleanup done.';
END;

/* ========================= NUMBERS TALLY ========================= */
IF OBJECT_ID(N'tempdb..#Nums') IS NOT NULL DROP TABLE #Nums;
CREATE TABLE #Nums (n INT NOT NULL PRIMARY KEY);

;WITH E1(n) AS (
    SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
    UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1 UNION ALL SELECT 1
),
E2(n) AS (SELECT 1 FROM E1 a CROSS JOIN E1 b),          -- 100
E3(n) AS (SELECT 1 FROM E2 a CROSS JOIN E2 b),          -- 10,000
Numbers AS (
    SELECT TOP (@MaxSeed) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM E3
)
INSERT INTO #Nums(n)
SELECT n FROM Numbers;

/* City lookup for addresses / customers */
IF OBJECT_ID(N'tempdb..#Cities') IS NOT NULL DROP TABLE #Cities;
CREATE TABLE #Cities (i INT NOT NULL PRIMARY KEY, City NVARCHAR(50), District NVARCHAR(50));
INSERT INTO #Cities(i, City, District) VALUES
 (0,N'Istanbul',N'Kadikoy'),(1,N'Ankara',N'Cankaya'),(2,N'Izmir',N'Karsiyaka'),
 (3,N'Bursa',N'Nilufer'),(4,N'Antalya',N'Muratpasa'),(5,N'Adana',N'Seyhan'),
 (6,N'Gaziantep',N'Sahinbey'),(7,N'Konya',N'Selcuklu'),(8,N'Trabzon',N'Ortahisar'),
 (9,N'Eskisehir',N'Tepebasi');

DECLARE @ProductStates TABLE (i INT, State NVARCHAR(50));
INSERT INTO @ProductStates VALUES
 (0,N'ProductInStock'),(1,N'ProductOutOfStock'),(2,N'PreOrder'),(3,N'Discontinued'),
 (4,N'Backorder'),(5,N'ComingSoon'),(6,N'LimitedStock'),(7,N'Reserved'),
 (8,N'AwaitingRestock'),(9,N'NotForSale');

BEGIN TRANSACTION;

/* ============================================================
   1) ASP.NET Identity roles + users
   ============================================================ */
PRINT N'Seeding AspNetRoles / AspNetUsers...';

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'Admin')
        INSERT INTO dbo.AspNetRoles (Id, Name) VALUES (N'seed-role-admin', N'Admin');
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'NormalUser')
        INSERT INTO dbo.AspNetRoles (Id, Name) VALUES (N'seed-role-editor', N'NormalUser');
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = N'Customer')
        INSERT INTO dbo.AspNetRoles (Id, Name) VALUES (N'seed-role-customer', N'Customer');
END;

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
BEGIN
    DECLARE @HasFirstName BIT = CASE WHEN COL_LENGTH(N'dbo.AspNetUsers', N'FirstName') IS NOT NULL THEN 1 ELSE 0 END;

    IF @HasFirstName = 1
    BEGIN
        INSERT INTO dbo.AspNetUsers
            (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed,
             TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName)
        VALUES
            (@AdminUserId, N'admin@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-admin', N'Seed', N'Admin'),
            (@EditorUserId, N'editor@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-editor', N'Seed', N'Editor'),
            (@Customer1Id, N'customer1@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-customer1', N'Seed', N'Customer');

        INSERT INTO dbo.AspNetUsers
            (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed,
             TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName)
        SELECT
            N'seed-user-' + RIGHT(N'00000000' + CAST(n.n AS NVARCHAR(8)), 8),
            N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test',
            1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0,
            N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
            N'Seed',
            N'User' + CAST(n.n AS NVARCHAR(10))
        FROM #Nums n
        WHERE n.n <= @SeedUsers;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AspNetUsers
            (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed,
             TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName)
        VALUES
            (@AdminUserId, N'admin@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-admin'),
            (@EditorUserId, N'editor@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-editor'),
            (@Customer1Id, N'customer1@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-customer1');

        INSERT INTO dbo.AspNetUsers
            (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed,
             TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName)
        SELECT
            N'seed-user-' + RIGHT(N'00000000' + CAST(n.n AS NVARCHAR(8)), 8),
            N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test',
            1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0,
            N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5)
        FROM #Nums n
        WHERE n.n <= @SeedUsers;
    END

    DECLARE @AdminRoleId NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'Admin');
    DECLARE @EditorRoleId NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'NormalUser');
    DECLARE @CustomerRoleId NVARCHAR(128) = (SELECT TOP 1 Id FROM dbo.AspNetRoles WHERE Name = N'Customer');

    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    SELECT @AdminUserId, @AdminRoleId WHERE @AdminRoleId IS NOT NULL
    UNION ALL
    SELECT @EditorUserId, @EditorRoleId WHERE @EditorRoleId IS NOT NULL
    UNION ALL
    SELECT @Customer1Id, @CustomerRoleId WHERE @CustomerRoleId IS NOT NULL;

    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    SELECT u.Id, @CustomerRoleId
    FROM dbo.AspNetUsers u
    WHERE u.UserName LIKE N'seeduser%'
      AND @CustomerRoleId IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = @CustomerRoleId);
END;

/* ============================================================
   2) FileStorages
   ============================================================ */
PRINT N'Seeding FileStorages...';
INSERT INTO dbo.FileStorages
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     FileName, FileUrl, MimeType, FileSize, Width, Height, Type, IsFileExist)
SELECT
    N'SEED File ' + CAST(n.n AS NVARCHAR(10)),
    DATEADD(MINUTE, -n.n, @Now), DATEADD(MINUTE, -n.n, @Now),
    1, n.n, @Lang,
    N'seed-image-' + CAST(n.n AS NVARCHAR(10)) + N'.jpg',
    N'/media/images/seed-image-' + CAST(n.n AS NVARCHAR(10)) + N'.jpg',
    N'image/jpeg',
    50000 + (n.n % 200000),
    800, 600,
    N'image',
    0
FROM #Nums n
WHERE n.n <= @SeedFiles;

DECLARE @MinFileId INT = (SELECT MIN(Id) FROM dbo.FileStorages WHERE Name LIKE N'SEED %');
DECLARE @MaxFileId INT = (SELECT MAX(Id) FROM dbo.FileStorages WHERE Name LIKE N'SEED %');
DECLARE @FileCount INT = @MaxFileId - @MinFileId + 1;

/* ============================================================
   3) Templates
   ============================================================ */
PRINT N'Seeding Templates...';
INSERT INTO dbo.Templates (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, TemplateXml)
SELECT
    N'SEED Template ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'<template><fields><field name="Color" type="text"/><field name="Size" type="text"/><field name="Material" type="text"/></fields></template>'
FROM #Nums n
WHERE n.n <= @SeedTemplates;

DECLARE @MinTemplateId INT = (SELECT MIN(Id) FROM dbo.Templates WHERE Name LIKE N'SEED %');
DECLARE @TemplateCount INT = (SELECT COUNT(*) FROM dbo.Templates WHERE Name LIKE N'SEED %');

/* ============================================================
   4) TagCategories + Tags
   ============================================================ */
PRINT N'Seeding TagCategories / Tags...';
INSERT INTO dbo.TagCategories (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang)
SELECT N'SEED TagCategory ' + CAST(n.n AS NVARCHAR(10)), @Now, @Now, 1, n.n, @Lang
FROM #Nums n
WHERE n.n <= @SeedTagCategories;

DECLARE @MinTagCatId INT = (SELECT MIN(Id) FROM dbo.TagCategories WHERE Name LIKE N'SEED %');
DECLARE @TagCatCount INT = (SELECT COUNT(*) FROM dbo.TagCategories WHERE Name LIKE N'SEED %');

INSERT INTO dbo.Tags (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, TagCategoryId)
SELECT
    N'SEED Tag ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    @MinTagCatId + ((n.n - 1) % @TagCatCount)
FROM #Nums n
WHERE n.n <= @SeedTags;

DECLARE @MinTagId INT = (SELECT MIN(Id) FROM dbo.Tags WHERE Name LIKE N'SEED %');
DECLARE @TagCount INT = (SELECT COUNT(*) FROM dbo.Tags WHERE Name LIKE N'SEED %');

/* ============================================================
   5) Brands
   ============================================================ */
PRINT N'Seeding Brands...';
INSERT INTO dbo.Brands
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId, MainPage)
SELECT
    N'SEED Brand ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Dummy brand description ' + CAST(n.n AS NVARCHAR(10)),
    1, N'seed,brand,' + CAST(n.n AS NVARCHAR(10)),
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @AdminUserId,
    CASE WHEN n.n <= 8 THEN 1 ELSE 0 END
FROM #Nums n
WHERE n.n <= @SeedBrands;

DECLARE @MinBrandId INT = (SELECT MIN(Id) FROM dbo.Brands WHERE Name LIKE N'SEED %');
DECLARE @BrandCount INT = (SELECT COUNT(*) FROM dbo.Brands WHERE Name LIKE N'SEED %');

/* ============================================================
   6) ProductCategories (tree: first @SeedCategoryRoots roots, rest children)
   ============================================================ */
PRINT N'Seeding ProductCategories...';
INSERT INTO dbo.ProductCategories
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
     ParentId, MainPage, ShortDescription, TemplateId, DiscountPercantage)
SELECT
    N'SEED ProductCategory ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Category description ' + CAST(n.n AS NVARCHAR(10)),
    1, N'seed,category',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @AdminUserId,
    CASE WHEN n.n <= @SeedCategoryRoots THEN 0 ELSE ((n.n - 1) % @SeedCategoryRoots) + 1 END,  -- temp child placeholder
    CASE WHEN n.n <= @SeedCategoryRoots THEN 1 ELSE 0 END,
    N'Short desc for category ' + CAST(n.n AS NVARCHAR(10)),
    @MinTemplateId + ((n.n - 1) % @TemplateCount),
    CASE WHEN n.n % 10 = 0 THEN 10.0 ELSE NULL END
FROM #Nums n
WHERE n.n <= @SeedProductCategories;

/* Fix ParentId for children to real root Ids */
DECLARE @MinCatId INT = (SELECT MIN(Id) FROM dbo.ProductCategories WHERE Name LIKE N'SEED %');
DECLARE @CatCount INT = (SELECT COUNT(*) FROM dbo.ProductCategories WHERE Name LIKE N'SEED %');

UPDATE pc
SET ParentId = CASE
    WHEN TRY_CAST(REPLACE(pc.Name, N'SEED ProductCategory ', N'') AS INT) <= @SeedCategoryRoots THEN 0
    ELSE @MinCatId + ((TRY_CAST(REPLACE(pc.Name, N'SEED ProductCategory ', N'') AS INT) - 1) % @SeedCategoryRoots)
END
FROM dbo.ProductCategories pc
WHERE pc.Name LIKE N'SEED ProductCategory %';

/* ============================================================
   7) Products
   ============================================================ */
PRINT N'Seeding Products...';

DECLARE @HasComputedRating BIT = 0;
IF EXISTS (
    SELECT 1 FROM sys.computed_columns cc
    INNER JOIN sys.tables t ON t.object_id = cc.object_id
    WHERE t.name = N'Products' AND cc.name = N'Rating'
)
    SET @HasComputedRating = 1;

IF @HasComputedRating = 1
BEGIN
    INSERT INTO dbo.Products
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
         NameShort, NameLong, ProductCategoryId, BrandId, MainPage, ShortDescription,
         Price, Discount, ProductCode, VideoUrl, IsCampaign, ProductColorOptions,
         State, ProductSizeOptions)
    SELECT
        N'SEED Product ' + CAST(n.n AS NVARCHAR(10)),
        DATEADD(DAY, -(n.n % 365), @Now), DATEADD(DAY, -(n.n % 30), @Now),
        CASE WHEN n.n % 50 = 0 THEN 0 ELSE 1 END,
        n.n, @Lang,
        N'<p>Dummy product description for product ' + CAST(n.n AS NVARCHAR(10)) + N'. Lorem ipsum dolor sit amet.</p>',
        1, N'seed,product,test',
        @MinFileId + ((n.n - 1) % @FileCount),
        @AdminUserId, @AdminUserId,
        N'Short ' + CAST(n.n AS NVARCHAR(10)),
        N'Long name for SEED Product ' + CAST(n.n AS NVARCHAR(10)),
        @MinCatId + ((n.n - 1) % @CatCount),
        @MinBrandId + ((n.n - 1) % @BrandCount),
        CASE WHEN n.n <= 12 THEN 1 ELSE 0 END,
        N'Short description ' + CAST(n.n AS NVARCHAR(10)),
        CAST((50.0 + (n.n % 950)) AS DECIMAL(18,2)),
        CASE WHEN n.n % 7 = 0 THEN CAST((5.0 + (n.n % 40)) AS DECIMAL(18,2)) ELSE NULL END,
        N'SKU-' + RIGHT(N'000000' + CAST(n.n AS NVARCHAR(6)), 6),
        CASE WHEN n.n % 20 = 0 THEN N'https://www.youtube.com/watch?v=dQw4w9WgXcQ' ELSE NULL END,
        CASE WHEN n.n % 11 = 0 THEN 1 ELSE 0 END,
        N'Red,Blue,Green',
        (SELECT State FROM @ProductStates WHERE i = n.n % 10),
        N'S,M,L,XL'
    FROM #Nums n
    WHERE n.n <= @SeedProducts;
END
ELSE
BEGIN
    INSERT INTO dbo.Products
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
         NameShort, NameLong, ProductCategoryId, BrandId, MainPage, ShortDescription,
         Price, Discount, ProductCode, VideoUrl, IsCampaign, ProductColorOptions,
         State, ProductSizeOptions, Rating)
    SELECT
        N'SEED Product ' + CAST(n.n AS NVARCHAR(10)),
        DATEADD(DAY, -(n.n % 365), @Now), DATEADD(DAY, -(n.n % 30), @Now),
        CASE WHEN n.n % 50 = 0 THEN 0 ELSE 1 END,
        n.n, @Lang,
        N'<p>Dummy product description for product ' + CAST(n.n AS NVARCHAR(10)) + N'. Lorem ipsum dolor sit amet.</p>',
        1, N'seed,product,test',
        @MinFileId + ((n.n - 1) % @FileCount),
        @AdminUserId, @AdminUserId,
        N'Short ' + CAST(n.n AS NVARCHAR(10)),
        N'Long name for SEED Product ' + CAST(n.n AS NVARCHAR(10)),
        @MinCatId + ((n.n - 1) % @CatCount),
        @MinBrandId + ((n.n - 1) % @BrandCount),
        CASE WHEN n.n <= 12 THEN 1 ELSE 0 END,
        N'Short description ' + CAST(n.n AS NVARCHAR(10)),
        CAST((50.0 + (n.n % 950)) AS DECIMAL(18,2)),
        CASE WHEN n.n % 7 = 0 THEN CAST((5.0 + (n.n % 40)) AS DECIMAL(18,2)) ELSE NULL END,
        N'SKU-' + RIGHT(N'000000' + CAST(n.n AS NVARCHAR(6)), 6),
        CASE WHEN n.n % 20 = 0 THEN N'https://www.youtube.com/watch?v=dQw4w9WgXcQ' ELSE NULL END,
        CASE WHEN n.n % 11 = 0 THEN 1 ELSE 0 END,
        N'Red,Blue,Green',
        (SELECT State FROM @ProductStates WHERE i = n.n % 10),
        N'S,M,L,XL',
        CAST((2.0 + (n.n % 30) / 10.0) AS FLOAT)
    FROM #Nums n
    WHERE n.n <= @SeedProducts;
END;

DECLARE @MinProductId INT = (SELECT MIN(Id) FROM dbo.Products WHERE Name LIKE N'SEED %');
DECLARE @ProductCount INT = (SELECT COUNT(*) FROM dbo.Products WHERE Name LIKE N'SEED %');

/* ============================================================
   8) ProductFiles / ProductTags / ProductSpecifications / ProductComments
   ============================================================ */
PRINT N'Seeding ProductFiles / ProductTags / ProductSpecifications / ProductComments...';

INSERT INTO dbo.ProductFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, FileStorageId, ProductId)
SELECT
    N'SEED ProductFile ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    @MinFileId + ((n.n - 1) % @FileCount),
    @MinProductId + ((n.n - 1) % @ProductCount)
FROM #Nums n
WHERE n.n <= @SeedProductFiles;

INSERT INTO dbo.ProductTags (TagId, ProductId)
SELECT
    @MinTagId + ((n.n - 1) % @TagCount),
    @MinProductId + ((n.n - 1) % @ProductCount)
FROM #Nums n
WHERE n.n <= @SeedProductTags;

INSERT INTO dbo.ProductSpecifications
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Value, Unit, ProductId)
SELECT
    CASE n.n % 5
        WHEN 0 THEN N'Color'
        WHEN 1 THEN N'Size'
        WHEN 2 THEN N'Weight'
        WHEN 3 THEN N'Material'
        ELSE N'Dimensions'
    END,
    @Now, @Now, 1, n.n, @Lang,
    CASE n.n % 5
        WHEN 0 THEN N'Red'
        WHEN 1 THEN N'M'
        WHEN 2 THEN CAST((100 + n.n % 900) AS NVARCHAR(10))
        WHEN 3 THEN N'Cotton'
        ELSE N'10x20x5'
    END,
    CASE n.n % 5 WHEN 2 THEN N'g' WHEN 4 THEN N'cm' ELSE N'' END,
    @MinProductId + ((n.n - 1) % @ProductCount)
FROM #Nums n
WHERE n.n <= @SeedProductSpecs;

INSERT INTO dbo.ProductComments
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     ProductId, UserId, Review, Email, Subject, Rating)
SELECT
    N'SEED Comment ' + CAST(n.n AS NVARCHAR(10)),
    DATEADD(HOUR, -n.n, @Now), DATEADD(HOUR, -n.n, @Now),
    CASE WHEN n.n % 15 = 0 THEN 0 ELSE 1 END,
    n.n, @Lang,
    @MinProductId + ((n.n - 1) % @ProductCount),
    CASE WHEN n.n = 1 THEN @Customer1Id ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(8)), 8) END,
    N'This is a dummy review for product testing. Comment #' + CAST(n.n AS NVARCHAR(10)),
    N'seeduser' + RIGHT(N'00000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(5)), 5) + N'@eimece.test',
    N'Review subject ' + CAST(n.n AS NVARCHAR(10)),
    1 + (n.n % 5)
FROM #Nums n
WHERE n.n <= @SeedProductComments;

/* ============================================================
   9) StoryCategories / Stories / StoryFiles / StoryTags
   ============================================================ */
PRINT N'Seeding Stories...';

INSERT INTO dbo.StoryCategories
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId, PageTheme)
SELECT
    N'SEED StoryCategory ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Story category ' + CAST(n.n AS NVARCHAR(10)),
    1, N'seed,story',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @AdminUserId,
    N'T' + CAST(1 + (n.n % 8) AS NVARCHAR(2))
FROM #Nums n
WHERE n.n <= @SeedStoryCategories;

DECLARE @MinStoryCatId INT = (SELECT MIN(Id) FROM dbo.StoryCategories WHERE Name LIKE N'SEED %');
DECLARE @StoryCatCount INT = (SELECT COUNT(*) FROM dbo.StoryCategories WHERE Name LIKE N'SEED %');

INSERT INTO dbo.Stories
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
     StoryCategoryId, MainPage, AuthorName, IsFeaturedStory, ShortDescription)
SELECT
    N'SEED Story ' + CAST(n.n AS NVARCHAR(10)),
    DATEADD(DAY, -(n.n % 200), @Now), @Now,
    1, n.n, @Lang,
    N'<p>Full story body for SEED Story ' + CAST(n.n AS NVARCHAR(10)) + N'.</p>',
    1, N'seed,blog',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @AdminUserId,
    @MinStoryCatId + ((n.n - 1) % @StoryCatCount),
    CASE WHEN n.n <= 15 THEN 1 ELSE 0 END,
    N'Author ' + CAST((n.n % 20) + 1 AS NVARCHAR(10)),
    CASE WHEN n.n <= 10 THEN 1 ELSE 0 END,
    N'Short story blurb ' + CAST(n.n AS NVARCHAR(10))
FROM #Nums n
WHERE n.n <= @SeedStories;

DECLARE @MinStoryId INT = (SELECT MIN(Id) FROM dbo.Stories WHERE Name LIKE N'SEED %');
DECLARE @StoryCount INT = (SELECT COUNT(*) FROM dbo.Stories WHERE Name LIKE N'SEED %');

INSERT INTO dbo.StoryFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, StoryId, FileStorageId)
SELECT
    N'SEED StoryFile ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    @MinStoryId + ((n.n - 1) % @StoryCount),
    @MinFileId + ((n.n - 1) % @FileCount)
FROM #Nums n
WHERE n.n <= @SeedStoryFiles;

INSERT INTO dbo.StoryTags (StoryId, TagId)
SELECT
    @MinStoryId + ((n.n - 1) % @StoryCount),
    @MinTagId + ((n.n - 1) % @TagCount)
FROM #Nums n
WHERE n.n <= @SeedStoryTags;

/* ============================================================
   10) Menus / MenuFiles / MainPageImages
   ============================================================ */
PRINT N'Seeding Menus / MainPageImages...';

INSERT INTO dbo.Menus
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
     ParentId, MainPage, MenuLink, Link, PageTheme, LinkIsActive)
SELECT
    N'SEED Menu ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now,
    1,  -- all seeded menus active; count stays small (@SeedMenus)
    n.n, @Lang,
    N'<p>Menu page content ' + CAST(n.n AS NVARCHAR(10)) + N'</p>',
    1, N'seed,menu',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @AdminUserId,
    0,
    CASE WHEN n.n <= 8 THEN 1 ELSE 0 END,
    N'pages-detail_' + CAST(n.n AS NVARCHAR(10)),
    CASE WHEN n.n % 6 = 0 THEN N'https://example.com/external-' + CAST(n.n AS NVARCHAR(10)) ELSE NULL END,
    N'T' + CAST(1 + (n.n % 8) AS NVARCHAR(2)),
    CASE WHEN n.n % 6 = 0 THEN 1 ELSE 0 END
FROM #Nums n
WHERE n.n <= @SeedMenus;

DECLARE @MinMenuId INT = (SELECT MIN(Id) FROM dbo.Menus WHERE Name LIKE N'SEED %');
DECLARE @MenuCount INT = (SELECT COUNT(*) FROM dbo.Menus WHERE Name LIKE N'SEED %');

INSERT INTO dbo.MenuFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, MenuId, FileStorageId)
SELECT
    N'SEED MenuFile ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    @MinMenuId + ((n.n - 1) % @MenuCount),
    @MinFileId + ((n.n - 1) % @FileCount)
FROM #Nums n
WHERE n.n <= @SeedMenuFiles;

INSERT INTO dbo.MainPageImages
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId, Link)
SELECT
    N'SEED MainPageImage ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now,
    1,
    n.n, @Lang,
    N'Slider image ' + CAST(n.n AS NVARCHAR(10)),
    1, N'seed,slider',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @AdminUserId,
    N'/c/SEED-ProductCategory-' + CAST(((n.n - 1) % @CatCount) + 1 AS NVARCHAR(10))
FROM #Nums n
WHERE n.n <= @SeedMainPageImages;

/* ============================================================
   11) FileStorageTags
   ============================================================ */
PRINT N'Seeding FileStorageTags...';
INSERT INTO dbo.FileStorageTags (FileStorageId, TagId)
SELECT
    @MinFileId + ((n.n - 1) % @FileCount),
    @MinTagId + ((n.n - 1) % @TagCount)
FROM #Nums n
WHERE n.n <= @SeedFileStorageTags;

/* ============================================================
   12) Settings (required keys + fillers)
   ============================================================ */
PRINT N'Seeding Settings...';

;WITH RequiredSettings AS (
    SELECT * FROM (VALUES
        (N'CompanyName', N'EImece Seed Shop', N'Company display name'),
        (N'CompanyAddress', N'Seed Street 1, Istanbul', N'Company address'),
        (N'WebSiteLogo', N'/images/logo.jpg', N'Logo path'),
        (N'WebSiteCompanyEmailAddress', N'info@eimece.test', N'Public email'),
        (N'WebSiteCompanyPhoneAndLocation', N'+90 212 000 0000 | Istanbul', N'Phone'),
        (N'CargoCompany', N'Yurtici Kargo', N'Cargo company'),
        (N'CargoPrice', N'49.90', N'Cargo price'),
        (N'BasketMinTotalPriceForCargo', N'500', N'Free cargo threshold'),
        (N'CargoDescription', N'Standart kargo 2-4 is gunu', N'Cargo description'),
        (N'SiteIndexMetaTitle', N'EImece Seed Shop', N'Meta title'),
        (N'SiteIndexMetaDescription', N'Dummy seed data storefront for testing', N'Meta description'),
        (N'SiteIndexMetaKeywords', N'eimece,seed,test', N'Meta keywords'),
        (N'IsProductPriceEnable', N'true', N'Show prices'),
        (N'IsProductReviewEnable', N'true', N'Show reviews'),
        (N'AdminEmail', N'admin@eimece.test', N'Admin email'),
        (N'AdminUserName', N'admin@eimece.test', N'Admin SMTP user'),
        (N'AdminEmailHost', N'smtp.example.com', N'SMTP host'),
        (N'AdminEmailPassword', N'seed-smtp-placeholder', N'SMTP credential placeholder'),
        (N'AdminEmailPort', N'587', N'SMTP port'),
        (N'AdminEmailEnableSsl', N'true', N'SMTP SSL'),
        (N'AdminEmailUseDefaultCredentials', N'false', N'SMTP default creds'),
        (N'AdminEmailDisplayName', N'EImece Seed Shop', N'SMTP display name'),
        (N'DefaultImageWidth', N'800', N'Default image width'),
        (N'DefaultImageHeight', N'600', N'Default image height'),
        (N'FooterDescription', N'Seed footer description', N'Footer'),
        (N'FooterHtmlDescription', N'<p>Seed footer HTML</p>', N'Footer HTML'),
        (N'FooterEmailListDescription', N'Subscribe to our newsletter', N'Footer email list'),
        (N'AboutUs', N'<p>About the seed shop</p>', N'About us'),
        (N'PrivacyPolicy', N'<p>Privacy policy seed</p>', N'Privacy'),
        (N'TermsAndConditions', N'<p>Terms seed</p>', N'Terms'),
        (N'DeliveryInfo', N'<p>Delivery info seed</p>', N'Delivery'),
        (N'FacebookWebSiteLink', N'https://facebook.com/', N'Facebook'),
        (N'InstagramWebSiteLink', N'https://instagram.com/', N'Instagram'),
        (N'TwitterWebSiteLink', N'https://twitter.com/', N'Twitter'),
        (N'LinkedinWebSiteLink', N'https://linkedin.com/', N'LinkedIn'),
        (N'YotubeWebSiteLink', N'https://youtube.com/', N'YouTube'),
        (N'PinterestWebSiteLink', N'https://pinterest.com/', N'Pinterest'),
        (N'WhatsAppCommunicationLink', N'https://wa.me/905550000000', N'WhatsApp')
    ) v(SettingKey, SettingValue, Description)
)
INSERT INTO dbo.Settings
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Description, SettingKey, SettingValue)
SELECT
    N'SEED Setting ' + rs.SettingKey,
    @Now, @Now, 1, 0, @Lang,
    rs.Description, rs.SettingKey, rs.SettingValue
FROM RequiredSettings rs
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Settings s
    WHERE s.SettingKey = rs.SettingKey AND s.Lang = @Lang
);

/* A few dummy settings for admin grid demos (not thousands) */
INSERT INTO dbo.Settings
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Description, SettingKey, SettingValue)
SELECT
    N'SEED Setting Dummy ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Dummy setting for admin grid demos',
    N'SEED_Dummy_' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
    N'value-' + CAST(n.n AS NVARCHAR(10))
FROM #Nums n
WHERE n.n <= @SeedSettingFillers;

/* ============================================================
   13) MailTemplates (required Names + fillers)
   ============================================================ */
PRINT N'Seeding MailTemplates...';

;WITH RequiredMails AS (
    SELECT * FROM (VALUES
        (N'OrderConfirmationEmail', N'Siparis Onayi #{OrderNumber}', N'<p>Merhaba, siparisiniz alindi.</p>'),
        (N'CompanyGotNewOrderEmail', N'Yeni Siparis #{OrderNumber}', N'<p>Yeni bir siparis var.</p>'),
        (N'ConfirmYourAccount', N'Hesabinizi Onaylayin', N'<p>Lutfen hesabinizi onaylayin: {CallbackUrl}</p>'),
        (N'ForgotPassword', N'Sifre Sifirlama', N'<p>Sifre sifirlama linki: {CallbackUrl}</p>'),
        (N'ContactUsAboutProductInfo', N'Urun Bilgi Talebi', N'<p>Urun hakkinda mesaj</p>'),
        (N'ContactUsForCommunication', N'Iletisim Formu', N'<p>Iletisim mesaji</p>'),
        (N'SendMessageToSeller', N'Saticiya Mesaj', N'<p>Satici mesaji</p>')
    ) v(Name, Subject, Body)
)
INSERT INTO dbo.MailTemplates
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
SELECT
    rm.Name, @Now, @Now, 1, 0, @Lang, rm.Subject, rm.Body, @AdminUserId, N'SEED', 0, 0
FROM RequiredMails rm
WHERE NOT EXISTS (SELECT 1 FROM dbo.MailTemplates mt WHERE mt.Name = rm.Name);

INSERT INTO dbo.MailTemplates
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
SELECT
    N'SEED MailTemplate ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Seed subject ' + CAST(n.n AS NVARCHAR(10)),
    N'<p>Seed mail body ' + CAST(n.n AS NVARCHAR(10)) + N'</p>',
    @AdminUserId, N'SEED', 0, 0
FROM #Nums n
WHERE n.n <= @SeedMailTemplateFillers;

/* ============================================================
   14) Lists / ListItems
   ============================================================ */
PRINT N'Seeding Lists / ListItems...';
INSERT INTO dbo.Lists (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, IsService, IsValues)
SELECT
    N'SEED List ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    CASE WHEN n.n % 2 = 0 THEN 1 ELSE 0 END,
    CASE WHEN n.n % 3 = 0 THEN 1 ELSE 0 END
FROM #Nums n
WHERE n.n <= @SeedLists;

DECLARE @MinListId INT = (SELECT MIN(Id) FROM dbo.Lists WHERE Name LIKE N'SEED %');
DECLARE @ListCount INT = (SELECT COUNT(*) FROM dbo.Lists WHERE Name LIKE N'SEED %');

INSERT INTO dbo.ListItems (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, ListId, Value)
SELECT
    N'SEED ListItem ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    @MinListId + ((n.n - 1) % @ListCount),
    N'value-' + CAST(n.n AS NVARCHAR(10))
FROM #Nums n
WHERE n.n <= @SeedListItems;

/* ============================================================
   15) Faqs / Subscribers / Coupons
   ============================================================ */
PRINT N'Seeding Faqs / Subscribers / Coupons...';

INSERT INTO dbo.Faqs
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Question, Answer, AddUserId, UpdateUserId)
SELECT
    N'SEED Faq ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Seed question #' + CAST(n.n AS NVARCHAR(10)) + N'?',
    N'<p>Seed answer for question ' + CAST(n.n AS NVARCHAR(10)) + N'.</p>',
    @AdminUserId, @AdminUserId
FROM #Nums n
WHERE n.n <= @SeedFaqs;

INSERT INTO dbo.Subscribers
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Email, Note)
SELECT
    N'SEED Subscriber ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'subscriber' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test',
    N'Seed subscriber note ' + CAST(n.n AS NVARCHAR(10))
FROM #Nums n
WHERE n.n <= @SeedSubscribers;

INSERT INTO dbo.Coupons
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Code, DiscountPercentage, Discount, StartDate, EndDate)
SELECT
    N'SEED Coupon ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now,
    CASE WHEN n.n % 20 = 0 THEN 0 ELSE 1 END,
    n.n, @Lang,
    N'SEED' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
    CASE WHEN n.n % 2 = 0 THEN 10 + (n.n % 40) ELSE 0 END,
    CASE WHEN n.n % 2 = 1 THEN 25 + (n.n % 100) ELSE 0 END,
    DATEADD(DAY, -30, @Now),
    DATEADD(DAY, 90 + (n.n % 180), @Now)
FROM #Nums n
WHERE n.n <= @SeedCoupons;

/* ============================================================
   16) Customers / Addresses
   ============================================================ */
PRINT N'Seeding Customers / Addresses...';

INSERT INTO dbo.Customers
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Surname, GsmNumber, Email, IdentityNumber, Ip, UserId, IsPermissionGranted,
     Gender, Street, Town, District, City, Country, ZipCode, Description, Company, CustomerType)
SELECT
    N'SEED Customer ' + CAST(n.n AS NVARCHAR(10)),
    DATEADD(DAY, -(n.n % 400), @Now), @Now,
    1, n.n, @Lang,
    N'Surname' + CAST(n.n AS NVARCHAR(10)),
    N'05' + RIGHT(N'000000000' + CAST((500000000 + n.n) AS NVARCHAR(9)), 9),
    CASE WHEN n.n = 1 THEN N'customer1@eimece.test'
         ELSE N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test' END,
    RIGHT(N'00000000000' + CAST((10000000000 + n.n) AS NVARCHAR(11)), 11),
    N'127.0.0.' + CAST((n.n % 254) + 1 AS NVARCHAR(3)),
    CASE WHEN n.n = 1 THEN @Customer1Id
         ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(n.n AS NVARCHAR(8)), 8) END,
    1,
    n.n % 3,
    N'Seed Street ' + CAST(n.n AS NVARCHAR(10)),
    c.District,
    c.District,
    c.City,
    N'Turkiye',
    RIGHT(N'00000' + CAST((34000 + n.n % 1000) AS NVARCHAR(5)), 5),
    N'Apartment ' + CAST((n.n % 50) + 1 AS NVARCHAR(10)),
    CASE WHEN n.n % 8 = 0 THEN N'Seed Company ' + CAST(n.n AS NVARCHAR(10)) ELSE NULL END,
    CASE WHEN n.n % 8 = 0 THEN 2 ELSE 1 END
FROM #Nums n
INNER JOIN #Cities c ON c.i = n.n % 10
WHERE n.n <= @SeedCustomers;

/* Addresses: half shipping (1), half billing (2) — Name marker SEED */
INSERT INTO dbo.Addresses
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, AddressType, City, Country, ZipCode, Street, District)
SELECT
    N'SEED Address ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Open address line for seed address ' + CAST(n.n AS NVARCHAR(10)),
    CASE WHEN n.n % 2 = 1 THEN 1 ELSE 2 END,
    c.City,
    N'Turkiye',
    RIGHT(N'00000' + CAST((34000 + n.n % 1000) AS NVARCHAR(5)), 5),
    N'Street ' + CAST(n.n AS NVARCHAR(10)),
    c.District
FROM #Nums n
INNER JOIN #Cities c ON c.i = n.n % 10
WHERE n.n <= @SeedAddresses;

DECLARE @MinAddressId INT = (SELECT MIN(Id) FROM dbo.Addresses WHERE Name LIKE N'SEED %');
DECLARE @AddressCount INT = (SELECT COUNT(*) FROM dbo.Addresses WHERE Name LIKE N'SEED %');

/* ============================================================
   17) Orders / OrderProducts / ShoppingCarts
   ============================================================ */
PRINT N'Seeding Orders / OrderProducts / ShoppingCarts...';

INSERT INTO dbo.Orders
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     DeliveryDate, UserId, OrderType, OrderStatus, AdminOrderNote, OrderComments,
     OrderNumber, CargoPrice, ShippingAddressId, BillingAddressId, OrderGuid,
     Coupon, CouponDiscount, Token, Price, PaidPrice, Installment, Currency,
     PaymentId, PaymentStatus, FraudStatus, MerchantCommissionRate, MerchantCommissionRateAmount,
     IyziCommissionRateAmount, IyziCommissionFee, CardType, CardAssociation, CardFamily,
     CardToken, CardUserKey, BinNumber, LastFourDigits, BasketId, ConversationId,
     ConnectorName, AuthCode, HostReference, Phase, Status, ErrorCode, ErrorMessage,
     Locale, SystemTime, ShipmentTrackingNumber, ShipmentCompanyName)
SELECT
    N'SEED Order ' + CAST(n.n AS NVARCHAR(10)),
    DATEADD(DAY, -(n.n % 180), @Now),
    DATEADD(DAY, -(n.n % 180), @Now),
    1, n.n, @Lang,
    DATEADD(DAY, 3 + (n.n % 10), DATEADD(DAY, -(n.n % 180), @Now)),
    CASE WHEN n.n = 1 THEN @Customer1Id
         WHEN n.n % 17 = 0 THEN N'BNC'
         WHEN n.n % 19 = 0 THEN N'SWA'
         ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(8)), 8) END,
    1 + (n.n % 3),                 -- OrderType 1..3
    1 + (n.n % 8),                 -- OrderStatus 1..8
    CASE WHEN n.n % 10 = 0 THEN N'Admin note ' + CAST(n.n AS NVARCHAR(10)) ELSE NULL END,
    N'Customer comment ' + CAST(n.n AS NVARCHAR(10)),
    N'ORD-' + RIGHT(N'0000000' + CAST(n.n AS NVARCHAR(7)), 7),
    CAST(CASE WHEN (100 + (n.n % 900)) >= 500 THEN 0 ELSE 49.90 END AS DECIMAL(18,2)),
    @MinAddressId + ((n.n - 1) % @AddressCount),
    @MinAddressId + (n.n % @AddressCount),
    LOWER(CONVERT(NVARCHAR(36), NEWID())),
    CASE WHEN n.n % 12 = 0 THEN N'SEED' + RIGHT(N'00000' + CAST(((n.n - 1) % @SeedCoupons) + 1 AS NVARCHAR(5)), 5) ELSE NULL END,
    CASE WHEN n.n % 12 = 0 THEN N'10' ELSE NULL END,
    LOWER(CONVERT(NVARCHAR(64), NEWID())),
    CAST(CAST((100.0 + (n.n % 900)) AS DECIMAL(18,2)) AS NVARCHAR(50)),
    CAST(CAST((100.0 + (n.n % 900)) AS DECIMAL(18,2)) AS NVARCHAR(50)),
    CAST(1 + (n.n % 6) AS NVARCHAR(10)),
    N'TRY',
    N'pay_' + CAST(n.n AS NVARCHAR(10)),
    CASE WHEN n.n % 9 = 0 THEN N'FAILED' ELSE N'SUCCESS' END,
    CASE WHEN n.n % 30 = 0 THEN 1 ELSE 0 END,
    N'2.5', N'5.00', N'3.00', N'1.50',
    N'CREDIT_CARD', N'MASTER_CARD', N'Bonus',
    NULL, NULL,
    N'554960',
    RIGHT(N'0000' + CAST((n.n % 10000) AS NVARCHAR(4)), 4),
    N'basket_' + CAST(n.n AS NVARCHAR(10)),
    N'conv_' + CAST(n.n AS NVARCHAR(10)),
    NULL, N'AUTH' + CAST(n.n AS NVARCHAR(10)), NULL, N'AUTH',
    CASE WHEN n.n % 9 = 0 THEN N'failure' ELSE N'success' END,
    CASE WHEN n.n % 9 = 0 THEN N'5001' ELSE NULL END,
    CASE WHEN n.n % 9 = 0 THEN N'Seed payment error' ELSE NULL END,
    N'tr',
    CAST(DATEDIFF(SECOND, '1970-01-01', DATEADD(DAY, -(n.n % 180), @Now)) AS BIGINT) * 1000,
    CASE WHEN (1 + (n.n % 8)) >= 4 THEN N'TRK' + RIGHT(N'000000000' + CAST(n.n AS NVARCHAR(9)), 9) ELSE NULL END,
    CASE WHEN (1 + (n.n % 8)) >= 4 THEN N'Yurtici Kargo' ELSE NULL END
FROM #Nums n
WHERE n.n <= @SeedOrders;

DECLARE @MinOrderId INT = (SELECT MIN(Id) FROM dbo.Orders WHERE Name LIKE N'SEED %');
DECLARE @OrderCount INT = (SELECT COUNT(*) FROM dbo.Orders WHERE Name LIKE N'SEED %');

INSERT INTO dbo.OrderProducts
    (OrderId, ProductId, Quantity, TotalPrice, ProductSalePrice, ProductName, ProductCode, CategoryName, ProductSpecItems)
SELECT
    @MinOrderId + ((n.n - 1) % @OrderCount),
    @MinProductId + ((n.n - 1) % @ProductCount),
    1 + (n.n % 5),
    CAST((1 + (n.n % 5)) * (50.0 + (n.n % 200)) AS DECIMAL(18,2)),
    CAST((50.0 + (n.n % 200)) AS DECIMAL(18,2)),
    N'SEED Product ' + CAST(((n.n - 1) % @ProductCount) + 1 AS NVARCHAR(10)),
    N'SKU-' + RIGHT(N'000000' + CAST(((n.n - 1) % @ProductCount) + 1 AS NVARCHAR(6)), 6),
    N'SEED ProductCategory ' + CAST(((n.n - 1) % @CatCount) + 1 AS NVARCHAR(10)),
    N'[{"Name":"Color","Value":"Red"}]'
FROM #Nums n
WHERE n.n <= @SeedOrderProducts;

INSERT INTO dbo.ShoppingCarts
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, OrderGuid, ShoppingCartJson, UserId)
SELECT
    N'SEED ShoppingCart ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    LOWER(CONVERT(NVARCHAR(36), NEWID())),
    N'{"Items":[{"ProductId":' + CAST(@MinProductId + ((n.n - 1) % @ProductCount) AS NVARCHAR(20))
        + N',"Quantity":' + CAST(1 + (n.n % 3) AS NVARCHAR(10)) + N'}]}',
    CASE WHEN n.n = 1 THEN @Customer1Id
         ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(8)), 8) END
FROM #Nums n
WHERE n.n <= @SeedShoppingCarts;

/* ============================================================
   18) Browser push stack (optional tables)
   ============================================================ */
IF OBJECT_ID(N'dbo.BrowserSubscriptions', N'U') IS NOT NULL
BEGIN
    PRINT N'Seeding Browser* tables...';

    INSERT INTO dbo.BrowserSubscriptions
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, BrowserType, PublicKey, PrivateKey)
    SELECT
        N'SEED BrowserSubscription ' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, n.n, @Lang,
        N'mailto:admin@eimece.test',
        n.n % 3,
        N'seed-public-key-' + CAST(n.n AS NVARCHAR(10)),
        N'seed-private-key-' + CAST(n.n AS NVARCHAR(10))
    FROM #Nums n
    WHERE n.n <= @SeedBrowserSubscriptions;

    DECLARE @MinBrowserSubId INT = (SELECT MIN(Id) FROM dbo.BrowserSubscriptions WHERE Name LIKE N'SEED %');
    DECLARE @BrowserSubCount INT = (SELECT COUNT(*) FROM dbo.BrowserSubscriptions WHERE Name LIKE N'SEED %');

    INSERT INTO dbo.BrowserSubscribers
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         BrowserSubscriptionId, EndPoint, Auth, P256dh, UserAgent, UserAddress)
    SELECT
        N'SEED BrowserSubscriber ' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, n.n, @Lang,
        @MinBrowserSubId + ((n.n - 1) % @BrowserSubCount),
        N'https://push.example.com/endpoint/' + CAST(n.n AS NVARCHAR(10)),
        N'auth' + CAST(n.n AS NVARCHAR(10)),
        N'p256dh' + CAST(n.n AS NVARCHAR(10)),
        N'Mozilla/5.0 SeedBrowser',
        N'127.0.0.' + CAST((n.n % 254) + 1 AS NVARCHAR(3))
    FROM #Nums n
    WHERE n.n <= @SeedBrowserSubscribers;

    DECLARE @MinBrowserSubscriberId INT = (SELECT MIN(Id) FROM dbo.BrowserSubscribers WHERE Name LIKE N'SEED %');
    DECLARE @BrowserSubscriberCount INT = (SELECT COUNT(*) FROM dbo.BrowserSubscribers WHERE Name LIKE N'SEED %');

    INSERT INTO dbo.BrowserNotifications
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         NotificationType, Body, ImageUrl, RedirectionUrl)
    SELECT
        N'SEED BrowserNotification ' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, n.n, @Lang,
        n.n % 5,
        N'Seed notification body ' + CAST(n.n AS NVARCHAR(10)),
        N'/media/images/seed-image-' + CAST(((n.n - 1) % @FileCount) + 1 AS NVARCHAR(10)) + N'.jpg',
        N'/p/SEED-Product-' + CAST(((n.n - 1) % @ProductCount) + 1 AS NVARCHAR(10))
    FROM #Nums n
    WHERE n.n <= @SeedBrowserNotifications;

    DECLARE @MinBrowserNotificationId INT = (SELECT MIN(Id) FROM dbo.BrowserNotifications WHERE Name LIKE N'SEED %');
    DECLARE @BrowserNotificationCount INT = (SELECT COUNT(*) FROM dbo.BrowserNotifications WHERE Name LIKE N'SEED %');

    INSERT INTO dbo.BrowserNotificationFeedBacks
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         BrowserNotificationId, BrowserSubscriberId, NotificationStatus, DateSend, DateTracked)
    SELECT
        N'SEED BrowserNotificationFeedBack ' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, n.n, @Lang,
        @MinBrowserNotificationId + ((n.n - 1) % @BrowserNotificationCount),
        @MinBrowserSubscriberId + ((n.n - 1) % @BrowserSubscriberCount),
        n.n % 4,
        DATEADD(HOUR, -n.n, @Now),
        CASE WHEN n.n % 3 = 0 THEN DATEADD(HOUR, -(n.n - 1), @Now) ELSE NULL END
    FROM #Nums n
    WHERE n.n <= @SeedBrowserFeedbacks;
END;

/* ============================================================
   19) ShortUrls / AppLogs (if present)
   ============================================================ */
IF OBJECT_ID(N'dbo.ShortUrls', N'U') IS NOT NULL
BEGIN
    PRINT N'Seeding ShortUrls...';
    INSERT INTO dbo.ShortUrls
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, UrlKey, Url, RequestCount)
    SELECT
        N'SEED ShortUrl ' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, n.n, @Lang,
        N's' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
        N'https://example.com/long-url/' + CAST(n.n AS NVARCHAR(10)),
        n.n % 1000
    FROM #Nums n
    WHERE n.n <= @SeedShortUrls;
END;

IF OBJECT_ID(N'dbo.AppLogs', N'U') IS NOT NULL
BEGIN
    PRINT N'Seeding AppLogs...';
    IF COL_LENGTH(N'dbo.AppLogs', N'CreatedDate') IS NOT NULL
    BEGIN
        INSERT INTO dbo.AppLogs
            (EventDateTime, EventLevel, UserName, MachineName, EventMessage,
             ErrorSource, ErrorClass, ErrorMethod, ErrorMessage, InnerErrorMessage, CreatedDate)
        SELECT
            CONVERT(VARCHAR(30), DATEADD(MINUTE, -n.n, @Now), 121),
            CASE n.n % 5 WHEN 0 THEN N'Error' WHEN 1 THEN N'Warn' WHEN 2 THEN N'Info' WHEN 3 THEN N'Debug' ELSE N'Fatal' END,
            N'seeduser' + RIGHT(N'00000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(5)), 5),
            N'SEED-MACHINE',
            N'SEED log message ' + CAST(n.n AS NVARCHAR(10)),
            CASE WHEN n.n % 5 = 0 THEN N'EImece.Domain' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'SomeService' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'DoWork' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'Seed exception message' ELSE NULL END,
            CASE WHEN n.n % 10 = 0 THEN N'Seed inner exception' ELSE NULL END,
            DATEADD(MINUTE, -n.n, @Now)
        FROM #Nums n
        WHERE n.n <= @SeedAppLogs;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AppLogs
            (EventDateTime, EventLevel, UserName, MachineName, EventMessage,
             ErrorSource, ErrorClass, ErrorMethod, ErrorMessage, InnerErrorMessage)
        SELECT
            CONVERT(VARCHAR(30), DATEADD(MINUTE, -n.n, @Now), 121),
            CASE n.n % 5 WHEN 0 THEN N'Error' WHEN 1 THEN N'Warn' WHEN 2 THEN N'Info' WHEN 3 THEN N'Debug' ELSE N'Fatal' END,
            N'seeduser' + RIGHT(N'00000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(5)), 5),
            N'SEED-MACHINE',
            N'SEED log message ' + CAST(n.n AS NVARCHAR(10)),
            CASE WHEN n.n % 5 = 0 THEN N'EImece.Domain' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'SomeService' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'DoWork' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'Seed exception message' ELSE NULL END,
            CASE WHEN n.n % 10 = 0 THEN N'Seed inner exception' ELSE NULL END
        FROM #Nums n
        WHERE n.n <= @SeedAppLogs;
    END
END;

COMMIT TRANSACTION;

/* ========================= SUMMARY ========================= */
PRINT N'';
PRINT N'========== SEED SUMMARY ==========';
SELECT N'AspNetUsers (seed)' AS [Table], COUNT(*) AS [Rows] FROM dbo.AspNetUsers WHERE UserName LIKE N'seed%' OR Email LIKE N'%@eimece.test'
UNION ALL SELECT N'FileStorages', COUNT(*) FROM dbo.FileStorages WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Templates', COUNT(*) FROM dbo.Templates WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'TagCategories', COUNT(*) FROM dbo.TagCategories WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Tags', COUNT(*) FROM dbo.Tags WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Brands', COUNT(*) FROM dbo.Brands WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'ProductCategories', COUNT(*) FROM dbo.ProductCategories WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Products', COUNT(*) FROM dbo.Products WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'ProductFiles', COUNT(*) FROM dbo.ProductFiles WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'ProductTags', COUNT(*) FROM dbo.ProductTags pt INNER JOIN dbo.Products p ON p.Id = pt.ProductId WHERE p.Name LIKE N'SEED %'
UNION ALL SELECT N'ProductSpecifications', COUNT(*) FROM dbo.ProductSpecifications WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'ProductComments', COUNT(*) FROM dbo.ProductComments WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'StoryCategories', COUNT(*) FROM dbo.StoryCategories WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Stories', COUNT(*) FROM dbo.Stories WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'StoryFiles', COUNT(*) FROM dbo.StoryFiles WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Menus', COUNT(*) FROM dbo.Menus WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'MainPageImages', COUNT(*) FROM dbo.MainPageImages WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Settings', COUNT(*) FROM dbo.Settings WHERE Name LIKE N'SEED %' OR SettingKey LIKE N'SEED_%'
UNION ALL SELECT N'MailTemplates', COUNT(*) FROM dbo.MailTemplates WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %'
UNION ALL SELECT N'Lists', COUNT(*) FROM dbo.Lists WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'ListItems', COUNT(*) FROM dbo.ListItems WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Faqs', COUNT(*) FROM dbo.Faqs WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Subscribers', COUNT(*) FROM dbo.Subscribers WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Coupons', COUNT(*) FROM dbo.Coupons WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Customers', COUNT(*) FROM dbo.Customers WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Addresses', COUNT(*) FROM dbo.Addresses WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'Orders', COUNT(*) FROM dbo.Orders WHERE Name LIKE N'SEED %'
UNION ALL SELECT N'OrderProducts', COUNT(*) FROM dbo.OrderProducts op INNER JOIN dbo.Orders o ON o.Id = op.OrderId WHERE o.Name LIKE N'SEED %'
UNION ALL SELECT N'ShoppingCarts', COUNT(*) FROM dbo.ShoppingCarts WHERE Name LIKE N'SEED %'
ORDER BY [Table];

PRINT N'';
PRINT N'Test logins (shared seed credential = N''Test'' + N''123'' + N''!''):';
PRINT N'  admin@eimece.test / Admin';
PRINT N'  editor@eimece.test / NormalUser';
PRINT N'  customer1@eimece.test / Customer';
PRINT CONVERT(VARCHAR(30), GETDATE(), 121) + N' — Seed complete.';

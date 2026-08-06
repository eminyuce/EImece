/*
================================================================================
  EImece — Seed Dummy Data for Manual / Demo Testing
================================================================================
  Inserts realistic volumes of related demo data so the storefront and admin
  feel like a small-to-medium shop (not thousands of menus/settings/brands).

  Values are production-like (believable product/brand/category names, prices,
  Turkish customer names, etc.). Cleanup uses technical markers — not Name
  prefixes — so the UI is not littered with "SEED Product 1" placeholders:
    - AddUserId = N'SEED'          (catalog / CMS content)
    - FileUrl LIKE N'/media/seed/%'
    - Email / UserName @eimece.test / seed*
    - Coupon Code LIKE N'EIMC-%'
    - OrderNumber LIKE N'EIMC-%'
    - TemplateXml contains <!--eimece-seed-->
    - SettingKey LIKE N'SEED_%' or N'__EIMECE_SEED%'

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
    .\RunSeedDummyData.ps1 -ConnectionString "..." -Scale 2   -- larger catalog/orders

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
  - Structural tables (Menus, MainPageImages, Templates, Settings, MailTemplates)
    use small fixed counts so the site stays usable; @Scale does not inflate them.
  - Products get a BrandId from seed brands (hash-distributed) and every
    seed brand is guaranteed at least one product.
  - Products get 1–4 seed tags via ProductTags; stories/blogs get 1–3 via StoryTags.
  - Product.Rating is omitted when the column is computed; otherwise set explicitly.
  - Script is idempotent when @CleanupFirst = 1.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ========================= CONFIG ========================= */
DECLARE @Scale         FLOAT        = 1.0;    -- multiplies catalog/order bulk tables only
DECLARE @CleanupFirst  BIT          = 1;      -- 1 = wipe previous seed data first
DECLARE @Lang          INT          = 1;      -- 1=TR, 2=EN
DECLARE @Now           DATETIME     = GETDATE();
DECLARE @SeedMarker    NVARCHAR(32) = N'SEED';
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
DECLARE @SeedStoryFiles         INT = CASE WHEN CAST(ROUND(40  * @Scale, 0) AS INT) < 1 THEN 1 ELSE CAST(ROUND(40 * @Scale, 0) AS INT) END;
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
    PRINT N'Running cleanup of previous seed data...';


    IF OBJECT_ID(N'dbo.BrowserNotificationFeedBacks', N'U') IS NOT NULL
        DELETE FROM dbo.BrowserNotificationFeedBacks WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.BrowserNotifications', N'U') IS NOT NULL
        DELETE FROM dbo.BrowserNotifications WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.BrowserSubscribers', N'U') IS NOT NULL
        DELETE FROM dbo.BrowserSubscribers WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.BrowserSubscriptions', N'U') IS NOT NULL
        DELETE FROM dbo.BrowserSubscriptions WHERE Position >= 900000 AND Position < 910000 OR Subject = N'mailto:admin@eimece.test' OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.OrderProducts', N'U') IS NOT NULL
        DELETE op FROM dbo.OrderProducts op INNER JOIN dbo.Orders o ON o.Id = op.OrderId WHERE o.OrderNumber LIKE N'EIMC-%' OR o.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
        DELETE FROM dbo.Orders WHERE OrderNumber LIKE N'EIMC-%' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ShoppingCarts', N'U') IS NOT NULL
        DELETE FROM dbo.ShoppingCarts WHERE UserId LIKE N'seed%' OR Name LIKE N'SEED %' OR UserId IN (N'seed-admin-000000000001', N'seed-editor-00000000001', N'seed-customer-0000000001');

    IF OBJECT_ID(N'dbo.ProductComments', N'U') IS NOT NULL
        DELETE FROM dbo.ProductComments WHERE Email LIKE N'%@eimece.test' OR UserId LIKE N'seed%' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductSpecifications', N'U') IS NOT NULL
        DELETE ps FROM dbo.ProductSpecifications ps INNER JOIN dbo.Products p ON p.Id = ps.ProductId WHERE p.AddUserId = N'SEED' OR p.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductTags', N'U') IS NOT NULL
        DELETE pt FROM dbo.ProductTags pt INNER JOIN dbo.Products p ON p.Id = pt.ProductId WHERE p.AddUserId = N'SEED' OR p.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductFiles', N'U') IS NOT NULL
        DELETE pf FROM dbo.ProductFiles pf INNER JOIN dbo.Products p ON p.Id = pf.ProductId WHERE p.AddUserId = N'SEED' OR p.Name LIKE N'SEED %' OR pf.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
        DELETE FROM dbo.Products WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ProductCategories', N'U') IS NOT NULL
        DELETE FROM dbo.ProductCategories WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Brands', N'U') IS NOT NULL
        DELETE FROM dbo.Brands WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.StoryTags', N'U') IS NOT NULL
        DELETE st FROM dbo.StoryTags st INNER JOIN dbo.Stories s ON s.Id = st.StoryId WHERE s.AddUserId = N'SEED' OR s.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.StoryFiles', N'U') IS NOT NULL
        DELETE sf FROM dbo.StoryFiles sf INNER JOIN dbo.Stories s ON s.Id = sf.StoryId WHERE s.AddUserId = N'SEED' OR s.Name LIKE N'SEED %' OR sf.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Stories', N'U') IS NOT NULL
        DELETE FROM dbo.Stories WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.StoryCategories', N'U') IS NOT NULL
        DELETE FROM dbo.StoryCategories WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.MenuFiles', N'U') IS NOT NULL
        DELETE mf FROM dbo.MenuFiles mf INNER JOIN dbo.Menus m ON m.Id = mf.MenuId WHERE m.AddUserId = N'SEED' OR m.Name LIKE N'SEED %' OR mf.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Menus', N'U') IS NOT NULL
        DELETE FROM dbo.Menus WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.MainPageImages', N'U') IS NOT NULL
        DELETE FROM dbo.MainPageImages WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.FileStorageTags', N'U') IS NOT NULL
        DELETE fst FROM dbo.FileStorageTags fst INNER JOIN dbo.FileStorages fs ON fs.Id = fst.FileStorageId WHERE fs.FileUrl LIKE N'/media/seed/%' OR fs.Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Tags', N'U') IS NOT NULL
        DELETE FROM dbo.Tags WHERE Position >= 900000 AND Position < 910000 OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.TagCategories', N'U') IS NOT NULL
        DELETE FROM dbo.TagCategories WHERE Position >= 900000 AND Position < 910000 OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.ListItems', N'U') IS NOT NULL
        DELETE FROM dbo.ListItems WHERE Position >= 900000 AND Position < 910000 OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Lists', N'U') IS NOT NULL
        DELETE FROM dbo.Lists WHERE Position >= 900000 AND Position < 910000 OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.Faqs', N'U') IS NOT NULL
        DELETE FROM dbo.Faqs WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Subscribers', N'U') IS NOT NULL
        DELETE FROM dbo.Subscribers WHERE Email LIKE N'%@eimece.test' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Coupons', N'U') IS NOT NULL
        DELETE FROM dbo.Coupons WHERE Code LIKE N'EIMC-%' OR Name LIKE N'SEED %' OR Code LIKE N'SEED%';
    IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
        DELETE FROM dbo.Customers WHERE Email LIKE N'%@eimece.test' OR UserId LIKE N'seed%' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Addresses', N'U') IS NOT NULL
        DELETE FROM dbo.Addresses WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';

    IF OBJECT_ID(N'dbo.MailTemplates', N'U') IS NOT NULL
        DELETE FROM dbo.MailTemplates WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.Settings', N'U') IS NOT NULL
        DELETE FROM dbo.Settings WHERE Name LIKE N'Demo: %' OR Name LIKE N'SEED %' OR SettingKey LIKE N'SEED_%' OR SettingKey = N'__EIMECE_SEED__';
    IF OBJECT_ID(N'dbo.Templates', N'U') IS NOT NULL
        DELETE FROM dbo.Templates WHERE TemplateXml LIKE N'%<!--eimece-seed-->%' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.FileStorages', N'U') IS NOT NULL
        DELETE FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.ShortUrls', N'U') IS NOT NULL
        DELETE FROM dbo.ShortUrls WHERE Position >= 900000 AND Position < 910000 OR Url LIKE N'https://eimece.test/%' OR Name LIKE N'SEED %';
    IF OBJECT_ID(N'dbo.AppLogs', N'U') IS NOT NULL
        DELETE FROM dbo.AppLogs WHERE UserName LIKE N'seed%' OR EventMessage LIKE N'SEED %';

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
E2(n) AS (SELECT 1 FROM E1 a CROSS JOIN E1 b),
E3(n) AS (SELECT 1 FROM E2 a CROSS JOIN E2 b),
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
 (0,N'İstanbul',N'Kadıköy'),(1,N'Ankara',N'Çankaya'),(2,N'İzmir',N'Karşıyaka'),
 (3,N'Bursa',N'Nilüfer'),(4,N'Antalya',N'Muratpaşa'),(5,N'Adana',N'Seyhan'),
 (6,N'Gaziantep',N'Şahinbey'),(7,N'Konya',N'Selçuklu'),(8,N'Trabzon',N'Ortahisar'),
 (9,N'Eskişehir',N'Tepebaşı');

DECLARE @ProductStates TABLE (i INT, State NVARCHAR(50));
INSERT INTO @ProductStates VALUES
 (0,N'ProductInStock'),(1,N'ProductOutOfStock'),(2,N'PreOrder'),(3,N'Discontinued'),
 (4,N'Backorder'),(5,N'ComingSoon'),(6,N'LimitedStock'),(7,N'Reserved'),
 (8,N'AwaitingRestock'),(9,N'NotForSale');

/* ---- Realistic name / catalog lookups ---- */
IF OBJECT_ID(N'tempdb..#FirstNames') IS NOT NULL DROP TABLE #FirstNames;
CREATE TABLE #FirstNames (i INT NOT NULL PRIMARY KEY, Name NVARCHAR(50) NOT NULL);

INSERT INTO #FirstNames(i, Name) VALUES (0,N'Ayşe');
INSERT INTO #FirstNames(i, Name) VALUES (1,N'Mehmet');
INSERT INTO #FirstNames(i, Name) VALUES (2,N'Elif');
INSERT INTO #FirstNames(i, Name) VALUES (3,N'Can');
INSERT INTO #FirstNames(i, Name) VALUES (4,N'Zeynep');
INSERT INTO #FirstNames(i, Name) VALUES (5,N'Emre');
INSERT INTO #FirstNames(i, Name) VALUES (6,N'Defne');
INSERT INTO #FirstNames(i, Name) VALUES (7,N'Burak');
INSERT INTO #FirstNames(i, Name) VALUES (8,N'Selin');
INSERT INTO #FirstNames(i, Name) VALUES (9,N'Onur');
INSERT INTO #FirstNames(i, Name) VALUES (10,N'İrem');
INSERT INTO #FirstNames(i, Name) VALUES (11,N'Kerem');
INSERT INTO #FirstNames(i, Name) VALUES (12,N'Deniz');
INSERT INTO #FirstNames(i, Name) VALUES (13,N'Cem');
INSERT INTO #FirstNames(i, Name) VALUES (14,N'Melis');
INSERT INTO #FirstNames(i, Name) VALUES (15,N'Tolga');
INSERT INTO #FirstNames(i, Name) VALUES (16,N'Ece');
INSERT INTO #FirstNames(i, Name) VALUES (17,N'Baran');
INSERT INTO #FirstNames(i, Name) VALUES (18,N'Sude');
INSERT INTO #FirstNames(i, Name) VALUES (19,N'Kaan');
INSERT INTO #FirstNames(i, Name) VALUES (20,N'Naz');
INSERT INTO #FirstNames(i, Name) VALUES (21,N'Arda');
INSERT INTO #FirstNames(i, Name) VALUES (22,N'Lara');
INSERT INTO #FirstNames(i, Name) VALUES (23,N'Yiğit');
INSERT INTO #FirstNames(i, Name) VALUES (24,N'Pınar');
INSERT INTO #FirstNames(i, Name) VALUES (25,N'Oğuz');
INSERT INTO #FirstNames(i, Name) VALUES (26,N'Gül');
INSERT INTO #FirstNames(i, Name) VALUES (27,N'Hakan');
INSERT INTO #FirstNames(i, Name) VALUES (28,N'Berna');
INSERT INTO #FirstNames(i, Name) VALUES (29,N'Serkan');
INSERT INTO #FirstNames(i, Name) VALUES (30,N'Aslı');
INSERT INTO #FirstNames(i, Name) VALUES (31,N'Mert');
INSERT INTO #FirstNames(i, Name) VALUES (32,N'Ceren');
INSERT INTO #FirstNames(i, Name) VALUES (33,N'Umut');
INSERT INTO #FirstNames(i, Name) VALUES (34,N'Dilan');
INSERT INTO #FirstNames(i, Name) VALUES (35,N'Furkan');
INSERT INTO #FirstNames(i, Name) VALUES (36,N'İpek');
INSERT INTO #FirstNames(i, Name) VALUES (37,N'Volkan');
INSERT INTO #FirstNames(i, Name) VALUES (38,N'Buse');
INSERT INTO #FirstNames(i, Name) VALUES (39,N'Alper');

IF OBJECT_ID(N'tempdb..#LastNames') IS NOT NULL DROP TABLE #LastNames;
CREATE TABLE #LastNames (i INT NOT NULL PRIMARY KEY, Name NVARCHAR(50) NOT NULL);

INSERT INTO #LastNames(i, Name) VALUES (0,N'Yılmaz');
INSERT INTO #LastNames(i, Name) VALUES (1,N'Kaya');
INSERT INTO #LastNames(i, Name) VALUES (2,N'Demir');
INSERT INTO #LastNames(i, Name) VALUES (3,N'Şahin');
INSERT INTO #LastNames(i, Name) VALUES (4,N'Çelik');
INSERT INTO #LastNames(i, Name) VALUES (5,N'Yıldız');
INSERT INTO #LastNames(i, Name) VALUES (6,N'Yıldırım');
INSERT INTO #LastNames(i, Name) VALUES (7,N'Öztürk');
INSERT INTO #LastNames(i, Name) VALUES (8,N'Aydın');
INSERT INTO #LastNames(i, Name) VALUES (9,N'Özdemir');
INSERT INTO #LastNames(i, Name) VALUES (10,N'Arslan');
INSERT INTO #LastNames(i, Name) VALUES (11,N'Doğan');
INSERT INTO #LastNames(i, Name) VALUES (12,N'Kılıç');
INSERT INTO #LastNames(i, Name) VALUES (13,N'Aslan');
INSERT INTO #LastNames(i, Name) VALUES (14,N'Çetin');
INSERT INTO #LastNames(i, Name) VALUES (15,N'Kara');
INSERT INTO #LastNames(i, Name) VALUES (16,N'Koç');
INSERT INTO #LastNames(i, Name) VALUES (17,N'Kurt');
INSERT INTO #LastNames(i, Name) VALUES (18,N'Özkan');
INSERT INTO #LastNames(i, Name) VALUES (19,N'Şimşek');
INSERT INTO #LastNames(i, Name) VALUES (20,N'Erdoğan');
INSERT INTO #LastNames(i, Name) VALUES (21,N'Acar');
INSERT INTO #LastNames(i, Name) VALUES (22,N'Polat');
INSERT INTO #LastNames(i, Name) VALUES (23,N'Korkmaz');
INSERT INTO #LastNames(i, Name) VALUES (24,N'Çakır');
INSERT INTO #LastNames(i, Name) VALUES (25,N'Güneş');
INSERT INTO #LastNames(i, Name) VALUES (26,N'Bulut');
INSERT INTO #LastNames(i, Name) VALUES (27,N'Aksoy');
INSERT INTO #LastNames(i, Name) VALUES (28,N'Bozkurt');
INSERT INTO #LastNames(i, Name) VALUES (29,N'Duman');
INSERT INTO #LastNames(i, Name) VALUES (30,N'Ateş');
INSERT INTO #LastNames(i, Name) VALUES (31,N'Taş');
INSERT INTO #LastNames(i, Name) VALUES (32,N'Akın');
INSERT INTO #LastNames(i, Name) VALUES (33,N'Soylu');
INSERT INTO #LastNames(i, Name) VALUES (34,N'Başaran');
INSERT INTO #LastNames(i, Name) VALUES (35,N'Ergin');
INSERT INTO #LastNames(i, Name) VALUES (36,N'Uçar');
INSERT INTO #LastNames(i, Name) VALUES (37,N'Sezer');
INSERT INTO #LastNames(i, Name) VALUES (38,N'Bilgin');
INSERT INTO #LastNames(i, Name) VALUES (39,N'Karaca');

IF OBJECT_ID(N'tempdb..#BrandLookup') IS NOT NULL DROP TABLE #BrandLookup;
CREATE TABLE #BrandLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Description NVARCHAR(400) NOT NULL, MetaKeywords NVARCHAR(200) NOT NULL);

INSERT INTO #BrandLookup VALUES (1,N'Nordline',N'İskandinav tarzı mobilya ve ev dekorasyonu.',N'mobilya,ev,dekorasyon');
INSERT INTO #BrandLookup VALUES (2,N'Atlas Tekstil',N'Günlük giyim ve temel tekstil ürünleri.',N'tekstil,giyim,pamuk');
INSERT INTO #BrandLookup VALUES (3,N'Sportiva',N'Koşu, fitness ve outdoor ekipmanları.',N'spor,fitness,outdoor');
INSERT INTO #BrandLookup VALUES (4,N'Lumina Kitchen',N'Mutfak gereçleri ve küçük ev aletleri.',N'mutfak,ev aleti');
INSERT INTO #BrandLookup VALUES (5,N'Beauté Lab',N'Cilt bakımı ve kişisel bakım ürünleri.',N'kozmetik,cilt bakımı');
INSERT INTO #BrandLookup VALUES (6,N'TeknoPlus',N'Elektronik aksesuar ve bilgisayar çevre birimleri.',N'elektronik,aksesuar');
INSERT INTO #BrandLookup VALUES (7,N'Casa Bella',N'Ev tekstili, yatak ve banyo ürünleri.',N'ev tekstili,yatak');
INSERT INTO #BrandLookup VALUES (8,N'MiniNest',N'Bebek ve çocuk ürünleri.',N'bebek,çocuk');
INSERT INTO #BrandLookup VALUES (9,N'Meridian Outdoor',N'Kamp, yürüyüş ve doğa sporları.',N'kamp,outdoor');
INSERT INTO #BrandLookup VALUES (10,N'Deri Atölyesi',N'Deri çanta, kemer ve cüzdan koleksiyonu.',N'deri,aksesuar');
INSERT INTO #BrandLookup VALUES (11,N'AquaPure',N'Su arıtma ve sağlıklı yaşam ürünleri.',N'su,sağlık');
INSERT INTO #BrandLookup VALUES (12,N'Kitap Köşesi',N'Kitap, kırtasiye ve hobi ürünleri.',N'kitap,kırtasiye');
INSERT INTO #BrandLookup VALUES (13,N'UrbanWear',N'Sokak stili ve günlük moda.',N'moda,streetwear');
INSERT INTO #BrandLookup VALUES (14,N'ChefPro',N'Profesyonel mutfak ekipmanları.',N'mutfak,profesyonel');
INSERT INTO #BrandLookup VALUES (15,N'GreenLeaf',N'Organik gıda ve doğal ürünler.',N'organik,doğal');
INSERT INTO #BrandLookup VALUES (16,N'SoundWave',N'Kulaklık, hoparlör ve ses sistemleri.',N'ses,kulaklık');
INSERT INTO #BrandLookup VALUES (17,N'FitLife',N'Spor giyim ve aktif yaşam.',N'spor giyim,aktif');
INSERT INTO #BrandLookup VALUES (18,N'HomeGlow',N'Aydınlatma ve ev dekorasyonu.',N'aydınlatma,dekor');
INSERT INTO #BrandLookup VALUES (19,N'PetFriend',N'Evcil hayvan bakımı ve aksesuarları.',N'pet,hayvan');
INSERT INTO #BrandLookup VALUES (20,N'Voyage Pack',N'Valiz, sırt çantası ve seyahat aksesuarları.',N'seyahat,valiz');

IF OBJECT_ID(N'tempdb..#CategoryLookup') IS NOT NULL DROP TABLE #CategoryLookup;
CREATE TABLE #CategoryLookup (
    rn INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(400) NOT NULL,
    ParentRn INT NULL,          -- NULL = root; else 1-based root rn
    Discount FLOAT NULL
);

INSERT INTO #CategoryLookup VALUES (1,N'Elektronik',N'Telefon, bilgisayar ve elektronik aksesuarlar.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (2,N'Moda & Giyim',N'Kadın, erkek ve unisex giyim.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (3,N'Ev & Yaşam',N'Mobilya, dekorasyon ve ev tekstili.',NULL,10.0);
INSERT INTO #CategoryLookup VALUES (4,N'Spor & Outdoor',N'Spor giyim, ekipman ve kamp ürünleri.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (5,N'Kozmetik & Bakım',N'Cilt bakımı, makyaj ve kişisel bakım.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (6,N'Bebek & Çocuk',N'Bebek bakımı ve çocuk ürünleri.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (7,N'Kitap & Hobi',N'Kitaplar, kırtasiye ve hobi malzemeleri.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (8,N'Mutfak',N'Mutfak gereçleri ve küçük ev aletleri.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (9,N'Kulaklık & Ses',N'Kablosuz kulaklık ve hoparlörler.',1,NULL);
INSERT INTO #CategoryLookup VALUES (10,N'Telefon Aksesuarları',N'Kılıf, şarj ve ekran koruyucular.',1,NULL);
INSERT INTO #CategoryLookup VALUES (11,N'Kadın Giyim',N'Elbise, bluz ve dış giyim.',2,NULL);
INSERT INTO #CategoryLookup VALUES (12,N'Erkek Giyim',N'Gömlek, pantolon ve mont.',2,NULL);
INSERT INTO #CategoryLookup VALUES (13,N'Ayakkabı',N'Spor ve günlük ayakkabılar.',2,NULL);
INSERT INTO #CategoryLookup VALUES (14,N'Oturma Grubu',N'Koltuk, berjer ve sehpa.',3,NULL);
INSERT INTO #CategoryLookup VALUES (15,N'Yatak Odası',N'Nevresim, yastık ve yorgan.',3,NULL);
INSERT INTO #CategoryLookup VALUES (16,N'Aydınlatma',N'Masa lambası ve avize.',3,NULL);
INSERT INTO #CategoryLookup VALUES (17,N'Koşu & Fitness',N'Koşu ayakkabısı ve fitness ekipmanı.',4,NULL);
INSERT INTO #CategoryLookup VALUES (18,N'Kamp & Doğa',N'Çadır, mat ve outdoor çanta.',4,NULL);
INSERT INTO #CategoryLookup VALUES (19,N'Cilt Bakımı',N'Temizleyici, serum ve nemlendirici.',5,NULL);
INSERT INTO #CategoryLookup VALUES (20,N'Saç Bakımı',N'Şampuan, saç kremi ve serum.',5,NULL);
INSERT INTO #CategoryLookup VALUES (21,N'Bebek Bakım',N'Bebek bezi ve bakım setleri.',6,NULL);
INSERT INTO #CategoryLookup VALUES (22,N'Oyuncak',N'Eğitici ve eğlenceli oyuncaklar.',6,NULL);
INSERT INTO #CategoryLookup VALUES (23,N'Roman & Edebiyat',N'Çağdaş ve klasik romanlar.',7,NULL);
INSERT INTO #CategoryLookup VALUES (24,N'Kırtasiye',N'Defter, kalem ve ofis malzemeleri.',7,NULL);
INSERT INTO #CategoryLookup VALUES (25,N'Pişirme Gereçleri',N'Tencere, tava ve mutfak setleri.',8,NULL);

IF OBJECT_ID(N'tempdb..#ProductLookup') IS NOT NULL DROP TABLE #ProductLookup;
CREATE TABLE #ProductLookup (
    rn INT NOT NULL PRIMARY KEY,
    NamePattern NVARCHAR(200) NOT NULL,
    BrandRn INT NOT NULL,          -- 1-based preferred brand; 0 = rotate
    CategoryRn INT NOT NULL,       -- 1-based category
    PriceBase DECIMAL(18,2) NOT NULL,
    PriceSpread DECIMAL(18,2) NOT NULL,
    Colors NVARCHAR(100) NULL,
    Sizes NVARCHAR(100) NULL,
    ShortDescription NVARCHAR(400) NOT NULL,
    DescriptionHtml NVARCHAR(MAX) NOT NULL
);

INSERT INTO #ProductLookup VALUES (1,N'{b} Wireless Bluetooth Kulaklık Pro',16,9,1299,800,N'Siyah,Beyaz,Lacivert',NULL,N'Aktif gürültü engelleme özellikli kablosuz kulaklık.',N'<p>Uzun pil ömrü, hızlı şarj ve konforlu kullanım için tasarlandı. Günlük commute ve ofis kullanımı için idealdir.</p>');
INSERT INTO #ProductLookup VALUES (2,N'{b} USB-C Hızlı Şarj Adaptörü 65W',6,10,449,200,N'Beyaz,Siyah',NULL,N'GaN teknolojili kompakt şarj adaptörü.',N'<p>Telefon, tablet ve dizüstü bilgisayarlarınızı tek adaptörle şarj edin. Aşırı ısınma koruması dahildir.</p>');
INSERT INTO #ProductLookup VALUES (3,N'{b} Silikon Telefon Kılıfı',6,10,149,80,N'Şeffaf,Siyah,Pembe,Mavi',NULL,N'Darbelere dayanıklı ince silikon kılıf.',N'<p>Hassas kenarları korur, kablosuz şarj ile uyumludur.</p>');
INSERT INTO #ProductLookup VALUES (4,N'{b} Kadın Pamuklu Basic Tişört',2,11,279,120,N'Beyaz,Siyah,Gri,Bej',N'XS,S,M,L,XL',N'Nefes alan pamuklu günlük tişört.',N'<p>%100 pamuk, yumuşak dokulu. Makinede yıkanabilir.</p>');
INSERT INTO #ProductLookup VALUES (5,N'{b} Erkek Slim Fit Chino Pantolon',13,12,599,250,N'Bej,Lacivert,Haki,Siyah',N'28,30,32,34,36',N'Ofis ve günlük kullanım için slim fit chino.',N'<p>Esnek kumaş, ütü gerektirmeyen form. Mevsimlik koleksiyon.</p>');
INSERT INTO #ProductLookup VALUES (6,N'{b} Unisex Koşu Ayakkabısı AirFlex',17,13,1899,600,N'Siyah,Gri,Mavi,Turuncu',N'36,37,38,39,40,41,42,43,44',N'Hafif tabanlı koşu ayakkabısı.',N'<p>Nefes alan mesh üst, darbe emici taban. Yol koşusu için optimize edildi.</p>');
INSERT INTO #ProductLookup VALUES (7,N'{b} Kadın Trençkot',13,11,1499,500,N'Bej,Siyah,Haki',N'S,M,L,XL',N'Su itici kumaşlı klasik trençkot.',N'<p>Mevsim geçişleri için ideal. Astarlı, bel kemerli kesim.</p>');
INSERT INTO #ProductLookup VALUES (8,N'{b} Erkek Oxford Gömlek',2,12,449,150,N'Beyaz,Açık Mavi,Pembe',N'S,M,L,XL,XXL',N'Klasik oxford gömlek.',N'<p>İş ve günlük kombinler için. Kolay ütülenir pamuk karışımı.</p>');
INSERT INTO #ProductLookup VALUES (9,N'{b} Köşe Koltuk Takımı 3+1',1,14,24999,8000,N'Antrasit,Krem,Yeşil',NULL,N'Geniş L köşe koltuk takımı.',N'<p>Yüksek yoğunluklu sünger, çıkarılabilir kılıflar. Oturma odanıza ferah bir görünüm katar.</p>');
INSERT INTO #ProductLookup VALUES (10,N'{b} Meşe Sehpa 90cm',1,14,3299,1000,N'Doğal Meşe,Ceviz',NULL,N'Masif görünümlü orta sehpa.',N'<p>Dayanıklı yüzey, kolay temizlenir. Minimal İskandinav çizgiler.</p>');
INSERT INTO #ProductLookup VALUES (11,N'{b} Pamuk Saten Nevresim Takımı',7,15,899,400,N'Beyaz,Gri,Pudra',N'Tek Kişilik,Çift Kişilik,King',N'200 tc pamuk saten nevresim.',N'<p>Yumuşak doku, renk solmaz. Yastık kılıfı dahildir.</p>');
INSERT INTO #ProductLookup VALUES (12,N'{b} LED Masa Lambası Dimmerlı',18,16,649,250,N'Siyah,Beyaz,Pirinç',NULL,N'Dokunmatik dimmerlı LED masa lambası.',N'<p>Üç renk sıcaklığı, göz yormayan ışık. USB şarj portu.</p>');
INSERT INTO #ProductLookup VALUES (13,N'{b} Yoga Matı 6mm',17,17,349,150,N'Mor,Mavi,Pembe,Siyah',NULL,N'Kaymaz yüzeyli yoga ve pilates matı.',N'<p>Taşıma askısı dahil. Lateks içermez, kolay silinir.</p>');
INSERT INTO #ProductLookup VALUES (14,N'{b} Dambıl Seti 2x5kg',3,17,429,200,N'Siyah',NULL,N'Neopren kaplı dambıl çifti.',N'<p>Ev antrenmanları için kaymaz tutuş. Zemin koruyucu uçlar.</p>');
INSERT INTO #ProductLookup VALUES (15,N'{b} 2 Kişilik Kamp Çadırı',9,18,2199,700,N'Yeşil,Turuncu',NULL,N'Hızlı kurulumlu su geçirmez çadır.',N'<p>2000 mm su kolonlu kumaş, sivrisinek tülü. Taşıma çantası dahil.</p>');
INSERT INTO #ProductLookup VALUES (16,N'{b} Trekking Sırt Çantası 40L',9,18,1599,500,N'Antrasit,Haki',NULL,N'Bel ve göğüs kemerli trekking çantası.',N'<p>Yağmurluk kılıfı, hidrasyon uyumlu. Sırt paneli nefes alır.</p>');
INSERT INTO #ProductLookup VALUES (17,N'{b} Vitamin C Aydınlatıcı Serum 30ml',5,19,389,150,NULL,NULL,N'Leke karşıtı C vitamini serumu.',N'<p>Sabah rutini için. SPF ile birlikte kullanın. Dermatolojik olarak test edilmiştir.</p>');
INSERT INTO #ProductLookup VALUES (18,N'{b} Nemlendirici Yüz Kremi 50ml',5,19,299,120,NULL,NULL,N'24 saat nem desteği sağlayan yüz kremi.',N'<p>Yağlı ve karma ciltler için hafif formül. Paraben içermez.</p>');
INSERT INTO #ProductLookup VALUES (19,N'{b} Onarıcı Şampuan 400ml',5,20,189,80,NULL,NULL,N'Yıpranmış saçlar için onarıcı şampuan.',N'<p>Keratin ve argan yağı kompleksi. Günlük kullanıma uygundur.</p>');
INSERT INTO #ProductLookup VALUES (20,N'{b} Bebek Bakım Seti 5''li',8,21,449,150,NULL,NULL,N'Hassas ciltler için bebek bakım seti.',N'<p>Şampuan, losyon, yağ, krem ve ıslak mendil. Hipoalerjenik.</p>');
INSERT INTO #ProductLookup VALUES (21,N'{b} Eğitici Ahşap Blok Seti',8,22,329,100,N'Renkli',NULL,N'48 parçalı ahşap blok seti.',N'<p>Su bazlı boya, keskin kenar yok. 3+ yaş için uygundur.</p>');
INSERT INTO #ProductLookup VALUES (22,N'{b} Çağdaş Roman - Seçki #{n}',12,23,149,80,NULL,NULL,N'Özenle seçilmiş çağdaş edebiyat.',N'<p>Sert kapak, yerli baskı. Okur yorumlarıyla öne çıkan başlık.</p>');
INSERT INTO #ProductLookup VALUES (23,N'{b} Sert Kapaklı Defter A5',12,24,89,40,N'Kraft,Siyah,Lacivert',NULL,N'Noktalı sayfa A5 defter.',N'<p>120 sayfa, 90 gsm. Lastik bant ve yer imi şeridi.</p>');
INSERT INTO #ProductLookup VALUES (24,N'{b} Granit Tava 28cm',14,25,549,200,N'Siyah',NULL,N'Yapışmaz granit kaplama tava.',N'<p>Indüksiyon uyumlu. Fırına dayanıklı sap. PFOA içermez.</p>');
INSERT INTO #ProductLookup VALUES (25,N'{b} Çelik Tencere Seti 6 Parça',4,25,2499,800,N'Çelik',NULL,N'Paslanmaz çelik tencere seti.',N'<p>Kapaklı 3 tencere. Bulaşık makinesinde yıkanabilir.</p>');
INSERT INTO #ProductLookup VALUES (26,N'{b} Cam Su Şişesi 750ml',11,8,249,80,N'Şeffaf,Füme,Mavi',NULL,N'BPA içermeyen cam su şişesi.',N'<p>Silikon kılıflı, sızdırmaz kapak. Ofis ve spor için.</p>');
INSERT INTO #ProductLookup VALUES (27,N'{b} Organik Zeytinyağı 1L',15,8,329,100,NULL,NULL,N'Soğuk sıkım organik zeytinyağı.',N'<p>Tek hasat, koyu cam şişe. Tadım notları etikette.</p>');
INSERT INTO #ProductLookup VALUES (28,N'{b} Kabin Boyu Valiz 55cm',20,2,1899,600,N'Siyah,Lacivert,Bordo',NULL,N'Hafif kabin boyu sert valiz.',N'<p>360° tekerlek, TSA kilit. İç organizer bölmeler.</p>');
INSERT INTO #ProductLookup VALUES (29,N'{b} Deri Omuz Çantası',10,2,1299,400,N'Taba,Siyah,Bordo',NULL,N'El yapımı görünümlü deri omuz çantası.',N'<p>Ayarlanabilir askı, fermuarlı iç cep. Günlük kullanım.</p>');
INSERT INTO #ProductLookup VALUES (30,N'{b} Evcil Hayvan Mama Kabı Seti',19,3,199,80,N'Gri,Pembe,Mavi',NULL,N'Çelik mama ve su kabı seti.',N'<p>Kaymaz taban, bulaşık makinesi uyumlu.</p>');
INSERT INTO #ProductLookup VALUES (31,N'{b} Akıllı LED Ampul 9W',18,16,179,60,N'Beyaz',NULL,N'Uygulama kontrollü renk değiştiren ampul.',N'<p>Sesli asistan uyumlu. Zamanlayıcı ve senaryo desteği.</p>');
INSERT INTO #ProductLookup VALUES (32,N'{b} Termos Mug 350ml',4,8,279,100,N'Siyah,Beyaz,Kırmızı',NULL,N'Paslanmaz çelik vakumlu termos mug.',N'<p>6 saat sıcak / 12 saat soğuk tutma. Araç tutucuya uygun.</p>');
INSERT INTO #ProductLookup VALUES (33,N'{b} Fitness Taytı Yüksek Bel',17,17,449,150,N'Siyah,Lacivert,Bordo',N'XS,S,M,L',N'Toparlayıcı yüksek bel spor taytı.',N'<p>Ter emici kumaş, cep detayı. Antrenman ve günlük kullanım.</p>');
INSERT INTO #ProductLookup VALUES (34,N'{b} Erkek Polar Mont',3,12,899,300,N'Antrasit,Lacivert,Haki',N'S,M,L,XL,XXL',N'Hafif polar fermuarlı mont.',N'<p>Soğuk hava katmanı olarak veya tek başına giyilebilir.</p>');
INSERT INTO #ProductLookup VALUES (35,N'{b} Bambu Kesme Tahtası Seti',14,25,349,120,N'Doğal',NULL,N'3 boy bambu kesme tahtası.',N'<p>Antibakteriyel doğal yüzey. Asma delikli.</p>');
INSERT INTO #ProductLookup VALUES (36,N'{b} Güneş Kremi SPF50 50ml',5,19,259,80,NULL,NULL,N'Yüz için hafif dokulu güneş koruyucu.',N'<p>Beyaz iz bırakmaz. Makyaj altı kullanıma uygun.</p>');
INSERT INTO #ProductLookup VALUES (37,N'{b} Bebek Body 3''lü Paket',8,21,249,80,N'Beyaz,Gri,Sarı',N'0-3 Ay,3-6 Ay,6-9 Ay,9-12 Ay',N'Organik pamuk bebek body seti.',N'<p>Çıtçıtlı, etiket içe basılmış. Hassas ciltler için.</p>');
INSERT INTO #ProductLookup VALUES (38,N'{b} Bluetooth Hoparlör Mini',16,9,799,300,N'Siyah,Mavi,Kırmızı',NULL,N'Taşınabilir suya dayanıklı hoparlör.',N'<p>12 saat pil, IPX7. Stereo eşleştirme destekler.</p>');
INSERT INTO #ProductLookup VALUES (39,N'{b} Laptop Standı Alüminyum',6,1,549,200,N'Gümüş,Uzay Grisi',NULL,N'Ayarlanabilir alüminyum laptop standı.',N'<p>Ergonomik açı, kablo geçişi. 10–16 inç uyumlu.</p>');
INSERT INTO #ProductLookup VALUES (40,N'{b} Yastık 2''li Memory Foam',7,15,699,250,N'Beyaz',N'Standart',N'Visco bellek köpük yastık çifti.',N'<p>Boyun desteği, çıkarılabilir kılıf. Anti-alerjik.</p>');

IF OBJECT_ID(N'tempdb..#TagCatLookup') IS NOT NULL DROP TABLE #TagCatLookup;
CREATE TABLE #TagCatLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL);

INSERT INTO #TagCatLookup VALUES (1,N'Kullanım Amacı');
INSERT INTO #TagCatLookup VALUES (2,N'Malzeme');
INSERT INTO #TagCatLookup VALUES (3,N'Sezon');
INSERT INTO #TagCatLookup VALUES (4,N'Hedef Kitle');
INSERT INTO #TagCatLookup VALUES (5,N'Özellik');
INSERT INTO #TagCatLookup VALUES (6,N'Koleksiyon');

IF OBJECT_ID(N'tempdb..#TagLookup') IS NOT NULL DROP TABLE #TagLookup;
CREATE TABLE #TagLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL);

INSERT INTO #TagLookup VALUES (1,N'Günlük kullanım');
INSERT INTO #TagLookup VALUES (2,N'Ofis');
INSERT INTO #TagLookup VALUES (3,N'Spor');
INSERT INTO #TagLookup VALUES (4,N'Seyahat');
INSERT INTO #TagLookup VALUES (5,N'Hediye fikri');
INSERT INTO #TagLookup VALUES (6,N'Pamuk');
INSERT INTO #TagLookup VALUES (7,N'Deri');
INSERT INTO #TagLookup VALUES (8,N'Polyester');
INSERT INTO #TagLookup VALUES (9,N'Organik');
INSERT INTO #TagLookup VALUES (10,N'Metal');
INSERT INTO #TagLookup VALUES (11,N'Yaz');
INSERT INTO #TagLookup VALUES (12,N'Kış');
INSERT INTO #TagLookup VALUES (13,N'İlkbahar');
INSERT INTO #TagLookup VALUES (14,N'Sonbahar');
INSERT INTO #TagLookup VALUES (15,N'Mevsimlik');
INSERT INTO #TagLookup VALUES (16,N'Kadın');
INSERT INTO #TagLookup VALUES (17,N'Erkek');
INSERT INTO #TagLookup VALUES (18,N'Unisex');
INSERT INTO #TagLookup VALUES (19,N'Çocuk');
INSERT INTO #TagLookup VALUES (20,N'Bebek');
INSERT INTO #TagLookup VALUES (21,N'Su geçirmez');
INSERT INTO #TagLookup VALUES (22,N'Nefes alan');
INSERT INTO #TagLookup VALUES (23,N'Hızlı kargo');
INSERT INTO #TagLookup VALUES (24,N'İndirimde');
INSERT INTO #TagLookup VALUES (25,N'Yeni sezon');
INSERT INTO #TagLookup VALUES (26,N'Minimal');
INSERT INTO #TagLookup VALUES (27,N'Klasik');
INSERT INTO #TagLookup VALUES (28,N'Modern');
INSERT INTO #TagLookup VALUES (29,N'Vintage');
INSERT INTO #TagLookup VALUES (30,N'Scandinavian');
INSERT INTO #TagLookup VALUES (31,N'Kampanya');
INSERT INTO #TagLookup VALUES (32,N'Çok satan');
INSERT INTO #TagLookup VALUES (33,N'Editörün seçimi');
INSERT INTO #TagLookup VALUES (34,N'Sınırlı stok');
INSERT INTO #TagLookup VALUES (35,N'Yerli üretim');
INSERT INTO #TagLookup VALUES (36,N'Eco-friendly');
INSERT INTO #TagLookup VALUES (37,N'BPA free');
INSERT INTO #TagLookup VALUES (38,N'Makinede yıkanır');
INSERT INTO #TagLookup VALUES (39,N'İndüksiyon uyumlu');
INSERT INTO #TagLookup VALUES (40,N'TSA kilit');

IF OBJECT_ID(N'tempdb..#StoryCatLookup') IS NOT NULL DROP TABLE #StoryCatLookup;
CREATE TABLE #StoryCatLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Description NVARCHAR(400) NOT NULL, PageTheme NVARCHAR(10) NOT NULL);

INSERT INTO #StoryCatLookup VALUES (1,N'Stil Rehberi',N'Moda ve stil ipuçları.',N'T1');
INSERT INTO #StoryCatLookup VALUES (2,N'Ev Dekorasyonu',N'Yaşam alanları için ilham.',N'T2');
INSERT INTO #StoryCatLookup VALUES (3,N'Sağlıklı Yaşam',N'Beslenme ve wellness yazıları.',N'T3');
INSERT INTO #StoryCatLookup VALUES (4,N'Teknoloji',N'Gadget incelemeleri ve ipuçları.',N'T4');
INSERT INTO #StoryCatLookup VALUES (5,N'Seyahat',N'Rota önerileri ve paketleme rehberleri.',N'T5');
INSERT INTO #StoryCatLookup VALUES (6,N'Ebeveynlik',N'Bebek ve çocuk bakımı.',N'T6');

IF OBJECT_ID(N'tempdb..#StoryLookup') IS NOT NULL DROP TABLE #StoryLookup;
CREATE TABLE #StoryLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(200) NOT NULL, ShortDescription NVARCHAR(400) NOT NULL, AuthorName NVARCHAR(100) NOT NULL, BodyHtml NVARCHAR(MAX) NOT NULL);

INSERT INTO #StoryLookup VALUES (1,N'2024 Sonbahar Kombin Önerileri',N'Katmanlı sonbahar stilleri.',N'Selin Arslan',N'<p>Mevsim geçişinde trençkot, polar ve chino pantolonları nasıl bir araya getirirsiniz? Editörlerimizin favori kombinleri.</p>');
INSERT INTO #StoryLookup VALUES (2,N'Küçük Salonlar İçin Mobilya İpuçları',N'Dar alanlarda ferahlık.',N'Can Yılmaz',N'<p>Köşe koltuk yerine doğru sehpa ve aydınlatma ile salonu büyütün. Nordline koleksiyonundan örnekler.</p>');
INSERT INTO #StoryLookup VALUES (3,N'Koşu Ayakkabısı Nasıl Seçilir?',N'Pronasyon, taban ve fit.',N'Emre Demir',N'<p>Haftalık kilometrenize ve ayak tipinize göre doğru koşu ayakkabısını seçmenin kısa rehberi.</p>');
INSERT INTO #StoryLookup VALUES (4,N'Cilt Bakım Rutini: 5 Adım',N'Temizlikten nemlendirmeye.',N'Elif Kaya',N'<p>Sabah ve akşam için sade ama etkili bir rutin. Serum ve güneş kreminin sırası neden önemli?</p>');
INSERT INTO #StoryLookup VALUES (5,N'Kamp Tatiline Çıkmadan Önce',N'Kontrol listesi.',N'Burak Şahin',N'<p>Çadır, mat, sırt çantası ve mutfak seti: ilk kampınız için unutmamanız gerekenler.</p>');
INSERT INTO #StoryLookup VALUES (6,N'Mutfakta Granit Tava Kullanımı',N'Bakım ve pişirme ipuçları.',N'Ayşe Çelik',N'<p>Yapışmaz yüzeyin ömrünü uzatmak için metal spatula kullanmayın. Doğru ısı ayarları.</p>');
INSERT INTO #StoryLookup VALUES (7,N'Bebek Odası Hazırlık Listesi',N'İlk 3 ay için temel ürünler.',N'Zeynep Öztürk',N'<p>Body setlerinden bakım ürünlerine, gerçekçi bir alışveriş listesi.</p>');
INSERT INTO #StoryLookup VALUES (8,N'Bluetooth Kulaklık Alırken',N'ANC, pil ve uyum.',N'Kerem Aydın',N'<p>Aktif gürültü engelleme gerçekten işe yarıyor mu? Ofis ve yolculuk senaryoları.</p>');
INSERT INTO #StoryLookup VALUES (9,N'Organik Ürün Etiketlerini Okumak',N'Sertifikalar ne anlama geliyor?',N'Defne Kara',N'<p>Organik, soğuk sıkım ve katkı maddesi ifadelerini doğru yorumlayın.</p>');
INSERT INTO #StoryLookup VALUES (10,N'Ev Ofisi Aydınlatması',N'Göz yorgunluğunu azaltın.',N'Onur Yıldız',N'<p>Masa lambası rengi, parlaklık ve ekran konumu hakkında pratik öneriler.</p>');
INSERT INTO #StoryLookup VALUES (11,N'Valiz Seçiminde Dikkat Edilecekler',N'Kabin kuralları ve tekerlek.',N'Melis Doğan',N'<p>Havayolu kabin ölçüleri, TSA kilit ve ağırlık dengesi.</p>');
INSERT INTO #StoryLookup VALUES (12,N'Spor Taytı Alırken Fit Kontrolü',N'Yüksek bel ve kumaş.',N'İrem Kılıç',N'<p>Antrenmanda kaymayan, ter emici kumaşlı tayt nasıl anlaşılır?</p>');
INSERT INTO #StoryLookup VALUES (13,N'Kitaplarla Küçük Bir Köşe',N'Okuma alanı kurmak.',N'Cem Polat',N'<p>Raf, aydınlatma ve rahat bir berjer ile evde mini kütüphane.</p>');
INSERT INTO #StoryLookup VALUES (14,N'Kışlık Mont Katmanlama',N'Polar + dış katman.',N'Hakan Kurt',N'<p>Soğuk havalarda nefes alan katmanlarla sıcak kalmanın yolu.</p>');
INSERT INTO #StoryLookup VALUES (15,N'Pet Dostu Ev Düzeni',N'Mama alanı ve güvenlik.',N'Buse Aksoy',N'<p>Kaymaz mama kapları ve kablo düzeni ile evcil dostunuz için güvenli alan.</p>');

IF OBJECT_ID(N'tempdb..#MenuLookup') IS NOT NULL DROP TABLE #MenuLookup;
CREATE TABLE #MenuLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Description NVARCHAR(MAX) NULL, MenuLink NVARCHAR(100) NOT NULL, ExternalLink NVARCHAR(200) NULL, MainPage BIT NOT NULL);

INSERT INTO #MenuLookup VALUES (1,N'Ana Sayfa',N'<p>EImece vitrine hoş geldiniz.</p>',N'home-index',NULL,1);
INSERT INTO #MenuLookup VALUES (2,N'Kurumsal',N'<p>Hakkımızda ve şirket bilgileri.</p>',N'pages-index',NULL,1);
INSERT INTO #MenuLookup VALUES (3,N'Hakkımızda',N'<p>EImece, seçili markaları tek çatı altında sunan online mağazadır.</p>',N'info-aboutus',NULL,0);
INSERT INTO #MenuLookup VALUES (4,N'İletişim',N'<p>Müşteri hizmetleri ve mağaza iletişim bilgileri.</p>',N'pages-index',NULL,0);
INSERT INTO #MenuLookup VALUES (5,N'Kargo & Teslimat',N'<p>Kargo süreleri, ücretsiz kargo limiti ve iade süreci.</p>',N'info-deliveryinfo',NULL,1);
INSERT INTO #MenuLookup VALUES (6,N'Sıkça Sorulan Sorular',N'<p>Sipariş, ödeme ve iade hakkında SSS.</p>',N'pages-index',NULL,1);
INSERT INTO #MenuLookup VALUES (7,N'Kampanyalar',N'<p>Güncel indirimler ve kuponlar.</p>',N'pages-index',NULL,1);
INSERT INTO #MenuLookup VALUES (8,N'Blog',N'<p>Stil, yaşam ve ürün rehberleri.</p>',N'stories-index',NULL,1);
INSERT INTO #MenuLookup VALUES (9,N'Gizlilik Politikası',N'<p>Kişisel verilerin korunması.</p>',N'info-privacypolicy',NULL,0);
INSERT INTO #MenuLookup VALUES (10,N'Mesafeli Satış Sözleşmesi',N'<p>Mesafeli satış ve tüketici hakları.</p>',N'info-termsandconditions',NULL,0);
INSERT INTO #MenuLookup VALUES (11,N'İade & Değişim',N'<p>14 gün içinde iade ve değişim koşulları.</p>',N'pages-index',NULL,0);
INSERT INTO #MenuLookup VALUES (12,N'Mağazalarımız',NULL,N'pages-index',N'https://maps.example.com/eimece',1);

IF OBJECT_ID(N'tempdb..#SlideLookup') IS NOT NULL DROP TABLE #SlideLookup;
CREATE TABLE #SlideLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(150) NOT NULL, Description NVARCHAR(400) NOT NULL, Link NVARCHAR(200) NOT NULL);

INSERT INTO #SlideLookup VALUES (1,N'Sonbahar Koleksiyonu',N'Yeni sezon trençkot ve botlarda %20''ye varan indirim.',N'/c/Moda-Giyim');
INSERT INTO #SlideLookup VALUES (2,N'Mutfakta Yenilikler',N'ChefPro tencere setlerinde ücretsiz kargo.',N'/c/Mutfak');
INSERT INTO #SlideLookup VALUES (3,N'Koşu Sezonu Başladı',N'AirFlex koşu ayakkabılarında peşin fiyatına taksit.',N'/c/Spor-Outdoor');
INSERT INTO #SlideLookup VALUES (4,N'Evini Yenile',N'Köşe koltuk ve sehpa takımlarında kaçırılmayacak fırsatlar.',N'/c/Ev-Yasam');
INSERT INTO #SlideLookup VALUES (5,N'Cilt Bakım Haftası',N'Beauté Lab serum ve kremlerde 2 al 1 öde.',N'/c/Kozmetik-Bakim');
INSERT INTO #SlideLookup VALUES (6,N'Seyahat Hazırlığı',N'Kabin boyu valizlerde ekstra %10.',N'/c/Moda-Giyim');

IF OBJECT_ID(N'tempdb..#FaqLookup') IS NOT NULL DROP TABLE #FaqLookup;
CREATE TABLE #FaqLookup (rn INT NOT NULL PRIMARY KEY, Question NVARCHAR(300) NOT NULL, Answer NVARCHAR(MAX) NOT NULL);

INSERT INTO #FaqLookup VALUES (1,N'Siparişimi nasıl takip ederim?',N'<p>Hesabım > Siparişlerim ekranından kargo takip numaranızı görebilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (2,N'Ücretsiz kargo limiti nedir?',N'<p>500 TL ve üzeri siparişlerde kargo ücretsizdir.</p>');
INSERT INTO #FaqLookup VALUES (3,N'İade süresi kaç gündür?',N'<p>Teslimattan itibaren 14 gün içinde iade talebi oluşturabilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (4,N'Kapıda ödeme var mı?',N'<p>Seçili bölgelerde kapıda kart ile ödeme seçeneği sunulmaktadır.</p>');
INSERT INTO #FaqLookup VALUES (5,N'Fatura nasıl alınır?',N'<p>Sipariş sonrası e-fatura kayıtlı e-posta adresinize gönderilir.</p>');
INSERT INTO #FaqLookup VALUES (6,N'Ürün beden tablosu nerede?',N'<p>Ürün detay sayfasında Beden Tablosu sekmesini inceleyebilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (7,N'Kupon kodu nasıl kullanılır?',N'<p>Sepet sayfasında kupon alanına kodunuzu girip Uygula demeniz yeterlidir.</p>');
INSERT INTO #FaqLookup VALUES (8,N'Stokta yok yazıyor, ne zaman gelir?',N'<p>Ürün sayfasından stok bildirimi bırakabilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (9,N'Hediye paketi yapıyor musunuz?',N'<p>Sepette hediye paketi seçeneğini işaretleyebilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (10,N'Same-day teslimat var mı?',N'<p>İstanbul Anadolu yakasında seçili SKU''larda aynı gün teslimat vardır.</p>');
INSERT INTO #FaqLookup VALUES (11,N'Ürün orijinal mi?',N'<p>Tüm ürünler yetkili distribütör ve marka garantisiyle satılır.</p>');
INSERT INTO #FaqLookup VALUES (12,N'Değişim kargo ücreti kimde?',N'<p>Üretim hatası ve yanlış ürün gönderimlerinde kargo bize aittir.</p>');
INSERT INTO #FaqLookup VALUES (13,N'Taksit seçenekleri neler?',N'<p>Anlaşmalı kartlarda 3–6–9 taksit seçenekleri sunulur.</p>');
INSERT INTO #FaqLookup VALUES (14,N'Üyeliksiz alışveriş yapabilir miyim?',N'<p>Evet, misafir ödeme ile sipariş verebilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (15,N'Şifremi unuttum ne yapmalıyım?',N'<p>Giriş ekranından Şifremi Unuttum ile sıfırlama bağlantısı alabilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (16,N'Mağazanız fiziksel olarak var mı?',N'<p>Showroom adresimiz İletişim sayfasında yer almaktadır.</p>');
INSERT INTO #FaqLookup VALUES (17,N'Toplu / kurumsal sipariş?',N'<p>kurumsal@eimece.test adresinden teklif alabilirsiniz.</p>');
INSERT INTO #FaqLookup VALUES (18,N'Ürün videoları neden açılmıyor?',N'<p>Tarayıcı eklentileri engelliyor olabilir; farklı tarayıcı deneyin.</p>');
INSERT INTO #FaqLookup VALUES (19,N'Hangi kargo firmasıyla çalışıyorsunuz?',N'<p>Yurtiçi Kargo ile anlaşmalıyız.</p>');
INSERT INTO #FaqLookup VALUES (20,N'Siparişimi iptal edebilir miyim?',N'<p>Kargoya verilmeden önce Hesabım üzerinden iptal edebilirsiniz.</p>');

IF OBJECT_ID(N'tempdb..#CouponLookup') IS NOT NULL DROP TABLE #CouponLookup;
CREATE TABLE #CouponLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Code NVARCHAR(40) NOT NULL, DiscountPercentage INT NOT NULL, Discount INT NOT NULL);

INSERT INTO #CouponLookup VALUES (1,N'Hoş Geldin İndirimi',N'EIMC-HOSGELDIN',15,0);
INSERT INTO #CouponLookup VALUES (2,N'Yaz Kampanyası',N'EIMC-YAZ25',25,0);
INSERT INTO #CouponLookup VALUES (3,N'Ücretsiz Kargo',N'EIMC-KARGO',0,50);
INSERT INTO #CouponLookup VALUES (4,N'Sezon Sonu',N'EIMC-SEZON20',20,0);
INSERT INTO #CouponLookup VALUES (5,N'VIP Müşteri',N'EIMC-VIP15',15,0);
INSERT INTO #CouponLookup VALUES (6,N'İlk Alışveriş',N'EIMC-ILK10',10,0);
INSERT INTO #CouponLookup VALUES (7,N'Flash İndirim',N'EIMC-FLASH30',30,0);
INSERT INTO #CouponLookup VALUES (8,N'Bahar Fırsatı',N'EIMC-BAHAR',12,0);
INSERT INTO #CouponLookup VALUES (9,N'Öğrenci İndirimi',N'EIMC-OGRENCI',10,0);
INSERT INTO #CouponLookup VALUES (10,N'Sepette 100 TL',N'EIMC-100TL',0,100);
INSERT INTO #CouponLookup VALUES (11,N'Anne Günü',N'EIMC-ANNE',18,0);
INSERT INTO #CouponLookup VALUES (12,N'Yılbaşı Özel',N'EIMC-YILBASI',22,0);

IF OBJECT_ID(N'tempdb..#ListLookup') IS NOT NULL DROP TABLE #ListLookup;
CREATE TABLE #ListLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, IsService BIT NOT NULL, IsValues BIT NOT NULL);

INSERT INTO #ListLookup VALUES (1,N'Ödeme Yöntemleri',0,1);
INSERT INTO #ListLookup VALUES (2,N'Kargo Firmaları',1,1);
INSERT INTO #ListLookup VALUES (3,N'İade Nedenleri',0,1);
INSERT INTO #ListLookup VALUES (4,N'Beden Rehberi Notları',1,0);
INSERT INTO #ListLookup VALUES (5,N'Mağaza Hizmetleri',1,0);
INSERT INTO #ListLookup VALUES (6,N'Bildirim Kanalları',0,1);
INSERT INTO #ListLookup VALUES (7,N'Ürün Durum Etiketleri',0,1);
INSERT INTO #ListLookup VALUES (8,N'Müşteri Segmentleri',0,1);

IF OBJECT_ID(N'tempdb..#ListItemLookup') IS NOT NULL DROP TABLE #ListItemLookup;
CREATE TABLE #ListItemLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL);

INSERT INTO #ListItemLookup VALUES (1,N'Kredi Kartı');
INSERT INTO #ListItemLookup VALUES (2,N'Havale / EFT');
INSERT INTO #ListItemLookup VALUES (3,N'Kapıda Ödeme');
INSERT INTO #ListItemLookup VALUES (4,N'Yurtiçi Kargo');
INSERT INTO #ListItemLookup VALUES (5,N'Aras Kargo');
INSERT INTO #ListItemLookup VALUES (6,N'MNG Kargo');
INSERT INTO #ListItemLookup VALUES (7,N'Beden uymadı');
INSERT INTO #ListItemLookup VALUES (8,N'Fikir değişikliği');
INSERT INTO #ListItemLookup VALUES (9,N'Hasarlı ürün');
INSERT INTO #ListItemLookup VALUES (10,N'Kalça ölçüsü kritik');
INSERT INTO #ListItemLookup VALUES (11,N'Boyuna göre etek boyu');
INSERT INTO #ListItemLookup VALUES (12,N'Hediye paketi');
INSERT INTO #ListItemLookup VALUES (13,N'Express kargo');
INSERT INTO #ListItemLookup VALUES (14,N'Montaj hizmeti');
INSERT INTO #ListItemLookup VALUES (15,N'E-posta');
INSERT INTO #ListItemLookup VALUES (16,N'SMS');
INSERT INTO #ListItemLookup VALUES (17,N'Push bildirim');
INSERT INTO #ListItemLookup VALUES (18,N'Stokta');
INSERT INTO #ListItemLookup VALUES (19,N'Ön sipariş');
INSERT INTO #ListItemLookup VALUES (20,N'Tükendi');
INSERT INTO #ListItemLookup VALUES (21,N'Yeni üye');
INSERT INTO #ListItemLookup VALUES (22,N'Tekrarlayan');
INSERT INTO #ListItemLookup VALUES (23,N'Kurumsal');

IF OBJECT_ID(N'tempdb..#TemplateLookup') IS NOT NULL DROP TABLE #TemplateLookup;
CREATE TABLE #TemplateLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL);

INSERT INTO #TemplateLookup VALUES (1,N'Giyim Şablonu');
INSERT INTO #TemplateLookup VALUES (2,N'Elektronik Şablonu');
INSERT INTO #TemplateLookup VALUES (3,N'Ev & Mobilya Şablonu');
INSERT INTO #TemplateLookup VALUES (4,N'Kozmetik Şablonu');
INSERT INTO #TemplateLookup VALUES (5,N'Spor Ekipman Şablonu');
INSERT INTO #TemplateLookup VALUES (6,N'Genel Ürün Şablonu');

IF OBJECT_ID(N'tempdb..#ReviewSubject') IS NOT NULL DROP TABLE #ReviewSubject;
CREATE TABLE #ReviewSubject (i INT NOT NULL PRIMARY KEY, Subject NVARCHAR(100) NOT NULL);

INSERT INTO #ReviewSubject VALUES (0,N'Beklentimi karşıladı');
INSERT INTO #ReviewSubject VALUES (1,N'Çok memnun kaldım');
INSERT INTO #ReviewSubject VALUES (2,N'Fiyat/performans iyi');
INSERT INTO #ReviewSubject VALUES (3,N'Kargo hızlıydı');
INSERT INTO #ReviewSubject VALUES (4,N'Ürün kaliteli');
INSERT INTO #ReviewSubject VALUES (5,N'Beden tam oldu');
INSERT INTO #ReviewSubject VALUES (6,N'Tekrar alırım');
INSERT INTO #ReviewSubject VALUES (7,N'Hediye olarak aldım');
INSERT INTO #ReviewSubject VALUES (8,N'Fotoğraftaki gibi');
INSERT INTO #ReviewSubject VALUES (9,N'Kullanışlı ürün');
INSERT INTO #ReviewSubject VALUES (10,N'Tavsiye ederim');
INSERT INTO #ReviewSubject VALUES (11,N'Biraz küçük geldi');

IF OBJECT_ID(N'tempdb..#ReviewBody') IS NOT NULL DROP TABLE #ReviewBody;
CREATE TABLE #ReviewBody (i INT NOT NULL PRIMARY KEY, Body NVARCHAR(500) NOT NULL);

INSERT INTO #ReviewBody VALUES (0,N'Ürün elime sorunsuz ulaştı, paketleme özenliydi. Bir süredir kullanıyorum, memnunum.');
INSERT INTO #ReviewBody VALUES (1,N'Açıklamadaki özelliklerle uyumlu. Günlük kullanım için gayet yeterli.');
INSERT INTO #ReviewBody VALUES (2,N'Kumaş kalitesi güzel, rengi ekranda gördüğüm gibi çıktı.');
INSERT INTO #ReviewBody VALUES (3,N'Kargo 2 günde geldi. Montaj / kullanım kolay, öneririm.');
INSERT INTO #ReviewBody VALUES (4,N'Fiyatına göre beklentimin üzerinde. Tekrar sipariş vereceğim.');
INSERT INTO #ReviewBody VALUES (5,N'İade sürecini denemedim ama ürün beklentimi karşıladı.');
INSERT INTO #ReviewBody VALUES (6,N'Eşim için aldım, çok beğendi. Hediye paketiniz de güzeldi.');
INSERT INTO #ReviewBody VALUES (7,N'Birkaç yıkamadan sonra formunu korudu. Memnun kaldım.');
INSERT INTO #ReviewBody VALUES (8,N'Ses kalitesi net, pil ömrü iddia edildiği gibi.');
INSERT INTO #ReviewBody VALUES (9,N'Küçük eksikler olsa da genel olarak iyi bir alışveriş oldu.');

IF OBJECT_ID(N'tempdb..#StreetLookup') IS NOT NULL DROP TABLE #StreetLookup;
CREATE TABLE #StreetLookup (i INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL);

INSERT INTO #StreetLookup VALUES (0,N'Bağdat Caddesi');
INSERT INTO #StreetLookup VALUES (1,N'Atatürk Bulvarı');
INSERT INTO #StreetLookup VALUES (2,N'İstiklal Caddesi');
INSERT INTO #StreetLookup VALUES (3,N'Cumhuriyet Mahallesi');
INSERT INTO #StreetLookup VALUES (4,N'Göztepe Sokak');
INSERT INTO #StreetLookup VALUES (5,N'Çankaya Caddesi');
INSERT INTO #StreetLookup VALUES (6,N'Alsancak Mahallesi');
INSERT INTO #StreetLookup VALUES (7,N'Nilüfer Caddesi');
INSERT INTO #StreetLookup VALUES (8,N'Lara Bulvarı');
INSERT INTO #StreetLookup VALUES (9,N'Tepebaşı Sokak');

DECLARE @BrandLookupCount INT = (SELECT COUNT(*) FROM #BrandLookup);
DECLARE @CategoryLookupCount INT = (SELECT COUNT(*) FROM #CategoryLookup);
DECLARE @ProductLookupCount INT = (SELECT COUNT(*) FROM #ProductLookup);
DECLARE @TagCatLookupCount INT = (SELECT COUNT(*) FROM #TagCatLookup);
DECLARE @TagLookupCount INT = (SELECT COUNT(*) FROM #TagLookup);
DECLARE @StoryCatLookupCount INT = (SELECT COUNT(*) FROM #StoryCatLookup);
DECLARE @StoryLookupCount INT = (SELECT COUNT(*) FROM #StoryLookup);
DECLARE @MenuLookupCount INT = (SELECT COUNT(*) FROM #MenuLookup);
DECLARE @SlideLookupCount INT = (SELECT COUNT(*) FROM #SlideLookup);
DECLARE @FaqLookupCount INT = (SELECT COUNT(*) FROM #FaqLookup);
DECLARE @CouponLookupCount INT = (SELECT COUNT(*) FROM #CouponLookup);
DECLARE @ListLookupCount INT = (SELECT COUNT(*) FROM #ListLookup);
DECLARE @ListItemLookupCount INT = (SELECT COUNT(*) FROM #ListItemLookup);
DECLARE @TemplateLookupCount INT = (SELECT COUNT(*) FROM #TemplateLookup);
DECLARE @FirstNameCount INT = (SELECT COUNT(*) FROM #FirstNames);
DECLARE @LastNameCount INT = (SELECT COUNT(*) FROM #LastNames);
DECLARE @ReviewSubjectCount INT = (SELECT COUNT(*) FROM #ReviewSubject);
DECLARE @ReviewBodyCount INT = (SELECT COUNT(*) FROM #ReviewBody);
DECLARE @StreetCount INT = (SELECT COUNT(*) FROM #StreetLookup);

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
            (@AdminUserId, N'admin@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-admin', N'Ayşe', N'Yönetim'),
            (@EditorUserId, N'editor@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-editor', N'Mehmet', N'Editör'),
            (@Customer1Id, N'customer1@eimece.test', 1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0, N'seed-customer1', N'Elif', N'Yılmaz');

        INSERT INTO dbo.AspNetUsers
            (Id, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed,
             TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName, FirstName, LastName)
        SELECT
            N'seed-user-' + RIGHT(N'00000000' + CAST(n.n AS NVARCHAR(8)), 8),
            N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test',
            1, @PasswordHash, @SecurityStamp, 0, 0, 1, 0,
            N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
            fn.Name,
            ln.Name
        FROM #Nums n
        INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
        INNER JOIN #LastNames ln ON ln.i = (n.n - 1) % @LastNameCount
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
    CASE n.n % 6
        WHEN 0 THEN N'Ürün görseli ' + CAST(n.n AS NVARCHAR(10)) + N' — ön görünüm'
        WHEN 1 THEN N'Ürün görseli ' + CAST(n.n AS NVARCHAR(10)) + N' — detay'
        WHEN 2 THEN N'Kategori kapak ' + CAST(n.n AS NVARCHAR(10))
        WHEN 3 THEN N'Marka logosu ' + CAST(n.n AS NVARCHAR(10))
        WHEN 4 THEN N'Slider görseli ' + CAST(n.n AS NVARCHAR(10))
        ELSE N'Blog görseli ' + CAST(n.n AS NVARCHAR(10))
    END,
    DATEADD(MINUTE, -n.n, @Now), DATEADD(MINUTE, -n.n, @Now),
    1, n.n, @Lang,
    N'product-' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'.jpg',
    N'/media/seed/images/product-' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'.jpg',
    N'image/jpeg',
    85000 + (n.n % 400000),
    1200, 900,
    N'image',
    0
FROM #Nums n
WHERE n.n <= @SeedFiles;

DECLARE @MinFileId INT = (SELECT MIN(Id) FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%');
DECLARE @MaxFileId INT = (SELECT MAX(Id) FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%');
DECLARE @FileCount INT = @MaxFileId - @MinFileId + 1;


/* ============================================================
   3) Templates
   ============================================================ */
PRINT N'Seeding Templates...';
INSERT INTO dbo.Templates (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, TemplateXml)
SELECT
    tl.Name,
    @Now, @Now, 1, n.n, @Lang,
    N'<!--eimece-seed--><template><fields><field name="Renk" type="text"/><field name="Beden" type="text"/><field name="Malzeme" type="text"/></fields></template>'
FROM #Nums n
INNER JOIN #TemplateLookup tl ON tl.rn = ((n.n - 1) % @TemplateLookupCount) + 1
WHERE n.n <= @SeedTemplates;

DECLARE @MinTemplateId INT = (SELECT MIN(Id) FROM dbo.Templates WHERE TemplateXml LIKE N'%<!--eimece-seed-->%');
DECLARE @TemplateCount INT = (SELECT COUNT(*) FROM dbo.Templates WHERE TemplateXml LIKE N'%<!--eimece-seed-->%');


/* ============================================================
   4) TagCategories + Tags
   ============================================================ */
PRINT N'Seeding TagCategories / Tags...';
INSERT INTO dbo.TagCategories (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang)
SELECT
    tcl.Name + CASE WHEN n.n <= @TagCatLookupCount THEN N'' ELSE N' (' + CAST(n.n AS NVARCHAR(10)) + N')' END,
    @Now, @Now, 1, 900000 + n.n, @Lang
FROM #Nums n
INNER JOIN #TagCatLookup tcl ON tcl.rn = ((n.n - 1) % @TagCatLookupCount) + 1
WHERE n.n <= @SeedTagCategories;

DECLARE @MinTagCatId INT = (SELECT MIN(Id) FROM dbo.TagCategories WHERE Position >= 900000 AND Position < 910000);
DECLARE @TagCatCount INT = (SELECT COUNT(*) FROM dbo.TagCategories WHERE Position >= 900000 AND Position < 910000);

INSERT INTO dbo.Tags (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, TagCategoryId)
SELECT
    tl.Name + CASE WHEN n.n <= @TagLookupCount THEN N'' ELSE N' #' + CAST(n.n AS NVARCHAR(10)) END,
    @Now, @Now, 1, 900000 + n.n, @Lang,
    @MinTagCatId + ((n.n - 1) % @TagCatCount)
FROM #Nums n
INNER JOIN #TagLookup tl ON tl.rn = ((n.n - 1) % @TagLookupCount) + 1
WHERE n.n <= @SeedTags;

DECLARE @MinTagId INT = (SELECT MIN(Id) FROM dbo.Tags WHERE Position >= 900000 AND Position < 910000);
DECLARE @TagCount INT = (SELECT COUNT(*) FROM dbo.Tags WHERE Position >= 900000 AND Position < 910000);


/* ============================================================
   5) Brands
   ============================================================ */
PRINT N'Seeding Brands...';
INSERT INTO dbo.Brands
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId, MainPage)
SELECT
    bl.Name + CASE WHEN n.n <= @BrandLookupCount THEN N'' ELSE N' ' + CAST(n.n AS NVARCHAR(10)) END,
    @Now, @Now, 1, n.n, @Lang,
    bl.Description,
    1, bl.MetaKeywords,
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @SeedMarker,
    CASE WHEN n.n <= 8 THEN 1 ELSE 0 END
FROM #Nums n
INNER JOIN #BrandLookup bl ON bl.rn = ((n.n - 1) % @BrandLookupCount) + 1
WHERE n.n <= @SeedBrands;

DECLARE @MinBrandId INT = (SELECT MIN(Id) FROM dbo.Brands WHERE AddUserId = @SeedMarker);
DECLARE @BrandCount INT = (SELECT COUNT(*) FROM dbo.Brands WHERE AddUserId = @SeedMarker);

IF OBJECT_ID(N'tempdb..#SeedBrandIds') IS NOT NULL DROP TABLE #SeedBrandIds;
SELECT ROW_NUMBER() OVER (ORDER BY Id) AS rn, Id, Name
INTO #SeedBrandIds
FROM dbo.Brands
WHERE AddUserId = @SeedMarker;

IF OBJECT_ID(N'tempdb..#SeedTagIds') IS NOT NULL DROP TABLE #SeedTagIds;
SELECT ROW_NUMBER() OVER (ORDER BY Id) AS rn, Id
INTO #SeedTagIds
FROM dbo.Tags
WHERE Position >= 900000 AND Position < 910000;

IF @BrandCount < 1 OR @TagCount < 1
BEGIN
    RAISERROR(N'Seed Brands/Tags were not created; cannot link products/stories.', 16, 1);
    RETURN;
END;


/* ============================================================
   6) ProductCategories (tree: first roots from lookup, rest children)
   ============================================================ */
PRINT N'Seeding ProductCategories...';
INSERT INTO dbo.ProductCategories
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
     ParentId, MainPage, ShortDescription, TemplateId, DiscountPercantage)
SELECT
    cl.Name + CASE WHEN n.n <= @CategoryLookupCount THEN N'' ELSE N' ' + CAST(n.n AS NVARCHAR(10)) END,
    @Now, @Now, 1, n.n, @Lang,
    cl.Description,
    1, N'kategori,' + LOWER(REPLACE(cl.Name, N' ', N',')),
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @SeedMarker,
    0,  -- ParentId fixed below
    CASE WHEN cl.ParentRn IS NULL AND n.n <= @SeedCategoryRoots THEN 1 ELSE 0 END,
    cl.Description,
    @MinTemplateId + ((n.n - 1) % @TemplateCount),
    cl.Discount
FROM #Nums n
INNER JOIN #CategoryLookup cl ON cl.rn = ((n.n - 1) % @CategoryLookupCount) + 1
WHERE n.n <= @SeedProductCategories;

DECLARE @MinCatId INT = (SELECT MIN(Id) FROM dbo.ProductCategories WHERE AddUserId = @SeedMarker);
DECLARE @CatCount INT = (SELECT COUNT(*) FROM dbo.ProductCategories WHERE AddUserId = @SeedMarker);

/* Fix ParentId using Position + category lookup parent mapping */
;WITH SeedCats AS (
    SELECT pc.Id, pc.Position,
           cl.ParentRn,
           ROW_NUMBER() OVER (ORDER BY pc.Position) AS rn
    FROM dbo.ProductCategories pc
    INNER JOIN #CategoryLookup cl ON cl.rn = ((pc.Position - 1) % @CategoryLookupCount) + 1
    WHERE pc.AddUserId = @SeedMarker
),
RootIds AS (
    SELECT sc.rn, sc.Id
    FROM SeedCats sc
    WHERE sc.ParentRn IS NULL AND sc.rn <= @SeedCategoryRoots
)
UPDATE pc
SET ParentId = CASE
    WHEN sc.ParentRn IS NULL OR sc.rn <= @SeedCategoryRoots THEN 0
    ELSE ISNULL((SELECT TOP 1 r.Id FROM RootIds r WHERE r.rn = ((sc.ParentRn - 1) % @SeedCategoryRoots) + 1), 0)
END
FROM dbo.ProductCategories pc
INNER JOIN SeedCats sc ON sc.Id = pc.Id;


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

IF OBJECT_ID(N'tempdb..#ProductRows') IS NOT NULL DROP TABLE #ProductRows;
SELECT
    n.n AS rn,
    REPLACE(REPLACE(pl.NamePattern, N'{b}',
        CASE WHEN pl.BrandRn BETWEEN 1 AND @BrandLookupCount
             THEN (SELECT Name FROM #BrandLookup WHERE rn = pl.BrandRn)
             ELSE (SELECT Name FROM #BrandLookup WHERE rn = 1 + ((n.n - 1) % @BrandLookupCount))
        END), N'{n}', CAST(n.n AS NVARCHAR(10)))
    + CASE WHEN n.n > @ProductLookupCount THEN N' #' + CAST(n.n AS NVARCHAR(10)) ELSE N'' END AS ProductName,
    CASE WHEN pl.BrandRn BETWEEN 1 AND @BrandCount
         THEN pl.BrandRn
         ELSE 1 + ((ABS(CHECKSUM(N'brand', n.n)) % @BrandCount))
    END AS BrandRn,
    1 + ((pl.CategoryRn - 1) % @CatCount) AS CatOffset,
    CAST(pl.PriceBase + (n.n % NULLIF(CAST(pl.PriceSpread AS INT), 0)) AS DECIMAL(18,2)) AS Price,
    CASE WHEN n.n % 7 = 0 THEN CAST((pl.PriceBase * 0.08) + (n.n % 40) AS DECIMAL(18,2)) ELSE NULL END AS Discount,
    ISNULL(pl.Colors, N'Siyah,Beyaz,Gri') AS Colors,
    ISNULL(pl.Sizes, N'S,M,L,XL') AS Sizes,
    pl.ShortDescription AS ShortDescription,
    pl.DescriptionHtml AS DescriptionHtml,
    N'EIMC-' + RIGHT(N'000000' + CAST(n.n AS NVARCHAR(6)), 6) AS ProductCode
INTO #ProductRows
FROM #Nums n
INNER JOIN #ProductLookup pl ON pl.rn = ((n.n - 1) % @ProductLookupCount) + 1
WHERE n.n <= @SeedProducts;

IF @HasComputedRating = 1
BEGIN
    INSERT INTO dbo.Products
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
         NameShort, NameLong, ProductCategoryId, BrandId, MainPage, ShortDescription,
         Price, Discount, ProductCode, VideoUrl, IsCampaign, ProductColorOptions,
         State, ProductSizeOptions)
    SELECT
        pr.ProductName,
        DATEADD(DAY, -(pr.rn % 365), @Now), DATEADD(DAY, -(pr.rn % 30), @Now),
        CASE WHEN pr.rn % 50 = 0 THEN 0 ELSE 1 END,
        pr.rn, @Lang,
        pr.DescriptionHtml,
        1, N'ürün,e-ticaret,' + LOWER(LEFT(pr.ProductName, 40)),
        @MinFileId + ((pr.rn - 1) % @FileCount),
        @AdminUserId, @SeedMarker,
        LEFT(pr.ProductName, 60),
        pr.ProductName,
        @MinCatId + pr.CatOffset - 1,
        (SELECT Id FROM #SeedBrandIds WHERE rn = pr.BrandRn),
        CASE WHEN pr.rn <= 12 THEN 1 ELSE 0 END,
        pr.ShortDescription,
        pr.Price,
        pr.Discount,
        pr.ProductCode,
        CASE WHEN pr.rn % 20 = 0 THEN N'https://www.youtube.com/watch?v=jNQXAC9IVRw' ELSE NULL END,
        CASE WHEN pr.rn % 11 = 0 THEN 1 ELSE 0 END,
        pr.Colors,
        (SELECT State FROM @ProductStates WHERE i = pr.rn % 10),
        pr.Sizes
    FROM #ProductRows pr;
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
        pr.ProductName,
        DATEADD(DAY, -(pr.rn % 365), @Now), DATEADD(DAY, -(pr.rn % 30), @Now),
        CASE WHEN pr.rn % 50 = 0 THEN 0 ELSE 1 END,
        pr.rn, @Lang,
        pr.DescriptionHtml,
        1, N'ürün,e-ticaret,' + LOWER(LEFT(pr.ProductName, 40)),
        @MinFileId + ((pr.rn - 1) % @FileCount),
        @AdminUserId, @SeedMarker,
        LEFT(pr.ProductName, 60),
        pr.ProductName,
        @MinCatId + pr.CatOffset - 1,
        (SELECT Id FROM #SeedBrandIds WHERE rn = pr.BrandRn),
        CASE WHEN pr.rn <= 12 THEN 1 ELSE 0 END,
        pr.ShortDescription,
        pr.Price,
        pr.Discount,
        pr.ProductCode,
        CASE WHEN pr.rn % 20 = 0 THEN N'https://www.youtube.com/watch?v=jNQXAC9IVRw' ELSE NULL END,
        CASE WHEN pr.rn % 11 = 0 THEN 1 ELSE 0 END,
        pr.Colors,
        (SELECT State FROM @ProductStates WHERE i = pr.rn % 10),
        pr.Sizes,
        CAST((3.2 + (pr.rn % 18) / 10.0) AS FLOAT)
    FROM #ProductRows pr;
END;

DECLARE @MinProductId INT = (SELECT MIN(Id) FROM dbo.Products WHERE AddUserId = @SeedMarker);
DECLARE @ProductCount INT = (SELECT COUNT(*) FROM dbo.Products WHERE AddUserId = @SeedMarker);

/* Guarantee every seed brand has at least one product */
;WITH BrandsRn AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn
    FROM dbo.Brands
    WHERE AddUserId = @SeedMarker
),
ProductsRn AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn
    FROM dbo.Products
    WHERE AddUserId = @SeedMarker
)
UPDATE p
SET BrandId = b.Id
FROM dbo.Products p
INNER JOIN ProductsRn pr ON pr.Id = p.Id
INNER JOIN BrandsRn b ON b.rn = pr.rn
WHERE pr.rn <= @BrandCount;

/* Align product display name brand with assigned BrandId for the first @BrandCount rows */
UPDATE p
SET Name = REPLACE(p.Name, LEFT(p.Name, CHARINDEX(N' ', p.Name + N' ') - 1), b.Name),
    NameLong = REPLACE(p.NameLong, LEFT(p.NameLong, CHARINDEX(N' ', p.NameLong + N' ') - 1), b.Name),
    NameShort = LEFT(REPLACE(p.Name, LEFT(p.Name, CHARINDEX(N' ', p.Name + N' ') - 1), b.Name), 60)
FROM dbo.Products p
INNER JOIN dbo.Brands b ON b.Id = p.BrandId
WHERE p.AddUserId = @SeedMarker
  AND p.Position <= @BrandCount
  AND b.AddUserId = @SeedMarker;


/* ============================================================
   8) ProductFiles / ProductTags / ProductSpecifications / ProductComments
   ============================================================ */
PRINT N'Seeding ProductFiles / ProductTags / ProductSpecifications / ProductComments...';

INSERT INTO dbo.ProductFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, FileStorageId, ProductId)
SELECT
    N'Galeri ' + CAST(1 + ((n.n - 1) % 4) AS NVARCHAR(2)) + N' — ' + LEFT(p.Name, 80),
    @Now, @Now, 1, n.n, @Lang,
    @MinFileId + ((n.n - 1) % @FileCount),
    p.Id
FROM #Nums n
INNER JOIN dbo.Products p ON p.Id = @MinProductId + ((n.n - 1) % @ProductCount)
WHERE n.n <= @SeedProductFiles
  AND p.AddUserId = @SeedMarker;

;WITH SeedProducts AS (
    SELECT p.Id AS ProductId
    FROM dbo.Products p
    WHERE p.AddUserId = @SeedMarker
),
ProductTagPicks AS (
    SELECT
        sp.ProductId,
        t.Id AS TagId,
        ROW_NUMBER() OVER (
            PARTITION BY sp.ProductId
            ORDER BY CHECKSUM(sp.ProductId, t.Id, N'SEED-PT')
        ) AS TagPickRn,
        1 + (ABS(CHECKSUM(CONCAT(N'SEED-PT-COUNT-', CAST(sp.ProductId AS NVARCHAR(20))))) % 4) AS TagCount
    FROM SeedProducts sp
    CROSS JOIN dbo.Tags t
    WHERE t.Position >= 900000 AND t.Position < 910000
)
INSERT INTO dbo.ProductTags (TagId, ProductId)
SELECT DISTINCT ptp.TagId, ptp.ProductId
FROM ProductTagPicks ptp
WHERE ptp.TagPickRn <= ptp.TagCount;

INSERT INTO dbo.ProductSpecifications
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Value, Unit, ProductId)
SELECT
    CASE n.n % 5
        WHEN 0 THEN N'Renk'
        WHEN 1 THEN N'Beden'
        WHEN 2 THEN N'Ağırlık'
        WHEN 3 THEN N'Malzeme'
        ELSE N'Ölçüler'
    END,
    @Now, @Now, 1, n.n, @Lang,
    CASE n.n % 5
        WHEN 0 THEN CASE n.n % 6 WHEN 0 THEN N'Siyah' WHEN 1 THEN N'Beyaz' WHEN 2 THEN N'Lacivert' WHEN 3 THEN N'Bej' WHEN 4 THEN N'Gri' ELSE N'Haki' END
        WHEN 1 THEN CASE n.n % 5 WHEN 0 THEN N'S' WHEN 1 THEN N'M' WHEN 2 THEN N'L' WHEN 3 THEN N'XL' ELSE N'XXL' END
        WHEN 2 THEN CAST((120 + n.n % 880) AS NVARCHAR(10))
        WHEN 3 THEN CASE n.n % 4 WHEN 0 THEN N'Pamuk' WHEN 1 THEN N'Polyester' WHEN 2 THEN N'Deri' ELSE N'Metal' END
        ELSE CAST((20 + n.n % 60) AS NVARCHAR(10)) + N'x' + CAST((15 + n.n % 40) AS NVARCHAR(10)) + N'x' + CAST((5 + n.n % 20) AS NVARCHAR(10))
    END,
    CASE n.n % 5 WHEN 2 THEN N'g' WHEN 4 THEN N'cm' ELSE N'' END,
    @MinProductId + ((n.n - 1) % @ProductCount)
FROM #Nums n
WHERE n.n <= @SeedProductSpecs;

INSERT INTO dbo.ProductComments
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     ProductId, UserId, Review, Email, Subject, Rating)
SELECT
    fn.Name + N' ' + ln.Name,
    DATEADD(HOUR, -n.n, @Now), DATEADD(HOUR, -n.n, @Now),
    CASE WHEN n.n % 15 = 0 THEN 0 ELSE 1 END,
    n.n, @Lang,
    @MinProductId + ((n.n - 1) % @ProductCount),
    CASE WHEN n.n = 1 THEN @Customer1Id ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(8)), 8) END,
    rb.Body,
    CASE WHEN n.n = 1 THEN N'customer1@eimece.test'
         ELSE N'seeduser' + RIGHT(N'00000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(5)), 5) + N'@eimece.test' END,
    rs.Subject,
    3 + (n.n % 3)
FROM #Nums n
INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
INNER JOIN #LastNames ln ON ln.i = (n.n * 3) % @LastNameCount
INNER JOIN #ReviewSubject rs ON rs.i = (n.n - 1) % @ReviewSubjectCount
INNER JOIN #ReviewBody rb ON rb.i = (n.n - 1) % @ReviewBodyCount
WHERE n.n <= @SeedProductComments;


/* ============================================================
   9) StoryCategories / Stories / StoryFiles / StoryTags
   ============================================================ */
PRINT N'Seeding Stories...';

INSERT INTO dbo.StoryCategories
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId, PageTheme)
SELECT
    scl.Name,
    @Now, @Now, 1, n.n, @Lang,
    scl.Description,
    1, N'blog,' + LOWER(REPLACE(scl.Name, N' ', N',')),
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @SeedMarker,
    scl.PageTheme
FROM #Nums n
INNER JOIN #StoryCatLookup scl ON scl.rn = ((n.n - 1) % @StoryCatLookupCount) + 1
WHERE n.n <= @SeedStoryCategories;

DECLARE @MinStoryCatId INT = (SELECT MIN(Id) FROM dbo.StoryCategories WHERE AddUserId = @SeedMarker);
DECLARE @StoryCatCount INT = (SELECT COUNT(*) FROM dbo.StoryCategories WHERE AddUserId = @SeedMarker);

INSERT INTO dbo.Stories
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
     StoryCategoryId, MainPage, AuthorName, IsFeaturedStory, ShortDescription)
SELECT
    sl.Name + CASE WHEN n.n <= @StoryLookupCount THEN N'' ELSE N' (' + CAST(n.n AS NVARCHAR(10)) + N')' END,
    DATEADD(DAY, -(n.n % 200), @Now), @Now,
    1, n.n, @Lang,
    sl.BodyHtml,
    1, N'blog,rehber',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @SeedMarker,
    @MinStoryCatId + ((n.n - 1) % @StoryCatCount),
    CASE WHEN n.n <= 15 THEN 1 ELSE 0 END,
    sl.AuthorName,
    CASE WHEN n.n <= 10 THEN 1 ELSE 0 END,
    sl.ShortDescription
FROM #Nums n
INNER JOIN #StoryLookup sl ON sl.rn = ((n.n - 1) % @StoryLookupCount) + 1
WHERE n.n <= @SeedStories;

DECLARE @MinStoryId INT = (SELECT MIN(Id) FROM dbo.Stories WHERE AddUserId = @SeedMarker);
DECLARE @StoryCount INT = (SELECT COUNT(*) FROM dbo.Stories WHERE AddUserId = @SeedMarker);

INSERT INTO dbo.StoryFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, StoryId, FileStorageId)
SELECT
    N'Kapak — ' + LEFT(s.Name, 80),
    @Now, @Now, 1, n.n, @Lang,
    s.Id,
    @MinFileId + ((n.n - 1) % @FileCount)
FROM #Nums n
INNER JOIN dbo.Stories s ON s.Id = @MinStoryId + ((n.n - 1) % @StoryCount)
WHERE n.n <= @SeedStoryFiles
  AND s.AddUserId = @SeedMarker;

;WITH SeedStories AS (
    SELECT s.Id AS StoryId
    FROM dbo.Stories s
    WHERE s.AddUserId = @SeedMarker
),
StoryTagPicks AS (
    SELECT
        ss.StoryId,
        t.Id AS TagId,
        ROW_NUMBER() OVER (
            PARTITION BY ss.StoryId
            ORDER BY CHECKSUM(ss.StoryId, t.Id, N'SEED-ST')
        ) AS TagPickRn,
        1 + (ABS(CHECKSUM(CONCAT(N'SEED-ST-COUNT-', CAST(ss.StoryId AS NVARCHAR(20))))) % 3) AS TagCount
    FROM SeedStories ss
    CROSS JOIN dbo.Tags t
    WHERE t.Position >= 900000 AND t.Position < 910000
)
INSERT INTO dbo.StoryTags (StoryId, TagId)
SELECT DISTINCT stp.StoryId, stp.TagId
FROM StoryTagPicks stp
WHERE stp.TagPickRn <= stp.TagCount;


/* ============================================================
   10) Menus / MenuFiles / MainPageImages
   ============================================================ */
PRINT N'Seeding Menus / MainPageImages...';

INSERT INTO dbo.Menus
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId,
     ParentId, MainPage, MenuLink, Link, PageTheme, LinkIsActive)
SELECT
    ml.Name,
    @Now, @Now,
    1,
    n.n, @Lang,
    ISNULL(ml.Description, N'<p>' + ml.Name + N'</p>'),
    1, N'sayfa,menü',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @SeedMarker,
    0,
    ml.MainPage,
    ml.MenuLink,
    ml.ExternalLink,
    N'T' + CAST(1 + (n.n % 8) AS NVARCHAR(2)),
    CASE WHEN ml.ExternalLink IS NOT NULL THEN 1 ELSE 0 END
FROM #Nums n
INNER JOIN #MenuLookup ml ON ml.rn = ((n.n - 1) % @MenuLookupCount) + 1
WHERE n.n <= @SeedMenus;

DECLARE @MinMenuId INT = (SELECT MIN(Id) FROM dbo.Menus WHERE AddUserId = @SeedMarker);
DECLARE @MenuCount INT = (SELECT COUNT(*) FROM dbo.Menus WHERE AddUserId = @SeedMarker);

/* Menu tree by Position:
   1 = root (Ana Sayfa / Kurumsal area)
   3,4,9,10,11 = children of position 2 (Kurumsal)
   others under position 1 when applicable */
UPDATE m
SET ParentId = CASE
    WHEN m.Position IN (1, 2, 5, 6, 7, 8, 12) THEN 0
    WHEN m.Position IN (3, 4, 9, 10, 11)
        THEN (SELECT TOP 1 Id FROM dbo.Menus WHERE AddUserId = @SeedMarker AND Position = 2 AND Lang = @Lang)
    ELSE 0
END
FROM dbo.Menus m
WHERE m.AddUserId = @SeedMarker AND m.Lang = @Lang;

INSERT INTO dbo.MenuFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, MenuId, FileStorageId)
SELECT
    N'Sayfa görseli — ' + LEFT(m.Name, 80),
    @Now, @Now, 1, n.n, @Lang,
    m.Id,
    @MinFileId + ((n.n - 1) % @FileCount)
FROM #Nums n
INNER JOIN dbo.Menus m ON m.Id = @MinMenuId + ((n.n - 1) % @MenuCount)
WHERE n.n <= @SeedMenuFiles
  AND m.AddUserId = @SeedMarker;

INSERT INTO dbo.MainPageImages
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, ImageState, MetaKeywords, MainImageId, UpdateUserId, AddUserId, Link)
SELECT
    sl.Name,
    @Now, @Now,
    1,
    n.n, @Lang,
    sl.Description,
    1, N'slider,kampanya',
    @MinFileId + ((n.n - 1) % @FileCount),
    @AdminUserId, @SeedMarker,
    sl.Link
FROM #Nums n
INNER JOIN #SlideLookup sl ON sl.rn = ((n.n - 1) % @SlideLookupCount) + 1
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
        (N'CompanyName', N'EImece', N'Şirket adı'),
        (N'CompanyAddress', N'Caferağa Mah. Moda Cad. No:42 Kadıköy / İstanbul', N'Şirket adresi'),
        (N'WebSiteLogo', N'/images/logo.jpg', N'Logo yolu'),
        (N'WebSiteCompanyEmailAddress', N'info@eimece.test', N'İletişim e-postası'),
        (N'WebSiteCompanyPhoneAndLocation', N'+90 216 555 01 23 | İstanbul', N'Telefon'),
        (N'CargoCompany', N'Yurtiçi Kargo', N'Kargo firması'),
        (N'CargoPrice', N'49.90', N'Kargo ücreti'),
        (N'BasketMinTotalPriceForCargo', N'500', N'Ücretsiz kargo limiti'),
        (N'CargoDescription', N'Standart kargo 2-4 iş günü', N'Kargo açıklaması'),
        (N'SiteIndexMetaTitle', N'EImece — Seçili Markalar, Tek Mağaza', N'Meta başlık'),
        (N'SiteIndexMetaDescription', N'Moda, ev, spor ve elektronik ürünlerinde seçili markalar. Hızlı kargo, güvenli ödeme.', N'Meta açıklama'),
        (N'SiteIndexMetaKeywords', N'eimece,online mağaza,moda,ev,spor', N'Meta anahtar kelimeler'),
        (N'IsProductPriceEnable', N'true', N'Fiyat göster'),
        (N'IsProductReviewEnable', N'true', N'Yorum göster'),
        (N'AdminEmail', N'admin@eimece.test', N'Admin e-posta'),
        (N'AdminUserName', N'admin@eimece.test', N'SMTP kullanıcı'),
        (N'AdminEmailHost', N'smtp.eimece.test', N'SMTP sunucu'),
        (N'AdminEmailPassword', N'seed-smtp-placeholder', N'SMTP parola (placeholder)'),
        (N'AdminEmailPort', N'587', N'SMTP port'),
        (N'AdminEmailEnableSsl', N'true', N'SMTP SSL'),
        (N'AdminEmailUseDefaultCredentials', N'false', N'SMTP varsayılan kimlik'),
        (N'AdminEmailDisplayName', N'EImece Müşteri Hizmetleri', N'SMTP görünen ad'),
        (N'DefaultImageWidth', N'1200', N'Varsayılan görsel genişlik'),
        (N'DefaultImageHeight', N'900', N'Varsayılan görsel yükseklik'),
        (N'FooterDescription', N'EImece — seçili markalar, özenli teslimat.', N'Footer metin'),
        (N'FooterHtmlDescription', N'<p>© EImece. Tüm hakları saklıdır.</p>', N'Footer HTML'),
        (N'FooterEmailListDescription', N'Kampanya ve yeniliklerden haberdar olmak için abone olun.', N'Bülten metni'),
        (N'AboutUs', N'<p>EImece, moda, ev ve yaşam kategorilerinde seçili markaları bir araya getiren online mağazadır.</p>', N'Hakkımızda'),
        (N'PrivacyPolicy', N'<p>Kişisel verileriniz KVKK kapsamında işlenir ve üçüncü taraflarla paylaşılmaz.</p>', N'Gizlilik'),
        (N'TermsAndConditions', N'<p>Sitedeki alışverişler mesafeli satış sözleşmesine tabidir.</p>', N'Şartlar'),
        (N'DeliveryInfo', N'<p>Siparişler ortalama 2-4 iş gününde kargoya verilir. 500 TL üzeri ücretsiz kargo.</p>', N'Teslimat'),
        (N'FacebookWebSiteLink', N'https://facebook.com/eimece', N'Facebook'),
        (N'InstagramWebSiteLink', N'https://instagram.com/eimece', N'Instagram'),
        (N'TwitterWebSiteLink', N'https://twitter.com/eimece', N'Twitter'),
        (N'LinkedinWebSiteLink', N'https://linkedin.com/company/eimece', N'LinkedIn'),
        (N'YotubeWebSiteLink', N'https://youtube.com/@eimece', N'YouTube'),
        (N'PinterestWebSiteLink', N'https://pinterest.com/eimece', N'Pinterest'),
        (N'WhatsAppCommunicationLink', N'https://wa.me/905555550123', N'WhatsApp')
    ) v(SettingKey, SettingValue, Description)
)
INSERT INTO dbo.Settings
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Description, SettingKey, SettingValue)
SELECT
    N'Demo: ' + rs.SettingKey,
    @Now, @Now, 1, 0, @Lang,
    rs.Description, rs.SettingKey, rs.SettingValue
FROM RequiredSettings rs
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Settings s
    WHERE s.SettingKey = rs.SettingKey AND s.Lang = @Lang
);

INSERT INTO dbo.Settings
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Description, SettingKey, SettingValue)
SELECT
    N'Demo ayar ' + CAST(n.n AS NVARCHAR(10)),
    @Now, @Now, 1, n.n, @Lang,
    N'Admin grid demo kaydı',
    N'SEED_Demo_' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
    CASE n.n % 3 WHEN 0 THEN N'true' WHEN 1 THEN N'100' ELSE N'örnek-değer' END
FROM #Nums n
WHERE n.n <= @SeedSettingFillers;

/* Marker row for cleanup of Position-based tags/lists */
IF NOT EXISTS (SELECT 1 FROM dbo.Settings WHERE SettingKey = N'__EIMECE_SEED__' AND Lang = @Lang)
INSERT INTO dbo.Settings
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Description, SettingKey, SettingValue)
VALUES
    (N'Demo seed marker', @Now, @Now, 1, 0, @Lang, N'Internal seed marker — do not edit', N'__EIMECE_SEED__', N'1');

/* ============================================================
   13) MailTemplates (required Names + fillers)
   ============================================================ */
PRINT N'Seeding MailTemplates...';

;WITH RequiredMails AS (
    SELECT * FROM (VALUES
        (N'OrderConfirmationEmail', N'Sipariş Onayı #{OrderNumber}', N'<p>Merhaba, siparişiniz alındı. Sipariş numaranız: #{OrderNumber}</p>'),
        (N'CompanyGotNewOrderEmail', N'Yeni Sipariş #{OrderNumber}', N'<p>Yeni bir sipariş var. Sipariş no: #{OrderNumber}</p>'),
        (N'ConfirmYourAccount', N'Hesabınızı Onaylayın', N'<p>Lütfen hesabınızı onaylayın: {CallbackUrl}</p>'),
        (N'ForgotPassword', N'Şifre Sıfırlama', N'<p>Şifre sıfırlama bağlantısı: {CallbackUrl}</p>'),
        (N'ContactUsAboutProductInfo', N'Ürün Bilgi Talebi', N'<p>Ürün hakkında müşteri mesajı.</p>'),
        (N'ContactUsForCommunication', N'İletişim Formu', N'<p>İletişim formu mesajı.</p>'),
        (N'SendMessageToSeller', N'Satıcıya Mesaj', N'<p>Satıcıya iletilen mesaj.</p>')
    ) v(Name, Subject, Body)
)
INSERT INTO dbo.MailTemplates
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
SELECT
    rm.Name, @Now, @Now, 1, 0, @Lang, rm.Subject, rm.Body, @AdminUserId, @SeedMarker, 0, 0
FROM RequiredMails rm
WHERE NOT EXISTS (SELECT 1 FROM dbo.MailTemplates mt WHERE mt.Name = rm.Name);

INSERT INTO dbo.MailTemplates
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Subject, Body, UpdateUserId, AddUserId, TrackWithBitly, TrackWithMlnk)
SELECT
    CASE n.n
        WHEN 1 THEN N'Stok Bildirimi'
        WHEN 2 THEN N'Kargo Çıktı'
        WHEN 3 THEN N'İade Onayı'
        WHEN 4 THEN N'Hoş Geldiniz'
        ELSE N'Kampanya Duyurusu ' + CAST(n.n AS NVARCHAR(10))
    END,
    @Now, @Now, 1, n.n, @Lang,
    CASE n.n
        WHEN 1 THEN N'Takip ettiğiniz ürün stokta'
        WHEN 2 THEN N'Siparişiniz kargoya verildi'
        WHEN 3 THEN N'İade talebiniz onaylandı'
        WHEN 4 THEN N'EImece''ye hoş geldiniz'
        ELSE N'Özel kampanya fırsatları'
    END,
    N'<p>EImece müşteri iletişimi — otomatik bildirim şablonu.</p>',
    @AdminUserId, @SeedMarker, 0, 0
FROM #Nums n
WHERE n.n <= @SeedMailTemplateFillers;


/* ============================================================
   14) Lists / ListItems
   ============================================================ */
PRINT N'Seeding Lists / ListItems...';
INSERT INTO dbo.Lists (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, IsService, IsValues)
SELECT
    ll.Name,
    @Now, @Now, 1, 900000 + n.n, @Lang,
    ll.IsService,
    ll.IsValues
FROM #Nums n
INNER JOIN #ListLookup ll ON ll.rn = ((n.n - 1) % @ListLookupCount) + 1
WHERE n.n <= @SeedLists;

DECLARE @MinListId INT = (SELECT MIN(Id) FROM dbo.Lists WHERE Position >= 900000 AND Position < 910000);
DECLARE @ListCount INT = (SELECT COUNT(*) FROM dbo.Lists WHERE Position >= 900000 AND Position < 910000);

INSERT INTO dbo.ListItems (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, ListId, Value)
SELECT
    lil.Name,
    @Now, @Now, 1, 900000 + n.n, @Lang,
    @MinListId + ((n.n - 1) % @ListCount),
    LOWER(REPLACE(lil.Name, N' ', N'-'))
FROM #Nums n
INNER JOIN #ListItemLookup lil ON lil.rn = ((n.n - 1) % @ListItemLookupCount) + 1
WHERE n.n <= @SeedListItems;

/* ============================================================
   15) Faqs / Subscribers / Coupons
   ============================================================ */
PRINT N'Seeding Faqs / Subscribers / Coupons...';

INSERT INTO dbo.Faqs
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Question, Answer, AddUserId, UpdateUserId)
SELECT
    LEFT(fl.Question, 100),
    @Now, @Now, 1, n.n, @Lang,
    fl.Question,
    fl.Answer,
    @SeedMarker, @AdminUserId
FROM #Nums n
INNER JOIN #FaqLookup fl ON fl.rn = ((n.n - 1) % @FaqLookupCount) + 1
WHERE n.n <= @SeedFaqs;

INSERT INTO dbo.Subscribers
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, Email, Note)
SELECT
    fn.Name + N' ' + ln.Name,
    @Now, @Now, 1, n.n, @Lang,
    N'subscriber' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test',
    N'Bülten abonesi — web formu'
FROM #Nums n
INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
INNER JOIN #LastNames ln ON ln.i = (n.n * 5) % @LastNameCount
WHERE n.n <= @SeedSubscribers;

INSERT INTO dbo.Coupons
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Code, DiscountPercentage, Discount, StartDate, EndDate)
SELECT
    cl.Name,
    @Now, @Now,
    CASE WHEN n.n % 20 = 0 THEN 0 ELSE 1 END,
    n.n, @Lang,
    CASE WHEN n.n <= @CouponLookupCount THEN cl.Code
         ELSE cl.Code + N'-' + CAST(n.n AS NVARCHAR(10)) END,
    cl.DiscountPercentage,
    cl.Discount,
    DATEADD(DAY, -30, @Now),
    DATEADD(DAY, 90 + (n.n % 180), @Now)
FROM #Nums n
INNER JOIN #CouponLookup cl ON cl.rn = ((n.n - 1) % @CouponLookupCount) + 1
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
    fn.Name,
    DATEADD(DAY, -(n.n % 400), @Now), @Now,
    1, n.n, @Lang,
    ln.Name,
    N'05' + RIGHT(N'000000000' + CAST((320000000 + n.n * 17) AS NVARCHAR(9)), 9),
    CASE WHEN n.n = 1 THEN N'customer1@eimece.test'
         ELSE N'seeduser' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'@eimece.test' END,
    RIGHT(N'00000000000' + CAST((10000000000 + n.n * 97) AS NVARCHAR(11)), 11),
    N'176.88.' + CAST((n.n % 200) + 1 AS NVARCHAR(3)) + N'.' + CAST((n.n % 254) + 1 AS NVARCHAR(3)),
    CASE WHEN n.n = 1 THEN @Customer1Id
         ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(n.n AS NVARCHAR(8)), 8) END,
    1,
    n.n % 3,
    st.Name + N' No:' + CAST(10 + (n.n % 120) AS NVARCHAR(10)),
    c.District,
    c.District,
    c.City,
    N'Türkiye',
    RIGHT(N'00000' + CAST((34000 + n.n % 1000) AS NVARCHAR(5)), 5),
    N'Daire ' + CAST((n.n % 40) + 1 AS NVARCHAR(10)) + N' / Kat ' + CAST((n.n % 8) + 1 AS NVARCHAR(10)),
    CASE WHEN n.n % 8 = 0 THEN fn.Name + N' ' + ln.Name + N' Ticaret Ltd. Şti.' ELSE NULL END,
    CASE WHEN n.n % 8 = 0 THEN 2 ELSE 1 END
FROM #Nums n
INNER JOIN #Cities c ON c.i = n.n % 10
INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
INNER JOIN #LastNames ln ON ln.i = (n.n - 1) % @LastNameCount
INNER JOIN #StreetLookup st ON st.i = (n.n - 1) % @StreetCount
WHERE n.n <= @SeedCustomers;

INSERT INTO dbo.Addresses
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
     Description, AddressType, City, Country, ZipCode, Street, District)
SELECT
    CASE WHEN n.n % 2 = 1 THEN N'Ev Adresi' ELSE N'İş Adresi' END
        + N' — ' + fn.Name + N' ' + ln.Name,
    @Now, @Now, 1, 900000 + n.n, @Lang,
    st.Name + N' No:' + CAST(5 + (n.n % 90) AS NVARCHAR(10)) + N', Daire ' + CAST((n.n % 20) + 1 AS NVARCHAR(10)),
    CASE WHEN n.n % 2 = 1 THEN 1 ELSE 2 END,
    c.City,
    N'Türkiye',
    RIGHT(N'00000' + CAST((34000 + n.n % 1000) AS NVARCHAR(5)), 5),
    st.Name + N' No:' + CAST(5 + (n.n % 90) AS NVARCHAR(10)),
    c.District
FROM #Nums n
INNER JOIN #Cities c ON c.i = n.n % 10
INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
INNER JOIN #LastNames ln ON ln.i = (n.n * 7) % @LastNameCount
INNER JOIN #StreetLookup st ON st.i = (n.n - 1) % @StreetCount
WHERE n.n <= @SeedAddresses;

DECLARE @MinAddressId INT = (SELECT MIN(Id) FROM dbo.Addresses WHERE Position >= 900000 AND Position < 1000000);
DECLARE @AddressCount INT = (SELECT COUNT(*) FROM dbo.Addresses WHERE Position >= 900000 AND Position < 1000000);


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
    N'Sipariş ' + N'EIMC-' + RIGHT(N'0000000' + CAST(n.n AS NVARCHAR(7)), 7),
    DATEADD(DAY, -(n.n % 180), @Now),
    DATEADD(DAY, -(n.n % 180), @Now),
    1, n.n, @Lang,
    DATEADD(DAY, 3 + (n.n % 10), DATEADD(DAY, -(n.n % 180), @Now)),
    CASE WHEN n.n = 1 THEN @Customer1Id
         WHEN n.n % 17 = 0 THEN N'BNC'
         WHEN n.n % 19 = 0 THEN N'SWA'
         ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(8)), 8) END,
    1 + (n.n % 3),
    1 + (n.n % 8),
    CASE WHEN n.n % 10 = 0 THEN N'Müşteri aradı — teslimat günü netleştirildi' ELSE NULL END,
    CASE n.n % 5
        WHEN 0 THEN N'Lütfen zili çalıştırmayın, kapıya bırakın.'
        WHEN 1 THEN N'Fatura e-posta ile gelsin.'
        WHEN 2 THEN N'Hediye paketi olsun.'
        ELSE N''
    END,
    N'EIMC-' + RIGHT(N'0000000' + CAST(n.n AS NVARCHAR(7)), 7),
    CAST(CASE WHEN (350 + (n.n % 1200)) >= 500 THEN 0 ELSE 49.90 END AS DECIMAL(18,2)),
    @MinAddressId + ((n.n - 1) % @AddressCount),
    @MinAddressId + (n.n % @AddressCount),
    LOWER(CONVERT(NVARCHAR(36), NEWID())),
    CASE WHEN n.n % 12 = 0 THEN (SELECT Code FROM #CouponLookup WHERE rn = 1 + ((n.n - 1) % @CouponLookupCount)) ELSE NULL END,
    CASE WHEN n.n % 12 = 0 THEN N'10' ELSE NULL END,
    LOWER(CONVERT(NVARCHAR(64), NEWID())),
    CAST(CAST((350.0 + (n.n % 1200)) AS DECIMAL(18,2)) AS NVARCHAR(50)),
    CAST(CAST((350.0 + (n.n % 1200)) AS DECIMAL(18,2)) AS NVARCHAR(50)),
    CAST(1 + (n.n % 6) AS NVARCHAR(10)),
    N'TRY',
    N'pay_eimece_' + CAST(n.n AS NVARCHAR(10)),
    CASE WHEN n.n % 9 = 0 THEN N'FAILED' ELSE N'SUCCESS' END,
    CASE WHEN n.n % 30 = 0 THEN 1 ELSE 0 END,
    N'2.5', N'5.00', N'3.00', N'1.50',
    N'CREDIT_CARD',
    CASE n.n % 3 WHEN 0 THEN N'MASTER_CARD' WHEN 1 THEN N'VISA' ELSE N'AMEX' END,
    CASE n.n % 4 WHEN 0 THEN N'Bonus' WHEN 1 THEN N'Maximum' WHEN 2 THEN N'Axess' ELSE N'World' END,
    NULL, NULL,
    CASE n.n % 3 WHEN 0 THEN N'554960' WHEN 1 THEN N'450803' ELSE N'374245' END,
    RIGHT(N'0000' + CAST((1000 + (n.n * 37) % 9000) AS NVARCHAR(4)), 4),
    N'basket_' + CAST(n.n AS NVARCHAR(10)),
    N'conv_' + CAST(n.n AS NVARCHAR(10)),
    NULL, N'AUTH' + CAST(n.n AS NVARCHAR(10)), NULL, N'AUTH',
    CASE WHEN n.n % 9 = 0 THEN N'failure' ELSE N'success' END,
    CASE WHEN n.n % 9 = 0 THEN N'5001' ELSE NULL END,
    CASE WHEN n.n % 9 = 0 THEN N'Ödeme banka tarafından reddedildi' ELSE NULL END,
    N'tr',
    CAST(DATEDIFF(SECOND, '1970-01-01', DATEADD(DAY, -(n.n % 180), @Now)) AS BIGINT) * 1000,
    CASE WHEN (1 + (n.n % 8)) >= 4 THEN N'TRK' + RIGHT(N'000000000' + CAST(100000000 + n.n AS NVARCHAR(9)), 9) ELSE NULL END,
    CASE WHEN (1 + (n.n % 8)) >= 4 THEN N'Yurtiçi Kargo' ELSE NULL END
FROM #Nums n
WHERE n.n <= @SeedOrders;

DECLARE @MinOrderId INT = (SELECT MIN(Id) FROM dbo.Orders WHERE OrderNumber LIKE N'EIMC-%');
DECLARE @OrderCount INT = (SELECT COUNT(*) FROM dbo.Orders WHERE OrderNumber LIKE N'EIMC-%');

INSERT INTO dbo.OrderProducts
    (OrderId, ProductId, Quantity, TotalPrice, ProductSalePrice, ProductName, ProductCode, CategoryName, ProductSpecItems)
SELECT
    o.Id,
    p.Id,
    1 + (n.n % 3),
    CAST((1 + (n.n % 3)) * p.Price AS DECIMAL(18,2)),
    p.Price,
    p.Name,
    p.ProductCode,
    ISNULL(pc.Name, N'Genel'),
    N'[{"Name":"Renk","Value":"Siyah"}]'
FROM #Nums n
INNER JOIN dbo.Orders o ON o.Id = @MinOrderId + ((n.n - 1) % @OrderCount)
INNER JOIN dbo.Products p ON p.Id = @MinProductId + ((n.n - 1) % @ProductCount)
LEFT JOIN dbo.ProductCategories pc ON pc.Id = p.ProductCategoryId
WHERE n.n <= @SeedOrderProducts
  AND o.OrderNumber LIKE N'EIMC-%'
  AND p.AddUserId = @SeedMarker;

INSERT INTO dbo.ShoppingCarts
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, OrderGuid, ShoppingCartJson, UserId)
SELECT
    N'Sepet — ' + fn.Name + N' ' + ln.Name,
    @Now, @Now, 1, n.n, @Lang,
    LOWER(CONVERT(NVARCHAR(36), NEWID())),
    N'{"Items":[{"ProductId":' + CAST(@MinProductId + ((n.n - 1) % @ProductCount) AS NVARCHAR(20))
        + N',"Quantity":' + CAST(1 + (n.n % 3) AS NVARCHAR(10)) + N'}]}',
    CASE WHEN n.n = 1 THEN @Customer1Id
         ELSE N'seed-user-' + RIGHT(N'00000000' + CAST(((n.n - 1) % @SeedUsers) + 1 AS NVARCHAR(8)), 8) END
FROM #Nums n
INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
INNER JOIN #LastNames ln ON ln.i = (n.n - 1) % @LastNameCount
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
        CASE n.n WHEN 1 THEN N'Masaüstü bildirimleri' WHEN 2 THEN N'Mobil bildirimleri' ELSE N'Kampanya bildirimleri' END,
        @Now, @Now, 1, 900000 + n.n, @Lang,
        N'mailto:admin@eimece.test',
        n.n % 3,
        N'BMAC1_seed_public_' + CAST(n.n AS NVARCHAR(10)),
        N'seed_private_' + CAST(n.n AS NVARCHAR(10))
    FROM #Nums n
    WHERE n.n <= @SeedBrowserSubscriptions;

    DECLARE @MinBrowserSubId INT = (SELECT MIN(Id) FROM dbo.BrowserSubscriptions WHERE Position >= 900000 AND Position < 910000);
    DECLARE @BrowserSubCount INT = (SELECT COUNT(*) FROM dbo.BrowserSubscriptions WHERE Position >= 900000 AND Position < 910000);

    INSERT INTO dbo.BrowserSubscribers
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         BrowserSubscriptionId, EndPoint, Auth, P256dh, UserAgent, UserAddress)
    SELECT
        N'Abone ' + fn.Name + N' ' + ln.Name,
        @Now, @Now, 1, 900000 + n.n, @Lang,
        @MinBrowserSubId + ((n.n - 1) % @BrowserSubCount),
        N'https://fcm.googleapis.com/fcm/send/seed-endpoint-' + CAST(n.n AS NVARCHAR(10)),
        N'auth' + CAST(n.n AS NVARCHAR(10)),
        N'p256dh' + CAST(n.n AS NVARCHAR(10)),
        N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
        N'176.88.' + CAST((n.n % 200) + 1 AS NVARCHAR(3)) + N'.' + CAST((n.n % 254) + 1 AS NVARCHAR(3))
    FROM #Nums n
    INNER JOIN #FirstNames fn ON fn.i = (n.n - 1) % @FirstNameCount
    INNER JOIN #LastNames ln ON ln.i = (n.n - 1) % @LastNameCount
    WHERE n.n <= @SeedBrowserSubscribers;

    DECLARE @MinBrowserSubscriberId INT = (SELECT MIN(Id) FROM dbo.BrowserSubscribers WHERE Position >= 900000 AND Position < 1000000);
    DECLARE @BrowserSubscriberCount INT = (SELECT COUNT(*) FROM dbo.BrowserSubscribers WHERE Position >= 900000 AND Position < 1000000);

    INSERT INTO dbo.BrowserNotifications
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         NotificationType, Body, ImageUrl, RedirectionUrl)
    SELECT
        CASE n.n % 5
            WHEN 0 THEN N'Sepetiniz sizi bekliyor'
            WHEN 1 THEN N'Kargonuz yola çıktı'
            WHEN 2 THEN N'Yeni sezon indirimi'
            WHEN 3 THEN N'Stoklara girdi'
            ELSE N'Özel kuponunuz hazır'
        END,
        @Now, @Now, 1, 900000 + n.n, @Lang,
        n.n % 5,
        CASE n.n % 5
            WHEN 0 THEN N'Sepetinizdeki ürünlerde stok azaluyor. Alışverişi tamamlayın.'
            WHEN 1 THEN N'Siparişiniz kargoya verildi. Takip numarası hesabınızda.'
            WHEN 2 THEN N'Seçili kategorilerde %20''ye varan indirim başladı.'
            WHEN 3 THEN N'Takip ettiğiniz ürün tekrar stokta.'
            ELSE N'EIMC-HOSGELDIN kodu ile ilk siparişinizde %15 indirim.'
        END,
        N'/media/seed/images/product-' + RIGHT(N'00000' + CAST(((n.n - 1) % @FileCount) + 1 AS NVARCHAR(5)), 5) + N'.jpg',
        N'/products'
    FROM #Nums n
    WHERE n.n <= @SeedBrowserNotifications;

    DECLARE @MinBrowserNotificationId INT = (SELECT MIN(Id) FROM dbo.BrowserNotifications WHERE Position >= 900000 AND Position < 1000000);
    DECLARE @BrowserNotificationCount INT = (SELECT COUNT(*) FROM dbo.BrowserNotifications WHERE Position >= 900000 AND Position < 1000000);

    INSERT INTO dbo.BrowserNotificationFeedBacks
        (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang,
         BrowserNotificationId, BrowserSubscriberId, NotificationStatus, DateSend, DateTracked)
    SELECT
        N'Bildirim sonucu #' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, 900000 + n.n, @Lang,
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
        N'Kampanya linki ' + CAST(n.n AS NVARCHAR(10)),
        @Now, @Now, 1, 900000 + n.n, @Lang,
        N'e' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5),
        N'https://eimece.test/c/kampanya-' + CAST(n.n AS NVARCHAR(10)),
        50 + (n.n % 900)
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
            N'WEB-01',
            CASE n.n % 5
                WHEN 0 THEN N'Sipariş ödeme doğrulaması başarısız'
                WHEN 1 THEN N'Yavaş sorgu tespit edildi: ProductRepository'
                WHEN 2 THEN N'Kullanıcı girişi başarılı'
                WHEN 3 THEN N'Önbellek yenilendi'
                ELSE N'Kritik: dış ödeme servisi zaman aşımı'
            END,
            CASE WHEN n.n % 5 = 0 THEN N'EImece.Domain' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'PaymentService' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'Charge' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'Gateway timeout after 30s' ELSE NULL END,
            CASE WHEN n.n % 10 = 0 THEN N'SocketException: connection reset' ELSE NULL END,
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
            N'WEB-01',
            CASE n.n % 5
                WHEN 0 THEN N'Sipariş ödeme doğrulaması başarısız'
                WHEN 1 THEN N'Yavaş sorgu tespit edildi: ProductRepository'
                WHEN 2 THEN N'Kullanıcı girişi başarılı'
                WHEN 3 THEN N'Önbellek yenilendi'
                ELSE N'Kritik: dış ödeme servisi zaman aşımı'
            END,
            CASE WHEN n.n % 5 = 0 THEN N'EImece.Domain' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'PaymentService' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'Charge' ELSE NULL END,
            CASE WHEN n.n % 5 = 0 THEN N'Gateway timeout after 30s' ELSE NULL END,
            CASE WHEN n.n % 10 = 0 THEN N'SocketException: connection reset' ELSE NULL END
        FROM #Nums n
        WHERE n.n <= @SeedAppLogs;
    END
END;

COMMIT TRANSACTION;

/* ========================= SUMMARY ========================= */
PRINT N'';
PRINT N'========== SEED SUMMARY ==========';
SELECT N'AspNetUsers (seed)' AS [Table], COUNT(*) AS [Rows] FROM dbo.AspNetUsers WHERE UserName LIKE N'seed%' OR Email LIKE N'%@eimece.test'
UNION ALL SELECT N'FileStorages', COUNT(*) FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%'
UNION ALL SELECT N'Templates', COUNT(*) FROM dbo.Templates WHERE TemplateXml LIKE N'%<!--eimece-seed-->%'
UNION ALL SELECT N'TagCategories', COUNT(*) FROM dbo.TagCategories WHERE Position >= 900000 AND Position < 910000
UNION ALL SELECT N'Tags', COUNT(*) FROM dbo.Tags WHERE Position >= 900000 AND Position < 910000
UNION ALL SELECT N'Brands', COUNT(*) FROM dbo.Brands WHERE AddUserId = N'SEED'
UNION ALL SELECT N'ProductCategories', COUNT(*) FROM dbo.ProductCategories WHERE AddUserId = N'SEED'
UNION ALL SELECT N'Products', COUNT(*) FROM dbo.Products WHERE AddUserId = N'SEED'
UNION ALL SELECT N'ProductFiles', COUNT(*) FROM dbo.ProductFiles pf INNER JOIN dbo.Products p ON p.Id = pf.ProductId WHERE p.AddUserId = N'SEED'
UNION ALL SELECT N'ProductTags', COUNT(*) FROM dbo.ProductTags pt INNER JOIN dbo.Products p ON p.Id = pt.ProductId WHERE p.AddUserId = N'SEED'
UNION ALL SELECT N'Products with BrandId', COUNT(*) FROM dbo.Products WHERE AddUserId = N'SEED' AND BrandId IS NOT NULL
UNION ALL SELECT N'Brands with products', COUNT(*) FROM dbo.Brands b WHERE b.AddUserId = N'SEED' AND EXISTS (SELECT 1 FROM dbo.Products p WHERE p.BrandId = b.Id)
UNION ALL SELECT N'ProductSpecifications', COUNT(*) FROM dbo.ProductSpecifications ps INNER JOIN dbo.Products p ON p.Id = ps.ProductId WHERE p.AddUserId = N'SEED'
UNION ALL SELECT N'ProductComments', COUNT(*) FROM dbo.ProductComments WHERE Email LIKE N'%@eimece.test'
UNION ALL SELECT N'StoryCategories', COUNT(*) FROM dbo.StoryCategories WHERE AddUserId = N'SEED'
UNION ALL SELECT N'Stories', COUNT(*) FROM dbo.Stories WHERE AddUserId = N'SEED'
UNION ALL SELECT N'StoryFiles', COUNT(*) FROM dbo.StoryFiles sf INNER JOIN dbo.Stories s ON s.Id = sf.StoryId WHERE s.AddUserId = N'SEED'
UNION ALL SELECT N'StoryTags', COUNT(*) FROM dbo.StoryTags st INNER JOIN dbo.Stories s ON s.Id = st.StoryId WHERE s.AddUserId = N'SEED'
UNION ALL SELECT N'Stories with tags', COUNT(DISTINCT s.Id) FROM dbo.Stories s INNER JOIN dbo.StoryTags st ON st.StoryId = s.Id WHERE s.AddUserId = N'SEED'
UNION ALL SELECT N'Menus', COUNT(*) FROM dbo.Menus WHERE AddUserId = N'SEED'
UNION ALL SELECT N'MainPageImages', COUNT(*) FROM dbo.MainPageImages WHERE AddUserId = N'SEED'
UNION ALL SELECT N'Settings', COUNT(*) FROM dbo.Settings WHERE Name LIKE N'Demo: %' OR SettingKey LIKE N'SEED_%' OR SettingKey = N'__EIMECE_SEED__'
UNION ALL SELECT N'MailTemplates', COUNT(*) FROM dbo.MailTemplates WHERE AddUserId = N'SEED'
UNION ALL SELECT N'Lists', COUNT(*) FROM dbo.Lists WHERE Position >= 900000 AND Position < 910000
UNION ALL SELECT N'ListItems', COUNT(*) FROM dbo.ListItems WHERE Position >= 900000 AND Position < 910000
UNION ALL SELECT N'Faqs', COUNT(*) FROM dbo.Faqs WHERE AddUserId = N'SEED'
UNION ALL SELECT N'Subscribers', COUNT(*) FROM dbo.Subscribers WHERE Email LIKE N'%@eimece.test'
UNION ALL SELECT N'Coupons', COUNT(*) FROM dbo.Coupons WHERE Code LIKE N'EIMC-%'
UNION ALL SELECT N'Customers', COUNT(*) FROM dbo.Customers WHERE Email LIKE N'%@eimece.test'
UNION ALL SELECT N'Addresses', COUNT(*) FROM dbo.Addresses WHERE Position >= 900000 AND Position < 1000000
UNION ALL SELECT N'Orders', COUNT(*) FROM dbo.Orders WHERE OrderNumber LIKE N'EIMC-%'
UNION ALL SELECT N'OrderProducts', COUNT(*) FROM dbo.OrderProducts op INNER JOIN dbo.Orders o ON o.Id = op.OrderId WHERE o.OrderNumber LIKE N'EIMC-%'
UNION ALL SELECT N'ShoppingCarts', COUNT(*) FROM dbo.ShoppingCarts WHERE UserId LIKE N'seed%'
ORDER BY [Table];

PRINT N'';
PRINT N'Test logins (shared seed credential = N''Test'' + N''123'' + N''!''):';
PRINT N'  admin@eimece.test / Admin';
PRINT N'  editor@eimece.test / NormalUser';
PRINT N'  customer1@eimece.test / Customer';
PRINT CONVERT(VARCHAR(30), GETDATE(), 121) + N' — Seed complete.';

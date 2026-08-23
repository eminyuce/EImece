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
    ~21 menus (12 nav/CMS + Tema Ornekleri + PT Dummy T1–T8), ~6 homepage slides,
    ~20 brands, ~25 categories, ~150 products, ~30 stories, ~40 customers/users,
    ~100 orders, plus supporting rows.

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
DECLARE @CleanupFirst  BIT          = 0;      -- 1 = wipe previous seed data first
DECLARE @Lang          INT          = 2;      -- 1=TR, 2=EN
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
DECLARE @SeedMenus              INT = 21;   -- 12 nav/CMS + Tema Ornekleri + PT Dummy T1–T8
DECLARE @SeedMenuFiles          INT = 96;   -- 8 theme pages × 12 MenuGallery images
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

/* One dedicated FileStorage row per image reference (MainImage + gallery files).
   Never share FileStorage across entities — shared IDs break deletes via FK_ProductFiles_FileStorages. */
DECLARE @SeedFiles INT =
      @SeedBrands
    + @SeedProductCategories
    + @SeedProducts
    + @SeedProductFiles
    + @SeedStoryCategories
    + @SeedStories
    + @SeedStoryFiles
    + @SeedMenus
    + @SeedMenuFiles
    + @SeedMainPageImages;

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
    + N', ExclusiveFiles=' + CAST(@SeedFiles AS VARCHAR(10))
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
        DELETE mf FROM dbo.MenuFiles mf INNER JOIN dbo.Menus m ON m.Id = mf.MenuId
        WHERE m.AddUserId = N'SEED' OR m.Name LIKE N'SEED %' OR mf.Name LIKE N'SEED %'
           OR m.Name LIKE N'PT Dummy T%' OR m.Name = N'Tema Ornekleri';
    IF OBJECT_ID(N'dbo.Menus', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.Menus SET MainImageId = NULL
        WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %' OR Name LIKE N'PT Dummy T%' OR Name = N'Tema Ornekleri';
        DELETE FROM dbo.Menus
        WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %' OR Name LIKE N'PT Dummy T%' OR Name = N'Tema Ornekleri';
    END
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

INSERT INTO #BrandLookup VALUES (1,N'Nordline',N'Scandinavian style furniture and home decor.',N'furniture,home,decor');
INSERT INTO #BrandLookup VALUES (2,N'Atlas Textile',N'Everyday clothing and basic textile products.',N'textile,clothing,cotton');
INSERT INTO #BrandLookup VALUES (3,N'Sportiva',N'Running, fitness and outdoor equipment.',N'sports,fitness,outdoor');
INSERT INTO #BrandLookup VALUES (4,N'Lumina Kitchen',N'Kitchen tools and small appliances.',N'kitchen,home appliance');
INSERT INTO #BrandLookup VALUES (5,N'Beauté Lab',N'Skincare and personal care products.',N'cosmetics,skincare');
INSERT INTO #BrandLookup VALUES (6,N'TechPlus',N'Electronic accessories and computer peripherals.',N'electronics,accessories');
INSERT INTO #BrandLookup VALUES (7,N'Casa Bella',N'Home textiles, bedding and bathroom products.',N'home textile,bedding');
INSERT INTO #BrandLookup VALUES (8,N'MiniNest',N'Baby and children''s products.',N'baby,children');
INSERT INTO #BrandLookup VALUES (9,N'Meridian Outdoor',N'Camping, hiking and outdoor sports.',N'camping,outdoor');
INSERT INTO #BrandLookup VALUES (10,N'Leather Workshop',N'Leather bags, belts and wallets.',N'leather,accessories');
INSERT INTO #BrandLookup VALUES (11,N'AquaPure',N'Water filtration and healthy living products.',N'water,health');
INSERT INTO #BrandLookup VALUES (12,N'Book Corner',N'Books, stationery and hobby products.',N'books,stationery');
INSERT INTO #BrandLookup VALUES (13,N'UrbanWear',N'Street style and everyday fashion.',N'fashion,streetwear');
INSERT INTO #BrandLookup VALUES (14,N'ChefPro',N'Professional kitchen equipment.',N'kitchen,professional');
INSERT INTO #BrandLookup VALUES (15,N'GreenLeaf',N'Organic food and natural products.',N'organic,natural');
INSERT INTO #BrandLookup VALUES (16,N'SoundWave',N'Headphones, speakers and audio systems.',N'audio,headphones');
INSERT INTO #BrandLookup VALUES (17,N'FitLife',N'Sportswear and active lifestyle.',N'sportswear,active');
INSERT INTO #BrandLookup VALUES (18,N'HomeGlow',N'Lighting and home decoration.',N'lighting,decor');
INSERT INTO #BrandLookup VALUES (19,N'PetFriend',N'Pet care and accessories.',N'pet,animal');
INSERT INTO #BrandLookup VALUES (20,N'Voyage Pack',N'Luggage, backpacks and travel accessories.',N'travel,luggage');

IF OBJECT_ID(N'tempdb..#CategoryLookup') IS NOT NULL DROP TABLE #CategoryLookup;
CREATE TABLE #CategoryLookup (
    rn INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(400) NOT NULL,
    ParentRn INT NULL,          -- NULL = root; else 1-based root rn
    Discount FLOAT NULL
);

INSERT INTO #CategoryLookup VALUES (1,N'Electronics',N'Phones, computers and electronic accessories.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (2,N'Fashion & Apparel',N'Women, men and unisex clothing.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (3,N'Home & Living',N'Furniture, decoration and home textiles.',NULL,10.0);
INSERT INTO #CategoryLookup VALUES (4,N'Sports & Outdoor',N'Sportswear, equipment and camping gear.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (5,N'Cosmetics & Care',N'Skincare, makeup and personal care.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (6,N'Baby & Kids',N'Baby care and children''s products.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (7,N'Books & Hobby',N'Books, stationery and hobby supplies.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (8,N'Kitchen',N'Kitchen tools and small appliances.',NULL,NULL);
INSERT INTO #CategoryLookup VALUES (9,N'Headphones & Audio',N'Wireless headphones and speakers.',1,NULL);
INSERT INTO #CategoryLookup VALUES (10,N'Phone Accessories',N'Cases, chargers and screen protectors.',1,NULL);
INSERT INTO #CategoryLookup VALUES (11,N'Women''s Clothing',N'Dresses, blouses and outerwear.',2,NULL);
INSERT INTO #CategoryLookup VALUES (12,N'Men''s Clothing',N'Shirts, pants and jackets.',2,NULL);
INSERT INTO #CategoryLookup VALUES (13,N'Shoes',N'Sports and casual shoes.',2,NULL);
INSERT INTO #CategoryLookup VALUES (14,N'Living Room',N'Sofas, armchairs and coffee tables.',3,NULL);
INSERT INTO #CategoryLookup VALUES (15,N'Bedroom',N'Bedding, pillows and duvets.',3,NULL);
INSERT INTO #CategoryLookup VALUES (16,N'Lighting',N'Table lamps and chandeliers.',3,NULL);
INSERT INTO #CategoryLookup VALUES (17,N'Running & Fitness',N'Running shoes and fitness equipment.',4,NULL);
INSERT INTO #CategoryLookup VALUES (18,N'Camping & Nature',N'Tents, mats and outdoor bags.',4,NULL);
INSERT INTO #CategoryLookup VALUES (19,N'Skin Care',N'Cleansers, serums and moisturizers.',5,NULL);
INSERT INTO #CategoryLookup VALUES (20,N'Hair Care',N'Shampoo, conditioner and serums.',5,NULL);
INSERT INTO #CategoryLookup VALUES (21,N'Baby Care',N'Diapers and care sets.',6,NULL);
INSERT INTO #CategoryLookup VALUES (22,N'Toys',N'Educational and fun toys.',6,NULL);
INSERT INTO #CategoryLookup VALUES (23,N'Fiction & Literature',N'Contemporary and classic novels.',7,NULL);
INSERT INTO #CategoryLookup VALUES (24,N'Stationery',N'Notebooks, pens and office supplies.',7,NULL);
INSERT INTO #CategoryLookup VALUES (25,N'Cookware',N'Pots, pans and kitchen sets.',8,NULL);

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

INSERT INTO #ProductLookup VALUES (1,N'{b} Wireless Bluetooth Headset Pro',16,9,1299,800,N'Black,White,Navy',NULL,N'Wireless headset with active noise cancellation.',N'<p>Long battery life, fast charging and comfortable fit. Ideal for daily commute and office use.</p>');
INSERT INTO #ProductLookup VALUES (2,N'{b} USB-C Fast Charger 65W',6,10,449,200,N'White,Black',NULL,N'Compact GaN fast charger.',N'<p>Charge your phone, tablet and laptop with one adapter. Overheat protection included.</p>');
INSERT INTO #ProductLookup VALUES (3,N'{b} Silicone Phone Case',6,10,149,80,N'Transparent,Black,Pink,Blue',NULL,N'Shock-resistant thin silicone case.',N'<p>Protects sensitive edges, compatible with wireless charging.</p>');
INSERT INTO #ProductLookup VALUES (4,N'{b} Women''s Cotton Basic T-Shirt',2,11,279,120,N'White,Black,Gray,Beige',N'XS,S,M,L,XL',N'Breathable cotton everyday t-shirt.',N'<p>100% cotton, soft texture. Machine washable.</p>');
INSERT INTO #ProductLookup VALUES (5,N'{b} Men''s Slim Fit Chino Pants',13,12,599,250,N'Beige,Navy,Khaki,Black',N'28,30,32,34,36',N'Slim fit chinos for office and casual.',N'<p>Stretch fabric, wrinkle-free. Seasonal collection.</p>');
INSERT INTO #ProductLookup VALUES (6,N'{b} Unisex Running Shoes AirFlex',17,13,1899,600,N'Black,Gray,Blue,Orange',N'36,37,38,39,40,41,42,43,44',N'Lightweight running shoes.',N'<p>Breathable mesh upper, shock-absorbing sole. Optimized for road running.</p>');
INSERT INTO #ProductLookup VALUES (7,N'{b} Women''s Trench Coat',13,11,1499,500,N'Beige,Black,Khaki',N'S,M,L,XL',N'Classic water-repellent trench coat.',N'<p>Ideal for seasonal transitions. Lined, belted cut.</p>');
INSERT INTO #ProductLookup VALUES (8,N'{b} Men''s Oxford Shirt',2,12,449,150,N'White,Light Blue,Pink',N'S,M,L,XL,XXL',N'Classic oxford shirt.',N'<p>For work and casual combinations. Easy-iron cotton blend.</p>');
INSERT INTO #ProductLookup VALUES (9,N'{b} Corner Sofa Set 3+1',1,14,24999,8000,N'Anthracite,Cream,Green',NULL,N'Spacious L-shaped corner sofa set.',N'<p>High-density foam, removable covers. Adds spacious look to your living room.</p>');
INSERT INTO #ProductLookup VALUES (10,N'{b} Oak Coffee Table 90cm',1,14,3299,1000,N'Natural Oak,Walnut',NULL,N'Solid wood look coffee table.',N'<p>Durable surface, easy to clean. Minimal Scandinavian lines.</p>');
INSERT INTO #ProductLookup VALUES (11,N'{b} Cotton Sateen Duvet Set',7,15,899,400,N'White,Gray,Powder',N'Single,Double,King',N'200 TC cotton sateen duvet.',N'<p>Soft texture, colorfast. Pillowcase included.</p>');
INSERT INTO #ProductLookup VALUES (12,N'{b} LED Desk Lamp with Dimmer',18,16,649,250,N'Black,White,Brass',NULL,N'Touch dimmable LED desk lamp.',N'<p>Three color temperatures, eye-friendly light. USB charging port.</p>');
INSERT INTO #ProductLookup VALUES (13,N'{b} Yoga Mat 6mm',17,17,349,150,N'Purple,Blue,Pink,Black',NULL,N'Non-slip yoga and pilates mat.',N'<p>Carrying strap included. Latex-free, easy to wipe.</p>');
INSERT INTO #ProductLookup VALUES (14,N'{b} Dumbbell Set 2x5kg',3,17,429,200,N'Black',NULL,N'Neoprene coated dumbbell pair.',N'<p>Non-slip grip for home workouts. Floor protectors.</p>');
INSERT INTO #ProductLookup VALUES (15,N'{b} 2-Person Camping Tent',9,18,2199,700,N'Green,Orange',NULL,N'Quick-setup waterproof tent.',N'<p>2000mm waterproof fabric, mosquito net. Carrying bag included.</p>');
INSERT INTO #ProductLookup VALUES (16,N'{b} Trekking Backpack 40L',9,18,1599,500,N'Anthracite,Khaki',NULL,N'Trekking backpack with waist and chest straps.',N'<p>Rain cover, hydration compatible. Breathable back panel.</p>');
INSERT INTO #ProductLookup VALUES (17,N'{b} Vitamin C Brightening Serum 30ml',5,19,389,150,NULL,NULL,N'Anti-spot vitamin C serum.',N'<p>For morning routine. Use with SPF. Dermatologically tested.</p>');
INSERT INTO #ProductLookup VALUES (18,N'{b} Moisturizing Face Cream 50ml',5,19,299,120,NULL,NULL,N'24-hour moisturizing face cream.',N'<p>Light formula for oily and combination skin. Paraben-free.</p>');
INSERT INTO #ProductLookup VALUES (19,N'{b} Repair Shampoo 400ml',5,20,189,80,NULL,NULL,N'Repair shampoo for damaged hair.',N'<p>Keratin and argan oil complex. Suitable for daily use.</p>');
INSERT INTO #ProductLookup VALUES (20,N'{b} Baby Care Set 5-Piece',8,21,449,150,NULL,NULL,N'Baby care set for sensitive skin.',N'<p>Shampoo, lotion, oil, cream and wipes. Hypoallergenic.</p>');
INSERT INTO #ProductLookup VALUES (21,N'{b} Educational Wooden Blocks 48-Piece',8,22,329,100,N'Colorful',NULL,N'48-piece wooden block set.',N'<p>Water-based paint, no sharp edges. Suitable for 3+ years.</p>');
INSERT INTO #ProductLookup VALUES (22,N'{b} Contemporary Novel - Selection #{n}',12,23,149,80,NULL,NULL,N'Curated contemporary literature.',N'<p>Hardcover, local print. Top rated by readers.</p>');
INSERT INTO #ProductLookup VALUES (23,N'{b} Hardcover Notebook A5',12,24,89,40,N'Kraft,Black,Navy',NULL,N'Dotted A5 notebook.',N'<p>120 pages, 90gsm. Elastic band and bookmark.</p>');
INSERT INTO #ProductLookup VALUES (24,N'{b} Granite Pan 28cm',14,25,549,200,N'Black',NULL,N'Non-stick granite coated pan.',N'<p>Induction compatible. Oven-safe handle. PFOA-free.</p>');
INSERT INTO #ProductLookup VALUES (25,N'{b} Stainless Steel Pot Set 6-Piece',4,25,2499,800,N'Steel',NULL,N'Stainless steel pot set.',N'<p>3 pots with lids. Dishwasher safe.</p>');
INSERT INTO #ProductLookup VALUES (26,N'{b} Glass Water Bottle 750ml',11,8,249,80,N'Transparent,Smoked,Blue',NULL,N'BPA-free glass water bottle.',N'<p>Silicone sleeve, leak-proof cap. For office and sports.</p>');
INSERT INTO #ProductLookup VALUES (27,N'{b} Organic Olive Oil 1L',15,8,329,100,NULL,NULL,N'Cold-pressed organic olive oil.',N'<p>Single harvest, dark glass bottle. Tasting notes on label.</p>');
INSERT INTO #ProductLookup VALUES (28,N'{b} Cabin Suitcase 55cm',20,2,1899,600,N'Black,Navy,Burgundy',NULL,N'Lightweight hard-shell cabin suitcase.',N'<p>360° wheels, TSA lock. Interior organizer compartments.</p>');
INSERT INTO #ProductLookup VALUES (29,N'{b} Leather Shoulder Bag',10,2,1299,400,N'Tan,Black,Burgundy',NULL,N'Handcrafted look leather shoulder bag.',N'<p>Adjustable strap, zip inner pocket. Daily use.</p>');
INSERT INTO #ProductLookup VALUES (30,N'{b} Pet Food Bowl Set',19,3,199,80,N'Gray,Pink,Blue',NULL,N'Stainless steel pet bowl set.',N'<p>Non-slip base, dishwasher safe.</p>');
INSERT INTO #ProductLookup VALUES (31,N'{b} Smart LED Bulb 9W',18,16,179,60,N'White',NULL,N'App-controlled color changing bulb.',N'<p>Voice assistant compatible. Timer and scene support.</p>');
INSERT INTO #ProductLookup VALUES (32,N'{b} Thermos Mug 350ml',4,8,279,100,N'Black,White,Red',NULL,N'Stainless steel vacuum thermos mug.',N'<p>Keeps hot 6h / cold 12h. Fits car holder.</p>');
INSERT INTO #ProductLookup VALUES (33,N'{b} High-Waist Fitness Leggings',17,17,449,150,N'Black,Navy,Burgundy',N'XS,S,M,L',N'Shaping high-waist sports leggings.',N'<p>Moisture-wicking fabric, pocket detail. For training and daily use.</p>');
INSERT INTO #ProductLookup VALUES (34,N'{b} Men''s Fleece Jacket',3,12,899,300,N'Anthracite,Navy,Khaki',N'S,M,L,XL,XXL',N'Lightweight fleece zip jacket.',N'<p>As a cold weather layer or standalone.</p>');
INSERT INTO #ProductLookup VALUES (35,N'{b} Bamboo Cutting Board Set',14,25,349,120,N'Natural',NULL,N'3-piece bamboo cutting board set.',N'<p>Antibacterial natural surface. Hanging hole.</p>');
INSERT INTO #ProductLookup VALUES (36,N'{b} Sunscreen SPF50 50ml',5,19,259,80,NULL,NULL,N'Light texture sunscreen for face.',N'<p>Leaves no white cast. Suitable under makeup.</p>');
INSERT INTO #ProductLookup VALUES (37,N'{b} Baby Bodysuit 3-Pack',8,21,249,80,N'White,Gray,Yellow',N'0-3M,3-6M,6-9M,9-12M',N'Organic cotton baby bodysuit set.',N'<p>Snap buttons, tagless. For sensitive skin.</p>');
INSERT INTO #ProductLookup VALUES (38,N'{b} Mini Bluetooth Speaker',16,9,799,300,N'Black,Blue,Red',NULL,N'Portable waterproof speaker.',N'<p>12h battery, IPX7. Stereo pairing.</p>');
INSERT INTO #ProductLookup VALUES (39,N'{b} Aluminum Laptop Stand',6,1,549,200,N'Silver,Space Gray',NULL,N'Adjustable aluminum laptop stand.',N'<p>Ergonomic angle, cable management. 10-16 inch compatible.</p>');
INSERT INTO #ProductLookup VALUES (40,N'{b} Memory Foam Pillow 2-Pack',7,15,699,250,N'White',N'Standard',N'Visco memory foam pillow pair.',N'<p>Neck support, removable cover. Anti-allergic.</p>');

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

INSERT INTO #TagLookup VALUES (1,N'Daily use');
INSERT INTO #TagLookup VALUES (2,N'Office');
INSERT INTO #TagLookup VALUES (3,N'Sports');
INSERT INTO #TagLookup VALUES (4,N'Travel');
INSERT INTO #TagLookup VALUES (5,N'Gift idea');
INSERT INTO #TagLookup VALUES (6,N'Cotton');
INSERT INTO #TagLookup VALUES (7,N'Leather');
INSERT INTO #TagLookup VALUES (8,N'Polyester');
INSERT INTO #TagLookup VALUES (9,N'Organic');
INSERT INTO #TagLookup VALUES (10,N'Metal');
INSERT INTO #TagLookup VALUES (11,N'Summer');
INSERT INTO #TagLookup VALUES (12,N'Winter');
INSERT INTO #TagLookup VALUES (13,N'Spring');
INSERT INTO #TagLookup VALUES (14,N'Autumn');
INSERT INTO #TagLookup VALUES (15,N'Seasonal');
INSERT INTO #TagLookup VALUES (16,N'Women');
INSERT INTO #TagLookup VALUES (17,N'Men');
INSERT INTO #TagLookup VALUES (18,N'Unisex');
INSERT INTO #TagLookup VALUES (19,N'Children');
INSERT INTO #TagLookup VALUES (20,N'Baby');
INSERT INTO #TagLookup VALUES (21,N'Waterproof');
INSERT INTO #TagLookup VALUES (22,N'Breathable');
INSERT INTO #TagLookup VALUES (23,N'Fast shipping');
INSERT INTO #TagLookup VALUES (24,N'On sale');
INSERT INTO #TagLookup VALUES (25,N'New season');
INSERT INTO #TagLookup VALUES (26,N'Minimal');
INSERT INTO #TagLookup VALUES (27,N'Classic');
INSERT INTO #TagLookup VALUES (28,N'Modern');
INSERT INTO #TagLookup VALUES (29,N'Vintage');
INSERT INTO #TagLookup VALUES (30,N'Scandinavian');
INSERT INTO #TagLookup VALUES (31,N'Campaign');
INSERT INTO #TagLookup VALUES (32,N'Bestseller');
INSERT INTO #TagLookup VALUES (33,N'Editor''s choice');
INSERT INTO #TagLookup VALUES (34,N'Limited stock');
INSERT INTO #TagLookup VALUES (35,N'Local production');
INSERT INTO #TagLookup VALUES (36,N'Eco-friendly');
INSERT INTO #TagLookup VALUES (37,N'BPA free');
INSERT INTO #TagLookup VALUES (38,N'Machine washable');
INSERT INTO #TagLookup VALUES (39,N'Induction compatible');
INSERT INTO #TagLookup VALUES (40,N'TSA lock');

IF OBJECT_ID(N'tempdb..#StoryCatLookup') IS NOT NULL DROP TABLE #StoryCatLookup;
CREATE TABLE #StoryCatLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, Description NVARCHAR(400) NOT NULL, PageTheme NVARCHAR(10) NOT NULL);

INSERT INTO #StoryCatLookup VALUES (1,N'Style Guide',N'Fashion and style tips.',N'T1');
INSERT INTO #StoryCatLookup VALUES (2,N'Home Decor',N'Inspiration for living spaces.',N'T2');
INSERT INTO #StoryCatLookup VALUES (3,N'Healthy Living',N'Nutrition and wellness articles.',N'T3');
INSERT INTO #StoryCatLookup VALUES (4,N'Technology',N'Gadget reviews and tips.',N'T4');
INSERT INTO #StoryCatLookup VALUES (5,N'Travel',N'Route suggestions and packing guides.',N'T5');
INSERT INTO #StoryCatLookup VALUES (6,N'Parenting',N'Baby and child care.',N'T6');

IF OBJECT_ID(N'tempdb..#StoryLookup') IS NOT NULL DROP TABLE #StoryLookup;
CREATE TABLE #StoryLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(200) NOT NULL, ShortDescription NVARCHAR(400) NOT NULL, AuthorName NVARCHAR(100) NOT NULL, BodyHtml NVARCHAR(MAX) NOT NULL);

INSERT INTO #StoryLookup VALUES (1,N'2024 Autumn Outfit Ideas',N'Layered autumn styles.',N'Selin Arslan',N'<p>How to combine trench coats, fleece and chino pants for seasonal transitions? Our editors'' favorite combinations.</p>');
INSERT INTO #StoryLookup VALUES (2,N'Furniture Tips for Small Living Rooms',N'Spaciousness in tight spaces.',N'Can Yilmaz',N'<p>Make your living room feel larger with the right coffee table and lighting instead of a corner sofa. Examples from Nordline collection.</p>');
INSERT INTO #StoryLookup VALUES (3,N'How to Choose Running Shoes?',N'Pronation, sole and fit.',N'Emre Demir',N'<p>Short guide to choosing the right running shoes based on your weekly mileage and foot type.</p>');
INSERT INTO #StoryLookup VALUES (4,N'Skincare Routine: 5 Steps',N'From cleansing to moisturizing.',N'Elif Kaya',N'<p>A simple yet effective routine for morning and evening. Why order of serum and sunscreen matters?</p>');
INSERT INTO #StoryLookup VALUES (5,N'Before Your Camping Holiday',N'Checklist.',N'Burak Sahin',N'<p>Tent, mat, backpack and kitchen set: what not to forget for your first camping trip.</p>');
INSERT INTO #StoryLookup VALUES (6,N'Using Granite Pans in the Kitchen',N'Care and cooking tips.',N'Ayse Celik',N'<p>Don''t use metal spatulas to extend non-stick life. Correct heat settings.</p>');
INSERT INTO #StoryLookup VALUES (7,N'Baby Room Preparation List',N'Essentials for first 3 months.',N'Zeynep Ozturk',N'<p>From bodysuit sets to care products, a realistic shopping list.</p>');
INSERT INTO #StoryLookup VALUES (8,N'When Buying Bluetooth Headphones',N'ANC, battery and compatibility.',N'Kerem Aydin',N'<p>Does active noise cancellation really work? Office and travel scenarios.</p>');
INSERT INTO #StoryLookup VALUES (9,N'Reading Organic Product Labels',N'What do certifications mean?',N'Defne Kara',N'<p>Correctly interpret organic, cold-pressed and additive statements.</p>');
INSERT INTO #StoryLookup VALUES (10,N'Home Office Lighting',N'Reduce eye strain.',N'Onur Yildiz',N'<p>Practical tips on desk lamp color, brightness and screen position.</p>');
INSERT INTO #StoryLookup VALUES (11,N'What to Consider When Choosing Luggage',N'Cabin rules and wheels.',N'Melis Dogan',N'<p>Airline cabin dimensions, TSA lock and weight balance.</p>');
INSERT INTO #StoryLookup VALUES (12,N'Fit Check When Buying Sports Tights',N'High waist and fabric.',N'Irem Kilic',N'<p>How to tell if sports leggings are non-slip and moisture-wicking?</p>');
INSERT INTO #StoryLookup VALUES (13,N'A Small Corner with Books',N'Creating a reading space.',N'Cem Polat',N'<p>Mini library at home with shelves, lighting and a cozy armchair.</p>');
INSERT INTO #StoryLookup VALUES (14,N'Layering Winter Jackets',N'Fleece + outer layer.',N'Hakan Kurt',N'<p>How to stay warm with breathable layers in cold weather.</p>');
INSERT INTO #StoryLookup VALUES (15,N'Pet-Friendly Home Layout',N'Feeding area and safety.',N'Buse Aksoy',N'<p>Safe space for your pet with non-slip bowls and cable management.</p>');

IF OBJECT_ID(N'tempdb..#MenuLookup') IS NOT NULL DROP TABLE #MenuLookup;
CREATE TABLE #MenuLookup
(
    rn INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    MenuLink NVARCHAR(100) NOT NULL,
    ExternalLink NVARCHAR(200) NULL,
    MainPage BIT NOT NULL,
    PageTheme NVARCHAR(10) NULL,
    GalleryCount INT NOT NULL
);

INSERT INTO #MenuLookup VALUES (1,N'Home',N'<p>Welcome to EImece showcase.</p>',N'home-index',NULL,1,NULL,0);
INSERT INTO #MenuLookup VALUES (2,N'Corporate',N'<p>About us and company information.</p>',N'pages-index',NULL,1,N'T1',0);
INSERT INTO #MenuLookup VALUES (3,N'About Us',N'<p>EImece is an online store bringing selected brands together under one roof.</p>',N'info-aboutus',NULL,0,NULL,0);
INSERT INTO #MenuLookup VALUES (4,N'Contact',N'<h2>Contact</h2><p>Customer service and store contact information.</p><p>For orders, returns and product questions, use the form below or write to <strong>info@eimece.test</strong>.</p><p>Working hours: Weekdays 09:00–18:00</p>',N'pages-index',NULL,0,N'T8',0);
INSERT INTO #MenuLookup VALUES (5,N'Shipping & Delivery',N'<p>Shipping times, free shipping limit and return process.</p>',N'info-deliveryinfo',NULL,1,NULL,0);
INSERT INTO #MenuLookup VALUES (6,N'FAQ',N'<p>FAQ about orders, payment and returns.</p>',N'pages-index',NULL,1,N'T2',0);
INSERT INTO #MenuLookup VALUES (7,N'Campaigns',N'<p>Current discounts and coupons.</p>',N'pages-index',NULL,1,N'T1',0);
INSERT INTO #MenuLookup VALUES (8,N'Blog',N'<p>Style, life and product guides.</p>',N'stories-index',NULL,1,NULL,0);
INSERT INTO #MenuLookup VALUES (9,N'Privacy Policy',N'<p>Protection of personal data.</p>',N'info-privacypolicy',NULL,0,NULL,0);
INSERT INTO #MenuLookup VALUES (10,N'Distance Sales Agreement',N'<p>Distance sales and consumer rights.</p>',N'info-termsandconditions',NULL,0,NULL,0);
INSERT INTO #MenuLookup VALUES (11,N'Returns & Exchange',N'<p>Return and exchange conditions within 14 days.</p>',N'pages-index',NULL,0,N'T3',0);
INSERT INTO #MenuLookup VALUES (12,N'Our Stores',N'<p>Our store locations will be listed here soon.</p><p><em>Admin note:</em> Add the real map link via Admin → Menus.</p>',N'pages-index',NULL,1,N'T4',0);
INSERT INTO #MenuLookup VALUES (13,N'Theme Examples',N'<p>Page theme examples (T1–T8). Each subpage has a main image and gallery.</p>',N'pages-index',NULL,1,N'T1',0);
INSERT INTO #MenuLookup VALUES (14,N'PT Dummy T1',N'<p>This page shows <strong>PageTheme T1</strong> layout. The large image above is the menu main image. The grid below is the menu gallery.</p>',N'pages-index',NULL,0,N'T1',12);
INSERT INTO #MenuLookup VALUES (15,N'PT Dummy T2',N'<p>This page shows <strong>PageTheme T2</strong> layout.</p>',N'pages-index',NULL,0,N'T2',12);
INSERT INTO #MenuLookup VALUES (16,N'PT Dummy T3',N'<p>This page shows <strong>PageTheme T3</strong> layout.</p>',N'pages-index',NULL,0,N'T3',12);
INSERT INTO #MenuLookup VALUES (17,N'PT Dummy T4',N'<p>This page shows <strong>PageTheme T4</strong> layout.</p>',N'pages-index',NULL,0,N'T4',12);
INSERT INTO #MenuLookup VALUES (18,N'PT Dummy T5',N'<p>This page shows <strong>PageTheme T5</strong> layout.</p>',N'pages-index',NULL,0,N'T5',12);
INSERT INTO #MenuLookup VALUES (19,N'PT Dummy T6',N'<p>This page shows <strong>PageTheme T6</strong> layout.</p>',N'pages-index',NULL,0,N'T6',12);
INSERT INTO #MenuLookup VALUES (20,N'PT Dummy T7',N'<p>This page shows <strong>PageTheme T7</strong> (large gallery) layout. At least 12 menu gallery images are added.</p>',N'pages-index',NULL,0,N'T7',12);
INSERT INTO #MenuLookup VALUES (21,N'PT Dummy T8',N'<h2>Contact</h2><p>This page shows <strong>PageTheme T8</strong> contact layout.</p>',N'pages-index',NULL,0,N'T8',12);

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
CREATE TABLE #TemplateLookup (rn INT NOT NULL PRIMARY KEY, Name NVARCHAR(100) NOT NULL, TemplateXml NVARCHAR(MAX) NOT NULL);

INSERT INTO #TemplateLookup VALUES (1,N'Giyim Şablonu',N'<!--eimece-seed-->
<component>
  <group name="Ürün Özellikleri">
    <textbox name="Renk" />
    <textbox name="Beden" />
    <textbox name="Malzeme" display="Kumaş / Malzeme" />
  </group>
</component>');
INSERT INTO #TemplateLookup VALUES (2,N'Elektronik Şablonu',N'<!--eimece-seed-->
<component>
  <group name="Ürün Özellikleri">
    <textbox name="Renk" />
    <textbox name="Marka" />
    <textbox name="Model" />
    <textbox name="Garanti" unit="ay" />
    <textbox name="Ağırlık" unit="kg" />
  </group>
</component>');
INSERT INTO #TemplateLookup VALUES (3,N'Ev & Mobilya Şablonu',N'<!--eimece-seed-->
<component>
  <group name="Ürün Özellikleri">
    <textbox name="Renk" />
    <textbox name="Malzeme" />
    <textbox name="Yükseklik" unit="cm" />
    <textbox name="Genişlik" unit="cm" />
    <textbox name="Derinlik" unit="cm" />
    <textbox name="Ağırlık" unit="kg" />
  </group>
</component>');
INSERT INTO #TemplateLookup VALUES (4,N'Kozmetik Şablonu',N'<!--eimece-seed-->
<component>
  <group name="Ürün Özellikleri">
    <textbox name="Renk" />
    <textbox name="Hacim" unit="ml" />
    <textbox name="Cilt Tipi" />
    <textbox name="Paket Adeti" />
  </group>
</component>');
INSERT INTO #TemplateLookup VALUES (5,N'Spor Ekipman Şablonu',N'<!--eimece-seed-->
<component>
  <group name="Ürün Özellikleri">
    <textbox name="Renk" />
    <textbox name="Beden" />
    <textbox name="Malzeme" />
    <textbox name="Ağırlık" unit="kg" />
  </group>
</component>');
INSERT INTO #TemplateLookup VALUES (6,N'Genel Ürün Şablonu',N'<!--eimece-seed-->
<component>
  <group name="Ürün Özellikleri 1">
    <dropdown name="Renk" values="Renkler" />
    <textbox name="Malzeme" />
    <textbox name="Ağırlık" unit="kg" />
  </group>
  <group name="Ürün Özellikleri 2">
    <textbox name="Paket Adeti" unit="tane" />
    <textbox name="Koli Adeti" display="Koli Adeti" unit="tane" />
    <checkbox name="Depoda Var mi?" />
  </group>
</component>');

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
   2) FileStorages — exclusive pool sized to @SeedFiles
      Each MainImage / gallery reference later takes a unique row.
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
    /* FileUrl keeps /media/seed/ marker for cleanup; physical files live under ~/media/images/{FileName} */
    N'/media/seed/images/product-' + RIGHT(N'00000' + CAST(n.n AS NVARCHAR(5)), 5) + N'.jpg',
    N'image/jpeg',
    85000 + (n.n % 400000),
    1200, 900,
    N'image',
    1
FROM #Nums n
WHERE n.n <= @SeedFiles;

DECLARE @MinFileId INT = (SELECT MIN(Id) FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%');
DECLARE @MaxFileId INT = (SELECT MAX(Id) FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%');
DECLARE @FileCount INT = ISNULL(@MaxFileId - @MinFileId + 1, 0);

/* Exclusive offset ranges into the seed FileStorages block (0-based). */
DECLARE @FsOffBrand     INT = 0;
DECLARE @FsOffProdCat   INT = @FsOffBrand + @SeedBrands;
DECLARE @FsOffProduct   INT = @FsOffProdCat + @SeedProductCategories;
DECLARE @FsOffProdFile  INT = @FsOffProduct + @SeedProducts;
DECLARE @FsOffStoryCat  INT = @FsOffProdFile + @SeedProductFiles;
DECLARE @FsOffStory     INT = @FsOffStoryCat + @SeedStoryCategories;
DECLARE @FsOffStoryFile INT = @FsOffStory + @SeedStories;
DECLARE @FsOffMenu      INT = @FsOffStoryFile + @SeedStoryFiles;
DECLARE @FsOffMenuFile  INT = @FsOffMenu + @SeedMenus;
DECLARE @FsOffSlide     INT = @FsOffMenuFile + @SeedMenuFiles;
DECLARE @FsRequired     INT = @FsOffSlide + @SeedMainPageImages;

IF @MinFileId IS NULL OR @FileCount < @FsRequired
BEGIN
    RAISERROR(N'Seed FileStorages were not created with enough exclusive slots. Expected at least %d rows.', 16, 1, @FsRequired);
    RETURN;
END;

PRINT N'FileStorage exclusive ranges ready. MinId=' + CAST(@MinFileId AS VARCHAR(20))
    + N', Count=' + CAST(@FileCount AS VARCHAR(20))
    + N', Required=' + CAST(@FsRequired AS VARCHAR(20));

UPDATE dbo.FileStorages
SET Type = N'MenuMainImage'
WHERE Id >= @MinFileId + @FsOffMenu
  AND Id <  @MinFileId + @FsOffMenuFile
  AND FileUrl LIKE N'/media/seed/%';

UPDATE dbo.FileStorages
SET Type = N'MenuGallery'
WHERE Id >= @MinFileId + @FsOffMenuFile
  AND Id <  @MinFileId + @FsOffSlide
  AND FileUrl LIKE N'/media/seed/%';


/* ============================================================
   3) Templates
   ============================================================ */
PRINT N'Seeding Templates...';
INSERT INTO dbo.Templates (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, TemplateXml)
SELECT
    tl.Name,
    @Now, @Now, 1, n.n, @Lang,
    tl.TemplateXml
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
    @MinFileId + @FsOffBrand + (n.n - 1),
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
    @MinFileId + @FsOffProdCat + (n.n - 1),
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
        1 + (ABS(CHECKSUM(CONCAT(N'SEED-POS-', CAST(pr.rn AS NVARCHAR(20))))) % 10000), @Lang,
        pr.DescriptionHtml,
        1, N'ürün,e-ticaret,' + LOWER(LEFT(pr.ProductName, 40)),
        @MinFileId + @FsOffProduct + (pr.rn - 1),
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
        1 + (ABS(CHECKSUM(CONCAT(N'SEED-POS-', CAST(pr.rn AS NVARCHAR(20))))) % 10000), @Lang,
        pr.DescriptionHtml,
        1, N'ürün,e-ticaret,' + LOWER(LEFT(pr.ProductName, 40)),
        @MinFileId + @FsOffProduct + (pr.rn - 1),
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
;WITH FirstBrandProducts AS (
    SELECT p.Id
    FROM dbo.Products p
    INNER JOIN (
        SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS rn
        FROM dbo.Products
        WHERE AddUserId = @SeedMarker
    ) pr ON pr.Id = p.Id
    WHERE pr.rn <= @BrandCount
)
UPDATE p
SET Name = REPLACE(p.Name, LEFT(p.Name, CHARINDEX(N' ', p.Name + N' ') - 1), b.Name),
    NameLong = REPLACE(p.NameLong, LEFT(p.NameLong, CHARINDEX(N' ', p.NameLong + N' ') - 1), b.Name),
    NameShort = LEFT(REPLACE(p.Name, LEFT(p.Name, CHARINDEX(N' ', p.Name + N' ') - 1), b.Name), 60)
FROM dbo.Products p
INNER JOIN FirstBrandProducts fbp ON fbp.Id = p.Id
INNER JOIN dbo.Brands b ON b.Id = p.BrandId
WHERE p.AddUserId = @SeedMarker
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
    @MinFileId + @FsOffProdFile + (n.n - 1),
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
    @MinFileId + @FsOffStoryCat + (n.n - 1),
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
    @MinFileId + @FsOffStory + (n.n - 1),
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
    @MinFileId + @FsOffStoryFile + (n.n - 1)
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
    @MinFileId + @FsOffMenu + (n.n - 1),
    @AdminUserId, @SeedMarker,
    0,
    ml.MainPage,
    ml.MenuLink,
    ml.ExternalLink,
    ml.PageTheme,
    CASE WHEN ml.ExternalLink IS NOT NULL THEN 1 ELSE 0 END
FROM #Nums n
INNER JOIN #MenuLookup ml ON ml.rn = ((n.n - 1) % @MenuLookupCount) + 1
WHERE n.n <= @SeedMenus;

DECLARE @MinMenuId INT = (SELECT MIN(Id) FROM dbo.Menus WHERE AddUserId = @SeedMarker);
DECLARE @MenuCount INT = (SELECT COUNT(*) FROM dbo.Menus WHERE AddUserId = @SeedMarker);

/* Menu tree by Position:
   1,2,5,6,7,8,12,13 = roots (13 = Tema Ornekleri)
   3,4,9,10,11 = children of position 2 (Kurumsal)
   14–21 = PT Dummy T1–T8 children of position 13 */
UPDATE m
SET ParentId = CASE
    WHEN m.Position IN (1, 2, 5, 6, 7, 8, 12, 13) THEN 0
    WHEN m.Position IN (3, 4, 9, 10, 11)
        THEN (SELECT TOP 1 Id FROM dbo.Menus WHERE AddUserId = @SeedMarker AND Position = 2 AND Lang = @Lang)
    WHEN m.Position BETWEEN 14 AND 21
        THEN (SELECT TOP 1 Id FROM dbo.Menus WHERE AddUserId = @SeedMarker AND Position = 13 AND Lang = @Lang)
    ELSE 0
END
FROM dbo.Menus m
WHERE m.AddUserId = @SeedMarker AND m.Lang = @Lang;

/* MenuGallery files: 12 per PT Dummy T1–T8 (Admin media imageType=MenuGallery). */
INSERT INTO dbo.MenuFiles
    (Name, CreatedDate, UpdatedDate, IsActive, Position, Lang, MenuId, FileStorageId)
SELECT
    N'SEED Galeri — ' + LEFT(tm.Name, 60) + N' #' + CAST(g.n AS NVARCHAR(10)),
    @Now, @Now, 1, g.n, @Lang,
    tm.MenuId,
    @MinFileId + @FsOffMenuFile + ((tm.ThemeRn - 1) * 12) + (g.n - 1)
FROM (
    SELECT
        m.Id AS MenuId,
        m.Name,
        ROW_NUMBER() OVER (ORDER BY m.Position, m.Id) AS ThemeRn
    FROM dbo.Menus m
    WHERE m.AddUserId = @SeedMarker
      AND m.Name LIKE N'PT Dummy T%'
) tm
CROSS JOIN #Nums g
WHERE g.n <= 12;

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
    @MinFileId + @FsOffSlide + (n.n - 1),
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
        -- Description=SystemSettings for keys bound by SystemSettingModel
        (N'CompanyName', N'EImece', N'SystemSettings'),
        (N'CompanyAddress', N'Caferağa Mah. Moda Cad. No:42 Kadıköy / İstanbul', N'SystemSettings'),
        (N'WebSiteLogo', N'/images/logo.jpg', N'Logo yolu'),
        (N'WebSiteCompanyEmailAddress', N'info@eimece.test', N'SystemSettings'),
        (N'WebSiteCompanyPhoneAndLocation', N'+90 216 555 01 23 | İstanbul', N'SystemSettings'),
        (N'CargoCompany', N'Yurtiçi Kargo', N'Kargo firması'),
        (N'CargoPrice', N'49.90', N'Kargo ücreti'),
        (N'BasketMinTotalPriceForCargo', N'500', N'SystemSettings'),
        (N'CargoDescription', N'Standart kargo 2-4 iş günü', N'Kargo açıklaması'),
        (N'SiteIndexMetaTitle', N'EImece — Seçili Markalar, Tek Mağaza', N'Meta başlık'),
        (N'SiteIndexMetaDescription', N'Moda, ev, spor ve elektronik ürünlerinde seçili markalar. Hızlı kargo, güvenli ödeme.', N'Meta açıklama'),
        (N'SiteIndexMetaKeywords', N'eimece,online mağaza,moda,ev,spor', N'Meta anahtar kelimeler'),
        (N'IsProductPriceEnable', N'true', N'SystemSettings'),
        (N'IsProductReviewEnable', N'true', N'SystemSettings'),
        (N'AdminEmail', N'admin@eimece.test', N'SystemSettings'),
        (N'AdminUserName', N'admin@eimece.test', N'SystemSettings'),
        (N'AdminEmailHost', N'smtp.eimece.test', N'SystemSettings'),
        (N'AdminEmailPassword', N'seed-smtp-placeholder', N'SystemSettings'),
        (N'AdminEmailPort', N'587', N'SystemSettings'),
        (N'AdminEmailEnableSsl', N'true', N'SystemSettings'),
        (N'AdminEmailUseDefaultCredentials', N'false', N'SystemSettings'),
        (N'AdminEmailDisplayName', N'EImece Müşteri Hizmetleri', N'SystemSettings'),
        (N'DefaultImageWidth', N'1200', N'SystemSettings'),
        (N'DefaultImageHeight', N'900', N'SystemSettings'),
        (N'FooterDescription', N'EImece — seçili markalar, özenli teslimat.', N'Footer metin'),
        (N'FooterHtmlDescription', N'<p>© EImece. Tüm hakları saklıdır.</p>', N'Footer HTML'),
        (N'FooterEmailListDescription', N'Kampanya ve yeniliklerden haberdar olmak için abone olun.', N'Bülten metni'),
        (N'AboutUs', N'<p>EImece, moda, ev ve yaşam kategorilerinde seçili markaları bir araya getiren online mağazadır.</p>', N'Hakkımızda'),
        (N'PrivacyPolicy', N'<p>Kişisel verileriniz KVKK kapsamında işlenir ve üçüncü taraflarla paylaşılmaz.</p>', N'Gizlilik'),
        (N'TermsAndConditions', N'<p>Sitedeki alışverişler mesafeli satış sözleşmesine tabidir.</p>', N'Şartlar'),
        (N'DeliveryInfo', N'<p>Siparişler ortalama 2-4 iş gününde kargoya verilir. 500 TL üzeri ücretsiz kargo.</p>', N'Teslimat'),
        (N'FacebookWebSiteLink', N'https://facebook.com/eimece', N'SystemSettings'),
        (N'InstagramWebSiteLink', N'https://instagram.com/eimece', N'SystemSettings'),
        (N'TwitterWebSiteLink', N'https://twitter.com/eimece', N'SystemSettings'),
        (N'LinkedinWebSiteLink', N'https://linkedin.com/company/eimece', N'SystemSettings'),
        -- Canonical YouTube SettingKey keeps historical typo (matches Constants.YotubeWebSiteLink).
        (N'YotubeWebSiteLink', N'https://youtube.com/@eimece', N'SystemSettings'),
        (N'PinterestWebSiteLink', N'https://pinterest.com/eimece', N'SystemSettings'),
        (N'WhatsAppCommunicationLink', N'https://wa.me/905555550123', N'SystemSettings')
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

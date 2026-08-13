/*
================================================================================
  EImece — Cleanup Dummy / Seed Data
================================================================================
  Removes rows created by SeedDummyData.sql.

  Cleanup markers (visible Name fields are realistic; markers are technical):
    - AddUserId = N'SEED'
    - FileUrl LIKE N'/media/seed/%'
    - TemplateXml LIKE N'%<!--eimece-seed-->%'
    - Email / UserName @eimece.test / seed*
    - Coupon Code LIKE N'EIMC-%'
    - OrderNumber LIKE N'EIMC-%'
    - Position in 900000..999999 for tags/lists/addresses/browser rows
    - SettingKey LIKE N'SEED_%' or N'__EIMECE_SEED__' / Name LIKE N'Demo: %'
    - Legacy: Name LIKE N'SEED %' (older seed script)

  Run in SSMS against your EImece database BEFORE re-seeding, or standalone
  to wipe test data.

  WARNING: This deletes ALL rows matching the seed markers. Do not run on
  production unless you are sure those markers are only used for test data.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

PRINT 'Cleaning seed dummy data...';

/* Child / junction tables first */
IF OBJECT_ID(N'dbo.BrowserNotificationFeedBacks', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserNotificationFeedBacks WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.BrowserNotifications', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserNotifications WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.BrowserSubscribers', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserSubscribers WHERE Position >= 900000 AND Position < 1000000 OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.BrowserSubscriptions', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserSubscriptions
    WHERE Position >= 900000 AND Position < 910000
       OR Subject = N'mailto:admin@eimece.test'
       OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.OrderProducts', N'U') IS NOT NULL
    DELETE op
    FROM dbo.OrderProducts op
    INNER JOIN dbo.Orders o ON o.Id = op.OrderId
    WHERE o.OrderNumber LIKE N'EIMC-%' OR o.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
    DELETE FROM dbo.Orders WHERE OrderNumber LIKE N'EIMC-%' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ShoppingCarts', N'U') IS NOT NULL
    DELETE FROM dbo.ShoppingCarts
    WHERE UserId LIKE N'seed%'
       OR Name LIKE N'SEED %'
       OR UserId IN (N'seed-admin-000000000001', N'seed-editor-00000000001', N'seed-customer-0000000001');

IF OBJECT_ID(N'dbo.ProductComments', N'U') IS NOT NULL
    DELETE FROM dbo.ProductComments WHERE Email LIKE N'%@eimece.test' OR UserId LIKE N'seed%' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductSpecifications', N'U') IS NOT NULL
    DELETE ps
    FROM dbo.ProductSpecifications ps
    INNER JOIN dbo.Products p ON p.Id = ps.ProductId
    WHERE p.AddUserId = N'SEED' OR p.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductTags', N'U') IS NOT NULL
    DELETE pt
    FROM dbo.ProductTags pt
    INNER JOIN dbo.Products p ON p.Id = pt.ProductId
    WHERE p.AddUserId = N'SEED' OR p.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductFiles', N'U') IS NOT NULL
    DELETE pf
    FROM dbo.ProductFiles pf
    INNER JOIN dbo.Products p ON p.Id = pf.ProductId
    WHERE p.AddUserId = N'SEED' OR p.Name LIKE N'SEED %' OR pf.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
    DELETE FROM dbo.Products WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductCategories', N'U') IS NOT NULL
    DELETE FROM dbo.ProductCategories WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Brands', N'U') IS NOT NULL
    DELETE FROM dbo.Brands WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.StoryTags', N'U') IS NOT NULL
    DELETE st
    FROM dbo.StoryTags st
    INNER JOIN dbo.Stories s ON s.Id = st.StoryId
    WHERE s.AddUserId = N'SEED' OR s.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.StoryFiles', N'U') IS NOT NULL
    DELETE sf
    FROM dbo.StoryFiles sf
    INNER JOIN dbo.Stories s ON s.Id = sf.StoryId
    WHERE s.AddUserId = N'SEED' OR s.Name LIKE N'SEED %' OR sf.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Stories', N'U') IS NOT NULL
    DELETE FROM dbo.Stories WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.StoryCategories', N'U') IS NOT NULL
    DELETE FROM dbo.StoryCategories WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.MenuFiles', N'U') IS NOT NULL
    DELETE mf
    FROM dbo.MenuFiles mf
    INNER JOIN dbo.Menus m ON m.Id = mf.MenuId
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
    DELETE fst
    FROM dbo.FileStorageTags fst
    INNER JOIN dbo.FileStorages fs ON fs.Id = fst.FileStorageId
    WHERE fs.FileUrl LIKE N'/media/seed/%' OR fs.Name LIKE N'SEED %';

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
    DELETE FROM dbo.Settings
    WHERE Name LIKE N'Demo: %'
       OR Name LIKE N'SEED %'
       OR SettingKey LIKE N'SEED_%'
       OR SettingKey = N'__EIMECE_SEED__';

IF OBJECT_ID(N'dbo.Templates', N'U') IS NOT NULL
    DELETE FROM dbo.Templates WHERE TemplateXml LIKE N'%<!--eimece-seed-->%' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.FileStorages', N'U') IS NOT NULL
    DELETE FROM dbo.FileStorages WHERE FileUrl LIKE N'/media/seed/%' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ShortUrls', N'U') IS NOT NULL
    DELETE FROM dbo.ShortUrls
    WHERE Position >= 900000 AND Position < 910000
       OR Url LIKE N'https://eimece.test/%'
       OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.AppLogs', N'U') IS NOT NULL
    DELETE FROM dbo.AppLogs WHERE UserName LIKE N'seed%' OR EventMessage LIKE N'SEED %';

/* Identity */
IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL
    DELETE ur
    FROM dbo.AspNetUserRoles ur
    INNER JOIN dbo.AspNetUsers u ON u.Id = ur.UserId
    WHERE u.UserName LIKE N'seed%' OR u.Email LIKE N'%@eimece.test';

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL
    DELETE uc
    FROM dbo.AspNetUserClaims uc
    INNER JOIN dbo.AspNetUsers u ON u.Id = uc.UserId
    WHERE u.UserName LIKE N'seed%' OR u.Email LIKE N'%@eimece.test';

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL
    DELETE ul
    FROM dbo.AspNetUserLogins ul
    INNER JOIN dbo.AspNetUsers u ON u.Id = ul.UserId
    WHERE u.UserName LIKE N'seed%' OR u.Email LIKE N'%@eimece.test';

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL
    DELETE FROM dbo.AspNetUsers
    WHERE UserName LIKE N'seed%' OR Email LIKE N'%@eimece.test';

COMMIT TRANSACTION;

PRINT 'Cleanup complete.';

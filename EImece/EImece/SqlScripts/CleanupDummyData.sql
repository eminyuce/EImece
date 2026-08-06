/*
================================================================================
  EImece — Cleanup Dummy / Seed Data
================================================================================
  Removes rows created by SeedDummyData.sql (marker: Name LIKE N'SEED %',
  UserName LIKE N'seed%', AddUserId = N'SEED', etc.).

  Run in SSMS against your EImece database BEFORE re-seeding, or standalone
  to wipe test data.

  WARNING: This deletes ALL rows matching the SEED markers. Do not run on
  production unless you are sure those markers are only used for test data.
================================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

PRINT 'Cleaning SEED dummy data...';

/* Child / junction tables first */
IF OBJECT_ID(N'dbo.BrowserNotificationFeedBacks', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserNotificationFeedBacks WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.BrowserNotifications', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserNotifications WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.BrowserSubscribers', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserSubscribers WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.BrowserSubscriptions', N'U') IS NOT NULL
    DELETE FROM dbo.BrowserSubscriptions WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.OrderProducts', N'U') IS NOT NULL
    DELETE op
    FROM dbo.OrderProducts op
    INNER JOIN dbo.Orders o ON o.Id = op.OrderId
    WHERE o.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
    DELETE FROM dbo.Orders WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ShoppingCarts', N'U') IS NOT NULL
    DELETE FROM dbo.ShoppingCarts WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductComments', N'U') IS NOT NULL
    DELETE FROM dbo.ProductComments WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductSpecifications', N'U') IS NOT NULL
    DELETE ps
    FROM dbo.ProductSpecifications ps
    INNER JOIN dbo.Products p ON p.Id = ps.ProductId
    WHERE p.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductTags', N'U') IS NOT NULL
    DELETE pt
    FROM dbo.ProductTags pt
    INNER JOIN dbo.Products p ON p.Id = pt.ProductId
    WHERE p.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductFiles', N'U') IS NOT NULL
    DELETE FROM dbo.ProductFiles WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
    DELETE FROM dbo.Products WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ProductCategories', N'U') IS NOT NULL
    DELETE FROM dbo.ProductCategories WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Brands', N'U') IS NOT NULL
    DELETE FROM dbo.Brands WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.StoryTags', N'U') IS NOT NULL
    DELETE st
    FROM dbo.StoryTags st
    INNER JOIN dbo.Stories s ON s.Id = st.StoryId
    WHERE s.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.StoryFiles', N'U') IS NOT NULL
    DELETE FROM dbo.StoryFiles WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Stories', N'U') IS NOT NULL
    DELETE FROM dbo.Stories WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.StoryCategories', N'U') IS NOT NULL
    DELETE FROM dbo.StoryCategories WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.MenuFiles', N'U') IS NOT NULL
    DELETE FROM dbo.MenuFiles WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Menus', N'U') IS NOT NULL
    DELETE FROM dbo.Menus WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.MainPageImages', N'U') IS NOT NULL
    DELETE FROM dbo.MainPageImages WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.FileStorageTags', N'U') IS NOT NULL
    DELETE fst
    FROM dbo.FileStorageTags fst
    INNER JOIN dbo.FileStorages fs ON fs.Id = fst.FileStorageId
    WHERE fs.Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Tags', N'U') IS NOT NULL
    DELETE FROM dbo.Tags WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.TagCategories', N'U') IS NOT NULL
    DELETE FROM dbo.TagCategories WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ListItems', N'U') IS NOT NULL
    DELETE FROM dbo.ListItems WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Lists', N'U') IS NOT NULL
    DELETE FROM dbo.Lists WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Faqs', N'U') IS NOT NULL
    DELETE FROM dbo.Faqs WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Subscribers', N'U') IS NOT NULL
    DELETE FROM dbo.Subscribers WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Coupons', N'U') IS NOT NULL
    DELETE FROM dbo.Coupons WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
    DELETE FROM dbo.Customers WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Addresses', N'U') IS NOT NULL
    DELETE FROM dbo.Addresses WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.MailTemplates', N'U') IS NOT NULL
    DELETE FROM dbo.MailTemplates WHERE AddUserId = N'SEED' OR Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.Settings', N'U') IS NOT NULL
    DELETE FROM dbo.Settings WHERE Name LIKE N'SEED %' OR SettingKey LIKE N'SEED_%';

IF OBJECT_ID(N'dbo.Templates', N'U') IS NOT NULL
    DELETE FROM dbo.Templates WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.FileStorages', N'U') IS NOT NULL
    DELETE FROM dbo.FileStorages WHERE Name LIKE N'SEED %';

IF OBJECT_ID(N'dbo.ShortUrls', N'U') IS NOT NULL
    DELETE FROM dbo.ShortUrls WHERE Name LIKE N'SEED %';

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

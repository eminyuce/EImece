/*
  EImece — SQL Server indexes for high-concurrency storefront / order lookups.

  Priority focus: Products and product-related tables (categories, tags, files,
  comments, specifications). These are the hottest tables under concurrent traffic.

  Targets the predicates used by:
    - ProductRepository category browsing / admin filters  (ProductCategoryId, BrandId, …)
    - ProductRepository.GetActiveProducts* / SearchProducts* (IsActive + Lang + Position/Name)
    - ProductCategoryRepository tree / main-page categories (ParentId, MainPage, Lang)
    - ProductTags / ProductFiles / ProductComments / ProductSpecifications by ProductId
    - OrderRepository.GetByOrderNumber* / GetOrdersUserId*  (OrderNumber, UserId, OrderGuid)

  Safe to re-run: each CREATE is guarded by IF NOT EXISTS.
  After applying, capture actual plans (see MonitorQueryExecutionPlans.sql) and confirm seeks.
*/

SET NOCOUNT ON;
GO

/* ==========================================================================
   1) Products — ProductCategoryId first (most important FK filter)
   ========================================================================== */

/*
  Category page / related-by-category / product-count-by-category:
    WHERE ProductCategoryId = @id AND IsActive = 1 AND Lang = @lang
    ORDER BY Position DESC
  Also covers IN (@categoryIds) GROUP BY ProductCategoryId counts.
*/
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_ProductCategoryId_IsActive_Lang_Position'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_ProductCategoryId_IsActive_Lang_Position
        ON dbo.Products (ProductCategoryId, IsActive, Lang, Position DESC)
        INCLUDE (Name, NameShort, NameLong, Price, Discount, ProductCode, BrandId, MainImageId, MainPage, IsCampaign, State, UpdatedDate);
    PRINT 'Created IX_Products_ProductCategoryId_IsActive_Lang_Position';
END
GO

/* Drop the older narrower category index if a previous deploy created it. */
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_ProductCategoryId_IsActive_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    DROP INDEX IX_Products_ProductCategoryId_IsActive_Lang ON dbo.Products;
    PRINT 'Dropped superseded IX_Products_ProductCategoryId_IsActive_Lang';
END
GO

/* Admin / filter grids: category + language, then Position/UpdatedDate ordering */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_ProductCategoryId_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_ProductCategoryId_Lang
        ON dbo.Products (ProductCategoryId, Lang)
        INCLUDE (Name, IsActive, BrandId, Price, Position, UpdatedDate, State, MainPage, IsCampaign, MainImageId, ProductCode);
    PRINT 'Created IX_Products_ProductCategoryId_Lang';
END
GO

/* Storefront listing: WHERE IsActive = 1 AND Lang = @lang ORDER BY Position DESC */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_IsActive_Lang_Position'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_Lang_Position
        ON dbo.Products (IsActive, Lang, Position DESC)
        INCLUDE (Name, NameShort, NameLong, Price, Discount, ProductCode, ProductCategoryId, BrandId, MainImageId, MainPage, IsCampaign, State, UpdatedDate);
    PRINT 'Created IX_Products_IsActive_Lang_Position';
END
GO

/* Home main-page strip: IsActive + MainPage + Lang ordered by Position */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_IsActive_MainPage_Lang_Position'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_MainPage_Lang_Position
        ON dbo.Products (IsActive, MainPage, Lang, Position DESC)
        INCLUDE (Name, Price, Discount, ProductCategoryId, MainImageId, UpdatedDate);
    PRINT 'Created IX_Products_IsActive_MainPage_Lang_Position';
END
GO

/* Campaign filter / admin toggles */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_IsCampaign_IsActive_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsCampaign_IsActive_Lang
        ON dbo.Products (IsCampaign, IsActive, Lang)
        INCLUDE (ProductCategoryId, Name, Price, Position, MainImageId);
    PRINT 'Created IX_Products_IsCampaign_IsActive_Lang';
END
GO

/* Stock/state filter (ProductState string) used by admin advanced filters */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_State_IsActive_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_State_IsActive_Lang
        ON dbo.Products (State, IsActive, Lang)
        INCLUDE (ProductCategoryId, Name, Price, BrandId, Position);
    PRINT 'Created IX_Products_State_IsActive_Lang';
END
GO

/* Admin SKU lookup */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_ProductCode'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_ProductCode
        ON dbo.Products (ProductCode)
        INCLUDE (Name, Lang, IsActive, ProductCategoryId, BrandId);
    PRINT 'Created IX_Products_ProductCode';
END
GO

/* Brand browsing + admin brand filter */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_BrandId_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_BrandId_Lang
        ON dbo.Products (BrandId, Lang)
        INCLUDE (Name, IsActive, ProductCategoryId, Price, Position, MainImageId);
    PRINT 'Created IX_Products_BrandId_Lang';
END
GO

/* FK seek when joining Products → FileStorages for MainImage eager loads */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_MainImageId'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_MainImageId
        ON dbo.Products (MainImageId)
        WHERE MainImageId IS NOT NULL;
    PRINT 'Created IX_Products_MainImageId';
END
GO

/*
  Optional full-text for CONTAINS-style search (Name / NameLong / NameShort).
  EF6 LINQ .Contains() still emits LIKE '%term%'; full-text requires a
  dedicated repository method using FREETEXT/CONTAINS. Enable when search volume
  justifies it — leave commented until then.

  -- CREATE FULLTEXT CATALOG EImeceFTC AS DEFAULT;
  -- CREATE FULLTEXT INDEX ON dbo.Products (Name, NameLong, NameShort)
  --     KEY INDEX PK_dbo.Products ON EImeceFTC WITH CHANGE_TRACKING AUTO;
*/

/* ==========================================================================
   2) ProductCategories — tree, main-page, language filters
   ========================================================================== */

/* GetProductCategoriesByParentId / tree build: ParentId + IsActive ORDER BY Position */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductCategories_ParentId_IsActive_Position'
      AND object_id = OBJECT_ID(N'dbo.ProductCategories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductCategories_ParentId_IsActive_Position
        ON dbo.ProductCategories (ParentId, IsActive, Position)
        INCLUDE (Name, Lang, MainPage, MainImageId, TemplateId, DiscountPercantage, UpdatedDate);
    PRINT 'Created IX_ProductCategories_ParentId_IsActive_Position';
END
GO

/* Language-scoped category lists / navigation */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductCategories_Lang_IsActive_Position'
      AND object_id = OBJECT_ID(N'dbo.ProductCategories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductCategories_Lang_IsActive_Position
        ON dbo.ProductCategories (Lang, IsActive, Position)
        INCLUDE (Name, ParentId, MainPage, MainImageId, TemplateId, DiscountPercantage);
    PRINT 'Created IX_ProductCategories_Lang_IsActive_Position';
END
GO

/* GetMainPageProductCategories: MainPage + IsActive + Lang */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductCategories_MainPage_IsActive_Lang'
      AND object_id = OBJECT_ID(N'dbo.ProductCategories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductCategories_MainPage_IsActive_Lang
        ON dbo.ProductCategories (MainPage, IsActive, Lang)
        INCLUDE (Name, ParentId, Position, MainImageId, TemplateId);
    PRINT 'Created IX_ProductCategories_MainPage_IsActive_Lang';
END
GO

/* ==========================================================================
   3) ProductTags — both directions of the many-to-many
   ========================================================================== */

/* Related products: ProductTags.Any(t => tagIdList.Contains(t.TagId)) */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductTags_TagId_ProductId'
      AND object_id = OBJECT_ID(N'dbo.ProductTags'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductTags_TagId_ProductId
        ON dbo.ProductTags (TagId, ProductId);
    PRINT 'Created IX_ProductTags_TagId_ProductId';
END
GO

/* GetAllByProductId / SaveProductTags / detail eager load */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductTags_ProductId_TagId'
      AND object_id = OBJECT_ID(N'dbo.ProductTags'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductTags_ProductId_TagId
        ON dbo.ProductTags (ProductId, TagId);
    PRINT 'Created IX_ProductTags_ProductId_TagId';
END
GO

/* ==========================================================================
   4) ProductFiles — gallery eager loads & deletes by product
   ========================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductFiles_ProductId'
      AND object_id = OBJECT_ID(N'dbo.ProductFiles'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductFiles_ProductId
        ON dbo.ProductFiles (ProductId)
        INCLUDE (FileStorageId, Position, IsActive);
    PRINT 'Created IX_ProductFiles_ProductId';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductFiles_FileStorageId'
      AND object_id = OBJECT_ID(N'dbo.ProductFiles'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductFiles_FileStorageId
        ON dbo.ProductFiles (FileStorageId)
        INCLUDE (ProductId);
    PRINT 'Created IX_ProductFiles_FileStorageId';
END
GO

/* ==========================================================================
   5) ProductComments — detail-page reviews by product + language
   ========================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductComments_ProductId_Lang_IsActive'
      AND object_id = OBJECT_ID(N'dbo.ProductComments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductComments_ProductId_Lang_IsActive
        ON dbo.ProductComments (ProductId, Lang, IsActive)
        INCLUDE (Rating, CreatedDate, Subject, Email, UserId, Position);
    PRINT 'Created IX_ProductComments_ProductId_Lang_IsActive';
END
GO

/* ==========================================================================
   6) ProductSpecifications — detail-page specs / admin replace-by-product
   ========================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_ProductSpecifications_ProductId'
      AND object_id = OBJECT_ID(N'dbo.ProductSpecifications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductSpecifications_ProductId
        ON dbo.ProductSpecifications (ProductId)
        INCLUDE (Name, Value, Unit, Position, IsActive);
    PRINT 'Created IX_ProductSpecifications_ProductId';
END
GO

/* ==========================================================================
   7) Brands — storefront brand lists filtered by language
   ========================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Brands_Lang_IsActive_Position'
      AND object_id = OBJECT_ID(N'dbo.Brands'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Brands_Lang_IsActive_Position
        ON dbo.Brands (Lang, IsActive, Position)
        INCLUDE (Name, MainImageId, UpdatedDate);
    PRINT 'Created IX_Brands_Lang_IsActive_Position';
END
GO

/* ==========================================================================
   8) Orders — customer lookup / order-number seek (secondary to products)
   ========================================================================== */

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Orders_OrderNumber'
      AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Orders_OrderNumber
        ON dbo.Orders (OrderNumber)
        WHERE OrderNumber IS NOT NULL;
    PRINT 'Created IX_Orders_OrderNumber';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Orders_UserId_UpdatedDate'
      AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_UserId_UpdatedDate
        ON dbo.Orders (UserId, UpdatedDate DESC)
        INCLUDE (OrderNumber, OrderGuid, OrderStatus, PaidPrice, PaymentStatus);
    PRINT 'Created IX_Orders_UserId_UpdatedDate';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Orders_OrderGuid'
      AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_OrderGuid
        ON dbo.Orders (OrderGuid);
    PRINT 'Created IX_Orders_OrderGuid';
END
GO

/* Order lines — FK seeks when eager-loading OrderProducts / product delete guard */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_OrderProducts_ProductId'
      AND object_id = OBJECT_ID(N'dbo.OrderProducts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderProducts_ProductId
        ON dbo.OrderProducts (ProductId)
        INCLUDE (OrderId);
    PRINT 'Created IX_OrderProducts_ProductId';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_OrderProducts_OrderId'
      AND object_id = OBJECT_ID(N'dbo.OrderProducts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OrderProducts_OrderId
        ON dbo.OrderProducts (OrderId)
        INCLUDE (ProductId);
    PRINT 'Created IX_OrderProducts_OrderId';
END
GO

PRINT 'AddPerformanceIndexes.sql completed.';
GO

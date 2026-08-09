/*
  EImece — SQL Server indexes for high-concurrency storefront / order lookups.

  Targets the predicates used by:
    - ProductRepository.GetActiveProducts* / SearchProducts*  (IsActive + Lang + Position/Name)
    - ProductRepository admin/list filters                     (ProductCategoryId, BrandId, ProductCode)
    - OrderRepository.GetByOrderNumber* / GetOrdersUserId*    (OrderNumber, UserId, OrderGuid)

  Safe to re-run: each CREATE is guarded by IF NOT EXISTS.
  After applying, capture actual plans (see MonitorQueryExecutionPlans.sql) and confirm seeks.
*/

SET NOCOUNT ON;
GO

/* --------------------------------------------------------------------------
   Products — listing / search hot path
   Predicate shape: WHERE IsActive = 1 AND Lang = @lang ORDER BY Position DESC
   -------------------------------------------------------------------------- */
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

/* Main-page strip: IsActive + MainPage + Lang ordered by Position */
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

/* Category browsing / related products */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_ProductCategoryId_IsActive_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_ProductCategoryId_IsActive_Lang
        ON dbo.Products (ProductCategoryId, IsActive, Lang)
        INCLUDE (Name, Price, Position, MainImageId, UpdatedDate);
    PRINT 'Created IX_Products_ProductCategoryId_IsActive_Lang';
END
GO

/* Admin lookup by SKU / brand */
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

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Products_BrandId_Lang'
      AND object_id = OBJECT_ID(N'dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_BrandId_Lang
        ON dbo.Products (BrandId, Lang)
        INCLUDE (Name, IsActive, ProductCategoryId, Price);
    PRINT 'Created IX_Products_BrandId_Lang';
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

/* --------------------------------------------------------------------------
   Orders — customer lookup / order-number seek
   -------------------------------------------------------------------------- */
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

/* Order lines — FK seeks when eager-loading OrderProducts for a product delete check */
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

/* ProductTags — related-product Any(TagId IN (...)) */
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

PRINT 'AddPerformanceIndexes.sql completed.';
GO

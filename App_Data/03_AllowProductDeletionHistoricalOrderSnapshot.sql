-- ============================================================================
-- EImece Migration Script: Allow Product Deletion & Historical Order Snapshot
-- ============================================================================
-- Database: eimece / yuva8905_yuvadan
-- Purpose : 1. Backfills missing historical snapshot columns on OrderProducts
--              from current Products/ProductCategories data.
--           2. Adds ProductImageUrl column to OrderProducts for image snapshots.
--           3. Backfills ProductImageUrl from existing Product main images.
--           4. Modifies OrderProducts.ProductId to be NULLable.
--           5. Reconfigures FK_Order_Products_Products with ON DELETE SET NULL.
-- ============================================================================

PRINT N'Starting migration: Allow Product Deletion & Historical Order Snapshot...';
GO

-- Step 1: Backfill missing product snapshot fields on OrderProducts using current Product data
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderProducts')
BEGIN
    PRINT N'1. Backfilling historical product snapshot fields in dbo.OrderProducts...';

    UPDATE op
    SET
        op.ProductName = COALESCE(NULLIF(op.ProductName, N''), p.Name, N'Ürün #' + CAST(op.ProductId AS NVARCHAR(50))),
        op.ProductCode = COALESCE(NULLIF(op.ProductCode, N''), p.ProductCode, N''),
        op.CategoryName = COALESCE(NULLIF(op.CategoryName, N''), pc.Name, N''),
        op.ProductSalePrice = COALESCE(op.ProductSalePrice, CASE WHEN op.Quantity > 0 THEN op.TotalPrice / op.Quantity ELSE p.Price END, 0)
    FROM dbo.OrderProducts op
    INNER JOIN dbo.Products p ON op.ProductId = p.Id
    LEFT JOIN dbo.ProductCategories pc ON p.ProductCategoryId = pc.Id
    WHERE op.ProductName IS NULL OR op.ProductName = N'' 
       OR op.ProductSalePrice IS NULL 
       OR op.CategoryName IS NULL;

    PRINT N'Historical snapshot fields backfilled.';
END
GO

-- Step 2: Add ProductImageUrl column to OrderProducts if not exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderProducts')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrderProducts') AND name = 'ProductImageUrl')
BEGIN
    PRINT N'2. Adding [ProductImageUrl] column to dbo.OrderProducts...';
    ALTER TABLE dbo.OrderProducts ADD [ProductImageUrl] NVARCHAR(1000) NULL;
    PRINT N'[ProductImageUrl] column added successfully.';
END
ELSE
BEGIN
    PRINT N'2. [ProductImageUrl] column already exists in dbo.OrderProducts.';
END
GO

-- Step 3: Backfill ProductImageUrl for existing order products
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderProducts')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrderProducts') AND name = 'ProductImageUrl')
BEGIN
    PRINT N'3. Backfilling ProductImageUrl for existing order items...';

    -- Try to match main image from FileStorages if available
    UPDATE op
    SET op.ProductImageUrl = CASE 
        WHEN fs.FileName IS NOT NULL AND fs.FileName = N'external_image' AND fs.FileUrl IS NOT NULL THEN fs.FileUrl
        WHEN fs.FileName IS NOT NULL THEN N'/images/products/' + fs.FileName
        ELSE NULL
    END
    FROM dbo.OrderProducts op
    INNER JOIN dbo.Products p ON op.ProductId = p.Id
    LEFT JOIN dbo.FileStorages fs ON p.MainImageId = fs.Id
    WHERE op.ProductImageUrl IS NULL AND p.MainImageId IS NOT NULL;

    PRINT N'ProductImageUrl backfilled.';
END
GO

-- Step 4: Drop old foreign key constraint FK_Order_Products_Products if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Order_Products_Products' AND parent_object_id = OBJECT_ID(N'dbo.OrderProducts'))
BEGIN
    PRINT N'4. Dropping existing constraint [FK_Order_Products_Products]...';
    ALTER TABLE dbo.OrderProducts DROP CONSTRAINT [FK_Order_Products_Products];
    PRINT N'Existing constraint dropped.';
END
GO

-- Step 5: Make ProductId nullable on OrderProducts
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.OrderProducts') AND name = 'ProductId' AND is_nullable = 0)
BEGIN
    PRINT N'5. Altering dbo.OrderProducts.ProductId to be NULLable...';
    ALTER TABLE dbo.OrderProducts ALTER COLUMN [ProductId] INT NULL;
    PRINT N'dbo.OrderProducts.ProductId altered to NULL.';
END
GO

-- Step 6: Re-create foreign key constraint with ON DELETE SET NULL and ON UPDATE CASCADE
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Order_Products_Products' AND parent_object_id = OBJECT_ID(N'dbo.OrderProducts'))
BEGIN
    PRINT N'6. Creating constraint [FK_Order_Products_Products] with ON DELETE SET NULL ON UPDATE CASCADE...';
    ALTER TABLE dbo.OrderProducts WITH CHECK ADD CONSTRAINT [FK_Order_Products_Products] 
        FOREIGN KEY([ProductId]) REFERENCES dbo.Products ([Id])
        ON DELETE SET NULL
        ON UPDATE CASCADE;

    ALTER TABLE dbo.OrderProducts CHECK CONSTRAINT [FK_Order_Products_Products];
    PRINT N'Constraint [FK_Order_Products_Products] created with ON DELETE SET NULL.';
END
GO

PRINT N'Migration completed successfully: Products can now be deleted without deleting or corrupting historical orders.';
GO

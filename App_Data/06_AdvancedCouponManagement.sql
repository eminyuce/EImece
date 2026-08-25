-- ============================================================================
-- EImece Advanced Coupon Management Migration
-- File: 06_AdvancedCouponManagement.sql
-- Purpose: Extends Coupons with campaign/rule-based fields, adds
--          CouponProducts, CouponCategories, CouponRedemptions plus
--          Customer.BirthDate, and migrates legacy discount data.
--          Idempotent: safe to re-run. Preserves existing coupons.
-- ============================================================================

SET NOCOUNT ON;
GO

PRINT N'=== Starting Advanced Coupon Management Migration ===';
PRINT N'Database: ' + DB_NAME();
GO

-- --------------------------------------------------------------------------
-- 0) Coupons: per-customer assignment (from former 05, retained for coupon feature)
-- --------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'AssignedUserId')
BEGIN
    PRINT N'Adding Coupons.AssignedUserId...';
    ALTER TABLE dbo.Coupons ADD AssignedUserId NVARCHAR(128) NULL;
    CREATE NONCLUSTERED INDEX IX_Coupons_AssignedUserId ON dbo.Coupons(AssignedUserId);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'AssignedCustomerId')
BEGIN
    PRINT N'Adding Coupons.AssignedCustomerId...';
    ALTER TABLE dbo.Coupons ADD AssignedCustomerId INT NULL;
    CREATE NONCLUSTERED INDEX IX_Coupons_AssignedCustomerId ON dbo.Coupons(AssignedCustomerId);
END
GO

-- --------------------------------------------------------------------------
-- 1) Coupons: add advanced columns if missing
-- --------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'DiscountType')
BEGIN
    PRINT N'Adding Coupons.DiscountType...';
    ALTER TABLE dbo.Coupons ADD DiscountType INT NOT NULL CONSTRAINT DF_Coupons_DiscountType DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'MaximumDiscountAmount')
BEGIN
    PRINT N'Adding Coupons.MaximumDiscountAmount...';
    ALTER TABLE dbo.Coupons ADD MaximumDiscountAmount DECIMAL(18,2) NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'GlobalUsageLimit')
BEGIN
    PRINT N'Adding Coupons.GlobalUsageLimit...';
    ALTER TABLE dbo.Coupons ADD GlobalUsageLimit INT NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'PerCustomerUsageLimit')
BEGIN
    PRINT N'Adding Coupons.PerCustomerUsageLimit...';
    ALTER TABLE dbo.Coupons ADD PerCustomerUsageLimit INT NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'MinimumOrderAmount')
BEGIN
    PRINT N'Adding Coupons.MinimumOrderAmount...';
    ALTER TABLE dbo.Coupons ADD MinimumOrderAmount DECIMAL(18,2) NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'ExcludeSaleItems')
BEGIN
    PRINT N'Adding Coupons.ExcludeSaleItems...';
    ALTER TABLE dbo.Coupons ADD ExcludeSaleItems BIT NOT NULL CONSTRAINT DF_Coupons_ExcludeSaleItems DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'IsFreeShipping')
BEGIN
    PRINT N'Adding Coupons.IsFreeShipping...';
    ALTER TABLE dbo.Coupons ADD IsFreeShipping BIT NOT NULL CONSTRAINT DF_Coupons_IsFreeShipping DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'AllowStacking')
BEGIN
    PRINT N'Adding Coupons.AllowStacking...';
    ALTER TABLE dbo.Coupons ADD AllowStacking BIT NOT NULL CONSTRAINT DF_Coupons_AllowStacking DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'RequireLogin')
BEGIN
    PRINT N'Adding Coupons.RequireLogin...';
    ALTER TABLE dbo.Coupons ADD RequireLogin BIT NOT NULL CONSTRAINT DF_Coupons_RequireLogin DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'IsFirstOrderOnly')
BEGIN
    PRINT N'Adding Coupons.IsFirstOrderOnly...';
    ALTER TABLE dbo.Coupons ADD IsFirstOrderOnly BIT NOT NULL CONSTRAINT DF_Coupons_IsFirstOrderOnly DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'IsNewCustomerOnly')
BEGIN
    PRINT N'Adding Coupons.IsNewCustomerOnly...';
    ALTER TABLE dbo.Coupons ADD IsNewCustomerOnly BIT NOT NULL CONSTRAINT DF_Coupons_IsNewCustomerOnly DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'IsBirthdayCoupon')
BEGIN
    PRINT N'Adding Coupons.IsBirthdayCoupon...';
    ALTER TABLE dbo.Coupons ADD IsBirthdayCoupon BIT NOT NULL CONSTRAINT DF_Coupons_IsBirthdayCoupon DEFAULT (0);
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'BirthdayWindow')
BEGIN
    PRINT N'Adding Coupons.BirthdayWindow...';
    ALTER TABLE dbo.Coupons ADD BirthdayWindow INT NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Coupons') AND name = 'Currency')
BEGIN
    PRINT N'Adding Coupons.Currency...';
    ALTER TABLE dbo.Coupons ADD Currency NVARCHAR(10) NULL;
END
GO

-- Migrate legacy DiscountType for existing coupons (Backward compat)
-- FixedAmount =0, Percentage=1, FreeShipping=2
-- If DiscountPercentage>0 and Discount=0 => Percentage
PRINT N'Migrating legacy DiscountType...';
UPDATE dbo.Coupons SET DiscountType = 1 WHERE DiscountPercentage > 0 AND Discount = 0 AND DiscountType = 0;
-- If IsFreeShipping=1 handled separately; otherwise keep FixedAmount 0
GO

-- --------------------------------------------------------------------------
-- 2) Customers: BirthDate
-- --------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Customers') AND name = 'BirthDate')
BEGIN
    PRINT N'Adding Customers.BirthDate...';
    ALTER TABLE dbo.Customers ADD BirthDate DATETIME NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_BirthDate' AND object_id = OBJECT_ID(N'dbo.Customers'))
BEGIN
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;
    PRINT N'Creating index IX_Customers_BirthDate...';
    CREATE NONCLUSTERED INDEX IX_Customers_BirthDate ON dbo.Customers(BirthDate) WHERE BirthDate IS NOT NULL;
END
GO

-- --------------------------------------------------------------------------
-- 3) CouponProducts (product restrictions)
-- --------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CouponProducts')
BEGIN
    PRINT N'Creating table CouponProducts...';
    CREATE TABLE dbo.CouponProducts (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
        CouponId INT NOT NULL,
        ProductId INT NOT NULL,
        CONSTRAINT FK_CouponProducts_Coupons FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CouponProducts_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX IX_CouponProducts_CouponId_ProductId ON dbo.CouponProducts(CouponId, ProductId);
    CREATE NONCLUSTERED INDEX IX_CouponProducts_ProductId ON dbo.CouponProducts(ProductId);
    PRINT N'CouponProducts created.';
END
GO

-- --------------------------------------------------------------------------
-- 4) CouponCategories (category restrictions)
-- --------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CouponCategories')
BEGIN
    PRINT N'Creating table CouponCategories...';
    CREATE TABLE dbo.CouponCategories (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
        CouponId INT NOT NULL,
        ProductCategoryId INT NOT NULL,
        CONSTRAINT FK_CouponCategories_Coupons FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CouponCategories_ProductCategories FOREIGN KEY (ProductCategoryId) REFERENCES dbo.ProductCategories(Id) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX IX_CouponCategories_CouponId_CategoryId ON dbo.CouponCategories(CouponId, ProductCategoryId);
    CREATE NONCLUSTERED INDEX IX_CouponCategories_CategoryId ON dbo.CouponCategories(ProductCategoryId);
    PRINT N'CouponCategories created.';
END
GO

-- --------------------------------------------------------------------------
-- 5) CouponRedemptions (history + concurrency)
-- --------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CouponRedemptions')
BEGIN
    PRINT N'Creating table CouponRedemptions...';
    CREATE TABLE dbo.CouponRedemptions (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY CLUSTERED,
        Name NVARCHAR(500) NOT NULL,
        CouponId INT NOT NULL,
        OrderId INT NOT NULL,
        CustomerId INT NULL,
        UserId NVARCHAR(128) NULL,
        CouponCode NVARCHAR(255) NOT NULL,
        DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CouponRedemptions_DiscountAmount DEFAULT (0),
        OrderTotalBeforeDiscount DECIMAL(18,2) NOT NULL CONSTRAINT DF_CouponRedemptions_OrderTotal DEFAULT (0),
        Currency NVARCHAR(10) NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_CouponRedemptions_CreatedDate DEFAULT (GETDATE()),
        UpdatedDate DATETIME NOT NULL CONSTRAINT DF_CouponRedemptions_UpdatedDate DEFAULT (GETDATE()),
        IsActive BIT NOT NULL CONSTRAINT DF_CouponRedemptions_IsActive DEFAULT (1),
        Position INT NOT NULL CONSTRAINT DF_CouponRedemptions_Position DEFAULT (0),
        Lang INT NOT NULL CONSTRAINT DF_CouponRedemptions_Lang DEFAULT (1),
        CONSTRAINT FK_CouponRedemptions_Coupons FOREIGN KEY (CouponId) REFERENCES dbo.Coupons(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_CouponRedemptions_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_CouponRedemptions_CouponId ON dbo.CouponRedemptions(CouponId);
    CREATE NONCLUSTERED INDEX IX_CouponRedemptions_OrderId ON dbo.CouponRedemptions(OrderId);
    CREATE NONCLUSTERED INDEX IX_CouponRedemptions_UserId_CouponId ON dbo.CouponRedemptions(UserId, CouponId);
    CREATE NONCLUSTERED INDEX IX_CouponRedemptions_CustomerId_CouponId ON dbo.CouponRedemptions(CustomerId, CouponId);
    CREATE NONCLUSTERED INDEX IX_CouponRedemptions_CreatedDate ON dbo.CouponRedemptions(CreatedDate);
    PRINT N'CouponRedemptions created.';
END
GO

-- Ensure indexes for Orders coupon lookups (usage limit checks)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_Coupon_CreatedDate' AND object_id = OBJECT_ID(N'dbo.Orders'))
BEGIN
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;
    PRINT N'Creating index IX_Orders_Coupon_CreatedDate...';
    CREATE NONCLUSTERED INDEX IX_Orders_Coupon_CreatedDate ON dbo.Orders(Coupon) INCLUDE (CreatedDate, UserId) WHERE Coupon IS NOT NULL AND Coupon <> '';
END
GO

PRINT N'=== Advanced Coupon Management Migration Completed ===';
GO

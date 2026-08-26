-- ============================================================================
-- Script: AddUserAuditReportsStoredProcedures.sql
-- Description: Stored procedures for Admin User Audit Reports:
--   1. sp_GetUserAuditSummaryReport (Report by User)
--   2. sp_GetUserAuditMonthlyBreakdown (Monthly Breakdown)
--   3. sp_GetUserAuditDetailedRecords (Detailed Records Audit Log)
--   4. sp_GetAuditUsersList (Available users filter list)
--   5. sp_GetAuditTablesList (Available tables filter list)
-- ============================================================================

IF OBJECT_ID(N'dbo.sp_GetAuditTablesList', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAuditTablesList;
GO

CREATE PROCEDURE dbo.sp_GetAuditTablesList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT c1.TABLE_NAME AS TableName
    FROM INFORMATION_SCHEMA.COLUMNS c1
    INNER JOIN INFORMATION_SCHEMA.COLUMNS c2
        ON c1.TABLE_SCHEMA = c2.TABLE_SCHEMA AND c1.TABLE_NAME = c2.TABLE_NAME
    WHERE c1.TABLE_SCHEMA = 'dbo'
      AND c1.COLUMN_NAME = 'AddUserId'
      AND c2.COLUMN_NAME = 'UpdateUserId'
      AND c1.TABLE_NAME NOT LIKE 'sys%'
    ORDER BY TableName ASC;
END
GO

IF OBJECT_ID(N'dbo.sp_GetAuditUsersList', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAuditUsersList;
GO

CREATE PROCEDURE dbo.sp_GetAuditUsersList
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.Id AS UserId,
        u.UserName,
        CASE 
            WHEN u.FirstName IS NOT NULL AND LTRIM(RTRIM(u.FirstName)) <> '' 
                 AND u.LastName IS NOT NULL AND LTRIM(RTRIM(u.LastName)) <> '' 
            THEN LTRIM(RTRIM(u.FirstName)) + ' ' + LTRIM(RTRIM(u.LastName))
            WHEN u.FirstName IS NOT NULL AND LTRIM(RTRIM(u.FirstName)) <> '' 
            THEN LTRIM(RTRIM(u.FirstName))
            WHEN u.LastName IS NOT NULL AND LTRIM(RTRIM(u.LastName)) <> '' 
            THEN LTRIM(RTRIM(u.LastName))
            WHEN u.UserName IS NOT NULL AND LTRIM(RTRIM(u.UserName)) <> ''
            THEN LTRIM(RTRIM(u.UserName))
            ELSE N'Unknown'
        END AS FullName
    FROM dbo.AspNetUsers u
    ORDER BY FullName ASC;
END
GO

IF OBJECT_ID(N'dbo.sp_GetUserAuditSummaryReport', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserAuditSummaryReport;
GO

CREATE PROCEDURE dbo.sp_GetUserAuditSummaryReport
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @UserId NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Adjust EndDate to include full end of day if time part is midnight
    DECLARE @AdjustedEndDate DATETIME = @EndDate;
    IF @AdjustedEndDate IS NOT NULL AND CAST(@AdjustedEndDate AS TIME) = '00:00:00'
    BEGIN
        SET @AdjustedEndDate = DATEADD(ms, -3, DATEADD(dd, 1, DATEDIFF(dd, 0, @AdjustedEndDate)));
    END

    -- Temporary table to hold unified audit raw events
    CREATE TABLE #AuditEvents (
        TableName NVARCHAR(128) NOT NULL,
        RecordId INT NOT NULL,
        CreatedDate DATETIME NULL,
        UpdatedDate DATETIME NULL,
        AddUserId NVARCHAR(128) NULL,
        UpdateUserId NVARCHAR(128) NULL
    );

    -- Insert records from all audit-tracked tables
    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Brands'
        INSERT INTO #AuditEvents SELECT 'Brands', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Brands;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Coupons'
        INSERT INTO #AuditEvents SELECT 'Coupons', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Coupons;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Faqs'
        INSERT INTO #AuditEvents SELECT 'Faqs', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Faqs;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'MailTemplates'
        INSERT INTO #AuditEvents SELECT 'MailTemplates', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.MailTemplates;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'MainPageImages'
        INSERT INTO #AuditEvents SELECT 'MainPageImages', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.MainPageImages;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Menus'
        INSERT INTO #AuditEvents SELECT 'Menus', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Menus;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'ProductCategories'
        INSERT INTO #AuditEvents SELECT 'ProductCategories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.ProductCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Products'
        INSERT INTO #AuditEvents SELECT 'Products', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Products;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Stories'
        INSERT INTO #AuditEvents SELECT 'Stories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Stories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'StoryCategories'
        INSERT INTO #AuditEvents SELECT 'StoryCategories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.StoryCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'TagCategories'
        INSERT INTO #AuditEvents SELECT 'TagCategories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.TagCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Tags'
        INSERT INTO #AuditEvents SELECT 'Tags', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Tags;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Templates'
        INSERT INTO #AuditEvents SELECT 'Templates', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Templates;

    ;WITH UnpivotedActions AS (
        -- Created events
        SELECT 
            TableName,
            RecordId,
            AddUserId AS ActionUserId,
            'Create' AS ActionType,
            CreatedDate AS ActionDate
        FROM #AuditEvents
        WHERE AddUserId IS NOT NULL AND LTRIM(RTRIM(AddUserId)) <> ''
          AND (@StartDate IS NULL OR CreatedDate >= @StartDate)
          AND (@AdjustedEndDate IS NULL OR CreatedDate <= @AdjustedEndDate)
          AND (@UserId IS NULL OR @UserId = '' OR AddUserId = @UserId)

        UNION ALL

        -- Updated events
        SELECT 
            TableName,
            RecordId,
            UpdateUserId AS ActionUserId,
            'Update' AS ActionType,
            UpdatedDate AS ActionDate
        FROM #AuditEvents
        WHERE UpdateUserId IS NOT NULL AND LTRIM(RTRIM(UpdateUserId)) <> ''
          AND (@StartDate IS NULL OR UpdatedDate >= @StartDate)
          AND (@AdjustedEndDate IS NULL OR UpdatedDate <= @AdjustedEndDate)
          AND (@UserId IS NULL OR @UserId = '' OR UpdateUserId = @UserId)
    ),
    UserActionAggregates AS (
        SELECT 
            ActionUserId,
            SUM(CASE WHEN ActionType = 'Create' THEN 1 ELSE 0 END) AS CreatedCount,
            SUM(CASE WHEN ActionType = 'Update' THEN 1 ELSE 0 END) AS UpdatedCount,
            COUNT(*) AS TotalActivity
        FROM UnpivotedActions
        GROUP BY ActionUserId
    ),
    UserDistinctTables AS (
        SELECT DISTINCT
            ActionUserId,
            TableName
        FROM UnpivotedActions
    ),
    UserTablesConcatenated AS (
        SELECT 
            t.ActionUserId,
            STUFF((
                SELECT ', ' + t2.TableName
                FROM UserDistinctTables t2
                WHERE t2.ActionUserId = t.ActionUserId
                ORDER BY t2.TableName
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TablesModified
        FROM UserDistinctTables t
        GROUP BY t.ActionUserId
    )
    SELECT 
        agg.ActionUserId AS UserId,
        ISNULL(u.UserName, agg.ActionUserId) AS UserName,
        CASE 
            WHEN u.FirstName IS NOT NULL AND LTRIM(RTRIM(u.FirstName)) <> '' 
                 AND u.LastName IS NOT NULL AND LTRIM(RTRIM(u.LastName)) <> '' 
            THEN LTRIM(RTRIM(u.FirstName)) + ' ' + LTRIM(RTRIM(u.LastName))
            WHEN u.FirstName IS NOT NULL AND LTRIM(RTRIM(u.FirstName)) <> '' 
            THEN LTRIM(RTRIM(u.FirstName))
            WHEN u.LastName IS NOT NULL AND LTRIM(RTRIM(u.LastName)) <> '' 
            THEN LTRIM(RTRIM(u.LastName))
            WHEN u.UserName IS NOT NULL AND LTRIM(RTRIM(u.UserName)) <> ''
            THEN LTRIM(RTRIM(u.UserName))
            ELSE N'Unknown'
        END AS FullName,
        agg.CreatedCount,
        agg.UpdatedCount,
        agg.TotalActivity,
        ISNULL(utc.TablesModified, '') AS TablesModified
    FROM UserActionAggregates agg
    LEFT JOIN dbo.AspNetUsers u ON u.Id = agg.ActionUserId
    LEFT JOIN UserTablesConcatenated utc ON utc.ActionUserId = agg.ActionUserId
    ORDER BY agg.TotalActivity DESC, FullName ASC;

    DROP TABLE #AuditEvents;
END
GO

IF OBJECT_ID(N'dbo.sp_GetUserAuditMonthlyBreakdown', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserAuditMonthlyBreakdown;
GO

CREATE PROCEDURE dbo.sp_GetUserAuditMonthlyBreakdown
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @UserId NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AdjustedEndDate DATETIME = @EndDate;
    IF @AdjustedEndDate IS NOT NULL AND CAST(@AdjustedEndDate AS TIME) = '00:00:00'
    BEGIN
        SET @AdjustedEndDate = DATEADD(ms, -3, DATEADD(dd, 1, DATEDIFF(dd, 0, @AdjustedEndDate)));
    END

    CREATE TABLE #AuditEvents (
        TableName NVARCHAR(128) NOT NULL,
        RecordId INT NOT NULL,
        CreatedDate DATETIME NULL,
        UpdatedDate DATETIME NULL,
        AddUserId NVARCHAR(128) NULL,
        UpdateUserId NVARCHAR(128) NULL
    );

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Brands'
        INSERT INTO #AuditEvents SELECT 'Brands', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Brands;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Coupons'
        INSERT INTO #AuditEvents SELECT 'Coupons', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Coupons;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Faqs'
        INSERT INTO #AuditEvents SELECT 'Faqs', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Faqs;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'MailTemplates'
        INSERT INTO #AuditEvents SELECT 'MailTemplates', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.MailTemplates;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'MainPageImages'
        INSERT INTO #AuditEvents SELECT 'MainPageImages', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.MainPageImages;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Menus'
        INSERT INTO #AuditEvents SELECT 'Menus', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Menus;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'ProductCategories'
        INSERT INTO #AuditEvents SELECT 'ProductCategories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.ProductCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Products'
        INSERT INTO #AuditEvents SELECT 'Products', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Products;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Stories'
        INSERT INTO #AuditEvents SELECT 'Stories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Stories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'StoryCategories'
        INSERT INTO #AuditEvents SELECT 'StoryCategories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.StoryCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'TagCategories'
        INSERT INTO #AuditEvents SELECT 'TagCategories', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.TagCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Tags'
        INSERT INTO #AuditEvents SELECT 'Tags', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Tags;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Templates'
        INSERT INTO #AuditEvents SELECT 'Templates', Id, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Templates;

    ;WITH UnpivotedActions AS (
        SELECT 
            AddUserId AS ActionUserId,
            'Create' AS ActionType,
            CreatedDate AS ActionDate,
            YEAR(CreatedDate) AS ActionYear,
            MONTH(CreatedDate) AS ActionMonth
        FROM #AuditEvents
        WHERE AddUserId IS NOT NULL AND LTRIM(RTRIM(AddUserId)) <> ''
          AND CreatedDate IS NOT NULL
          AND (@StartDate IS NULL OR CreatedDate >= @StartDate)
          AND (@AdjustedEndDate IS NULL OR CreatedDate <= @AdjustedEndDate)
          AND (@UserId IS NULL OR @UserId = '' OR AddUserId = @UserId)

        UNION ALL

        SELECT 
            UpdateUserId AS ActionUserId,
            'Update' AS ActionType,
            UpdatedDate AS ActionDate,
            YEAR(UpdatedDate) AS ActionYear,
            MONTH(UpdatedDate) AS ActionMonth
        FROM #AuditEvents
        WHERE UpdateUserId IS NOT NULL AND LTRIM(RTRIM(UpdateUserId)) <> ''
          AND UpdatedDate IS NOT NULL
          AND (@StartDate IS NULL OR UpdatedDate >= @StartDate)
          AND (@AdjustedEndDate IS NULL OR UpdatedDate <= @AdjustedEndDate)
          AND (@UserId IS NULL OR @UserId = '' OR UpdateUserId = @UserId)
    ),
    MonthlyAggregates AS (
        SELECT 
            ActionUserId,
            ActionYear AS [Year],
            ActionMonth AS [Month],
            SUM(CASE WHEN ActionType = 'Create' THEN 1 ELSE 0 END) AS CreatedCount,
            SUM(CASE WHEN ActionType = 'Update' THEN 1 ELSE 0 END) AS UpdatedCount,
            COUNT(*) AS TotalCount
        FROM UnpivotedActions
        WHERE ActionYear IS NOT NULL AND ActionMonth IS NOT NULL
        GROUP BY ActionUserId, ActionYear, ActionMonth
    )
    SELECT 
        m.ActionUserId AS UserId,
        ISNULL(u.UserName, m.ActionUserId) AS UserName,
        CASE 
            WHEN u.FirstName IS NOT NULL AND LTRIM(RTRIM(u.FirstName)) <> '' 
                 AND u.LastName IS NOT NULL AND LTRIM(RTRIM(u.LastName)) <> '' 
            THEN LTRIM(RTRIM(u.FirstName)) + ' ' + LTRIM(RTRIM(u.LastName))
            WHEN u.FirstName IS NOT NULL AND LTRIM(RTRIM(u.FirstName)) <> '' 
            THEN LTRIM(RTRIM(u.FirstName))
            WHEN u.LastName IS NOT NULL AND LTRIM(RTRIM(u.LastName)) <> '' 
            THEN LTRIM(RTRIM(u.LastName))
            WHEN u.UserName IS NOT NULL AND LTRIM(RTRIM(u.UserName)) <> ''
            THEN LTRIM(RTRIM(u.UserName))
            ELSE N'Unknown'
        END AS FullName,
        m.[Year],
        m.[Month],
        CONVERT(VARCHAR(4), m.[Year]) + '-' + RIGHT('0' + CONVERT(VARCHAR(2), m.[Month]), 2) AS YearMonth,
        m.CreatedCount,
        m.UpdatedCount,
        m.TotalCount
    FROM MonthlyAggregates m
    LEFT JOIN dbo.AspNetUsers u ON u.Id = m.ActionUserId
    ORDER BY m.[Year] DESC, m.[Month] DESC, m.TotalCount DESC, FullName ASC;

    DROP TABLE #AuditEvents;
END
GO

IF OBJECT_ID(N'dbo.sp_GetUserAuditDetailedRecords', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserAuditDetailedRecords;
GO

CREATE PROCEDURE dbo.sp_GetUserAuditDetailedRecords
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @UserId NVARCHAR(128) = NULL,
    @TableName NVARCHAR(128) = NULL,
    @ActionType NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AdjustedEndDate DATETIME = @EndDate;
    IF @AdjustedEndDate IS NOT NULL AND CAST(@AdjustedEndDate AS TIME) = '00:00:00'
    BEGIN
        SET @AdjustedEndDate = DATEADD(ms, -3, DATEADD(dd, 1, DATEDIFF(dd, 0, @AdjustedEndDate)));
    END

    CREATE TABLE #AuditRecords (
        TableName NVARCHAR(128) NOT NULL,
        RecordId INT NOT NULL,
        RecordName NVARCHAR(500) NULL,
        CreatedDate DATETIME NULL,
        UpdatedDate DATETIME NULL,
        AddUserId NVARCHAR(128) NULL,
        UpdateUserId NVARCHAR(128) NULL
    );

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Brands'
        INSERT INTO #AuditRecords SELECT 'Brands', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Brands;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Coupons'
        INSERT INTO #AuditRecords SELECT 'Coupons', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Coupons;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Faqs'
        INSERT INTO #AuditRecords SELECT 'Faqs', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Faqs;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'MailTemplates'
        INSERT INTO #AuditRecords SELECT 'MailTemplates', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.MailTemplates;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'MainPageImages'
        INSERT INTO #AuditRecords SELECT 'MainPageImages', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.MainPageImages;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Menus'
        INSERT INTO #AuditRecords SELECT 'Menus', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Menus;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'ProductCategories'
        INSERT INTO #AuditRecords SELECT 'ProductCategories', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.ProductCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Products'
        INSERT INTO #AuditRecords SELECT 'Products', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Products;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Stories'
        INSERT INTO #AuditRecords SELECT 'Stories', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Stories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'StoryCategories'
        INSERT INTO #AuditRecords SELECT 'StoryCategories', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.StoryCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'TagCategories'
        INSERT INTO #AuditRecords SELECT 'TagCategories', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.TagCategories;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Tags'
        INSERT INTO #AuditRecords SELECT 'Tags', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Tags;

    IF @TableName IS NULL OR @TableName = '' OR @TableName = 'Templates'
        INSERT INTO #AuditRecords SELECT 'Templates', Id, Name, CreatedDate, UpdatedDate, AddUserId, UpdateUserId FROM dbo.Templates;

    SELECT 
        r.TableName,
        r.RecordId,
        ISNULL(r.RecordName, N'-') AS RecordName,
        r.CreatedDate,
        r.UpdatedDate,
        r.AddUserId,
        CASE 
            WHEN cu.FirstName IS NOT NULL AND LTRIM(RTRIM(cu.FirstName)) <> '' 
                 AND cu.LastName IS NOT NULL AND LTRIM(RTRIM(cu.LastName)) <> '' 
            THEN LTRIM(RTRIM(cu.FirstName)) + ' ' + LTRIM(RTRIM(cu.LastName))
            WHEN cu.FirstName IS NOT NULL AND LTRIM(RTRIM(cu.FirstName)) <> '' 
            THEN LTRIM(RTRIM(cu.FirstName))
            WHEN cu.LastName IS NOT NULL AND LTRIM(RTRIM(cu.LastName)) <> '' 
            THEN LTRIM(RTRIM(cu.LastName))
            WHEN cu.UserName IS NOT NULL AND LTRIM(RTRIM(cu.UserName)) <> ''
            THEN LTRIM(RTRIM(cu.UserName))
            WHEN r.AddUserId IS NOT NULL AND LTRIM(RTRIM(r.AddUserId)) <> ''
            THEN r.AddUserId
            ELSE N'Unknown'
        END AS CreatorFullName,
        r.UpdateUserId,
        CASE 
            WHEN uu.FirstName IS NOT NULL AND LTRIM(RTRIM(uu.FirstName)) <> '' 
                 AND uu.LastName IS NOT NULL AND LTRIM(RTRIM(uu.LastName)) <> '' 
            THEN LTRIM(RTRIM(uu.FirstName)) + ' ' + LTRIM(RTRIM(uu.LastName))
            WHEN uu.FirstName IS NOT NULL AND LTRIM(RTRIM(uu.FirstName)) <> '' 
            THEN LTRIM(RTRIM(uu.FirstName))
            WHEN uu.LastName IS NOT NULL AND LTRIM(RTRIM(uu.LastName)) <> '' 
            THEN LTRIM(RTRIM(uu.LastName))
            WHEN uu.UserName IS NOT NULL AND LTRIM(RTRIM(uu.UserName)) <> ''
            THEN LTRIM(RTRIM(uu.UserName))
            WHEN r.UpdateUserId IS NOT NULL AND LTRIM(RTRIM(r.UpdateUserId)) <> ''
            THEN r.UpdateUserId
            ELSE N'Unknown'
        END AS UpdaterFullName
    FROM #AuditRecords r
    LEFT JOIN dbo.AspNetUsers cu ON cu.Id = r.AddUserId
    LEFT JOIN dbo.AspNetUsers uu ON uu.Id = r.UpdateUserId
    WHERE 
        -- Action filter
        (
            @ActionType IS NULL OR @ActionType = '' OR @ActionType = 'All'
            OR (@ActionType = 'Created' AND r.AddUserId IS NOT NULL AND LTRIM(RTRIM(r.AddUserId)) <> '')
            OR (@ActionType = 'Updated' AND r.UpdateUserId IS NOT NULL AND LTRIM(RTRIM(r.UpdateUserId)) <> '')
        )
        -- User filter
        AND (
            @UserId IS NULL OR @UserId = ''
            OR r.AddUserId = @UserId
            OR r.UpdateUserId = @UserId
        )
        -- Date range filter against either created or updated date
        AND (
            (@StartDate IS NULL AND @AdjustedEndDate IS NULL)
            OR (@StartDate IS NOT NULL AND @AdjustedEndDate IS NOT NULL AND ((r.CreatedDate BETWEEN @StartDate AND @AdjustedEndDate) OR (r.UpdatedDate BETWEEN @StartDate AND @AdjustedEndDate)))
            OR (@StartDate IS NOT NULL AND @AdjustedEndDate IS NULL AND (r.CreatedDate >= @StartDate OR r.UpdatedDate >= @StartDate))
            OR (@StartDate IS NULL AND @AdjustedEndDate IS NOT NULL AND (r.CreatedDate <= @AdjustedEndDate OR r.UpdatedDate <= @AdjustedEndDate))
        )
    ORDER BY COALESCE(r.UpdatedDate, r.CreatedDate) DESC;

    DROP TABLE #AuditRecords;
END
GO

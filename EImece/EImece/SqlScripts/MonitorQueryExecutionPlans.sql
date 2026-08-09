/*
  EImece — how to monitor SQL Server query plans for the product/order hot paths.

  Use these scripts in SSMS against the EImece database after deploying
  AddPerformanceIndexes.sql and exercising the storefront under load.
*/

SET NOCOUNT ON;
GO

/* --------------------------------------------------------------------------
   1) Capture an *actual* execution plan for a representative product listing
      (mirrors ProductRepository.GetActiveProducts paging predicate).
   -------------------------------------------------------------------------- */
-- In SSMS: Query → Include Actual Execution Plan (Ctrl+M), then run:
/*
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

SELECT TOP (20) p.Id, p.Name, p.Price, p.Position, p.ProductCategoryId, p.MainImageId
FROM dbo.Products AS p
WHERE p.IsActive = 1 AND p.Lang = 1
ORDER BY p.Position DESC;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
*/

-- What to look for in the plan:
--   * Index Seek (or ordered Index Scan) on IX_Products_IsActive_Lang_Position
--   * No Key Lookup storms (covering INCLUDE columns should avoid them)
--   * Estimated vs Actual rows roughly aligned (parameter sniffing otherwise)

/* --------------------------------------------------------------------------
   2) Order lookup by number — should be a unique seek
   -------------------------------------------------------------------------- */
/*
SELECT o.Id, o.OrderNumber, o.UserId, o.UpdatedDate
FROM dbo.Orders AS o
WHERE o.OrderNumber = N'ORD-EXAMPLE';
-- Expect: Index Seek on IX_Orders_OrderNumber
*/

/* --------------------------------------------------------------------------
   3) Find expensive cached plans that touch Products / Orders
   -------------------------------------------------------------------------- */
SELECT TOP (25)
    qs.execution_count,
    qs.total_worker_time / 1000 AS total_cpu_ms,
    qs.total_elapsed_time / 1000 AS total_elapsed_ms,
    qs.total_logical_reads,
    qs.total_logical_reads / NULLIF(qs.execution_count, 0) AS avg_logical_reads,
    SUBSTRING(st.text, (qs.statement_start_offset / 2) + 1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(st.text)
            ELSE qs.statement_end_offset
         END - qs.statement_start_offset) / 2) + 1) AS statement_text,
    qp.query_plan
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st
CROSS APPLY sys.dm_exec_query_plan(qs.plan_handle) AS qp
WHERE st.text LIKE N'%Products%'
   OR st.text LIKE N'%Orders%'
ORDER BY qs.total_logical_reads DESC;
GO

/* --------------------------------------------------------------------------
   4) Missing-index suggestions (heuristic — validate before creating)
   -------------------------------------------------------------------------- */
SELECT
    mid.statement AS table_name,
    migs.avg_user_impact,
    migs.user_seeks,
    migs.user_scans,
    mid.equality_columns,
    mid.inequality_columns,
    mid.included_columns
FROM sys.dm_db_missing_index_groups AS mig
INNER JOIN sys.dm_db_missing_index_group_stats AS migs
    ON migs.group_handle = mig.index_group_handle
INNER JOIN sys.dm_db_missing_index_details AS mid
    ON mid.index_handle = mig.index_handle
WHERE mid.database_id = DB_ID()
ORDER BY migs.avg_user_impact * migs.user_seeks DESC;
GO

/* --------------------------------------------------------------------------
   5) Index usage — confirm new indexes are sought, not only scanned/written
   -------------------------------------------------------------------------- */
SELECT
    OBJECT_NAME(s.object_id) AS table_name,
    i.name AS index_name,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats AS s
INNER JOIN sys.indexes AS i
    ON i.object_id = s.object_id AND i.index_id = s.index_id
WHERE s.database_id = DB_ID()
  AND OBJECT_NAME(s.object_id) IN (N'Products', N'Orders', N'OrderProducts', N'ProductTags')
ORDER BY table_name, index_name;
GO

/* --------------------------------------------------------------------------
   6) Live EF6 plan capture tip
   Enable Extended Events session "query_post_execution_showplan" briefly in
   staging, or turn on EF6 Database.Log / EImece EfSqlLogger and paste the SQL
   into SSMS with "Include Actual Execution Plan". Avoid leaving showplan XE
   on in production — it is expensive under concurrency.
   -------------------------------------------------------------------------- */

-- GetRegionalSalesReport: PaymentStatus is supplied by the caller (nullable = all statuses).
-- Addresses are in dbo.Addresses (Orders.ShippingAddressId FK).
ALTER PROCEDURE [dbo].[GetRegionalSalesReport]
    @PaymentStatus NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sa.City,
        COUNT(o.Id) AS OrderCount,
        SUM(CAST(o.PaidPrice AS DECIMAL(18, 2))) AS TotalRevenue,
        o.Currency
    FROM [dbo].[Orders] o
    LEFT JOIN [dbo].[Addresses] sa ON o.ShippingAddressId = sa.Id
    WHERE o.IsActive = 1
      AND (
            @PaymentStatus IS NULL
            OR LTRIM(RTRIM(@PaymentStatus)) = N''
            OR o.PaymentStatus = @PaymentStatus
          )
    GROUP BY sa.City, o.Currency
    ORDER BY TotalRevenue DESC;
END
GO

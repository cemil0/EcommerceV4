-- Phase 7.2: Performance Indexes for Admin Panel
-- Critical indexes for order filtering performance

USE ECommerceDB;
GO

PRINT '==============================================';
PRINT 'PHASE 7.2: ADMIN PANEL PERFORMANCE INDEXES';
PRINT '==============================================';
PRINT '';

-- 1. Order Status Index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_OrderStatus' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
    PRINT '✓ IX_Orders_OrderStatus created';
END
ELSE
    PRINT '⚠ IX_Orders_OrderStatus already exists';

-- 2. Order CreatedAt Index (for date range filtering)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_CreatedAt' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt DESC);
    PRINT '✓ IX_Orders_CreatedAt created';
END
ELSE
    PRINT '⚠ IX_Orders_CreatedAt already exists';

-- 3. Order TotalAmount Index (for amount range filtering)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_TotalAmount' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE INDEX IX_Orders_TotalAmount ON Orders(TotalAmount);
    PRINT '✓ IX_Orders_TotalAmount created';
END
ELSE
    PRINT '⚠ IX_Orders_TotalAmount already exists';

-- 4. Composite Index for common filter combinations
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_Status_CreatedAt' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE INDEX IX_Orders_Status_CreatedAt ON Orders(OrderStatus, CreatedAt DESC);
    PRINT '✓ IX_Orders_Status_CreatedAt created (composite)';
END
ELSE
    PRINT '⚠ IX_Orders_Status_CreatedAt already exists';

-- 5. Customer Email Index (for customer search)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_Email' AND object_id = OBJECT_ID('Customers'))
BEGIN
    CREATE INDEX IX_Customers_Email ON Customers(Email);
    PRINT '✓ IX_Customers_Email created';
END
ELSE
    PRINT '⚠ IX_Customers_Email already exists';

-- 6. Product Name Index (for product search)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_ProductName' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE INDEX IX_Products_ProductName ON Products(ProductName);
    PRINT '✓ IX_Products_ProductName created';
END
ELSE
    PRINT '⚠ IX_Products_ProductName already exists';

-- 7. Product SKU Index (for SKU search)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_SKU' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE INDEX IX_Products_SKU ON Products(SKU);
    PRINT '✓ IX_Products_SKU created';
END
ELSE
    PRINT '⚠ IX_Products_SKU already exists';

PRINT '';
PRINT '==============================================';
PRINT '✅ PERFORMANCE INDEXES COMPLETE!';
PRINT '==============================================';
PRINT 'Created/Verified:';
PRINT '  • Order filtering indexes (4)';
PRINT '  • Customer search index (1)';
PRINT '  • Product search indexes (2)';
PRINT '';
PRINT 'Admin panel queries will be significantly faster!';
GO

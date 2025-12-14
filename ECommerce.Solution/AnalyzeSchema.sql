-- =============================================
-- DATABASE SCHEMA ANALYZER
-- Purpose: Get all table columns from actual database
-- =============================================

USE ECommerceDB;
GO

-- Get all columns for all tables
SELECT 
    t.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT,
    c.ORDINAL_POSITION
FROM 
    INFORMATION_SCHEMA.TABLES t
    INNER JOIN INFORMATION_SCHEMA.COLUMNS c 
        ON t.TABLE_NAME = c.TABLE_NAME
WHERE 
    t.TABLE_TYPE = 'BASE TABLE'
    AND t.TABLE_NAME IN ('Customers', 'Companies', 'Addresses', 'Categories', 'Products', 'ProductVariants')
ORDER BY 
    t.TABLE_NAME, c.ORDINAL_POSITION;
GO

-- =============================================
-- SCHEMA COMPARISON SCRIPT
-- Purpose: Compare Desktop .sql files with actual SQL Server database
-- =============================================

USE ECommerceDB;
GO

PRINT '==============================================';
PRINT 'SCHEMA COMPARISON REPORT';
PRINT '==============================================';
PRINT '';

-- =============================================
-- 1. TABLES IN SQL SERVER
-- =============================================
PRINT '1. TABLES IN SQL SERVER DATABASE:';
PRINT '----------------------------------------';

SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME NOT LIKE '__EF%'
  AND TABLE_NAME NOT LIKE 'AspNet%'
ORDER BY TABLE_NAME;

PRINT '';

-- =============================================
-- 2. DETAILED COLUMN COMPARISON
-- =============================================
PRINT '2. COLUMN DETAILS FOR KEY TABLES:';
PRINT '----------------------------------------';

-- Customers table
PRINT 'CUSTOMERS TABLE:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customers'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'COMPANIES TABLE:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Companies'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'ADDRESSES TABLE:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Addresses'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'PRODUCTS TABLE:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'PRODUCTVARIANTS TABLE:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ProductVariants'
ORDER BY ORDINAL_POSITION;

-- =============================================
-- 3. FOREIGN KEY RELATIONSHIPS
-- =============================================
PRINT '';
PRINT '3. FOREIGN KEY RELATIONSHIPS:';
PRINT '----------------------------------------';

SELECT 
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM 
    sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fc 
        ON fk.object_id = fc.constraint_object_id
WHERE 
    OBJECT_NAME(fk.parent_object_id) IN ('Customers', 'Companies', 'Addresses', 'Products', 'ProductVariants', 'Orders', 'OrderItems')
ORDER BY 
    TableName, ForeignKey;

-- =============================================
-- 4. INDEXES
-- =============================================
PRINT '';
PRINT '4. INDEXES ON KEY TABLES:';
PRINT '----------------------------------------';

SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM 
    sys.indexes i
    INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE 
    t.name IN ('Customers', 'Products', 'ProductVariants', 'Orders')
    AND i.name IS NOT NULL
ORDER BY 
    t.name, i.name, ic.key_ordinal;

-- =============================================
-- 5. SUMMARY
-- =============================================
PRINT '';
PRINT '==============================================';
PRINT 'SUMMARY:';
PRINT '==============================================';

SELECT 
    'Total Tables' AS Metric,
    COUNT(*) AS Count
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME NOT LIKE '__EF%'
  AND TABLE_NAME NOT LIKE 'AspNet%'

UNION ALL

SELECT 
    'Total Columns',
    COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('Customers', 'Companies', 'Addresses', 'Products', 'ProductVariants', 'Orders', 'OrderItems')

UNION ALL

SELECT 
    'Foreign Keys',
    COUNT(*)
FROM sys.foreign_keys
WHERE OBJECT_NAME(parent_object_id) IN ('Customers', 'Companies', 'Addresses', 'Products', 'ProductVariants', 'Orders', 'OrderItems');

PRINT '';
PRINT '==============================================';
PRINT 'COMPARISON COMPLETE';
PRINT '==============================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Review the output above';
PRINT '2. Compare with Desktop .sql files';
PRINT '3. If mismatch found, use Entity Framework migrations';
PRINT '4. Command: dotnet ef database update';
GO

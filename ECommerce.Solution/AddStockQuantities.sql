-- =============================================
-- PHASE 5: ADD STOCK QUANTITIES
-- Purpose: Update ProductVariants with realistic stock levels
-- =============================================

USE ECommerceDB;
GO

PRINT 'Adding realistic stock quantities to ProductVariants...';

-- Update stock quantities with variety
-- High stock items (50-200)
UPDATE ProductVariants SET StockQuantity = 150 WHERE ProductVariantId IN (1, 2, 3, 4, 5);
UPDATE ProductVariants SET StockQuantity = 120 WHERE ProductVariantId IN (6, 7, 8, 9, 10);
UPDATE ProductVariants SET StockQuantity = 100 WHERE ProductVariantId IN (11, 12, 13, 14, 15);

-- Medium stock items (20-50)
UPDATE ProductVariants SET StockQuantity = 45 WHERE ProductVariantId IN (16, 17, 18, 19, 20);
UPDATE ProductVariants SET StockQuantity = 35 WHERE ProductVariantId IN (21, 22, 23, 24, 25);
UPDATE ProductVariants SET StockQuantity = 25 WHERE ProductVariantId IN (26, 27, 28, 29, 30);

-- Low stock items (5-20)
UPDATE ProductVariants SET StockQuantity = 15 WHERE ProductVariantId IN (31, 32, 33, 34, 35);
UPDATE ProductVariants SET StockQuantity = 10 WHERE ProductVariantId IN (36, 37, 38, 39, 40);
UPDATE ProductVariants SET StockQuantity = 5 WHERE ProductVariantId IN (41, 42, 43, 44, 45);

-- Out of stock items (0)
UPDATE ProductVariants SET StockQuantity = 0 WHERE ProductVariantId IN (46, 47, 48, 49, 50);

-- Update remaining variants with random stock (if any)
UPDATE ProductVariants 
SET StockQuantity = ABS(CHECKSUM(NEWID()) % 100) + 10
WHERE StockQuantity = 0 AND ProductVariantId > 50;

PRINT '✅ Stock quantities updated successfully!';
PRINT '';
PRINT 'Stock Distribution:';
PRINT '  • High Stock (100-150): 15 variants';
PRINT '  • Medium Stock (25-45): 15 variants';
PRINT '  • Low Stock (5-15): 15 variants';
PRINT '  • Out of Stock (0): 5 variants';
GO

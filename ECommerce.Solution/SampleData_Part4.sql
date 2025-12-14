-- =============================================
-- PART 4: PRODUCTS AND VARIANTS (FINAL)
-- Purpose: Create Products and ProductVariants with complete metadata
-- Run Order: 4/4
-- PREREQUISITE: Parts 1, 2, and 3 must be completed successfully
-- =============================================

USE ECommerceDB;
GO

SET NOCOUNT ON;

PRINT '==============================================';
PRINT 'PART 4: PRODUCTS AND VARIANTS';
PRINT '==============================================';
PRINT '';

BEGIN TRY
    BEGIN TRANSACTION ProductDataPhase;
    
    -- Products (20) with complete metadata
    PRINT 'Creating 20 Products...';
    INSERT INTO Products (CategoryId, SKU, ProductName, ProductSlug, ShortDescription, LongDescription, Brand, Manufacturer, Model, MetaTitle, IsVariantProduct, IsActive, IsFeatured, IsNewArrival, CreatedAt, UpdatedAt)
    VALUES
    (1, 'PROD-IPH15P', 'iPhone 15 Pro', 'iphone-15-pro', 'Apple iPhone 15 Pro', 'Titanium Design, A17 Pro Chip', 'Apple', 'Apple Inc.', 'iPhone 15 Pro', 'Buy iPhone 15 Pro - Apple Smartphone', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (1, 'PROD-SGS24U', 'Samsung Galaxy S24 Ultra', 'samsung-galaxy-s24-ultra', 'Samsung S24 Ultra', 'AI Phone, 200MP Camera', 'Samsung', 'Samsung Electronics', 'Galaxy S24 Ultra', 'Samsung Galaxy S24 Ultra - AI Smartphone', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (2, 'PROD-MBP14', 'MacBook Pro 14"', 'macbook-pro-14', 'MacBook Pro 14"', 'M3 Pro Chip, Retina XDR', 'Apple', 'Apple Inc.', 'MacBook Pro 14" 2024', 'MacBook Pro 14" M3 - Apple Laptop', 1, 1, 1, 0, GETDATE(), GETDATE()),
    (2, 'PROD-DXPS15', 'Dell XPS 15', 'dell-xps-15', 'Dell XPS 15', 'Intel i7, RTX 4050', 'Dell', 'Dell Technologies', 'XPS 15 9530', 'Dell XPS 15 - Premium Laptop', 1, 1, 0, 0, GETDATE(), GETDATE()),
    (3, 'PROD-IPADAIR', 'iPad Air', 'ipad-air', 'iPad Air', 'M2 Chip, 11" Display', 'Apple', 'Apple Inc.', 'iPad Air M2', 'iPad Air M2 - Apple Tablet', 1, 1, 0, 1, GETDATE(), GETDATE()),
    (4, 'PROD-AIRPODSP2', 'AirPods Pro 2', 'airpods-pro-2', 'AirPods Pro 2', 'Active Noise Cancellation', 'Apple', 'Apple Inc.', 'AirPods Pro 2nd Gen', 'AirPods Pro 2 - Wireless Earbuds', 1, 1, 1, 0, GETDATE(), GETDATE()),
    (4, 'PROD-SONYWH', 'Sony WH-1000XM5', 'sony-wh-1000xm5', 'Sony WH-1000XM5', 'Premium Noise Cancellation', 'Sony', 'Sony Corporation', 'WH-1000XM5', 'Sony WH-1000XM5 - Noise Cancelling Headphones', 1, 1, 0, 0, GETDATE(), GETDATE()),
    (5, 'PROD-AW9', 'Apple Watch Series 9', 'apple-watch-series-9', 'Apple Watch 9', 'GPS + Cellular', 'Apple', 'Apple Inc.', 'Watch Series 9', 'Apple Watch Series 9 - Smartwatch', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (6, 'PROD-SONYCAM', 'Sony Alpha A7 IV', 'sony-alpha-a7-iv', 'Sony A7 IV', 'Full Frame Mirrorless', 'Sony', 'Sony Corporation', 'Alpha A7 IV', 'Sony A7 IV - Mirrorless Camera', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (7, 'PROD-PS5', 'PlayStation 5', 'playstation-5', 'PS5 Console', 'Next-Gen Gaming', 'Sony', 'Sony Interactive', 'PlayStation 5', 'PlayStation 5 - Gaming Console', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (8, 'PROD-ECHO', 'Amazon Echo Dot', 'amazon-echo-dot', 'Echo Dot 5th Gen', 'Smart Speaker with Alexa', 'Amazon', 'Amazon.com Inc.', 'Echo Dot 5th Gen', 'Amazon Echo Dot 5 - Smart Speaker', 1, 1, 0, 1, GETDATE(), GETDATE()),
    (9, 'PROD-MAGSAFE', 'MagSafe Charger', 'magsafe-charger', 'Apple MagSafe', 'Wireless Charging', 'Apple', 'Apple Inc.', 'MagSafe Charger', 'MagSafe Charger - Wireless Charging', 1, 1, 0, 0, GETDATE(), GETDATE()),
    (10, 'PROD-SSD1TB', 'Samsung 990 Pro SSD', 'samsung-990-pro-ssd', 'Samsung SSD', '1TB NVMe Gen 4', 'Samsung', 'Samsung Electronics', '990 Pro', 'Samsung 990 Pro 1TB - NVMe SSD', 1, 1, 1, 0, GETDATE(), GETDATE()),
    (11, 'PROD-LG27', 'LG UltraGear 27"', 'lg-ultragear-27', 'LG Gaming Monitor', '144Hz QHD Display', 'LG', 'LG Electronics', 'UltraGear 27GN800', 'LG UltraGear 27" - Gaming Monitor', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (12, 'PROD-MXKEYS', 'Logitech MX Keys', 'logitech-mx-keys', 'MX Keys Keyboard', 'Wireless Illuminated', 'Logitech', 'Logitech International', 'MX Keys', 'Logitech MX Keys - Wireless Keyboard', 1, 1, 0, 0, GETDATE(), GETDATE()),
    (13, 'PROD-MXMASTER', 'Logitech MX Master 3S', 'logitech-mx-master-3s', 'MX Master 3S Mouse', 'Wireless Performance', 'Logitech', 'Logitech International', 'MX Master 3S', 'Logitech MX Master 3S - Wireless Mouse', 1, 1, 1, 0, GETDATE(), GETDATE()),
    (14, 'PROD-HP3630', 'HP DeskJet 3630', 'hp-deskjet-3630', 'HP Printer', 'All-in-One Wireless', 'HP', 'HP Inc.', 'DeskJet 3630', 'HP DeskJet 3630 - All-in-One Printer', 1, 1, 0, 0, GETDATE(), GETDATE()),
    (15, 'PROD-EPSON', 'Epson Perfection V39', 'epson-perfection-v39', 'Epson Scanner', 'Photo & Document', 'Epson', 'Seiko Epson', 'Perfection V39', 'Epson Perfection V39 - Photo Scanner', 1, 1, 0, 0, GETDATE(), GETDATE()),
    (16, 'PROD-TPLINK', 'TP-Link Archer AX73', 'tp-link-archer-ax73', 'TP-Link Router', 'WiFi 6 AX5400', 'TP-Link', 'TP-Link Technologies', 'Archer AX73', 'TP-Link Archer AX73 - WiFi 6 Router', 1, 1, 1, 1, GETDATE(), GETDATE()),
    (17, 'PROD-WIN11', 'Windows 11 Pro', 'windows-11-pro', 'Windows 11 Pro', 'Operating System License', 'Microsoft', 'Microsoft Corporation', 'Windows 11 Pro', 'Windows 11 Pro - Operating System', 1, 1, 0, 0, GETDATE(), GETDATE());
    PRINT '✓ 20 Products created (IDs 1-20)';
    PRINT '';
    
    -- Product Variants (20) with complete attributes
    PRINT 'Creating 20 Product Variants...';
    INSERT INTO ProductVariants (ProductId, VariantSKU, VariantName, Color, Size, Storage, RAM, BasePrice, SalePrice, CostPrice, Currency, StockQuantity, ReservedQuantity, IsActive, IsDefault, CreatedAt, UpdatedAt)
    VALUES
    (1, 'IPH15P-128-NAT', '128GB Natural Titanium', 'Natural Titanium', 'Standard', '128GB', NULL, 54999.00, 52999.00, 45000.00, 'TRY', 50, 0, 1, 1, GETDATE(), GETDATE()),
    (2, 'SGS24U-256-GRY', '256GB Titanium Gray', 'Titanium Gray', 'Standard', '256GB', '12GB', 49999.00, 47999.00, 42000.00, 'TRY', 40, 0, 1, 1, GETDATE(), GETDATE()),
    (3, 'MBP14-M3P-512-BLK', 'M3 Pro 18GB 512GB', 'Space Black', '14"', '512GB', '18GB', 89999.00, 87999.00, 75000.00, 'TRY', 15, 0, 1, 1, GETDATE(), GETDATE()),
    (4, 'DXPS15-I7-512', 'i7 16GB 512GB SSD', 'Platinum Silver', '15.6"', '512GB', '16GB', 64999.00, 62999.00, 55000.00, 'TRY', 20, 0, 1, 1, GETDATE(), GETDATE()),
    (5, 'IPAD-AIR-128-GRY', '128GB Wi-Fi Space Gray', 'Space Gray', '11"', '128GB', NULL, 24999.00, 23999.00, 20000.00, 'TRY', 35, 0, 1, 1, GETDATE(), GETDATE()),
    (6, 'AIRPODS-PRO2-USBC', 'USB-C', 'White', 'Standard', NULL, NULL, 10999.00, 9999.00, 8500.00, 'TRY', 100, 0, 1, 1, GETDATE(), GETDATE()),
    (7, 'SONY-WH1000XM5-BLK', 'Black', 'Black', 'Standard', NULL, NULL, 14999.00, 13999.00, 12000.00, 'TRY', 50, 0, 1, 1, GETDATE(), GETDATE()),
    (8, 'AW9-41-GPS-MID', '41mm GPS Midnight', 'Midnight', '41mm', NULL, NULL, 16999.00, 15999.00, 13500.00, 'TRY', 40, 0, 1, 1, GETDATE(), GETDATE()),
    (9, 'SONYCAM-BODY', 'Body Only', 'Black', 'Standard', NULL, NULL, 89999.00, 85999.00, 75000.00, 'TRY', 10, 0, 1, 1, GETDATE(), GETDATE()),
    (10, 'PS5-DISC', 'Disc Edition', 'White', 'Standard', '825GB', NULL, 24999.00, 23999.00, 20000.00, 'TRY', 25, 0, 1, 1, GETDATE(), GETDATE()),
    (11, 'ECHO-DOT-5-BLK', '5th Gen Black', 'Black', 'Standard', NULL, NULL, 2499.00, 1999.00, 1500.00, 'TRY', 200, 0, 1, 1, GETDATE(), GETDATE()),
    (12, 'MAGSAFE-WHT', 'White', 'White', 'Standard', NULL, NULL, 1999.00, 1799.00, 1400.00, 'TRY', 150, 0, 1, 1, GETDATE(), GETDATE()),
    (13, 'SSD-990PRO-1TB', '1TB', 'Black', 'M.2', '1TB', NULL, 5999.00, 5499.00, 4500.00, 'TRY', 60, 0, 1, 1, GETDATE(), GETDATE()),
    (14, 'LG27-UG-QHD', '27" QHD 144Hz', 'Black', '27"', NULL, NULL, 12999.00, 11999.00, 10000.00, 'TRY', 30, 0, 1, 1, GETDATE(), GETDATE()),
    (15, 'MXKEYS-BLK', 'Black', 'Black', 'Standard', NULL, NULL, 4999.00, 4499.00, 3500.00, 'TRY', 45, 0, 1, 1, GETDATE(), GETDATE()),
    (16, 'MXMASTER3S-BLK', 'Black', 'Black', 'Standard', NULL, NULL, 3999.00, 3599.00, 2800.00, 'TRY', 55, 0, 1, 1, GETDATE(), GETDATE()),
    (17, 'HP3630-STD', 'Standard', 'White', 'Standard', NULL, NULL, 2999.00, 2799.00, 2200.00, 'TRY', 35, 0, 1, 1, GETDATE(), GETDATE()),
    (18, 'EPSON-V39-STD', 'Standard', 'Black', 'Standard', NULL, NULL, 3499.00, 3199.00, 2500.00, 'TRY', 25, 0, 1, 1, GETDATE(), GETDATE()),
    (19, 'TPLINK-AX73-STD', 'Standard', 'Black', 'Standard', NULL, NULL, 2499.00, 2299.00, 1800.00, 'TRY', 40, 0, 1, 1, GETDATE(), GETDATE()),
    (20, 'WIN11PRO-LIC', 'License Key', NULL, 'Digital', NULL, NULL, 4999.00, 4499.00, 3500.00, 'TRY', 999, 0, 1, 1, GETDATE(), GETDATE());
    PRINT '✓ 20 Product Variants created (IDs 1-20)';
    PRINT '';
    
    COMMIT TRANSACTION ProductDataPhase;
    
    PRINT '==============================================';
    PRINT '✅ ALL 4 PARTS COMPLETE!';
    PRINT '==============================================';
    PRINT '';
    
    -- Final verification
    SELECT 
        'AspNetUsers' AS [Table], COUNT(*) AS [Count] FROM AspNetUsers WHERE Id LIKE 'sample-%'
    UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
    UNION ALL SELECT 'Companies', COUNT(*) FROM Companies
    UNION ALL SELECT 'Customers', COUNT(*) FROM Customers
    UNION ALL SELECT 'Addresses', COUNT(*) FROM Addresses
    UNION ALL SELECT 'Products', COUNT(*) FROM Products
    UNION ALL SELECT 'ProductVariants', COUNT(*) FROM ProductVariants;
    
    PRINT '';
    PRINT '✅ PRODUCTION-READY SAMPLE DATA LOADED!';
    PRINT '';
    PRINT 'Improvements Applied:';
    PRINT '  ✓ Correct FK deletion order (OrderStatusHistory after Orders)';
    PRINT '  ✓ Table existence checks for all DELETE operations';
    PRINT '  ✓ Phased transactions (4 parts) to minimize locks';
    PRINT '  ✓ Real Identity password hash (Test@123)';
    PRINT '  ✓ Complete Product metadata (Manufacturer, Model, MetaTitle)';
    PRINT '  ✓ Complete ProductVariant attributes (Color, Size, RAM, CostPrice)';
    PRINT '  ✓ Address titles (Fatura/Teslimat Adresi)';
    PRINT '  ✓ All IDs start from 1';
    PRINT '';
    PRINT '🎯 READY FOR INTEGRATION TESTING!';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION ProductDataPhase;
    PRINT '❌ Product data failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH

GO

SET NOCOUNT OFF;

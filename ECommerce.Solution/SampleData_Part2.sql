-- =============================================
-- PART 2: CATEGORIES AND COMPANIES
-- Purpose: Create master data (Categories, Companies)
-- Run Order: 2/4
-- PREREQUISITE: Part 1 must be completed successfully
-- =============================================

USE ECommerceDB;
GO

SET NOCOUNT ON;

PRINT '==============================================';
PRINT 'PART 2: CATEGORIES AND COMPANIES';
PRINT '==============================================';
PRINT '';

BEGIN TRY
    BEGIN TRANSACTION MasterDataPhase;
    
    -- Categories (20)
    PRINT 'Creating 20 Categories...';
    INSERT INTO Categories (CategoryName, CategorySlug, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES
    ('Smartphones', 'smartphones', NULL, 1, 1, GETDATE(), GETDATE()),
    ('Laptops', 'laptops', NULL, 2, 1, GETDATE(), GETDATE()),
    ('Tablets', 'tablets', NULL, 3, 1, GETDATE(), GETDATE()),
    ('Audio', 'audio', NULL, 4, 1, GETDATE(), GETDATE()),
    ('Wearables', 'wearables', NULL, 5, 1, GETDATE(), GETDATE()),
    ('Cameras', 'cameras', NULL, 6, 1, GETDATE(), GETDATE()),
    ('Gaming', 'gaming', NULL, 7, 1, GETDATE(), GETDATE()),
    ('Smart Home', 'smart-home', NULL, 8, 1, GETDATE(), GETDATE()),
    ('Accessories', 'accessories', NULL, 9, 1, GETDATE(), GETDATE()),
    ('Storage', 'storage', NULL, 10, 1, GETDATE(), GETDATE()),
    ('Monitors', 'monitors', NULL, 11, 1, GETDATE(), GETDATE()),
    ('Keyboards', 'keyboards', NULL, 12, 1, GETDATE(), GETDATE()),
    ('Mice', 'mice', NULL, 13, 1, GETDATE(), GETDATE()),
    ('Printers', 'printers', NULL, 14, 1, GETDATE(), GETDATE()),
    ('Scanners', 'scanners', NULL, 15, 1, GETDATE(), GETDATE()),
    ('Networking', 'networking', NULL, 16, 1, GETDATE(), GETDATE()),
    ('Software', 'software', NULL, 17, 1, GETDATE(), GETDATE()),
    ('Cables', 'cables', NULL, 18, 1, GETDATE(), GETDATE()),
    ('Chargers', 'chargers', NULL, 19, 1, GETDATE(), GETDATE()),
    ('Cases', 'cases', NULL, 20, 1, GETDATE(), GETDATE());
    PRINT '✓ 20 Categories created (IDs 1-20)';
    PRINT '';
    
    -- Companies (20)
    PRINT 'Creating 20 Companies...';
    INSERT INTO Companies (CompanyName, TaxNumber, TaxOffice, Phone, Email, IsActive, CreatedAt, UpdatedAt)
    VALUES
    ('TechnoMarket A.Ş.', '1234567890', 'Kadıköy', '+902161234567', 'info@technomarket.com.tr', 1, GETDATE(), GETDATE()),
    ('Dijital Dünya Ltd.', '0987654321', 'Beşiktaş', '+902161234568', 'info@dijitaldunya.com.tr', 1, GETDATE(), GETDATE()),
    ('Elektronik Çözümler A.Ş.', '1122334455', 'Şişli', '+902161234569', 'info@elektronikcozumler.com.tr', 1, GETDATE(), GETDATE()),
    ('Bilişim Teknolojileri Ltd.', '2233445566', 'Ataşehir', '+902161234570', 'info@bilisimtek.com.tr', 1, GETDATE(), GETDATE()),
    ('Yazılım Dünyası A.Ş.', '3344556677', 'Maltepe', '+902161234571', 'info@yazilimdunyasi.com.tr', 1, GETDATE(), GETDATE()),
    ('Network Solutions Ltd.', '4455667788', 'Kartal', '+902161234572', 'info@networksol.com.tr', 1, GETDATE(), GETDATE()),
    ('Cloud Systems A.Ş.', '5566778899', 'Pendik', '+902161234573', 'info@cloudsys.com.tr', 1, GETDATE(), GETDATE()),
    ('Data Center Ltd.', '6677889900', 'Tuzla', '+902161234574', 'info@datacenter.com.tr', 1, GETDATE(), GETDATE()),
    ('Smart Tech A.Ş.', '7788990011', 'Üsküdar', '+902161234575', 'info@smarttech.com.tr', 1, GETDATE(), GETDATE()),
    ('Digital Solutions Ltd.', '8899001122', 'Çekmeköy', '+902161234576', 'info@digitalsol.com.tr', 1, GETDATE(), GETDATE()),
    ('IT Services A.Ş.', '9900112233', 'Sancaktepe', '+902161234577', 'info@itservices.com.tr', 1, GETDATE(), GETDATE()),
    ('Tech Innovations Ltd.', '0011223344', 'Sultanbeyli', '+902161234578', 'info@techinno.com.tr', 1, GETDATE(), GETDATE()),
    ('Cyber Security A.Ş.', '1122334466', 'Kağıthane', '+902161234579', 'info@cybersec.com.tr', 1, GETDATE(), GETDATE()),
    ('Mobile Solutions Ltd.', '2233445577', 'Eyüpsultan', '+902161234580', 'info@mobilesol.com.tr', 1, GETDATE(), GETDATE()),
    ('Web Development A.Ş.', '3344556688', 'Bayrampaşa', '+902161234581', 'info@webdev.com.tr', 1, GETDATE(), GETDATE()),
    ('App Studio Ltd.', '4455667799', 'Güngören', '+902161234582', 'info@appstudio.com.tr', 1, GETDATE(), GETDATE()),
    ('E-Commerce Tech A.Ş.', '5566778800', 'Bağcılar', '+902161234583', 'info@ecomtech.com.tr', 1, GETDATE(), GETDATE()),
    ('AI Solutions Ltd.', '6677889911', 'Küçükçekmece', '+902161234584', 'info@aisol.com.tr', 1, GETDATE(), GETDATE()),
    ('Blockchain Systems A.Ş.', '7788990022', 'Avcılar', '+902161234585', 'info@blockchainsys.com.tr', 1, GETDATE(), GETDATE()),
    ('IoT Devices Ltd.', '8899001133', 'Esenyurt', '+902161234586', 'info@iotdevices.com.tr', 1, GETDATE(), GETDATE());
    PRINT '✓ 20 Companies created (IDs 1-20)';
    PRINT '';
    
    COMMIT TRANSACTION MasterDataPhase;
    
    PRINT '==============================================';
    PRINT '✅ PART 2 COMPLETE!';
    PRINT '==============================================';
    PRINT 'Next: Run Part 3 (Customers and Addresses)';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION MasterDataPhase;
    PRINT '❌ Master data failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH

GO

SET NOCOUNT OFF;

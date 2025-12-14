-- =============================================
-- PART 3: CUSTOMERS AND ADDRESSES
-- Purpose: Create Customers (B2C + B2B) and Addresses
-- Run Order: 3/4
-- PREREQUISITE: Parts 1 and 2 must be completed successfully
-- =============================================

USE ECommerceDB;
GO

SET NOCOUNT ON;

PRINT '==============================================';
PRINT 'PART 3: CUSTOMERS AND ADDRESSES';
PRINT '==============================================';
PRINT '';

BEGIN TRY
    BEGIN TRANSACTION CustomerDataPhase;
    
    -- Customers (20: 10 B2C + 10 B2B)
    PRINT 'Creating 20 Customers...';
    
    -- 10 B2C Customers
    INSERT INTO Customers (ApplicationUserId, CustomerType, CompanyId, FirstName, LastName, Email, Phone, IsActive, IsEmailVerified, CreatedAt, UpdatedAt)
    VALUES
    ('sample-b2c-01', 'B2C', NULL, 'Ahmet', 'Yılmaz', 'ahmet.yilmaz@example.com', '+905551234567', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-02', 'B2C', NULL, 'Ayşe', 'Kaya', 'ayse.kaya@example.com', '+905551234568', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-03', 'B2C', NULL, 'Mehmet', 'Demir', 'mehmet.demir@example.com', '+905551234569', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-04', 'B2C', NULL, 'Fatma', 'Çelik', 'fatma.celik@example.com', '+905551234570', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-05', 'B2C', NULL, 'Ali', 'Öztürk', 'ali.ozturk@example.com', '+905551234571', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-06', 'B2C', NULL, 'Zeynep', 'Arslan', 'zeynep.arslan@example.com', '+905551234572', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-07', 'B2C', NULL, 'Mustafa', 'Koç', 'mustafa.koc@example.com', '+905551234573', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-08', 'B2C', NULL, 'Elif', 'Şahin', 'elif.sahin@example.com', '+905551234574', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-09', 'B2C', NULL, 'Burak', 'Aydın', 'burak.aydin@example.com', '+905551234575', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2c-10', 'B2C', NULL, 'Selin', 'Polat', 'selin.polat@example.com', '+905551234576', 1, 1, GETDATE(), GETDATE());
    
    -- 10 B2B Customers
    INSERT INTO Customers (ApplicationUserId, CustomerType, CompanyId, FirstName, LastName, Email, Phone, IsActive, IsEmailVerified, CreatedAt, UpdatedAt)
    VALUES
    ('sample-b2b-01', 'B2B', 1, 'Can', 'Yılmaz', 'can.yilmaz@technomarket.com.tr', '+902161234567', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-02', 'B2B', 2, 'Deniz', 'Kara', 'deniz.kara@dijitaldunya.com.tr', '+902161234568', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-03', 'B2B', 3, 'Ece', 'Aydın', 'ece.aydin@elektronikcozumler.com.tr', '+902161234569', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-04', 'B2B', 4, 'Emre', 'Kılıç', 'emre.kilic@bilisimtek.com.tr', '+902161234570', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-05', 'B2B', 5, 'Gizem', 'Öz', 'gizem.oz@yazilimdunyasi.com.tr', '+902161234571', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-06', 'B2B', 6, 'Hakan', 'Tekin', 'hakan.tekin@networksol.com.tr', '+902161234572', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-07', 'B2B', 7, 'İrem', 'Yurt', 'irem.yurt@cloudsys.com.tr', '+902161234573', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-08', 'B2B', 8, 'Kerem', 'Aksoy', 'kerem.aksoy@datacenter.com.tr', '+902161234574', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-09', 'B2B', 9, 'Lale', 'Güneş', 'lale.gunes@smarttech.com.tr', '+902161234575', 1, 1, GETDATE(), GETDATE()),
    ('sample-b2b-10', 'B2B', 10, 'Mert', 'Yıldız', 'mert.yildiz@digitalsol.com.tr', '+902161234576', 1, 1, GETDATE(), GETDATE());
    
    PRINT '✓ 20 Customers created (IDs 1-20: 10 B2C + 10 B2B)';
    PRINT '';
    
    -- Addresses (40: 2 per customer)
    PRINT 'Creating 40 Addresses...';
    
    DECLARE @CustomerId INT = 1;
    DECLARE @CustomerData TABLE (CustomerId INT, FirstName NVARCHAR(100), LastName NVARCHAR(100), Phone NVARCHAR(20));
    
    INSERT INTO @CustomerData
    SELECT CustomerId, FirstName, LastName, Phone FROM Customers;
    
    WHILE @CustomerId <= 20
    BEGIN
        DECLARE @FName NVARCHAR(100), @LName NVARCHAR(100), @Phone NVARCHAR(20);
        SELECT @FName = FirstName, @LName = LastName, @Phone = Phone FROM @CustomerData WHERE CustomerId = @CustomerId;
        
        INSERT INTO Addresses (CustomerId, AddressType, AddressTitle, FirstName, LastName, Phone, AddressLine1, City, District, PostalCode, Country, IsDefault, CreatedAt, UpdatedAt)
        VALUES
        (@CustomerId, 'Billing', 'Fatura Adresi', @FName, @LName, @Phone, 'Address Line ' + CAST(@CustomerId AS NVARCHAR), 'İstanbul', 'Kadıköy', '34710', 'Türkiye', 1, GETDATE(), GETDATE()),
        (@CustomerId, 'Shipping', 'Teslimat Adresi', @FName, @LName, @Phone, 'Address Line ' + CAST(@CustomerId AS NVARCHAR), 'İstanbul', 'Kadıköy', '34710', 'Türkiye', 1, GETDATE(), GETDATE());
        
        SET @CustomerId = @CustomerId + 1;
    END
    
    PRINT '✓ 40 Addresses created (IDs 1-40: 2 per customer)';
    PRINT '';
    
    COMMIT TRANSACTION CustomerDataPhase;
    
    PRINT '==============================================';
    PRINT '✅ PART 3 COMPLETE!';
    PRINT '==============================================';
    PRINT 'Next: Run Part 4 (Products and Variants)';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION CustomerDataPhase;
    PRINT '❌ Customer data failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH

GO

SET NOCOUNT OFF;

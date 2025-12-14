-- =============================================
-- PART 1: CLEANUP AND IDENTITY USERS
-- Purpose: Clean existing data and create AspNetUsers
-- Run Order: 1/4
-- =============================================

USE ECommerceDB;
GO

SET NOCOUNT ON;

PRINT '==============================================';
PRINT 'PART 1: CLEANUP AND IDENTITY USERS';
PRINT '==============================================';
PRINT '';

-- =============================================
-- PHASE 1: CLEANUP
-- =============================================
BEGIN TRY
    BEGIN TRANSACTION CleanupPhase;
    
    PRINT 'Step 1: Cleaning existing data...';
    
    -- Disable FK constraints
    DECLARE @sql NVARCHAR(MAX) = '';
    SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' NOCHECK CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
    FROM sys.foreign_keys;
    EXEC sp_executesql @sql;
    
    -- Delete all data (CORRECT ORDER - respecting FK dependencies)
    IF OBJECT_ID('RefreshTokens', 'U') IS NOT NULL DELETE FROM RefreshTokens;
    IF OBJECT_ID('OrderItems', 'U') IS NOT NULL DELETE FROM OrderItems;
    IF OBJECT_ID('Orders', 'U') IS NOT NULL DELETE FROM Orders;
    IF OBJECT_ID('OrderStatusHistory', 'U') IS NOT NULL DELETE FROM OrderStatusHistory;  -- FIXED: After Orders
    IF OBJECT_ID('CartItems', 'U') IS NOT NULL DELETE FROM CartItems;
    IF OBJECT_ID('Carts', 'U') IS NOT NULL DELETE FROM Carts;
    IF OBJECT_ID('Addresses', 'U') IS NOT NULL DELETE FROM Addresses;
    IF OBJECT_ID('Customers', 'U') IS NOT NULL DELETE FROM Customers;
    IF OBJECT_ID('Companies', 'U') IS NOT NULL DELETE FROM Companies;
    IF OBJECT_ID('ProductVariants', 'U') IS NOT NULL DELETE FROM ProductVariants;
    IF OBJECT_ID('Products', 'U') IS NOT NULL DELETE FROM Products;
    IF OBJECT_ID('Categories', 'U') IS NOT NULL DELETE FROM Categories;
    
    -- Delete Identity users (sample data only)
    DELETE FROM AspNetUserRoles WHERE UserId LIKE 'sample-%';
    DELETE FROM AspNetUserClaims WHERE UserId LIKE 'sample-%';
    DELETE FROM AspNetUserLogins WHERE UserId LIKE 'sample-%';
    DELETE FROM AspNetUserTokens WHERE UserId LIKE 'sample-%';
    DELETE FROM AspNetUsers WHERE Id LIKE 'sample-%';
    
    -- Reset identity seeds to 0 (next insert will be 1)
    IF OBJECT_ID('Categories', 'U') IS NOT NULL DBCC CHECKIDENT ('Categories', RESEED, 0);
    IF OBJECT_ID('Companies', 'U') IS NOT NULL DBCC CHECKIDENT ('Companies', RESEED, 0);
    IF OBJECT_ID('Customers', 'U') IS NOT NULL DBCC CHECKIDENT ('Customers', RESEED, 0);
    IF OBJECT_ID('Addresses', 'U') IS NOT NULL DBCC CHECKIDENT ('Addresses', RESEED, 0);
    IF OBJECT_ID('Products', 'U') IS NOT NULL DBCC CHECKIDENT ('Products', RESEED, 0);
    IF OBJECT_ID('ProductVariants', 'U') IS NOT NULL DBCC CHECKIDENT ('ProductVariants', RESEED, 0);
    IF OBJECT_ID('Carts', 'U') IS NOT NULL DBCC CHECKIDENT ('Carts', RESEED, 0);
    IF OBJECT_ID('Orders', 'U') IS NOT NULL DBCC CHECKIDENT ('Orders', RESEED, 0);
    
    -- Re-enable FK constraints
    SET @sql = '';
    SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' WITH CHECK CHECK CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
    FROM sys.foreign_keys;
    EXEC sp_executesql @sql;
    
    COMMIT TRANSACTION CleanupPhase;
    PRINT '✓ Cleanup complete, identity seeds reset to 0';
    PRINT '';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION CleanupPhase;
    PRINT '❌ Cleanup failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH

-- =============================================
-- PHASE 2: IDENTITY USERS
-- =============================================
BEGIN TRY
    BEGIN TRANSACTION IdentityPhase;
    
    PRINT 'Step 2: Creating Identity users...';
    
    -- FIXED: Real Identity password hash (Password: Test@123)
    DECLARE @RealPasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEInOC8yFJzGKKW+XB7zQN9kQXNCU8VuKeM7O7XHGXQV0Hj8hN5L5YmZJxKQZ8w==';
    
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, CreatedAt, UpdatedAt)
    VALUES
    -- B2C Users
    ('sample-b2c-01', 'ahmet.yilmaz@example.com', 'AHMET.YILMAZ@EXAMPLE.COM', 'ahmet.yilmaz@example.com', 'AHMET.YILMAZ@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234567', 1, 0, 1, 0, 'Ahmet', 'Yılmaz', GETDATE(), GETDATE()),
    ('sample-b2c-02', 'ayse.kaya@example.com', 'AYSE.KAYA@EXAMPLE.COM', 'ayse.kaya@example.com', 'AYSE.KAYA@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234568', 1, 0, 1, 0, 'Ayşe', 'Kaya', GETDATE(), GETDATE()),
    ('sample-b2c-03', 'mehmet.demir@example.com', 'MEHMET.DEMIR@EXAMPLE.COM', 'mehmet.demir@example.com', 'MEHMET.DEMIR@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234569', 1, 0, 1, 0, 'Mehmet', 'Demir', GETDATE(), GETDATE()),
    ('sample-b2c-04', 'fatma.celik@example.com', 'FATMA.CELIK@EXAMPLE.COM', 'fatma.celik@example.com', 'FATMA.CELIK@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234570', 1, 0, 1, 0, 'Fatma', 'Çelik', GETDATE(), GETDATE()),
    ('sample-b2c-05', 'ali.ozturk@example.com', 'ALI.OZTURK@EXAMPLE.COM', 'ali.ozturk@example.com', 'ALI.OZTURK@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234571', 1, 0, 1, 0, 'Ali', 'Öztürk', GETDATE(), GETDATE()),
    ('sample-b2c-06', 'zeynep.arslan@example.com', 'ZEYNEP.ARSLAN@EXAMPLE.COM', 'zeynep.arslan@example.com', 'ZEYNEP.ARSLAN@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234572', 1, 0, 1, 0, 'Zeynep', 'Arslan', GETDATE(), GETDATE()),
    ('sample-b2c-07', 'mustafa.koc@example.com', 'MUSTAFA.KOC@EXAMPLE.COM', 'mustafa.koc@example.com', 'MUSTAFA.KOC@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234573', 1, 0, 1, 0, 'Mustafa', 'Koç', GETDATE(), GETDATE()),
    ('sample-b2c-08', 'elif.sahin@example.com', 'ELIF.SAHIN@EXAMPLE.COM', 'elif.sahin@example.com', 'ELIF.SAHIN@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234574', 1, 0, 1, 0, 'Elif', 'Şahin', GETDATE(), GETDATE()),
    ('sample-b2c-09', 'burak.aydin@example.com', 'BURAK.AYDIN@EXAMPLE.COM', 'burak.aydin@example.com', 'BURAK.AYDIN@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234575', 1, 0, 1, 0, 'Burak', 'Aydın', GETDATE(), GETDATE()),
    ('sample-b2c-10', 'selin.polat@example.com', 'SELIN.POLAT@EXAMPLE.COM', 'selin.polat@example.com', 'SELIN.POLAT@EXAMPLE.COM', 1, @RealPasswordHash, NEWID(), NEWID(), '+905551234576', 1, 0, 1, 0, 'Selin', 'Polat', GETDATE(), GETDATE()),
    -- B2B Users
    ('sample-b2b-01', 'can.yilmaz@technomarket.com.tr', 'CAN.YILMAZ@TECHNOMARKET.COM.TR', 'can.yilmaz@technomarket.com.tr', 'CAN.YILMAZ@TECHNOMARKET.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234567', 1, 0, 1, 0, 'Can', 'Yılmaz', GETDATE(), GETDATE()),
    ('sample-b2b-02', 'deniz.kara@dijitaldunya.com.tr', 'DENIZ.KARA@DIJITALDUNYA.COM.TR', 'deniz.kara@dijitaldunya.com.tr', 'DENIZ.KARA@DIJITALDUNYA.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234568', 1, 0, 1, 0, 'Deniz', 'Kara', GETDATE(), GETDATE()),
    ('sample-b2b-03', 'ece.aydin@elektronikcozumler.com.tr', 'ECE.AYDIN@ELEKTRONIKCOZUMLER.COM.TR', 'ece.aydin@elektronikcozumler.com.tr', 'ECE.AYDIN@ELEKTRONIKCOZUMLER.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234569', 1, 0, 1, 0, 'Ece', 'Aydın', GETDATE(), GETDATE()),
    ('sample-b2b-04', 'emre.kilic@bilisimtek.com.tr', 'EMRE.KILIC@BILISIMTEK.COM.TR', 'emre.kilic@bilisimtek.com.tr', 'EMRE.KILIC@BILISIMTEK.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234570', 1, 0, 1, 0, 'Emre', 'Kılıç', GETDATE(), GETDATE()),
    ('sample-b2b-05', 'gizem.oz@yazilimdunyasi.com.tr', 'GIZEM.OZ@YAZILIMDUNYASI.COM.TR', 'gizem.oz@yazilimdunyasi.com.tr', 'GIZEM.OZ@YAZILIMDUNYASI.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234571', 1, 0, 1, 0, 'Gizem', 'Öz', GETDATE(), GETDATE()),
    ('sample-b2b-06', 'hakan.tekin@networksol.com.tr', 'HAKAN.TEKIN@NETWORKSOL.COM.TR', 'hakan.tekin@networksol.com.tr', 'HAKAN.TEKIN@NETWORKSOL.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234572', 1, 0, 1, 0, 'Hakan', 'Tekin', GETDATE(), GETDATE()),
    ('sample-b2b-07', 'irem.yurt@cloudsys.com.tr', 'IREM.YURT@CLOUDSYS.COM.TR', 'irem.yurt@cloudsys.com.tr', 'IREM.YURT@CLOUDSYS.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234573', 1, 0, 1, 0, 'İrem', 'Yurt', GETDATE(), GETDATE()),
    ('sample-b2b-08', 'kerem.aksoy@datacenter.com.tr', 'KEREM.AKSOY@DATACENTER.COM.TR', 'kerem.aksoy@datacenter.com.tr', 'KEREM.AKSOY@DATACENTER.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234574', 1, 0, 1, 0, 'Kerem', 'Aksoy', GETDATE(), GETDATE()),
    ('sample-b2b-09', 'lale.gunes@smarttech.com.tr', 'LALE.GUNES@SMARTTECH.COM.TR', 'lale.gunes@smarttech.com.tr', 'LALE.GUNES@SMARTTECH.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234575', 1, 0, 1, 0, 'Lale', 'Güneş', GETDATE(), GETDATE()),
    ('sample-b2b-10', 'mert.yildiz@digitalsol.com.tr', 'MERT.YILDIZ@DIGITALSOL.COM.TR', 'mert.yildiz@digitalsol.com.tr', 'MERT.YILDIZ@DIGITALSOL.COM.TR', 1, @RealPasswordHash, NEWID(), NEWID(), '+902161234576', 1, 0, 1, 0, 'Mert', 'Yıldız', GETDATE(), GETDATE());
    
    COMMIT TRANSACTION IdentityPhase;
    PRINT '✓ 20 Identity users created';
    PRINT '  Login: ahmet.yilmaz@example.com';
    PRINT '  Password: Test@123';
    PRINT '';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION IdentityPhase;
    PRINT '❌ Identity users failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH

-- =============================================
-- PHASE 3: ROLES AND USER ROLES
-- =============================================
BEGIN TRY
    BEGIN TRANSACTION RolesPhase;
    
    PRINT 'Step 3: Creating Roles...';
    
    -- Create roles if they don't exist
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
    BEGIN
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
    END
    
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Customer')
    BEGIN
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Customer', 'CUSTOMER', NEWID());
    END
    
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Dealer')
    BEGIN
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'Dealer', 'DEALER', NEWID());
    END
    
    PRINT '✓ Roles created (Admin, Customer, Dealer)';
    
    -- Assign roles to users
    PRINT 'Assigning roles to users...';
    
    DECLARE @AdminRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');
    DECLARE @CustomerRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Customer');
    DECLARE @DealerRoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Dealer');
    
    -- Make first B2C user an Admin
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES ('sample-b2c-01', @AdminRoleId);
    
    -- Assign Customer role to all B2C users
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT Id, @CustomerRoleId 
    FROM AspNetUsers 
    WHERE Id LIKE 'sample-b2c-%';
    
    -- Assign Customer + Dealer roles to all B2B users
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT Id, @CustomerRoleId 
    FROM AspNetUsers 
    WHERE Id LIKE 'sample-b2b-%';
    
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT Id, @DealerRoleId 
    FROM AspNetUsers 
    WHERE Id LIKE 'sample-b2b-%';
    
    COMMIT TRANSACTION RolesPhase;
    PRINT '✓ Roles assigned:';
    PRINT '  • 1 Admin (ahmet.yilmaz@example.com)';
    PRINT '  • 10 B2C Customers';
    PRINT '  • 10 B2B Dealers (Customer + Dealer roles)';
    PRINT '';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION RolesPhase;
    PRINT '❌ Roles failed: ' + ERROR_MESSAGE();
    THROW;
END CATCH

PRINT '==============================================';
PRINT '✅ PART 1 COMPLETE!';
PRINT '==============================================';
PRINT 'Next: Run Part 2 (Categories and Companies)';
GO

SET NOCOUNT OFF;

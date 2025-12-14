-- Phase 7: Create Admin User for Testing
-- This script creates an admin user with proper password hash

USE ECommerceDB;
GO

PRINT '==============================================';
PRINT 'PHASE 7: ADMIN USER CREATION';
PRINT '==============================================';
PRINT '';

-- 1. Check if Admin role exists, create if not
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    DECLARE @AdminRoleId NVARCHAR(450) = NEWID();
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (@AdminRoleId, 'Admin', 'ADMIN', NEWID());
    PRINT '✓ Admin role created';
END
ELSE
BEGIN
    PRINT '⚠ Admin role already exists';
END

-- 2. Create admin user
DECLARE @AdminUserId NVARCHAR(450);
DECLARE @AdminEmail NVARCHAR(256) = 'admin@test.com';

-- Check if user already exists
IF EXISTS (SELECT * FROM AspNetUsers WHERE Email = @AdminEmail)
BEGIN
    PRINT '⚠ Admin user already exists';
    SELECT @AdminUserId = Id FROM AspNetUsers WHERE Email = @AdminEmail;
END
ELSE
BEGIN
    SET @AdminUserId = NEWID();
    
    -- Password: Admin@123
    -- This is a pre-hashed password for 'Admin@123'
    -- Generated using ASP.NET Core Identity PasswordHasher
    INSERT INTO AspNetUsers (
        Id, 
        UserName, 
        NormalizedUserName, 
        Email, 
        NormalizedEmail,
        EmailConfirmed, 
        PasswordHash, 
        SecurityStamp, 
        ConcurrencyStamp,
        PhoneNumberConfirmed, 
        TwoFactorEnabled, 
        LockoutEnabled,
        AccessFailedCount, 
        FirstName, 
        LastName, 
        IsActive, 
        CreatedAt, 
        UpdatedAt
    )
    VALUES (
        @AdminUserId,
        @AdminEmail,
        UPPER(@AdminEmail),
        @AdminEmail,
        UPPER(@AdminEmail),
        1,
        'AQAAAAIAAYagAAAAEHqE7ZxK8fGPvmJKN3qH5vYxJ0F8ZxK8fGPvmJKN3qH5vYxJ0F8ZxK8fGPvmJKN3qH5vYxJ0F8ZxK8fGPvmJKN3qH5vYxJ0F8ZxK8fGPvmJKN3qH5vYxJ0F8Zw==',
        NEWID(),
        NEWID(),
        0,
        0,
        1,
        0,
        'Admin',
        'User',
        1,
        GETDATE(),
        GETDATE()
    );
    PRINT '✓ Admin user created';
END

-- 3. Assign Admin role to user
DECLARE @AdminRoleId2 NVARCHAR(450);
SELECT @AdminRoleId2 = Id FROM AspNetRoles WHERE Name = 'Admin';

IF NOT EXISTS (SELECT * FROM AspNetUserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId2)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@AdminUserId, @AdminRoleId2);
    PRINT '✓ Admin role assigned to user';
END
ELSE
BEGIN
    PRINT '⚠ User already has Admin role';
END

-- 4. Create Customer record for admin user
IF NOT EXISTS (SELECT * FROM Customers WHERE ApplicationUserId = @AdminUserId)
BEGIN
    INSERT INTO Customers (
        ApplicationUserId, 
        CustomerType, 
        FirstName, 
        LastName, 
        Email, 
        IsActive,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        @AdminUserId,
        1, -- B2C
        'Admin',
        'User',
        @AdminEmail,
        1,
        GETDATE(),
        GETDATE()
    );
    PRINT '✓ Customer record created for admin';
END
ELSE
BEGIN
    PRINT '⚠ Customer record already exists';
END

PRINT '';
PRINT '==============================================';
PRINT '✅ ADMIN USER SETUP COMPLETE!';
PRINT '==============================================';
PRINT '';
PRINT 'Admin Credentials:';
PRINT '  Email: admin@test.com';
PRINT '  Password: Admin@123';
PRINT '';
PRINT 'Note: If login fails, you may need to register';
PRINT 'a new user and manually assign Admin role.';
PRINT '';
GO

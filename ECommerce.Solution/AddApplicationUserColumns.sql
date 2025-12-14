-- Add IsActive and LastLoginAt columns to AspNetUsers table

USE ECommerceDB;
GO

-- Check if columns exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IsActive')
BEGIN
    ALTER TABLE AspNetUsers ADD IsActive BIT NOT NULL DEFAULT 1;
    PRINT '✓ IsActive column added';
END
ELSE
BEGIN
    PRINT '⚠ IsActive column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'LastLoginAt')
BEGIN
    ALTER TABLE AspNetUsers ADD LastLoginAt DATETIME2 NULL;
    PRINT '✓ LastLoginAt column added';
END
ELSE
BEGIN
    PRINT '⚠ LastLoginAt column already exists';
END

PRINT '';
PRINT '✅ ApplicationUser migration complete!';
GO

USE ECommerceDB;
GO

-- Add missing columns to AspNetUsers table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'IsActive')
BEGIN
    ALTER TABLE AspNetUsers ADD IsActive BIT NOT NULL DEFAULT 1;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'LastLoginAt')
BEGIN
    ALTER TABLE AspNetUsers ADD LastLoginAt DATETIME2 NULL;
END

-- Verify columns
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AspNetUsers' 
AND COLUMN_NAME IN ('IsActive', 'LastLoginAt');
GO

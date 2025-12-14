-- =============================================
-- VERIFY OrderStatusHistory TABLE (PRODUCTION-GRADE)
-- Purpose: Enterprise-level table verification and creation
-- Date: December 11, 2025
-- Improvements: Schema filtering, FK to AspNetUsers, GETDATE() consistency, index optimization
-- =============================================

USE ECommerceDB;
GO

-- Check if table exists under dbo schema (IMPROVED: explicit schema check)
IF OBJECT_ID('dbo.OrderStatusHistory', 'U') IS NOT NULL
BEGIN
    PRINT '✅ OrderStatusHistory table EXISTS!';
    PRINT '';
    
    -- Show table structure (IMPROVED: added TABLE_SCHEMA filter)
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'OrderStatusHistory'
      AND TABLE_SCHEMA = 'dbo'  -- CRITICAL: schema filter added
    ORDER BY ORDINAL_POSITION;
    
    PRINT '';
    SELECT COUNT(*) AS [RecordCount] FROM dbo.OrderStatusHistory;
END
ELSE
BEGIN
    PRINT '❌ OrderStatusHistory table DOES NOT EXIST!';
    PRINT '';
    PRINT 'Creating table manually...';
    
    CREATE TABLE dbo.OrderStatusHistory (
        OrderStatusHistoryId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        FromStatus NVARCHAR(50) NOT NULL,
        ToStatus NVARCHAR(50) NOT NULL,
        Reason NVARCHAR(500) NULL,
        ChangedByUserId NVARCHAR(450) NULL,
        ChangedAt DATETIME2 NOT NULL DEFAULT GETDATE(),  -- IMPROVED: GETDATE() for consistency
        
        -- Foreign Keys
        CONSTRAINT FK_OrderStatusHistory_Orders FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
            
        -- IMPROVED: FK to AspNetUsers for data integrity
        CONSTRAINT FK_OrderStatusHistory_AspNetUsers FOREIGN KEY (ChangedByUserId)
            REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL
    );
    
    -- IMPROVED: Optimized indexes with INCLUDE columns for better query performance
    -- Check if indexes already exist before creating
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderStatusHistory_OrderId' AND object_id = OBJECT_ID('dbo.OrderStatusHistory'))
    BEGIN
        CREATE INDEX IX_OrderStatusHistory_OrderId 
            ON dbo.OrderStatusHistory(OrderId)
            INCLUDE (FromStatus, ToStatus, ChangedAt);  -- IMPROVED: covering index
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderStatusHistory_ChangedAt' AND object_id = OBJECT_ID('dbo.OrderStatusHistory'))
    BEGIN
        CREATE INDEX IX_OrderStatusHistory_ChangedAt 
            ON dbo.OrderStatusHistory(ChangedAt DESC)  -- IMPROVED: DESC for recent-first queries
            INCLUDE (OrderId, FromStatus, ToStatus);
    END
    
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderStatusHistory_ChangedByUserId' AND object_id = OBJECT_ID('dbo.OrderStatusHistory'))
    BEGIN
        CREATE INDEX IX_OrderStatusHistory_ChangedByUserId 
            ON dbo.OrderStatusHistory(ChangedByUserId)
            WHERE ChangedByUserId IS NOT NULL;  -- IMPROVED: filtered index
    END
    
    PRINT '✅ OrderStatusHistory table created successfully!';
    PRINT '✅ Foreign keys added (Orders, AspNetUsers)';
    PRINT '✅ Optimized indexes created with INCLUDE columns';
END
GO

-- Verification summary
PRINT '';
PRINT '==============================================';
PRINT 'VERIFICATION COMPLETE';
PRINT '==============================================';
PRINT 'Improvements applied:';
PRINT '  ✓ Schema filter (TABLE_SCHEMA = dbo)';
PRINT '  ✓ FK to AspNetUsers for data integrity';
PRINT '  ✓ GETDATE() instead of SYSUTCDATETIME() for consistency';
PRINT '  ✓ Covering indexes with INCLUDE columns';
PRINT '  ✓ Index existence check before creation';
PRINT '  ✓ Filtered index for ChangedByUserId';
GO

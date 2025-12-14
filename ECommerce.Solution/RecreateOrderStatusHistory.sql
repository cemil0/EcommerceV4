-- =============================================
-- DROP AND RECREATE OrderStatusHistory TABLE
-- Purpose: Replace old table with production-grade version
-- Date: December 11, 2025
-- =============================================

USE ECommerceDB;
GO

PRINT 'Dropping old OrderStatusHistory table...';

-- Drop old table if exists
IF OBJECT_ID('dbo.OrderStatusHistory', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.OrderStatusHistory;
    PRINT '✓ Old table dropped';
END
ELSE
BEGIN
    PRINT '✓ No existing table to drop';
END

PRINT '';
PRINT 'Creating new production-grade OrderStatusHistory table...';

-- Create new table with all improvements
CREATE TABLE dbo.OrderStatusHistory (
    OrderStatusHistoryId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    FromStatus NVARCHAR(50) NOT NULL,
    ToStatus NVARCHAR(50) NOT NULL,
    Reason NVARCHAR(500) NULL,
    ChangedByUserId NVARCHAR(450) NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Keys
    CONSTRAINT FK_OrderStatusHistory_Orders FOREIGN KEY (OrderId)
        REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        
    CONSTRAINT FK_OrderStatusHistory_AspNetUsers FOREIGN KEY (ChangedByUserId)
        REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL
);

PRINT '✓ Table created with FK to Orders and AspNetUsers';

-- Create optimized indexes
CREATE INDEX IX_OrderStatusHistory_OrderId 
    ON dbo.OrderStatusHistory(OrderId)
    INCLUDE (FromStatus, ToStatus, ChangedAt);

CREATE INDEX IX_OrderStatusHistory_ChangedAt 
    ON dbo.OrderStatusHistory(ChangedAt DESC)
    INCLUDE (OrderId, FromStatus, ToStatus);

CREATE INDEX IX_OrderStatusHistory_ChangedByUserId 
    ON dbo.OrderStatusHistory(ChangedByUserId)
    WHERE ChangedByUserId IS NOT NULL;

PRINT '✓ Optimized indexes created';

PRINT '';
PRINT '==============================================';
PRINT '✅ OrderStatusHistory TABLE RECREATED!';
PRINT '==============================================';
PRINT 'Improvements:';
PRINT '  ✓ FK to AspNetUsers for data integrity';
PRINT '  ✓ GETDATE() for consistency with other tables';
PRINT '  ✓ Covering indexes with INCLUDE columns';
PRINT '  ✓ Filtered index for ChangedByUserId';
PRINT '  ✓ DESC index for recent-first queries';
GO

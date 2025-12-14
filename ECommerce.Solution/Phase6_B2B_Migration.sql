-- =============================================
-- PHASE 6: B2B ADVANCED FEATURES - DATABASE MIGRATION
-- Purpose: Add ERP-level B2B features (Credit Limit, Price Lists, Approval Rules)
-- Run Order: After SampleData_Part4.sql
-- =============================================

USE ECommerceDB;
GO

SET NOCOUNT ON;

PRINT '=============================================='
PRINT 'PHASE 6: B2B ADVANCED FEATURES MIGRATION'
PRINT '=============================================='
PRINT ''

-- =============================================
-- STEP 1: UPDATE COMPANIES TABLE
-- =============================================
BEGIN TRY
    PRINT 'Step 1: Updating Companies table...'
    
    -- Check if columns already exist
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Companies') AND name = 'CreditLimit')
    BEGIN
        ALTER TABLE Companies ADD
            CreditLimit DECIMAL(18,2) DEFAULT 0 NOT NULL,
            CurrentBalance DECIMAL(18,2) DEFAULT 0 NOT NULL,
            PaymentTermDays INT DEFAULT 30 NOT NULL,
            IsApprovalRequired BIT DEFAULT 0 NOT NULL,
            PriceListId INT NULL,
            DiscountPercentage DECIMAL(5,2) DEFAULT 0 NOT NULL;
        
        PRINT '✓ Companies table updated with B2B columns'
    END
    ELSE
    BEGIN
        PRINT '⚠ Companies table already has B2B columns, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ Companies update failed: ' + ERROR_MESSAGE();
    RAISERROR('Companies update failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

-- =============================================
-- STEP 2: CREATE B2B PRICE LISTS TABLE
-- =============================================
BEGIN TRY
    PRINT 'Step 2: Creating B2BPriceLists table...'
    
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'B2BPriceLists')
    BEGIN
        CREATE TABLE B2BPriceLists (
            PriceListId INT IDENTITY(1,1) PRIMARY KEY,
            PriceListName NVARCHAR(100) NOT NULL,
            Description NVARCHAR(500),
            IsActive BIT DEFAULT 1 NOT NULL,
            ValidFrom DATETIME2 NOT NULL,
            ValidTo DATETIME2 NULL,
            CreatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            UpdatedAt DATETIME2 DEFAULT GETDATE() NOT NULL
        );
        
        PRINT '✓ B2BPriceLists table created'
    END
    ELSE
    BEGIN
        PRINT '⚠ B2BPriceLists table already exists, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ B2BPriceLists creation failed: ' + ERROR_MESSAGE();
    RAISERROR('B2BPriceLists creation failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

-- =============================================
-- STEP 3: CREATE B2B PRICE LIST ITEMS TABLE
-- =============================================
BEGIN TRY
    PRINT 'Step 3: Creating B2BPriceListItems table...'
    
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'B2BPriceListItems')
    BEGIN
        CREATE TABLE B2BPriceListItems (
            PriceListItemId INT IDENTITY(1,1) PRIMARY KEY,
            PriceListId INT NOT NULL,
            ProductVariantId INT NOT NULL,
            B2BPrice DECIMAL(18,2) NOT NULL,
            MinQuantity INT DEFAULT 1 NOT NULL,
            MaxQuantity INT NULL,
            DiscountPercentage DECIMAL(5,2) DEFAULT 0 NOT NULL,
            CreatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            UpdatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            CONSTRAINT FK_B2BPriceListItems_PriceList FOREIGN KEY (PriceListId) 
                REFERENCES B2BPriceLists(PriceListId) ON DELETE CASCADE,
            CONSTRAINT FK_B2BPriceListItems_ProductVariant FOREIGN KEY (ProductVariantId) 
                REFERENCES ProductVariants(ProductVariantId) ON DELETE CASCADE
        );
        
        -- Create index for performance
        CREATE INDEX IX_B2BPriceListItems_PriceListId ON B2BPriceListItems(PriceListId);
        CREATE INDEX IX_B2BPriceListItems_ProductVariantId ON B2BPriceListItems(ProductVariantId);
        
        PRINT '✓ B2BPriceListItems table created with indexes'
    END
    ELSE
    BEGIN
        PRINT '⚠ B2BPriceListItems table already exists, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ B2BPriceListItems creation failed: ' + ERROR_MESSAGE();
    RAISERROR('B2BPriceListItems creation failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

-- =============================================
-- STEP 4: CREATE COMPANY CAMPAIGNS TABLE
-- =============================================
BEGIN TRY
    PRINT 'Step 4: Creating CompanyCampaigns table...'
    
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompanyCampaigns')
    BEGIN
        CREATE TABLE CompanyCampaigns (
            CampaignId INT IDENTITY(1,1) PRIMARY KEY,
            CompanyId INT NOT NULL,
            CampaignName NVARCHAR(200) NOT NULL,
            Description NVARCHAR(1000),
            DiscountPercentage DECIMAL(5,2) NOT NULL,
            MinOrderAmount DECIMAL(18,2) DEFAULT 0 NOT NULL,
            ValidFrom DATETIME2 NOT NULL,
            ValidTo DATETIME2 NOT NULL,
            IsActive BIT DEFAULT 1 NOT NULL,
            CreatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            UpdatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            CONSTRAINT FK_CompanyCampaigns_Company FOREIGN KEY (CompanyId) 
                REFERENCES Companies(CompanyId) ON DELETE CASCADE
        );
        
        -- Create index for performance
        CREATE INDEX IX_CompanyCampaigns_CompanyId ON CompanyCampaigns(CompanyId);
        CREATE INDEX IX_CompanyCampaigns_Active ON CompanyCampaigns(IsActive, ValidFrom, ValidTo);
        
        PRINT '✓ CompanyCampaigns table created with indexes'
    END
    ELSE
    BEGIN
        PRINT '⚠ CompanyCampaigns table already exists, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ CompanyCampaigns creation failed: ' + ERROR_MESSAGE();
    RAISERROR('CompanyCampaigns creation failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

-- =============================================
-- STEP 5: CREATE COMPANY APPROVAL RULES TABLE
-- =============================================
BEGIN TRY
    PRINT 'Step 5: Creating CompanyApprovalRules table...'
    
    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CompanyApprovalRules')
    BEGIN
        CREATE TABLE CompanyApprovalRules (
            RuleId INT IDENTITY(1,1) PRIMARY KEY,
            CompanyId INT NOT NULL,
            RuleName NVARCHAR(200) NOT NULL,
            ThresholdAmount DECIMAL(18,2) NOT NULL,
            CategoryId INT NULL,
            ApproverRole NVARCHAR(100) NOT NULL,
            ApprovalLevel INT DEFAULT 1 NOT NULL,
            IsActive BIT DEFAULT 1 NOT NULL,
            CreatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            UpdatedAt DATETIME2 DEFAULT GETDATE() NOT NULL,
            CONSTRAINT FK_CompanyApprovalRules_Company FOREIGN KEY (CompanyId) 
                REFERENCES Companies(CompanyId) ON DELETE CASCADE,
            CONSTRAINT FK_CompanyApprovalRules_Category FOREIGN KEY (CategoryId) 
                REFERENCES Categories(CategoryId) ON DELETE SET NULL
        );
        
        -- Create indexes for performance
        CREATE INDEX IX_CompanyApprovalRules_CompanyId ON CompanyApprovalRules(CompanyId);
        CREATE INDEX IX_CompanyApprovalRules_Active ON CompanyApprovalRules(IsActive);
        
        PRINT '✓ CompanyApprovalRules table created with indexes'
    END
    ELSE
    BEGIN
        PRINT '⚠ CompanyApprovalRules table already exists, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ CompanyApprovalRules creation failed: ' + ERROR_MESSAGE();
    RAISERROR('CompanyApprovalRules creation failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

-- =============================================
-- STEP 6: UPDATE ORDERS TABLE
-- =============================================
BEGIN TRY
    PRINT 'Step 6: Updating Orders table...'
    
    -- Check if columns already exist
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Orders') AND name = 'PaymentTermDays')
    BEGIN
        ALTER TABLE Orders ADD
            PaymentTermDays INT NULL,
            DueDate DATETIME2 NULL,
            ApprovalStatus NVARCHAR(50) DEFAULT 'Approved' NOT NULL,
            ApprovedBy NVARCHAR(450) NULL,
            ApprovedAt DATETIME2 NULL,
            CorporateInvoiceNumber NVARCHAR(50) NULL,
            TaxOffice NVARCHAR(200) NULL,
            CompanyTaxNumber NVARCHAR(50) NULL,
            PurchaseOrderNumber NVARCHAR(100) NULL,
            CostCenter NVARCHAR(100) NULL;
        
        PRINT '✓ Orders table updated with B2B columns'
    END
    ELSE
    BEGIN
        PRINT '⚠ Orders table already has B2B columns, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ Orders update failed: ' + ERROR_MESSAGE();
    RAISERROR('Orders update failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

-- =============================================
-- STEP 7: ADD FOREIGN KEY FOR PRICE LIST
-- =============================================
BEGIN TRY
    PRINT 'Step 7: Adding foreign key for PriceListId...'
    
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Companies_PriceList')
    BEGIN
        ALTER TABLE Companies
        ADD CONSTRAINT FK_Companies_PriceList FOREIGN KEY (PriceListId)
            REFERENCES B2BPriceLists(PriceListId) ON DELETE SET NULL;
        
        PRINT '✓ Foreign key added for PriceListId'
    END
    ELSE
    BEGIN
        PRINT '⚠ Foreign key already exists, skipping...'
    END
    
END TRY
BEGIN CATCH
    PRINT '❌ Foreign key creation failed: ' + ERROR_MESSAGE();
    RAISERROR('Foreign key creation failed', 16, 1);
    RETURN;
END CATCH

PRINT ''

PRINT '=============================================='
PRINT '✅ PHASE 6 MIGRATION COMPLETE!'
PRINT '=============================================='
PRINT 'Created/Updated:'
PRINT '  • Companies table (6 new columns)'
PRINT '  • B2BPriceLists table'
PRINT '  • B2BPriceListItems table'
PRINT '  • CompanyCampaigns table'
PRINT '  • CompanyApprovalRules table'
PRINT '  • Orders table (10 new columns)'
PRINT ''
PRINT 'Next: Run sample data for B2B features'
GO

SET NOCOUNT OFF;
